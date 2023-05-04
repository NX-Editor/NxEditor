using System.Diagnostics;

namespace ExKingEditor.Helpers;

public class LogsViewTraceListener : TraceListener
{
    private readonly Action<string> _writeToBase;

    public LogsViewTraceListener(Action<string> writeToBase)
    {
        Name = nameof(LogsViewTraceListener);
        _writeToBase = writeToBase;
    }

    public override void Write(string? message)
    {
        if (message != null) {
            _writeToBase(message);
        }
    }

    public override void WriteLine(string? message)
    {
        if (message != null) {
            _writeToBase(message);
        }
    }
}
