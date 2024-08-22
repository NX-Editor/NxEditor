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
}