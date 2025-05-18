using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;
using System;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable SKEXP0010 // Some SK methods are still experimental

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

public static class SemanticServiceExtensions
{
    public static void AddSemanticService(this IServiceCollection services, IConfiguration configuration)
    {
        var kernelBuilder = services.AddKernel();

        var endpoint = configuration["Mistral:Endpoint"];
        var apiKey = configuration["Mistral:ApiKey"];
        var completionModel = configuration["Mistral:CompletionModel"];
        var embeddingModel = configuration["Mistral:EmbeddingModel"];

        kernelBuilder.AddMistralTextEmbeddingGeneration(
            embeddingModel,
            endpoint,
            apiKey
            );

        kernelBuilder.AddMistralChatCompletion(
            completionModel,
            endpoint,
            apiKey
            );

        kernelBuilder.AddVectorStoreTextSearch<Markdown>();

        var elasticUrl = configuration["Elasticsearch:Url"];
        var elasticUser = configuration["Elasticsearch:User"];
        var elasticPassword = configuration["Elasticsearch:Password"];
        var elasticIndex = configuration["Elasticsearch:Index"] ?? "markdown";

        var elasticsearchClientSettings = new ElasticsearchClientSettings(new Uri(elasticUrl))
            .Authentication(new BasicAuthentication(elasticUser, elasticPassword));

        kernelBuilder.AddElasticsearchVectorStoreRecordCollection<string, Markdown>(elasticIndex, elasticsearchClientSettings);

        services.AddSingleton<QuestionService>();
        services.AddSingleton<EmbeddingGeneratorService>();
    }

    public static void AddSemanticService(this IHost host, Kernel kernel)
    {
        var textSearch = host.Services.GetService<VectorStoreTextSearch<Markdown>>()!;
        kernel.Plugins.Add(textSearch.CreateWithGetTextSearchResults("SearchPlugin"));
    }

    public static IKernelBuilder AddMistralTextEmbeddingGeneration(
        this IKernelBuilder builder,
        string embeddingModel = null,
        string endpoint = null,
        string apiKey = null)
    {
        builder.Services.AddKeyedSingleton<ITextEmbeddingGenerationService>(embeddingModel, (sp, _) =>
            new EmbeddingService(
                sp.GetService<ILogger<EmbeddingService>>(),
                endpoint,
                apiKey,
                embeddingModel
            ));

        // Register default embedding service (non-keyed) for VectorStoreTextSearch<T>
        builder.Services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
            new EmbeddingService(
                sp.GetService<ILogger<EmbeddingService>>(),
                endpoint,
                apiKey,
                embeddingModel
            ));

        return builder;
    }

    public static IKernelBuilder AddMistralChatCompletion(
      this IKernelBuilder builder,
      string completionModel = null,
      string endpoint = null,
      string apiKey = null)
    {
        Func<IServiceProvider, object, ChatCompletionService> factory = (serviceProvider, _) =>
        {
            return new ChatCompletionService(
                serviceProvider.GetService<ILogger<ChatCompletionService>>(),
                endpoint,
                apiKey,
                completionModel
            );
        };

        builder.Services.AddKeyedSingleton<IChatCompletionService>(completionModel, factory);
        builder.Services.AddKeyedSingleton<ITextGenerationService>(completionModel, factory);

        // Register default (non-keyed) ITextGenerationService
        builder.Services.AddSingleton<ITextGenerationService>(sp =>
            new ChatCompletionService(
                sp.GetService<ILogger<ChatCompletionService>>(),
                endpoint,
                apiKey,
                completionModel
            ));

        return builder;
    }

}

