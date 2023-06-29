using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace NxEditor.Core.Models;

public partial class PluginInfo : ObservableObject
{
    private bool _isLoading = true;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _folder = string.Empty;

    [ObservableProperty]
    private string _assembly = string.Empty;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private long _gitHubRepoId = -1;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private bool _isOnline = false;

    [ObservableProperty]
    private bool _canUpdate = false;

    public PluginInfo() { }

    public static PluginInfo FromPath(string meta)
    {
        using FileStream fs = File.OpenRead(meta);
        PluginInfo info = JsonSerializer.Deserialize<PluginInfo>(fs)!;
        info.Folder = Path.GetDirectoryName(meta)!;
        info._isLoading = false;

        return info;
    }

    public void Serialize()
    {
        if (!_isLoading) {
            using FileStream fs = File.Create(Path.Combine(Folder, "meta.json"));
            JsonSerializer.Serialize(fs, this);
        }
    }

    partial void OnIsEnabledChanged(bool value) => Serialize();
}
