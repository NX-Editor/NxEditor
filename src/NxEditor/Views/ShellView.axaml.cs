using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using NxEditor.Component;
using NxEditor.Generators;
using NxEditor.Models.Menus;
using System.Collections.ObjectModel;

namespace NxEditor.Views;
public partial class ShellView : Window
{
    private List<KeyBinding> _keyBindings = new();

    public Dictionary<KeyGesture, string> KeyBindingHeaders { get; } = new();
    public static ObservableCollection<Control>? MainMenu { get; private set; }
    public static int MenuOverflow { get; set; } = 0;

    public ShellView()
    {
        InitializeComponent();
        MenuFactory.Init(this);
        // TODO: this is kinda stupid, refactor the MenuFactor to avoid this weird dependency injection method
        MenuFactory.ItemsSource = MainMenu = MenuFactory.Generate<ShellViewMenu>();

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

        RootMenu.ItemsSource = MainMenu;

        DropClient.AddHandler(DragDrop.DragEnterEvent, DragEnterEvent);
        DropClient.AddHandler(DragDrop.DragLeaveEvent, DragLeaveEvent);
        DropClient.AddHandler(DragDrop.DropEvent, DragDropEvent);
    }

    public void ActivateGlobalShortcuts()
    {
        if (_keyBindings != null) {
            foreach (var binding in _keyBindings) {
                KeyBindings.Add(binding);
            }
        }

        _keyBindings = new();
    }

    public void DisableGlobalShortcuts() => DisableGlobalShortcuts(Array.Empty<string>());
    public void DisableGlobalShortcuts(params string[] targets)
    {
        if (targets.Length <= 0) {
            _keyBindings = KeyBindings.ToList();
            KeyBindings.Clear();
            return;
        }

        for (int i = 0; i < KeyBindings.Count; i++) {
            KeyBinding key = KeyBindings[i];
            foreach (var target in targets) {
                if (KeyBindingHeaders[key.Gesture].StartsWith(target)) {
                    _keyBindings.Add(key);
                    KeyBindings.RemoveAt(i);
                }
            }
        }
    }

    public static void ClearOverflowMenu()
    {
        for (int i = 0; i < MenuOverflow; i++) {
            MainMenu?.RemoveAt(MainMenu.Count - 1);
        }

        MenuOverflow = 0;
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

    protected override void OnLoaded()
    {
        base.OnLoaded();
        App.NotificationManager = new(GetTopLevel(this)) {
            Position = NotificationPosition.BottomRight,
            MaxItems = 3,
            Margin = new(0, 0, 4, 0)
        };
    }

    public void DragDropEvent(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
            foreach (var path in paths.Select(x => x.Path.LocalPath)) {
                if (!EditorMgr.TryLoadEditorSafe(path)) {
                    // TODO: throw message dialog
                }
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
                bool canEdit = EditorMgr.CanEdit(path, out string? editor);
                DragFadeMaskInfo.Children.Add(new TextBlock {
                    Text = $"{(canEdit ? editor?.Replace("ViewModel", "") : "Unknown")}: " +
                           Path.GetFileName(path),
                    Foreground = canEdit ? Brushes.LightGreen : Brushes.Orange,
                });
            }
        }
    }

    public void DragLeaveEvent(object? sender, DragEventArgs e)
    {
        DragFadeMask.IsVisible = false;
    }
}
