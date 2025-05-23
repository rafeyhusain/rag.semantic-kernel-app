namespace Rag.Startup.ConsoleApp.Events;
using Microsoft.Extensions.Hosting;
using System;

public class AfterServiceContainerCreatedEventArgs : EventArgs
{
    public IHost Host { get; }

    public AfterServiceContainerCreatedEventArgs(IHost host)
    {
        Host = host;
    }
}
