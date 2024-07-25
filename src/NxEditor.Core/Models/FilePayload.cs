using NxEditor.Core.IO;
using System.Diagnostics.CodeAnalysis;

namespace NxEditor.Core.Models;

public class FilePayload : IDisposable
{
    /// <summary>
    /// The absolute or nested file path of the incoming <see cref="FilePayload"/>.
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// The <see cref="IDataProvider"/> for the <see cref="FilePayload"/>.
    /// </summary>
    public required IDataProvider DataProvider { get; set; } 

    public FilePayload()
    {
    }

    [SetsRequiredMembers]
    public FilePayload(string filePath, IDataProvider dataProvider)
    {
        FilePath = filePath;
        DataProvider = dataProvider;
    }

    public void Dispose()
    {
        DataProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
