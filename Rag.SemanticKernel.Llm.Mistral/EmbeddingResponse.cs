using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rag.SemanticKernel.Llm.Mistral;

/// <summary>
/// Response model for Mistral embeddings API
/// </summary>
public class EmbeddingResponse
{
    [JsonPropertyName("data")]
    public List<EmbeddingData> Data { get; set; } = [];
}
