using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Rag.SemanticKernel.Core.Sdk.Service.Mistral;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Rag.SemanticKernel.Core.Sdk.App;

public class Application
{
    private Kernel _kernel;
    private SemanticService _semanticService;

    public async Task Init(string[] args)
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

        _kernel = host.Services.GetService<Kernel>()!;

        host.AddSemanticService(_kernel);

        _semanticService = host.Services.GetService<SemanticService>();

        await Task.CompletedTask;
    }

    public void ClearLog()
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var today = DateTime.Now.ToString("yyyyMMdd"); // Use UtcNow if needed
        var logFileName = $"log-{today}.txt";
        var logFilePath = Path.Combine(logDirectory, logFileName);

        if (File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath, string.Empty);
        }
        else
        {
            Console.WriteLine($"Log file not found: {logFilePath}");
        }
    }

    public async Task GenerateEmbeddings()
    {
        await _semanticService.GenerateEmbeddings(_kernel);
    }

    public async Task<string> Ask(string question)
    {
        return await _semanticService.Ask(_kernel, question);
    }
}
