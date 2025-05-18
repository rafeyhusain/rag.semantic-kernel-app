using System;
using System.IO;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable SKEXP0010 // Some SK methods are still experimental

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

public class EmbeddingGeneratorServiceOptions
{
    public string InputFolder { get; set; } = Path.Combine(AppContext.BaseDirectory, @"data\input");
    public string CompletedFolder { get; set; } = Path.Combine(AppContext.BaseDirectory, @"data\completed");
    public bool IncludeSubfolders { get; set; } = false;
    public string Extension { get; set; } = ".csv";
}
