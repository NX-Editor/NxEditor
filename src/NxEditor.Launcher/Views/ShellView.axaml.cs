using Avalonia.Controls;

namespace NxEditor.Launcher.Views;

public partial class ShellView : Window
{
    public ShellView()
    {
        InitializeComponent();
        Client.PointerPressed += (s, e) => BeginMoveDrag(e);
    }
}
