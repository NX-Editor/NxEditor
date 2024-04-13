using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using System.Text.Json.Serialization;

namespace NxEditor.Core;

public partial class Config : ConfigModule<Config>
{
    [JsonIgnore]
    public override string Name { get; } = "nx-editor-static";

    [ObservableProperty]
    [property: ConfigFactory.Core.Attributes.Config(
        Header = "Theme",
        Description = "",
        Group = "Application")]
    [property: ConfigFactory.Core.Attributes.DropdownConfig("Dark", "Light")]
    private string _theme = "Dark";

    [ObservableProperty]
    [property: ConfigFactory.Core.Attributes.Config(
        Header = "Single Instance Lock",
        Description = "Files opened through the shell will be opened as a tab in the current instance, rather than starting a second running application",
        Group = "Lock Settings")]
    private bool _useSingleInstance = true;

    [ObservableProperty]
    [property: ConfigFactory.Core.Attributes.Config(
        Header = "Single File Lock",
        Description = "Restrict two files with the same path to be loaded concurrently",
        Group = "Lock Settings")]
    private bool _useSingleFileLock = false;

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

    partial void OnThemeChanged(string value)
    {
        SetTheme(value);
    }

    public static void SetTheme(string variant)
    {
        if (Application.Current is Application app) {
            app.RequestedThemeVariant = variant == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }
}
