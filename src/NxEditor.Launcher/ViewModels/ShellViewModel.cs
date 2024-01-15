using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Markdown.Avalonia;
using NxEditor.Core.Components;
using NxEditor.Core.Helpers;
using NxEditor.Core.Models;
using NxEditor.Launcher.Helpers;
using NxEditor.PluginBase;
using NxEditor.PluginBase.Common;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace NxEditor.Launcher.ViewModels;

public record PluginInfoView(long Id, string Name, string Description);

public partial class ShellViewModel : ObservableObject
{
    private bool _canUpdate = false;

    [ObservableProperty]
    private bool _isEditorInstalled = AppUpdater.IsInstalled;

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
        byte[] pluginsData = await GithubHelper.GetAsset("NX-Editor", "Plugins", "public.json");
        Dictionary<string, PluginInfoView> plugins = JsonSerializer.Deserialize<Dictionary<string, PluginInfoView>>(pluginsData)!;

        foreach ((var id, var plugin) in plugins) {
            if (Plugins.FirstOrDefault(x => x.Name == plugin.Name) is PluginInfo existing) {
                existing.GitHubRepoId = plugin.Id;
                continue;
            }

            Plugins.Add(new() {
                Description = plugin.Description,
                Folder = Path.Combine(GlobalConfig.Shared.StorageFolder, "plugins", id),
                GitHubRepoId = plugin.Id,
                IsOnline = true,
                Name = plugin.Name
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
            await AppUpdater.Install();
        }

        await PluginUpdater.InstallAll(Plugins);

        FoundUpdates = 0;
        IsLoading = false;
    }

    [RelayCommand]
    public async Task PrimaryButton()
    {
        IsLoading = true;
        if (IsEditorInstalled) {
            await PluginUpdater.InstallAll(Plugins);
            Process.Start(
                Path.Combine(GlobalConfig.StaticPath, "bin", PlatformHelper.GetExecutableName())
            );

            IsLoading = false;
            return;
        }


        await AppUpdater.Install();
        await PluginUpdater.InstallAll(Plugins);

        IsEditorInstalled = true;
        PrimaryButtonContent = "Open NX Editor";
        IsLoading = false;
        FoundUpdates = 0;
    }

    [RelayCommand]
    public async Task Uninstall()
    {
        IsLoading = true;

        await AppUpdater.Uninstall();

        Plugins.Clear();
        await ShowOnlinePlugins();

        IsEditorInstalled = false;
        PrimaryButtonContent = "Install NX Editor";
        IsLoading = false;
    }

    [RelayCommand]
    public static Task Exit()
    {
        Environment.Exit(0);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private static async Task ShowHelp()
    {
        using Stream stream = AssetLoader.Open(new("avares://NxEditor.Launcher/Assets/Readme.md"));
        int strlen = (int)stream.Length;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(strlen);
        stream.Read(buffer, 0, buffer.Length);
        string markdown = Encoding.UTF8.GetString(buffer[..strlen]);
        ArrayPool<byte>.Shared.Return(buffer);

        DialogBox dialog = new() {
            Title = $"NX Editor, version {App.Version}",
            IsSecondaryButtonVisible = false,
            Content = new MarkdownScrollViewer {
                Markdown = markdown,
                MaxHeight = 200,
                MaxWidth = 500
            },
        };

        await dialog.ShowAsync();
    }

    private async Task CheckForUpdates()
    {
        if (IsEditorInstalled && await UpdateHelper.HasUpdate()) {
            _canUpdate = true;
            FoundUpdates++;
        }

        FoundUpdates += await UpdateHelper.GetPluginUpdates(Plugins);
        IsLoading = false;
    }
}
