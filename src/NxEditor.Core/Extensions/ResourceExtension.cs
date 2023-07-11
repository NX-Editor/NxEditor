using System.Reflection;
using System.Text.Json;

namespace NxEditor.Core.Extensions;

public static class ResourceExtension
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "nx-editor", "resources");

    static ResourceExtension()
    {
        Directory.CreateDirectory(_path);
    }

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
