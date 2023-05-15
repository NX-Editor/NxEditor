using AvaloniaEdit;
using Cead;
using Cead.Handles;
using ExKingEditor.Core;
using ExKingEditor.Models;
using System.Buffers.Binary;

namespace ExKingEditor.ViewModels.Editors;

public partial class BymlViewModel : ReactiveEditor
{
    private string _yaml;
    private readonly int _version;
    private readonly bool _isBigEndian;

    public TextEditor Editor { get; set; } = null!;
    public string Yaml { get; set; }

    public BymlViewModel(string file, byte[] data, Stream? fs, Action<byte[]>? setSource = null) : base(file, data, fs, setSource)
    {
        using Byml byml = Byml.FromBinary(_data);
        Yaml = _yaml = byml.ToText();

        _version = (_isBigEndian = _data.AsSpan()[..2].SequenceEqual("BY"u8))
            ? BinaryPrimitives.ReadInt16BigEndian(_data.AsSpan()[2..4])
            : BinaryPrimitives.ReadInt16LittleEndian(_data.AsSpan()[2..4]);
    }

    public override void SaveAs(string path)
    {
        using Byml byml = Byml.FromText(Yaml);
        using DataHandle handle = byml.ToBinary(_isBigEndian, _version);

        Span<byte> data = (_compressed = path.EndsWith(".zs")) ? TotkZstd.Compress(path, handle) : handle;

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
