using System.Text.Json;

namespace NxEditor.Core;

public class PluginConfig : Dictionary<string, bool>
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "nx-editor", "plugin-config.json");

    public static PluginConfig Shared { get; } = Load();

    public PluginConfig Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        using FileStream fs = File.Create(_path);
        JsonSerializer.Serialize(fs, this);
        return this;
    }

    public static PluginConfig Load()
    {
        if (!File.Exists(_path)) {
            return new PluginConfig().Save();
        }

        using FileStream fs = File.OpenRead(_path);
        return JsonSerializer.Deserialize<PluginConfig>(fs)!;
    }
}
