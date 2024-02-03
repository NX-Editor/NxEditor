using Avalonia.Controls;

namespace NxEditor.Launcher.Views;

public partial class ShellView : Window
{
    public ShellView()
    {
        InitializeComponent();
        Client.PointerPressed += (s, e) => BeginMoveDrag(e);
    }

    private void Binding(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}
