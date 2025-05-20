using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;
using Rag.SemanticKernel.AppSettings;
using System;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable SKEXP0010 // Some SK methods are still experimental

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

public static class SemanticServiceExtensions
{
    private const string ModelName = "Mistral";

    public static void AddSemanticService(this IServiceCollection services, Settings settings)
    {
        var kernelBuilder = services.AddKernel();

        kernelBuilder.AddMistralTextEmbeddingGeneration(settings);

        kernelBuilder.AddMistralChatCompletion(settings);

        kernelBuilder.AddVectorStoreTextSearch<Markdown>();

        var elasticsearchClientSettings = new ElasticsearchClientSettings(new Uri(settings.Elasticsearch.Url))
            .Authentication(new BasicAuthentication(settings.Elasticsearch.User, settings.Elasticsearch.Password));

        kernelBuilder.AddElasticsearchVectorStoreRecordCollection<string, Markdown>(settings.Elasticsearch.Index, elasticsearchClientSettings);

        services.AddSingleton<QuestionService>();
        services.AddSingleton<EmbeddingGeneratorService>();
    }

    public static IKernelBuilder AddMistralTextEmbeddingGeneration(
        this IKernelBuilder builder,
        Settings settings)
    {
        var model = settings[ModelName];

        builder.Services.AddKeyedSingleton<ITextEmbeddingGenerationService>(model.EmbeddingModel, (sp, _) =>
            new EmbeddingService(
                sp.GetService<ILogger<EmbeddingService>>(),
                model
            ));

        // Register default embedding service (non-keyed) for VectorStoreTextSearch<T>
        builder.Services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
            new EmbeddingService(
                sp.GetService<ILogger<EmbeddingService>>(),
                model
            ));

        return builder;
    }

    public static IKernelBuilder AddMistralChatCompletion(
      this IKernelBuilder builder,
      Settings settings)
    {
        var model = settings[ModelName];

        Func<IServiceProvider, object, ChatCompletionService> factory = (serviceProvider, _) =>
        {
            return new ChatCompletionService(
                serviceProvider.GetService<ILogger<ChatCompletionService>>(),
                model
            );
        };

        builder.Services.AddKeyedSingleton<IChatCompletionService>(model.CompletionModel, factory);
        builder.Services.AddKeyedSingleton<ITextGenerationService>(model.CompletionModel, factory);

        // Register default (non-keyed) ITextGenerationService
        builder.Services.AddSingleton<ITextGenerationService>(sp =>
            new ChatCompletionService(
                sp.GetService<ILogger<ChatCompletionService>>(),
                model
            ));

        return builder;
    }

}

