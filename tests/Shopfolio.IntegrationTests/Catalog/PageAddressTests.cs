using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;

namespace Shopfolio.IntegrationTests.Catalog;

/// <summary>
/// The page addresses of products and categories on a real PostgreSQL (T363 criteria 1, 2, 4; KK1, KK2, KK4; 02 3.30.1, 3.30.2, 3.30.4): the address is made from the name and not from the
/// tree, follows the name until the first publication and is fixed from it, a draft's and an archived product's address counts as used, a deleted product's address is free again,
/// a name with no Latin letter or digit gives the type's root, a category's address is fixed when it is created, and an address that holds nothing a visitor may see is one and the same
/// "page not found". It shares the category tree collection's database.
/// </summary>
[Collection(CategoryTreeAndListsDefinition.Name)]
public sealed class PageAddressTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    public PageAddressTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE product_categories, categories, variants, option_values, options, products CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the product's address does not depend on the tree ----

    [Fact]
    public async Task AProductsAddress_AndTheCategoriesAddresses_StayWhenTheCategoryIsRenamedOrMoved_KK1()
    {
        Guid parent = await CreateCategoryAsync("Mobilya", null);
        Guid child = await CreateCategoryAsync("Masalar", null);
        Product product = await SeedProductAsync("Ahşap Masa", "ahsap-masa", PublishStatus.Published);
        ProductCategoriesResult linked = await SetLinksAsync(product.Id, [child], child);
        Assert.Equal(ProductCategoriesOutcome.Written, linked.Outcome);

        CategoryWriteResult moved = await RunTreeAsync((tree, ct) => tree.MoveAsync(child, parent, ct));
        CategoryWriteResult renamed = await RunTreeAsync((tree, ct) => tree.RenameAsync(child, "Yemek Masaları", ct));

        Assert.Equal(CategoryWriteOutcome.Written, moved.Outcome);
        Assert.Equal(CategoryWriteOutcome.Written, renamed.Outcome);
        Assert.Equal("ahsap-masa", await ScalarAsync<string>($"SELECT slug FROM products WHERE id = '{product.Id}'"));
        Assert.Equal("masalar", await ScalarAsync<string>($"SELECT slug FROM categories WHERE id = '{child}'"));
        Assert.Equal("mobilya", await ScalarAsync<string>($"SELECT slug FROM categories WHERE id = '{parent}'"));
    }

    [Fact]
    public async Task ACategorysAddress_IsFixedWhenItIsCreated_ClashesTakeANumber_AndANameWithoutLatinLettersGivesTheRoot_KK2()
    {
        Guid first = await CreateCategoryAsync("Çelik Kapı", null);
        Guid second = await CreateCategoryAsync("çelik kapi", null);
        Guid blank = await CreateCategoryAsync("!!!", null);
        Guid blankToo = await CreateCategoryAsync("???", null);

        Assert.Equal("celik-kapi", await ScalarAsync<string>($"SELECT slug FROM categories WHERE id = '{first}'"));
        Assert.Equal("celik-kapi-2", await ScalarAsync<string>($"SELECT slug FROM categories WHERE id = '{second}'"));
        Assert.Equal("kategori", await ScalarAsync<string>($"SELECT slug FROM categories WHERE id = '{blank}'"));
        Assert.Equal("kategori-2", await ScalarAsync<string>($"SELECT slug FROM categories WHERE id = '{blankToo}'"));

        CategoryWriteResult renamed = await RunTreeAsync((tree, ct) => tree.RenameAsync(first, "Tamamen Başka", ct));

        Assert.Equal(CategoryWriteOutcome.Written, renamed.Outcome);
        Assert.Equal("celik-kapi", await ScalarAsync<string>($"SELECT slug FROM categories WHERE id = '{first}'"));
    }

    // ---- KK2: the product's address ----

    [Fact]
    public async Task AProductsAddress_IsMadeFromTheName_TurkishLettersAreSimplified_AndAClashTakesTheNextNumber_KK2()
    {
        string first = await AddressForAsync("Çelik Kapı");
        await SeedProductAsync("Çelik Kapı", first, PublishStatus.Draft);
        string second = await AddressForAsync("çelik KAPI");
        await SeedProductAsync("çelik KAPI", second, PublishStatus.Published);
        string third = await AddressForAsync("Çelik Kapı");

        Assert.Equal("celik-kapi", first);
        Assert.Equal("celik-kapi-2", second);
        Assert.Equal("celik-kapi-3", third);
    }

    [Fact]
    public async Task ADraftsAndAnArchivedProductsAddress_CountAsUsed_ButAPermanentlyDeletedOnesIsFreeAgain_KK2()
    {
        Product draft = await SeedProductAsync("Masa", "masa", PublishStatus.Draft);
        await SeedProductAsync("Masa", "masa-2", PublishStatus.Archived);

        Assert.Equal("masa-3", await AddressForAsync("Masa"));

        await ExecuteAsync($"DELETE FROM products WHERE id = '{draft.Id}'");

        Assert.Equal("masa", await AddressForAsync("Masa"));
        Assert.Equal("masa", await AddressForAsync("masa"));
    }

    [Fact]
    public async Task ANameWithoutALatinLetterOrDigit_GivesTheRootOfTheType_AndTheNextOneTakesANumber_KK2()
    {
        string first = await AddressForAsync("!!! ???");
        await SeedProductAsync("!!! ???", first, PublishStatus.Draft);

        Assert.Equal("urun", first);
        Assert.Equal("urun-2", await AddressForAsync("***"));
    }

    [Fact]
    public async Task ADraftProductsAddress_FollowsTheNameUntilTheFirstPublication_AndASameNameKeepsItsOwnAddress_KK2()
    {
        Product product = await SeedProductAsync("Masa", "masa", PublishStatus.Draft);

        Assert.Equal("masa", await ForSaveAsync("Masa", product));
        Assert.Equal("sandalye", await ForSaveAsync("Sandalye", product));
        await SeedProductAsync("Sandalye", "sandalye", PublishStatus.Draft);
        Assert.Equal("sandalye-2", await ForSaveAsync("Sandalye", product));
    }

    [Fact]
    public async Task APublishedProductsAddress_IsFixed_WhateverTheNameBecomes_AndTheEntityRefusesAnotherAddress_KK2()
    {
        Product product = await SeedProductAsync("Masa", "masa", PublishStatus.Published);
        product.SlugFixedAt = Now;

        Assert.Equal("masa", await ForSaveAsync("Tamamen Başka Bir Ad", product));
        Assert.Throws<InvalidOperationException>(() => product.SetAddress("baska"));
        product.SetAddress("masa");
        Assert.Equal("masa", product.Slug);
    }

    [Fact]
    public async Task TheFirstPublication_FixesTheAddress_AndTheWayBackToDraftKeepsIt_KK2()
    {
        Product product = Product.Create("Masa", "masa", ProductType.Digital, 20m, Now);
        Assert.Null(product.SlugFixedAt);

        product.ApplyPublishStatus(PublishStatus.Published, Now);
        Assert.Equal(Now, product.SlugFixedAt);
        product.ApplyPublishStatus(PublishStatus.Draft, Now.AddDays(1));
        product.ApplyPublishStatus(PublishStatus.Published, Now.AddDays(2));

        Assert.Equal(Now, product.SlugFixedAt);
        Assert.Equal("masa", await ForSaveAsync("Başka", product));
    }

    // ---- KK4: the three states are one page ----

    [Fact]
    public async Task AnUnknownAddress_ADeletedProductsAddress_AndADraftSeenByAVisitor_AreTheSamePageNotFound_KK4()
    {
        Product draft = await SeedProductAsync("Taslak Masa", "taslak-masa", PublishStatus.Draft);
        Product gone = await SeedProductAsync("Silinen Masa", "silinen-masa", PublishStatus.Published);
        await ExecuteAsync($"DELETE FROM products WHERE id = '{gone.Id}'");

        ProductPage unknown = await ReadPageAsync("hic-olmayan", isAdministrator: false);
        ProductPage deleted = await ReadPageAsync("silinen-masa", isAdministrator: false);
        ProductPage draftForVisitor = await ReadPageAsync("taslak-masa", isAdministrator: false);
        ProductPage nothing = await ReadPageAsync(null, isAdministrator: false);

        Assert.All([unknown, deleted, draftForVisitor, nothing], page => Assert.Same(ProductPage.NotFound, page));
        Assert.Equal(ProductPageKind.DraftPreview, (await ReadPageAsync("taslak-masa", isAdministrator: true)).Kind);
        Assert.NotEqual(draft.Id, Guid.Empty);
    }

    [Fact]
    public async Task ADeletedProductsAddress_OpensTheNewProductThatTookIt_TheKnownRiskOfK568_KK4()
    {
        Product gone = await SeedProductAsync("Masa", "masa", PublishStatus.Published);
        await ExecuteAsync($"DELETE FROM products WHERE id = '{gone.Id}'");
        Assert.Same(ProductPage.NotFound, await ReadPageAsync("masa", isAdministrator: false));

        string address = await AddressForAsync("Masa");
        Product next = await SeedProductAsync("Masa", address, PublishStatus.Published);
        ProductPage page = await ReadPageAsync("masa", isAdministrator: false);

        Assert.Equal("masa", address);
        Assert.Equal(ProductPageKind.Published, page.Kind);
        Assert.Equal(next.Id, page.ProductId);
    }

    // ---- helpers ----

    private async Task<ProductPage> ReadPageAsync(string? slug, bool isAdministrator)
    {
        await using AppDbContext context = NewContext();
        return await new ProductPageReader(context).ReadAsync(slug, isAdministrator, Ct);
    }

    private async Task<string> AddressForAsync(string name)
    {
        await using AppDbContext context = NewContext();
        return await ProductAddresses.ForSaveAsync(context, name, string.Empty, null, Ct);
    }

    private async Task<string> ForSaveAsync(string name, Product product)
    {
        await using AppDbContext context = NewContext();
        return await ProductAddresses.ForSaveAsync(context, name, product.Slug, product.SlugFixedAt, Ct);
    }

    private async Task<Product> SeedProductAsync(string name, string slug, PublishStatus status)
    {
        Product product = Product.Create(name, slug, ProductType.Digital, 20m, Now);
        product.PublishStatus = status;
        await using AppDbContext context = NewContext();
        context.Products.Add(product);
        await context.SaveChangesAsync(Ct);
        return product;
    }

    private async Task<Guid> CreateCategoryAsync(string name, Guid? parent)
    {
        CategoryWriteResult result = await RunTreeAsync((tree, ct) => tree.CreateAsync(name, parent, ct));
        Assert.Equal(CategoryWriteOutcome.Written, result.Outcome);
        return result.CategoryId!.Value;
    }

    private async Task<CategoryWriteResult> RunTreeAsync(Func<CategoryTree, CancellationToken, Task<CategoryWriteResult>> work)
    {
        await using AppDbContext context = NewContext();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        CategoryTree tree = new(context, new AdvisoryLocks(context), new CacheInvalidations(context), TimeProvider.System);
        CategoryWriteResult result = await work(tree, Ct);
        if (result.IsRejected)
        {
            await transaction.RollbackAsync(Ct);
            return result;
        }

        await context.SaveChangesAsync(Ct);
        await transaction.CommitAsync(Ct);
        return result;
    }

    private async Task<ProductCategoriesResult> SetLinksAsync(Guid productId, Guid[] categories, Guid? primary)
    {
        await using AppDbContext context = NewContext();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        ProductCategoriesResult result = await new ProductCategoryLinks(context, TimeProvider.System, new CacheInvalidations(context)).SetAsync(productId, categories, primary, Ct);
        if (result.IsRejected)
        {
            await transaction.RollbackAsync(Ct);
            return result;
        }

        await context.SaveChangesAsync(Ct);
        await transaction.CommitAsync(Ct);
        return result;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }
}
