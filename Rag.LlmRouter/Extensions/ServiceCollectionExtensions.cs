using Microsoft.Extensions.DependencyInjection;
using Rag.Abstractions.Pairs;
using Rag.AppSettings;
using Rag.Connector.Core.Extensions;
using Rag.Parser.Markdown;

namespace Rag.LlmRouter.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddLlms(this IServiceCollection services, Settings settings)
    {
        services.AddTransient<Connector.Mistral.EmbeddingService>();
        services.AddTransient<Connector.Mistral.ChatCompletionService>();
        services.AddTransient<Connector.Mistral.SemanticService>();
        services.AddSemanticService<Connector.Mistral.Markdown, MarkdownFileParser>(settings, ModelPairs.Mistral);

        services.AddTransient<Connector.OpenAi.EmbeddingService>();
        services.AddTransient<Connector.OpenAi.ChatCompletionService>();
        services.AddTransient<Connector.OpenAi.SemanticService>();
        services.AddSemanticService<Connector.OpenAi.Markdown, MarkdownFileParser>(settings, ModelPairs.OpenAi);

        services.AddTransient<Connector.Berget.EmbeddingService>();
        services.AddTransient<Connector.Berget.ChatCompletionService>();
        services.AddTransient<Connector.Berget.SemanticService>();
        services.AddSemanticService<Connector.Berget.Markdown, MarkdownFileParser>(settings, ModelPairs.Berget);

        services.AddTransient<Connector.Scaleway.EmbeddingService>();
        services.AddTransient<Connector.Scaleway.ChatCompletionService>();
        services.AddTransient<Connector.Scaleway.SemanticService>();
        services.AddSemanticService<Connector.Scaleway.Markdown, MarkdownFileParser>(settings, ModelPairs.Scaleway);
    }
}
