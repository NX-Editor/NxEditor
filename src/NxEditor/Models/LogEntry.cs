using CommunityToolkit.Mvvm.ComponentModel;

namespace NxEditor.Models;

public partial class LogEntry : ObservableObject
{
    [ObservableProperty]
    private string _meta;

    [ObservableProperty]
    private string _message;

    public LogEntry(string meta, string message)
    {
        _meta = meta;
        _message = message;
    }
}
