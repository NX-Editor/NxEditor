using NxEditor.Core.Components;
using NxEditor.Core.Models;
using NxEditor.PluginBase;
using System.IO.Compression;
using System.Text.Json;

namespace NxEditor.Core.Helpers;

public class UpdateHelper
{
    private const string LAUNCHER_NAME = "NxEditor.Launcher.exe";
    private const string GITHUB_ORG = "NX-Editor";
    private const string GITHUB_REPO = "NxEditor";

    public static readonly string LauncherPath = Path.Combine(GlobalConfig.StaticPath, LAUNCHER_NAME);

    private static readonly string _versionFile = Path.Combine(GlobalConfig.StaticPath, "version.json");

    public static async Task<bool> HasUpdate()
    {
        if (File.Exists(_versionFile)) {
            using FileStream fs = File.OpenRead(_versionFile);
            if (JsonSerializer.Deserialize<string>(fs) is string version) {
                return await GithubHelper.HasUpdate(GITHUB_ORG, GITHUB_REPO, version);
            }
        }


        return false;
    }

    public static async Task<int> GetPluginUpdates()
    {
        return await GetPluginUpdates(PluginManager.GetPluginInfo());
    }

    public static async Task<int> GetPluginUpdates(IEnumerable<PluginInfo> plugins)
    {
        int updates = 0;

        foreach (var plugin in plugins.Where(x => !x.IsOnline && x.GitHubRepoId != -1)) {
            if (plugin.CanUpdate = await GithubHelper.HasUpdate(plugin.GitHubRepoId, plugin.Version)) {
                updates++;
            }
        }

        return updates;
    }

    public static async Task DownloadLauncher()
    {
        (var stream, _) = await GithubHelper.GetRelease(GITHUB_ORG, GITHUB_REPO, PlatformHelper.GetLauncherFileName());
        ZipArchive zip = new(stream);
        if (zip.Entries.FirstOrDefault(x => x.Name == LAUNCHER_NAME)?.Open() is Stream exe) {
            using FileStream fs = File.Create(LauncherPath);
            await exe.CopyToAsync(fs);
        }
    }
}
