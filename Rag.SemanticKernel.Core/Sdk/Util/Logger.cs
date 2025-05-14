
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Serilog;

//namespace Rag.SemanticKernel.Core.Sdk.Util;

//public class Logger
//{
//    public string LogFileName { get; private set; }

//    public Logger(IConfiguration configuration)
//    {
//        LogFileName = configuration["Log:FileName"]!;

//        Log.Logger = new LoggerConfiguration()
//        .WriteTo.Console()
//        .WriteTo.File(LogFileName)
//        .CreateLogger();
//    }

//    public void Close()
//    {
//        Log.CloseAndFlush();
//    }

//    public void Info(string message)
//    {
//        Log.Information(message);
//    }

//    public void Error(string message, Exception? ex)
//    {
//        string fullMessage = message + Environment.NewLine + GetExceptionMessages(ex);

//        Log.Error(fullMessage);
//    }

//    private string GetExceptionMessages(Exception? ex)
//    {
//        List<string> messages = [];
//        string stackTrace = "";

//        if (ex != null)
//        {
//            stackTrace = ex.StackTrace + Environment.NewLine;
//        }

//        while (ex != null)
//        {
//            messages.Add(ex.Message + Environment.NewLine);
//            ex = ex.InnerException;
//        }

//        messages.Add(stackTrace);

//        return string.Join(Environment.NewLine, messages);
//    }
//}