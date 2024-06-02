namespace NxEditor.Core;

public static class NXE
{
    static NXE()
    {
        Directory.CreateDirectory(SystemPath);
    }

    public static readonly string SystemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nxe");
    public static readonly AppConfig Config = new();
}