using Dock.Model.Controls;
using NxEditor.Component;
using NxEditor.Generators;

namespace NxEditor.ViewModels;

public class ShellViewModel : ReactiveSingleton<ShellViewModel>
{
    //
    // Layout

    private readonly ShellDockFactory _factory = new();

    private IRootDock? _layout;
    public IRootDock? Layout {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public static void InitDock()
    {
        Shared.Layout = Shared._factory.CreateLayout();
        Shared._factory.InitLayout(Shared.Layout);
    }
}
