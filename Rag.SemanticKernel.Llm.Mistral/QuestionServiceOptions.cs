using Rag.SemanticKernel.Template.Handlebar;

namespace Rag.SemanticKernel.Llm.Mistral;

public class QuestionServiceOptions
{
    public TemplateFile Template { get; set; }

    public QuestionServiceOptions()
    {
        Template ??= new TemplateFile();
        Template.Load();
    }
}
