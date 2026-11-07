using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Modules.Catalog;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The category level has one writer (09 9.4 item 4; T333 criterion 6): <c>Category.Level</c> has a private setter, and the only code that gives a category its level - <c>Category.Create</c>
/// with a level and <c>Category.Relocate</c> - is called from <c>Catalog/CategoryTree</c> alone (and from the model tests, which build rows to test the database). The scan reads the
/// application's sources and is itself shown to catch a call from another place (09 19.2 item 3).
/// </summary>
public sealed partial class CategoryLevelWriterTests
{
    private const string TreeFile = "Modules/Catalog/CategoryTree.cs";

    private const string EntityFile = "Modules/Catalog/Category.cs";

    [GeneratedRegex(@"(\bCategory\.Create|\.Relocate)\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LevelWriteCall();

    [Fact]
    public void TheLevelSetter_IsPrivate()
    {
        PropertyInfo level = typeof(Category).GetProperty(nameof(Category.Level), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        Assert.NotNull(level.GetMethod);
        Assert.True(level.SetMethod is { IsPrivate: true });
    }

    [Fact]
    public void TheCallsThatWriteALevel_AreInTheCategoryTreeAlone()
    {
        string root = Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web");
        Dictionary<string, string> sources = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToDictionary(path => Path.GetRelativePath(root, path).Replace('\\', '/'), File.ReadAllText);

        Assert.True(sources.ContainsKey(TreeFile));
        Assert.Empty(Offenders(sources));
        Assert.Contains(TreeFile, CallersOf(sources));
    }

    [Fact]
    public void TheScan_FindsACallFromAnotherFile_AndIgnoresTheTreeAndTheEntity()
    {
        Dictionary<string, string> sample = new(StringComparer.Ordinal)
        {
            [TreeFile] = "category.Relocate(parent, level); Category.Create(name, slug, parent, level, now);",
            [EntityFile] = "public void Relocate(Guid? parent, int level) { } public static Category Create(string name) => null;",
            ["Modules/Catalog/Other.cs"] = "tree[0].Relocate(null, 1);",
            ["Pages/Panel/Categories.cshtml.cs"] = "var c = Category.Create(\"x\", \"x\", null, 1, now);",
        };

        Assert.Equal(["Modules/Catalog/Other.cs", "Pages/Panel/Categories.cshtml.cs"], Offenders(sample).Order(StringComparer.Ordinal));
    }

    private static List<string> Offenders(Dictionary<string, string> sources)
    {
        return [.. CallersOf(sources).Where(file => file is not TreeFile and not EntityFile)];
    }

    private static List<string> CallersOf(Dictionary<string, string> sources)
    {
        return [.. sources.Where(pair => LevelWriteCall().IsMatch(pair.Value) && pair.Key != EntityFile).Select(pair => pair.Key)];
    }
}
