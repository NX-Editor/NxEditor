using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Cead.Interop;
using CsMsbt;
using CsRestbl;
using Native.IO.Services;
using NxEditor.Component;
using NxEditor.Core;
using NxEditor.Generators;
using NxEditor.Models.Menus;
using NxEditor.PluginBase.Component;
using NxEditor.PluginBase.Models;
using NxEditor.ViewModels;
using NxEditor.Views;
using System.Runtime.CompilerServices;

namespace NxEditor;

public partial class App : Application
{
    private static readonly List<IEditor> _openEditors = new();

    public static string Title { get; } = "NX-Editor";
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
            Logger.Initialize();
            Logger.SetTraceListener(LogsViewModel.Shared.TraceListener);

            DllManager.LoadCead();
            NativeLibraryManager.RegisterAssembly(typeof(App).Assembly, out bool isCommonLoaded)
                .Register(new RestblLibrary(), out bool isRestblLoaded)
                .Register(new MsbtLibrary(), out bool isMsbtLoaded);

            PluginLoader.LoadInstalledPlugins();
            ShellViewModel.InitDock();

            Log($"Loaded native io library: {isCommonLoaded}");
            Log($"Loaded native cs_msbt: {isMsbtLoaded}");
            Log($"Loaded native cs_restbl: {isRestblLoaded}");

            desktop.MainWindow = Desktop = new ShellView() {
                DataContext = ShellViewModel.Shared
            };

            VisualRoot = desktop.MainWindow.GetVisualRoot() as TopLevel;

            // Make sure settings are always set
            SettingsView settings = new();
            if (Config.Shared.RequiresInput || settings.ValidateSave() != null) {
                ShellDockFactory.AddDoc<SettingsViewModel>();
                await Task.Run(() => {
                    while (Config.Shared.RequiresInput) {
                        // Wait for setting to save successfully
                    }
                });
            }

            desktop.MainWindow.Closed += (s, e) => {
                for (int i = 0; i < _openEditors.Count; i++) {
                    _openEditors[i].Dispose();
                }
            };

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
        NotificationManager?.Show(
            new Notification(title, message, type, expiration));
    }

    public static void ToastError(Exception ex)
    {
        NotificationManager?.Show(new Notification(
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
