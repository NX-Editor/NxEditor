using AvaloniaEdit;
using CsMsbt;
using NxEditor.Core;
using NxEditor.Models;
using Native.IO.Handles;

namespace NxEditor.ViewModels.Editors;

public partial class MsbtViewModel : ReactiveEditor
{
    private string _yaml;

    public TextEditor Editor { get; set; } = null!;
    public string Yaml { get; set; }

    public MsbtViewModel(string file, byte[] data, Action<byte[]>? setSource = null) : base(file, data, setSource)
    {
        using Msbt msbt = Msbt.FromBinary(_data);
        Yaml = _yaml = msbt.ToText().ToString();

        SupportedExtensions.Add("Yaml:*.yml;*.yaml|");
    }

    public override void SaveAs(string path)
    {
        string ext = Path.GetExtension(path);

        if (ext is ".yaml" or ".yml") {
            File.WriteAllText(path, Yaml);
        }
        else {
            using Msbt byml = Msbt.FromText(Yaml);
            using DataMarshal handle = byml.ToBinary();

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
