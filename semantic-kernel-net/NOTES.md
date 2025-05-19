# Notes

`Elastic.SemanticKernel.Connectors.Elasticsearch` pre-release is not sending `num_candidates` when it builds `search` query. [This](https://github.com/marcarl/rag-elastic/blob/d1bfcf5537d3542166b09039f6b2aae5b962357f/semantic-kernel-net/Elastic.SemanticKernel.Connectors.Elasticsearch/ElasticsearchVectorStoreRecordCollection.cs#L347) line sends it.

```csharp
knnQuery.NumCandidates = 100;
```

In Elasticsearch, when doing vector search using the knn query with HNSW indexing, you must provide `num_candidates`. It is number of candidates to consider during the approximate search.

```bash
curl -X POST "http://localhost:9200/hotels/_search" -H "Content-Type: application/json" -d '
{
  "knn": {
    "field": "embeddings",
    "query_vector": [-0.0268249512, 0.123, ...], 
    "k": 5,
    "num_candidates": 100
  }
}'
```