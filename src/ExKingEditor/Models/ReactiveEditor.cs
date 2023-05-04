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

    public abstract void SaveEditor();
    public abstract bool HasChanged();

    protected Span<byte> RawData()
    {
        return new(_data, _length);
    }
}
