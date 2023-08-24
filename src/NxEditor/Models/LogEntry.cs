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
        if (ShellViewModel.Shared.View?.Clipboard?.SetTextAsync($"{Message}") is Task task) {
            await task;
        }
    }
}
