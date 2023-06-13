using CommunityToolkit.Mvvm.ComponentModel;
using NxEditor.ViewModels.Editors;

namespace NxEditor.Models;

public partial class RestblEntry : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private uint? _hash;

    [ObservableProperty]
    private uint _size;

    public RestblEntry(string name, uint? hash, uint size)
    {
        _name = name;
        _hash = hash;
        _size = size;
    }

    public void Edit(RestblViewModel parent)
    {
        parent.CurrentName = Name;
        parent.CurrentSize = Size;
    }

    public void Remove(RestblViewModel parent)
    {
        if (Hash is uint hash) {
            parent.Table.CrcTable.Remove(hash);
        }
        else {
            parent.Table.NameTable.Remove(Name);
        }

        parent.Items.Remove(this);
    }
}
