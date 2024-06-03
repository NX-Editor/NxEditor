global using static NxEditor.Core.Localization.StringResources;

using Avalonia;
using NxEditor.Components;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace NxEditor.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register(new FontAwesomeIconProvider(FontAwesomeJsonStreamProvider.Instance));

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
