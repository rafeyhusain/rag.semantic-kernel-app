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

namespace Rag.SemanticKernel.Llm.Core.Extensions;

/// <summary>
/// Extension methods for registering semantic services based on a configurable model and generic document type.
/// </summary>
public static class SemanticServiceExtensions
{
    /// <summary>
    /// Registers semantic services for the given model name and document type.
    /// </summary>
    public static void AddSemanticService<T, TRecord, P>(this IServiceCollection services, Settings settings, string modelName) 
        where T : class, IDocument, new() 
        where TRecord : class
        where P : class, IFileParser
    {
        var model = settings[modelName];

        var kernelBuilder = services.AddKernel();

        // Model-specific registration (can be extended with switch or reflection)
        kernelBuilder.AddTextEmbeddingGeneration<T, TRecord>(model);
        kernelBuilder.AddChatCompletion(model);

        // Register vector search for the specified document type
        kernelBuilder.AddVectorStoreTextSearch<TRecord>();

        // Elasticsearch setup
        var elasticSettings = new ElasticsearchClientSettings(new Uri(settings.Elasticsearch.Url))
            .Authentication(new BasicAuthentication(settings.Elasticsearch.User, settings.Elasticsearch.Password));

        kernelBuilder.AddElasticsearchVectorStoreRecordCollection<string, TRecord>(settings.Elasticsearch[model.Name], elasticSettings);

        // Register dependent services
        services.AddTransient<IFileParser, P>();
        services.AddTransient(typeof(ModelSettings));

        // Register a _kernel instance with plugin injection
        services.AddTransient(typeof(VectorStoreTextSearch<TRecord>));
        services.AddTransient(typeof(IKernelPluginInjector), typeof(KernelPluginInjector<TRecord>));

        services.AddSingleton(LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        }));

        services.AddTransient<ModelSettings>(_ => settings[modelName]);

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
    /// Registers text embedding generation service based on the model name.
    /// </summary>
    public static IKernelBuilder AddTextEmbeddingGeneration<T, TRecord>(
        this IKernelBuilder builder,
        ModelSettings model)
        where T : class, IDocument, new()
        where TRecord : class
    {
        builder.Services.AddKeyedSingleton<ITextEmbeddingGenerationService>(model.EmbeddingModel, (sp, _) =>
            new EmbeddingService<T, TRecord>(
                sp.GetRequiredService<ILogger<EmbeddingService<T, TRecord>>>(),
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<IVectorStoreRecordCollection<string, TRecord>>(),
                sp.GetRequiredService<IFileParser>(),
                model
            ));

        builder.Services.AddTransient<ITextEmbeddingGenerationService>(sp =>
            new EmbeddingService<T, TRecord>(
                sp.GetRequiredService<ILogger<EmbeddingService<T, TRecord>>>(),
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<IVectorStoreRecordCollection<string, TRecord>>(),
                sp.GetRequiredService<IFileParser>(),
                model
            ));

        return builder;
    }

    /// <summary>
    /// Registers chat completion and text generation services based on the model name.
    /// </summary>
    public static IKernelBuilder AddChatCompletion(
        this IKernelBuilder builder,
        ModelSettings model)
    {
        Func<IServiceProvider, object, ChatCompletionService> factory = (sp, _) =>
            new ChatCompletionService(
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<ILogger<ChatCompletionService>>(), 
                model);

        builder.Services.AddKeyedSingleton<IChatCompletionService>(model.CompletionModel, factory!);
        builder.Services.AddKeyedSingleton<ITextGenerationService>(model.CompletionModel, factory!);

        builder.Services.AddSingleton<ITextGenerationService>(sp =>
            new ChatCompletionService(
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<ILogger<ChatCompletionService>>(),
                model));

        return builder;
    }
}
