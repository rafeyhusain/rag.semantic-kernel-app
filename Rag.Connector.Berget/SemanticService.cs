using Microsoft.SemanticKernel;

namespace Rag.Connector.Berget;

public class SemanticService : Core.Api.SemanticService<Markdown>
{
    public SemanticService(
        Kernel kernel,
        ChatCompletionService chatCompletionService,
        EmbeddingService embeddingService) 
        : base(kernel, chatCompletionService, embeddingService)
    {
    }
}
