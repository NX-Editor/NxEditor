global using static NxEditor.Core.Localization.StringResources;

using Avalonia;
using NxEditor.Components;
using NxEditor.Core;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace NxEditor.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        NXE.Logger.LogToConsole();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
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
