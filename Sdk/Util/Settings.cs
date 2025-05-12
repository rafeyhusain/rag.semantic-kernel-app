using Microsoft.Extensions.Configuration;

namespace Rag.SemanticKernel.Core.Sdk.Util;

public class Settings
{
    public Settings()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        IndexName = config.GetSection("IndexName").Get<string>()!;
    }

    public string IndexName { get; set; }
}
