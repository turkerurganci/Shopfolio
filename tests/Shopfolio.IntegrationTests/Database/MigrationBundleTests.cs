using Shopfolio.UnitTests.Support;

namespace Shopfolio.IntegrationTests.Database;

[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class MigrationBundleTests
{
    [Fact]
    public async Task BuildFreshAsync_WhenBundleIsBuilt_LeavesTrackedLockFilesUnchanged()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        string root = RepositoryRoot.Find();
        string[] lockFiles = Directory.GetFiles(Path.Combine(root, "src"), "packages.lock.json", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();
        Assert.NotEmpty(lockFiles);
        Dictionary<string, byte[]> before = lockFiles.ToDictionary(path => path, File.ReadAllBytes);

        string executable;
        using (MigrationBundle bundle = await MigrationBundle.BuildFreshAsync(ct))
        {
            executable = bundle.ExecutablePath;
            Assert.True(File.Exists(executable));
        }

        Assert.False(File.Exists(executable));
        foreach ((string path, byte[] content) in before)
        {
            Assert.Equal(content, await File.ReadAllBytesAsync(path, ct));
        }
    }

    [Fact]
    public async Task BuildAsync_WhenCalledTwiceForTheSameBuild_ReusesTheCachedBundle()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;

        string first;
        using (MigrationBundle bundle = await MigrationBundle.BuildAsync(ct))
        {
            first = bundle.ExecutablePath;
        }

        using MigrationBundle again = await MigrationBundle.BuildAsync(ct);

        Assert.Equal(first, again.ExecutablePath);
        Assert.True(File.Exists(again.ExecutablePath));
    }
}
