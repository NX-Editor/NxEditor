using NxEditor.Plugin.Models;

namespace NxEditor.Plugin.Services;

/// <summary>
/// Processes the request <see cref="IFileHandle"/> before being sent to a <see cref="IFormatService"/>
/// </summary>
public interface IProcessingService : IServiceModule
{
    /// <summary>
    /// Converts processed data to raw data and returns the result
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public IFileHandle Process(IFileHandle handle);

    /// <summary>
    /// Reprocesses raw data and returns the result
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public IFileHandle Reprocess(IFileHandle handle);
}
