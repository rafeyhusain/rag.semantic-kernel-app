namespace Rag.SemanticKernel.Startup.ConsoleApp.Events;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Rag.SemanticKernel.AppSettings;
using System;

public class BeforeServiceContainerCreatedEventArgs : EventArgs
{
    public HostApplicationBuilder Builder { get; }
    public Settings Settings { get; }    

    public BeforeServiceContainerCreatedEventArgs(HostApplicationBuilder builder, Settings settings)
    {
        Builder = builder;
        Settings = settings;
    }
}
