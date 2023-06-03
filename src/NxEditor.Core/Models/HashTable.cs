using NxEditor.Core.Extensions;
using System.Buffers.Binary;
using System.Text;

namespace NxEditor.Core.Models;

public static class HashTable
{
    private static readonly Encoding _encoding = Encoding.UTF8;

    public static Dictionary<uint, string> Shared { get; } = new();

    static HashTable()
    {
        Stream stream = typeof(HashTable).Assembly.FetchEmbed("HashTable")!;

        // Decompress
        Span<byte> block = new byte[stream.Length];
        stream.Read(block);
        block = TotkZstd.Decompress(".bin", block);

        // Decode
        uint count = BinaryPrimitives.ReadUInt32LittleEndian(block[..4]);
        int offset = 4;

        for (int i = 0; i < count; i++) {
            uint hash = BinaryPrimitives.ReadUInt32LittleEndian(block[offset..(offset += 4)]);
            int size = block[offset++];
            Shared.Add(hash, _encoding.GetString(block[offset..(offset += size)]));
        }
    }
}
