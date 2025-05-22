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

        // Model-specific registration (can be extended with switch or reflection)
        kernelBuilder.AddTextEmbeddingGeneration<T, TRecord>(pairSettings);
        kernelBuilder.AddChatCompletion(pairSettings);

        // Register vector search for the specified document type
        kernelBuilder.AddVectorStoreTextSearch<TRecord>();

        // Elasticsearch setup
        var elasticSettings = new ElasticsearchClientSettings(new Uri(settings.Elasticsearch.Url))
            .Authentication(new BasicAuthentication(settings.Elasticsearch.User, settings.Elasticsearch.Password));

        kernelBuilder.AddElasticsearchVectorStoreRecordCollection<string, TRecord>(settings.Elasticsearch[pairSettings.Name], elasticSettings);

        // Register dependent services
        services.AddTransient<IFileParser, P>();
        services.AddTransient<ModelPairSettings>();
        services.AddTransient<RestService>();

        // Register a _kernel instance with plugin injection
        services.AddTransient<VectorStoreTextSearch<TRecord>>();
        services.AddTransient<IKernelPluginInjector, KernelPluginInjector<TRecord>>();

        services.AddSingleton(LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        }));

        services.AddTransient<ModelPairSettings>(_ => settings[modelPair]);

        // Register Kernel with plugin injection
        services.AddTransient(sp =>
        {
            var kernel = Kernel.CreateBuilder().Build();

            // Inject all available plugin injectors
            var injectors = sp.GetServices<KernelPluginInjector<T>>();
            foreach (var injector in injectors)
            {
                injector.InjectPlugins(kernel);
            }

            return kernel;
        });
    }

    /// <summary>
    /// Registers text embedding generation service based on the pairSettings name.
    /// </summary>
    public static IKernelBuilder AddTextEmbeddingGeneration<T, TRecord>(
        this IKernelBuilder builder,
        ModelPairSettings pairSettings)
        where T : class, IDocument, new()
        where TRecord : class
    {
        builder.Services.AddKeyedSingleton<ITextEmbeddingGenerationService>(pairSettings.EmbeddingModel, (sp, _) =>
            new EmbeddingService<T, TRecord>(
                sp.GetRequiredService<ILogger<EmbeddingService<T, TRecord>>>(),
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<IVectorStoreRecordCollection<string, TRecord>>(),
                sp.GetRequiredService<IFileParser>(),
                sp.GetRequiredService<RestService>(),
                pairSettings));

        builder.Services.AddTransient<ITextEmbeddingGenerationService>(sp =>
            new EmbeddingService<T, TRecord>(
                sp.GetRequiredService<ILogger<EmbeddingService<T, TRecord>>>(),
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<IVectorStoreRecordCollection<string, TRecord>>(),
                sp.GetRequiredService<IFileParser>(),
                sp.GetRequiredService<RestService>(),
                pairSettings));

        return builder;
    }

    /// <summary>
    /// Registers chat completion and text generation services based on the pairSettings name.
    /// </summary>
    public static IKernelBuilder AddChatCompletion(
        this IKernelBuilder builder,
        ModelPairSettings pairSettings)
    {
        Func<IServiceProvider, object, ChatCompletionService> factory = (sp, _) =>
            new ChatCompletionService(
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<ILogger<ChatCompletionService>>(),
                sp.GetRequiredService<RestService>(),
                pairSettings);

        builder.Services.AddKeyedTransient<IChatCompletionService>(pairSettings.CompletionModel, factory!);
        builder.Services.AddKeyedTransient<ITextGenerationService>(pairSettings.CompletionModel, factory!);

        builder.Services.AddTransient<IChatCompletionService>(sp =>
            new ChatCompletionService(
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<ILogger<ChatCompletionService>>(),
                sp.GetRequiredService<RestService>(),
                pairSettings));

        builder.Services.AddTransient<ITextGenerationService>(sp =>
        new ChatCompletionService(
            sp.GetRequiredService<Kernel>(),
            sp.GetRequiredService<ILogger<ChatCompletionService>>(),
            sp.GetRequiredService<RestService>(),
            pairSettings));

        return builder;
    }
}
