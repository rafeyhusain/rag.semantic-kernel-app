//using System.Net.Http.Headers;
//using System.Text.Json;
//using Elastic.Clients.Elasticsearch;
//using Elastic.SemanticKernel.Connectors.Elasticsearch;
//using Elastic.Transport;
//using Microsoft.Extensions.AI;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.VectorData;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.Data;
//using Microsoft.SemanticKernel.Embeddings;
//using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
//using Microsoft.SemanticKernel.TextGeneration;
//using Rag.SemanticKernel.Core.Sdk.Service;

//namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

//#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
//#pragma warning disable SKEXP0010 // Some SK methods are still experimental
//#pragma warning disable SKEXP0001 // Some SK methods are still experimental


//public class EmbeddingService
//{
//    private readonly IConfiguration _configuration;
//    private readonly ILogger<EmbeddingService> _logger;
//    private readonly HttpClient _httpClient;
//    private readonly string _apiKey;
//    private readonly string _embeddingModel;
//    private readonly string _completionModel;
//    private readonly string _elasticsearchUrl;
//    private readonly string _elasticsearchIndexName;
//    private readonly ElasticsearchClient _elasticsearchClient;
//    private readonly ElasticsearchVectorStoreRecordCollection<Markdown> _markdownCollection;
//    private readonly Kernel _kernel;

//    public EmbeddingService(IConfiguration configuration, ILogger<EmbeddingService> logger)
//    {
//        try
//        {
//            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

//            // Load configuration values
//            _apiKey = _configuration["Mistral:ApiKey"] ?? throw new InvalidOperationException("Missing Mistral:ApiKey configuration");
//            _embeddingModel = _configuration["Mistral:EmbeddingModel"] ?? "mistral-embed";
//            _completionModel = _configuration["Mistral:CompletionModel"] ?? "mistral-large-latest";
//            _elasticsearchUrl = _configuration["Elasticsearch:Url"] ?? throw new InvalidOperationException("Missing Elasticsearch:Url configuration");
//            _elasticsearchIndexName = _configuration["Elasticsearch:IndexName"] ?? "markdown";

//            // Initialize HTTP client for Mistral API
//            _httpClient = new HttpClient();
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
//            _httpClient.BaseAddress = new Uri("https://api.mistral.ai/v1/");

//            // Initialize Elasticsearch client
//            var elasticsearchClientSettings = new ElasticsearchClientSettings(new Uri(_elasticsearchUrl));
//            elasticsearchClientSettings.DefaultFieldNameInferrer(name => JsonNamingPolicy.SnakeCaseUpper.ConvertName(name));
//            _elasticsearchClient = new ElasticsearchClient(elasticsearchClientSettings);

//            // Initialize Semantic Kernel with custom Mistral text embeddings
//            var kernelBuilder = Kernel.CreateBuilder()
//                .AddElasticsearchVectorStoreRecordCollection<string, Hotel>("skhotels", elasticsearchClientSettings);

//            // Add custom Mistral text embedding generation
//            kernelBuilder.Services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
//                new CustomMistralEmbeddingService(_httpClient, _embeddingModel, _logger));

//            // Add custom Mistral text completion service
//            kernelBuilder.Services.AddSingleton<ITextGenerationService>(sp =>
//                new CustomMistralCompletionService(_httpClient, _completionModel, _logger));

//            // Register text search service.
//            kernelBuilder.AddVectorStoreTextSearch<Hotel>();

//            _kernel = kernelBuilder.Build();

//            // Initialize vector store collection
//            _markdownCollection = new ElasticsearchVectorStoreRecordCollection<Markdown>(
//                _elasticsearchClient,
//                _elasticsearchIndexName);

//            // For demo purposes, we access the services directly without using a DI context.
//            var embeddings = _kernel.GetRequiredService<ITextEmbeddingGenerationService>()!;
//            var vectorStoreCollection = _kernel.GetRequiredService<IVectorStoreRecordCollection<string, Hotel>>()!;

//            // Register search plugin.
//            var textSearch = _kernel.GetRequiredService<VectorStoreTextSearch<Hotel>>()!;
//            _kernel.Plugins.Add(textSearch.CreateWithGetTextSearchResults("SearchPlugin"));

//            _logger.LogInformation("EmbeddingService initialized successfully");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error initializing EmbeddingService");
//            throw;
//        }
//    }


//    /// <summary>
//    /// Initializes the Elasticsearch index with the proper mapping for vector search
//    /// </summary>
//    public async Task InitializeIndexAsync()
//    {
//        try
//        {
//            _logger.LogInformation("Initializing Elasticsearch index: {IndexName}", _elasticsearchIndexName);

//            // Create collection and ingest a few demo records.
//            await vectorStoreCollection.CreateCollectionIfNotExistsAsync();

//            // CSV format: ID;Hotel Name;Description;Reference Link
//            var hotels = (await File.ReadAllLinesAsync("hotels.csv"))
//                .Select(x => x.Split(';'));

//            foreach (var chunk in hotels.Chunk(25))
//            {
//                var descriptionEmbeddings = await embeddings.GenerateEmbeddingsAsync(chunk.Select(x => x[2]).ToArray());

//                for (var i = 0; i < chunk.Length; ++i)
//                {
//                    var hotel = chunk[i];
//                    await vectorStoreCollection.UpsertAsync(new Hotel
//                    {
//                        HotelId = hotel[0],
//                        HotelName = hotel[1],
//                        Description = hotel[2],
//                        DescriptionEmbedding = descriptionEmbeddings[i],
//                        ReferenceLink = hotel[3]
//                    });
//                }
//            }

//            // Invoke the LLM with a template that uses the search plugin to
//            // 1. get related information to the user query from the vector store
//            // 2. add the information to the LLM prompt.
//            var response = await _kernel.InvokePromptAsync(
//                promptTemplate: """
//                            Please use this information to answer the question:
//                            {{#with (SearchPlugin-GetTextSearchResults question)}}
//                              {{#each this}}
//                                Name: {{Name}}
//                                Value: {{Value}}
//                                Source: {{Link}}
//                                -----------------
//                              {{/each}}
//                            {{/with}}

//                            Include the source of relevant information in the response.

//                            Question: {{question}}
//                            """,
//                arguments: new KernelArguments
//                {
//                { "question", "Please show me all hotels that have a rooftop bar." },
//                },
//                templateFormat: "handlebars",
//                promptTemplateFactory: new HandlebarsPromptTemplateFactory());

//            Console.WriteLine(response.ToString());

//            // > Urban Chic Hotel has a rooftop bar with stunning views (Source: https://example.com/stu654).

//            //// Check if index exists
//            //var indexExists = await _elasticsearchClient.Indices.ExistsAsync(_elasticsearchIndexName);

//            //if (!indexExists.Exists)
//            //{
//            //    _logger.LogInformation("Creating new index: {IndexName}", _elasticsearchIndexName);

//            //    // Get the vector store service
//            //    var vectorStoreService = _kernel.GetRequiredService<IElasticsearchVectorStoreService>();

//            //    // Create the index with appropriate settings for vector search
//            //    await vectorStoreService.CreateIndexAsync<Markdown>(_elasticsearchIndexName);

//            _logger.LogInformation("Index successfully created");
//            //}
//            //else
//            //{
//            //    _logger.LogInformation("Index already exists, skipping creation");
//            //}
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error initializing Elasticsearch index");
//            throw;
//        }
//    }

//}

//internal sealed class Program
//{
//    public static async Task Main(string[] args)
//    {

//        var builder = Host.CreateApplicationBuilder(args);

//        // Register AI services.
//        var kernelBuilder = builder.Services.AddKernel();
//        kernelBuilder.AddAzureOpenAIChatCompletion("gpt-4o", "https://my-service.openai.azure.com", "my_token");
//        kernelBuilder.AddAzureOpenAITextEmbeddingGeneration("ada-002", "https://my-service.openai.azure.com", "my_token");

//        // Register text search service.
//        kernelBuilder.AddVectorStoreTextSearch<Hotel>();

//        // Register Elasticsearch vector store.
//        var elasticsearchClientSettings = new ElasticsearchClientSettings(new Uri("https://my-elasticsearch-instance.cloud"))
//            .Authentication(new BasicAuthentication("elastic", "my_password"));
//        kernelBuilder.AddElasticsearchVectorStoreRecordCollection<string, Hotel>("skhotels", elasticsearchClientSettings);

//        // Build the host.
//        using var host = builder.Build();

//        // For demo purposes, we access the services directly without using a DI context.

//        var kernel = host.Services.GetService<Kernel>()!;
//        var embeddings = host.Services.GetService<ITextEmbeddingGenerationService>()!;
//        var vectorStoreCollection = host.Services.GetService<IVectorStoreRecordCollection<string, Hotel>>()!;

//        // Register search plugin.
//        var textSearch = host.Services.GetService<VectorStoreTextSearch<Hotel>>()!;
//        kernel.Plugins.Add(textSearch.CreateWithGetTextSearchResults("SearchPlugin"));

//        // Crate collection and ingest a few demo records.
//        await vectorStoreCollection.CreateCollectionIfNotExistsAsync();

//        // CSV format: ID;Hotel Name;Description;Reference Link
//        var hotels = (await File.ReadAllLinesAsync("hotels.csv"))
//            .Select(x => x.Split(';'));

//        foreach (var chunk in hotels.Chunk(25))
//        {
//            var descriptionEmbeddings = await embeddings.GenerateEmbeddingsAsync(chunk.Select(x => x[2]).ToArray());

//            for (var i = 0; i < chunk.Length; ++i)
//            {
//                var hotel = chunk[i];
//                await vectorStoreCollection.UpsertAsync(new Hotel
//                {
//                    HotelId = hotel[0],
//                    HotelName = hotel[1],
//                    Description = hotel[2],
//                    DescriptionEmbedding = descriptionEmbeddings[i],
//                    ReferenceLink = hotel[3]
//                });
//            }
//        }

//        // Invoke the LLM with a template that uses the search plugin to
//        // 1. get related information to the user query from the vector store
//        // 2. add the information to the LLM prompt.
//        var response = await kernel.InvokePromptAsync(
//            promptTemplate: """
//                            Please use this information to answer the question:
//                            {{#with (SearchPlugin-GetTextSearchResults question)}}
//                              {{#each this}}
//                                Name: {{Name}}
//                                Value: {{Value}}
//                                Source: {{Link}}
//                                -----------------
//                              {{/each}}
//                            {{/with}}

//                            Include the source of relevant information in the response.

//                            Question: {{question}}
//                            """,
//            arguments: new KernelArguments
//            {
//                { "question", "Please show me all hotels that have a rooftop bar." },
//            },
//            templateFormat: "handlebars",
//            promptTemplateFactory: new HandlebarsPromptTemplateFactory());

//        Console.WriteLine(response.ToString());

//        // > Urban Chic Hotel has a rooftop bar with stunning views (Source: https://example.com/stu654).
//    }
//}

//public sealed record Hotel
//{
//    [VectorStoreRecordKey]
//    public required string HotelId { get; set; }

//    [TextSearchResultName]
//    [VectorStoreRecordData(IsIndexed = true)]
//    public required string HotelName { get; set; }

//    [TextSearchResultValue]
//    [VectorStoreRecordData(IsIndexed = true)]
//    public required string Description { get; set; }

//    [VectorStoreRecordVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
//    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }

//    [TextSearchResultLink]
//    [VectorStoreRecordData]
//    public string? ReferenceLink { get; set; }
//}