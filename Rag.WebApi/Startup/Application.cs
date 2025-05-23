using Rag.LlmRouter;
using Rag.LlmRouter.Extensions;
using Rag.Startup.WebApp.Events;

namespace Rag.WebApi.Startup;

public class Application : Rag.Startup.WebApp.Startup.Application
{
    private readonly Router _router;

    public Application()
    {
        BeforeServiceContainerCreated += Application_BeforeServiceContainerCreated;
        AfterServiceContainerCreated += Application_AfterServiceContainerCreated;
        _router = new Router();
    }

    public override async Task Init(string[] args)
    {
        await base.Init(args);
    }

    private void Application_BeforeServiceContainerCreated(object? sender, BeforeServiceContainerCreatedEventArgs e)
    {
        e.Builder.Services.AddSingleton(_router);
        e.Builder.Services.AddLlms(e.Settings);
    }

    private void Application_AfterServiceContainerCreated(object? sender, AfterServiceContainerCreatedEventArgs e)
    {
        _router.Mistral = e.Host.Services.GetRequiredService<Connector.Mistral.SemanticService>();
        _router.OpenAi = e.Host.Services.GetRequiredService<Connector.OpenAi.SemanticService>();
        _router.Berget = e.Host.Services.GetRequiredService<Connector.Berget.SemanticService>();
        _router.Scaleway = e.Host.Services.GetRequiredService<Connector.Scaleway.SemanticService>();
    }
}
