using System.Runtime.InteropServices;
using System.Text;

namespace ExKingEditor.Core.Windows;

[StructLayout(LayoutKind.Sequential, Size = 20, Pack = 4)]
public readonly struct CF_HDROP
{
    private readonly uint _filesOffset = 0x14;
    private readonly long _dropPoint = 0;
    private readonly uint _useNonClientArea = 0;
    private readonly uint _useUnicodePath = 1;

    public CF_HDROP() { }

    public static byte[] Build(IEnumerable<string> paths)
    {
        List<byte> unicode = new();
        foreach (var path in paths) {
            unicode.AddRange(Encoding.Unicode.GetBytes(path + '\0'));
        }

        unicode.Add((byte)'\0');

        int size = Marshal.SizeOf<CF_HDROP>();
        byte[] data = new byte[size + unicode.Count];
        nint ptr = nint.Zero;

        try {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(new CF_HDROP(), ptr, true);
            Marshal.Copy(ptr, data, 0, size);
            Array.Copy(unicode.ToArray(), 0, data, size, unicode.Count);
        }
        finally {
            Marshal.FreeHGlobal(ptr);
        }

        return data;
    }
}
