using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;

namespace NxEditor.Core;

public class Config : ConfigModule<Config>
{
    public override string Name => "nx-editor";
    public static string AppFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "nx-editor");

    [Config(
        Header = "Load Resources from Disk",
        Description = "Loads a customizable copy of the resource files from disk instead of donwloading the latest version from the live server")]
    public bool LoadResourcesFromDisk { get; set; }

    [Config(
        Header = "Single Instance Lock",
        Description = "Files opened through the shell will be opened as a tab in the current instance, rather than starting a second running application",
        Category = "Lock Settings")]
    public bool UseSingleInstance { get; set; } = true;

    [Config(
        Header = "Single File Lock",
        Description = "Restrict two files with the same path to be loaded concurrently",
        Category = "Lock Settings")]
    public bool UseSingleFileLock { get; set; } = false;

    [Config(
        Header = "Default RESTBL Hash Table",
        Description = "The absolue path to the hash table loaded when a RESTBL file is opened\n\n(Please paste the file path in, file browsing is currently not supported)",
        Category = "RESTBL",
        Group = "Editor Config")]
    [BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Title = "Browse for a RESTBl string table")]
    public string DefaultHashTable { get; set; } = string.Empty;

    [Config(Header = "Theme", Description = "", Category = "Appearance")]
    public string Theme { get; set; } = "Dark";

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
