using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable SKEXP0010 // Some SK methods are still experimental

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

public class SemanticService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SemanticService> _logger;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IVectorStoreRecordCollection<string, Hotel> _vectorStoreCollection;
    private readonly Kernel _kernel;

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _embeddingModel;
    private readonly string _elasticsearchUrl;
    private readonly string _elasticsearchIndexName;

    public SemanticService(IConfiguration configuration, ILogger<SemanticService> logger, ITextEmbeddingGenerationService embeddingService, IVectorStoreRecordCollection<string, Hotel> vectorStoreCollection, Kernel kernel)
    {
        try
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _vectorStoreCollection = vectorStoreCollection ?? throw new ArgumentNullException(nameof(vectorStoreCollection));
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

            //RegisterSearchPlugin();

            _logger.LogInformation("SemanticService initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing SemanticService");
            throw;
        }
    }

    private void RegisterSearchPlugin()
    {
        // Create a TextSearchPlugin that can search the vector collection
        var searchPlugin = new VectorStoreTextSearch<Hotel>(_vectorStoreCollection, _embeddingService);

        // Register the plugin with the kernel
        _kernel.Plugins.Add(searchPlugin.CreateWithGetTextSearchResults("SearchPlugin"));

        _logger.LogInformation("SearchPlugin registered successfully");
    }

    public async Task Ask(string[] args)
    {
        //await CreateEmbeddings("hotels.csv");
        await GetAnswer();
    }

    private async Task CreateEmbeddings(string filePath)
    {
        // Crate collection and ingest a few demo records.
        await _vectorStoreCollection.CreateCollectionIfNotExistsAsync();

        var hotelsRaw = await File.ReadAllLinesAsync(filePath);

        var hotels = hotelsRaw
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split(';'))
            .Where(parts => parts.Length >= 4) // ID;HotelName;Description;Link
            .ToList();

        foreach (var chunk in hotels.Chunk(25))
        {
            var descriptions = chunk.Select(x => x[2]).ToArray();

            var descriptionEmbeddings = await GenerateWithRetry(descriptions);

            for (var i = 0; i < chunk.Length && i < descriptionEmbeddings.Count; ++i)
            {
                var hotel = chunk[i];
                await _vectorStoreCollection.UpsertAsync(new Hotel
                {
                    HotelId = hotel[0],
                    HotelName = hotel[1],
                    Description = hotel[2],
                    DescriptionEmbedding = descriptionEmbeddings[i],
                    Link = hotel[3]
                });
            }
        }
    }

    private async Task GetAnswer2()
    {
        // First, execute the search function directly
        var searchResults = await _kernel.InvokeAsync("SearchPlugin", "GetTextSearchResults",
            new KernelArguments { { "input", "Please show me all hotels that have a rooftop bar." } });

        // Create a prompt with the search results
        var searchResultsStr = searchResults.GetValue<string>();

        // Now pass the search results to a simpler prompt
        var promptTemplate = """
            Please use this information to answer the question:
            {{searchResults}}

            Include the source of relevant information in the response.

            Question: {{question}}
            """;

        var response = await _kernel.InvokePromptAsync(
            promptTemplate: promptTemplate,
            arguments: new KernelArguments
            {
                { "question", "Please show me all hotels that have a rooftop bar." },
                { "searchResults", searchResultsStr }
            },
            templateFormat: "handlebars",
            promptTemplateFactory: new HandlebarsPromptTemplateFactory());

        Console.WriteLine(response.ToString());

        // > Urban Chic Hotel has a rooftop bar with stunning views (Source: https://example.com/stu654).
    }

    private async Task GetAnswer()
    {
        var question = "rooftop";
        //var question = "Please show me all hotels that have a rooftop bar.";

        // Invoke the search function
        var searchResults = await _kernel.InvokeAsync("SearchPlugin", "GetTextSearchResults",
            new KernelArguments { { "input", question } });

        // Extract the array of results
        var resultsArray = searchResults.GetValue<Microsoft.SemanticKernel.Data.TextSearchResult[]>();

        if (resultsArray == null || resultsArray.Length == 0)
        {
            Console.WriteLine("No results found.");
            return;
        }

        if (resultsArray == null || resultsArray.Length == 0)
        {
            Console.WriteLine($"Sorry, I couldn't find any answer for $`{question}`");
            return;
        }

        // Convert the array to a string for the prompt
        var searchResultsStr = string.Join("\n\n", resultsArray.Select(result =>
            $"Title: {result.Name}\n" +
            $"Excerpt: {result.Value}\n" +
            $"Source: {result.Link}"));

        // Optional: log the formatted results
        Console.WriteLine("--- Search Results for Prompt ---");
        Console.WriteLine(searchResultsStr);

        // Prompt template
        var promptTemplate = """
        Please use this information to answer the question:
        {{searchResults}}

        Include the source of relevant information in the response.

        Question: {{question}}
        """;

        // Use the formatted string as input
        var response = await _kernel.InvokePromptAsync(
            promptTemplate: promptTemplate,
            arguments: new KernelArguments
            {
            { "question", question},
            { "searchResults", searchResultsStr }
            },
            templateFormat: "handlebars",
            promptTemplateFactory: new HandlebarsPromptTemplateFactory());

        Console.WriteLine("--- Final Answer ---");
        Console.WriteLine(response.ToString());
    }

    private async Task<IList<ReadOnlyMemory<float>>> GenerateWithRetry(string[] texts, int maxRetries = 5)
    {
        int delay = 1000; // Start with 1 second
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return await _embeddingService.GenerateEmbeddingsAsync(texts);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("TooManyRequests"))
            {
                Console.WriteLine($"Rate limit hit. Retrying in {delay}ms... (attempt {attempt + 1}/{maxRetries})");
                await Task.Delay(delay);
                delay *= 2; // Exponential backoff
            }
        }

        throw new Exception("Failed to generate embeddings after several retries.");
    }
}

