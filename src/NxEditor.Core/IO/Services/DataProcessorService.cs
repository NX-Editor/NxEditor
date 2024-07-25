using NxEditor.Core.Models;

namespace NxEditor.Core.IO.Services;

public class DataProcessorService
{
    private static readonly List<IDataProcessor> _dataProcessors = [];

    /// <summary>
    /// Add a new <see cref="IDataProcessor"/> to the collection of data processors.
    /// </summary>
    /// <param name="dataProcessor"></param>
    public static void Register(IDataProcessor dataProcessor)
    {
        _dataProcessors.Add(dataProcessor);
    }

    /// <summary>
    /// Remove all processing from the provided <paramref name="payload"/>.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="appliedDataProcessors">The list of processors applied to the <paramref name="payload"/></param>
    public static void RemoveProcessing(FilePayload payload, out List<IDataProcessor> appliedDataProcessors)
    {
        appliedDataProcessors = [];

        foreach (IDataProcessor processor in GetDataProcessors(payload)) {
            processor.RemoveProcessing(payload);
            appliedDataProcessors.Add(processor);
        }

        appliedDataProcessors.Reverse();
    }

    /// <summary>
    /// Remove all processing from the provided <paramref name="payload"/>.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="appliedDataProcessors">The list of processors applied to the <paramref name="payload"/></param>
    public static void RemoveProcessing(FilePayload payload)
    {
        foreach (IDataProcessor processor in GetDataProcessors(payload)) {
            processor.RemoveProcessing(payload);
        }
    }

    /// <summary>
    /// Recursively retrieve all processors applied to the <paramref name="payload"/> until all processing is removed or no processors can be found.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static IEnumerable<IDataProcessor> GetDataProcessors(FilePayload payload)
    {
    ResetLoop:
        foreach (IDataProcessor processor in _dataProcessors) {
            if (processor.IsProcessingApplied(payload)) {
                yield return processor;
                goto ResetLoop;
            }
        }
    }
}
