using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;
using Rag.Abstractions.Parser;
using Rag.AppSettings;
using Rag.Connector.Core.ChatCompletion;
using Rag.Connector.Core.Embedding;
using Rag.Connector.Core.Plugins;
using Rag.Rest;

namespace Rag.Connector.Core.Extensions;

/// <summary>
/// Extension methods for registering semantic services based on a configurable pairSettings and generic document type.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers semantic services for the given pairSettings name and document type.
    /// </summary>
    public static void AddSemanticService<TRecord, P>(this IServiceCollection services, Settings settings, string modelPair)
        where TRecord : class
        where P : class, IFileParser
    {
        var pairSettings = settings[modelPair];

        var kernelBuilder = services.AddKernel();

        // scoped
        kernelBuilder.Services.AddTransient<ITextEmbeddingGenerationService, EmbeddingService<TRecord>>();
        kernelBuilder.Services.AddTransient<IChatCompletionService, ChatCompletionService<TRecord>>();
        kernelBuilder.Services.AddTransient<ITextGenerationService, ChatCompletionService<TRecord>>();

        services.AddTransient<IFileParser, P>();
        services.AddTransient<VectorStoreTextSearch<TRecord>>();
        services.AddTransient<ModelPairSettings>();
        services.AddTransient<RestService>();

        // singleton
        services.AddSingleton<KernelPluginInjector<TRecord>>();

        // Register vector search for the specified document type
        kernelBuilder.AddVectorStoreTextSearch<TRecord>();

        // Elasticsearch setup
        var elasticSettings = new ElasticsearchClientSettings(new Uri(settings.Elasticsearch.Url))
            .Authentication(new BasicAuthentication(settings.Elasticsearch.User, settings.Elasticsearch.Password));

        kernelBuilder.AddElasticsearchVectorStoreRecordCollection<string, TRecord>(settings.Elasticsearch[pairSettings.Name], elasticSettings);
    }
}
