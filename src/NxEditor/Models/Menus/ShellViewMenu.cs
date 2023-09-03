using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using NxEditor.Core.Extensions;
using NxEditor.Generators;
using NxEditor.PluginBase.Attributes;
using NxEditor.PluginBase.Common;
using NxEditor.PluginBase.Models;
using System.Collections.ObjectModel;
using System.Text;

namespace NxEditor.Models.Menus;

public class ShellViewMenu
{
    // 
    // File

    [Menu("Open File", "File", "Ctrl + O", "fa-regular fa-folder-open")]
    public static async Task OpenFile()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Open File", "Any File:*.*", instanceBrowserKey: "open-file");
        if (await dialog.ShowDialog() is string path) {
            await EditorManager.Shared.TryLoadEditor(new FileHandle(path));
        }
    }

    [Menu("Save", "File", "Ctrl + S", "fa-solid fa-floppy-disk", IsSeparator = true)]
    public static void Save()
    {
        EditorManager.Shared.Current?.Save();
    }

    [Menu("Save As", "File", "Ctrl + Shift + S", "fa-regular fa-floppy-disk")]
    public static async Task SaveAs()
    {
        string[]? supportedExtensions = EditorManager.Shared.Current?.ExportExtensions;
        StringBuilder extensionsFilterString = new();
        if (supportedExtensions?.Any() == true) {
            foreach (var ext in supportedExtensions) {
                extensionsFilterString.Append(ext);
            }
        }

        BrowserDialog dialog = new(BrowserMode.SaveFile, "Save File", $"{extensionsFilterString}Any File:*.*", Path.GetFileName(EditorManager.Shared.Current?.Handle.FilePath), "save-file");
        if (await dialog.ShowDialog() is string path) {
            EditorManager.Shared.Current?.Save(path);
        }
    }

    [Menu("Recent", "File", icon: "fa-solid fa-clock-rotate-left", IsSeparator = true, GetCollectionMethodName = "GetRecentFilesCollection")]
    public static async Task Recent(MenuItem item)
    {
        if (item.Tag is string path && File.Exists(path)) {
            await EditorManager.Shared.TryLoadEditor(new FileHandle(path));
        }
    }

    [Menu("Clear Recent", "File", icon: "fa-regular fa-circle-xmark")]
    public static void ClearRecent()
    {
        RecentFiles.Shared.Clear();
    }

    [Menu("Clear Editor Cache", "File", "Ctrl + F8", "fa-regular fa-circle-xmark", IsSeparator = true)]
    public static void ClearEditorCache()
    {
        if (Directory.Exists(EditorConfig.CacheFolder)) {
            Directory.Delete(EditorConfig.CacheFolder, true);
        }

        App.Toast("Editor cache cleared successfully", "Clear Editor Cache", NotificationType.Success);
    }

    [Menu("Open Logs Folder", "File", "Ctrl + Alt + L", "fa-solid fa-arrow-up-right-from-square", IsSeparator = true)]
    public static async Task OpenLogsFolder()
    {
        await BrowserExtension.OpenUrl(Logger.LogsPath);
    }

    [Menu("Clear Logs Folder", "File", "Ctrl + F7", "fa-solid fa-file-circle-xmark")]
    public static void ClearLogsFolder()
    {
        foreach (var file in Directory.EnumerateFiles(Logger.LogsPath, "*.yml")) {
            if (file != Logger.CurrentLog) {
                File.Delete(file);
            }
        }

        App.Toast("Logs folder cleared successfully", "Clear Logs Folder", NotificationType.Success);
    }

    [Menu("Exit", "File", "Alt + F4", "fa-solid fa-arrow-right-from-bracket", IsSeparator = true)]
    public static void Exit()
    {
        Environment.Exit(0);
    }

    // 
    // Edit

    [Menu("Undo", "Edit", "Ctrl + Z", "fa-solid fa-arrow-rotate-left")]
    public static void Undo()
    {
        EditorManager.Shared.Current?.Undo();
    }

    [Menu("Redo", "Edit", "Ctrl + Shift + Z", "fa-solid fa-arrow-rotate-right")]
    public static void Redo()
    {
        EditorManager.Shared.Current?.Redo();
    }

    [Menu("Select All", "Edit", "Ctrl + A", "fa-solid fa-object-group", IsSeparator = true)]
    public static void SelectAll()
    {
        EditorManager.Shared.Current?.SelectAll();
    }

    [Menu("Cut", "Edit", "Ctrl + X", "fa-solid fa-scissors", IsSeparator = true)]
    public static async Task Cut()
    {
        await (EditorManager.Shared.Current?.Cut()).SafeInvoke();
    }

    [Menu("Copy", "Edit", "Ctrl + C", "fa-solid fa-copy")]
    public static async Task Copy()
    {
        await (EditorManager.Shared.Current?.Copy()).SafeInvoke();
    }

    [Menu("Paste", "Edit", "Ctrl + V", "fa-solid fa-paste")]
    public static async Task Paste()
    {
        await (EditorManager.Shared.Current?.Paste()).SafeInvoke();
    }

    [Menu("Find", "Edit", "Ctrl + F", "fa-solid fa-magnifying-glass", IsSeparator = true)]
    public static void Find()
    {
        EditorManager.Shared.Current?.Find();
    }

    [Menu("Find & Replace", "Edit", "Ctrl + H", "fa-solid fa-arrows-turn-to-dots")]
    public static async Task FindAndReplace()
    {
        await (EditorManager.Shared.Current?.FindAndReplace()).SafeInvoke();
    }

    //
    // View

    [Menu("Settings", "View", "Ctrl + Alt + W", "fa-solid fa-cog")]
    public static void Settings()
    {
        ShellDockFactory.AddDoc(ConfigViewModel.Shared);
    }

    [Menu("Show/Hide Console", "View", "Ctrl + F11", "fa-solid fa-terminal", IsSeparator = true)]
    public static void ShowHideConsole()
    {
        if (OperatingSystem.IsWindows()) {
            ConsoleExtension.SwapWindowMode();
        }
        else {
            App.Toast("This action is only supported on Win32 platforms", "OS Error", NotificationType.Error);
        }
    }

    [Menu("Open Logs", "View", "Ctrl + L", "fa-solid fa-file-circle-check")]
    public static void OpenLogs()
    {
        ShellDockFactory.AddDoc(LogsViewModel.Shared);
    }

    // 
    // About

    [Menu("Help", "About", "F1", "fa-solid fa-handshake-angle")]
    public static async Task Help()
    {
        await new DialogBox {
            Title = "Help!",
            IsSecondaryButtonVisible = false,
            Content = new StackPanel {
                Spacing = 5,
                Children = {
                    new TextBlock {
                        Margin = new(0, 0, 0, 15),
                        Text = """
                        Eventually I'll have documentation, but for now
                        you can just message me on Discord or GitHub.
                        """
                    },
                    new StackPanel {
                        Orientation = Orientation.Horizontal,
                        Spacing = 15,
                        Children = {
                            new Button {
                                Content = "Discord Server",
                                Classes = { "Hyperlink" },
                                Command = new RelayCommand(async () => {
                                    await BrowserExtension.OpenUrl("https://discord.gg/8Saj6tTkNB");
                                })
                            },
                            new Button {
                                Content = "NX-Editor GitHub",
                                Classes = { "Hyperlink" },
                                Command = new RelayCommand(async () => {
                                    await BrowserExtension.OpenUrl("https://github.com/orgs/NX-Editor/discussions/new/choose");
                                })
                            }
                        }
                    }
                }
            }
        }.ShowAsync();
    }

    [Menu("About", "About", "Ctrl + F12", "fa-solid fa-circle-info")]
    public static async Task About()
    {
        await new DialogBox {
            Title = "About",
            IsSecondaryButtonVisible = false,
            Content = new StackPanel {
                Spacing = 5,
                Children = {
                    new TextBlock {
                        Text = "Nintendo Extended Editor (NX-Editor)",
                        FontWeight = Avalonia.Media.FontWeight.Bold,
                        FontSize = 18
                    },
                    new TextBlock {
                        Text = """
                        A general purpose file editor for first-party
                        Nintendo formats. Primarily aimed at support
                        for Tears of the Kingdom files and more
                        modern Nintendo Switch files.

                        © 2023 Arch Leaders, AGPL-3.0
                        """
                    },
                    new Button {
                        Content = "nx-editor.github.io",
                        Classes = { "Hyperlink" },
                        Command = new RelayCommand(async () => {
                            await BrowserExtension.OpenUrl("https://nx-editor.github.io");
                        })
                    }
                }
            }
        }.ShowAsync();
    }

    public static ObservableCollection<MenuItem> GetRecentFilesCollection()
        => RecentFiles.Shared;
}
