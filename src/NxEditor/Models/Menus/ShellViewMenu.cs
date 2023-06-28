using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using NxEditor.Attributes;
using NxEditor.Component;
using NxEditor.Core;
using NxEditor.Core.Extensions;
using NxEditor.Dialogs;
using NxEditor.Generators;
using NxEditor.Helpers;
using NxEditor.PluginBase.Component;
using NxEditor.PluginBase.Models;
using NxEditor.ViewModels;
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
            if (!EditorMgr.TryLoadEditorSafe(new FileHandle(path))) {
                // TODO: throw message dialog
            }
        }
    }

    [Menu("Save", "File", "Ctrl + S", "fa-solid fa-floppy-disk", IsSeparator = true)]
    public static void Save()
    {
        EditorMgr.Current?.Save();
    }

    [Menu("Save As", "File", "Ctrl + Shift + S", "fa-regular fa-floppy-disk")]
    public static async Task SaveAs()
    {
        string[]? supportedExtensions = EditorMgr.Current?.ExportExtensions;
        StringBuilder extensionsFilterString = new();
        if (supportedExtensions?.Any() == true) {
            foreach (var ext in supportedExtensions) {
                extensionsFilterString.Append(ext);
            }
        }

        BrowserDialog dialog = new(BrowserMode.SaveFile, "Save File", $"{extensionsFilterString}Any File:*.*", Path.GetFileName(EditorMgr.Current?.Handle.Path), "save-file");
        if (await dialog.ShowDialog() is string path) {
            EditorMgr.Current?.Save(path);
        }
    }

    [Menu("Recent", "File", icon: "fa-solid fa-clock-rotate-left", IsSeparator = true)]
    public static void Recent(string path)
    {
        EditorMgr.TryLoadEditorSafe(new FileHandle(path));
    }

    [Menu("Clear Recent", "File", icon: "fa-regular fa-circle-xmark")]
    public static void ClearRecent()
    {
        RecentFiles.Shared.Clear();
    }

    [Menu("Exit", "File", "Alt + F4", "fa-solid fa-arrow-right-from-bracket", IsSeparator = true)]
    public static void Exit()
    {

    }

    // 
    // Edit

    [Menu("Undo", "Edit", "Ctrl + Z", "fa-solid fa-arrow-rotate-left")]
    public static void Undo()
    {
        EditorMgr.Current?.Undo();
    }

    [Menu("Redo", "Edit", "Ctrl + Shift + Z", "fa-solid fa-arrow-rotate-right")]
    public static void Redo()
    {
        EditorMgr.Current?.Redo();
    }

    [Menu("Select All", "Edit", "Ctrl + A", "fa-solid fa-object-group", IsSeparator = true)]
    public static void SelectAll()
    {
        EditorMgr.Current?.SelectAll();
    }

    [Menu("Cut", "Edit", "Ctrl + X", "fa-solid fa-scissors", IsSeparator = true)]
    public static async Task Cut()
    {
        await (EditorMgr.Current?.Cut()).SafeInvoke();
    }

    [Menu("Copy", "Edit", "Ctrl + C", "fa-solid fa-copy")]
    public static async Task Copy()
    {
        await (EditorMgr.Current?.Copy()).SafeInvoke();
    }

    [Menu("Paste", "Edit", "Ctrl + V", "fa-solid fa-paste")]
    public static async Task Paste()
    {
        await (EditorMgr.Current?.Paste()).SafeInvoke();
    }

    [Menu("Find", "Edit", "Ctrl + F", "fa-solid fa-magnifying-glass", IsSeparator = true)]
    public static void Find()
    {
        EditorMgr.Current?.Find();
    }

    [Menu("Find & Replace", "Edit", "Ctrl + H", "fa-solid fa-arrows-turn-to-dots")]
    public static async Task FindAndReplace()
    {
        await (EditorMgr.Current?.FindAndReplace()).SafeInvoke();
    }

    // 
    // Tools

    [Menu("Settings", "Tools", "Ctrl + Alt + W", "fa-solid fa-cog")]
    public static void Settings()
    {
        ShellDockFactory.AddDoc<SettingsViewModel>();
    }

    [Menu("Open Logs", "Tools", "Ctrl + L", "fa-solid fa-file-circle-check", IsSeparator = true)]
    public static void OpenLogs()
    {
        ShellDockFactory.AddDoc(LogsViewModel.Shared);
    }

    [Menu("Open Logs Folder", "Tools", "Ctrl + Alt + L", "fa-solid fa-arrow-up-right-from-square")]
    public static async Task OpenLogsFolder()
    {
        await BrowserExtension.OpenUrl(Logger.LogsPath);
    }

    [Menu("Clear Logs Folder", "Tools", "Ctrl + F7", "fa-solid fa-file-circle-xmark")]
    public static void ClearLogsFolder()
    {
        foreach (var file in Directory.EnumerateFiles(Logger.LogsPath, "*.log")) {
            if (file != Logger.CurrentLog) {
                File.Delete(file);
            }
        }

        App.Toast("Logs folder cleared successfully", "Clear Logs Folder", NotificationType.Success);
    }

    [Menu("Clear Editor Cache", "Tools", "Ctrl + F8", "fa-regular fa-circle-xmark", IsSeparator = true)]
    public static void ClearEditorCache()
    {
        if (Directory.Exists(EditorConfig.CacheFolder)) {
            Directory.Delete(EditorConfig.CacheFolder, true);
        }

        App.Toast("Editor cache cleared successfully", "Clear Editor Cache", NotificationType.Success);
    }

    // 
    // About

    [Menu("Help", "About", "F1", "fa-solid fa-handshake-angle")]
    public static void Help()
    {
        throw new NotImplementedException();
    }

    [Menu("About", "About", "F12", "fa-solid fa-circle-info")]
    public static async Task About()
    {
        await new ContentDialog {
            Title = "About",
            Content = new StackPanel {
                Spacing = 5,
                Children = {
                    new TextBlock {
                        Text = "Nintendo Extended Editor (NX-Editor)",
                        FontWeight = Avalonia.Media.FontWeight.Bold
                    },
                    new TextBlock {
                        Text = """
                        A general editor for editing first-party
                        Nintendo formats. Primary aimed at support
                        for Tears of the Kingdom files and more
                        modern Nintendo Switch files.
                        """
                    },
                    new Button {
                        Content = "nx-editor.github.io",
                        Classes = { "Hyperlink" },
                        Command = ReactiveCommand.Create(async () => {
                            await BrowserExtension.OpenUrl("https://nx-editor.github.io");
                        })
                    }
                }
            }
        }.ShowAsync();
    }
}
