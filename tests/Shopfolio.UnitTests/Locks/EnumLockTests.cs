using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure.Errors;
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
    public void EveryEnumOfTheModel_HasALockFileOnDisk_WhateverItsNamespace()
    {
        DbContextOptionsBuilder<Shopfolio.Infrastructure.AppDbContext> options = new();
        Shopfolio.Infrastructure.AppDbContext.Configure(options, "Host=localhost;Database=shopfolio_lock_model");
        using Shopfolio.Infrastructure.AppDbContext context = new(options.Options);
        string root = RepositoryRoot.Find();

        // Independent of StoredEnums(): the files on disk are looked up straight from the model's property types.
        string[] modelEnumsWithoutAFile = ModelEnums(context)
            .Where(t => !File.Exists(EnumLock.FilePath(root, t)))
            .Select(t => t.FullName!)
            .ToArray();

        Assert.Empty(modelEnumsWithoutAFile);
        Assert.NotEmpty(ModelEnums(context));
    }

    [Fact]
    public void TheErrorCodeRegistry_HasALockFile_EqualToItsMembersBothWays_KK1()
    {
        string root = RepositoryRoot.Find();
        string file = EnumLock.FilePath(root, typeof(ErrorCode));

        Assert.Contains(typeof(ErrorCode), StoredEnums());
        Assert.True(File.Exists(file), "The lock file of ErrorCode is missing.");
        Assert.Empty(EnumLock.Compare(EnumLock.ReadNames(File.ReadAllText(file)), Enum.GetNames<ErrorCode>()));
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
        return ApplicationEnums.Persistent().Concat(ModelEnums(context)).Concat(NamedEnums).Distinct().ToList();
    }

    /// <summary>
    /// The enums that are no stored value but whose names are a contract all the same, added to the set by name (D-47 item 2): <c>ErrorCode</c> - the log
    /// and every comparison carry its names, and a code once given does not change (07 6.1.1; 09 8.2 item 1, 12 item 3). <c>NoticeCode</c> joins in T214.
    /// </summary>
    private static readonly Type[] NamedEnums = [typeof(ErrorCode)];

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
