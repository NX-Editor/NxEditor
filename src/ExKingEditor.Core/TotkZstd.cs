using ZstdSharp;

namespace ExKingEditor.Core;

public class TotkZstd
{
    private static readonly string _zsDicPath = Path.Combine(TotkConfig.Shared.GamePath, "Pack", "ZsDic.pack.zs");

    private static readonly Decompressor _commonDecompressor = new();
    private static readonly Decompressor _mapDecompressor = new();
    private static readonly Decompressor _packDecompressor = new();

    private static readonly Compressor _commonCompressor = new(15);
    private static readonly Compressor _mapCompressor = new(15);
    private static readonly Compressor _packCompressor = new(15);

    static TotkZstd()
    {
        Span<byte> data = _commonDecompressor.Unwrap(File.ReadAllBytes(_zsDicPath));
        using Sarc sarc = Sarc.FromBinary(data);

        _commonDecompressor.LoadDictionary(sarc["zs.zsdic"]);
        _mapDecompressor.LoadDictionary(sarc["bcett.byml.zsdic"]);
        _packDecompressor.LoadDictionary(sarc["pack.zsdic"]);

        _commonCompressor.LoadDictionary(sarc["zs.zsdic"]);
        _mapCompressor.LoadDictionary(sarc["bcett.byml.zsdic"]);
        _packCompressor.LoadDictionary(sarc["pack.zsdic"]);
    }

    public static Span<byte> Decompress(string file)
    {
        Span<byte> src = File.ReadAllBytes(file);
        return
            file.EndsWith(".bcett.byml.zs") ? _mapDecompressor.Unwrap(src) :
            file.EndsWith(".pack.zs") ? _packDecompressor.Unwrap(src) :
            _commonDecompressor.Unwrap(src);
    }
    public static Span<byte> Compress(string file)
    {
        Span<byte> src = File.ReadAllBytes(file);
        return
            file.EndsWith(".bcett.byml") ? _mapCompressor.Wrap(src) :
            file.EndsWith(".pack") ? _packCompressor.Wrap(src) :
            _commonCompressor.Wrap(src);
    }
}
