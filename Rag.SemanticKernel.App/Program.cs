using Rag.SemanticKernel.Logger.Extensions;
using Rag.SemanticKernel.Model.Vector;
using System;
using System.Threading.Tasks;

namespace Rag.SemanticKernel.App;

internal sealed class Program
{
    private static Startup.Application<MarkdownDocument, Markdown> _app;

    public static async Task Main(string[] args)
    {
        try
        {
            _app = new Startup.Application<MarkdownDocument, Markdown>();

            await _app.Init(args);

            await _app.GenerateEmbeddings();
            //await Ask();

            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine(Log.GetMessage(ex, "Application terminated unexpectedly"));
        }
    }

    private static async Task Ask()
    {
        var answer = await _app.Ask("det sanna värdet");
        Console.WriteLine($"{answer}");
    }
}
