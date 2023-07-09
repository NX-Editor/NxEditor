using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;

namespace NxEditor.Core;

public partial class Config : ConfigModule<Config>
{
    public override string Name => "nx-editor";
    public static string AppFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "nx-editor");

    [ObservableProperty]
    [property: Config(
        Header = "Load Resources from Disk",
        Description = "Loads a customizable copy of the resource files from disk instead of donwloading the latest version from the live server")]
    private bool _loadResourcesFromDisk;

    [ObservableProperty]
    [property: Config(
        Header = "Single Instance Lock",
        Description = "Files opened through the shell will be opened as a tab in the current instance, rather than starting a second running application",
        Group = "Lock Settings")]
    private bool _useSingleInstance = true;

    [ObservableProperty]
    [property: Config(
        Header = "Single File Lock",
        Description = "Restrict two files with the same path to be loaded concurrently",
        Group = "Lock Settings")]
    private bool _useSingleFileLock = false;

    [ObservableProperty]
    [property: Config(
        Header = "Default RESTBL Hash Table",
        Description = "The absolue path to the hash table loaded when a RESTBL file is opened\n\n(Please paste the file path in, file browsing is currently not supported)",
        Group = "RESTBL",
        Category = "Editor Config")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Title = "Browse for a RESTBl string table")]
    private string _defaultHashTable = string.Empty;

    [ObservableProperty]
    [property: Config(
        Header = "Theme",
        Description = "",
        Group = "Appearance")]
    [property: DropdownConfig("Dark", "Light")]
    private string _theme = "Dark";

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

    partial void OnThemeChanged(string? oldValue, string newValue)
    {
        SetTheme(newValue);
    }

    public static void SetTheme(string variant)
    {
        if (Application.Current is Application app) {
            app.RequestedThemeVariant = variant == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }
}
