using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using ConfigFactory.Avalonia.Helpers;
using NxEditor.Core.Components;
using NxEditor.Generators;
using NxEditor.Models.Menus;
using NxEditor.PluginBase.Models;
using System.Runtime.CompilerServices;

namespace NxEditor;

public partial class App : Application
{
    private static readonly List<IEditor> _openEditors = new();
    private static WindowNotificationManager? _notificationManager;

    public static string Title { get; } = "NX-Editor";
    public static string? Version { get; } = typeof(App).Assembly.GetName().Version?.ToString(3);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        Logger.Initialize();
        Logger.SetTraceListener(LogsViewModel.Shared.TraceListener);

        PluginManager pluginMgr = new(ConfigViewModel.Shared);
        pluginMgr.LoadInstalled(out bool isConfigValid);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            ShellViewModel.Shared.InitDock();
            Config.SetTheme(Config.Shared.Theme);

            desktop.MainWindow = ShellViewModel.Shared.View;

            TopLevel? visualRoot = desktop.MainWindow.GetVisualRoot() as TopLevel;
            BrowserDialog.StorageProvider = visualRoot?.StorageProvider ?? null;

            desktop.MainWindow.Closed += (s, e) => {
                for (int i = 0; i < _openEditors.Count; i++) {
                    _openEditors[i].Dispose();
                }
            };

            desktop.MainWindow.Loaded += (s, e) => {
                _notificationManager = new(visualRoot) {
                    Position = NotificationPosition.BottomRight,
                    MaxItems = 3,
                    Margin = new(0, 0, 4, 0)
                };
            };

            ConfigViewModel.Shared.IsValid = isConfigValid;

            if (!ConfigViewModel.Shared.IsValid) {
                ShellDockFactory.AddDoc(ConfigViewModel.Shared);

                await Task.Run(() => {
                    while (!ConfigViewModel.Shared.IsValid) { }
                });
            }

            pluginMgr.Register();

            if (desktop.Args != null && desktop.Args.Length > 0) {
                foreach (var arg in desktop.Args) {
                    EditorMgr.TryLoadEditorSafe(new FileHandle(arg));
                }
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void Toast(string message, string title = "Notice", NotificationType type = NotificationType.Information, TimeSpan? expiration = null)
    {
        _notificationManager?.Show(
            new Notification(title, message, type, expiration));
    }

    public static void ToastError(Exception ex)
    {
        _notificationManager?.Show(new Notification(
            ex.GetType().Name, ex.Message, NotificationType.Error, onClick: ShellViewMenu.OpenLogs));
    }

    public static void Log(object obj, [CallerMemberName] string method = "", [CallerFilePath] string filepath = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (obj is Exception ex) {
            ToastError(ex);
        }

        Logger.Write(obj, method, filepath, lineNumber);
    }
}
