using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.TextGeneration;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Guards;
using Rag.SemanticKernel.Model.Llm.ChatCompletion;
using Rag.SemanticKernel.Rest;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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
