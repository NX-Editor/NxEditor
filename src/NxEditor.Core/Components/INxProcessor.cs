using NxEditor.Core.Models;

namespace NxEditor.Core.Components;

public interface INxProcessor
{
    /// <summary>
    /// Remove any processing applied to the <paramref name="payload"/>.
    /// </summary>
    /// <param name="payload"></param>
    void RemoveProcessing(FilePayload payload);

    /// <summary>
    /// Apply processing to the <paramref name="payload"/>.
    /// </summary>
    /// <param name="payload"></param>
    void ApplyProcessing(FilePayload payload);

    /// <summary>
    /// Determines if the <paramref name="payload"/> is processed by this <see cref="INxProcessor"/>
    /// </summary>
    /// <param name="payload"></param>
    /// <returns><see langword="true"/> if the <paramref name="payload"/> is processed by this <see cref="INxProcessor"/></returns>
    bool IsProcessingApplied(FilePayload payload);
}
