using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace ExKingEditor.Models;

public partial class FileItemNode : ObservableObject
{
    [ObservableProperty]
    private FileItemNode? _parent;

    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private ObservableCollection<FileItemNode> _children = new();

    [ObservableProperty]
    private bool _isRenaming;

    private nint _data;
    private int? _len;

    public bool IsFile => _len != null;

    public FileItemNode(string header, FileItemNode? parent = null)
    {
        _header = header;
        _parent = parent;
    }

    public void Sort()
    {
        Children = new(Children.OrderBy(x => x.Header));
        foreach (var child in Children) {
            child.Sort();
        }
    }

    public async Task ExportAsync(string path, bool recursive = true, bool isSingleFile = false, FileItemNode? relativeTo = null)
        => await Task.Run(() => Export(path, recursive, isSingleFile, relativeTo));
    public void Export(string path, bool recursive = true, bool isSingleFile = false, FileItemNode? relativeTo = null)
    {
        if (IsFile) {
            Directory.CreateDirectory(isSingleFile ? Path.GetDirectoryName(path)! : path);
            using FileStream fs = File.Create(isSingleFile ? path : Path.Combine(path, Header));
            fs.Write(GetData());
        }
        else {
            foreach (var file in GetFileNodes(recursive)) {
                file.Export(Path.Combine(path, file.GetPath(relativeTo)));
            }
        }
    }

    public string GetPath(FileItemNode? relativeTo = null)
    {
        Stack<string> parts = new();
        FileItemNode? parent = _parent;
        while (parent != null && parent != relativeTo && parent.Header != "__root__") {
            parts.Push(parent.Header);
            parent = parent.Parent;
        }

        return Path.Combine(parts.ToArray());
    }

    public IEnumerable<FileItemNode> GetFileNodes(bool recursive = true)
    {
        IEnumerable<FileItemNode> result = Children.Where(x => x.IsFile);
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

    public FileItemNode Clone()
    {
        return (FileItemNode)MemberwiseClone();
    }
}
