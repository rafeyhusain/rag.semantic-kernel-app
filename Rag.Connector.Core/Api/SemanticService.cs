using Microsoft.SemanticKernel;
using Rag.Guards;
using Rag.Connector.Core.ChatCompletion;
using Rag.Connector.Core.Embedding;
using Rag.Model.Api;
using System.Text.Json;

namespace Rag.Connector.Core.Api;

public class SemanticService<TRecord>
    where TRecord : class
{
    private readonly ChatCompletionService<TRecord> _chatCompletionService;
    private readonly EmbeddingService<TRecord> _embeddingService;
    private readonly Kernel _kernel;

    public SemanticService(
        Kernel kernel,
        ChatCompletionService<TRecord> chatCompletionService,
        EmbeddingService<TRecord> embeddingService)
    {
        _kernel = Guard.ThrowIfNull(kernel);
        _chatCompletionService = Guard.ThrowIfNull(chatCompletionService);
        _embeddingService = Guard.ThrowIfNull(embeddingService);
    }

    public virtual string PairName => "";

    public async Task<AskResponse> AskModel(string question)
    {
        var answerText = await Ask(question);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        AskResponse answer = JsonSerializer.Deserialize<AskResponse>(answerText)!;

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
