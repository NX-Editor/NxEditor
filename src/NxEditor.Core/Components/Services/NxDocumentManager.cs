using NxEditor.Core.Models;

namespace NxEditor.Core.Components.Services;

public static class NxDocumentManager
{
    private static readonly List<NxDocumentProvider> _documentProviders = [];

    /// <summary>
    /// Add a new <see cref="NxDocumentProvider"/> to the collection of document providers.
    /// </summary>
    /// <param name="documentProvider"></param>
    public static void Register(NxDocumentProvider documentProvider)
    {
        _documentProviders.Add(documentProvider);
    }

    /// <summary>
    /// Returns a collection of valid <see cref="NxDocumentProvider"/>'s for the provided <paramref name="filePayload"/>.
    /// </summary>
    /// <param name="filePayload"></param>
    public static async IAsyncEnumerable<NxDocumentProvider> GetProviders(FilePayload filePayload)
    {
        foreach (var provider in _documentProviders) {
            if (await provider.CanRead(filePayload)) {
                yield return provider;
            }
        }
    }
}
