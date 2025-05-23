using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Rag.SemanticKernel.Abstractions.Parser;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Guards;
using Rag.SemanticKernel.Model.Llm.Embedding;
using Rag.SemanticKernel.Rest;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rag.Connector.Core.Embedding;

/// <summary>
/// Embedding Service for embeddings
/// </summary>
public class EmbeddingService<TRecord> : ITextEmbeddingGenerationService
    where TRecord : class
{
    protected readonly ILogger<EmbeddingService<TRecord>> _logger;
    protected readonly IVectorStoreRecordCollection<string, TRecord> _vectorStoreCollection;
    protected readonly IFileParser _parser;
    protected readonly Kernel _kernel;
    protected readonly ModelPairSettings _pairSettings;
    protected readonly RestService _restService;

    public EmbeddingService(
        ILogger<EmbeddingService<TRecord>> logger,
        Kernel kernel,
        IVectorStoreRecordCollection<string, TRecord> vectorStoreCollection,
        IFileParser parser,
        RestService restService,
        ModelPairSettings pairSettings)
    {
        _logger = Guard.ThrowIfNull(logger);
        _kernel = Guard.ThrowIfNull(kernel);
        _vectorStoreCollection = Guard.ThrowIfNull(vectorStoreCollection);
        _parser = Guard.ThrowIfNull(parser);
        _pairSettings = Guard.ThrowIfNull(pairSettings);
        _restService = Guard.ThrowIfNull(restService);

        _restService.BaseUrl = _pairSettings.Endpoint;
        _restService.SetApiKey(_pairSettings.ApiKey);
    }

    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = [];

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>
    {
        ["Provider"] = _pairSettings.Name,
        ["Endpoint"] = _pairSettings.Endpoint,
        ["EmbeddingModel"] = _pairSettings.EmbeddingModel,
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
        _logger.LogInformation($"Generating embeddings for [${filePath}]");

        await _vectorStoreCollection.CreateCollectionIfNotExistsAsync();

        _parser.Parse(filePath);

        foreach (var blockChunk in _parser.Blocks.Chunk(25))
        {
            var contents = blockChunk.Select(h => h.Content).ToArray();
            var embeddings = await GenerateEmbeddingsAsync(contents);

            _logger.LogInformation($"Requested {contents.Length} embeddings, received {embeddings.Count}");

            for (int i = 0; i < blockChunk.Length && i < embeddings.Count; i++)
            {
                var block = blockChunk[i];

                await UpsertAsync(
                    Guid.NewGuid().ToString(),
                    _parser.FileName,
                    _parser.FilePath,
                    block.Text,
                    block.Content,
                    embeddings[i]
                );
            }
        }
    }

    protected virtual async Task UpsertAsync(
        string id,
        string fileName,
        string filePath,
        string text,
        string content,
        ReadOnlyMemory<float> embeddings)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    //    /// Generates embeddings for the specified text using the API
    //    /// </summary>
    //    /// <param name="text">The text to generate embeddings for</param>
    //    /// <returns>A ReadOnlyMemory<float> containing the embedding vector</returns>
    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating embeddings for {Count} items", data.Count);

            var request = new EmbeddingRequest(_pairSettings.EmbeddingModel, data);
            var response = await _restService.PostAsync($"embeddings", request);
            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(response);

            if (embeddingResponse?.Data == null || embeddingResponse.Data.Count == 0)
            {
                var message = $"No embeddings returned from {_pairSettings.Endpoint}/embeddings API";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }

            _logger.LogDebug("Successfully generated {Count} embeddings", embeddingResponse.Data.Count);

            return [.. embeddingResponse.Data.Select(d => new ReadOnlyMemory<float>(d.Embedding))];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings");
            throw;
        }
    }
}
