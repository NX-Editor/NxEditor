using Avalonia.Platform;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace NxEditor.Components;

public class FontAwesomeJsonStreamProvider : IFontAwesomeUtf8JsonStreamProvider
{
    public static readonly FontAwesomeJsonStreamProvider Instance = new();

    public Stream GetUtf8JsonStream()
    {
        const string iconsResourcePath = "avares://NxEditor/Assets/fa-icons.json";
        Uri iconsResourceUri = new(iconsResourcePath);
        return AssetLoader.Open(iconsResourceUri);
    }
}
