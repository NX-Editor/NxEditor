using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NxEditor.Launcher.ViewModels;
using NxEditor.Launcher.Views;
using NxEditor.PluginBase.Common;

namespace NxEditor.Launcher;
public partial class App : Application
{
    public static string? Version { get; } = typeof(App).Assembly.GetName().Version?.ToString(3);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new ShellView {
                DataContext = new ShellViewModel(),
            };

            if (desktop.MainWindow.Content is Grid root) {
                DialogBox.SetViewRoot(root);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}