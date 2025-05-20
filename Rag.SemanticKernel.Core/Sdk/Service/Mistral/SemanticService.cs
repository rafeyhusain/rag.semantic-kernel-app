using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Rag.SemanticKernel.Core.Sdk.Model;
using Rag.SemanticKernel.Core.Sdk.Service.Azure;
using System.Text.Json;
using System.Threading.Tasks;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable SKEXP0010 // Some SK methods are still experimental

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

public class SemanticService
{
    private readonly QuestionService _questionService;
    private readonly EmbeddingGeneratorService _embeddingGenerator;
    public Kernel Kernel { get; internal set; }

    public SemanticService(IHost host, QuestionService questionService, EmbeddingGeneratorService embeddingGenerator)
    {
        Kernel = host.Services.GetService<Kernel>()!;

        var textSearch = host.Services.GetService<VectorStoreTextSearch<Markdown>>()!;
        Kernel.Plugins.Add(textSearch.CreateWithGetTextSearchResults("SearchPlugin"));

        _questionService = questionService;
        _questionService.Kernel = Kernel;

        _embeddingGenerator = embeddingGenerator;
        _embeddingGenerator.Kernel = Kernel;
    }

    public async Task<AnswerModel> AskModel(string question)
    {
        var answerText = await Ask(question);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        AnswerModel answer = JsonSerializer.Deserialize<AnswerModel>(answerText, options);

        return answer;
    }

    public Task<string> Ask(string question)
        => Ask(question, new QuestionServiceOptions());

    public Task<string> Ask(string question, QuestionServiceOptions options)
    => _questionService.Ask(question, options);

    public Task GenerateEmbeddings()
        => GenerateEmbeddings(new EmbeddingGeneratorServiceOptions());

    public Task GenerateEmbeddings(EmbeddingGeneratorServiceOptions options)
        => _embeddingGenerator.Generate(options);
}
