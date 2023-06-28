using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using NxEditor.Component;
using NxEditor.Core;
using NxEditor.PluginBase.Models;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace NxEditor;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static unsafe void Main(string[] args)
    {
        if (Config.Shared.UseSingleInstance && !SingleInstanceMgr.Start(args, Attach)) {
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static void Attach(string[] args)
    {
        Dispatcher.UIThread.InvokeAsync(() => {
            App.Desktop?.Activate();
            EditorMgr.TryLoadEditorSafe(new FileHandle(args[0]));
        });
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .WithIcons(x => x.Register(new FontAwesomeIconProvider()));
}
