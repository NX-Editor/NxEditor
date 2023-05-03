using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ExKingEditor.Generators;
using ExKingEditor.Models;

namespace ExKingEditor.Views;
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
            if (IsInResizeBorder(e, out WindowEdge edge) && !RootMenu.IsOpen) {
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

        RootMenu.ItemsSource = MenuFactory.Generate<ShellMenu>();
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

    protected override void HandleWindowStateChanged(WindowState state)
    {
        if (state == WindowState.Normal) {
            ICON_Fullscreen.IsVisible = !(ICON_Restore.IsVisible = false);
        }
        else {
            ICON_Fullscreen.IsVisible = !(ICON_Restore.IsVisible = true);
        }

        base.HandleWindowStateChanged(state);
    }
}
