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

public static class EmbeddingServiceExtensions
{
    public static void AddAzureEmbeddingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var kernelBuilder = services.AddKernel();

        // These could be read from appsettings.json if desired
        var openAiEndpoint = configuration["AzureAI:Endpoint"];
        var openAiKey = configuration["AzureAI:Key"];
        var gptModel = configuration["AzureAI:GptModel"];
        var embeddingModel = configuration["AzureAI:EmbeddingModel"];

        kernelBuilder.AddAzureOpenAIChatCompletion(
            gptModel ?? "gpt-4o",
            openAiEndpoint,
            openAiKey);

        kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
            embeddingModel ?? "ada-002",
            openAiEndpoint,
            openAiKey);

        kernelBuilder.AddVectorStoreTextSearch<Hotel>();

        var elasticUrl = configuration["Elasticsearch:Url"];
        var elasticUser = configuration["Elasticsearch:User"];
        var elasticPassword = configuration["Elasticsearch:Password"];
        var elasticIndex = configuration["Elasticsearch:Index"] ?? "skhotels";

        var elasticsearchClientSettings = new ElasticsearchClientSettings(new Uri(elasticUrl))
            .Authentication(new BasicAuthentication(elasticUser, elasticPassword));

        kernelBuilder.AddElasticsearchVectorStoreRecordCollection<string, Hotel>(elasticIndex, elasticsearchClientSettings);

    }
}

