using System;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace Rag.Core.Sdk.Service.Azure;

public sealed record Markdown
{
    [VectorStoreRecordKey]
    public required string MarkdownId { get; set; }

    [TextSearchResultName]
    [VectorStoreRecordData(IsFilterable = true)]
    public required string MarkdownName { get; set; }

    [TextSearchResultValue]
    [VectorStoreRecordData(IsFullTextSearchable = true)]
    public required string Description { get; set; }

    [VectorStoreRecordVector(Dimensions: 1536, DistanceFunction.CosineSimilarity, IndexKind.Hnsw)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }

    [TextSearchResultLink]
    [VectorStoreRecordData]
    public string? ReferenceLink { get; set; }
}