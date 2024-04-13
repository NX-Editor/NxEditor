namespace NxEditor.Models;

public partial class LogEntry(string meta, string message) : ObservableObject
{
    [ObservableProperty]
    private string _meta = meta;

    [ObservableProperty]
    private string _message = message;

    public async Task Copy()
    {
        if (ShellViewModel.Shared.View?.Clipboard?.SetTextAsync(Message != Meta ? $"""
            **{Message}**
            ```
            {Meta}
            ```
            """ : Message) is Task task) {
            await task;
        }
    }
}
