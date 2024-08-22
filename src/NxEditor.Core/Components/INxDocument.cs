using NxEditor.Core.Models;

namespace NxEditor.Core.Components;

public interface INxDocument
{
    bool CanFind { get; }
    bool CanReplace { get; }
    bool CanWrite { get; }

    ValueTask<FindResult> Find(FindContext context);
    ValueTask<FindResult> FindAndReplace(FindAndReplaceContext context);

    ValueTask Read(FilePayload filePayload);
    ValueTask Write(string output);
}
