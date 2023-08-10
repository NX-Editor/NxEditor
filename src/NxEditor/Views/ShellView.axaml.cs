using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using NxEditor.PluginBase.Models;

namespace NxEditor.Views;
public partial class ShellView : Window
{
    public ShellView()
    {
        InitializeComponent();

        Minimize.Click += (s, e) => WindowState = WindowState.Minimized;
        Fullscreen.Click += (s, e) => WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        Quit.Click += (s, e) => {
            Close();
        };

        ChromeClient.PointerPressed += (s, e) => {
            if (!IsInResizeBorder(e, out _)) {
                BeginMoveDrag(e);
            }
        };

        ResizeClient.PointerPressed += (s, e) => {
            if (IsInResizeBorder(e, out WindowEdge edge)) {
                BeginResizeDrag(edge, e);
            }
        };

        PointerClient.PointerMoved += (s, e) => {
            if (IsInResizeBorder(e, out WindowEdge edge) && e.Source is Border border && border.Name is "ResizeClient" or "ChromeClient" or "ChromeStack") {
                ChromeStack.IsHitTestVisible = false;
                Cursor = new(edge switch {
                    WindowEdge.NorthWest => StandardCursorType.TopLeftCorner,
                    WindowEdge.NorthEast => StandardCursorType.TopRightCorner,
                    WindowEdge.SouthWest => StandardCursorType.BottomLeftCorner,
                    WindowEdge.SouthEast => StandardCursorType.BottomRightCorner,
                    WindowEdge.North or WindowEdge.South => StandardCursorType.SizeNorthSouth,
                    WindowEdge.West or WindowEdge.East => StandardCursorType.SizeWestEast,
                    _ => StandardCursorType.Arrow,
                });
            }
            else {
                ChromeStack.IsHitTestVisible = true;
                Cursor = new(StandardCursorType.Arrow);
            }
        };

        DropClient.AddHandler(DragDrop.DragEnterEvent, DragEnterEvent);
        DropClient.AddHandler(DragDrop.DragLeaveEvent, DragLeaveEvent);
        DropClient.AddHandler(DragDrop.DropEvent, DragDropEvent);
    }

    private bool IsInResizeBorder(PointerEventArgs e, out WindowEdge edge)
    {
        const int boxSize = 5;
        const int borderSize = 3;

        if (WindowState == WindowState.FullScreen || WindowState == WindowState.Maximized) {
            edge = 0;
            return false;
        }

        Point pos = e.GetPosition(this);
        if (pos.X <= boxSize && pos.Y <= boxSize) {
            edge = WindowEdge.NorthWest;
            return true;
        }
        else if (pos.X >= Bounds.Width - boxSize && pos.Y <= boxSize) {
            edge = WindowEdge.NorthEast;
            return true;
        }
        if (pos.X <= boxSize && pos.Y >= Bounds.Height - boxSize) {
            edge = WindowEdge.SouthWest;
            return true;
        }
        else if (pos.X >= Bounds.Width - boxSize && pos.Y >= Bounds.Height - boxSize) {
            edge = WindowEdge.SouthEast;
            return true;
        }
        else if (pos.Y <= borderSize) {
            edge = WindowEdge.North;
            return true;
        }
        else if (pos.Y >= Bounds.Height - borderSize) {
            edge = WindowEdge.South;
            return true;
        }
        else if (pos.X <= borderSize) {
            edge = WindowEdge.West;
            return true;
        }
        else if (pos.X >= Bounds.Width - borderSize) {
            edge = WindowEdge.East;
            return true;
        }

        edge = 0;
        return false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == WindowStateProperty && change.NewValue is WindowState state) {
            if (state == WindowState.Normal) {
                ICON_Fullscreen.IsVisible = !(ICON_Restore.IsVisible = false);
            }
            else {
                ICON_Fullscreen.IsVisible = !(ICON_Restore.IsVisible = true);
            }
        }

        base.OnPropertyChanged(change);
    }

    public void DragDropEvent(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
            foreach (var path in paths.Select(x => x.Path.LocalPath)) {
                _ = EditorManager.Shared.TryLoadEditor(new FileHandle(path));
            }
        }

        DragFadeMask.IsVisible = false;
    }

    public void DragEnterEvent(object? sender, DragEventArgs e)
    {
        DragFadeMaskInfo.Children.Clear();
        DragFadeMask.IsVisible = true;

        if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
            foreach (var path in paths.Select(x => x.Path.LocalPath)) {
                DragFadeMaskInfo.Children.Add(new TextBlock {
                    Text = Path.GetFileName(path),
                    Foreground = Brushes.Orange,
                });
            }
        }
    }

    public void DragLeaveEvent(object? sender, DragEventArgs e)
    {
        DragFadeMask.IsVisible = false;
    }
}
