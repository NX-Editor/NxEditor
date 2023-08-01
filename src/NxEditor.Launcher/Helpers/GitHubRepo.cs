using Octokit;

namespace NxEditor.Launcher.Helpers;

public static class GitHubRepo
{
    private static readonly GitHubClient _githubClient = new(new ProductHeaderValue("NxEditor.Launcher.Helpers.GitHubRepo"));

    public static async Task<(Stream stream, string tag)> GetRelease(string org, string repo, string assetName)
        => await GetRelease((await _githubClient.Repository.Get(org, repo)).Id, assetName);
    public static async Task<(Stream stream, string tag)> GetRelease(long repoId, string assetName)
    {
        IReadOnlyList<Release> releases = await _githubClient.Repository.Release.GetAll(repoId);
        Release latest = releases[0];

        using HttpClient client = new();
        return (await client.GetStreamAsync(latest.Assets
            .Where(x => x.Name == assetName)
            .First().BrowserDownloadUrl), latest.TagName);
    }

    public static async Task<byte[]> GetAsset(string org, string repo, string assetPath)
    {
        return await _githubClient.Repository.Content.GetRawContent(org, repo, assetPath);
    }

    public static async Task<bool> HasUpdate(string org, string repo, string currentTag)
        => await HasUpdate((await _githubClient.Repository.Get(org, repo)).Id, currentTag);
    public static async Task<bool> HasUpdate(long repoId, string currentTag)
    {
        IReadOnlyList<Release> releases = await _githubClient.Repository.Release.GetAll(repoId);
        return releases.Count > 0 && releases[0].TagName != currentTag;
    }
}
