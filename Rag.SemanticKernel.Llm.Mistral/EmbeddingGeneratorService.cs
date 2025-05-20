using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Rag.SemanticKernel.Abstractions.Parser;
using Rag.SemanticKernel.Guards;
using Rag.SemanticKernel.Model.Vector;

namespace Rag.SemanticKernel.Llm.Mistral;

public class EmbeddingGeneratorService
{
    private readonly ILogger<EmbeddingService> _logger;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IVectorStoreRecordCollection<string, Markdown> _vectorStoreCollection;
    private readonly IFileParser _parser;
    private readonly Kernel _kernel;

    public EmbeddingGeneratorService(
        Kernel kernel, 
        ILogger<EmbeddingService> logger,
        ITextEmbeddingGenerationService embeddingService,
        IVectorStoreRecordCollection<string, Markdown> vectorStoreCollection,
        IFileParser parser)
    {
        _kernel = Guard.ThrowIfNull(kernel);
        _logger = Guard.ThrowIfNull(logger);
        _embeddingService = Guard.ThrowIfNull(embeddingService);
        _vectorStoreCollection = Guard.ThrowIfNull(vectorStoreCollection);
        _parser = Guard.ThrowIfNull(parser);
    }

    public async Task Generate(EmbeddingGeneratorServiceOptions options)
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

    private async Task CreateEmbeddingsFile(string filePath, EmbeddingGeneratorServiceOptions options)
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

                await _vectorStoreCollection.UpsertAsync(new Markdown
                {
                    MarkdownId = Guid.NewGuid().ToString(),
                    FileName = _parser.FileName,
                    Url = _parser.FilePath,
                    Text = block.Text,
                    Content = block.Content,
                    Embeddings = embeddings[i]
                });
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
                return await _embeddingService.GenerateEmbeddingsAsync(texts);
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
}
