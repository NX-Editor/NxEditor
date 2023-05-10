using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ExKingEditor.Generators;
using ExKingEditor.Models.Editors;
using ExKingEditor.ViewModels.Editors;

namespace ExKingEditor.Views.Editors;
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

    public void RenameKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is TextBox tb && (e.Key == Key.Enter || e.Key == Key.Escape)) {
            (tb.Parent as Grid)?.Focus();
        }
    }

    public void RenameDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is TextBlock tb) {
            tb.IsVisible = false;
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
                    if (File.Exists(path)) {
                        vm.ImportFile(path, File.ReadAllBytes(path));
                    }
                    else {
                        vm.ImportFolder(path, importTopLevel: true);
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
