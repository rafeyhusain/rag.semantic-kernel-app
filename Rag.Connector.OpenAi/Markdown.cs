using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using System;

namespace Rag.Connector.OpenAi;

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

    [VectorStoreRecordVector(Dimensions: 3072, DistanceFunction.CosineSimilarity, IndexKind.Hnsw)]
    public ReadOnlyMemory<float>? Embeddings { get; set; }

    [TextSearchResultLink]
    [VectorStoreRecordData]
    public string Text { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public string Url { get; set; }
}
