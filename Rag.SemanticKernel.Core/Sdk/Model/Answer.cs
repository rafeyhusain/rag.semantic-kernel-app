using System.Collections.Generic;

namespace Rag.SemanticKernel.Core.Sdk.Model;

public class QuestionRequest
{
    public string Question { get; set; } = "";
}

public class Reference
{
    public string FileName { get; set; } = "";
    public string MdHeader { get; set; } = "";
}

public class AnswerModel
{
    public string Answer { get; set; } = "";
    public List<Reference> Refs { get; set; } = new();
}