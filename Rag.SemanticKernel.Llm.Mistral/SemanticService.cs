using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Rag.SemanticKernel.Guards;
using Rag.SemanticKernel.Model.Api;
using Rag.SemanticKernel.Model.Vector;
using System.Text.Json;


namespace Rag.SemanticKernel.Llm.Mistral;

public class SemanticService
{
    private readonly QuestionService _questionService;
    private readonly EmbeddingGeneratorService _embeddingGenerator;
    private readonly Kernel _kernel;

    public SemanticService(Kernel kernel, QuestionService questionService, EmbeddingGeneratorService embeddingGenerator)
    {
        _kernel = Guard.ThrowIfNull(kernel);
        _questionService = Guard.ThrowIfNull(questionService);
        _embeddingGenerator = Guard.ThrowIfNull(embeddingGenerator);
    }

    public async Task<AskResponse> AskModel(string question)
    {
        var answerText = await Ask(question);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        AskResponse answer = JsonSerializer.Deserialize<AskResponse>(answerText, options)!;

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
