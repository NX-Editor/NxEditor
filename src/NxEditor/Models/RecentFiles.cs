using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace NxEditor.Models;

public class RecentFiles : ObservableCollection<MenuItem>
{
    public static RecentFiles Shared { get; } = new();

    public void AddPath(string path)
    {
        // TODO: impl proper ordering

        MenuItem item = new()
        {
            Header = Path.GetFileName(path),
        };

        ToolTip.SetTip(item, path);
        Add(item);
    }
}
