using AvaloniaEdit;
using NxEditor.Models;
using System.Text;

namespace NxEditor.ViewModels.Editors;

public partial class TextViewModel : ReactiveEditor
{
    private string _text;

    public TextEditor Editor { get; set; } = null!;
    public string Text { get; set; }

    public TextViewModel(string file, byte[] data, Action<byte[]>? setSource = null) : base(file, data, setSource)
    {
        Text = _text = Encoding.UTF8.GetString(_data);
    }

    public override void SaveAs(string path)
    {
        string ext = Path.GetExtension(path);
        File.WriteAllText(path, Text);

        _text = Text;
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
        return _text != Text;
    }
}
