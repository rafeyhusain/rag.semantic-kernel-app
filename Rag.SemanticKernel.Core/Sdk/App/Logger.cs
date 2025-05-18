
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Rag.SemanticKernel.Core.Sdk.App;

public static class Logger
{
    public static void LogInformation2(this ILogger logger, string message)
    {
        logger.LogInformation(message);
    }

    public static void LogCritical2(this ILogger logger, Exception ex, string message)
    {
        var fullMessage = GetMessage(ex, message);

        logger.LogError(fullMessage);
    }

    public static string GetMessage(Exception ex, string message = null)
    {
        List<string> messages = [];
        string stackTrace = "";

        if (ex != null)
        {
            stackTrace = ex.StackTrace + Environment.NewLine;
        }

        while (ex != null)
        {
            messages.Add(ex.Message + Environment.NewLine);
            ex = ex.InnerException;
        }

        messages.Add(stackTrace);

        var fullMessage = message + Environment.NewLine + string.Join(Environment.NewLine, messages);

        return fullMessage;
    }
}