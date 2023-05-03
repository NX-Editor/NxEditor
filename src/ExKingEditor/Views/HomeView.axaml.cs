using Avalonia.Controls;
using Avalonia.Input;

namespace ExKingEditor.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        DragRegion.AddHandler(DragDrop.DropEvent, DragDropEvent);
    }

    public void DragDropEvent(object? sender, DragEventArgs e)
    {
        IEnumerable<string>? folders = e.Data.GetFileNames();

        if (folders != null) {
            foreach (var folder in folders) {
                // check folder and open mod if validated
            }
        }
    }
}
