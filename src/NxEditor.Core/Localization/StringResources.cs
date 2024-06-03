global using static NxEditor.Core.Localization.StringResources;
using System.Text.Json;
using LocalizationResources = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;

namespace NxEditor.Core.Localization;

public static class StringResources
{
    private static readonly string _localizationsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Languages");
    private static readonly LocalizationResources _resources = LoadResources();

    public static readonly StringResources_Exceptions Exceptions = new();
    public static readonly StringResources_Statuses Statuses = new();
    public static readonly StringResources_System SystemMsg = new();

    internal static LocalizationResources LoadResources()
    {
        string configuredLocalizationFilePath = Path.Combine(_localizationsFolderPath, $"{NXE.Config.CultureName}.json");
        if (File.Exists(configuredLocalizationFilePath)) {
            using FileStream configuredLocalizationFileStream = File.OpenRead(configuredLocalizationFilePath);
            return JsonSerializer.Deserialize<LocalizationResources>(configuredLocalizationFileStream) ?? [];
        }

        string defaultLocalizationFilePath = Path.Combine(_localizationsFolderPath, "en-US.json");
        using FileStream defaultLocalizationFileStream = File.OpenRead(defaultLocalizationFilePath);
        return JsonSerializer.Deserialize<LocalizationResources>(defaultLocalizationFileStream) ?? [];
    }

    internal static string GetStringResource(string group, string name)
    {
        return _resources[group][name];
    }
}
