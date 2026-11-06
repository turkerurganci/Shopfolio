using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the paging tests (T210): one database copied from the template.</summary>
[CollectionDefinition(ListPagingDefinition.Name)]
public sealed class ListPagingDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ListPaging";
}

/// <summary>
/// The paging helper and the search pattern on a real PostgreSQL (T210 criteria 3, 4, 7; KK3, KK4, KK7; 07 2.6.2, 2.6.5; 09 10.5 items 1, 2, 5, 6; 09 20 item 2):
/// records of an equal order key that fall on a page boundary are neither repeated nor lost for every page size, in the descending and the ascending
/// direction; a page past the last is the last, a value past the <c>int</c> range is the last; a list that fits one page has one page; the wildcard
/// characters of a search word match themselves.
/// </summary>
[Collection(ListPagingDefinition.Name)]
public sealed class ListPagingTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Moment = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    public ListPagingTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await ListSeed.ClearAsync(_database.Dsn);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static string[] Names(int count) => [.. Enumerable.Range(1, count).Select(ListSeed.NameOf)];

    // --- KK4: equal keys, finished by the inner key, in both directions, for every page size ---

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(25)]
    public async Task EqualKeys_OnPageBoundaries_AreNeitherRepeatedNorLost_Descending(int pageSize)
    {
        IReadOnlyList<Guid> ids = await ListSeed.SeedTiedAsync(_database.Dsn, Names(12), Moment, PublishStatus.Published);
        await using AppDbContext db = ListSeed.Open(_database.Dsn);

        List<Guid> walked = await WalkAsync(db, pageSize, descending: true);

        Assert.Equal(ids.Count, walked.Count);
        Assert.Equal(ids.OrderByDescending(id => id).ToList(), walked);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(25)]
    public async Task EqualKeys_OnPageBoundaries_AreNeitherRepeatedNorLost_Ascending(int pageSize)
    {
        IReadOnlyList<Guid> ids = await ListSeed.SeedTiedAsync(_database.Dsn, Names(12), Moment, PublishStatus.Published);
        await using AppDbContext db = ListSeed.Open(_database.Dsn);

        List<Guid> walked = await WalkAsync(db, pageSize, descending: false);

        Assert.Equal(ids.OrderBy(id => id).ToList(), walked);
    }

    private static async Task<List<Guid>> WalkAsync(AppDbContext db, int pageSize, bool descending)
    {
        // The direction of the inner key follows the first key: newest first sorts it descending, the other sorts ascending (07 2.6.5; 09 10.5 item 1).
        IOrderedQueryable<Guid> Ordered() => descending
            ? db.Products.AsNoTracking().OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id).Select(p => p.Id).OrderBy(_ => 0)
            : db.Products.AsNoTracking().OrderBy(p => p.CreatedAt).ThenBy(p => p.Id).Select(p => p.Id).OrderBy(_ => 0);

        List<Guid> walked = [];
        PagedList<Guid> first = await PageOfAsync(db, descending, 1, pageSize);
        for (int page = 1; page <= first.PageCount; page++)
        {
            PagedList<Guid> current = await PageOfAsync(db, descending, page, pageSize);
            Assert.Equal(page, current.PageNumber);
            walked.AddRange(current.Items);
        }

        _ = Ordered;
        return walked;
    }

    private static Task<PagedList<Guid>> PageOfAsync(AppDbContext db, bool descending, int page, int pageSize)
    {
        IOrderedQueryable<Product> ordered = descending
            ? db.Products.AsNoTracking().OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            : db.Products.AsNoTracking().OrderBy(p => p.CreatedAt).ThenBy(p => p.Id);
        return ordered.Select(p => p.Id).OrderBy(id => 0).ToPageAsync(page, pageSize, Ct);
    }

    // --- KK3: the page asked for and the page shown ---

    [Fact]
    public async Task APagePastTheLast_IsTheLastPage_AndAValuePastTheIntRange_IsTheLastPage()
    {
        await ListSeed.SeedPublishedAsync(_database.Dsn, 60);
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        IOrderedQueryable<Product> ordered = db.Products.AsNoTracking().OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);

        PagedList<Product> third = await ordered.ToPageAsync(3, PageSizes.StorefrontProducts, Ct);
        PagedList<Product> past = await ordered.ToPageAsync(4, PageSizes.StorefrontProducts, Ct);
        PagedList<Product> huge = await ordered.ToPageAsync(PageNumber.Parse("99999999999"), PageSizes.StorefrontProducts, Ct);
        PagedList<Product> zero = await ordered.ToPageAsync(0, PageSizes.StorefrontProducts, Ct);

        Assert.Equal(3, third.PageCount);
        Assert.Equal(60, third.TotalCount);
        Assert.Equal(12, third.Items.Count);
        Assert.Equal(3, past.PageNumber);
        Assert.Equal(third.Items.Select(p => p.Id), past.Items.Select(p => p.Id));
        Assert.Equal(3, huge.PageNumber);
        Assert.Equal(third.Items.Select(p => p.Id), huge.Items.Select(p => p.Id));
        Assert.Equal(1, zero.PageNumber);
        Assert.Equal(PageSizes.StorefrontProducts, zero.Items.Count);
    }

    [Fact]
    public async Task AListThatFitsOnePage_HasOnePage_AndAnEmptyListIsOneEmptyPage()
    {
        await ListSeed.SeedPublishedAsync(_database.Dsn, 3);
        await using AppDbContext db = ListSeed.Open(_database.Dsn);

        PagedList<Product> small = await db.Products.AsNoTracking().OrderBy(p => p.CreatedAt).ThenBy(p => p.Id).ToPageAsync(5, PageSizes.Panel, Ct);
        PagedList<Product> none = await db.Products.AsNoTracking().Where(p => p.Slug == "none").OrderBy(p => p.Id).ToPageAsync(5, PageSizes.Panel, Ct);

        Assert.Equal(1, small.PageCount);
        Assert.False(small.ShowsNavigation);
        Assert.Equal(3, small.Items.Count);
        Assert.Equal(1, none.PageCount);
        Assert.Equal(1, none.PageNumber);
        Assert.Empty(none.Items);
        Assert.Equal(0, none.TotalCount);
    }

    // --- KK7 and 09 10.5 item 6: the wildcard characters match themselves ---

    [Fact]
    public async Task APercentSign_IsNotAWildcard_AnUnderscoreMatchesNoLetter_AndABackslashIsLiteral()
    {
        await ListSeed.SeedTiedAsync(_database.Dsn, ["Indirim 50% urun", "Ahsap masa", "Alt_cizgi", "Ters\\bolu", "Isıtıcı", "AMPUL"], Moment, PublishStatus.Published);
        await using AppDbContext db = ListSeed.Open(_database.Dsn);

        Assert.Equal(["Indirim 50% urun"], await SearchAsync(db, "%"));
        Assert.Equal(["Alt_cizgi"], await SearchAsync(db, "_"));
        Assert.Equal(["Ters\\bolu"], await SearchAsync(db, "\\"));
        Assert.Equal(["Isıtıcı"], await SearchAsync(db, "isitici"));
        Assert.Equal(["Isıtıcı"], await SearchAsync(db, "ISITICI"));
        Assert.Equal(["AMPUL"], await SearchAsync(db, "ampul"));
        Assert.Equal(["AMPUL"], await SearchAsync(db, "  AMPUL  "));
        Assert.Equal(["Ahsap masa", "Alt_cizgi"], await SearchAsync(db, "a", ordered: true));
    }

    [Fact]
    public async Task ABlankWord_MakesNoPattern_SoNoQueryRuns()
    {
        Assert.Null(SearchTerm.ToLikePattern("   "));
        await Task.CompletedTask;
    }

    private static async Task<string[]> SearchAsync(AppDbContext db, string word, bool ordered = false)
    {
        string pattern = SearchTerm.ToLikePattern(word)!;
        IQueryable<string> names = db.Products.AsNoTracking()
            .Where(p => EF.Functions.Like(p.NameSearchForm, pattern, SearchTerm.EscapeCharacter))
            .Select(p => p.Name);
        string[] found = await names.ToArrayAsync(Ct);
        return ordered ? [.. found.Order(StringComparer.Ordinal)] : found;
    }
}
