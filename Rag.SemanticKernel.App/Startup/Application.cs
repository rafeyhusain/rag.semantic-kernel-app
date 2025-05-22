using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.TextGeneration;
using Rag.SemanticKernel.Llm.Core.ChatCompletion;
using Rag.SemanticKernel.Llm.Core.Extensions;
using Rag.SemanticKernel.Model.Vector;
using Rag.SemanticKernel.Parser.Markdown;
using Rag.SemanticKernel.Startup.ConsoleApp.Events;
using Rag.SemanticKernel.Startup.ConsoleApp.Startup;
using System.Threading.Tasks;

namespace Rag.SemanticKernel.App.Startup;

public class Application<T, TRecord> : Application
    where T : class, IDocument, new()
    where TRecord : class
{
    private Llm.Core.Api.SemanticService<T, TRecord> _semanticService;

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
        e.Builder.Services.AddTransient<Llm.Core.Embedding.EmbeddingService<T, TRecord>>();
        e.Builder.Services.AddTransient<Llm.Core.ChatCompletion.ChatCompletionService<TRecord>>();
        e.Builder.Services.AddTransient<Llm.Core.Api.SemanticService<T, TRecord>>();

        e.Builder.Services.AddSemanticService<T, TRecord, MarkdownFileParser>(
            e.Settings,
            Options.PairName);
    }

    private void Application_AfterServiceContainerCreated(object sender, AfterServiceContainerCreatedEventArgs e)
    {
        _semanticService = e.Host.Services.GetRequiredService<Llm.Core.Api.SemanticService<T, TRecord>>();
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
