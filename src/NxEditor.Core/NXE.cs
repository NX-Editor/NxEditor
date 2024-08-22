using NxEditor.Core.Components;
using NxEditor.Core.Components.Services;
using NxEditor.Core.IO;
using NxEditor.Core.Models;

namespace NxEditor.Core;

public static class NXE
{
    static NXE()
    {
        Directory.CreateDirectory(SystemPath);
    }

    public static readonly string SystemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nxe");
    public static readonly NxeConfig Config = new();
    public static readonly NxeStatus Status = new();
    public static readonly NxeLogger Logger = new();

    public static async ValueTask<T?> OpenFile<T>(string filePath) where T : class, INxDocument
    {
        return await OpenFile(filePath) as T;
    }

    public static async ValueTask<INxDocument?> OpenFile(string filePath)
    {
        IDataProvider dataProvider = new StreamDataProvider(File.OpenRead(filePath));
        FilePayload payload = new(filePath, dataProvider);

        NxProcessorManager.RemoveProcessing(payload);

        await foreach (var provider in NxDocumentManager.GetProviders(payload)) {
            return await provider.GetDocument(payload);
        }

        return null;
    }
}