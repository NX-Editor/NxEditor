using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.SettingsFactory;
using Avalonia.SettingsFactory.Core;
using Avalonia.SettingsFactory.ViewModels;
using Avalonia.Styling;
using ExKingEditor.Core;
using ExKingEditor.Core.Extensions;
using ExKingEditor.Helpers;
using System.Reflection;

namespace ExKingEditor.Views;

public partial class SettingsView : SettingsFactory, ISettingsValidator
{
    public static SettingsView? Live { get; set; } = null;

    private static readonly SettingsFactoryOptions _options = new() {
        AlertAction = (msg) => App.Toast(msg, "Settings", NotificationType.Warning),
        BrowseAction = async (title) => await new BrowserDialog(BrowserMode.OpenFolder).ShowDialog(),
        FetchResource = name => ResourceExtension.Parse<App, Dictionary<string, string>?>($"{name}.json")
    };

    public SettingsView()
    {
        InitializeComponent();

        FocusDelegate.PointerPressed += (s, e) => FocusDelegate.Focus();
        FocusDelegate2.PointerPressed += (s, e) => FocusDelegate.Focus();

        AfterSaveEvent += () => App.Toast("Saved succefully", "Settings", NotificationType.Success);
        InitializeSettingsFactory(new SettingsFactoryViewModel(false), this, ExConfig.Shared, _options);

        Live = this;
    }

    public bool? ValidateBool(string key, bool value)
    {
        return key switch {
            _ => null
        };
    }

    public bool? ValidateString(string key, string? value)
    {
        value ??= string.Empty;
        return key switch {
            "GamePath" => ExConfig.ValidatePath(value, key),
            "Theme" => ValidateTheme(value!),
            _ => null,
        };
    }

    public static bool? ValidateTheme(string value)
    {
        Application.Current!.RequestedThemeVariant = value == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;
        return null;
    }

    public string? ValidateSave()
    {
        Dictionary<string, bool?> validated = new();
        foreach (var prop in ExConfig.Shared.GetType().GetProperties().Where(x => x.GetCustomAttributes<SettingAttribute>(false).Any())) {
            object? value = prop.GetValue(ExConfig.Shared);

            if (value is bool boolean) {
                validated.Add(prop.Name, ValidateBool(prop.Name, boolean));
            }
            else {
                validated.Add(prop.Name, ValidateString(prop.Name, value as string));
            }
        }

        return ValidateSave(validated);
    }

    public string? ValidateSave(Dictionary<string, bool?> validated)
    {
        if (validated["GamePath"] == false) {
            return "The game path is invalid.\nMake sure the path points directly to the game files (not a folder with romfs or the title id folder).";
        }

        return null;
    }
}
