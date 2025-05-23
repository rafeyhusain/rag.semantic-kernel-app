using Microsoft.Extensions.DependencyInjection;
using Rag.LlmRouter;
using Rag.LlmRouter.Extensions;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Parser.Markdown;
using Rag.SemanticKernel.Startup.ConsoleApp.Events;
using System.Threading.Tasks;

namespace Rag.SemanticKernel.App.Startup;

public class Application : SemanticKernel.Startup.ConsoleApp.Startup.Application
{
    private readonly Router _router;
    private Settings _settings;
    
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
        e.Builder.Services.AddLlms(e.Settings);
    }

    private void Application_AfterServiceContainerCreated(object sender, AfterServiceContainerCreatedEventArgs e)
    {
        _settings = e.Host.Services.GetRequiredService<AppSettings.Settings>();
        _router.Mistral = e.Host.Services.GetRequiredService<Connector.Mistral.SemanticService>();
        _router.OpenAi = e.Host.Services.GetRequiredService<Connector.OpenAi.SemanticService>();
        _router.Berget = e.Host.Services.GetRequiredService<Connector.Berget.SemanticService>();
        _router.Scaleway = e.Host.Services.GetRequiredService<Connector.Scaleway.SemanticService>();
    }

    public async Task GenerateEmbeddings()
    {
        _settings.CurrentPairName = Options.PairName;
        await _router.GenerateEmbeddings(Options.PairName);
    }

    public async Task<string> Ask(string question)
    {
        _settings.CurrentPairName = Options.PairName;
        var answer = await _router.Ask(Options.PairName, question);

        return answer;
    }
}
