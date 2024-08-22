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

public class NxeLogger
{
    public event LogEventHandler? LogWritten;
    public event LogEventHandler? EventOccured;
    public event LogEventHandler? ExceptionOccured;

    public IList<LogEvent> Logs { get; } = [];

    /// <summary>
    /// Register an <see cref="IAppLogger"/> implementation.
    /// </summary>
    public NxeLogger Register<T>() where T : IAppLogger, new()
    {
        T logger = new();
        return Register(logger);
    }

    /// <summary>
    /// Register an <see cref="IAppLogger"/> implementation.
    /// </summary>
    public NxeLogger Register(IAppLogger logger)
    {
        LogWritten += logger.Log;
        EventOccured += logger.LogEvent;
        ExceptionOccured += logger.LogException;

        return this;
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
        Logs.Add(logEvent);
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
        Logs.Add(logEvent);
    }

    /// <summary>
    /// Triggers the <see cref="ExceptionOccured"/> event handler and stores the <see langword="event"/>.
    /// </summary>
    /// <param name="ex"></param>
    public void LogException(Exception ex)
    {
        LogEvent logEvent = new(ex.Message, Severity.Error, ex);
        ExceptionOccured?.Invoke(logEvent);
        Logs.Add(logEvent);
    }
}
