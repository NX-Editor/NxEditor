using CommunityToolkit.Mvvm.ComponentModel;
using NxEditor.ViewModels.Editors;

namespace NxEditor.Models;

public partial class RestblEntry : ObservableObject
{
    [ObservableProperty]
    private string _pinHeaderText = "Pin";

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private uint? _hash;

    [ObservableProperty]
    private uint _size;

    [ObservableProperty]
    private bool _isPinned;

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

    public void Pin(RestblViewModel parent)
    {
        if (IsPinned = !IsPinned) {
            PinHeaderText = "Unpin";
        }
        else {
            PinHeaderText = "Pin";
            parent.Pinned.Remove(this);
        }
    }

    public void Remove(RestblViewModel parent)
    {
        if (Hash is uint hash) {
            parent.Table.CrcTable.Remove(hash);
        }
        else {
            parent.Table.NameTable.Remove(Name);
        }

        parent.Pinned.Remove(this);
    }
}
