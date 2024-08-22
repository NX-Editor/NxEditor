using NxEditor.Core.Models;

namespace NxEditor.Core.Components.Services;

public static class NxDocumentManager
{
    private static readonly List<INxDocumentProvider> _documentProviders = [];

    /// <summary>
    /// Add a new <see cref="INxDocumentProvider"/> to the collection of document providers.
    /// </summary>
    /// <param name="documentProvider"></param>
    public static void Register(INxDocumentProvider documentProvider)
    {
        _documentProviders.Add(documentProvider);
    }

    /// <summary>
    /// Returns a collection of valid <see cref="INxDocumentProvider"/>'s for the provided <paramref name="filePayload"/>.
    /// </summary>
    /// <param name="filePayload"></param>
    public static async IAsyncEnumerable<INxDocumentProvider> GetProviders(FilePayload filePayload)
    {
        foreach (var provider in _documentProviders) {
            if (await provider.CanRead(filePayload)) {
                yield return provider;
            }
        }
    }
}
