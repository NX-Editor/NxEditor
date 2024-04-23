using NxEditor.Generators;
using NxEditor.PluginBase.Attributes;

namespace NxEditor.Models.Footers;

public class ShellViewFooter
{
    [Footer("fa-solid fa-floppy-disk", "Save")]
    public static void Save()
    {
        EditorManager.Shared.Current?.Save();
    }

    [Footer("fa-solid fa-cog", "Settings")]
    public static void Settings()
    {
        ShellDockFactory.AddDoc(ConfigViewModel.Shared);
    }
}
