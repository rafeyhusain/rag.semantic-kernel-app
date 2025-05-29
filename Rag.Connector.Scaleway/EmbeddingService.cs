using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Rag.Abstractions.Parser;
using Rag.AppSettings;
using Rag.Rest;

namespace Rag.Connector.Scaleway;

/// <summary>
/// Embedding Service for Scaleway embeddings
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

    public override string PairName => Abstractions.Pairs.ModelPairs.Scaleway;
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