using NxEditor.Core.Components;

namespace NxEditor.Core;

public interface INxeFrontend
{
    public IMenuFactory MenuFactory { get; }

    /// <summary>
    /// Prompt the user interface to pick one of <typeparamref name="T"/> from the provided <paramref name="values"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <returns>
    /// <typeparamref name="T"/> or <see langword="null"/> if no values are provided and/or the user cancelled the operation.
    /// </returns>
    ValueTask<T?> PickOneAsync<T>(IAsyncEnumerable<T> values, string? footerContent = null);
}
