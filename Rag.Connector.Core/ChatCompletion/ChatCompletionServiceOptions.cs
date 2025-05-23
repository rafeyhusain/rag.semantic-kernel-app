using Rag.Template.Handlebar;

namespace Rag.Connector.Core.ChatCompletion;

public class ChatCompletionServiceOptions
{
    public TemplateFile Template { get; set; }

    public ChatCompletionServiceOptions()
    {
        Template ??= new TemplateFile();
        Template.Load();
    }
}
