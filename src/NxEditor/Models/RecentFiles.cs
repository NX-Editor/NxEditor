using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace NxEditor.Models;

public class RecentFiles : ObservableCollection<MenuItem>
{
    public static readonly string _path = Path.Combine(Config.AppFolder, "recent.json");

    public static RecentFiles Shared { get; } = new();

    public void AddPath(string path)
    {
        if (this.FirstOrDefault(x => (string?)x.Tag == path) is MenuItem target) {
            Remove(target);
            InsertItem(0, target);
            goto Save;
        }

        if (Count >= 7) {
            RemoveAt(Count - 1);
        }

        InsertItem(0, FromPath(path));

    Save:
        Save();
    }

    public void Load()
    {
        if (!File.Exists(_path)) {
            return;
        }

        using FileStream fs = File.OpenRead(_path);
        List<string> paths = JsonSerializer.Deserialize<List<string>>(fs) ?? new();

        foreach (var path in paths.Where(File.Exists)) {
            Add(FromPath(path));
        }
    }

    private void Save()
    {
        using FileStream fs = File.Create(_path);
        JsonSerializer.Serialize(fs, this.Select(x => x.Tag));
    }

    private static MenuItem FromPath(string path)
    {
        MenuItem item = new() {
            Header = Path.GetFileName(path),
            Tag = path
        };

        ToolTip.SetTip(item, path);
        return item;
    }
}
