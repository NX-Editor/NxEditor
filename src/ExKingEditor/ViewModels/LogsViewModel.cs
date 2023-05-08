using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;
using ExKingEditor.Helpers;
using ExKingEditor.Models;
using ExKingEditor.Views;
using System.Collections.ObjectModel;

namespace ExKingEditor.ViewModels;

public partial class LogsViewModel : Document
{
    private LogsView? _view;

    public static LogsViewModel Shared { get; } = new();
    public LogsViewTraceListener TraceListener { get; }

    [ObservableProperty]
    private ObservableCollection<LogEntry> _logTrace = new();

    [ObservableProperty]
    private LogEntry? _selected;

    public void AddLog(string message)
    {
        // Ignore avalonia errors
        if (message.Contains("[Visual]") == true || message.Contains("[Binding]") == true) {
            // Write to the system console
            Console.WriteLine(message);
            return;
        }

        int idx = message.LastIndexOf('|');
        if (idx >= 0) {
            string meta = message[..idx];
            string msg = message[(idx + 1)..].Replace(new string(' ', meta.Length + 2), "");
            LogTrace.Add(new(meta.Trim(), msg.Trim()));
        }
        else {
            LogTrace.Add(new(message, message));
        }

        Dispatcher.UIThread.InvokeAsync(() => {
            _view?.LogsClient.ScrollIntoView(LogTrace.Count - 1);
        });
    }

    public async Task Copy()
    {
        await Application.Current!.Clipboard!.SetTextAsync($"{Selected?.Meta}\n{Selected?.Message}");
    }

    public async Task CopyMarkdown()
    {
        await Application.Current!.Clipboard!.SetTextAsync($"""
            **{Selected?.Meta}**
            ```
            {Selected?.Message}
            ```
            """);
    }

    internal void InjectView(LogsView logsView)
    {
        _view = logsView;
    }

    public LogsViewModel()
    {
        Id = nameof(LogsViewModel);
        Title = "Logs";
        TraceListener = new(AddLog);
    }
}
