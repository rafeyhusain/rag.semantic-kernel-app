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
        try
        {
            Log.Information("Starting application");

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
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
