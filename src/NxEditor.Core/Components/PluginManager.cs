using ConfigFactory;
using ConfigFactory.Core;
using ConfigFactory.Models;
using NxEditor.Core.Models;
using NxEditor.PluginBase;
using System.Collections.ObjectModel;
using System.IO.Compression;

namespace NxEditor.Core.Components;

public class PluginManager
{
    private static readonly string _path = Path.Combine(Config.AppFolder, "plugins");
    private readonly ConfigPageModel _configPageModel = new();
    private List<IServiceExtension> _extensions = new();

    public PluginManager(ConfigPageModel configPageModel)
    {
        _configPageModel = configPageModel;
    }

    public void LoadInstalled(out bool isConfigValid)
    {
        if (!Directory.Exists(_path)) {
            isConfigValid = true;
            return;
        }

        LoadFolder(_path, out isConfigValid);
    }

    public void Install(string pluginPack, out bool isConfigValid)
    {
        string pluginPath = Path.Combine(_path, Path.GetFileNameWithoutExtension(pluginPack));

        Directory.CreateDirectory(pluginPath);
        ZipFile.ExtractToDirectory(pluginPack, pluginPath);

        LoadFolder(pluginPath, out isConfigValid);
    }

    public void Register()
    {
        foreach (var ext in _extensions) {
            try {
                ext.RegisterExtension(ServiceLoader.Shared);
            }
            catch (Exception ex) {
                Logger.Write(new Exception($"Failed to load extension: {ext.Name}", ex));
            }
        }
    }

    public static ObservableCollection<PluginInfo> GetPluginInfo() => GetPluginInfo(_path);
    public static ObservableCollection<PluginInfo> GetPluginInfo(string path)
    {
        return Directory.Exists(path) ? new(Directory
            .EnumerateFiles(path, "meta.json", SearchOption.AllDirectories)
            .Select(PluginInfo.FromPath)) : new();
    }

    private void LoadFolder(string path, out bool isConfigValid)
    {
        isConfigValid = true;
        foreach (var info in GetPluginInfo(path).Where(x => x.IsEnabled)) {
            Load(info, out isConfigValid);
        }
    }

    private void Load(PluginInfo info, out bool isConfigValid)
    {
        isConfigValid = true;
        string assemblyPath = Path.Combine(info.Folder, info.Assembly);

        PluginLoadContext loader = new(assemblyPath);
        using FileStream fs = File.OpenRead(assemblyPath);
        Type[] types = loader.LoadFromStream(fs).GetExportedTypes();

        foreach (var type in types.Where(x => x.GetInterface("IConfigModule") == typeof(IConfigModule))) {
            IConfigModule module = (IConfigModule)Activator.CreateInstance(type)!;
            isConfigValid = isConfigValid && module.Shared.Validate(out _);
            _configPageModel.Append(module.Shared);
            _configPageModel.ConfigModules.Add(type.Name, module.Shared);
        }

        foreach (var type in types.Where(x => x.GetInterface("IServiceExtension") == typeof(IServiceExtension))) {
            try {
                _extensions.Add((IServiceExtension)Activator.CreateInstance(type)!);
            }
            catch (Exception ex) {
                Logger.Write(new Exception($"Failed to load plugin: {info.Name}", ex));
            }
        }

        StatusModal.Set($"Loaded {info.Assembly}.dll", temporaryStatusTime: 2);
    }
}
