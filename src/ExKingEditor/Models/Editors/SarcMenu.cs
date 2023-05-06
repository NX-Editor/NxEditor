using ExKingEditor.Attributes;
using ExKingEditor.ViewModels.Editors;
using ExKingEditor.Views.Editors;

namespace ExKingEditor.Models.Editors;

public class SarcMenu
{
    public readonly SarcView _view;

    public SarcViewModel Sarc => _view.DataContext as SarcViewModel ??
        throw new Exception("The SarcView did not have a valid data context");

    public SarcMenu(SarcView view)
    {
        _view = view;
    }

    [Menu("Add File", "Sarc", "Ctrl + Shift + A", "fa-solid fa-file-circle-plus")]
    public void AddFile()
    {
        App.Log("Add File");
    }

    [Menu("Add Folder", "Sarc", "Ctrl + Shift + F", "fa-solid fa-folder-plus")]
    public void AddFolder()
    {
        App.Log("Add Folder");
    }
}
