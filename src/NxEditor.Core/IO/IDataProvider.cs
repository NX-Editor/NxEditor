namespace NxEditor.Core.IO;

public interface IDataProvider : IDisposable
{
    public ArraySegment<byte> GetData();
}
