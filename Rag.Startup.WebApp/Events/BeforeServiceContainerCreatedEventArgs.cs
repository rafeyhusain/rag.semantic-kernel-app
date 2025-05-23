namespace Rag.Startup.WebApp.Events;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Rag.AppSettings;
using System;

public class BeforeServiceContainerCreatedEventArgs : EventArgs
{
    public WebApplicationBuilder Builder { get; }
    public Settings Settings { get; }    

    public BeforeServiceContainerCreatedEventArgs(WebApplicationBuilder builder, Settings settings)
    {
        Builder = builder;
        Settings = settings;
    }
}
