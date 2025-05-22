using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Rest;

namespace Rag.SemanticKernel.Llm.Mistral;

/// <summary>
/// Chat Completion Service for Mistral
/// </summary>
public class ChatCompletionService : Core.ChatCompletion.ChatCompletionService
{
    public ChatCompletionService(
        Kernel kernel,
        ILogger<ChatCompletionService> logger, 
        RestService restService,
        ModelPairSettings modelSettings) : base(kernel, logger, restService, modelSettings)
    {
    }
}