using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Cead;
using Cead.Handles;
using CommunityToolkit.Mvvm.ComponentModel;
using ExKingEditor.Core;
using ExKingEditor.Helpers;
using ExKingEditor.Models;
using ExKingEditor.Models.Editors;
using ExKingEditor.Views.Editors;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using NodeMap = System.Collections.Generic.Dictionary<string, (ExKingEditor.Models.FileItemNode root, object map)>;

namespace ExKingEditor.ViewModels.Editors;

public partial class SarcViewModel : ReactiveEditor
{
    private readonly List<FileItemNode> _searchCache = new();
    private readonly NodeMap _map = new();

    public SarcHistoryStack History { get; } = new();

    public SarcView? View { get; set; }
    
    [ObservableProperty]
    private FileItemNode _root = new("__root__");

    [ObservableProperty]
    private ObservableCollection<FileItemNode> _selected = new();

    [ObservableProperty]
    private bool _isFinding;

    [ObservableProperty]
    private bool _isReplacing;

    [ObservableProperty]
    private bool _matchCase;

    [ObservableProperty]
    private string? _findField;

    [ObservableProperty]
    private string? _replaceField;

    [ObservableProperty]
    private int _searchCount;

    [ObservableProperty]
    private int _searchIndex = -1;

    public SarcViewModel(string file, byte[] data, Stream? fs = null, Action<byte[]>? setSource = null) : base(file, data, fs, setSource)
    {
        using Sarc sarc = Sarc.FromBinary(_data);

        foreach ((var name, var sarcFile) in sarc.OrderBy(x => x.Key)) {
            CreateNodeFromPath(name, sarcFile.ToArray());
        }
    }

    public override void SaveAs(string path)
    {
        using Sarc sarc = new();
        foreach (var file in Root.GetFileNodes()) {
            sarc.Add(Path.Combine(file.GetPath(), file.Header).Replace(Path.DirectorySeparatorChar, '/'), file.Data);
        }

        using DataHandle handle = sarc.ToBinary();

        Span<byte> data = (_compressed = path.EndsWith(".zs")) ? TotkZstd.Compress(path, handle) : handle;

        if (path == _file) {
            WriteToSource(data.ToArray());
        }
        else {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using FileStream fs = File.Create(path);
            fs.Write(data);
        }

        ToastSaveSuccess(path);
    }

    public override bool HasChanged()
    {
        return History.HasChanges;
    }

    public override void Undo()
    {
        History.InvokeLastAction(isRedo: false);
        base.Undo();
    }

    public override void Redo()
    {
        History.InvokeLastAction(isRedo: true);
        base.Redo();
    }

    public override void SelectAll()
    {
        View?.DropClient.SelectAll();
        base.SelectAll();
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

    public override void Find()
    {
        IsFinding = !(IsReplacing = false);

        base.Find();
    }

    public override Task FindAndReplace()
    {
        IsFinding = IsReplacing = true;

        return base.FindAndReplace();
    }

    partial void OnMatchCaseChanged(bool value) => OnFindFieldChanged(FindField);
    partial void OnFindFieldChanged(string? value)
    {
        _searchCache.Clear();
        SearchCount = -1;

        if (string.IsNullOrEmpty(value)) {
            return;
        }

        Iter(Root);
        SearchCount = _searchCache.Count - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Iter(FileItemNode node)
        {
            foreach (var child in node.Children) {
                if (!child.IsFile) {
                    Iter(child);
                }
                else if ((MatchCase ? child.Header : child.Header.ToLower()).Contains(MatchCase ? value! : value!.ToLower())) {
                    _searchCache.Add(child);
                }
            }
        }
    }

    public void FindNextCommand(bool clearSelection) => FindNext(clearSelection, findLast: false);
    public void FindNext(bool clearSelection, bool findLast = false)
    {
        if (!_searchCache.Any()) {
            return;
        }

        if (clearSelection) {
            Selected.Clear();
        }

        // Find/select next node
        FileItemNode node = findLast
            ? _searchCache[SearchIndex = SearchIndex == 0 ? SearchCount : --SearchIndex]
            : _searchCache[SearchIndex = SearchIndex >= SearchCount ? 0 : ++SearchIndex];
        node.IsSelected = true;

        // Expand path to node
        FileItemNode? parent = node;
        while ((parent = parent.Parent) != null) {
            parent.IsExpanded = true;
        }

        // Add to selection
        Selected.Add(node);

        // Execute replace function
        if (IsReplacing && ReplaceField != null) {
            // Rename the selected item
            node.PrevName = node.Header;
            node.Header = node.Header.Replace(FindField ?? "", ReplaceField);
            RenameMapNode(node);
        }
    }

    public void FindAll()
    {
        Selected.Clear();
        SearchIndex = -1;
        while (SearchIndex < SearchCount) {
            FindNext(clearSelection: false);
        }
    }

    public void ChangeFindMode() => IsReplacing = !IsReplacing;
    public void CloseFindDialog() => IsFinding = false;

    public FileItemNode ImportFile(string path, byte[] data, bool isRelPath = false, FileItemNode? parentNode = null)
    {
        if (CreateNodeFromPath(isRelPath ? path : Path.GetFileName(path), data, expandParentTree: true, parentNode) is FileItemNode node) {
            node.IsSelected = true;
            Selected.Add(node);

            if (!isRelPath) {
                History.StageChange(SarcChange.Import, new(1) {
                    node
                });
            }

            return node;
        }

        throw new InvalidOperationException("Import Failed: The file node could not be created");
    }

    public void ImportFolder(string path, bool importTopLevel = false, FileItemNode? parentNode = null)
    {
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        if (files.Length > 0) {
            FileItemNode[] nodes = new FileItemNode[files.Length];
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                nodes[i] = ImportFile(Path.GetRelativePath(importTopLevel ? Path.GetDirectoryName(path)! : path, file),
                    File.ReadAllBytes(file), isRelPath: true, parentNode);
            }

            History.StageChange(SarcChange.Import, nodes.ToList());
        }
    }

    public void Edit()
    {
        if (Selected.FirstOrDefault() is FileItemNode node && node.IsFile) {
            EditorMgr.TryLoadEditorSafe(node.GetFilePath(), node.Data, node.SetData);
        }
    }

    public void Rename()
    {
        if (Selected.FirstOrDefault() is FileItemNode node) {
            node.BeginRename();
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

    public async Task ExportPath()
    {
        if (Selected.Count <= 0) {
            return;
        }

        if (Selected.Count == 1 && Selected[0].IsFile) {
            BrowserDialog dialog = new(BrowserMode.SaveFile, "Save File", "Any File:*.*", Path.GetFileName(Selected[0].Header), "export-sarc-file");
            if (await dialog.ShowDialog() is string path) {
                Selected[0].Export(path, relativeTo: Root);
            }
        }
        else {
            BrowserDialog dialog = new(BrowserMode.OpenFolder, "Save to Folder", "Any File:*.*", instanceBrowserKey: "export-sarc-folder");
            if (await dialog.ShowDialog() is string path) {
                foreach (var node in Selected) {
                    node.Export(path, relativeTo: Root);
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
                App.Toast("File Replaced", "Sucess", NotificationType.Success);
            }
        }
    }

    public void Remove()
    {
        if (Selected.Any()) {
            History.StageChange(SarcChange.Remove, Selected.ToList());
            foreach (var item in Selected) {
                (item.Parent ?? Root).Children.Remove(item);
                RemoveNodeFromMap(item);
            }
        }
    }

    private FileItemNode? CreateNodeFromPath(string path, byte[] data, bool expandParentTree = false, FileItemNode? parentNode = null)
    {
        NodeMap map = parentNode != null ? FindNodeMap(parentNode)! : _map;
        FileItemNode item = parentNode ?? Root;

        foreach (var part in path.Replace('\\', '/').Split('/')) {
            if (!map.TryGetValue(part, out var node)) {
                map[part] = node = (new(part, item), new NodeMap());
                item.Children.Add(node.root);
            }
            else if (node.root.IsFile) {
                App.Toast($"Replaced {node.root.Header}", "Sucess", NotificationType.Success);
            }

            item = node.root;
            map = (NodeMap)node.map;

            if (expandParentTree) {
                item.IsExpanded = true;
                Selected.Add(item);
            }
        }

        if (item != null) {
            item.SetData(data);
            return item;
        }

        throw new Exception($"Import Failed: the tree item was null - '{path}' ({data.Length})");
    }

    internal NodeMap? RemoveNodeFromMap(FileItemNode node, string? key = null)
    internal (NodeMap, NodeMap) RemoveNodeFromMap(FileItemNode node, string? key = null)
    {
        key ??= node.Header;
        NodeMap? map = FindNodeMap(node);
        if (map != null) {
            NodeMap child = (NodeMap)map[key].map;
        map?.Remove(key);
            return (map!, child!);
        }

        return (null!, null!);
    }

    internal NodeMap? FindNodeMap(FileItemNode node)
    {
        NodeMap map = _map;
        foreach (var part in node.GetPathParts()) {
            if (!map.TryGetValue(part, out var _node)) {
                return null;
            }

            map = (NodeMap)_node.map;
        }

        return map;
    }

    internal void RenameMapNode(FileItemNode node)
    {
        (NodeMap map, NodeMap child) = RemoveNodeFromMap(node, node.PrevName)!;

        map![node.Header] = (node, child);
        node.PrevName = null;
    }
}
