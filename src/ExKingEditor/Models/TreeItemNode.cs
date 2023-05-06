using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ExKingEditor.Models;

public partial class TreeItemNode : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TreeItemNode> _children = new();

    public TreeItemNode(string header)
    {
        _header = header;
    }
}
