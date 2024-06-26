using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Models;
using NxEditor.Core.Components;
using NxEditor.Generators;
using NxEditor.Models;
using NxEditor.Models.Footers;
using NxEditor.Models.Menus;
using NxEditor.PluginBase;
using NxEditor.PluginBase.Common;
using NxEditor.PluginBase.Models;
using System.Runtime.CompilerServices;

namespace NxEditor;

public partial class App : Application
{
    private static WindowNotificationManager? _notificationManager;

    public static string Title { get; } = "NX-Editor";
    public static string? Version { get; } = typeof(App).Assembly.GetName().Version?.ToString(3);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        Logger.SetTraceListener(LogsViewModel.Shared.TraceListener);
        Frontend.Register<IEditorManager>(EditorManager.Shared);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            BindingPlugins.DataValidators.RemoveAt(0);
            ShellViewModel.Shared.InitDock();
            Config.SetTheme(Config.Shared.Theme);

            desktop.MainWindow = ShellViewModel.Shared.View;
            DialogBox.SetViewRoot(ShellViewModel.Shared.View.DropClient);

            TopLevel? visualRoot = desktop.MainWindow.GetVisualRoot() as TopLevel;
            MenuFactory menu = new(visualRoot);
            FooterFactory footer = new();

            ShellViewModel.Shared.View.RootMenu.ItemsSource = menu.Items;
            menu.Append(new ShellViewMenu());

            ShellViewModel.Shared.FooterItems = footer.Items;
            footer.Append(new ShellViewFooter());

            RecentFiles.Shared.Load();

            Frontend.Register<IMenuFactory>(menu);
            Frontend.Register<IFooterFactory>(footer);
            Frontend.Register(visualRoot?.Clipboard!);
            Frontend.Register(
                BrowserDialog.StorageProvider = visualRoot?.StorageProvider!
            );

            desktop.MainWindow.Loaded += (s, e) => {
                _notificationManager = new(visualRoot) {
                    Position = NotificationPosition.BottomRight,
                    MaxItems = 3,
                    Margin = new(0, 0, 4, 0)
                };

#if RELEASE
                if (OperatingSystem.IsWindows()) {
                    Core.Extensions.ConsoleExtension.SetWindowMode(
                        Core.Extensions.WindowMode.Hidden);
                }
#endif
            };

            ConfigViewModel.Shared.IsValid = PluginManager.RegisterModules(ConfigViewModel.Shared);
            Frontend.Register<ConfigPageModel>(ConfigViewModel.Shared);

            if (!ConfigViewModel.Shared.IsValid) {
                ShellDockFactory.AddDoc(ConfigViewModel.Shared);

                await ConfigViewModel.Shared.PrimaryRelay();
                await Task.Run(() => {
                    while (!ConfigViewModel.Shared.IsValid) { }
                });
            }

            PluginManager.RegisterExtensions();

            if (desktop.Args != null && desktop.Args.Length > 0) {
                foreach (string? arg in desktop.Args.Where(File.Exists)) {
                    await EditorManager.Shared.TryLoadEditor(EditorFile.FromFile(arg));
                }
            }

#if RELEASE
            _ = Task.Run(async () => {
                if (await Core.Helpers.UpdateHelper.HasUpdate() || await Core.Helpers.UpdateHelper.GetPluginUpdates() > 0) {
                    Toast("Updates available!", action: OpenLauncherAndExit);
                }
            });
#endif
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void Toast(string message, string title = "Notice", NotificationType type = NotificationType.Information, TimeSpan? expiration = null, Action? action = null)
    {
        Dispatcher.UIThread.Invoke(() => {
            _notificationManager?.Show(
                new Notification(title, message, type, expiration, action));
        });
    }

    public static void ToastError(Exception ex)
    {
        Dispatcher.UIThread.Invoke(() => {
            _notificationManager?.Show(new Notification(
                ex.GetType().Name, ex.Message, NotificationType.Error, onClick: ShellViewMenu.OpenLogs));
        });
    }

    public static void Log(object obj, [CallerMemberName] string method = "")
    {
        if (obj is Exception ex) {
            ToastError(ex);
        }

        Logger.Write(obj, method);
    }

#if RELEASE
    private static async void OpenLauncherAndExit()
    {
        DialogResult result = await DialogBox.ShowAsync("Install Updates", """
                            Your current session will be lost.

                            Are you sure you would you like to open
                            the launcher and close NX-Editor?
                            """, primaryButtonContent: "Yes");

        if (result == DialogResult.Primary) {
            ShellViewModel.Shared.View.IsEnabled = false;
            StatusModal.Set("Downloading launcher", "fa-solid fa-download", isWorkingStatus: true);
            await Core.Helpers.UpdateHelper.DownloadLauncher();
            System.Diagnostics.Process.Start(Core.Helpers.UpdateHelper.LauncherPath);
            Environment.Exit(0);
        }
    }
#endif
}
