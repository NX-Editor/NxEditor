using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ExKingEditor.Generators;
using ExKingEditor.Models;

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
        if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
            foreach (var path in paths.Select(x => x.Path.LocalPath)) {
                if (File.Exists(path) && EditorMgr.CanEdit(path)) {
                    ShellDockFactory.AddDoc(EditorMgr.GetEditor(path));
                }
            }
        }
    }
}
