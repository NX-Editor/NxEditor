using ConfigFactory;
using ConfigFactory.Core;
using ConfigFactory.Models;
using NxEditor.Core.Models;
using NxEditor.PluginBase;
using System.IO.Compression;

namespace NxEditor.Core.Components;

public static class PluginManager
{
    private static readonly string _path = Path.Combine(Config.AppFolder, "plugins");
    private static readonly List<IServiceExtension> _extensions = new();
    private static readonly Dictionary<Type, IConfigModule> _modules = new();

    public static void Load()
    {
        if (!Directory.Exists(_path)) {
            return;
        }

        LoadDirectory(_path);
    }

    public static bool ValidateModules()
    {
        bool result = true;
        foreach ((_, var module) in _modules) {
            result = result && module.Shared.Validate(out _);
        }

        return result;
    }

    public static bool RegisterModules(ConfigPageModel configPage)
    {
        bool result = true;
        foreach ((var type, var module) in _modules) {
            try {
                configPage.Append(module.Shared);
                configPage.ConfigModules.Add(type.Name, module.Shared);
                result = result && module.Shared.Validate(out _);
            }
            catch (Exception ex) {
                Logger.Write(new Exception($"Failed to register ConfigModule from {type.Name}", ex));
            }
        }

        return result;
    }

    public static void RegisterExtensions()
    {
        foreach (var extension in _extensions) {
            try {
                extension.RegisterExtension(ServiceLoader.Shared);
            }
            catch (Exception ex) {
                Logger.Write(new Exception($"Failed to register ServiceExtension: {extension.Name}", ex));
            }
        }
    }

    public static void Install(string pluginPack)
    {
        string pluginPath = Path.Combine(_path, Path.GetFileNameWithoutExtension(pluginPack));

        Directory.CreateDirectory(pluginPath);
        ZipFile.ExtractToDirectory(pluginPack, pluginPath);

        LoadDirectory(pluginPath);
    }

    public static IEnumerable<PluginInfo> GetPluginInfo() => GetPluginInfo(_path);
    public static IEnumerable<PluginInfo> GetPluginInfo(string path)
    {
        return Directory.Exists(path) ? Directory
            .EnumerateFiles(path, "meta.json", SearchOption.AllDirectories)
            .Select(PluginInfo.FromPath) : Array.Empty<PluginInfo>();
    }

    private static void LoadDirectory(string path)
    {
        foreach (var info in GetPluginInfo(path).Where(x => x.IsEnabled)) {
            LoadPlugin(info);
        }
    }

    private static void LoadPlugin(PluginInfo info)
    {
        string assemblyPath = Path.Combine(info.Folder, info.Assembly);

        PluginLoadContext loader = new(assemblyPath);
        using FileStream fs = File.OpenRead(assemblyPath);
        Type[] types = loader.LoadFromStream(fs).GetExportedTypes();

        foreach (var type in types.Where(x => x.GetInterface("IConfigModule") == typeof(IConfigModule))) {
            try {
                _modules.Add(type, (IConfigModule)Activator.CreateInstance(type)!);
            }
            catch (Exception ex) {
                Logger.Write(new Exception($"Failed to load ConfigModule: {info.Name}", ex));
            }
        }

        foreach (var type in types.Where(x => x.GetInterface("IServiceExtension") == typeof(IServiceExtension))) {
            try {
                _extensions.Add((IServiceExtension)Activator.CreateInstance(type)!);
            }
            catch (Exception ex) {
                Logger.Write(new Exception($"Failed to load ServiceExtension: {info.Name}", ex));
            }
        }

        string msg = $"Loaded {info.Assembly}";

        StatusModal.Set(msg, isWorkingStatus: false, temporaryStatusTime: 2);
        Logger.Write(msg);
    }
}
