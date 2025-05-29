using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;

namespace Rag.Connector.Core.ChatCompletion
{
    public class TestService : ITextGenerationService
    {
        public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

        public IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<TextContent>> GetTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
