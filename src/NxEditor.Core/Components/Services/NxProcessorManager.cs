using NxEditor.Core.Models;

namespace NxEditor.Core.Components.Services;

public static class NxProcessorManager
{
    private static readonly List<INxProcessor> _dataProcessors = [];

    /// <summary>
    /// Add a new <see cref="INxProcessor"/> to the collection of data processors.
    /// </summary>
    /// <param name="dataProcessor"></param>
    public static void Register(INxProcessor dataProcessor)
    {
        _dataProcessors.Add(dataProcessor);
    }

    /// <summary>
    /// Remove all processing from the provided <paramref name="payload"/>.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="appliedDataProcessors">The list of processors applied to the <paramref name="payload"/></param>
    public static void RemoveProcessing(FilePayload payload, out List<INxProcessor> appliedDataProcessors)
    {
        appliedDataProcessors = [];

        foreach (INxProcessor processor in GetDataProcessors(payload)) {
            processor.RemoveProcessing(payload);
            appliedDataProcessors.Add(processor);
        }

        appliedDataProcessors.Reverse();
    }

    /// <summary>
    /// Remove all processing from the provided <paramref name="payload"/>.
    /// </summary>
    /// <param name="payload"></param>
    public static void RemoveProcessing(FilePayload payload)
    {
        foreach (INxProcessor processor in GetDataProcessors(payload)) {
            processor.RemoveProcessing(payload);
        }
    }

    /// <summary>
    /// Recursively retrieve all processors applied to the <paramref name="payload"/> until all processing is removed or no processors can be found.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static IEnumerable<INxProcessor> GetDataProcessors(FilePayload payload)
    {
    ResetLoop:
        foreach (INxProcessor processor in _dataProcessors) {
            if (processor.IsProcessingApplied(payload)) {
                yield return processor;
                goto ResetLoop;
            }
        }
    }
}
