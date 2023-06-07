using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CsRestbl;
using Native.IO.Handles;
using NxEditor.Core;
using NxEditor.Core.Utils;
using NxEditor.Models;
using System.Collections.ObjectModel;

namespace NxEditor.ViewModels.Editors;

public partial class RestblViewModel : ReactiveEditor
{
    private readonly Restbl _table;
    public Restbl Table => _table;

    [ObservableProperty]
    private ObservableCollection<RestblEntry> _pinned = new();

    [ObservableProperty]
    private string _currentName = string.Empty;

    [ObservableProperty]
    private uint? _currentSize = 0;

    public RestblViewModel(string file, byte[] data, Action<byte[]>? setSource = null) : base(file, data, setSource)
    {
        _table = Restbl.FromBinary(_data);
        
        SupportedExtensions.Add("Yaml:*.yml;*.yaml|");
    }

    public void Search()
    {
        for (int i = 0; i < Pinned.Count; i++) {
            if (!Pinned[i].IsPinned) {
                Pinned.RemoveAt(i);
                i--;
            }
        }

        if (!Pinned.Select(x => x.Name).Contains(CurrentName)) {
            if (_table.NameTable.Contains(CurrentName)) {
                Pinned.Add(new(CurrentName, null, _table.NameTable[CurrentName]));
                return;
            }

            uint hash = Crc32.Compute(CurrentName);
            if (_table.CrcTable.Contains(hash)) {
                Pinned.Add(new(CurrentName, hash, _table.CrcTable[hash]));
                return;
            }
        }
    }

    public bool IsEntryValid(string action)
    {
        if (string.IsNullOrEmpty(CurrentName)) {
            App.Toast("Name entry cannot be empty when saving an entry", $"Error {action}", NotificationType.Error);
            return false;
        }
        
        if (CurrentSize == null || CurrentSize < 0) {
            App.Toast("Size entry cannot be empty when saving an entry", $"Error {action}", NotificationType.Error);
            return false;
        }

        return true;
    }

    public void SaveEntry()
    {
        if (!IsEntryValid("saving entry")) {
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
        _table.CrcTable[hash] = (uint)CurrentSize!;
    }

    public void AddEntry()
    {
        if (!IsEntryValid("adding entry")) {
            return;
        }

        uint hash = Crc32.Compute(CurrentName);
        if (_table.CrcTable.Contains(hash)) {
            _table.NameTable[CurrentName] = (uint)CurrentSize!;
            return;
        }

        _table.CrcTable[hash] = (uint)CurrentSize!;
    }

    public override void OnSelected()
    {
        base.OnSelected();
        App.Desktop?.DisableGlobalShortcuts("Edit");
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
}
