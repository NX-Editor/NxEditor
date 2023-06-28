using NxEditor.Core.Models;
using NxEditor.PluginBase;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;

namespace NxEditor.Core;

public record PluginMeta(string Name, string Path);

public static class PluginLoader
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "nx-editor", "plugins");

    public static void LoadInstalledPlugins()
    {
        if (!Directory.Exists(_path)) {
            return;
        }

        LoadFolder(_path);
    }

    public static void InstallPlugin(string pluginPack)
    {
        string pluginPath = Path.Combine(_path, Path.GetFileNameWithoutExtension(pluginPack));

        Directory.CreateDirectory(pluginPath);
        ZipFile.ExtractToDirectory(pluginPack, pluginPath);

        LoadFolder(pluginPath);
    }

    private static void LoadFolder(string path)
    {
        foreach (var pluginMeta in Directory.EnumerateFiles(path, "meta.json", SearchOption.AllDirectories)) {
            using FileStream fs = File.OpenRead(pluginMeta);
            PluginMeta meta = JsonSerializer.Deserialize<PluginMeta>(fs)!;
            
            string pluginPath = Path.Combine(Path.GetDirectoryName(pluginMeta)!, meta.Path);
            Load(pluginPath, meta.Name);
        }
    }

    private static void Load(string pluginPath, string pluginName)
    {
        if (PluginConfig.Shared.TryGetValue(pluginName, out bool isEnabled) && !isEnabled) {
            return;
        }

        PluginLoadContext loader = new(pluginPath);
        using FileStream fs = File.OpenRead(pluginPath);
        Assembly asm = loader.LoadFromStream(fs);

        foreach (var type in asm.GetExportedTypes().Where(x => x.GetInterface("IServiceExtension") == typeof(IServiceExtension))) {
            try {
                IServiceExtension service = (IServiceExtension)Activator.CreateInstance(type)!;
                service.RegisterExtension(ServiceMgr.Shared);
                Logger.Write($"Loaded {service.Name}");
            }
            catch (Exception ex) {
                Logger.Write(new Exception($"Failed to load plugin: {pluginName}", ex));
            }
        }
    }
}
