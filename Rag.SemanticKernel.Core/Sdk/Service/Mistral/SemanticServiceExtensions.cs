using System;
using System.Net.Http;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable SKEXP0010 // Some SK methods are still experimental

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

public static class SemanticServiceExtensions
{
    public static void AddSemanticService(this IServiceCollection services, IConfiguration configuration)
    {
        var kernelBuilder = services.AddKernel();

        kernelBuilder.AddMistralTextEmbeddingGeneration();

        //var endpoint = configuration["Mistral:Endpoint"];
        //var apiKey = configuration["Mistral:ApiKey"];
        //var completionModel = configuration["Mistral:CompletionModel"];
        //var embeddingModel = configuration["Mistral:EmbeddingModel"];

        //kernelBuilder.AddAzureOpenAIChatCompletion(
        //    completionModel ?? "gpt-4o",
        //    endpoint,
        //    apiKey);

        //kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
        //    embeddingModel ?? "ada-002",
        //    endpoint,
        //    apiKey);

        kernelBuilder.AddVectorStoreTextSearch<Hotel>();

        var elasticUrl = configuration["Elasticsearch:Url"];
        var elasticUser = configuration["Elasticsearch:User"];
        var elasticPassword = configuration["Elasticsearch:Password"];
        var elasticIndex = configuration["Elasticsearch:Index"] ?? "skhotels";

        var elasticsearchClientSettings = new ElasticsearchClientSettings(new Uri(elasticUrl))
            .Authentication(new BasicAuthentication(elasticUser, elasticPassword));

        kernelBuilder.AddElasticsearchVectorStoreRecordCollection<string, Hotel>(elasticIndex, elasticsearchClientSettings);
    }

    public static void AddSemanticService(this IHost host)
    {
        var kernel = host.Services.GetService<Kernel>()!;
        var textSearch = host.Services.GetService<VectorStoreTextSearch<Hotel>>()!;
        kernel.Plugins.Add(textSearch.CreateWithGetTextSearchResults("SearchPlugin"));
    }

    public static IKernelBuilder AddMistralTextEmbeddingGeneration(
     this IKernelBuilder builder,
     string? serviceId = null)
    {
        builder.Services.AddKeyedSingleton<ITextEmbeddingGenerationService>(serviceId, (serviceProvider, _) =>
            new EmbeddingService());

        return builder;
    }
}

