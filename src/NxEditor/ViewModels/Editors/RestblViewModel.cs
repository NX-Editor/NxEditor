using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CsRestbl;
using Native.IO.Handles;
using NxEditor.Component;
using NxEditor.Core;
using NxEditor.Core.Utils;
using NxEditor.Helpers;
using NxEditor.Models;
using System.Collections.ObjectModel;

namespace NxEditor.ViewModels.Editors;

public partial class RestblViewModel : ReactiveEditor
{
    private readonly Restbl _table;
    private string[] _strings;
    public Restbl Table => _table;

    [ObservableProperty]
    private ObservableCollection<RestblEntry> _items = new();

    [ObservableProperty]
    private string _currentName = string.Empty;

    [ObservableProperty]
    private uint? _currentSize = 0;

    [ObservableProperty]
    private RestblEntry? _current;

    public RestblViewModel(string file, byte[] data, Action<byte[]>? setSource = null) : base(file, data, setSource)
    {
        _table = Restbl.FromBinary(_data);
        _strings = File.Exists(Config.Shared.DefaultHashTable) ? File.ReadAllLines(Config.Shared.DefaultHashTable) : Array.Empty<string>();
        ResetPinned();

        SupportedExtensions.Add("Yaml:*.yml;*.yaml|");
    }

    public void Search()
    {
        if (CurrentName == null) {
            ResetPinned();
            return;
        }

        if (_strings.Length == 0 && !Items.Select(x => x.Name).Contains(CurrentName) && TryGetEntry(CurrentName) is RestblEntry entry) {
            Items.Add(entry);
            return;
        }

        Items = new(_strings.Where(x => x.Contains(CurrentName)).Select(x => TryGetEntry(x)!).Where(x => x != null));
    }

    public async Task Import()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Import Hash Table", instanceBrowserKey: "import-hash-table");
        if (await dialog.ShowDialog() is string path && File.Exists(path)) {
            _strings = File.ReadAllLines(path);
            ResetPinned();
        }
    }

    public bool IsEntryValid(string action)
    {
        if (string.IsNullOrEmpty(CurrentName)) {
            App.Toast($"Name entry cannot be empty when {action}", $"Error {action}", NotificationType.Error);
            return false;
        }

        if (CurrentSize == null || CurrentSize < 0) {
            App.Toast($"Size entry cannot be empty when {action}", $"Error {action}", NotificationType.Error);
            return false;
        }

        return true;
    }

    public void SaveEntry()
    {
        if (!IsEntryValid("saving an entry")) {
            return;
        }

        // Check the string table first
        // as it's more accurate
        if (_table.NameTable.Contains(CurrentName)) {
            _table.NameTable[CurrentName] = (uint)CurrentSize!;
            return;
        }

        // Overwrite or add the value
        // in the CrcTable
        uint hash = Crc32.Compute(CurrentName);
        if (_table.CrcTable.Contains(hash)) {
            _table.CrcTable[hash] = (uint)CurrentSize!;
        }

        App.Toast("The entry could not be found. Did you mean to add (+) instead?", $"Warning", NotificationType.Warning);
    }

    public void AddEntry()
    {
        if (!IsEntryValid("adding an entry")) {
            return;
        }

        uint hash = Crc32.Compute(CurrentName);
        if (_table.CrcTable.Contains(hash)) {
            _table.NameTable[CurrentName] = (uint)CurrentSize!;
            return;
        }

        _table.CrcTable[hash] = (uint)CurrentSize!;
    }

    public void RemoveEntry()
    {
        if (!IsEntryValid("removing an entry")) {
            return;
        }

        if (Current != null) {
            if (Current.Hash is uint _hash && _table.CrcTable.Contains(_hash)) {
                _table.CrcTable.Remove(_hash);
                Items.Remove(Current);
            }
            else if (_table.NameTable.Contains(Current.Name)) {
                _table.NameTable.Remove(Current.Name);
                Items.Remove(Current);
            }
        }

        // Check the string table first
        // as it's more accurate
        if (_table.NameTable.Contains(CurrentName)) {
            _table.NameTable.Remove(CurrentName);
            return;
        }

        // If the entry is not in the string
        // table remove the hash entry
        uint hash = Crc32.Compute(CurrentName);
        if (_table.CrcTable.Contains(hash)) {
            _table.CrcTable.Remove(hash);
        }

        App.Toast("The entry could not be found", $"Error", NotificationType.Error);
    }

    public override void SaveAs(string path)
    {
        string ext = Path.GetExtension(path);

        if (ext is ".yml" or ".yaml") {
            using FileStream fs = File.Create(path);
            using StreamWriter writer = new(fs);

            writer.WriteLine("CrcTable:");
            foreach (var entry in Table.CrcTable) {
                writer.WriteLine($"  {entry.Key}: {entry.Value}");
            }

            writer.WriteLine("NameTable:");
            foreach (var entry in Table.NameTable) {
                writer.WriteLine($"  {entry.Key}: {entry.Value}");
            }
        }
        else {
            using DataMarshal handle = _table.ToBinary();

            Span<byte> data = (_compressed = path.EndsWith(".zs")) ? TotkZstd.Compress(path, handle) : handle;

            if (path == _file) {
                WriteToSource(data);
            }
            else {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                using FileStream fs = File.Create(path);
                fs.Write(data);
            }
        }

        ToastSaveSuccess(path);
    }

    public override void Undo()
    {
        base.Undo();
    }

    public override void Redo()
    {
        base.Redo();
    }

    public override void SelectAll()
    {
        base.SelectAll();
    }

    public override Task Cut()
    {
        return base.Cut();
    }

    public override Task Copy()
    {
        return base.Copy();
    }

    public override Task Paste()
    {
        return base.Paste();
    }

    public override void Find()
    {
        base.Find();
    }

    public override Task FindAndReplace()
    {
        return base.FindAndReplace();
    }

    public override bool HasChanged()
    {
        return true;
    }

    private void ResetPinned()
    {
        Items = new(_strings.Select(x => TryGetEntry(x)!).Where(x => x != null).ToList());
    }

    private RestblEntry? TryGetEntry(string name)
    {
        if (_table.NameTable.Contains(name)) {
            return new(name, null, _table.NameTable[name]);
        }

        uint hash = Crc32.Compute(name);
        if (_table.CrcTable.Contains(hash)) {
            return new(name, hash, _table.CrcTable[hash]);
        }

        return null;
    }
}
