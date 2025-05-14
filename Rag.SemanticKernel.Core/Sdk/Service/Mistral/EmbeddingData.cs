using System;
using System.Text.Json.Serialization;

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

/// <summary>
/// Data model for Mistral embeddings
/// </summary>
public class EmbeddingData
{
    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = Array.Empty<float>();
}