using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ExKingEditor.ViewModels.Editors;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace ExKingEditor.Models;

public partial class FileItemNode : ObservableObject
{
    private TextBox? _renameClient = null;

    [ObservableProperty]
    private FileItemNode? _parent;

    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private ObservableCollection<FileItemNode> _children = new();

    [ObservableProperty]
    private bool _isRenaming;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isSelected;

    private byte[]? _data;

    public byte[] Data => _data ?? throw new InvalidDataException("The node does not have any data!");
    public bool IsFile => _data != null;
    public string? PrevName { get; set; }

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

    public void BeginRename()
    {
        App.Desktop?.DisableGlobalShortcuts();
        _renameClient?.SelectAll();
        _renameClient?.Focus();
        PrevName = Header;

        IsRenaming = true;
    }

    public void EndRename(SarcViewModel owner)
    {
        App.Desktop?.ActivateGlobalShortcuts();
        owner.RenameMapNode(this);
        // Stage change in owner
        IsRenaming = false;
    }

    public async Task ExportAsync(string path, bool recursive = true, bool isSingleFile = false, FileItemNode? relativeTo = null)
        => await Task.Run(() => Export(path, recursive, isSingleFile, relativeTo));
    public void Export(string path, bool recursive = true, bool isSingleFile = false, FileItemNode? relativeTo = null)
    {
        if (IsFile && relativeTo != null) {
            Directory.CreateDirectory(path = Path.Combine(Path.GetDirectoryName(path)!, GetPath(relativeTo)));
            using FileStream fs = File.Create(Path.Combine(path, Header));
            fs.Write(Data);
        }
        else if (IsFile) {
            Directory.CreateDirectory(isSingleFile ? Path.GetDirectoryName(path)! : path);
            using FileStream fs = File.Create(isSingleFile ? path : Path.Combine(path, Header));
            fs.Write(Data);
        }
        else {
            foreach (var file in GetFileNodes(recursive)) {
                file.Export(Path.Combine(path, file.GetPath(relativeTo)));
            }
        }
    }

    public string GetFilePath(FileItemNode? relativeTo = null) => Path.Combine(GetPathParts(relativeTo).Append(Header).ToArray());
    public string GetPath(FileItemNode? relativeTo = null) => Path.Combine(GetPathParts(relativeTo).ToArray());
    public Stack<string> GetPathParts(FileItemNode? relativeTo = null)
    {
        Stack<string> parts = new();
        FileItemNode? parent = Parent;
        while (parent != null && parent != relativeTo && parent.Header != "__root__") {
            parts.Push(parent.Header);
            parent = parent.Parent;
        }

        return parts;
    }

    public IEnumerable<FileItemNode> GetFileNodes(bool recursive = true)
    {
        IEnumerable<FileItemNode> result;
        if (!IsFile) {
            result = Children.Where(x => x.IsFile);
            foreach (var child in Children.Where(x => !x.IsFile)) {
                result = result.Concat(child.GetFileNodes(recursive));
            }
        }
        else {
            result = new FileItemNode[1] { this };
        }

        return result;
    }

    public void SetData(byte[] data)
    {
        _data = data;
    }

    internal void SetRenameClient(TextBox? renameClient)
    {
        _renameClient = renameClient;
    }

    public FileItemNode Clone()
    {
        return (FileItemNode)MemberwiseClone();
    }
}
