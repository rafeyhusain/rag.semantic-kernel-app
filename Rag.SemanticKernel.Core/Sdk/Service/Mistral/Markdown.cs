using System;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;


/// <summary>
/// Class that represents a Markdown document with vector embeddings
/// </summary>
internal class Markdown
{
    [VectorStoreRecordKey]
    public string Id { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public string FileName { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public string Url { get; set; }

    [TextSearchResultName]
    [VectorStoreRecordData(IsFilterable = true)]
    public string Heading { get; set; }

    [TextSearchResultValue]
    [VectorStoreRecordData(IsFullTextSearchable = true)]
    public string Content { get; set; }

    [VectorStoreRecordVector(Dimensions: 1536, DistanceFunction.CosineSimilarity, IndexKind.Hnsw)]
    public ReadOnlyMemory<float>? Embeddings { get; set; }
}
