using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rag.SemanticKernel.Model.Llm.ChatCompletion;

public class ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; }
    [JsonPropertyName("messages")]
    public List<object> Messages { get; }
    [JsonPropertyName("temperature")]
    public double Temperature { get; }
    [JsonPropertyName("max_tokens")]
    public int Max_tokens { get; }
    [JsonPropertyName("top_p")]
    public double Top_p { get; }
    [JsonPropertyName("stream")]
    public bool Stream { get; }

    public ChatCompletionRequest(string model, List<object> messages, double temperature, int max_tokens, double top_p, bool stream)
    {
        Model = model;
        Messages = messages;
        Temperature = temperature;
        Max_tokens = max_tokens;
        Top_p = top_p;
        Stream = stream;
    }

    public override bool Equals(object obj)
    {
        return obj is ChatCompletionRequest other &&
               Model == other.Model &&
               EqualityComparer<List<object>>.Default.Equals(Messages, other.Messages) &&
               Temperature == other.Temperature &&
               Max_tokens == other.Max_tokens &&
               Top_p == other.Top_p &&
               Stream == other.Stream;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Model, Messages, Temperature, Max_tokens, Top_p, Stream);
    }
}