using NxEditor.Plugin.Models;

namespace NxEditor.Plugin.Services;

public interface IServiceModule
{
    /// <summary>
    /// Checks if the provided <paramref name="handle"/> can be processed by the <see cref="IServiceModule"/>
    /// </summary>
    /// <param name="handle"></param>
    /// <returns><see langword="true"/> if the <see cref="IServiceModule"/> can process the <see cref="IFileHandle"/></returns>
    public bool IsValid(IFileHandle handle);
}
