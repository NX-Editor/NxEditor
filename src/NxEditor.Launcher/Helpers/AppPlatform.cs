namespace NxEditor.Launcher.Helpers;

public class AppPlatform
{
    public static string GetName()
    {
        if (OperatingSystem.IsWindows()) {
            return "nxe.exe";
        }
        else {
            return "nxe";
        }
    }

    public static string GetDownload()
    {
        if (OperatingSystem.IsWindows()) {
            return "win-x64.zip";
        }
        else if (OperatingSystem.IsMacOS()) {
            return "osx-x64.zip";
        }
        else if (OperatingSystem.IsLinux()) {
            return "linux-x64.zip";
        }
        else {
            throw new NotSupportedException("The running operating system is not supported");
        }
    }
}
