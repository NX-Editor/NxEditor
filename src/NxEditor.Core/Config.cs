using Avalonia.SettingsFactory.Core;
using System.Text.Json;

namespace NxEditor.Core;

public class Config : ISettingsBase
{
    public static string AppFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "nx-editor");

    private static readonly string _path = Path.Combine(AppFolder, "config.json");

    public static Config Shared { get; } = Load();
    public bool RequiresInput { get; set; } = true;

    [Setting(UiType.Dropdown, "Resource:Langs", Name = "Game Region/Language")]
    public required string Lang { get; set; }

    [Setting("Load Resources from Disk", "Loads a customizable copy of the resource files from disk instead of donwloading the latest version from the live server")]
    public bool LoadResourcesFromDisk { get; set; }

    [Setting("Single Instance Lock", "Files opened through the shell will be opened as a tab in the current instance, rather than starting a second running application", Category = "Lock Settings")]
    public bool UseSingleInstance { get; set; } = true;

    [Setting("Single File Lock", "Restrict two files with the same path to be loaded concurrently", Category = "Lock Settings")]
    public bool UseSingleFileLock { get; set; } = false;

    // TODO: Support file dialog in place of folder dialog
    [Setting("Default RESTBL Hash Table", "The absolue path to the hash table loaded when a RESTBL file is opened\n\n(Please paste the file path in, file browsing is currently not supported)", Category = "RESTBL", Folder = "Editor Config", ShowBrowseButton = false)]
    public string DefaultHashTable { get; set; } = string.Empty;

    [Setting(UiType.Dropdown, "Dark", "Light", Category = "Appearance")]
    public string Theme { get; set; } = "Dark";

    public ISettingsBase Save()
    {
        RequiresInput = false;

        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        using FileStream fs = File.Create(_path);
        JsonSerializer.Serialize(fs, this);
        return this;
    }

    public static Config Load()
    {
        if (!File.Exists(_path)) {
            return Create();
        }

        using FileStream fs = File.OpenRead(_path);
        return JsonSerializer.Deserialize<Config>(fs) ?? Create();
    }

    private static Config Create()
    {
        Config config = new() {
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
            "GamePath" => File.Exists(Path.Combine(path, "Pack", "ZsDic.pack.zs")),
            _ => false,
        };
    }
}
