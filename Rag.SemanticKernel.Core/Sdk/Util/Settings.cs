using System.IO;
using Microsoft.Extensions.Configuration;

namespace Rag.SemanticKernel.Core.Sdk.Util;

public class Settings
{
    public Settings()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        IndexName = Configuration.GetSection("IndexName").Get<string>()!;
    }

    public string IndexName { get; set; }
    public IConfigurationRoot Configuration { get; }
}
