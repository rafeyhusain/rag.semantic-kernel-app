using Microsoft.Extensions.Logging;

namespace Rag.SemanticKernel.Logger.Extensions;

public static class Log
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

    public static string GetMessage(Exception? ex, string? message)
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

    public static void ClearLog()
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var today = DateTime.Now.ToString("yyyyMMdd");
        var logFileName = $"log-{today}.txt";
        var logFilePath = Path.Combine(logDirectory, logFileName);

        if (File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath, string.Empty);
        }
        else
        {
            Console.WriteLine($"Log file not found: {logFilePath}");
        }
    }
}