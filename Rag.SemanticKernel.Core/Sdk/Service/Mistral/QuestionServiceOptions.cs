using Rag.SemanticKernel.Core.Sdk.Handlebar;
using System;
using System.IO;

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

public class QuestionServiceOptions
{
    public HandlebarTemplate HandlebarTemplate { get; set; }

    public QuestionServiceOptions()
    {
        HandlebarTemplate ??= new HandlebarTemplate();
        HandlebarTemplate.Load();
    }
}
