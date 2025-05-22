using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;
using Rag.SemanticKernel.Abstractions.Parser;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Llm.Core.ChatCompletion;
using Rag.SemanticKernel.Llm.Core.Embedding;
using Rag.SemanticKernel.Llm.Core.Plugins;
using Rag.SemanticKernel.Model.Vector;
using Rag.SemanticKernel.Rest;

namespace Rag.SemanticKernel.Llm.Core.Extensions;

/// <summary>
/// Extension methods for registering semantic services based on a configurable pairSettings and generic document type.
/// </summary>
public static class SemanticServiceExtensions
{
    /// <summary>
    /// Registers semantic services for the given pairSettings name and document type.
    /// </summary>
    public static void AddSemanticService<T, TRecord, P>(this IServiceCollection services, Settings settings, string modelPair) 
        where T : class, IDocument, new() 
        where TRecord : class
        where P : class, IFileParser
    {
        var pairSettings = settings[modelPair];

        var kernelBuilder = services.AddKernel();

        // Model-specific registration
        kernelBuilder.Services.AddTransient<ITextEmbeddingGenerationService, EmbeddingService<T, TRecord>>();
        kernelBuilder.Services.AddTransient<IChatCompletionService, ChatCompletionService<TRecord>>();
        kernelBuilder.Services.AddTransient<ITextGenerationService, ChatCompletionService<TRecord>>();

        // Register vector search for the specified document type
        kernelBuilder.AddVectorStoreTextSearch<TRecord>();

        // Elasticsearch setup
        var elasticSettings = new ElasticsearchClientSettings(new Uri(settings.Elasticsearch.Url))
            .Authentication(new BasicAuthentication(settings.Elasticsearch.User, settings.Elasticsearch.Password));

        kernelBuilder.AddElasticsearchVectorStoreRecordCollection<string, TRecord>(settings.Elasticsearch[pairSettings.Name], elasticSettings);

        // Register dependent services
        services.AddTransient<IFileParser, P>();
        services.AddTransient<RestService>();

        services.AddTransient<VectorStoreTextSearch<TRecord>>();
        services.AddTransient<KernelPluginInjector<TRecord>>();
        services.AddTransient<ModelPairSettings>(_ => settings[modelPair]);
    }
}
