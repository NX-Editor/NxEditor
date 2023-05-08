using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Cead.Interop;
using ExKingEditor.Core;
using ExKingEditor.Generators;
using ExKingEditor.Models;
using ExKingEditor.ViewModels;
using ExKingEditor.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExKingEditor;

public partial class App : Application
{
    public static string Title { get; } = "EX-King Editor";
    public static string? Version { get; } = typeof(App).Assembly.GetName().Version?.ToString(3);
    public static TopLevel? VisualRoot { get; private set; }
    public static WindowNotificationManager? NotificationManager { get; set; }

    public static ShellView? Desktop { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            DllManager.LoadCead();
            ShellViewModel.InitDock();

            Logger.Initialize();
            Logger.SetTraceListener(LogsViewModel.Shared.TraceListener);

            desktop.MainWindow = Desktop = new ShellView() {
                DataContext = ShellViewModel.Shared
            };

            VisualRoot = desktop.MainWindow.GetVisualRoot() as TopLevel;

            // Make sure settings are always set
            SettingsView settings = new();
            if (ExConfig.Shared.RequiresInput || settings.ValidateSave() != null) {
                ShellDockFactory.AddDoc<SettingsViewModel>();
                await Task.Run(() => {
                    while (ExConfig.Shared.RequiresInput) {
                        // Wait for setting to save successfully
                    }
                });
            }

            desktop.MainWindow.Closed += (s, e) => {
                for (int i = 0; i < ReactiveEditor.OpenEditors.Count; i++) {
                    ReactiveEditor.OpenEditors[i].Dispose();
                }
            };

            if (desktop.Args != null && desktop.Args.Length > 0) {
                foreach (var arg in desktop.Args) {
                    EditorMgr.TryLoadEditorSafe(arg, out _);
                }
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void Toast(string message, string title = "Notice", NotificationType type = NotificationType.Information, TimeSpan? expiration = null)
    {
        NotificationManager?.Show(
            new Notification(title, message, type, expiration));
    }

    public static void ToastError(Exception ex)
    {
        NotificationManager?.Show(new Notification(
            ex.GetType().Name, ex.Message, NotificationType.Error, onClick: ShellMenu.OpenLogs));
    }

    public static void Log(object obj, [CallerMemberName] string method = "", [CallerFilePath] string filepath = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (obj is Exception ex) {
            ToastError(ex);
        }

        Logger.Write(obj, method, filepath, lineNumber);
    }
}
