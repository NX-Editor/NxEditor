using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace ExKingEditor.Models;

public class RecentList : ObservableCollection<MenuItem>
{
    public void AddPath(string path)
    {
        // TODO: impl proper ordering

        MenuItem item = new() {
            Header = Path.GetFileName(path),
        };

        ToolTip.SetTip(item, path);
        Add(item);
    }
}
