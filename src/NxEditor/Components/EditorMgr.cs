using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using NxEditor.Generators;
using NxEditor.Models;
using NxEditor.PluginBase;
using NxEditor.PluginBase.Models;
using NxEditor.PluginBase.Services;

namespace NxEditor.Components;

public static class EditorMgr
{
    public static IEditor? Current => ShellDockFactory.Current() as IEditor;

    public static bool TryLoadEditor(IFileHandle handle)
    {
        try {
            LoadEditor(handle);
            return true;
        }
        catch (Exception ex) {
            App.Log(ex);
        }

        return false;
    }

    public static void LoadEditor(IFileHandle handle)
    {
        App.Log($"Processing {handle.Name}");

        if (ShellDockFactory.TryFocus(handle.Path ?? handle.Name, out IDockable? dock) && dock is IEditor) {
            return;
        }

        IFormatService service = ServiceLoader.Shared
            .RequestService(handle);

        if (service is IEditor editor) {
            _ = editor.Read();
            ShellDockFactory.AddDoc((Document)editor);
            if (handle.Path is not null) {
                RecentFiles.Shared.AddPath(handle.Path);
            }

            return;
        }

        throw new InvalidCastException($"Could not cast the found type of {service} to {typeof(Editor<,>).Name}");
    }
}
