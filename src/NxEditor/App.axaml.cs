using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using NxEditor.ViewModels;
using NxEditor.Views;

namespace NxEditor;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) {
            throw Exceptions.TargetPlatformNotSupported;
        }

        desktop.MainWindow = new ShellView {
            DataContext = new ShellViewModel()
        };

        base.OnFrameworkInitializationCompleted();
    }
}
