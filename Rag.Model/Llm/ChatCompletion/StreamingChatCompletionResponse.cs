using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rag.Model.Llm.ChatCompletion;

public class StreamingChatCompletionResponse
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
    public List<StreamingChoice> Choices { get; set; }
}
