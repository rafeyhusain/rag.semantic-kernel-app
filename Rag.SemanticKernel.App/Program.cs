using Rag.SemanticKernel.Core.Sdk.App;
using System;
using System.Threading.Tasks;

namespace Rag.SemanticKernel.App;

internal sealed class Program
{
    private static Startup.Application _app;

    public static async Task Main(string[] args)
    {
        try
        {
            _app = new Startup.Application();
            
            await _app.Init(args);

            await _app.GenerateEmbeddings();

            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine(Logger.GetMessage(ex, "Application terminated unexpectedly"));
        }
    }
}
