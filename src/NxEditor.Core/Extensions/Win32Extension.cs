using Avalonia.Platform;
using System.Runtime.InteropServices;

namespace NxEditor.Core.Extensions;

public partial class Win32Extension
{
    [Flags]
    private enum SetWindowPosFlags : uint
    {
        HideWindow = 128,
        ShowWindow = 64
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    public static void FixAfterMaximizing(IntPtr hWnd, Screen screen)
    {
        SetWindowPos(hWnd, IntPtr.Zero, screen.WorkingArea.X, screen.WorkingArea.Y, screen.WorkingArea.Width, screen.WorkingArea.Height, (uint)SetWindowPosFlags.ShowWindow);
    }
}
