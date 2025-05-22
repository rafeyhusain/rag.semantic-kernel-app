using Microsoft.SemanticKernel;
using Rag.SemanticKernel.Guards;
using Rag.SemanticKernel.Llm.Core.ChatCompletion;
using Rag.SemanticKernel.Llm.Core.Embedding;
using Rag.SemanticKernel.Model.Api;
using Rag.SemanticKernel.Model.Vector;
using System.Text.Json;

namespace Rag.SemanticKernel.Llm.Core.Api;

public class SemanticService<T, TRecord>
    where T : class, IDocument, new()
    where TRecord : class
{
    private readonly ChatCompletionService<TRecord> _chatCompletionService;
    private readonly EmbeddingService<T, TRecord> _embeddingService;
    private readonly Kernel _kernel;

    public SemanticService(
        Kernel kernel,
        ChatCompletionService<TRecord> chatCompletionService,
        EmbeddingService<T, TRecord> embeddingService)
    {
        _kernel = Guard.ThrowIfNull(kernel);
        _chatCompletionService = Guard.ThrowIfNull(chatCompletionService);
        _embeddingService = Guard.ThrowIfNull(embeddingService);
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
        => Ask(question, new ChatCompletionServiceOptions());

    public Task<string> Ask(string question, ChatCompletionServiceOptions options)
    => _chatCompletionService.Ask(question, options);

    public Task GenerateEmbeddings()
        => GenerateEmbeddings(new EmbeddingServiceOptions());

    public Task GenerateEmbeddings(EmbeddingServiceOptions options)
        => _embeddingService.Generate(options);
}
