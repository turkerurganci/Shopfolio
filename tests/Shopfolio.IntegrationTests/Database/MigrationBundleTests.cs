using Shopfolio.UnitTests.Support;

namespace Shopfolio.IntegrationTests.Database;

[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class MigrationBundleTests
{
    [Fact]
    public async Task BuildAsync_WhenBundleIsBuilt_LeavesTrackedLockFilesUnchanged()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        string root = RepositoryRoot.Find();
        string[] lockFiles = Directory.GetFiles(Path.Combine(root, "src"), "packages.lock.json", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();
        Assert.NotEmpty(lockFiles);
        Dictionary<string, byte[]> before = lockFiles.ToDictionary(path => path, File.ReadAllBytes);

        using MigrationBundle bundle = await MigrationBundle.BuildAsync(ct);

        Assert.True(File.Exists(bundle.ExecutablePath));
        foreach ((string path, byte[] content) in before)
        {
            Assert.Equal(content, await File.ReadAllBytesAsync(path, ct));
        }
    }
}
