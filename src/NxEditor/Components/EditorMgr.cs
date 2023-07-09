using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using NxEditor.Core;
using NxEditor.Generators;
using NxEditor.Models;
using NxEditor.PluginBase;
using NxEditor.PluginBase.Component;
using NxEditor.PluginBase.Models;
using NxEditor.PluginBase.Services;

namespace NxEditor.Components;

public static class EditorMgr
{
    public static IEditor? Current => ShellDockFactory.Current() as IEditor;

    public static bool TryLoadEditorSafe(IFileHandle handle)
    {
        try {
            return TryLoadEditor(handle);
        }
        catch (Exception ex) {
            App.Log(ex);
        }

        return false;
    }

    public static bool TryLoadEditor(IFileHandle handle)
    {
        App.Log($"Processing {handle.Name}");

        if (Config.Shared.UseSingleFileLock && ShellDockFactory.TryFocus(handle.Name, out IDockable? dock) && dock is IEditor) {
            return true;
        }

        IFormatService service = ServiceLoader.Shared
            .RequestService(handle);

        if (service is IEditor editor) {
            editor.Read();
            ShellDockFactory.AddDoc((Document)editor);
            if (handle.Path is not null) {
                RecentFiles.Shared.AddPath(handle.Path);
            }

            return true;
        }

        throw new InvalidCastException($"Could not cast the found type of {service} to {typeof(Editor<,>).Name}");
    }
}
