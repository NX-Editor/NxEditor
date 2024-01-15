namespace NxEditor.Core.Helpers;

public class PlatformHelper
{
    public static string GetExecutableName()
    {
        if (OperatingSystem.IsWindows()) {
            return "nxe.exe";
        }
        else {
            return "nxe";
        }
    }

    public static string GetOsFileName()
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

    public static string GetLauncherFileName()
    {
        if (OperatingSystem.IsWindows()) {
            return "Windows-Launcher.zip";
        }
        else if (OperatingSystem.IsMacOS()) {
            return "MacOS-Launcher.zip";
        }
        else if (OperatingSystem.IsLinux()) {
            return "Linux-Launcher.zip";
        }
        else {
            throw new NotSupportedException("The running operating system is not supported");
        }
    }
}
