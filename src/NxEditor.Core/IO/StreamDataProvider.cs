using System.Buffers;

namespace NxEditor.Core.IO;

public class StreamDataProvider : IDataProvider
{
    private readonly Stream _stream;
    private readonly byte[] _rented = [];
    private readonly int _size;
    private bool _isBufferFilled = false;

    public StreamDataProvider(Stream stream)
    {
        _stream = stream;
        _size = Convert.ToInt32(_stream.Length);
        _rented = ArrayPool<byte>.Shared.Rent(_size);
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(_rented);
        _stream.Dispose();
        GC.SuppressFinalize(this);
    }

    public ArraySegment<byte> GetData()
    {
        ArraySegment<byte> result = new(_rented, offset: 0, _size);

        if (_isBufferFilled) {
            return result;
        }

        _stream.Read(result);
        _isBufferFilled = true;
        return result;
    }
}
