using Rag.SemanticKernel.Template.Handlebar;

namespace Rag.SemanticKernel.Llm.Core.ChatCompletion;

public class ChatCompletionServiceOptions
{
    public TemplateFile Template { get; set; }

    public ChatCompletionServiceOptions()
    {
        Template ??= new TemplateFile();
        Template.Load();
    }
}
