using CommunityToolkit.Mvvm.ComponentModel;

namespace NxEditor.Models;

public partial class StateMgr : ObservableObject
{
    public static StateMgr Shared { get; } = new();

    [ObservableProperty]
    private RecentList _recent = new();

    [ObservableProperty]
    private bool _canSave = false;

    [ObservableProperty]
    private bool _canFind = false;
}
