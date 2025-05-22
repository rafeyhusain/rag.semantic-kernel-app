using System.Text.Json.Serialization;

namespace Rag.SemanticKernel.Model.Llm.ChatCompletion;

public class StreamingChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("delta")]
    public Delta Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
}
