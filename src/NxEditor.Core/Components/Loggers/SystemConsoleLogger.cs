using NxEditor.Core.Models;

namespace NxEditor.Core.Components.Loggers;

public class SystemConsoleLogger : IAppLogger
{
    public void Log(LogEvent e)
    {
        Console.WriteLine(e);
    }

    public void LogEvent(LogEvent e)
    {
        Console.ForegroundColor = e.Severity switch {
            Severity.Info => ConsoleColor.Blue,
            Severity.Success => ConsoleColor.Green,
            Severity.Warning => ConsoleColor.Yellow,
            Severity.Error => ConsoleColor.Red,
            _ => Console.ForegroundColor
        };

        Console.WriteLine($"[{DateTime.UtcNow:u}] " + e);
        Console.ResetColor();
    }

    public void LogException(LogEvent e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Exception);
        Console.ResetColor();
    }
}
