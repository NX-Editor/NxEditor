using System.Net.NetworkInformation;
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
        Stream? stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.{name}");
        if (Config.Shared.LoadResourcesFromDisk) {
            string path = Path.Combine(_path, name);
            if (!File.Exists(path)) {
                if (stream != null) {
                    using FileStream fs = File.Create(path);
                    stream.CopyTo(fs);
                    stream.Seek(0, SeekOrigin.Begin);
                }

                return stream;
            }

            return File.OpenRead(path);
        }
        else {
            return stream;
        }
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
