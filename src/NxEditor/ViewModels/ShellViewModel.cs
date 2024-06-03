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

        factory.AddDockable(factory.Documents, new Document {
            Title = "Test A",
            Id = "Test_A"
        });

        factory.AddDockable(factory.Documents, new Document {
            Title = "Test B",
            Id = "Test_B"
        });

        factory.AddDockable(factory.Tools, new Tool {
            Title = "Test C",
            Id = "Test_C"
        });

        factory.AddDockable(factory.Tools, new Tool {
            Title = "Test D",
            Id = "Test_D"
        });
    }
}
