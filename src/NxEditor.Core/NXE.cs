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

    private static INxeFrontend? _frontend;

    public static readonly string SystemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nxe");
    public static readonly NxeConfig Config = new();
    public static readonly NxeStatus Status = new();
    public static readonly NxeLogger Logger = new();
    public static INxeFrontend Frontend => _frontend
        ?? throw Exceptions.NoFrontendException;

    public static void RegisterFrontend(INxeFrontend frontend)
    {
        _frontend = frontend;
    }

    public static async ValueTask<T?> OpenFile<T>(string filePath) where T : class, INxDocument
    {
        return await OpenFile(filePath) as T;
    }

    public static async ValueTask<INxDocument?> OpenFile(string filePath)
    {
        IDataProvider dataProvider = new StreamDataProvider(File.OpenRead(filePath));
        FilePayload payload = new(filePath, dataProvider);

        NxProcessorManager.RemoveProcessing(payload);

        if (await Frontend.PickOneAsync(NxDocumentManager.GetProviders(payload), payload.FilePath) is NxDocumentProvider documentProvider) {
            return await documentProvider.GetDocument(payload);
        }

        return null;
    }
}