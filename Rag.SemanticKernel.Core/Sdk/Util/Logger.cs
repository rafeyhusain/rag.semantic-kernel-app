
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Rag.SemanticKernel.Core.Sdk.Util;

public static class Logger
{
    public static void LogInformation2(this ILogger logger, string message)
    {
        logger.LogInformation(message);
    }

    public static void LogCritical2(this ILogger logger, Exception ex, string message)
    {
        string fullMessage = message + Environment.NewLine + GetExceptionMessages(ex);

        logger.LogError(fullMessage);
    }

    private static string GetExceptionMessages(Exception ex)
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

        return string.Join(Environment.NewLine, messages);
    }
}