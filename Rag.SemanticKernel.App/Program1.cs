//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Serilog;
//using Rag.SemanticKernel.Core.Sdk.Service;
//using Rag.SemanticKernel.Core.Sdk.Service.Mistral;

//namespace Rag.SemanticKernel.App;

//class Program
//{
//    static async Task Main(string[] args)
//    {
//        try
//        {
//            var builder = Host.CreateApplicationBuilder(args);

//            // Configure and set up logging
//            Log.Logger = new LoggerConfiguration()
//                .ReadFrom.Configuration(GetConfiguration())
//                .Enrich.FromLogContext()
//                .WriteTo.Console()
//                .WriteTo.File(GetConfiguration()["Log:FileName"] ?? "log.txt", rollingInterval: RollingInterval.Day)
//                .CreateLogger();

//            Log.Information("Starting application");

//            // Build host with DI
//            using var host = Host.CreateDefaultBuilder(args)
//                .ConfigureServices((context, services) =>
//                {
//                    // Register services
//                    services.AddSingleton<MistralEmbeddingService>();
//                    services.AddSingleton(Log.Logger);
//                })
//                .UseSerilog()
//                .Build();

//            // Get the service and initialize the index
//            var embeddingService = host.Services.GetRequiredService<EmbeddingService>();
//            await embeddingService.InitializeIndexAsync();

//            // Example usage
//            string sampleContent = "# Introduction to Vector Search\n\nVector search is a powerful technique for finding semantically similar documents based on their meaning rather than just keywords.";

//            // Generate embeddings and store document
//            string docId = await embeddingService.StoreMarkdownWithEmbeddingsAsync(
//                "getting-started.md",
//                "Introduction to Vector Search",
//                sampleContent);

//            Log.Information("Document stored with ID: {DocId}", docId);

//            // Search for similar documents
//            var results = await embeddingService.SearchSimilarDocumentsAsync("How does semantic search work?");

//            Log.Information("Found {Count} similar documents", results);
//            foreach (var doc in results)
//            {
//                Log.Information("Document: {Heading} - {ContentPreview}", doc.Heading,
//                    doc.Content.Length > 50 ? $"{doc.Content.Substring(0, 50)}..." : doc.Content);
//            }

//            Log.Information("Application completed successfully");
//        }
//        catch (Exception ex)
//        {
//            Log.Fatal(ex, "Application terminated unexpectedly");
//        }
//        finally
//        {
//            Log.CloseAndFlush();
//        }
//    }

//    private static IConfiguration GetConfiguration()
//    {
//        return new ConfigurationBuilder()
//            .SetBasePath(Directory.GetCurrentDirectory())
//            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//            .AddEnvironmentVariables()
//            .Build();
//    }
//}

//using System.Text.Json;
//using Elastic.Clients.Elasticsearch;
//using Elastic.SemanticKernel.Connectors.Elasticsearch;
//using Elastic.Transport;

//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.VectorData;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.Data;
//using Microsoft.SemanticKernel.Embeddings;
//using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

//namespace Elastic.SemanticKernel.Playground;

//#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
//#pragma warning disable SKEXP0010 // Some SK methods are still experimental
//#pragma warning disable SKEXP0020 // Some SK methods are still experimental
//#pragma warning disable SKEXP0001 // Some SK methods are still experimental
//#pragma warning disable CS7069 // Ignore Reference to type 'UpsertRecordOptions' claims it is defined in 'Microsoft.Extensions.VectorData.Abstractions', but it could not be found

//internal sealed class Program1
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
//        //var vectorStoreCollection = host.Services.GetService<IVectorStoreRecordCollection<string, Hotel>>()!;

//        /////////////



//        // Using Kernel Builder.
//        kernelBuilder.Services.AddSingleton<ElasticsearchClient>(sp =>
//            new ElasticsearchClient(new ElasticsearchClientSettings(new Uri("http://localhost:9200"))));
//        kernelBuilder.AddElasticsearchVectorStore();


//        var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"));
//        settings.DefaultFieldNameInferrer(name => JsonNamingPolicy.SnakeCaseUpper.ConvertName(name));
//        var client = new ElasticsearchClient(settings);

//        var vectorStoreCollection = new ElasticsearchVectorStoreRecordCollection<Hotel>(
//            client,
//            "skhotelsjson");

//        ////////////


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
//    [VectorStoreRecordData(IsFullTextIndexed = true)]
//    public required string HotelName { get; set; }

//    [TextSearchResultValue]
//    [VectorStoreRecordData(IsFullTextIndexed = true)]
//    public required string Description { get; set; }

//    [VectorStoreRecordVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
//    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }

//    [TextSearchResultLink]
//    [VectorStoreRecordData]
//    public string? ReferenceLink { get; set; }
//}