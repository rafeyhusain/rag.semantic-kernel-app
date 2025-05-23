using Microsoft.SemanticKernel;

namespace Rag.Connector.OpenAi;

public class SemanticService : Core.Api.SemanticService<Markdown>
{
    public SemanticService(
        Kernel kernel,
        ChatCompletionService chatCompletionService,
        EmbeddingService embeddingService) 
        : base(kernel, chatCompletionService, embeddingService)
    {
    }

    public override string PairName => Abstractions.Pairs.ModelPairs.OpenAi;
}
