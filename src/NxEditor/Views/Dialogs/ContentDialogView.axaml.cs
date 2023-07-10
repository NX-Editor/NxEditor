using Avalonia.Controls;
using NxEditor.ViewModels;
using NxEditor.ViewModels.Dialogs;

namespace NxEditor.Views.Dialogs;

public partial class ContentDialogView : UserControl
{
    public ContentDialogView() { }
    public ContentDialogView(ContentDialog content)
    {
        InitializeComponent();
        DataContext = content;
    }

    public void Show()
    {
        ShellViewModel.Shared.View?.DropClient.Children.Add(this);
        PrimaryButton.Focus();
    }

    public void Hide()
    {
        ShellViewModel.Shared.View?.DropClient.Children.Remove(this);
    }
}
