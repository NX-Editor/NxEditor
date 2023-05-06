using Cead;
using Cead.Handles;
using CommunityToolkit.Mvvm.ComponentModel;
using ExKingEditor.Core;
using ExKingEditor.Models;
using System.Collections.ObjectModel;
using NodeMap = System.Collections.Generic.Dictionary<string, (ExKingEditor.Models.TreeItemNode root, object map)>;

namespace ExKingEditor.ViewModels.Editors;

public partial class SarcViewModel : ReactiveEditor
{
    private int _originalCount;
    private readonly Sarc _live;
    private readonly NodeMap _map = new();

    [ObservableProperty]
    private ObservableCollection<TreeItemNode> _children = new();

    public SarcViewModel(string file) : base(file)
    {
        _live = Sarc.FromBinary(RawData());
        _originalCount = _live.Count;

        foreach ((var key, var name) in _live.OrderBy(x => x.Key)) {
            AppendPathToView(key);
        }
    }

    private void AppendPathToView(string path)
    {
        NodeMap map = _map;
        ObservableCollection<TreeItemNode> children = _children;

        foreach (var part in path.Replace('\\', '/').Split('/')) {
            if (!map.TryGetValue(part, out var node)) {
                map[part] = node = (new(part), new NodeMap());
                children.Add(node.root);
            }

            map = (NodeMap)node.map;
            children = node.root.Children;
        }
    }

    public override bool HasChanged()
    {
        return _originalCount != _live.Count;
    }

    public override void SaveAs(string path)
    {
        using DataHandle handle = _live.ToBinary();

        Span<byte> data = _compressed ? TotkZstd.Compress(path, handle) : handle;

        if (path == _file) {
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.Write(data);
        }
        else {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using FileStream fs = File.Create(path);
            fs.Write(data);
        }

        _originalCount = _live.Count;
        ToastSaveSuccess(path);
    }

    public override bool OnClose()
    {
        _live.Dispose();
        return base.OnClose();
    }
}
