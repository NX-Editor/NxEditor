using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using NxEditor.Core;
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
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) {
            throw Exceptions.TargetPlatformNotSupported;
        }

        BindingPlugins.DataValidators.RemoveAt(0);
        NXE.Config.AppThemeChanged += SetRequestedThemeVariant;

        desktop.MainWindow = new ShellView {
            DataContext = new ShellViewModel()
        };
    }

    private void SetRequestedThemeVariant(object? _, AppTheme theme)
    {
        RequestedThemeVariant = theme switch {
            AppTheme.System => ThemeVariant.Default,
            AppTheme.Dark => ThemeVariant.Dark,
            AppTheme.Light => ThemeVariant.Light,
            _ => throw Exceptions.AppThemeNotSupported
        };
    }
}
