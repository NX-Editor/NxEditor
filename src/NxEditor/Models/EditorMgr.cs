using Dock.Model.Core;
using NxEditor.Core;
using NxEditor.Core.Extensions;
using NxEditor.Generators;
using System.Runtime.CompilerServices;

namespace NxEditor.Models;

public static class EditorMgr
{
    private static Dictionary<string, string> _editors = ResourceExtension.Parse<App, Dictionary<string, string>>("Editors.json")!;

    public static ReactiveEditor? Current => ShellDockFactory.Current() as ReactiveEditor;

    public static bool TryLoadEditorSafe(string path, byte[]? data = null, Action<byte[]>? setSource = null)
    {
        try {
            return TryLoadEditor(path, data, setSource);
        }
        catch (Exception ex) {
            App.Log(ex);
        }

        return false;
    }

    public static bool TryLoadEditor(string path, byte[]? data = null, Action<byte[]>? setSource = null)
    {
        App.Log($"Processing {Path.GetFileName(path)}");

        if (ExConfig.Shared.UseSingleFileLock && ShellDockFactory.TryFocus(path, out IDockable? dock) && dock is ReactiveEditor) {
            return true;
        }

        data ??= File.ReadAllBytes(path);

        // Decompress if necessary
        data = path.EndsWith(".zs") ? TotkZstd.Decompress(path, data).ToArray() : data;

        string ext = GetExt(path);
        string? editorName = _editors.TryGetValue(ext, out editorName) ? editorName : "TextViewModel";

        object? instance = Activator.CreateInstance(Type.GetType($"{nameof(NxEditor)}.ViewModels.Editors.{editorName}")
            ?? throw new InvalidDataException($"Invalid editor type for '{ext}' - '{editorName}' was not found"), path, data, setSource);

        if (instance is ReactiveEditor editor) {
            ShellDockFactory.AddDoc(editor);
            StateMgr.Shared.Recent.AddPath(path);

            return true;
        }

        throw new InvalidCastException($"Could not cast the found type of {instance} to {nameof(ReactiveEditor)}");
    }

    public static bool CanEdit(string file, out string? editor)
    {
        return _editors.TryGetValue(GetExt(file), out editor);
    }

    public static bool CanEdit(string file)
    {
        return _editors.ContainsKey(GetExt(file));
    }

    public static bool? ReloadEditorsConfig()
    {
        _editors = ResourceExtension.Parse<App, Dictionary<string, string>>("Editors.json")!;
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetExt(string file)
    {
        if (file.EndsWith(".zs")) {
            file = file[..^3];
        }

        return Path.GetExtension(file);
    }
}
