using AvaloniaEdit;
using Cead;
using Cead.Handles;
using ExKingEditor.Core;
using ExKingEditor.Models;

namespace ExKingEditor.ViewModels.Editors;

public partial class BymlViewModel : ReactiveEditor
{
    private string _yaml;

    public TextEditor Editor { get; set; } = null!;
    public string Yaml { get; set; }

    public BymlViewModel(string file) : base(file)
    {
        // Load source into readonly field for diffing
        using Byml byml = Byml.FromBinary(RawData());
        _yaml = byml.ToText();

        Yaml = _yaml;
    }

    public override void SaveAs(string path)
    {
        using Byml byml = Byml.FromText(Yaml);
        using DataHandle handle = byml.ToBinary(false, version: 7);

        Span<byte> data = (_compressed = path.EndsWith(".zs")) ? TotkZstd.Compress(path, handle) : handle;

        if (path == _file) {
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.SetLength(data.Length);
            _stream.Write(data);
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
