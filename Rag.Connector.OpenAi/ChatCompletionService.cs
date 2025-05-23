using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Rag.AppSettings;
using Rag.Rest;

namespace Rag.Connector.OpenAi;

/// <summary>
/// Chat Completion Service for OpenAi
/// </summary>
public class ChatCompletionService : Core.ChatCompletion.ChatCompletionService<Markdown>
{
    public ChatCompletionService(
        Kernel kernel,
        ILogger<ChatCompletionService> logger, 
        RestService restService,
        VectorStoreTextSearch<Markdown> searchService,
        ModelPairSettings pairSettings) : base(kernel, logger, restService, searchService, pairSettings)
    {
        RefreshModelPair();
    }

    public override string PairName => Abstractions.Pairs.ModelPairs.OpenAi;
}