namespace Rag.SemanticKernel.AppSettings;

public class Settings
{
    public ElasticsearchSettings Elasticsearch { get; set; } = new();
    public List<ModelSettings> Models { get; set; } = new();
    public SerilogSettings Serilog { get; set; } = new();
    public PollySettings Polly { get; set; } = new();
    public ModelSettings this[string modelName] =>
        Models.FirstOrDefault(m => m.Name.ToLower() == modelName.ToLower()) ?? throw new KeyNotFoundException($"Model '{modelName}' not found.");
}

public class ElasticsearchSettings
{
    public string Url { get; set; } = "";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string Index { get; set; } = "";
    public string this[string modelName]
    {
        get
        {
            var raw = $"{Index}_{modelName}";
            var lower = raw.ToLowerInvariant();

            // Replace invalid characters (keep lowercase letters, digits, underscores, and hyphens)
            var safe = System.Text.RegularExpressions.Regex.Replace(lower, @"[^a-z0-9_\-]", "");
            return safe;
        }
    }
}

public class ModelSettings
{
    public string Name { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string CompletionModel { get; set; } = "";
    public string EmbeddingModel { get; set; } = "";
}

public class SerilogSettings
{
    public List<string> Using { get; set; } = new();
    public MinimumLevelSettings MinimumLevel { get; set; } = new();
    public List<WriteToSettings> WriteTo { get; set; } = new();
    public List<string> Enrich { get; set; } = new();
    public Dictionary<string, string> Properties { get; set; } = new();
}

public class MinimumLevelSettings
{
    public string Default { get; set; } = "";
    public Dictionary<string, string> Override { get; set; } = new();
}

public class WriteToSettings
{
    public string Name { get; set; } = "";
    public Dictionary<string, string> Args { get; set; } = new();
}

public class PollySettings
{
    public RetryPolicySettings RetryPolicy { get; set; } = new();
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();
}

public class RetryPolicySettings
{
    public int RetryCount { get; set; } = 5;
    public double BaseDelaySeconds { get; set; } = 2.0;
}

public class CircuitBreakerSettings
{
    public int Failures { get; set; } = 5;
    public double DurationMinutes { get; set; } = 1.0;
}
