using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

public class QuestionService
{
    private readonly ILogger<QuestionService> _logger;

    public QuestionService(ILogger<QuestionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Kernel Kernel { get; internal set; }

    public async Task<string> Ask(string question, QuestionServiceOptions options)
    {
        try
        {
            return await AskWithRetry(question, options);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in QuestionService.Ask");
            throw;
        }
    }

    public async Task<string> AskWithRetry(string question, QuestionServiceOptions options, int maxRetries = 5)
    {
        int delay = 1000;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var response = await Kernel.InvokePromptAsync(
                        promptTemplate: options.HandlebarTemplate.Prompt,
                        arguments: new KernelArguments
                        {
                            { "question", question }
                        },
                        templateFormat: "handlebars",
                        promptTemplateFactory: new HandlebarsPromptTemplateFactory());

                var result = response.ToString();

                return result;
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("TooManyRequests"))
            {
                _logger.LogWarning("Rate limited. Retrying in {Delay}ms (Attempt {Attempt}/{Max})", delay, attempt + 1, maxRetries);
                await Task.Delay(delay);
                delay *= 2;
            }
        }

        throw new Exception("Failed to generate embeddings after retries.");
    }
}
