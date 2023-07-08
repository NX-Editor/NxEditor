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

    public async Task Copy()
    {
        await App.Desktop!.Clipboard!.SetTextAsync($"{Meta}\n{Message}");
    }

    public async Task CopyMarkdown()
    {
        await App.Desktop!.Clipboard!.SetTextAsync($"""
            **{Meta}**
            ```
            {Message}
            ```
            """);
    }
}
