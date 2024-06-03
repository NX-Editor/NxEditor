using NxEditor.Core.Models;

namespace NxEditor.Core;

public interface IAppLogger
{
    public void Log(LogEvent e);
    public void LogEvent(LogEvent e);
    public void LogException(LogEvent e);
}
