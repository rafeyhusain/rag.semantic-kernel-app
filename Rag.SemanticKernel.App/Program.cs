using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rag.SemanticKernel.Core.Sdk.Service.Mistral;
using Serilog;

namespace Rag.SemanticKernel.App;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        //await Init(args);

        try
        {
            await Init(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static async Task Init(string[] args)
    {
        Log.Information("Starting application");

        ClearLog();

        var builder = Host.CreateApplicationBuilder(args);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(configuration["Log:FileName"] ?? "log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Services.AddSingleton<IConfiguration>(configuration);

        builder.Services.AddSemanticService(configuration);

        builder.Services.AddSingleton<SemanticService>();

        var host = builder.Build();

        // Register search plugin.
        host.AddSemanticService();

        var embeddingService = host.Services.GetRequiredService<SemanticService>();
        await embeddingService.Ask(args);

        Log.Information("Application completed successfully");

        Console.ReadKey();
    }

    private static void ClearLog()
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var today = DateTime.Now.ToString("yyyyMMdd"); // Use UtcNow if needed
        var logFileName = $"log-{today}.txt";
        var logFilePath = Path.Combine(logDirectory, logFileName);

        if (File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath, string.Empty);
            Console.WriteLine($"Cleared log file: {logFilePath}");
        }
        else
        {
            Console.WriteLine($"Log file not found: {logFilePath}");
        }
    }

}
