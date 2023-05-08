using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ExKingEditor.Core;

public static class Logger
{
    private static readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    private static readonly string SourceRoot = Path.Combine("D:", "a", "EXKing-Editor", "src", "ExKingEditor");

    public static string? CurrentLog { get; set; }
    public static string LogsPath => _path; 

    public static void Initialize()
    {
        Directory.CreateDirectory(_path);
        CurrentLog = Path.Combine(_path, $"{DateTime.Now:yyyy-MM-dd-HH-mm}.log");

        TextWriterTraceListener listener = new(CurrentLog);

        SetTraceListener(listener, 1);
        Trace.AutoFlush = true;
    }

    public static void Write(object obj, [CallerMemberName] string method = "", [CallerFilePath] string filepath = "", [CallerLineNumber] int lineNumber = 0)
    {
        string meta = $"{DateTime.Now:dd:mm:ss:fff} [{method}] | \"{Path.GetRelativePath(SourceRoot, filepath)}\":{lineNumber} | ";
        Trace.WriteLine($"{meta}{obj.ToString()?.Replace("\n", $"\n{new string(' ', meta.Length)}")}".Replace('\\', '/'));
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
