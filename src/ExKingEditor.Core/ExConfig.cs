using Avalonia.SettingsFactory.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExKingEditor.Core;

public class ExConfig : ISettingsBase
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EX-King-Editor", "config.json");

    public static ExConfig Shared { get; } = Load();
    public bool RequiresInput { get; set; } = false;

    [JsonIgnore]
    [Setting("Game Path", "The absolute path to your Totk game dump\n(e.g. F:\\Games\\Totk\\RomFS)")]
    public static string GamePath {
        get => TotkConfig.Shared.GamePath;
        set => TotkConfig.Shared.GamePath = value;
    }

    [Setting(UiType.Dropdown, "Resource:Langs", Name = "Game Region/Language")]
    public required string Lang { get; set; }

    [Setting(UiType.Dropdown, "Dark", "Light", Category = "Appearance")]
    public string Theme { get; set; } = "Dark";

    public ISettingsBase Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        using FileStream fs = File.Create(_path);
        JsonSerializer.Serialize(fs, this);
        return this;
    }

    public static ExConfig Load()
    {
        if (!File.Exists(_path)) {
            return Create();
        }

        using FileStream fs = File.OpenRead(_path);
        return JsonSerializer.Deserialize<ExConfig>(fs) ?? Create();
    }

    private static ExConfig Create()
    {
        ExConfig config = new() {
            Lang = "USen",
            RequiresInput = true,
        };

        config.Save();
        return config;
    }

    public static bool ValidatePath(string path, string key)
    {
        if (path == null || File.Exists(path)) {
            return false;
        }

        return key switch {
            "GamePath" => File.Exists(Path.Combine(path, "ActorSystem", "ActorSystemSetting","GameActor.engine__actor__ActorSystemSetting.bgyml")),
            _ => false,
        };
    }
}
