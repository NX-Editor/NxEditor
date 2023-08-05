using NxEditor.PluginBase;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace NxEditor.Launcher.Helpers;

public class AppUpdater
{
    private static readonly char pathEnvChar = Environment.OSVersion.Platform == PlatformID.Win32NT ? ';' : ':';
    private static readonly string _download = AppPlatform.GetDownload();
    private static readonly string _path = Path.Combine(GlobalConfig.StaticPath, "bin");
    private static readonly string _versionFile = Path.Combine(GlobalConfig.StaticPath, "version.json");
    private static readonly string _launcher = Path.Combine(GlobalConfig.StaticPath, "NxEditor.Launcher.exe");

    public static bool IsInstalled => File.Exists(_versionFile);

    public static async Task<bool> HasUpdate()
    {
        using FileStream fs = File.OpenRead(_versionFile);
        string version = JsonSerializer.Deserialize<string>(fs)!;

        return await GitHubRepo.HasUpdate("NX-Editor", "NxEditor", version);
    }

    public static async Task Install(bool addToPath = true)
    {
        CopyLauncher();
        CreateShortcuts();
        Directory.CreateDirectory(_path);

        (Stream stream, string tag) = await GitHubRepo.GetRelease("NX-Editor", "NxEditor", _download);
        ZipArchive archive = new(stream);
        archive.ExtractToDirectory(_path, true);

        using (FileStream fs = File.Create(_versionFile)) {
            await JsonSerializer.SerializeAsync(fs, tag);
        }

        if (addToPath && Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) is string pathEnv && !pathEnv.Contains(_path)) {
            if (!pathEnv.EndsWith(pathEnvChar)) {
                pathEnv += pathEnvChar;
            }

            Environment.SetEnvironmentVariable("PATH", pathEnv + _path, EnvironmentVariableTarget.User);
        }
    }

    public static Task<bool> Uninstall()
    {
        try {
            if (File.Exists(_versionFile)) {
                File.Delete(_versionFile);
            }

            if (Directory.Exists(_path)) {
                Directory.Delete(_path, true);
            }

            string pluginsDirectory = Path.Combine(GlobalConfig.Shared.StorageFolder, "plugins");
            if (Directory.Exists(pluginsDirectory)) {
                Directory.Delete(pluginsDirectory, true);
            }

            string logsDirectory = Path.Combine(GlobalConfig.Shared.StorageFolder, "logs");
            if (Directory.Exists(logsDirectory)) {
                Directory.Delete(logsDirectory, true);
            }

            DeleteShortcuts();
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    private static void CopyLauncher()
    {
        string? exe = Process.GetCurrentProcess().MainModule?.FileName;
        if (exe is not null && File.Exists(exe)) {
            File.Copy(exe, _launcher);
        }
    }

    public static void CreateShortcuts()
    {
        string app = Path.Combine(_path, AppPlatform.GetName());
        Shortcut.Create("NX Editor", Location.Application, app, "nxe");
        Shortcut.Create("NX Editor Launcher", Location.Application, _launcher, "nxe");
        Shortcut.Create("NX Editor", Location.Desktop, app, "nxe");
    }

    public static void DeleteShortcuts()
    {
        Shortcut.Remove("NX Editor", Location.Application);
        Shortcut.Remove("NX Editor Launcher", Location.Application);
        Shortcut.Remove("NX Editor", Location.Desktop);
    }
}
