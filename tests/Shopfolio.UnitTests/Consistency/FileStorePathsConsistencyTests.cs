using Shopfolio.Infrastructure.Files;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the file store's two directories equal to the Compose volumes that mount them and the app image's prepared directories (T223; 09 21.4
/// item 1; D-32 (a)). The code's only copy of the paths is <see cref="FileStoreDirectories"/>; the file area's volumes are what the container
/// writes to, so a change in any of the three places fails here. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed class FileStorePathsConsistencyTests
{
    private static string Read(params string[] parts) =>
        File.ReadAllText(Path.Combine([RepositoryRoot.Find(), .. parts])).ReplaceLineEndings("\n");

    [Fact]
    public void TheImageDirectory_IsAComposeVolumeOfTheAppService_AndAPreparedDirectoryOfTheImage()
    {
        Assert.Contains($"- files-images:{FileStoreDirectories.DefaultImagesDirectory}\n", Read("deploy", "compose.yaml"), StringComparison.Ordinal);
        Assert.Contains(FileStoreDirectories.DefaultImagesDirectory, PreparedDirectories(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheDigitalDirectory_IsAComposeVolumeOfTheAppService_AndAPreparedDirectoryOfTheImage()
    {
        Assert.Contains($"- files-digital:{FileStoreDirectories.DefaultDigitalDirectory}\n", Read("deploy", "compose.yaml"), StringComparison.Ordinal);
        Assert.Contains(FileStoreDirectories.DefaultDigitalDirectory, PreparedDirectories(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheTwoDirectories_AreDifferent_SoADigitalFileNeverSitsInThePublicImageDirectory()
    {
        Assert.NotEqual(FileStoreDirectories.DefaultImagesDirectory, FileStoreDirectories.DefaultDigitalDirectory);
        Assert.False(FileStoreDirectories.DefaultDigitalDirectory.StartsWith(FileStoreDirectories.DefaultImagesDirectory + "/", StringComparison.Ordinal));
        Assert.False(FileStoreDirectories.DefaultImagesDirectory.StartsWith(FileStoreDirectories.DefaultDigitalDirectory + "/", StringComparison.Ordinal));
    }

    private static string PreparedDirectories()
    {
        string dockerfile = Read("deploy", "images", "app", "Dockerfile");
        return string.Concat(dockerfile.Split('\n').Where(line => line.StartsWith("RUN mkdir -p", StringComparison.Ordinal)));
    }
}
