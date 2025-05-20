using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable SKEXP0010 // Some SK methods are still experimental

namespace Rag.SemanticKernel.Core.Sdk.Service.Azure;

public class SemanticService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SemanticService> _logger;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IVectorStoreRecordCollection<string, Markdown> _vectorStoreCollection;
    private readonly Kernel _kernel;

    public SemanticService(IConfiguration configuration, ILogger<SemanticService> logger, ITextEmbeddingGenerationService embeddingService, IVectorStoreRecordCollection<string, Markdown> vectorStoreCollection, Kernel kernel)
    {
        try
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _vectorStoreCollection = vectorStoreCollection ?? throw new ArgumentNullException(nameof(vectorStoreCollection));
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

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

        // CSV format: ID;Markdown Name;Description;Reference Link
        var Markdowns = (await File.ReadAllLinesAsync("Markdowns.csv"))
            .Select(x => x.Split(';'));

        foreach (var chunk in Markdowns.Chunk(25))
        {
            var descriptionEmbeddings = await _embeddingService.GenerateEmbeddingsAsync(chunk.Select(x => x[2]).ToArray());

            for (var i = 0; i < chunk.Length; ++i)
            {
                var Markdown = chunk[i];
                await _vectorStoreCollection.UpsertAsync(new Markdown
                {
                    MarkdownId = Markdown[0],
                    MarkdownName = Markdown[1],
                    Description = Markdown[2],
                    DescriptionEmbedding = descriptionEmbeddings[i],
                    ReferenceLink = Markdown[3]
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
                { "question", "Please show me all Markdowns that have a rooftop bar." },
            },
            templateFormat: "handlebars",
            promptTemplateFactory: new HandlebarsPromptTemplateFactory());

        Console.WriteLine(response.ToString());

        // > Urban Chic Markdown has a rooftop bar with stunning views (Source: https://example.com/stu654).
    }
}
