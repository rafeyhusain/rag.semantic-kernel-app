using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Rag.SemanticKernel.Abstractions.Parser;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Guards;
using Rag.SemanticKernel.Model.Llm.Embedding;
using Rag.SemanticKernel.Model.Vector;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rag.SemanticKernel.Llm.Core.Embedding;

/// <summary>
/// Embedding Service for embeddings
/// </summary>
public class EmbeddingService<T, TRecord> : ITextEmbeddingGenerationService
    where T : class, IDocument, new()
    where TRecord : class
{
    private readonly ILogger<EmbeddingService<T, TRecord>> _logger;
    private readonly IVectorStoreRecordCollection<string, TRecord> _vectorStoreCollection;
    private readonly IFileParser _parser;
    private readonly Kernel _kernel;
    private readonly ModelSettings _model;
    private readonly HttpClient _httpClient;

    public EmbeddingService(
        ILogger<EmbeddingService<T, TRecord>> logger, 
        Kernel kernel,
        IVectorStoreRecordCollection<string, TRecord> vectorStoreCollection,
        IFileParser parser,
        ModelSettings model
        ) 
    {
        _logger = Guard.ThrowIfNull(logger);
        _kernel = Guard.ThrowIfNull(kernel);
        _vectorStoreCollection = Guard.ThrowIfNull(vectorStoreCollection);
        _parser = Guard.ThrowIfNull(parser);
        _model = Guard.ThrowIfNull(model);

        // Initialize HTTP client for API
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _model.ApiKey);
    }

    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = Array.Empty<float>();

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>
    {
        ["Provider"] = _model.Name,
        ["Endpoint"] = _model.Endpoint,
        ["EmbeddingModel"] = _model.EmbeddingModel,
        ["SupportsStreaming"] = true,
        ["SupportsFunctionCalling"] = false,
        ["SupportsEmbeddings"] = true
    };

    public async Task Generate(EmbeddingServiceOptions options)
    {
        if (!Directory.Exists(options.InputFolder))
        {
            _logger.LogError("Input folder does not exist: {Folder}", options.InputFolder);
            return;
        }

        var searchOption = options.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(options.InputFolder, $"*{options.Extension}", searchOption);

        foreach (var file in files)
        {
            await CreateEmbeddingsFile(file, options);
        }
    }

    private async Task CreateEmbeddingsFile(string filePath, EmbeddingServiceOptions options)
    {
        try
        {
            await GenerateEmbeddings(filePath);

            var fileName = Path.GetFileName(filePath);
            var destPath = Path.Combine(options.CompletedFolder, fileName);
            Directory.CreateDirectory(options.CompletedFolder);
            File.Move(filePath, destPath, overwrite: true);

            _logger.LogInformation("Moved processed file to completed folder: {CompletedFile}", destPath);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in EmbeddingService.CreateEmbeddingsFile for file {File}", filePath);
            throw;
        }
    }

    private async Task GenerateEmbeddings(string filePath)
    {
        _logger.LogInformation("Generating embeddings for {File}", filePath);

        await _vectorStoreCollection.CreateCollectionIfNotExistsAsync();

        _parser.Parse(filePath);

        foreach (var blockChunk in _parser.Blocks.Chunk(25))
        {
            var contents = blockChunk.Select(h => h.Content).ToArray();
            var embeddings = await GenerateWithRetry(contents);

            _logger.LogInformation($"Requested {contents.Length} embeddings, received {embeddings.Count}");

            for (int i = 0; i < blockChunk.Length && i < embeddings.Count; i++)
            {
                var block = blockChunk[i];

                var item = (T)Activator.CreateInstance(
                                     typeof(T),
                                     Guid.NewGuid().ToString(),
                                     _parser.FileName,
                                     _parser.FilePath,
                                     block.Text,
                                     block.Content,
                                     embeddings[i]
                                 )!;

                var record = item.Record as TRecord;

                await _vectorStoreCollection.UpsertAsync(record!);
            }
        }
    }

    private async Task<IList<ReadOnlyMemory<float>>> GenerateWithRetry(string[] texts, int maxRetries = 5)
    {
        int delay = 1000;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return await this.GenerateEmbeddingsAsync(texts);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("TooManyRequests"))
            {
                _logger.LogWarning("Rate limited. Retrying in {Delay}ms (Attempt {Attempt}/{Max})", delay, attempt + 1, maxRetries);
                await Task.Delay(delay);
                delay *= 2;
            }
        }

        throw new Exception("Failed to generate embeddings after retries.");
    }

    /// <summary>
    //    /// Generates embeddings for the specified text using the API
    //    /// </summary>
    //    /// <param name="text">The text to generate embeddings for</param>
    //    /// <returns>A ReadOnlyMemory<float> containing the embedding vector</returns>
    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel kernel = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating embeddings for {Count} items", data.Count);

            var requestBody = JsonSerializer.Serialize(new
            {
                model = _model.EmbeddingModel,
                input = data  // Send full list
            });

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_model.Endpoint}/embeddings", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error from API: {StatusCode}, {Response}", response.StatusCode, errorContent);
                throw new HttpRequestException($"API error: {response.StatusCode}, {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseContent);

            if (embeddingResponse?.Data == null || embeddingResponse.Data.Count == 0)
            {
                _logger.LogError("No embeddings returned from API");
                throw new InvalidOperationException("No embeddings returned from API");
            }

            _logger.LogDebug("Successfully generated {Count} embeddings", embeddingResponse.Data.Count);

            return embeddingResponse.Data
                .Select(d => new ReadOnlyMemory<float>(d.Embedding))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings");
            throw;
        }
    }

}