using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NxEditor.Core;
using NxEditor.Core.Components;
using NxEditor.Core.Models;
using NxEditor.Launcher.Helpers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace NxEditor.Launcher.ViewModels;

public record PluginInfoView(long Id, string Name, string Description);

public partial class ShellViewModel : ObservableObject
{
    private static readonly string _versionFile = Path.Combine(Config.AppFolder, "version.json");

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
    private ObservableCollection<PluginInfo> _plugins = new(PluginManager.GetPluginInfo());

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
        byte[] pluginsData = await GitHubRepo.GetAsset("NX-Editor", "Plugins", "public.json");
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
            await AppUpdater.Download();
        }

        await PluginUpdater.Download(Plugins);

        FoundUpdates = 0;
        IsLoading = false;
    }

    [RelayCommand]
    public async Task PrimaryButton()
    {
        IsLoading = true;
        if (IsEditorInstalled) {
            await PluginUpdater.Download(Plugins);
            Process.Start(
                Path.Combine(Config.AppFolder, "bin", AppPlatform.GetName())
            );

            IsLoading = false;
            return;
        }


        await AppUpdater.Download();
        await PluginUpdater.Download(Plugins);

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
        if (IsEditorInstalled && await AppUpdater.HasUpdate()) {
            _canUpdate = true;
            FoundUpdates++;
        }

        foreach (var plugin in Plugins.Where(x => !x.IsOnline && x.GitHubRepoId != -1)) {
            if (plugin.CanUpdate = await GitHubRepo.HasUpdate(plugin.GitHubRepoId, plugin.Version)) {
                FoundUpdates++;
            }
        }

        IsLoading = false;
    }
}
