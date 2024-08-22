using NxEditor.Core.Models;

namespace NxEditor.Core.Components;

public abstract class NxDocumentProvider
{
    public abstract string Name { get; }
    public abstract ValueTask<bool> CanRead(FilePayload filePayload);
    public abstract ValueTask<INxDocument> GetDocument(FilePayload filePayload);
    public async ValueTask<T?> GetDocument<T>(FilePayload filePayload) where T : class, INxDocument
    {
        return await GetDocument(filePayload) as T;
    }

    public override string ToString()
    {
        return Name;
    }
}
