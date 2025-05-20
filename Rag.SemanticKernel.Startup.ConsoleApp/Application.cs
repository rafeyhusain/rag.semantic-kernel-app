namespace Rag.SemanticKernel.Startup.ConsoleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Startup.ConsoleApp.Events;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

public class Application
{
    public event EventHandler<BeforeServiceContainerCreatedEventArgs>? BeforeServiceContainerCreated;
    public event EventHandler<AfterServiceContainerCreatedEventArgs>? AfterServiceContainerCreated;

    public virtual async Task Init(string[] args)
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

        builder.Services.AddHttpClient("MyApi", client =>
        {
            client.BaseAddress = new Uri("https://api.example.com/");
        });

        builder.Services.AddSingleton<IConfiguration>(configuration);

        var settings = new Settings();
        configuration.Bind(settings); 

        OnBeforeServiceContainerCreated(builder, settings); 

        builder.Services.Configure<Settings>(configuration);
        builder.Services.AddSingleton(_ => settings);

        var host = builder.Build();

        OnAfterServiceContainerCreated(host); // raise AFTER event

        await Task.CompletedTask;
    }

    private void OnBeforeServiceContainerCreated(HostApplicationBuilder builder, Settings settings)
    {
        BeforeServiceContainerCreated?.Invoke(this, new BeforeServiceContainerCreatedEventArgs(builder, settings));
    }

    private void OnAfterServiceContainerCreated(IHost host)
    {
        AfterServiceContainerCreated?.Invoke(this, new AfterServiceContainerCreatedEventArgs(host));
    }

    public void ClearLog()
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var today = DateTime.Now.ToString("yyyyMMdd");
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
}
