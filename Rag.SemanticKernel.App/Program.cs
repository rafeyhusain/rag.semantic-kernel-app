using Microsoft.SemanticKernel;
using Rag.SemanticKernel.Core.Sdk.App;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Rag.SemanticKernel.App;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var app = new Application();

            await app.Init(args);

            await app.GenerateEmbeddings();

            var answer = await app.Ask("det sanna värdet");

            Console.WriteLine($"{answer}");

            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine(Logger.GetMessage(ex, "Application terminated unexpectedly"));
        }
    }
}
