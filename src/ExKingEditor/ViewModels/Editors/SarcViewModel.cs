using Avalonia;
using Avalonia.Input;
using Cead;
using Cead.Handles;
using CommunityToolkit.Mvvm.ComponentModel;
using ExKingEditor.Core;
using ExKingEditor.Core.Windows;
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

    private readonly Stack<(Change change, FileItemNode node)> _history = new();
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
            AppendPathToView(name, sarcFile.AsSpan());
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

        // Sorry to anyone trying
        // to read this mess
        // 
        // TODO: Document this
        IEnumerable<string> paths = Selected
            .Select(parent => parent.GetFileNodes()
                .Select(x => {
                    string file = Path.Combine(_temp, x.GetPath(), x.Header);
                    x.Export(file, isSingleFile: true);

                    return parent.IsFile ? file : Path.Combine(_temp, parent.GetPath(), parent.Header);
                }
            ))
            .Aggregate((current, next) => current.Concat(next))
            .Distinct();

        obj.Set("Files", CF_HDROP.Build(paths));
        obj.Set("FileName", @"D:\Bin\Totk\Dummy.root.asb"u8.ToArray());
        obj.Set("FileNameW", @"D:\Bin\Totk\Dummy.root.asb");

        await Application.Current!.Clipboard!.SetDataObjectAsync(obj);
        await base.Copy();
    }
    public override void SelectAll()
    {
        View?.DropClient.SelectAll();
        base.SelectAll();
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
        if (Selected.FirstOrDefault() is FileItemNode node) {
            node.IsRenaming = true;
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
        foreach (var item in Selected) {
            (item.Parent ?? Root).Children.Remove(item);
            _history.Push((Change.Remove, item));
        }
    }

    private void AppendPathToView(string path, ReadOnlySpan<byte> data)
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
