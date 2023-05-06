using CommunityToolkit.Mvvm.ComponentModel;
using ExKingEditor.Helpers;
using System.Collections.ObjectModel;

namespace ExKingEditor.Models;

public partial class TreeItemNode : ObservableObject
{
    [ObservableProperty]
    private TreeItemNode? _parent;

    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TreeItemNode> _children = new();

    private nint _data;
    private int? _len;

    public bool IsFile => _len != null;

    public TreeItemNode(string header, TreeItemNode? parent = null)
    {
        _header = header;
        _parent = parent;
    }

    public string GetPath(TreeItemNode? relativeTo = null)
    {
        Stack<string> parts = new();
        TreeItemNode? parent = _parent;
        while (parent != null && parent != relativeTo) {
            parts.Push(parent.Header);
            parent = parent.Parent;
        }

        return Path.Combine(parts.ToArray());
    }

    public IEnumerable<TreeItemNode> GetFileNodes(bool recursive = true)
    {
        IEnumerable<TreeItemNode> result = Children.Where(x => x.IsFile);
        foreach (var child in Children.Where(x => !x.IsFile)) {
            result = result.Concat(child.GetFileNodes(recursive));
        }

        return result;
    }

    public unsafe void SetData(ReadOnlySpan<byte> data)
    {
        _len = data.Length;
        fixed (byte* ptr = data) {
            _data = (nint)ptr;
        }
    }

    public unsafe Span<byte> GetData()
    {
        if (_len is int len) {
            return new((byte*)_data, len);
        }

        throw new InvalidDataException("The node does not have any data!");
    }

    public TreeItemNode Clone()
    {
        return (TreeItemNode)MemberwiseClone();
    }
}
