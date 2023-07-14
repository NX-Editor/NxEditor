using System.Runtime.InteropServices;

namespace NxEditor.Core.Extensions;

public enum WindowMode : int { Hidden = 0, Visible = 5 }

public static partial class ConsoleExtension
{
    private static readonly IntPtr _handle = GetConsoleWindow();
    private static WindowMode _current = WindowMode.Visible;

    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetConsoleWindow();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr window_handle, int cmd_show_mode);

    public static void SetWindowMode(WindowMode mode)
    {
        _current = mode;
        ShowWindow(_handle, (int)mode);
    }

    public static void SwapWindowMode()
    {
        _current = _current == WindowMode.Hidden
            ? WindowMode.Visible : WindowMode.Hidden;
        ShowWindow(_handle, (int)_current);
    }
}