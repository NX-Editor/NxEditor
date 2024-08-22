using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using NxEditor.Components;
using NxEditor.ViewModels;
using NxEditor.Views;
using System.Reflection;

namespace NxEditor;

public partial class App : Application
{
    private ApplicationNotificationManager? _notificationManager;

    public static readonly string Title = SystemMsg.Title;
    public static readonly string Version = typeof(App).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion.Split('+')[0] ?? SystemMsg.VersionNotFound;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) {
#if RELEASE
            throw Exceptions.TargetPlatformNotSupported;
#else
            return;
#endif
        }

        BindingPlugins.DataValidators.RemoveAt(0);
        NXE.Config.AppThemeChanged += SetRequestedThemeVariant;

        desktop.MainWindow = new ShellView();
        ApplicationDockFactory factory = new(desktop.MainWindow);
        desktop.MainWindow.DataContext = new ShellViewModel(factory);

        // Register Notification Manager
        desktop.MainWindow.Loaded += (s, e) => {
            WindowNotificationManager windowNotificationManager = new((ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow) {
                Position = NotificationPosition.BottomRight,
                MaxItems = 3,
                Margin = new(0, 0, 4, 0)
            };

            _notificationManager = new(windowNotificationManager);
            NXE.Logger.EventOccured += _notificationManager.ShowLogEventNotification;
            NXE.Logger.ExceptionOccured += _notificationManager.ShowLogEventNotification;
        };

        NXE.RegisterFrontend(new NxeFrontend(
            desktop.MainWindow
        ));
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
