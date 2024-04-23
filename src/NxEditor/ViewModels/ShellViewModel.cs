using Avalonia.Controls;
using Dock.Model.Controls;
using NxEditor.Generators;
using System.Collections.ObjectModel;

namespace NxEditor.ViewModels;

public partial class ShellViewModel : StaticPage<ShellViewModel, ShellView>
{
    //
    // Layout

    private readonly ShellDockFactory _factory = new();

    [ObservableProperty]
    private IRootDock? _layout;

    [ObservableProperty]
    private ObservableCollection<Control> _footerItems = [];

    public void InitDock()
    {
        Layout = _factory.CreateLayout();
        _factory.InitLayout(Layout);
    }
}
