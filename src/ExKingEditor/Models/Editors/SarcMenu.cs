using ExKingEditor.Attributes;
using ExKingEditor.Helpers;
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

    [Menu("Import File", "Sarc", "Ctrl + Shift + A", "fa-solid fa-file-circle-plus")]
    public async Task ImportFile()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Open File", "Any File:*.*", instanceBrowserKey: "import-sarc-file");
        if (await dialog.ShowDialog() is string path && File.Exists(path)) {
            Sarc.ImportFile(path, File.ReadAllBytes(path));
        }
    }

    [Menu("Import Folder", "Sarc", "Ctrl + Shift + F", "fa-solid fa-folder-plus")]
    public async Task ImportFolder()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Open File", "Any File:*.*", instanceBrowserKey: "import-sarc-folder");
        if (await dialog.ShowDialog() is string path) {
            Sarc.ImportFolder(path);
        }
    }

    [Menu("Export All", "Sarc", "Ctrl + Shift + E", "fa-solid fa-arrow-right-from-bracket", IsSeparator = true)]
    public async Task ExportAll()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Open Folder", instanceBrowserKey: "export-all-sarc-folder");
        if (await dialog.ShowDialog() is string path) {
            Sarc.Root.Export(path);
            App.Toast("Files exported sucessfully!", type: Avalonia.Controls.Notifications.NotificationType.Success);
        }
    }
}
