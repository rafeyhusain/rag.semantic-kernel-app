using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace Rag.Connector.Berget;

/// <summary>
/// Class that represents a Markdown document with vector embeddings
/// </summary>
public sealed record Markdown
{
    [VectorStoreRecordKey]
    public required string MarkdownId { get; set; }

    [TextSearchResultName]
    [VectorStoreRecordData(IsFilterable = true)]
    public required string FileName { get; set; }

    [TextSearchResultValue]
    [VectorStoreRecordData(IsFullTextSearchable = true)]
    public required string Content { get; set; }

    [VectorStoreRecordVector(Dimensions: 1024, DistanceFunction.CosineSimilarity, IndexKind.Hnsw)]
    public ReadOnlyMemory<float>? Embeddings { get; set; }

    [TextSearchResultLink]
    [VectorStoreRecordData]
    public string Text { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public string Url { get; set; }
}
