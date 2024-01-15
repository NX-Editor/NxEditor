using Octokit;

namespace NxEditor.Core.Helpers;

public static class GithubHelper
{
    private static readonly GitHubClient _client = new(
        new ProductHeaderValue($"NxEditor.Core.Helpers.GithubHelper.{Guid.NewGuid()}")
    );

    public static async Task<(Stream stream, string tag)> GetRelease(string org, string repo, string assetName)
    {
        return await GetRelease((await _client.Repository.Get(org, repo)).Id, assetName);
    }

    public static async Task<(Stream stream, string tag)> GetRelease(long repoId, string assetName)
    {
        IReadOnlyList<Release> releases = await _client.Repository.Release.GetAll(repoId);

        Release? latest = null;
        ReleaseAsset? asset = null;

        int index = -1;
        while (asset == null || latest == null) {
            index++;
            latest = releases[index];
            asset = latest.Assets.Where(x => x.Name == assetName).FirstOrDefault();
        }

        using HttpClient client = new();
        return (await client.GetStreamAsync(asset.BrowserDownloadUrl), latest.TagName);
    }

    public static async Task<byte[]> GetAsset(string org, string repo, string assetPath)
    {
        return await _client.Repository.Content.GetRawContent(org, repo, assetPath);
    }

    public static async Task<bool> HasUpdate(string org, string repo, string currentTag)
    {
        return await HasUpdate((await _client.Repository.Get(org, repo)).Id, currentTag);
    }

    public static async Task<bool> HasUpdate(long repoId, string currentTag)
    {
        IReadOnlyList<Release> releases = await _client.Repository.Release.GetAll(repoId);
        return releases.Count > 0 && releases[0].TagName != currentTag;
    }
}
