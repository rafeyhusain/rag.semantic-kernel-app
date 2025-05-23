using Microsoft.Extensions.Options;
using Rag.SemanticKernel.AppSettings;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

builder.Services.Configure<Settings>(builder.Configuration);
builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<IOptions<Settings>>().Value);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(configuration["Log:FileName"] ?? "log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddSingleton<IConfiguration>(configuration);

builder.Services.AddTransient<Rag.Connector.Mistral.EmbeddingService>();
builder.Services.AddTransient<Rag.Connector.Mistral.ChatCompletionService>();
builder.Services.AddTransient<Rag.Connector.OpenAi.SemanticService>();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
