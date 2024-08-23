using Avalonia.Controls;
using Avalonia.Input;
using NxEditor.ViewModels;

namespace NxEditor.Views;

public partial class HomeView : UserControl
{
    public HomeViewModel ViewModel => (HomeViewModel)DataContext!;

    public HomeView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DragEnterEvent, DragEnterEvent);
        AddHandler(DragDrop.DropEvent, DragDropEvent);
        AddHandler(DragDrop.DragLeaveEvent, DragLeaveEvent);
    }

    private void DragEnterEvent(object? sender, DragEventArgs e)
    {
        DragIndicatorMask.Opacity = 0.5;
        ViewModel.Icon = HomeViewModel.DROPPING_ICON;
    }

    private void DragDropEvent(object? sender, DragEventArgs e)
    {
        ResetDropIndicatorIcon();
        // Open File
        ReleaseDropIndicator();
    }

    private void DragLeaveEvent(object? sender, DragEventArgs e)
    {
        ReleaseDropIndicator();
    }

    private void ResetDropIndicatorIcon()
    {
        ViewModel.Icon = HomeViewModel.DEFAULT_ICON;
    }

    private void ReleaseDropIndicator()
    {
        DragIndicatorMask.Opacity = 0;
        ViewModel.Icon = HomeViewModel.DEFAULT_ICON;
    }
}
