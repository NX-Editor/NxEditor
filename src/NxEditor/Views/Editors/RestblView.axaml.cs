using Avalonia.Controls;
using Avalonia.Input;
using NxEditor.ViewModels.Editors;

namespace NxEditor.Views.Editors;
public partial class RestblView : UserControl
{
    public RestblView()
    {
        InitializeComponent();
    }

    public void EditEvent(object sender, TappedEventArgs e)
    {
        if (DataContext is RestblViewModel vm) {
            vm.Current?.Edit(vm);
        }
    }
}
