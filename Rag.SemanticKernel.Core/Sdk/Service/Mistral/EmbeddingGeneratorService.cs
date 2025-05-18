using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Rag.SemanticKernel.Core.Sdk.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

public class EmbeddingGeneratorService
{
    private readonly ILogger<EmbeddingService> _logger;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IVectorStoreRecordCollection<string, Markdown> _vectorStoreCollection;

    public EmbeddingGeneratorService(
        ILogger<EmbeddingService> logger,
        ITextEmbeddingGenerationService embeddingService,
        IVectorStoreRecordCollection<string, Markdown> vectorStoreCollection)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _vectorStoreCollection = vectorStoreCollection ?? throw new ArgumentNullException(nameof(vectorStoreCollection));
    }

    public async Task Generate(Kernel kernel, EmbeddingGeneratorServiceOptions options)
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

        var file = new MarkdownFile();

        file.Parse(filePath);

        foreach (var chunk in file.Headings.Chunk(25))
        {
            var contents = chunk.Select(x => x.Content).ToArray();

            var embeddings = await GenerateWithRetry(contents);

            for (var i = 0; i < chunk.Length && i < embeddings.Count; ++i)
            {
                var heading = chunk[i];

                await _vectorStoreCollection.UpsertAsync(new Markdown
                {
                    Id = "test-id",
                    FileName = "test.md",
                    Url = "test/path",
                    Heading = "Test Heading",
                    Content = "This is test content",
                    Embeddings = embeddings[i]
                });

                //await _vectorStoreCollection.UpsertAsync(new Markdown
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    FileName = file.FileName,
                //    Url = file.FilePath,
                //    Heading = heading.Text,
                //    Content = heading.Content,
                //    Embeddings = embeddings[i]
                //});
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
