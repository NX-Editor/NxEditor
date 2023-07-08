using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using NxEditor.ViewModels;

namespace NxEditor.Generators;

public class ShellDockFactory : Factory
{
    private static IDock Root => ((ShellViewModel.Shared.Layout?.ActiveDockable as IDock)?.ActiveDockable as IDock)!;

    public static void AddDoc<T>() where T : Document, new() => AddDoc(new T());
    public static T AddDoc<T>(T doc) where T : Document
    {
        (var dock, var index) = CheckDockable(Root, doc.Id);
        if (index >= 0) {
            doc = (dock.VisibleDockables![index] as T)!;
        }
        else {
            dock.VisibleDockables!.Add(doc);
        }

        dock.ActiveDockable = doc;
        return doc;
    }

    public static bool TryFocus(string id, out IDockable? target)
    {
        (var dock, var index) = CheckDockable(Root, id);
        if (index >= 0) {
            target = dock.VisibleDockables![index];
            dock.ActiveDockable = target;
            return true;
        }

        target = null;
        return false;
    }

    private static (DocumentDock dock, int index) CheckDockable(IDockable root, string id)
    {
        DocumentDock _default = null!;

        if (root is DocumentDock documentDock) {
            return (documentDock, documentDock.VisibleDockables?.Select((_, i) => i).First() ?? -1);
        }
        else if (root is ProportionalDock proportionalDock && proportionalDock.VisibleDockables != null) {
            foreach (var dockable in proportionalDock.VisibleDockables) {
                (_default, var index) = CheckDockable(dockable, id);
                if (index >= 0) {
                    return (_default, index);
                }
            }
        }

        return (_default, -1);
    }

    public static Document? Current() => Current(Root);
    private static Document? Current(IDockable? root)
    {
        if (root is DocumentDock documentDock) {
            return documentDock.ActiveDockable as Document;
        }
        else if (root is ProportionalDock proportionalDock && proportionalDock.ActiveDockable != null) {
            return Current(proportionalDock.ActiveDockable);
        }

        return null;
    }

    public override IRootDock CreateLayout()
    {
        DocumentDock documentdock = new() {
            Id = "Documents",
            VisibleDockables = CreateList<IDockable>(new HomeViewModel())
        };

        ProportionalDock dockLayout = new() {
            Id = "DocumentsDock",
            ActiveDockable = documentdock,
            VisibleDockables = CreateList<IDockable>(documentdock)
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
