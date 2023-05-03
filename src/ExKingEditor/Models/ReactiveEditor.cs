using ExKingEditor.Core;

namespace ExKingEditor.Models;

public abstract unsafe class ReactiveEditor : ReactiveObject
{
    protected readonly string _file;
    protected readonly bool _compressed;

    private readonly byte* _data;
    private readonly int _length;

    public ReactiveEditor(string file)
    {
        _file = file;
        _compressed = file.EndsWith(".zs");

        Span<byte> data = TotkZstd.Decompress(file);
        _length = data.Length;

        // Store a ptr on the heap to
        // avoid copying to an array
        fixed (byte* ptr = data) {
            _data = ptr;
        }
    }

    public abstract ReactiveEditor LoadEditor();
    public abstract void SaveEditor();

    protected Span<byte> RawData()
    {
        return new(_data, _length);
    }
}
