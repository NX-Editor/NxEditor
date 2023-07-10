using CommunityToolkit.Mvvm.ComponentModel;
using NxEditor.ViewModels;

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
        if (ShellViewModel.Shared.View?.Clipboard?.SetTextAsync($"{Meta}\n{Message}") is Task task) {
            await task;
        }
    }

    public async Task CopyMarkdown()
    {
        if (ShellViewModel.Shared.View?.Clipboard?.SetTextAsync($"""
            **{Meta}**
            ```
            {Message}
            ```
            """) is Task task) {
            await task;
        }
    }
}
