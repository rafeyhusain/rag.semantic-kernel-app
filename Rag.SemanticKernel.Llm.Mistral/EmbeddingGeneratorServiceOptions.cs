using System;
using System.IO;

namespace Rag.SemanticKernel.Llm.Mistral;

public class EmbeddingGeneratorServiceOptions
{
    public string InputFolder { get; set; } = Path.Combine(AppContext.BaseDirectory, @"data\input");
    public string CompletedFolder { get; set; } = Path.Combine(AppContext.BaseDirectory, @"data\completed");
    public bool IncludeSubfolders { get; set; } = false;
    public string Extension { get; set; } = ".md";
}
