using Avalonia.Controls;
using ExKingEditor.Attributes;
using ExKingEditor.Core;
using ExKingEditor.Core.Extensions;
using ExKingEditor.Generators;
using ExKingEditor.Helpers;
using ExKingEditor.ViewModels;

namespace ExKingEditor.Models;

public class ShellMenu
{
    // 
    // File

    [Menu("Open File", "File", "Ctrl + O", "fa-regular fa-folder-open")]
    public static async Task OpenFile()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Open File", "Any File:*.*", instanceBrowserKey: "open-file");
        if (await dialog.ShowDialog() is string path) {
            if (!EditorMgr.TryLoadEditorSafe(path, out _)) {
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
        BrowserDialog dialog = new(BrowserMode.SaveFile, "Save File", "Any File:*.*", Path.GetFileName(EditorMgr.Current?.FilePath), "save-file");
        if (await dialog.ShowDialog() is string path) {
            EditorMgr.Current?.SaveAs(path);
        }
    }

    [Menu("Recent", "File", icon: "fa-solid fa-clock-rotate-left", IsSeparator = true)]
    public static void Recent(string path)
    {
        EditorMgr.TryLoadEditorSafe(path, out _);
    }

    [Menu("Clear Recent", "File", icon: "fa-regular fa-circle-xmark")]
    public static void ClearRecent()
    {
        StateMgr.Shared.Recent.Clear();
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
    public static void Cut()
    {
        EditorMgr.Current?.Cut();
    }

    [Menu("Copy", "Edit", "Ctrl + C", "fa-solid fa-copy")]
    public static void Copy()
    {
        EditorMgr.Current?.Copy();
    }

    [Menu("Paste", "Edit", "Ctrl + V", "fa-solid fa-paste")]
    public static void Paste()
    {
        EditorMgr.Current?.Paste();
    }

    [Menu("Find", "Edit", "Ctrl + F", "fa-solid fa-magnifying-glass", IsSeparator = true)]
    public static void Find()
    {
        EditorMgr.Current?.Find();
    }

    [Menu("Find & Replace", "Edit", "Ctrl + H", "fa-solid fa-arrows-turn-to-dots")]
    public static void FindAndReplace()
    {
        EditorMgr.Current?.FindAndReplace();
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

    [Menu("Clear Logs Folder", "Tools", icon: "fa-solid fa-file-circle-xmark")]
    public static void ClearLogsFolder()
    {
        foreach (var file in Directory.EnumerateFiles(Logger.LogsPath, "*.log")) {
            if (file != Logger.CurrentLog) {
                File.Delete(file);
            }
        }
    }

    // 
    // About

    [Menu("Help", "About", "F1", "fa-solid fa-handshake-angle")]
    public static void Help()
    {
        throw new NotImplementedException();
    }

    [Menu("Credits", "About", "F12", "fa-solid fa-user-check")]
    public static void Credits()
    {
        throw new NotImplementedException();
    }
}
