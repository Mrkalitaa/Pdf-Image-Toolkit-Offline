using System;
using System.IO;
using System.Threading;

namespace PdfImageToolkit.Core.Services;

public interface ILoggingService
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
    string GetLogFilePath();
}

public class LoggingService : ILoggingService
{
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public LoggingService()
    {
        var localDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PdfImageToolkit",
            "Logs");

        Directory.CreateDirectory(localDir);
        _logFilePath = Path.Combine(localDir, "app.log");
    }

    public string GetLogFilePath() => _logFilePath;

    public void LogInfo(string message) => WriteLog("INFO", message);

    public void LogWarning(string message) => WriteLog("WARN", message);

    public void LogError(string message, Exception? exception = null)
    {
        var formatted = exception != null 
            ? $"{message}{Environment.NewLine}Exception: {exception.GetType().FullName}: {exception.Message}{Environment.NewLine}{exception.StackTrace}"
            : message;
        WriteLog("ERROR", formatted);
    }

    private void WriteLog(string level, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logLine = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";

        lock (_lock)
        {
            try
            {
                File.AppendAllText(_logFilePath, logLine);
            }
            catch
            {
                // Never crash the app due to logging failure
            }
        }
    }
}
