using Dock.Model.Core;
using ExKingEditor.Core;
using ExKingEditor.Core.Extensions;
using ExKingEditor.Generators;
using System.Runtime.CompilerServices;

namespace ExKingEditor.Models;

public static class EditorMgr
{
    private static Dictionary<string, string> _editors = ResourceExtension.Parse<App, Dictionary<string, string>>("Editors.json")!;

    public static ReactiveEditor? Current => ShellDockFactory.Current() as ReactiveEditor;

    public static bool TryLoadEditorSafe(string path, byte[]? data = null)
    {
        try {
            return TryLoadEditor(path, data);
        }
        catch (Exception ex) {
            App.Log(ex);
        }

        return false;
    }

    public static bool TryLoadEditor(string path, byte[]? data = null)
    {
        App.Log($"Processing {Path.GetFileName(path)}");

        if (!CanEdit(path)) {
            throw new NotSupportedException($"Unsupported file type {Path.GetExtension(path)}");
        }

        if (ShellDockFactory.TryFocus(path, out IDockable? dock) && dock is ReactiveEditor) {
            return true;
        }

        Stream? fs = null;
        if (data == null) {
            fs = File.Open(path, FileMode.Open);
            data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
        }

        // Decompress if necessary
        data = path.EndsWith(".zs") ? TotkZstd.Decompress(path, data).ToArray() : data;

        string ext = GetExt(path);
        object? instance = Activator.CreateInstance(Type.GetType($"{nameof(ExKingEditor)}.ViewModels.Editors.{_editors[ext]}")
            ?? throw new InvalidDataException($"Invalid editor type for '{ext}' - '{_editors[ext]}' was not found"), path, data, fs);

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
