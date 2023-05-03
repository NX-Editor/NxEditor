using Dock.Model.Mvvm.Controls;
using ExKingEditor.Core;

namespace ExKingEditor.Models;

public abstract unsafe class ReactiveEditor : Document
{
    protected readonly FileStream _stream;
    protected readonly string _file;
    protected readonly bool _compressed;

    private readonly byte* _data;
    private readonly int _length;

    public ReactiveEditor(string file)
    {
        Title = Path.GetFileName(file);

        _file = file;
        _stream = File.OpenRead(file);
        _compressed = file.EndsWith(".zs");

        Span<byte> data = _compressed ? TotkZstd.Decompress(file) : File.ReadAllBytes(file);
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
        return true;
    }

    public abstract void SaveEditor();
    public abstract bool HasChanged();
    public void CloseEditor() => _stream.Close();

    protected Span<byte> RawData()
    {
        return new(_data, _length);
    }
}
