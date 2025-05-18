using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Rag.SemanticKernel.Core.Sdk.App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable SKEXP0010 // Some SK methods are still experimental

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

public class SemanticService
{
    private readonly QuestionService _questionService;
    private readonly EmbeddingGeneratorService _embeddingGenerator;

    public SemanticService(QuestionService questionService, EmbeddingGeneratorService embeddingGenerator)
    {
        _questionService = questionService;
        _embeddingGenerator = embeddingGenerator;
    }

    public Task<string> Ask(Kernel kernel, string question) 
        => Ask(kernel, question, new QuestionServiceOptions());

    public Task<string> Ask(Kernel kernel, string question, QuestionServiceOptions options)
    => _questionService.Ask(kernel, question, options);

    public Task GenerateEmbeddings(Kernel kernel)
        => GenerateEmbeddings(kernel, new EmbeddingGeneratorServiceOptions());

    public Task GenerateEmbeddings(Kernel kernel, EmbeddingGeneratorServiceOptions options)
        => _embeddingGenerator.Generate(kernel, options);
}
