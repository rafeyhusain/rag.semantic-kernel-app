
namespace Rag.SemanticKernel.Core.Sdk.Handlebar;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class HandlebarTemplate
{
    public string FilePath { get; set; } = Path.Combine(AppContext.BaseDirectory, @"data\templates\", "json.md");

    public string Prompt { get; set; }

    public void Load()
    {
        if (!File.Exists(FilePath))
            throw new FileNotFoundException("Handlebar template file not found.", FilePath);

        var content = File.ReadAllText(FilePath);
        var match = Regex.Match(content, @"^---\s*\n(?<yaml>[\s\S]*?)\n---\s*\n(?<markdown>[\s\S]*)$", RegexOptions.Multiline);

        if (!match.Success)
            throw new InvalidDataException("Invalid markdown format with front matter.");

        var yamlPart = match.Groups["yaml"].Value;
        var promptBody = match.Groups["markdown"].Value.Trim();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var yamlData = deserializer.Deserialize<Dictionary<string, string>>(yamlPart);

        Prompt = yamlData.TryGetValue("prompt", out var q) ? q : throw new InvalidDataException("Missing 'prompt' in YAML front matter.");
    }
}

