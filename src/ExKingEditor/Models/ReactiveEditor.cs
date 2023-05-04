using Avalonia.Controls.Notifications;
using Dock.Model.Mvvm.Controls;
using ExKingEditor.Core;

namespace ExKingEditor.Models;

public abstract unsafe class ReactiveEditor : Document
{
    protected readonly string _file;
    protected readonly bool _compressed;
    protected readonly FileStream _stream;

    private readonly byte* _data;
    private readonly int _length;

    public string FilePath => _file;

    public ReactiveEditor(string file)
    {
        Id = file;
        Title = Path.GetFileName(file);

        _file = file;
        _compressed = file.EndsWith(".zs");

        // keep the file open until the
        // tab closes
        _stream = File.Open(file, FileMode.Open);
        Span<byte> raw = new byte[_stream.Length];
        _stream.Read(raw);

        // Decompress if necessary
        Span<byte> data = _compressed ? TotkZstd.Decompress(file, raw) : raw;
        _length = data.Length;

        // Store a ptr on the heap to
        // avoid copying to an array
        fixed (byte* ptr = data) {
            _data = ptr;
        }
    }

    public override bool OnClose()
    {
        // Handle pending changes
        _stream.Close();
        return true;
    }

    public abstract void Save();
    public abstract void SaveAs(string path);
    public abstract bool HasChanged();

    public virtual void Undo() { }
    public virtual void Redo() { }

    public virtual void SelectAll() { }
    public virtual void Cut() { }
    public virtual void Copy() { }
    public virtual void Paste() { }

    public virtual void Find() { }
    public virtual void FindAndReplace() { }

    protected Span<byte> RawData()
    {
        return new(_data, _length);
    }

    protected void ToastSaveSuccess(string path)
    {
        App.Toast(
            $"Saved {(_compressed ? "and compressed " : "")}{Path.GetFileName(path)}", "Success",
            NotificationType.Success, TimeSpan.FromSeconds(1.2));
    }
}
