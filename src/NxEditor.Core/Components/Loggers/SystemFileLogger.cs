using NxEditor.Core.Models;

namespace NxEditor.Core.Components.Loggers;

public class SystemFileLogger : IAppLogger, IDisposable
{
    private static readonly string _logsFolderPath = Path.Combine(NXE.SystemPath, "logs");
    private readonly StreamWriter _writer;

    static SystemFileLogger()
    {
        Directory.CreateDirectory(_logsFolderPath);
    }

    public SystemFileLogger()
    {
        string[] existingLogFiles = Directory.GetFiles(_logsFolderPath);
        if (existingLogFiles.Length > 9) {
            ClearOldestFiles(existingLogFiles);
        }

        string currentLogFile = Path.Combine(_logsFolderPath, $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm}.log");
        FileStream fs = File.Open(currentLogFile, FileMode.OpenOrCreate);
        fs.Seek(fs.Length, SeekOrigin.Begin);
        _writer = new StreamWriter(fs);
    }

    public void Log(LogEvent e)
    {
        _writer.WriteLine(e);
        _writer.Flush();
    }

    public void LogEvent(LogEvent e)
    {
        _writer.WriteLine($"[{DateTime.UtcNow:u}] " + e);
        _writer.Flush();
    }

    public void LogException(LogEvent e)
    {
        _writer.WriteLine(e.Exception);
        _writer.Flush();
    }

    public void Dispose()
    {
        _writer.Dispose();
        GC.SuppressFinalize(this);
    }

    private static void ClearOldestFiles(Span<string> existingLogFiles)
    {
        foreach (string logFilePath in existingLogFiles[..^9]) {
            try {
                File.Delete(logFilePath);
            }
            catch (Exception ex) {
                NXE.Logger.LogError(ex);
            }
        }
    }
}
