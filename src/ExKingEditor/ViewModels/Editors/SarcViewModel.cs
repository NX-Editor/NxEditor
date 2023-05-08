using Avalonia;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Cead;
using Cead.Handles;
using CommunityToolkit.Mvvm.ComponentModel;
using ExKingEditor.Core;
using ExKingEditor.Helpers;
using ExKingEditor.Models;
using ExKingEditor.Views.Editors;
using System.Collections.ObjectModel;
using NodeMap = System.Collections.Generic.Dictionary<string, (ExKingEditor.Models.FileItemNode root, object map)>;

namespace ExKingEditor.ViewModels.Editors;



public partial class SarcViewModel : ReactiveEditor
{
    public enum Change
    {
        Add,
        Remove,
        Rename
    }

    private readonly Stack<(Change change, FileItemNode[] nodes)> _history = new();
    private readonly NodeMap _map = new();

    public SarcView? View { get; set; }

    [ObservableProperty]
    private FileItemNode _root = new("__root__");

    [ObservableProperty]
    private ObservableCollection<FileItemNode> _selected = new();

    public SarcViewModel(string file) : base(file)
    {
        using Sarc sarc = Sarc.FromBinary(RawData());

        foreach ((var name, var sarcFile) in sarc.OrderBy(x => x.Key)) {
            CreateNodeFromPath(name, sarcFile.AsSpan());
        }
    }

    public override async Task Cut()
    {
        await Copy();
        Remove();
        await base.Cut();
    }

    public override async Task Copy()
    {
        DataObject obj = new();

        List<IStorageItem?> payload = new();
        IStorageProvider storageProvider = App.VisualRoot!.StorageProvider;
        foreach (var node in Selected) {
            foreach (var file in node.GetFileNodes()) {
                string path = Path.Combine(_temp, file.GetPath(), file.Header);
                file.Export(path, isSingleFile: true);

                payload.Add(node.IsFile
                    ? await storageProvider.TryGetFileFromPathAsync(path)
                    : await storageProvider.TryGetFolderFromPathAsync(Path.Combine(_temp, node.GetPath(), node.Header))
                );
            }
        }

        obj.Set("Files", payload.DistinctBy(x => x?.Path));

        await Application.Current!.Clipboard!.SetDataObjectAsync(obj);
        await base.Copy();
    }

    public override async Task Paste()
    {
        if (await Application.Current!.Clipboard!.GetDataAsync("Files") is IEnumerable<IStorageItem> files) {
            foreach (var path in files.Select(x => x.Path.LocalPath)) {
                if (File.Exists(path)) {
                    ImportFile(path, File.ReadAllBytes(path));
                }
                else if (Directory.Exists(path)) {
                    ImportFolder(path);
                }
            }
        }

        await base.Paste();
    }

    public override void SelectAll()
    {
        View?.DropClient.SelectAll();
        base.SelectAll();
    }

    public void ImportFile(string path, ReadOnlySpan<byte> data, bool isRelPath = false)
    {
        if (CreateNodeFromPath(isRelPath ? path : Path.GetFileName(path), data, expandParentTree: true) is FileItemNode node) {
            Selected.Add(node);
        }
    }

    public void ImportFolder(string path, bool importTopLevel = false)
    {
        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {
            ImportFile(Path.GetRelativePath(importTopLevel ? Path.GetDirectoryName(path)! : path, file),
                File.ReadAllBytes(file), isRelPath: true);
        }
    }

    public void Rename()
    {
        if (Selected.FirstOrDefault() is FileItemNode node) {
            if (node.IsRenaming && node.PrevName != null) {
                NodeMap map = RemoveNodeFromMap(node, node.PrevName);
                map[node.Header] = (node, new NodeMap());
                node.PrevName = null;
            }
            else {
                node.PrevName = node.Header;
                node.IsRenaming = true;
            }
        }
    }

    public async Task Export()
    {
        if (Selected.Count <= 0) {
            return;
        }

        if (Selected.Count == 1 && Selected[0].IsFile) {
            BrowserDialog dialog = new(BrowserMode.SaveFile, "Save File", "Any File:*.*", Path.GetFileName(Selected[0].Header), "export-sarc-file");
            if (await dialog.ShowDialog() is string path) {
                Selected[0].Export(path, isSingleFile: true);
            }
        }
        else {
            BrowserDialog dialog = new(BrowserMode.OpenFolder, "Save to Folder", "Any File:*.*", instanceBrowserKey: "export-sarc-folder");
            if (await dialog.ShowDialog() is string path) {
                foreach (var node in Selected) {
                    node.Export(path, relativeTo: node.Parent);
                }
            }
        }
    }

    public async Task Replace()
    {
        if (Selected.FirstOrDefault() is FileItemNode node && node.IsFile) {
            BrowserDialog dialog = new(BrowserMode.OpenFile, "Replace File", "Any File:*.*", instanceBrowserKey: "replace-sarc-file");
            if (await dialog.ShowDialog() is string path && File.Exists(path)) {
                node.SetData(File.ReadAllBytes(path));
            }
        }
    }

    public void Remove()
    {
        if (Selected.Any()) {
            _history.Push((Change.Remove, Selected.ToArray()));
        }

        foreach (var item in Selected) {
            (item.Parent ?? Root).Children.Remove(item);
            RemoveNodeFromMap(item);
        }
    }

    private FileItemNode? CreateNodeFromPath(string path, ReadOnlySpan<byte> data, bool expandParentTree = false)
    {
        NodeMap map = _map;
        FileItemNode item = _root;

        foreach (var part in path.Replace('\\', '/').Split('/')) {
            if (!map.TryGetValue(part, out var node)) {
                map[part] = node = (new(part, item), new NodeMap());
                item.Children.Add(node.root);
            }

            item = node.root;
            map = (NodeMap)node.map;

            if (expandParentTree) {
                item.IsExpanded = true;
                Selected.Add(item);
            }
        }

        item?.SetData(data);
        return item;
    }

    private NodeMap RemoveNodeFromMap(FileItemNode node, string? key = null)
    {
        NodeMap map = _map;
        foreach (var part in node.GetPathParts()) {
            map = (NodeMap)map[part].map;
        }

        key ??= node.Header;
        map.Remove(key);
        return map;
    }

    public override bool HasChanged()
    {
        return _history.Count > 0;
    }

    public override void SaveAs(string path)
    {
        using Sarc sarc = new();
        foreach (var file in Root.GetFileNodes()) {
            sarc.Add(Path.Combine(file.GetPath(), file.Header).Replace(Path.DirectorySeparatorChar, '/'), file.GetData());
        }

        using DataHandle handle = sarc.ToBinary();

        Span<byte> data = (_compressed = path.EndsWith(".zs")) ? TotkZstd.Compress(path, handle) : handle;

        if (path == _file) {
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.SetLength(data.Length);
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
