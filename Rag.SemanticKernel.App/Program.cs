using Microsoft.Extensions.Logging;
using Rag.SemanticKernel.Core.Sdk.Util;

namespace Rag.SemanticKernel.ConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        var settings = new Settings();

        var logger = new Logger(settings.Configuration);

        logger.Info("RAG Semantic Search App Started...");

        await Task.CompletedTask;

        Console.ReadKey();
    }
}
