using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Rest;

namespace Rag.Connector.Berget;

/// <summary>
/// Chat Completion Service for Berget
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
    }
}