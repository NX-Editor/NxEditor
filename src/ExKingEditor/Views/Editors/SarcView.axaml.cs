using Avalonia;
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

    public void DragDropEvent(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
            foreach (var path in paths.Select(x => x.Path.LocalPath).Where(File.Exists)) {
                (DataContext as SarcViewModel)?.ImportFile(path, File.ReadAllBytes(path));
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
        (DataContext as SarcViewModel)!.View = this;
        base.OnDataContextChanged(e);
    }
}
