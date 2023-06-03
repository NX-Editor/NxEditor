using ExKingEditor.Core.Exceptions;
using ExKingEditor.Core.Extensions;
using System.Reflection;

namespace ExKingEditor.Core.Models;

public class Localization
{
    public static Localization Texts { get; set; } = null!;
    public static Dictionary<string, string> TextsMap { get; set; } = null!;

    public static void Load(string localizationName = "USen")
    {
        Assembly asm = typeof(Localization).Assembly;
        Texts = asm.ParseEmbed<Localization>($"Localizations/{localizationName}.json")
            ?? throw new LocalizationException($"Unsupported localization: '{localizationName}'");
        TextsMap = asm.ParseEmbed<Dictionary<string, string>>($"Localizations/{localizationName}.json")!;
    }

    public required string ShellMenu_File_OpenFile { get; set; }
}
