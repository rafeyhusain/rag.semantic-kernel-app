using Rag.SemanticKernel.Guards;

namespace Rag.SemanticKernel.AppSettings;

public class Settings
{
    public string CurrentPairName { get; set; } = "";
    public ModelPairSettings CurrentPairSetting => string.IsNullOrEmpty(CurrentPairName) ? new ModelPairSettings(this) : this[CurrentPairName];
    public ElasticsearchSettings Elasticsearch { get; set; } = new();
    public List<ModelPairSettings> Pairs { get; set; } = [];
    public SerilogSettings Serilog { get; set; } = new();
    public PollySettings Polly { get; set; } = new();
    public ModelPairSettings this[string modelPair]
    {
        get
        {
            var pair = Pairs.FirstOrDefault(m => m.Name.Equals(modelPair, StringComparison.CurrentCultureIgnoreCase)) ?? throw new KeyNotFoundException($"Model '{modelPair}' not found.");

            pair.Settings = this;

            return pair;
        }
    }
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

public class ModelPairSettings
{
    public ModelPairSettings() { }

    public ModelPairSettings(Settings settings)
    {
        Settings = settings;
    }

    public string Name { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string CompletionModel { get; set; } = "";
    public string EmbeddingModel { get; set; } = "";
    public Settings Settings { get; set; }

    public void Set(ModelPairSettings pairSettings)
    {
        Guard.ThrowIfNull(pairSettings);

        Name = pairSettings.Name;
        Endpoint = pairSettings.Endpoint;
        ApiKey = pairSettings.ApiKey;
        CompletionModel = pairSettings.CompletionModel;
        EmbeddingModel = pairSettings.EmbeddingModel;
        Settings = pairSettings.Settings;
    }
}

public class SerilogSettings
{
    public List<string> Using { get; set; } = [];
    public MinimumLevelSettings MinimumLevel { get; set; } = new();
    public List<WriteToSettings> WriteTo { get; set; } = [];
    public List<string> Enrich { get; set; } = [];
    public Dictionary<string, string> Properties { get; set; } = [];
}

public class MinimumLevelSettings
{
    public string Default { get; set; } = "";
    public Dictionary<string, string> Override { get; set; } = [];
}

public class WriteToSettings
{
    public string Name { get; set; } = "";
    public Dictionary<string, string> Args { get; set; } = [];
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
