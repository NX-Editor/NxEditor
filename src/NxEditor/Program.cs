global using static NxEditor.Core.Localization.StringResources;

using Avalonia;
using NxEditor.Components;
using NxEditor.Core.Components.Loggers;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace NxEditor;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        using SystemFileLogger systemFileLogger = new();
        NXE.Logger
            .Register<SystemConsoleLogger>()
            .Register(systemFileLogger);

        try {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex) {
            NXE.Logger.LogException(ex);
        }
        finally {
            systemFileLogger.Flush();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register(new FontAwesomeIconProvider(FontAwesomeJsonStreamProvider.Instance));

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont();
    }
}
