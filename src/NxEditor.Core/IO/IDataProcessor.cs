using NxEditor.Core.Models;

namespace NxEditor.Core.IO;

public interface IDataProcessor
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
    /// Determines if the <paramref name="payload"/> is processed by this <see cref="IDataProcessor"/>
    /// </summary>
    /// <param name="payload"></param>
    /// <returns><see langword="true"/> if the <paramref name="payload"/> is processed by this <see cref="IDataProcessor"/></returns>
    bool IsProcessingApplied(FilePayload payload);
}
