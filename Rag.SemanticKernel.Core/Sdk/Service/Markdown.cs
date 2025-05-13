//using Microsoft.Extensions.VectorData;

//namespace Rag.SemanticKernel.Core.Sdk.Service;


///// <summary>
///// Class that represents a Markdown document with vector embeddings
///// </summary>
//internal class Markdown
//{
//    [VectorStoreRecordKey]
//    public string Id { get; set; }

//    [VectorStoreRecordData(IsIndexed = true)]
//    public string FileName { get; set; }

//    [VectorStoreRecordData(IsFullTextIndexed = true)]
//    public string Heading { get; set; }

//    [VectorStoreRecordData(IsFullTextIndexed = true)]
//    public string Content { get; set; }

//    [VectorStoreRecordVector(Dimensions: 4, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
//    public ReadOnlyMemory<float>? ContentEmbedding { get; set; }
//}
