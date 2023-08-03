using NxEditor.PluginBase;
using System.IO.Compression;
using System.Text.Json;

namespace NxEditor.Launcher.Helpers;

public class AppUpdater
{
    private static readonly char pathEnvChar = Environment.OSVersion.Platform == PlatformID.Win32NT ? ';' : ':';
    private static readonly string _download = AppPlatform.GetDownload();
    private static readonly string _path = Path.Combine(GlobalConfig.StaticPath, "bin");
    private static readonly string _versionFile = Path.Combine(GlobalConfig.StaticPath, "version.json");

    public static bool IsInstalled => File.Exists(_versionFile);

    public static async Task<bool> HasUpdate()
    {
        using FileStream fs = File.OpenRead(_versionFile);
        string version = JsonSerializer.Deserialize<string>(fs)!;

        return await GitHubRepo.HasUpdate("NX-Editor", "NxEditor", version);
    }

    public static async Task Download(bool addToPath = true)
    {
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

}
