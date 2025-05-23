namespace Rag.SemanticKernel.Model.Api;

public class AskRequest
{
    public string Question { get; set; } = "";
    public string PairName { get; set; } = "mistral";
}
