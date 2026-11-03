namespace Shopfolio.UnitTests.Locks;

/// <summary>
/// The lock file of an enum (09 12 item 3): <c>UnitTests/Locks/&lt;EnumFullName&gt;.txt</c> holds the member names line by line. The
/// comparison is two-way: a name in the file and not in the enum (a removal or a rename) and a name in the enum and not in the file
/// (an addition) both fail. An addition is made by adding a line, so the change shows in the pull request.
/// </summary>
internal static class EnumLock
{
    public static string DirectoryPath(string repositoryRoot) => Path.Combine(repositoryRoot, "tests", "Shopfolio.UnitTests", "Locks");

    public static string FilePath(string repositoryRoot, Type enumType) => Path.Combine(DirectoryPath(repositoryRoot), enumType.FullName + ".txt");

    public static IReadOnlyList<string> ReadNames(string text)
    {
        return text.ReplaceLineEndings("\n").Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
    }

    /// <summary>The differences between the locked names and the enum's members; empty when they are equal.</summary>
    public static IReadOnlyList<string> Compare(IReadOnlyCollection<string> locked, IReadOnlyCollection<string> members)
    {
        List<string> findings = [];
        findings.AddRange(locked.Except(members, StringComparer.Ordinal).Select(n => $"'{n}' is in the lock file but not in the enum (removed or renamed)."));
        findings.AddRange(members.Except(locked, StringComparer.Ordinal).Select(n => $"'{n}' is in the enum but not in the lock file (added)."));
        findings.AddRange(locked.GroupBy(n => n, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => $"'{g.Key}' is in the lock file more than once."));
        return findings;
    }
}
