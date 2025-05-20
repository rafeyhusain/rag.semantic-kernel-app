using System;
using System.Threading.Tasks;
using Rag.SemanticKernel.Logger.Extensions;

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
            Console.WriteLine(Log.GetMessage(ex, "Application terminated unexpectedly"));
        }
    }
}
