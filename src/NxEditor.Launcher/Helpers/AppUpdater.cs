using NxEditor.PluginBase;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace NxEditor.Launcher.Helpers;

public class AppUpdater
{
    private const string APP_NAME = "NX Editor";
    private const string LAUNCHER_NAME = "NX Editor Launcher";
    private const string GITHUB_ORG = "NX-Editor";
    private const string GITHUB_REPO = "NxEditor";

    private static readonly char _pathEnvChar = OperatingSystem.IsWindows() ? ';' : ':';
    private static readonly string _zipFileName = AppPlatform.GetOsFileName();
    private static readonly string _outputPath = Path.Combine(GlobalConfig.StaticPath, "bin");
    private static readonly string _versionFile = Path.Combine(GlobalConfig.StaticPath, "version.json");
    private static readonly string _launcherOutputPath = Path.Combine(GlobalConfig.StaticPath, "NxEditor.Launcher.exe");

    public static bool IsInstalled => File.Exists(_versionFile);

    public static async Task<bool> HasUpdate()
    {
        using FileStream fs = File.OpenRead(_versionFile);
        string version = JsonSerializer.Deserialize<string>(fs)!;

        return await GitHubRepo.HasUpdate(GITHUB_ORG, GITHUB_REPO, version);
    }

    public static async Task Install(bool addToPath = true)
    {
        CopyRunningLauncherToOutput();
        CreateDesktopShortcuts();
        Directory.CreateDirectory(_outputPath);

        (Stream stream, string tag) = await GitHubRepo.GetRelease(GITHUB_ORG, GITHUB_REPO, _zipFileName);
        ZipArchive archive = new(stream);
        archive.ExtractToDirectory(_outputPath, true);

        using (FileStream fs = File.Create(_versionFile)) {
            await JsonSerializer.SerializeAsync(fs, tag);
        }

        if (addToPath && Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) is string pathEnv && !pathEnv.Contains(_outputPath)) {
            if (!pathEnv.EndsWith(_pathEnvChar)) {
                pathEnv += _pathEnvChar;
            }

            Environment.SetEnvironmentVariable("PATH", pathEnv + _outputPath, EnvironmentVariableTarget.User);
        }
    }

    public static Task<bool> Uninstall()
    {
        try {
            if (File.Exists(_versionFile)) {
                File.Delete(_versionFile);
            }

            if (Directory.Exists(_outputPath)) {
                Directory.Delete(_outputPath, true);
            }

            string pluginsDirectory = Path.Combine(GlobalConfig.Shared.StorageFolder, "plugins");
            if (Directory.Exists(pluginsDirectory)) {
                Directory.Delete(pluginsDirectory, true);
            }

            string logsDirectory = Path.Combine(GlobalConfig.Shared.StorageFolder, "logs");
            if (Directory.Exists(logsDirectory)) {
                Directory.Delete(logsDirectory, true);
            }

            DeleteDesktopShortcuts();
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    private static void CopyRunningLauncherToOutput()
    {
        string? exe = Process.GetCurrentProcess().MainModule?.FileName;
        if (exe is not null && File.Exists(exe) && !exe.SequenceEqual(_launcherOutputPath)) {
            File.Copy(exe, _launcherOutputPath, true);
        }
    }

    private static void CreateDesktopShortcuts()
    {
        string app = Path.Combine(_outputPath, AppPlatform.GetExecutableName());
        Shortcut.Create(APP_NAME, Location.Application, app, "nxe");
        Shortcut.Create(LAUNCHER_NAME, Location.Application, _launcherOutputPath, "nxe");
        Shortcut.Create(APP_NAME, Location.Desktop, app, "nxe");
    }

    private static void DeleteDesktopShortcuts()
    {
        Shortcut.Remove(APP_NAME, Location.Application);
        Shortcut.Remove(LAUNCHER_NAME, Location.Application);
        Shortcut.Remove(APP_NAME, Location.Desktop);
    }
}
