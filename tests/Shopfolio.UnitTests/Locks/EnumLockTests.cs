using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.UnitTests.Samples;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Locks;

/// <summary>
/// The lock file test of the stored enums (T102 criterion 5, KK5; 09 12 item 3): every enum whose value is stored has a file under
/// <c>UnitTests/Locks/</c>, and the file and the enum's members are equal both ways. The set of enums is the enums of the module
/// contracts and of the stored values, together with every enum found on a property of the database model.
/// </summary>
public sealed class EnumLockTests
{
    [Fact]
    public void EveryStoredEnum_HasALockFile_EqualToItsMembersBothWays()
    {
        string root = RepositoryRoot.Find();
        List<string> findings = [];
        foreach (Type enumType in StoredEnums())
        {
            string file = EnumLock.FilePath(root, enumType);
            if (!File.Exists(file))
            {
                findings.Add($"{enumType.FullName}: there is no lock file {Path.GetRelativePath(root, file)}.");
                continue;
            }

            IReadOnlyList<string> locked = EnumLock.ReadNames(File.ReadAllText(file));
            findings.AddRange(EnumLock.Compare(locked, Enum.GetNames(enumType)).Select(f => $"{enumType.FullName}: {f}"));
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void EveryLockFile_BelongsToAStoredEnum()
    {
        string root = RepositoryRoot.Find();
        HashSet<string> expected = StoredEnums().Select(t => t.FullName + ".txt").ToHashSet(StringComparer.Ordinal);

        string[] orphans = Directory.EnumerateFiles(EnumLock.DirectoryPath(root), "*.txt")
            .Select(f => Path.GetFileName(f))
            .Where(name => !expected.Contains(name))
            .ToArray();

        Assert.Empty(orphans);
    }

    [Fact]
    public void StoredEnums_IncludeTheEnumsOfTheModelProperties()
    {
        using Shopfolio.Infrastructure.AppDbContext context = SampleModel.CreateGoodContext();

        IEnumerable<Type> modelEnums = ModelEnums(context);

        Assert.Contains(typeof(OrderStatus), StoredEnums().Concat(ModelEnums(context)));
        Assert.Contains(modelEnums, t => t.Name == "PublishStatus");
    }

    [Fact]
    public void StoredEnums_AreTheUnionOfTheNamespaceEnumsAndTheEnumsOfTheModel_WithoutANamespaceFilter()
    {
        DbContextOptionsBuilder<Shopfolio.Infrastructure.AppDbContext> options = new();
        Shopfolio.Infrastructure.AppDbContext.Configure(options, "Host=localhost;Database=shopfolio_lock_model");
        using Shopfolio.Infrastructure.AppDbContext context = new(options.Options);

        HashSet<Type> stored = [.. StoredEnums()];
        HashSet<Type> union = [.. ApplicationEnums.Persistent(), .. ModelEnums(context)];

        Assert.True(union.SetEquals(stored));
        Assert.All(ModelEnums(context), t => Assert.Contains(t, stored));
    }

    [Fact]
    public void Compare_WhenAMemberIsAddedAndTheFileIsNotUpdated_Fails()
    {
        IReadOnlyList<string> findings = EnumLock.Compare(["Placed", "Preparing"], ["Placed", "Preparing", "Shipped"]);

        Assert.Equal(["'Shipped' is in the enum but not in the lock file (added)."], findings);
    }

    [Fact]
    public void Compare_WhenAMemberIsRemovedOrRenamed_Fails()
    {
        IReadOnlyList<string> removed = EnumLock.Compare(["Placed", "Preparing"], ["Placed"]);
        IReadOnlyList<string> renamed = EnumLock.Compare(["Placed", "Preparing"], ["Placed", "Preparation"]);

        Assert.Equal(["'Preparing' is in the lock file but not in the enum (removed or renamed)."], removed);
        Assert.Equal(2, renamed.Count);
    }

    [Fact]
    public void Compare_WhenTheFileAndTheEnumAreEqual_ReportsNothing()
    {
        Assert.Empty(EnumLock.Compare(["B", "A"], ["A", "B"]));
    }

    private static List<Type> StoredEnums()
    {
        DbContextOptionsBuilder<Shopfolio.Infrastructure.AppDbContext> options = new();
        Shopfolio.Infrastructure.AppDbContext.Configure(options, "Host=localhost;Database=shopfolio_lock_model");
        using Shopfolio.Infrastructure.AppDbContext context = new(options.Options);
        return ApplicationEnums.Persistent().Concat(ModelEnums(context)).Distinct().ToList();
    }

    private static IEnumerable<Type> ModelEnums(DbContext context)
    {
        IModel model = context.GetService<IDesignTimeModel>().Model;
        return model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Select(p => Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType)
            .Where(t => t.IsEnum)
            .Distinct();
    }
}
