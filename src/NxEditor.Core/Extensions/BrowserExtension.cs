using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NxEditor.Core.Extensions;

public static class BrowserExtension
{
    public static async Task OpenUrl(string url)
    {
        ProcessStartInfo info = new() {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            info.FileName = "cmd";
            info.Arguments = $"/c start {url}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            info.FileName = "xdg-open";
            info.Arguments = url;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            info.FileName = "open";
            info.Arguments = url;
        }
        else {
            throw new Exception("Unknown operating system");
        }

        await Process.Start(info)!.WaitForExitAsync();
    }
}

