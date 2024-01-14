using NxEditor.Core.Models;
using System.IO.Compression;

namespace NxEditor.Launcher.Helpers;

public class PluginUpdater
{
    public static async Task InstallAll(IEnumerable<PluginInfo> plugins)
    {
        foreach (var plugin in plugins.Where(x => (x.IsOnline || x.CanUpdate) && x.IsEnabled)) {
            await Install(plugin);
            plugin.IsOnline = plugin.CanUpdate = false;
        }
    }

    private static async Task Install(PluginInfo info)
    {
        if (info.GitHubRepoId == -1) {
            return;
        }

        (Stream stream, string tag) = await GitHubRepo.GetRelease(info.GitHubRepoId, AppPlatform.GetOsFileName());
        ZipArchive archive = new(stream);
        Directory.CreateDirectory(info.Folder);
        archive.ExtractToDirectory(info.Folder, true);

        string meta = Path.Combine(info.Folder, "meta.json");

        if (File.Exists(meta)) {
            info = PluginInfo.FromPath(meta);
            info.Version = tag;
            info.Serialize();
        }
    }
}
