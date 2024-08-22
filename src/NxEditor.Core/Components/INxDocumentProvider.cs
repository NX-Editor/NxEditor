using NxEditor.Core.Models;

namespace NxEditor.Core.Components;

public interface INxDocumentProvider
{
    ValueTask<bool> CanRead(FilePayload filePayload);
    ValueTask<INxDocument> GetDocument(FilePayload filePayload);
    async ValueTask<T?> GetDocument<T>(FilePayload filePayload) where T : class, INxDocument
    {
        return await GetDocument(filePayload) as T;
    }
}
