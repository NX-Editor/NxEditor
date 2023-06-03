using AvaloniaEdit;
using CsRestbl.Managed;
using ExKingEditor.Core;
using ExKingEditor.Core.Models;
using ExKingEditor.Core.Utils;
using ExKingEditor.Models;
using System.Text;

namespace ExKingEditor.ViewModels.Editors;

public class RestblViewModel : ReactiveEditor
{
    private string _yaml;

    public TextEditor Editor { get; set; } = null!;
    public string Yaml { get; set; }

    public RestblViewModel(string file, byte[] data, Action<byte[]>? setSource = null) : base(file, data, setSource)
    {
        Restbl table = Restbl.FromBinary(_data);
        StringBuilder yaml = new();

        IEnumerable<NameEntry> entries = table.NameTable
            .Concat(table.CrcTable.Select(
                x => new NameEntry(HashTable.Shared.TryGetValue(x.Hash, out string? name) ? name : $"0x{x.Hash:X2}", x.Size)))
            .OrderBy(x => x.Name);
        
        foreach (var entry in entries) {
            yaml.Append($"{entry.Name}: {entry.Size}\n");
        }
        
        Yaml = _yaml = yaml.ToString();
    }

    public override void SaveAs(string path)
    {
        HashSet<uint> lookup = new();

        Restbl restbl = new();
        foreach ((var nameStr, var sizeStr) in Yaml.Split('\n')[..(^1)].Select(x => x.Split(": ")).Select(x => (name: x[0], size: x[1])).OrderBy(x => x.name)) {
            uint size = Convert.ToUInt32(sizeStr, 10);
            uint hash = nameStr.StartsWith("0x") ? Convert.ToUInt32(nameStr.Remove(0, 2), 16) : Crc32.Compute(nameStr);
            if (lookup.Contains(hash)) {
                restbl.NameTable.Add(new(nameStr, size));
            }
            else {
                lookup.Add(hash);
                restbl.CrcTable.Add(new(hash, size));
            }
        }

        Span<byte> data = (_compressed = path.EndsWith(".zs")) ? TotkZstd.Compress(path, restbl.ToBinary()) : restbl.ToBinary();

        if (path == _file) {
            WriteToSource(data);
        }
        else {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using FileStream fs = File.Create(path);
            fs.Write(data);
        }

        _yaml = Yaml;
        ToastSaveSuccess(path);
    }

    public override void Undo()
    {
        Editor.Undo();
        base.Undo();
    }

    public override void Redo()
    {
        Editor.Redo();
        base.Redo();
    }

    public override void SelectAll()
    {
        Editor.SelectAll();
        base.SelectAll();
    }

    public override Task Cut()
    {
        Editor.Cut();
        return base.Cut();
    }

    public override Task Copy()
    {
        Editor.Copy();
        return base.Copy();
    }

    public override Task Paste()
    {
        Editor.Paste();
        return base.Paste();
    }

    public override void Find()
    {
        Editor.SearchPanel.IsReplaceMode = false;
        Editor.SearchPanel.Open();
        base.Find();
    }

    public override Task FindAndReplace()
    {
        Editor.SearchPanel.IsReplaceMode = true;
        Editor.SearchPanel.Open();
        return base.FindAndReplace();
    }

    public override bool HasChanged()
    {
        return _yaml != Yaml;
    }
}
