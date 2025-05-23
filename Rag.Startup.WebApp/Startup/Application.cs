namespace Rag.Startup.WebApp.Startup;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rag.AppSettings;
using Rag.CommandLine;
using Rag.Startup.WebApp.Events;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

public class Application
{
    public CommandLineOptions Options { get; private set; } = new();

    public event EventHandler<BeforeServiceContainerCreatedEventArgs>? BeforeServiceContainerCreated;
    public event EventHandler<AfterServiceContainerCreatedEventArgs>? AfterServiceContainerCreated;

    public virtual async Task Init(string[] args)
    {
        Options = new CommandLineOptions(args);
        Log.Information($"Starting application with pair[{Options.PairName}]");

        Logger.Extensions.Log.ClearLog();

        var builder = WebApplication.CreateBuilder(args);

        var env = builder.Environment;

        Log.Information($"Running in: {env.EnvironmentName}");

        var configuration = builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var settings = new Settings();
        configuration.Bind(settings);

        foreach (var pair in settings.Pairs)
        {
            pair.Settings = settings;
        }

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(configuration["Log:FileName"] ?? "log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Services.AddSingleton<IConfiguration>(configuration);

        OnBeforeServiceContainerCreated(builder, settings);

        builder.Services.Configure<Settings>(configuration);
        builder.Services.AddSingleton(_ => settings);
        builder.Services.AddHttpClient();

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var host = builder.Build();

        if (host.Environment.IsDevelopment())
        {
            host.MapOpenApi();
        }

        host.UseHttpsRedirection();

        host.UseAuthorization();

        host.MapControllers();

        OnAfterServiceContainerCreated(host);

        host.Run();

        await Task.CompletedTask;
    }

    private void OnBeforeServiceContainerCreated(WebApplicationBuilder builder, Settings settings)
    {
        BeforeServiceContainerCreated?.Invoke(this, new BeforeServiceContainerCreatedEventArgs(builder, settings));
    }

    private void OnAfterServiceContainerCreated(IHost host)
    {
        AfterServiceContainerCreated?.Invoke(this, new AfterServiceContainerCreatedEventArgs(host));
    }
}

