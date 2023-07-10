using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using NxEditor.Components;
using NxEditor.Generators;
using NxEditor.Views;

namespace NxEditor.ViewModels;

public partial class ShellViewModel : StaticPage<ShellViewModel, ShellView>
{
    //
    // Layout

    private readonly ShellDockFactory _factory = new();

    [ObservableProperty]
    private IRootDock? _layout;

    public void InitDock()
    {
        Layout = _factory.CreateLayout();
        _factory.InitLayout(Layout);
    }
}
