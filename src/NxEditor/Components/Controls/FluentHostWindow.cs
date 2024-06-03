using Dock.Model.Core;
using FluentAvalonia.UI.Windowing;

namespace NxEditor.Components.Controls;

internal class FluentHostWindow : AppWindow, IHostWindow
{
    public IDockManager? DockManager { get; }
    public IHostWindowState? HostWindowState { get; }
    public bool IsTracked { get; set; }
    public IDockWindow? Window { get; set; }

    public void Exit()
    {
        if (Window is not null && Window.OnClose() == false) {
            return;
        }

        Close();
    }

    public void Present(bool isDialog)
    {
        if (IsVisible) {
            return;
        }

        Window?.Factory?.OnWindowOpened(Window);

        if (isDialog) {
            ShowDialog(null);
            return;
        }

        Show(null);
    }

    public void SetLayout(IDock layout)
    {
        DataContext = layout;
    }

    public void SetPosition(double x, double y)
    {
        Position = new((int)x, (int)y);
    }

    public void GetPosition(out double x, out double y)
    {
        x = Position.X;
        y = Position.Y;
    }

    public void SetSize(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public void GetSize(out double width, out double height)
    {
        width = Width;
        height = Height;
    }

    public void SetTitle(string title)
    {
        Title = title;
    }
}
