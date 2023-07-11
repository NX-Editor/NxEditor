using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NxEditor.Core;
using NxEditor.Core.Components;
using NxEditor.Core.Models;
using Octokit;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace NxEditor.Launcher.ViewModels;

public record PluginInfoView(long Id, string Name, string Description);

public partial class ShellViewModel : ObservableObject
{
    private const long RepoId = 601421384;

    private static readonly string _versionFile = Path.Combine(Config.AppFolder, "version.json");
    private static readonly GitHubClient _githubClient = new(new ProductHeaderValue("NxEditor.Launcher.Updater"));

    private bool _canUpdate = false;

    [ObservableProperty]
    private bool _isEditorInstalled = File.Exists(_versionFile);

    [ObservableProperty]
    private string _primaryButtonContent = "Install NX Editor";

    [ObservableProperty]
    private int _foundUpdates = 0;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private ObservableCollection<PluginInfo> _plugins = PluginManager.GetPluginInfo();

    public ShellViewModel()
    {
        PrimaryButtonContent = IsEditorInstalled ? "Open NX Editor" : "Install NX Editor";

        if (Plugins.Count > 0 || IsEditorInstalled) {
            IsLoading = true;
            _ = CheckForUpdates();
        }

        _ = ShowOnlinePlugins();
    }

    [RelayCommand]
    public async Task ShowOnlinePlugins()
    {
        byte[] pluginsData = await _githubClient.Repository.Content.GetRawContent("NX-Editor", "Plugins", "public.json");
        Dictionary<string, PluginInfoView> plugins = JsonSerializer.Deserialize<Dictionary<string, PluginInfoView>>(pluginsData)!;

        IEnumerable<string> loaded = Plugins.Select(x => x.Name);

        foreach (var plugin in plugins.Where(x => !loaded.Contains(x.Value.Name))) {
            Plugins.Add(new() {
                Description = plugin.Value.Description,
                Folder = Path.Combine(Config.AppFolder, "plugins", plugin.Key),
                GitHubRepoId = plugin.Value.Id,
                IsOnline = true,
                Name = plugin.Value.Name
            });
        }
    }

    [RelayCommand]
    public async Task InstallUpdates()
    {
        if (FoundUpdates <= 0) {
            return;
        }

        IsLoading = true;

        if (_canUpdate) {
            await InstallBinary();
        }

        await InstallPlugins();
        FoundUpdates = 0;
        IsLoading = false;
    }

    [RelayCommand]
    public async Task PrimaryButton()
    {
        IsLoading = true;
        if (IsEditorInstalled) {
            await InstallPlugins();
            Process.Start(
                Path.Combine(Config.AppFolder, "bin", Environment.OSVersion.Platform == PlatformID.Unix ? "nxe" : "nxe.exe")
            );

            IsLoading = false;
            return;
        }


        await InstallBinary();

        await InstallPlugins();

        IsEditorInstalled = true;
        PrimaryButtonContent = "Open NX Editor";
        IsLoading = false;
        FoundUpdates = 0;
    }

    [RelayCommand]
    public static Task Uninstall()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    public static Task Exit()
    {
        Environment.Exit(0);
        return Task.CompletedTask;
    }

    private async Task CheckForUpdates()
    {
        if (IsEditorInstalled) {
            using FileStream fs = File.OpenRead(_versionFile);
            string version = JsonSerializer.Deserialize<string>(fs)!;

            if (await HasUpdate(RepoId, version)) {
                _canUpdate = true;
                FoundUpdates++;
            }
        }

        foreach (var plugin in Plugins.Where(x => !x.IsOnline && x.GitHubRepoId != -1)) {
            if (plugin.CanUpdate = await HasUpdate(plugin.GitHubRepoId, plugin.Version)) {
                FoundUpdates++;
            }
        }

        IsLoading = false;
    }

    private static async Task InstallBinary()
    {
        string appName = Environment.OSVersion.Platform == PlatformID.Unix ? "nxe" : "nxe.exe";
        string binPath = Path.Combine(Config.AppFolder, "bin");
        string appPath = Path.Combine(binPath, appName);

        Directory.CreateDirectory(binPath);

        IReadOnlyList<Release> releases = await _githubClient.Repository.Release.GetAll("NX-Editor", "NxEditor");
        Release latest = releases[0];

        using (HttpClient client = new()) {
            Stream stream = await client.GetStreamAsync(latest.Assets.Where(x => x.Name == appName).First().BrowserDownloadUrl);

            using FileStream fs = File.Create(appPath);
            await stream.CopyToAsync(fs);
        }


        using (FileStream fs = File.Create(_versionFile)) {
            await JsonSerializer.SerializeAsync(fs, latest.TagName);
        }

        if (Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) is string pathEnv && !pathEnv.Contains(binPath)) {
            Environment.SetEnvironmentVariable("PATH",
                $"{pathEnv}{binPath}{(Environment.OSVersion.Platform == PlatformID.Unix ? ":" : ";")}",
                EnvironmentVariableTarget.User);
        }
    }

    private async Task InstallPlugins()
    {
        foreach (var plugin in Plugins.Where(x => (x.IsOnline || x.CanUpdate) && x.IsEnabled)) {
            await InstallPlugin(plugin);
            plugin.IsOnline = plugin.CanUpdate = false;
        }
    }

    private static async Task InstallPlugin(PluginInfo info)
    {
        if (info.GitHubRepoId == -1) {
            return;
        }

        IReadOnlyList<Release> releases = await _githubClient.Repository.Release.GetAll(info.GitHubRepoId);
        if (releases.Count > 0 && releases[0].Assets.FirstOrDefault(x => x.Name == (Environment.OSVersion.Platform == PlatformID.Unix ? "linux-x64.zip" : "win-x64.zip")) is ReleaseAsset asset) {
            using HttpClient client = new();
            Stream stream = await client.GetStreamAsync(asset.BrowserDownloadUrl);

            ZipArchive arc = new(stream);
            Directory.CreateDirectory(info.Folder);
            arc.ExtractToDirectory(info.Folder, true);

            string meta = Path.Combine(info.Folder, "meta.json");
            if (File.Exists(meta)) {
                info = PluginInfo.FromPath(meta);
                info.Version = releases[0].TagName;
                info.Serialize();
            }
        }
    }

    private static async Task<bool> HasUpdate(long repoId, string currentTag)
    {
        IReadOnlyList<Release> releases = await _githubClient.Repository.Release.GetAll(repoId);
        return releases.Count > 0 && releases[0].TagName != currentTag;
    }
}
