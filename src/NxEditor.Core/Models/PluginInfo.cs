using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace NxEditor.Core.Models;

public partial class PluginInfo : ObservableObject
{
    private bool _isLoading = true;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
#pragma warning disable CS0657 // Not a valid attribute location for this declaration
    [property: System.Text.Json.Serialization.JsonIgnore]
#pragma warning restore CS0657 // Not a valid attribute location for this declaration
    private string _folder = string.Empty;

    [ObservableProperty]
    private string _assembly = string.Empty;

    [ObservableProperty]
    private bool _isEnabled;

    public PluginInfo() { }

    public static PluginInfo FromPath(string meta)
    {
        using FileStream fs = File.OpenRead(meta);
        PluginInfo info = JsonSerializer.Deserialize<PluginInfo>(fs)!;
        info.Folder = Path.GetDirectoryName(meta)!;
        info._isLoading = false;

        return info;
    }

    partial void OnIsEnabledChanged(bool value)
    {
        if (!_isLoading) {
            using FileStream fs = File.Create(Path.Combine(Folder, "meta.json"));
            JsonSerializer.Serialize(fs, this);
        }
    }
}
