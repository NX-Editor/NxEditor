using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using ExKingEditor.ViewModels;

namespace ExKingEditor.Generators;

public class ShellDockFactory : Factory
{
    public override IRootDock CreateLayout()
    {
        DocumentDock dockLayout = new() {
            Id = "Documents",
            Title = "Documents",
            VisibleDockables = CreateList<IDockable>(new HomeViewModel())
        };

        RootDock rootDock = new() {
            Id = "RootDock",
            Title = "RootDock",
            ActiveDockable = dockLayout,
            VisibleDockables = CreateList<IDockable>(dockLayout)
        };

        IRootDock root = CreateRootDock();
        root.Id = "DockLayout";
        root.Title = "DockLayout";
        root.ActiveDockable = rootDock;
        root.DefaultDockable = rootDock;
        root.VisibleDockables = CreateList<IDockable>(rootDock);

        return root;
    }

    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>> {
            [nameof(IDockWindow)] = () => new HostWindow() {
                MinWidth = 770,
                MinHeight = 430,
                ShowInTaskbar = true,
                Icon = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow?.Icon
            }
        };

        base.InitLayout(layout);
    }
}
