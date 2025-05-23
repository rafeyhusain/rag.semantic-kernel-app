namespace Rag.LlmRouter;

public class Router
{
    private Connector.Mistral.SemanticService _mistral;
    private Connector.OpenAi.SemanticService _openAi;
    private Connector.Berget.SemanticService _berget;
    private Connector.Scaleway.SemanticService _scaleway;

    public Connector.Mistral.SemanticService Mistral { get => _mistral; set => _mistral = value; }
    public Connector.OpenAi.SemanticService OpenAi { get => _openAi; set => _openAi = value; }
    public Connector.Berget.SemanticService Berget { get => _berget; set => _berget = value; }
    public Connector.Scaleway.SemanticService Scaleway { get => _scaleway; set => _scaleway = value; }

    public Router()
    {
    }

    public async Task GenerateEmbeddings(string pairName)
    {
        switch (pairName.ToLower())
        {
            case "mistral":
                await _mistral.GenerateEmbeddings();
                break;

            case "openai":
                await _openAi.GenerateEmbeddings();
                break;

            case "berget":
                await _berget.GenerateEmbeddings();
                break;

            case "scaleway":
                await _scaleway.GenerateEmbeddings();
                break;

            default:
                throw new ArgumentException($"Unknown pair name: {pairName}");
        }
    }

    public async Task<string> Ask(string pairName, string question)
    {
        var answer = string.Empty;

        switch (pairName.ToLower())
        {
            case "mistral":
                answer = await _mistral.Ask(question);
                break;

            case "openai":
                answer = await _openAi.Ask(question);
                break;

            case "berget":
                answer = await _berget.Ask(question);
                break;

            case "scaleway":
                answer = await _scaleway.Ask(question);
                break;

            default:
                throw new ArgumentException($"Unknown pair name: {pairName}");
        }

        return answer;
    }
}
