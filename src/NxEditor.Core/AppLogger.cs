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
    public event LogEventHandler? EventOccured;
    public event LogEventHandler? ExceptionOccured;

    public IList<LogEvent> LogEvents { get; } = [];

    /// <summary>
    /// Enabled logging to the system console.
    /// </summary>
    public void LogToConsole()
    {
        LogWritten += static (LogEvent e)
            => Console.WriteLine(e);

        EventOccured += static (LogEvent e) => {
            Console.ForegroundColor = e.Severity switch {
                Severity.Info => ConsoleColor.Blue,
                Severity.Success => ConsoleColor.Green,
                Severity.Warning => ConsoleColor.Yellow,
                Severity.Error => ConsoleColor.Red,
                _ => Console.ForegroundColor
            };

            Console.WriteLine(e);
            Console.ResetColor();
        };

        ExceptionOccured += static (LogEvent e) => {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Exception);
            Console.ResetColor();
        };
    }

    /// <summary>
    /// Triggers the <see cref="LogWritten"/> event handler and stores the <see langword="event"/>.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="severity"></param>
    public void Log(string message, Severity severity)
    {
        LogEvent logEvent = new(message, severity);
        LogWritten?.Invoke(logEvent);
        LogEvents.Add(logEvent);
    }

    /// <summary>
    /// Triggers the <see cref="EventOccured"/> event handler and stores the <see langword="event"/>.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="severity"></param>
    public void LogEvent(string message, Severity severity)
    {
        LogEvent logEvent = new(message, severity);
        EventOccured?.Invoke(logEvent);
        LogEvents.Add(logEvent);
    }

    /// <summary>
    /// Triggers the <see cref="ExceptionOccured"/> event handler and stores the <see langword="event"/>.
    /// </summary>
    /// <param name="ex"></param>
    public void LogError(Exception ex)
    {
        LogEvent logEvent = new(ex.Message, Severity.Error, ex);
        ExceptionOccured?.Invoke(logEvent);
        LogEvents.Add(logEvent);
    }
}
