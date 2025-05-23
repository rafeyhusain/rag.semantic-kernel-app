using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Rag.SemanticKernel.Abstractions.Parser;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Rest;

namespace Rag.Connector.OpenAi;

/// <summary>
/// Embedding Service for OpenAi embeddings
/// </summary>
public class EmbeddingService : Core.Embedding.EmbeddingService<Markdown>
{
    public EmbeddingService(
        ILogger<EmbeddingService> logger,
        Kernel kernel,
        IVectorStoreRecordCollection<string, Markdown> vectorStoreCollection,
        IFileParser parser,
        RestService restService,
        ModelPairSettings modelSettings
    ) : base(logger, kernel, vectorStoreCollection, parser, restService, modelSettings)
    {
        RefreshModelPair();
    }
    
    public override string PairName => SemanticKernel.Abstractions.Pairs.ModelPairs.OpenAi;
    protected override async Task UpsertAsync(
    string id,
    string fileName,
    string filePath,
    string text,
    string content,
    ReadOnlyMemory<float> embeddings)
    {
        var record = new Markdown()
        {
            MarkdownId = id,
            FileName = fileName,
            Url = filePath,
            Text = text,
            Content = content,
            Embeddings = embeddings
        };

        await _vectorStoreCollection.UpsertAsync(record!);
    }
}