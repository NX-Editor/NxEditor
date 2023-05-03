using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ExKingEditor.Core;
using ExKingEditor.Generators;
using ExKingEditor.ViewModels;
using ExKingEditor.Views;

namespace ExKingEditor;

public partial class App : Application
{
    public static string Title { get; } = "EX-King Editor";
    public static string? Version { get; } = typeof(App).Assembly.GetName().Version?.ToString(3);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            ShellViewModel.InitDock();
            desktop.MainWindow = new ShellView() {
                DataContext = ShellViewModel.Shared
            };

            // Make sure settings are always set
            SettingsView settings = new();
            if (ExConfig.Shared.RequiresInput || settings.ValidateSave() != null) {
                ShellDockFactory.AddDoc<SettingsViewModel>();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
