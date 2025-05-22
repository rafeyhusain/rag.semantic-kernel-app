using System.Text.Json.Serialization;

namespace Rag.SemanticKernel.Model.Llm.ChatCompletion;

public class ChatChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
}
