using System.Reflection;
using System.Text.Json;

namespace ExKingEditor.Core.Extensions;

public static class EmbedExtension
{
    public static Stream? Fetch<T>(string name) => FetchEmbed(typeof(T).Assembly, name);
    public static Stream? FetchEmbed(this Assembly assembly, string name)
    {
        return assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.{name}");
    }

    public static TValue? Parse<T, TValue>(string name) => ParseEmbed<TValue>(typeof(T).Assembly, name);
    public static TValue? ParseEmbed<TValue>(this Assembly assembly, string name)
    {
        using Stream? stream = FetchEmbed(assembly, name);
        if (stream != null) {
            return JsonSerializer.Deserialize<TValue>(stream);
        }

        return default;
    }
}
