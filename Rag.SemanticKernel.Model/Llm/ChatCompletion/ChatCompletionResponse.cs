using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rag.SemanticKernel.Model.Llm.ChatCompletion;

// Response classes for  API
public class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; }

    [JsonPropertyName("usage")]
    public Usage Usage { get; set; }
}
