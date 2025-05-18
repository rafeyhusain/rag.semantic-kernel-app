using System;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable SKEXP0010 // Some SK methods are still experimental

namespace Rag.SemanticKernel.Core.Sdk.Service.Azure;

public static class SemanticServiceExtensions
{
    public static void AddSemanticService(this IServiceCollection services, IConfiguration configuration)
    {
        var kernelBuilder = services.AddKernel();

        var endpoint = configuration["AzureAI:Endpoint"];
        var apiKey = configuration["AzureAI:ApiKey"];
        var completionModel = configuration["AzureAI:CompletionModel"];
        var embeddingModel = configuration["AzureAI:EmbeddingModel"];

        kernelBuilder.AddAzureOpenAIChatCompletion(
            completionModel ?? "gpt-4o",
            endpoint,
            apiKey);

        kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
            embeddingModel ?? "ada-002",
            endpoint,
            apiKey);

        kernelBuilder.AddVectorStoreTextSearch<Markdown>();

        var elasticUrl = configuration["Elasticsearch:Url"];
        var elasticUser = configuration["Elasticsearch:User"];
        var elasticPassword = configuration["Elasticsearch:Password"];
        var elasticIndex = configuration["Elasticsearch:Index"] ?? "skMarkdowns";

        var elasticsearchClientSettings = new ElasticsearchClientSettings(new Uri(elasticUrl))
            .Authentication(new BasicAuthentication(elasticUser, elasticPassword));

        kernelBuilder.AddElasticsearchVectorStoreRecordCollection<string, Markdown>(elasticIndex, elasticsearchClientSettings);
    }

    public static void AddSemanticService(this IHost host)
    {
        var kernel = host.Services.GetService<Kernel>()!;
        var textSearch = host.Services.GetService<VectorStoreTextSearch<Markdown>>()!;
        kernel.Plugins.Add(textSearch.CreateWithGetTextSearchResults("SearchPlugin"));
    }
}

