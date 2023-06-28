using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using NxEditor.Generators;

namespace NxEditor.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    public static ShellViewModel Shared { get; } = new();

    //
    // Layout

    private readonly ShellDockFactory _factory = new();

    [ObservableProperty]
    private IRootDock? _layout;

    public static void InitDock()
    {
        Shared.Layout = Shared._factory.CreateLayout();
        Shared._factory.InitLayout(Shared.Layout);
    }
}
