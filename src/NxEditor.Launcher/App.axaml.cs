using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NxEditor.Launcher.ViewModels;
using NxEditor.Launcher.Views;

namespace NxEditor.Launcher;
public partial class App : Application
{
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
        }

        base.OnFrameworkInitializationCompleted();
    }
}