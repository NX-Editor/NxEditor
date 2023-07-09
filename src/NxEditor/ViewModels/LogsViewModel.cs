using CommunityToolkit.Mvvm.ComponentModel;
using NxEditor.Components;
using NxEditor.Helpers;
using NxEditor.Models;
using NxEditor.Views;
using System.Collections.ObjectModel;

namespace NxEditor.ViewModels;

public partial class LogsViewModel : StaticPage<LogsViewModel, LogsView>
{
    public LogsViewTraceListener TraceListener { get; }

    [ObservableProperty]
    private ObservableCollection<LogEntry> _logTrace = new();

    [ObservableProperty]
    private LogEntry? _selected;

    public void AddLog(string message)
    {
        // Ignore avalonia errors
        if (message.Contains("[Visual]") == true || message.Contains("[Binding]") == true) {
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
    }

    public LogsViewModel()
    {
        Id = nameof(LogsViewModel);
        Title = "Logs";
        TraceListener = new(AddLog);
    }
}
