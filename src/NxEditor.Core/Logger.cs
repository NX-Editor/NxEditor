using NxEditor.PluginBase;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NxEditor.Core;

public static class Logger
{
    private static readonly string _path = Path.Combine(GlobalConfig.Shared.StorageFolder, "logs");

    public static string? CurrentLog { get; set; }
    public static string LogsPath => _path;

    public static void Initialize()
    {
        Directory.CreateDirectory(_path);
        CurrentLog = Path.Combine(_path, $"{DateTime.Now:yyyy-MM-dd-HH-mm}.yml");

        TextWriterTraceListener listener = new(CurrentLog);
        SetTraceListener(listener, 1);

        ConsoleTraceListener console = new();
        SetTraceListener(console, 2);

        Trace.AutoFlush = true;
    }

    public static void Write(object obj, [CallerMemberName] string method = "")
    {
        string? msg = obj.ToString();

        if (msg?.Contains('\n') == true) {
            Trace.WriteLine($"""
                {DateTime.Now:dd:mm:ss:fff} [{method}]: |- 
                  {obj.ToString()?
                    .Replace("\n", "\n  ")
                    .Replace('\\', '/')}
                """);
        }
        else {
            Trace.WriteLine($"""
                {DateTime.Now:dd:mm:ss:fff} [{method}]: {obj.ToString()?.Replace("\n", "\n  ").Replace('\\', '/')}
                """);
        }
    }

    public static void SetTraceListener(TraceListener listener, int? pos = null)
    {
        if (pos is int _pos && Trace.Listeners.Count > _pos) {
            Trace.Listeners[_pos] = listener;
        }
        else {
            Trace.Listeners.Add(listener);
        }
    }
}
