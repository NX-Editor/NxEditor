using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using NxEditor.Generators;
using NxEditor.Models;
using NxEditor.Models.Editors;
using NxEditor.ViewModels.Editors;

namespace NxEditor.Views.Editors;
public partial class SarcView : UserControl
{
    private readonly Control _extension;

    public SarcView()
    {
        InitializeComponent();

        _extension = MenuFactory.Generate(new SarcMenu(this))[0];

        DetachedFromVisualTree += (s, e) => ShellView.ClearOverflowMenu();
        AttachedToVisualTree += (s, e) => {
            ShellView.MainMenu?.Add(new Separator {
                Width = 1,
                Height = 12,
                Margin = new(2, 0)
            });

            ShellView.MainMenu?.Add(_extension);
            ShellView.MenuOverflow = 2;
        };

        DropClient.AddHandler(DragDrop.DragEnterEvent, DragEnterEvent);
        DropClient.AddHandler(DragDrop.DragLeaveEvent, DragLeaveEvent);
        DropClient.AddHandler(DragDrop.DropEvent, DragDropEvent);
    }

    public async void TreeViewDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is SarcViewModel vm) {
            await vm.Edit();
        }
    }

    public void RenameClientAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is TextBox tb && tb.DataContext is FileItemNode node) {
            node.SetRenameClient(tb);
        }
    }

    public void RenameLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SarcViewModel vm && sender is TextBox tb && tb.DataContext is FileItemNode node) {
            node.EndRename(vm);
        }
    }

    public void RenameKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is TextBox tb && (e.Key == Key.Enter || e.Key == Key.Escape)) {
            (tb.Parent as Grid)?.Focus();
        }
    }

    public void RenameDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is TextBlock tb && tb.DataContext is FileItemNode node) {
            node.BeginRename();
        }

        e.Handled = true;
    }

    public void FieldKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is TextBox tb) {
            if (e.Key == Key.Escape) {
                tb.IsVisible = false;
            }
            else if (e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.Shift) {
                if (DataContext is SarcViewModel vm) {
                    vm.FindNext(clearSelection: true, findLast: true);
                }
            }
            else if (e.Key == Key.Enter) {
                if (DataContext is SarcViewModel vm) {
                    vm.FindNext(clearSelection: true);
                }
            }
            else if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.Control) {
                if (DataContext is SarcViewModel vm) {
                    vm.Find();
                }
            }
            else if (e.Key == Key.H && e.KeyModifiers == KeyModifiers.Control) {
                if (DataContext is SarcViewModel vm) {
                    vm.FindAndReplace();
                }
            }
            else if (e.Key == Key.Tab) {
                (tb.Name == nameof(FindField) ? ReplaceField : FindField).Focus();
            }
            else {
                return;
            }

            e.Handled = true;
        }
    }

    public void DragDropEvent(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
            foreach (var path in paths.Select(x => x.Path.LocalPath)) {
                if (DataContext is SarcViewModel vm) {
                    object? src = e.Source;
                    FileItemNode? node = null;
                    while (src is not TreeView && src is Control control) {
                        if (src is TreeViewItem target) {
                            node = target.DataContext as FileItemNode;
                            break;
                        }

                        src = control.Parent;
                    }

                    node = node?.IsFile == true ? node?.Parent : node;

                    if (File.Exists(path)) {
                        vm.ImportFile(path, File.ReadAllBytes(path), parentNode: node);
                    }
                    else {
                        vm.ImportFolder(path, importTopLevel: true, parentNode: node);
                    }
                }
            }
        }

        e.Handled = true;
    }

    public void DragEnterEvent(object? sender, DragEventArgs e)
    {
        DragDrop.SetAllowDrop(App.Desktop!.DropClient, false);
        e.Handled = true;
    }

    public void DragLeaveEvent(object? sender, DragEventArgs e)
    {
        DragDrop.SetAllowDrop(App.Desktop!.DropClient, true);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is SarcViewModel vm) {
            vm.View = this;
        }

        base.OnDataContextChanged(e);
    }
}
