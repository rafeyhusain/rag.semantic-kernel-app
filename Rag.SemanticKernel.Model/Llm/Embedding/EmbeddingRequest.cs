using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rag.SemanticKernel.Model.Llm.Embedding;

public class EmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; }

    [JsonPropertyName("input")]
    public IList<string> Input { get; }

    public EmbeddingRequest(string model, IList<string> input)
    {
        Model = model;
        Input = input;
    }

    public override bool Equals(object obj)
    {
        return obj is EmbeddingRequest other &&
               Model == other.Model &&
               EqualityComparer<IList<string>>.Default.Equals(Input, other.Input);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Model, Input);
    }
}