using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using NxEditor.Attributes;
using NxEditor.Core.Extensions;
using NxEditor.Generators;
using NxEditor.PluginBase.Models;
using NxEditor.ViewModels.Dialogs;
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
            if (!EditorManager.Shared.TryLoadEditor(new FileHandle(path))) {
                // TODO: throw message dialog
            }
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

    [Menu("Recent", "File", icon: "fa-solid fa-clock-rotate-left", IsSeparator = true)]
    public static void Recent(string path)
    {
        EditorManager.Shared.TryLoadEditor(new FileHandle(path));
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
    // Tools

    [Menu("Settings", "Tools", "Ctrl + Alt + W", "fa-solid fa-cog")]
    public static void Settings()
    {
        ShellDockFactory.AddDoc(ConfigViewModel.Shared);
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
        foreach (var file in Directory.EnumerateFiles(Logger.LogsPath, "*.yml")) {
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
    // View

    [Menu("Show/Hide Console", "View", "Ctrl + F12", "fa-solid fa-terminal")]
    public static void ShowHideConsole()
    {
#if WIN_X64
        ConsoleExtension.SwapWindowMode();
#else
        App.Toast("This action is only supported on Win32 platforms", "OS Error", NotificationType.Error);
#endif
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
                        Command = new RelayCommand(async () => {
                            await BrowserExtension.OpenUrl("https://nx-editor.github.io");
                        })
                    }
                }
            }
        }.ShowAsync();
    }
}
