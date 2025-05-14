using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable SKEXP0010 // Some SK methods are still experimental

namespace Rag.SemanticKernel.Core.Sdk.Service.Azure;

public class EmbeddingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmbeddingService> _logger;

    private readonly string _apiKey;
    private readonly string _embeddingModel;
    private readonly string _completionModel;
    private readonly string _elasticsearchUrl;
    private readonly string _elasticsearchIndexName;
    private readonly ElasticsearchClient _elasticsearchClient;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IVectorStoreRecordCollection<string, Hotel> _vectorStoreCollection;
    private readonly Kernel _kernel;

    public EmbeddingService(IConfiguration configuration, ILogger<EmbeddingService> logger, ITextEmbeddingGenerationService embeddingService, IVectorStoreRecordCollection<string, Hotel> vectorStoreCollection, Kernel kernel)
    {
        try
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _vectorStoreCollection = vectorStoreCollection ?? throw new ArgumentNullException(nameof(vectorStoreCollection));
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

            // Load configuration values
            _apiKey = _configuration["Mistral:ApiKey"] ?? throw new InvalidOperationException("Missing Mistral:ApiKey configuration");
            _embeddingModel = _configuration["Mistral:EmbeddingModel"] ?? "mistral-embed";
            _completionModel = _configuration["Mistral:CompletionModel"] ?? "mistral-large-latest";
            _elasticsearchUrl = _configuration["Elasticsearch:Url"] ?? throw new InvalidOperationException("Missing Elasticsearch:Url configuration");
            _elasticsearchIndexName = _configuration["Elasticsearch:IndexName"] ?? "markdown";

            _logger.LogInformation("EmbeddingService initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing EmbeddingService");
            throw;
        }
    }

    public async Task Ask(string[] args)
    {
        // Crate collection and ingest a few demo records.
        await _vectorStoreCollection.CreateCollectionIfNotExistsAsync();

        // CSV format: ID;Hotel Name;Description;Reference Link
        var hotels = (await File.ReadAllLinesAsync("hotels.csv"))
            .Select(x => x.Split(';'));

        foreach (var chunk in hotels.Chunk(25))
        {
            var descriptionEmbeddings = await _embeddingService.GenerateEmbeddingsAsync(chunk.Select(x => x[2]).ToArray());

            for (var i = 0; i < chunk.Length; ++i)
            {
                var hotel = chunk[i];
                await _vectorStoreCollection.UpsertAsync(new Hotel
                {
                    HotelId = hotel[0],
                    HotelName = hotel[1],
                    Description = hotel[2],
                    DescriptionEmbedding = descriptionEmbeddings[i],
                    ReferenceLink = hotel[3]
                });
            }
        }

        // Invoke the LLM with a template that uses the search plugin to
        // 1. get related information to the user query from the vector store
        // 2. add the information to the LLM prompt.
        var response = await _kernel.InvokePromptAsync(
            promptTemplate: """
                            Please use this information to answer the question:
                            {{#with (SearchPlugin-GetTextSearchResults question)}}
                              {{#each this}}
                                Name: {{Name}}
                                Value: {{Value}}
                                Source: {{Link}}
                                -----------------
                              {{/each}}
                            {{/with}}

                            Include the source of relevant information in the response.

                            Question: {{question}}
                            """,
            arguments: new KernelArguments
            {
                { "question", "Please show me all hotels that have a rooftop bar." },
            },
            templateFormat: "handlebars",
            promptTemplateFactory: new HandlebarsPromptTemplateFactory());

        Console.WriteLine(response.ToString());

        // > Urban Chic Hotel has a rooftop bar with stunning views (Source: https://example.com/stu654).
    }
}
