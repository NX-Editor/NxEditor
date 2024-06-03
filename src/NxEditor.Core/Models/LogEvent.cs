namespace NxEditor.Core.Models;

public class LogEvent(string message, Severity severity, Exception? exception = null)
{
    public string Message { get; } = message;
    public Severity Severity { get; } = severity;
    public Exception? Exception { get; } = exception;

    public override string ToString()
    {
        return Exception switch {
            Exception ex => ex.ToString(),
            _ => $"[{Severity}] {Message}"
        };
    }
}
