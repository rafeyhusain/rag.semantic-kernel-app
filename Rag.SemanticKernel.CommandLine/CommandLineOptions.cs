using System;
using System.Collections.Generic;

namespace Rag.SemanticKernel.CommandLine;


public class CommandLineOptions
{
    private readonly Dictionary<string, string> _parameters = new();

    public string? PairName => GetValue("model-pair") ?? "mistral";

    public CommandLineOptions(string[] args)
    {
        foreach (var arg in args)
        {
            var parts = arg.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                _parameters[key] = value;
            }
        }
    }

    public CommandLineOptions()
    {
    }

    private string? GetValue(string key)
    {
        return _parameters.TryGetValue(key, out var value) ? value : null;
    }
}
