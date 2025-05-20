using Microsoft.SemanticKernel;
using Rag.SemanticKernel.Llm.Core.Embedding;
using Rag.SemanticKernel.Model.Vector;

namespace Rag.SemanticKernel.Llm.Mistral;

public class SemanticService<T, TRecord> : Core.Api.SemanticService<T, TRecord>
    where T : class, IDocument, new()
    where TRecord : class
{
    public SemanticService(
        Kernel kernel,
        ChatCompletionService chatCompletionService,
        EmbeddingService<T, TRecord> embeddingService) 
        : base(kernel, chatCompletionService, embeddingService)
    {
    }
}
