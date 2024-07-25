namespace NxEditor.Core.IO;

public class DataProvider(ArraySegment<byte> data) : IDataProvider
{
    public ArraySegment<byte> GetData()
    {
        return data;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
