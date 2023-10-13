using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using NxEditor.Core.Extensions;
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
            BeginMoveDrag(e);
        };

        DropClient.AddHandler(DragDrop.DragEnterEvent, DragEnterEvent);
        DropClient.AddHandler(DragDrop.DragLeaveEvent, DragLeaveEvent);
        DropClient.AddHandler(DragDrop.DropEvent, DragDropEvent);

        this.GetPropertyChangedObservable(WindowStateProperty).AddClassHandler<Visual>((t, args) => {
            if (args.GetNewValue<WindowState>() == WindowState.Maximized) {
                ICON_Fullscreen.IsVisible = !(ICON_Restore.IsVisible = true);
                if (OperatingSystem.IsWindows()) {
                    FixScreenSize();
                }
            }
            else {
                ICON_Fullscreen.IsVisible = !(ICON_Restore.IsVisible = false);
            }
        });
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

    private void FixScreenSize()
    {
        Screen? screen = Screens.ScreenFromWindow(this);
        if (screen != null) {
            if (screen.WorkingArea.Height < ClientSize.Height * screen.Scaling) {
                ClientSize = screen.WorkingArea.Size.ToSize(screen.Scaling);
                if (Position.X < 0 || Position.Y < 0) {
                    Position = screen.WorkingArea.Position;
                    if (TryGetPlatformHandle() is IPlatformHandle platform) {
                        Win32Extension.FixAfterMaximizing(platform.Handle, screen);
                    }
                }
            }
        }
    }
}
