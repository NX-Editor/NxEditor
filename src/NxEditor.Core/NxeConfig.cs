using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace NxEditor.Core;

public enum AppTheme
{
    System,
    Dark,
    Light,
}

public partial class NxeConfig : ObservableValidator
{
    private static readonly string _appConfigFilePath = Path.Combine(NXE.SystemPath, "AppConfig.json");

    public event EventHandler<AppTheme>? AppThemeChanged;

    [ObservableProperty]
    private AppTheme _theme = AppTheme.System;

    [ObservableProperty]
    private string _cultureName = CultureInfo.CurrentCulture.TextInfo.CultureName;

    public void Save()
    {
        using FileStream fs = File.Create(_appConfigFilePath);
        JsonSerializer.Serialize(fs, this);
    }

    internal static NxeConfig LoadFromDisk()
    {
        if (!File.Exists(_appConfigFilePath)) {
            return new();
        }

        using FileStream fs = File.OpenRead(_appConfigFilePath);
        return JsonSerializer.Deserialize<NxeConfig>(fs)
            ?? new();
    }

    partial void OnThemeChanged(AppTheme value)
    {
        AppThemeChanged?.Invoke(this, value);
    }
}
