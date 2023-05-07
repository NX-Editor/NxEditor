using AvaloniaEdit.Editing;
using Cead;
using Cead.Handles;
using CommunityToolkit.Mvvm.ComponentModel;
using ExKingEditor.Core;
using ExKingEditor.Helpers;
using ExKingEditor.Models;
using System.Collections.ObjectModel;
using NodeMap = System.Collections.Generic.Dictionary<string, (ExKingEditor.Models.TreeItemNode root, object map)>;

namespace ExKingEditor.ViewModels.Editors;



public partial class SarcViewModel : ReactiveEditor
{
    public enum Change
    {
        Add,
        Remove,
        Rename
    }

    private readonly Stack<(Change change, TreeItemNode node)> _history = new();
    private readonly NodeMap _map = new();

    [ObservableProperty]
    private TreeItemNode _root = new("__root__");

    [ObservableProperty]
    private ObservableCollection<TreeItemNode> _selected = new();

    public SarcViewModel(string file) : base(file)
    {
        using Sarc sarc = Sarc.FromBinary(RawData());

        foreach ((var name, var sarcFile) in sarc.OrderBy(x => x.Key)) {
            AppendPathToView(name, sarcFile.AsSpan());
        }
    }

    public void ImportFile(string path, ReadOnlySpan<byte> data)
    {
        string name = Path.GetFileName(path);
        AppendPathToView(name, data);
    }

    public void ImportFolder(string path)
    {
        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {
            ImportFile(Path.GetRelativePath(path, file), File.ReadAllBytes(file));
        }
    }

    public void Rename()
    {
        if (Selected.FirstOrDefault() is TreeItemNode node) {
            node.IsRenaming = true;
        }
    }

    public async Task Export()
    {
        if (Selected.Count <= 0) {
            return;
        }

        if (Selected[0].IsFile) {
            BrowserDialog dialog = new(BrowserMode.SaveFile, "Save File", "Any File:*.*", Path.GetFileName(Selected[0].Header), "export-sarc-file");
            if (await dialog.ShowDialog() is string path) {
                using FileStream fs = File.Create(path);
                fs.Write(Selected[0].GetData());
            }
        }
        else {
            BrowserDialog dialog = new(BrowserMode.OpenFolder, "Save to Folder", "Any File:*.*", instanceBrowserKey: "export-sarc-folder");
            if (await dialog.ShowDialog() is string path) {
                foreach (var node in Selected) {
                    var nodes = node.GetFileNodes(recursive: true);
                    foreach (var child in nodes) {
                        string output = Path.Combine(path, child.GetPath(relativeTo: node));
                        Directory.CreateDirectory(output);
                        using FileStream fs = File.Create(Path.Combine(output, child.Header));
                        fs.Write(child.GetData());
                    }
                }
            }
        }
    }

    public async Task Replace()
    {
        if (Selected.FirstOrDefault() is TreeItemNode node && node.IsFile) {
            BrowserDialog dialog = new(BrowserMode.OpenFile, "Replace File", "Any File:*.*", instanceBrowserKey: "replace-sarc-file");
            if (await dialog.ShowDialog() is string path && File.Exists(path)) {
                node.SetData(File.ReadAllBytes(path));
            }
        }
    }

    public void Remove()
    {
        foreach (var item in Selected) {
            (item.Parent ?? Root).Children.Remove(item);
            _history.Push((Change.Remove, item));
        }
    }

    private void AppendPathToView(string path, ReadOnlySpan<byte> data)
    {
        NodeMap map = _map;
        TreeItemNode item = _root;

        foreach (var part in path.Replace('\\', '/').Split('/')) {
            if (!map.TryGetValue(part, out var node)) {
                map[part] = node = (new(part, item), new NodeMap());
                item.Children.Add(node.root);
            }

            item = node.root;
            map = (NodeMap)node.map;
        }

        item?.SetData(data);
    }

    public override bool HasChanged()
    {
        return _history.Count > 0;
    }

    public override void SaveAs(string path)
    {
        using Sarc sarc = new();
        foreach (var file in Root.GetFileNodes()) {
            sarc.Add(file.GetPath(relativeTo: Root), file.GetData());
        }

        using DataHandle handle = sarc.ToBinary();

        Span<byte> data = (_compressed = path.EndsWith(".zs")) ? TotkZstd.Compress(path, handle) : handle;

        if (path == _file) {
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.Write(data);
        }
        else {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using FileStream fs = File.Create(path);
            fs.Write(data);
        }

        ToastSaveSuccess(path);
    }
}
