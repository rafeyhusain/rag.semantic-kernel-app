using System.Collections.Generic;

namespace Rag.SemanticKernel.Model.Api;

public class AskResponse
{
    public string Answer { get; set; } = "";
    public List<Reference> Refs { get; set; } = new();
}

public class Reference
{
    public string FileName { get; set; } = "";
    public string MdHeader { get; set; } = "";
}
