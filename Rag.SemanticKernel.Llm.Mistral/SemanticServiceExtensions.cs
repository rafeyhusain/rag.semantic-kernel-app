using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;
using Rag.SemanticKernel.Abstractions.Parser;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Model.Vector;
using Rag.SemanticKernel.Parser.Markdown;

namespace Rag.SemanticKernel.Llm.Mistral;
public interface IKernelPluginInjector
{
    void InjectPlugins(Kernel kernel);
}
public class KernelPluginInjector<T> : IKernelPluginInjector
{
    private readonly VectorStoreTextSearch<T> _searchService;

    public KernelPluginInjector(VectorStoreTextSearch<T> searchService)
    {
        _searchService = searchService;
    }

    public void InjectPlugins(Kernel kernel)
    {
        var plugin = _searchService.CreateWithGetTextSearchResults("SearchPlugin");
        kernel.Plugins.Add(plugin);

        // Add more plugins if needed here
    }
}

/// <summary>
/// Extension methods for registering semantic services based on a configurable model and generic document type.
/// </summary>
public static class SemanticServiceExtensions
{
    /// <summary>
    /// Registers semantic services for the given model name and document type.
    /// </summary>
    public static void AddSemanticService<T>(this IServiceCollection services, Settings settings, string modelName) where T : class
    {
        var model = settings[modelName];

        var kernelBuilder = services.AddKernel();

        // Model-specific registration (can be extended with switch or reflection)
        kernelBuilder.AddTextEmbeddingGeneration(model);
        kernelBuilder.AddChatCompletion(model);

        // Register vector search for the specified document type
        kernelBuilder.AddVectorStoreTextSearch<T>();

        // Elasticsearch setup
        var elasticSettings = new ElasticsearchClientSettings(new Uri(settings.Elasticsearch.Url))
            .Authentication(new BasicAuthentication(settings.Elasticsearch.User, settings.Elasticsearch.Password));
        kernelBuilder.AddElasticsearchVectorStoreRecordCollection<string, T>(settings.Elasticsearch.Index, elasticSettings);

        // Register dependent services
        services.AddTransient<IFileParser, MarkdownFileParser>();
        services.AddTransient<QuestionService>();
        services.AddTransient<EmbeddingGeneratorService>();

        // Register a _kernel instance with plugin injection
        services.AddTransient(typeof(VectorStoreTextSearch<Markdown>));
        services.AddTransient(typeof(IKernelPluginInjector), typeof(KernelPluginInjector<Markdown>));

        services.AddSingleton<ILoggerFactory>(LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        }));

        // Register Kernel with plugin injection
        services.AddTransient<Kernel>(sp =>
        {
            var kernel = Kernel.CreateBuilder().Build();

            // Inject all available plugin injectors
            var injectors = sp.GetServices<IKernelPluginInjector>();
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
    public static IKernelBuilder AddTextEmbeddingGeneration(
        this IKernelBuilder builder,
        ModelSettings model)
    {
        builder.Services.AddKeyedSingleton<ITextEmbeddingGenerationService>(model.EmbeddingModel, (sp, _) =>
            new EmbeddingService(sp.GetRequiredService<ILogger<EmbeddingService>>(), model));

        builder.Services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
            new EmbeddingService(sp.GetRequiredService<ILogger<EmbeddingService>>(), model));

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
            new ChatCompletionService(sp.GetRequiredService<ILogger<ChatCompletionService>>(), model);

        builder.Services.AddKeyedSingleton<IChatCompletionService>(model.CompletionModel, factory!);
        builder.Services.AddKeyedSingleton<ITextGenerationService>(model.CompletionModel, factory!);

        builder.Services.AddSingleton<ITextGenerationService>(sp =>
            new ChatCompletionService(sp.GetRequiredService<ILogger<ChatCompletionService>>(), model));

        return builder;
    }
}
