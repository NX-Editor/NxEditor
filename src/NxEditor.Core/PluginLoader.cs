using NxEditor.Core.Models;
using NxEditor.PluginBase;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Reflection;

namespace NxEditor.Core;

public static class PluginLoader
{
    private static readonly string _path = Path.Combine(Config.AppFolder, "plugins");

    public static void LoadInstalledPlugins()
    {
        if (!Directory.Exists(_path)) {
            return;
        }

        LoadFolder(_path);
    }

    public static ObservableCollection<PluginInfo> GetPluginInfo() => GetPluginInfo(_path);
    public static ObservableCollection<PluginInfo> GetPluginInfo(string path)
    {
        return Directory.Exists(path) ? new(Directory
            .EnumerateFiles(path, "meta.json", SearchOption.AllDirectories)
            .Select(PluginInfo.FromPath)) : new();
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
        foreach (var info in GetPluginInfo(path)) {
            Load(info);
        }
    }

    private static void Load(PluginInfo info)
    {
        if (!info.IsEnabled) {
            return;
        }

        string assemblyPath = Path.Combine(info.Folder, info.Assembly);

        PluginLoadContext loader = new(assemblyPath);
        using FileStream fs = File.OpenRead(assemblyPath);
        Assembly asm = loader.LoadFromStream(fs);

        foreach (var type in asm.GetExportedTypes().Where(x => x.GetInterface("IServiceExtension") == typeof(IServiceExtension))) {
            try {
                IServiceExtension service = (IServiceExtension)Activator.CreateInstance(type)!;
                service.RegisterExtension(ServiceLoader.Shared);
                Logger.Write($"Loaded {service.Name}");
            }
            catch (Exception ex) {
                Logger.Write(new Exception($"Failed to load plugin: {info.Name}", ex));
            }
        }
    }
}
