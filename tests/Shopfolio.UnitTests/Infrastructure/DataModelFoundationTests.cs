using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Samples;
using Shopfolio.UnitTests.Samples.Good;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// Unit tests of the data model foundation that need no database (T101): one context, version 7 ids, the empty key guard and the
/// identity normalization.
/// </summary>
public sealed class DataModelFoundationTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Solution_HasOneDbContextClass_InTheInfrastructureFolder()
    {
        Type[] contexts = typeof(AppDbContext).Assembly.GetTypes().Where(t => typeof(DbContext).IsAssignableFrom(t)).ToArray();

        Type context = Assert.Single(contexts);
        Assert.Equal(typeof(AppDbContext), context);
        Assert.Equal("Shopfolio.Infrastructure", context.Namespace);
    }

    [Fact]
    public void SourceFiles_DeclareNoSecondDbContext()
    {
        string root = RepositoryRoot.Find();
        string[] declarations = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(f => File.ReadAllText(f).Contains(": DbContext", StringComparison.Ordinal))
            .Select(f => Path.GetRelativePath(root, f).Replace('\\', '/'))
            .ToArray();

        Assert.Equal(["src/Shopfolio.Web/Infrastructure/AppDbContext.cs"], declarations);
    }

    [Fact]
    public void EntityIds_New_IsVersion7AndSuccessiveCallsDiffer()
    {
        Guid first = EntityIds.New();
        Guid second = EntityIds.New();

        Assert.Equal(7, first.Version);
        Assert.Equal(7, second.Version);
        Assert.NotEqual(first, second);
        Assert.NotEqual(Guid.Empty, first);
    }

    [Fact]
    public void EntityIds_NewStorageKey_IsThirtyTwoHexDigitsOfAVersion7Id()
    {
        string key = EntityIds.NewStorageKey();

        Assert.Equal(32, key.Length);
        Assert.Equal('7', key[12]);
        Assert.Matches("^[0-9a-f]{32}$", key);
    }

    [Theory]
    [InlineData("I", "i")]
    [InlineData("İ", "i")]
    [InlineData("ı", "i")]
    [InlineData("i", "i")]
    [InlineData("  ISPARTA@Örnek.COM \t", "isparta@örnek.com")]
    [InlineData("İzmirLı", "izmirli")]
    public void NormalizeIdentity_FoldsTheFourIsAndTrimsAndLowercases(string input, string expected)
    {
        Assert.Equal(expected, TextNormalizer.NormalizeIdentity(input));
    }

    [Fact]
    public void NormalizeIdentity_TheFourIs_ReachTheSameForm()
    {
        string[] forms = ["I", "İ", "ı", "i"];

        Assert.Single(forms.Select(TextNormalizer.NormalizeIdentity).Distinct());
    }

    [Fact]
    public void NormalizeIdentity_DoesNotDependOnTheCurrentCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            string underTurkish = TextNormalizer.NormalizeIdentity("TITLE-Iİ");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            string underInvariant = TextNormalizer.NormalizeIdentity("TITLE-Iİ");

            Assert.Equal(underInvariant, underTurkish);
            Assert.Equal("title-ii", underTurkish);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void SaveChanges_NewRecordWithoutAKey_FailsBeforeAnySqlIsSent()
    {
        using AppDbContext context = SampleModel.CreateGoodContext();
        context.Set<SampleOwner>().Add(new SampleOwner { CreatedAt = Now });

        // A statement would need the connection; the placeholder host is not reachable, so any other exception type would mean SQL was tried.
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => context.SaveChanges());

        Assert.Contains("EntityIds.New()", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveChangesAsync_NewRecordWithoutAKey_FailsBeforeAnySqlIsSent()
    {
        using AppDbContext context = SampleModel.CreateGoodContext();
        context.Set<SampleOwner>().Add(new SampleOwner { CreatedAt = Now });

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }
}
