using NxEditor.Core.Models;

namespace NxEditor.Core;

public enum Severity
{
    Info,
    Success,
    Warning,
    Error
}

public delegate void LogEventHandler(LogEvent e);

public class AppLogger
{
    public event LogEventHandler? LogWritten;
    public event LogEventHandler? ExceptionOccured;

    public IList<LogEvent> LogEvents { get; } = [];

    public void LogToConsole()
    {
        LogWritten += static (LogEvent e)
            => Console.WriteLine(e);

        ExceptionOccured += static (LogEvent e)
            => Console.WriteLine(e.Exception);
    }

    public void Log(string message, Severity severity)
    {
        LogEvent logEvent = new(message, severity);
        LogWritten?.Invoke(logEvent);
        LogEvents.Add(logEvent);
    }

    public void LogError(Exception ex)
    {
        LogEvent logEvent = new(ex.Message, Severity.Error, ex);
        ExceptionOccured?.Invoke(logEvent);
        LogEvents.Add(logEvent);
    }
}
