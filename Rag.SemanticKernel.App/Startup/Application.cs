using Microsoft.Extensions.DependencyInjection;
using Rag.Connector.Core.Extensions;
using Rag.LlmRouter;
using Rag.SemanticKernel.Parser.Markdown;
using Rag.SemanticKernel.Startup.ConsoleApp.Events;
using System.Threading.Tasks;

namespace Rag.SemanticKernel.App.Startup;

public class Application : SemanticKernel.Startup.ConsoleApp.Startup.Application
{
    private readonly Router _router;

    public Application()
    {
        this.BeforeServiceContainerCreated += Application_BeforeServiceContainerCreated;
        this.AfterServiceContainerCreated += Application_AfterServiceContainerCreated;
        this._router = new Router();
    }

    public override async Task Init(string[] args)
    {
        await base.Init(args);
    }

    private void Application_BeforeServiceContainerCreated(object sender, BeforeServiceContainerCreatedEventArgs e)
    {
        e.Builder.Services.AddTransient<Connector.Mistral.EmbeddingService>();
        e.Builder.Services.AddTransient<Connector.Mistral.ChatCompletionService>();
        e.Builder.Services.AddTransient<Connector.Mistral.SemanticService>();
        e.Builder.Services.AddSemanticService<Connector.Mistral.Markdown, MarkdownFileParser>(e.Settings, Options.PairName);

        e.Builder.Services.AddTransient<Connector.OpenAi.EmbeddingService>();
        e.Builder.Services.AddTransient<Connector.OpenAi.ChatCompletionService>();
        e.Builder.Services.AddTransient<Connector.OpenAi.SemanticService>();
        e.Builder.Services.AddSemanticService<Connector.OpenAi.Markdown, MarkdownFileParser>(e.Settings, Options.PairName);

        e.Builder.Services.AddTransient<Connector.Berget.EmbeddingService>();
        e.Builder.Services.AddTransient<Connector.Berget.ChatCompletionService>();
        e.Builder.Services.AddTransient<Connector.Berget.SemanticService>();
        e.Builder.Services.AddSemanticService<Connector.Berget.Markdown, MarkdownFileParser>(e.Settings, Options.PairName);

        e.Builder.Services.AddTransient<Connector.Scaleway.EmbeddingService>();
        e.Builder.Services.AddTransient<Connector.Scaleway.ChatCompletionService>();
        e.Builder.Services.AddTransient<Connector.Scaleway.SemanticService>();
        e.Builder.Services.AddSemanticService<Connector.Scaleway.Markdown, MarkdownFileParser>(e.Settings, Options.PairName);
    }

    private void Application_AfterServiceContainerCreated(object sender, AfterServiceContainerCreatedEventArgs e)
    {
        _router.Mistral = e.Host.Services.GetRequiredService<Connector.Mistral.SemanticService>();
        _router.OpenAi = e.Host.Services.GetRequiredService<Connector.OpenAi.SemanticService>();
        _router.Berget = e.Host.Services.GetRequiredService<Connector.Berget.SemanticService>();
        _router.Scaleway = e.Host.Services.GetRequiredService<Connector.Scaleway.SemanticService>();
    }

    public async Task GenerateEmbeddings()
    {
        await _router.GenerateEmbeddings(Options.PairName);
    }

    public async Task<string> Ask(string question)
    {
        var answer = await _router.Ask(Options.PairName, question);

        return answer;
    }
}
