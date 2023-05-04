using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Cead.Interop;
using ExKingEditor.Core;
using ExKingEditor.Generators;
using ExKingEditor.Helpers;
using ExKingEditor.Models;
using ExKingEditor.ViewModels;
using ExKingEditor.Views;
using System.Runtime.CompilerServices;

namespace ExKingEditor;

public partial class App : Application
{
    public static string Title { get; } = "EX-King Editor";
    public static string? Version { get; } = typeof(App).Assembly.GetName().Version?.ToString(3);
    public static TopLevel? VisualRoot { get; private set; }

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

            desktop.MainWindow = new ShellView() {
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

            if (desktop.Args != null && desktop.Args.Length > 0) {
                foreach (var arg in desktop.Args) {
                    EditorMgr.TryLoadEditorSafe(arg, out _);
                }
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void Log(object message, [CallerMemberName] string method = "", [CallerFilePath] string filepath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Logger.Write(message, method, filepath, lineNumber);
    }
}
