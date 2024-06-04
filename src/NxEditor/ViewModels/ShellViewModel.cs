using Dock.Model.Controls;
using Dock.Model.Mvvm.Controls;
using NxEditor.Components;

namespace NxEditor.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly ApplicationDockFactory _factory;
    private readonly IRootDock _layout;

    public IRootDock Layout => _layout;

    public ShellViewModel(ApplicationDockFactory factory)
    {
        _factory = factory;
        _layout = factory.CreateLayout();
        factory.InitLayout(_layout);

        factory.AddDockable(factory.Documents, new HomeViewModel());
    }
}
