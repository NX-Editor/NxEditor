using Avalonia;
using Avalonia.Threading;
using NxEditor.Core.Components;
using NxEditor.PluginBase.Models;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace NxEditor;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Forward to running process
        if (Config.Shared.UseSingleInstance && !SingleInstanceMgr.Start(args, Attach)) {
            return;
        }

        Logger.Initialize();
        PluginManager.Load();

        // Check if the args match
        // a shell process command
        if (args.Length <= 0 || args.All(File.Exists)) {
            goto GUI;
        }

        // Validate the modules, if it
        // fails open the user interface
        if (!PluginManager.ValidateModules()) {
            goto GUI;
        }

        PluginManager.RegisterExtensions();
        Task.Run(() => {
            throw new NotImplementedException();
        }).ConfigureAwait(false).GetAwaiter().GetResult();

        return;

    GUI:
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static async Task Attach(string[] args)
    {
        await Dispatcher.UIThread.InvokeAsync(async () => {
            ShellViewModel.Shared.View.ActivateWindow();
            await EditorManager.Shared.TryLoadEditor(EditorFile.FromFile(args[0]));
        });
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current.Register(new FontAwesomeIconProvider());

        return AppBuilder.Configure<App>()
            .WithInterFont()
            .UsePlatformDetect();
    }
}
