namespace Shopfolio.UnitTests.Support;

/// <summary>
/// Finds the repository root from <c>Shopfolio.slnx</c> (09 21.4 item 1: the consistency checks and source scans read repository files).
/// </summary>
internal static class RepositoryRoot
{
    public static string Find()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Shopfolio.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Shopfolio.slnx was not found above the test assembly.");
    }
}
