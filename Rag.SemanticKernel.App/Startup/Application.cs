using Microsoft.Extensions.DependencyInjection;
using Rag.SemanticKernel.Core.Sdk.Service.Mistral;
using Rag.SemanticKernel.Startup.ConsoleApp.Events;
using System.Threading.Tasks;

namespace Rag.SemanticKernel.App.Startup;

public class Application : Rag.SemanticKernel.Startup.ConsoleApp.Application
{
    private SemanticService _semanticService;

    public Application()
    {
        this.BeforeServiceContainerCreated += Application_BeforeServiceContainerCreated;
        this.AfterServiceContainerCreated += Application_AfterServiceContainerCreated;
    }

    public override async Task Init(string[] args)
    {
        await base.Init(args);
    }

    private void Application_BeforeServiceContainerCreated(object sender, BeforeServiceContainerCreatedEventArgs e)
    {
        e.Builder.Services.AddSemanticService(e.Settings);

        e.Builder.Services.AddSingleton<SemanticService>();
    }

    private void Application_AfterServiceContainerCreated(object sender, AfterServiceContainerCreatedEventArgs e)
    {
        _semanticService = e.Host.Services.GetService<SemanticService>();
    }

    public async Task GenerateEmbeddings()
    {
        await _semanticService.GenerateEmbeddings();
    }

    public async Task<string> Ask(string question)
    {
        return await _semanticService.Ask(question);
    }
}
