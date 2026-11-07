using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Lists;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the category tree and product list tests (T333): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(CategoryTreeAndListsDefinition.Name)]
public sealed class CategoryTreeAndListsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CategoryTreeAndLists";
}

/// <summary>
/// The category tree, the product links and the storefront lists through the real Catalog code on a real PostgreSQL (T333 criteria 1-8; KK1-KK8; 02 3.4, 3.5; 05 7.9; 06 4.9, 4.10;
/// 09 9.4 item 4, 10.5): at most three levels with siblings in the Turkish alphabetical order of the database; a product hangs on any node, on several, with one main category; a
/// category page lists its whole branch once per product and the count, the pages and the filters follow that scope; a category holding anything cannot be deleted; a move keeps the two
/// limits, shifts the branch together and two opposing moves are put in order by the tree lock; the search reads the product name's simplified form only; and the lists have the price
/// and stock filters and no sort, newest first.
/// </summary>
[Collection(CategoryTreeAndListsDefinition.Name)]
public sealed class CategoryTreeAndListsTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    public CategoryTreeAndListsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE product_categories, categories, variants, option_values, options, products CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: three levels, siblings in the Turkish alphabetical order ----

    [Fact]
    public async Task AFourthLevel_IsRefused_AndNothingIsWritten()
    {
        Guid one = await CreateAsync("Bir", null);
        Guid two = await CreateAsync("İki", one);
        Guid three = await CreateAsync("Üç", two);

        CategoryWriteResult fourth = await RunAsync((tree, ct) => tree.CreateAsync("Dört", three, ct));

        Assert.Equal(CategoryWriteOutcome.DepthExceeded, fourth.Outcome);
        Assert.Equal((long)ProductConstants.CategoryLevelMax, await ScalarAsync<long>("SELECT max(level)::bigint FROM categories"));
        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM categories"));
    }

    [Fact]
    public async Task Siblings_AreInTheTurkishAlphabeticalOrder_AtEveryLevel_WhateverTheCreationOrder()
    {
        Guid root = await CreateAsync("Kök", null);
        foreach (string name in new[] { "Dal", "Çay", "Cam" })
        {
            await CreateAsync(name, root);
        }

        Guid camId = (await ReadTreeAsync()).Single(n => n.Name == "Cam").Id;
        foreach (string name in new[] { "İzmir", "Ilgaz", "Iğdır" })
        {
            await CreateAsync(name, camId);
        }

        IReadOnlyList<CategoryNode> tree = await ReadTreeAsync();

        // Depth first: the root, its children Cam, Çay, Dal - Cam's own children right after Cam - the Turkish order puts ğ before l and the dotless I before the dotted İ.
        Assert.Equal(["Kök", "Cam", "Iğdır", "Ilgaz", "İzmir", "Çay", "Dal"], tree.Select(n => n.Name));
        Assert.Equal([1, 2, 3, 3, 3, 2, 2], tree.Select(n => n.Level));
    }

    [Fact]
    public async Task ACreate_WithABlankNameOrAMissingParent_IsRefused_AndASecondSameNameTakesTheNextAddress()
    {
        Assert.Equal(CategoryWriteOutcome.NameMissing, (await RunAsync((tree, ct) => tree.CreateAsync("   ", null, ct))).Outcome);
        Assert.Equal(CategoryWriteOutcome.NameMissing, (await RunAsync((tree, ct) => tree.CreateAsync(null, null, ct))).Outcome);
        Assert.Equal(CategoryWriteOutcome.ParentNotFound, (await RunAsync((tree, ct) => tree.CreateAsync("X", EntityIds.New(), ct))).Outcome);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM categories"));

        await CreateAsync("Çelik Kapı", null);
        await CreateAsync("Çelik Kapı", null);

        Assert.Equal(["celik-kapi", "celik-kapi-2"], await QueryAsync("SELECT slug FROM categories ORDER BY slug"));
    }

    [Fact]
    public async Task ARename_KeepsTheAddressAndTheLevel_AndMovesThePlaceAmongTheSiblings()
    {
        Guid root = await CreateAsync("Kök", null);
        Guid child = await CreateAsync("Ba", root);
        await CreateAsync("Cc", root);

        CategoryWriteResult renamed = await RunAsync((tree, ct) => tree.RenameAsync(child, "Zz", ct));

        Assert.Equal(CategoryWriteOutcome.Written, renamed.Outcome);
        Assert.Equal(["Kök", "Cc", "Zz"], (await ReadTreeAsync()).Select(n => n.Name));
        Assert.Equal("ba", await ScalarAsync<string>($"SELECT slug FROM categories WHERE id = '{child}'"));
        Assert.Equal(2L, await ScalarAsync<long>($"SELECT level::bigint FROM categories WHERE id = '{child}'"));
        Assert.Equal(CategoryWriteOutcome.NameMissing, (await RunAsync((tree, ct) => tree.RenameAsync(child, " ", ct))).Outcome);
        Assert.Equal(CategoryWriteOutcome.NotFound, (await RunAsync((tree, ct) => tree.RenameAsync(EntityIds.New(), "A", ct))).Outcome);
    }

    [Fact]
    public async Task EveryWrite_LeavesTheLevelAtItsParentsPlusOne()
    {
        Guid a = await CreateAsync("A", null);
        Guid b = await CreateAsync("B", a);
        Guid c = await CreateAsync("C", b);
        Guid d = await CreateAsync("D", null);
        await AssertLevelInvariantAsync();

        await RunAsync((tree, ct) => tree.MoveAsync(b, d, ct));
        await AssertLevelInvariantAsync();
        await RunAsync((tree, ct) => tree.MoveAsync(c, null, ct));
        await AssertLevelInvariantAsync();
        await RunAsync((tree, ct) => tree.MoveAsync(d, a, ct));
        await AssertLevelInvariantAsync();
        Assert.Equal(["A=1", "B=3", "C=1", "D=2"], await QueryAsync("SELECT name || '=' || level FROM categories ORDER BY name"));
    }

    // ---- KK2, KK4: a product hangs on any node and on several; one main category ----

    [Fact]
    public async Task AProduct_HangsOnAnInnerNode_AndTheNodesPageHoldsItBesideItsSubCategories()
    {
        Guid root = await CreateAsync("Elektrik", null);
        Guid cable = await CreateAsync("Kablo", root);
        await CreateAsync("NYA Kablo", cable);
        Product product = await SeedProductAsync("Bobin", Now);

        ProductCategoriesResult linked = await SetLinksAsync(product.Id, [cable], cable);

        Assert.Equal(ProductCategoriesOutcome.Written, linked.Outcome);
        IReadOnlyList<CategoryNode> tree = await ReadTreeAsync();
        CategoryNode node = tree.Single(n => n.Id == cable);
        Assert.Equal(1, node.ChildCount);
        Assert.Equal(1, node.ProductCount);
        Assert.Equal(["Bobin"], (await ListCategoryAsync("kablo")).Items.Select(i => i.Name));
    }

    [Fact]
    public async Task AProductOnSeveralCategories_HasExactlyOneMainCategory_AndTheMarkMoves()
    {
        Guid a = await CreateAsync("A", null);
        Guid b = await CreateAsync("B", null);
        Guid c = await CreateAsync("C", null);
        Product product = await SeedProductAsync("Çok", Now);

        await SetLinksAsync(product.Id, [a, b], b);
        Assert.Equal([b], await PrimaryCategoriesAsync(product.Id));

        await SetLinksAsync(product.Id, [a, b, c], a);
        Assert.Equal([a], await PrimaryCategoriesAsync(product.Id));
        Assert.Equal(3L, await ScalarAsync<long>($"SELECT count(*) FROM product_categories WHERE product_id = '{product.Id}'"));

        await SetLinksAsync(product.Id, [c], c);
        Assert.Equal([c], await PrimaryCategoriesAsync(product.Id));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM product_categories WHERE product_id = '{product.Id}'"));

        await SetLinksAsync(product.Id, [], null);
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM product_categories WHERE product_id = '{product.Id}'"));
    }

    [Fact]
    public async Task TheMainMark_MovesBetweenLinksThatStay_InBothDirections_EveryTime()
    {
        Guid a = await CreateAsync("A", null);
        Guid b = await CreateAsync("B", null);
        Guid c = await CreateAsync("C", null);
        Product product = await SeedProductAsync("Gidiş", Now);
        await SetLinksAsync(product.Id, [a, b, c], a);

        for (int i = 0; i < 12; i++)
        {
            Guid next = new[] { b, c, a }[i % 3];
            await SetLinksAsync(product.Id, [a, b, c], next);
            Assert.Equal([next], await PrimaryCategoriesAsync(product.Id));
        }

        // The marked link goes while a link that stays takes the mark.
        await SetLinksAsync(product.Id, [b, c], c);
        Assert.Equal([c], await PrimaryCategoriesAsync(product.Id));
        await SetLinksAsync(product.Id, [b, c], b);
        Assert.Equal([b], await PrimaryCategoriesAsync(product.Id));
    }

    [Fact]
    public async Task TheMainCategoryRule_RefusesAMissingMark_AMarkOutsideTheSet_AnUnknownCategoryAndAnUnknownProduct()
    {
        Guid a = await CreateAsync("A", null);
        Guid b = await CreateAsync("B", null);
        Product product = await SeedProductAsync("Kural", Now);

        Assert.Equal(ProductCategoriesOutcome.PrimaryMissing, (await SetLinksAsync(product.Id, [a], null)).Outcome);
        Assert.Equal(ProductCategoriesOutcome.PrimaryNotAmongCategories, (await SetLinksAsync(product.Id, [a], b)).Outcome);
        Assert.Equal(ProductCategoriesOutcome.PrimaryNotAmongCategories, (await SetLinksAsync(product.Id, [], a)).Outcome);
        Assert.Equal(ProductCategoriesOutcome.CategoryNotFound, (await SetLinksAsync(product.Id, [EntityIds.New()], null)).Outcome);
        Assert.Equal(ProductCategoriesOutcome.ProductNotFound, (await SetLinksAsync(EntityIds.New(), [a], a)).Outcome);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM product_categories"));
    }

    [Fact]
    public async Task ChangingTheCategories_RaisesTheProductsVersion_AndTheSameSetAgainDoesNot()
    {
        Guid a = await CreateAsync("A", null);
        Product product = await SeedProductAsync("Sürüm", Now);
        long before = await ScalarAsync<int>($"SELECT version FROM products WHERE id = '{product.Id}'");

        await SetLinksAsync(product.Id, [a], a);
        long afterChange = await ScalarAsync<int>($"SELECT version FROM products WHERE id = '{product.Id}'");
        await SetLinksAsync(product.Id, [a], a);
        long afterSame = await ScalarAsync<int>($"SELECT version FROM products WHERE id = '{product.Id}'");

        Assert.Equal(before + 1, afterChange);
        Assert.Equal(afterChange, afterSame);
    }

    // ---- KK3: the category page lists the whole branch ----

    [Fact]
    public async Task ACategoryPage_ListsTheBranchOnce_PerProduct_AndTheCountPagesAndFiltersFollowThatScope()
    {
        Guid root = await CreateAsync("Kök", null);
        Guid child = await CreateAsync("Çocuk", root);
        Guid grandchild = await CreateAsync("Torun", child);
        Guid other = await CreateAsync("Başka", null);
        int size = PageSizes.StorefrontProducts;

        // size + 5 products in the branch (one hung on two categories of it), one outside it.
        // Three shapes by index: on the root and the grandchild (the root marked), on the child alone, on the grandchild alone.
        List<Product> inBranch = [];
        int onGrandchild = 0;
        for (int i = 0; i < size + 5; i++)
        {
            Product product = await SeedProductAsync($"Dal {i:00}", Now.AddMinutes(i));
            await SeedVariantAsync(product.Id, $"DAL-{i:00}", 100m + i, stock: 100, PublishStatus.Published);
            inBranch.Add(product);
            Guid[] hung = (i % 3) switch
            {
                0 => [root, grandchild],
                1 => [child],
                _ => [grandchild],
            };
            onGrandchild += hung.Contains(grandchild) ? 1 : 0;
            await SetLinksAsync(product.Id, hung, hung[0]);
        }

        Product outsider = await SeedProductAsync("Dışarıda", Now.AddHours(1));
        await SetLinksAsync(outsider.Id, [other], other);

        ProductListPage first = await ListCategoryAsync("kok");
        ProductListPage second = await ListCategoryAsync("kok", page: 2);

        Assert.Equal(size + 5, first.TotalCount);
        Assert.Equal(2, first.PageCount);
        Assert.Equal(size, first.Items.Count);
        Assert.Equal(5, second.Items.Count);
        Assert.Equal(inBranch.Count, first.Items.Concat(second.Items).Select(i => i.Id).Distinct().Count());
        Assert.DoesNotContain(first.Items.Concat(second.Items), i => i.Id == outsider.Id);

        // The filters work on the same scope: the prices are 100 + index; from 120 up leaves every product but the first twenty.
        ProductListPage filtered = await ListCategoryAsync("kok", new ProductListFilter(MinPrice: 120m));
        Assert.Equal(inBranch.Count - 20, filtered.TotalCount);

        // A leaf's page holds just the products hung on it.
        Assert.Equal(onGrandchild, (await ListCategoryAsync("torun")).TotalCount);
        Assert.True(onGrandchild < inBranch.Count);
    }

    [Fact]
    public async Task TheListsQueryCount_DoesNotGrowWithThePageFill_ACategoryPageAndASearchPage()
    {
        Guid category = await CreateAsync("Sayım", null);
        Product single = await SeedProductAsync("Sayım tek", Now);
        await SeedVariantAsync(single.Id, "SAYIM-TEK", 10m, stock: 5, PublishStatus.Published);
        await SetLinksAsync(single.Id, [category], category);

        int oneProductCategory = await CountCommandsAsync(context => new ProductListReader(context).ListCategoryAsync("sayim", null, 1, Ct));
        int oneProductSearch = await CountCommandsAsync(context => new ProductListReader(context).SearchAsync("sayım", null, 1, Ct));

        // The page fills up: more than one page of products, each with two variants (the figures read their rows), and the first page is a full one.
        int size = PageSizes.StorefrontProducts;
        for (int i = 0; i < size + 3; i++)
        {
            Product product = await SeedProductAsync($"Sayım dolu {i:00}", Now.AddMinutes(i + 1));
            await SeedVariantAsync(product.Id, $"SAYIM-{i:00}-A", 10m + i, stock: 7, PublishStatus.Published);
            await SeedVariantAsync(product.Id, $"SAYIM-{i:00}-B", 20m + i, stock: 5, PublishStatus.Published);
            await SetLinksAsync(product.Id, [category], category);
        }

        ProductListPage fullCategoryPage = await ListCategoryAsync("sayim");
        Assert.Equal(size, fullCategoryPage.Items.Count);
        Assert.Equal(size, (await SearchAsync("sayım")).Items.Count);

        int fullCategory = await CountCommandsAsync(context => new ProductListReader(context).ListCategoryAsync("sayim", null, 1, Ct));
        int fullSearch = await CountCommandsAsync(context => new ProductListReader(context).SearchAsync("sayım", null, 1, Ct));

        Assert.True(oneProductCategory > 0);
        Assert.Equal(oneProductCategory, fullCategory);
        Assert.Equal(oneProductSearch, fullSearch);
    }

    [Fact]
    public async Task AnEmptyCategory_IsAnEmptyPage_AnUnknownAddressIsNull_AndAPastLastPageShowsTheLast()
    {
        await CreateAsync("Boş", null);
        Guid full = await CreateAsync("Dolu", null);
        Product product = await SeedProductAsync("Tek", Now);
        await SetLinksAsync(product.Id, [full], full);

        ProductListPage empty = await ListCategoryAsync("bos");
        ProductListPage past = await ListCategoryAsync("dolu", page: int.MaxValue);

        Assert.Empty(empty.Items);
        Assert.Equal(0, empty.TotalCount);
        Assert.Equal(1, empty.PageCount);
        Assert.Null(await ListCategoryOrNullAsync("yok"));
        Assert.Null(await ListCategoryOrNullAsync(null));
        Assert.Equal(1, past.PageNumber);
        Assert.Single(past.Items);
    }

    [Fact]
    public async Task OnlyPublishedProducts_AreListed_AnArchivedAndADraftProductAreNot()
    {
        Guid cat = await CreateAsync("Liste", null);
        Product published = await SeedProductAsync("Yayında", Now);
        Product draft = await SeedProductAsync("Taslak", Now.AddMinutes(1), PublishStatus.Draft);
        Product archived = await SeedProductAsync("Arşiv", Now.AddMinutes(2), PublishStatus.Archived);
        foreach (Product product in new[] { published, draft, archived })
        {
            await SetLinksAsync(product.Id, [cat], cat);
        }

        Assert.Equal(["Yayında"], (await ListCategoryAsync("liste")).Items.Select(i => i.Name));
        Assert.Equal(["Yayında"], (await SearchAsync("a")).Items.Select(i => i.Name));
    }

    // ---- KK5: a category that holds something is not deleted ----

    [Fact]
    public async Task ACategoryWithAProductOrASubCategory_IsNotDeleted_AndTheRefusalCarriesTheCounts()
    {
        Guid root = await CreateAsync("Dolu", null);
        await CreateAsync("Alt Bir", root);
        await CreateAsync("Alt İki", root);
        Product first = await SeedProductAsync("Bir", Now);
        Product second = await SeedProductAsync("İki", Now.AddMinutes(1));
        await SetLinksAsync(first.Id, [root], root);
        await SetLinksAsync(second.Id, [root], root);

        CategoryWriteResult blocked = await RunAsync((tree, ct) => tree.DeleteAsync(root, ct));

        Assert.Equal(CategoryWriteOutcome.NotEmpty, blocked.Outcome);
        Assert.Equal(2, blocked.BlockingChildren);
        Assert.Equal(2, blocked.BlockingProducts);
        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM categories"));
    }

    [Fact]
    public async Task AnEmptyCategory_IsDeleted_AndItsAddressIsFreeAgain_AndAMissingOneIsNotFound()
    {
        Guid empty = await CreateAsync("Boşalan", null);

        CategoryWriteResult deleted = await RunAsync((tree, ct) => tree.DeleteAsync(empty, ct));
        Guid again = await CreateAsync("Boşalan", null);

        Assert.Equal(CategoryWriteOutcome.Written, deleted.Outcome);
        Assert.Equal("bosalan", await ScalarAsync<string>($"SELECT slug FROM categories WHERE id = '{again}'"));
        Assert.Equal(CategoryWriteOutcome.NotFound, (await RunAsync((tree, ct) => tree.DeleteAsync(EntityIds.New(), ct))).Outcome);
    }

    [Fact]
    public async Task TheDatabase_IsTheLastDefenseOfADeleteThatSlipsPastTheCounts()
    {
        Guid root = await CreateAsync("Kilit", null);
        Product product = await SeedProductAsync("Bağlı", Now);
        await SetLinksAsync(product.Id, [root], root);

        PostgresException refused = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM categories WHERE id = '{root}'"));

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, refused.SqlState);
    }

    // ---- KK6: a move is free with two limits ----

    [Fact]
    public async Task AMove_UnderItsOwnBranchOrPastTheDepth_IsRefused_AndNoLevelChanges()
    {
        Guid a = await CreateAsync("A", null);
        Guid b = await CreateAsync("B", a);
        Guid c = await CreateAsync("C", b);
        Guid x = await CreateAsync("X", null);
        Guid y = await CreateAsync("Y", x);
        List<string> before = await QueryAsync("SELECT name || ':' || level || ':' || coalesce(parent_category_id::text, '-') FROM categories ORDER BY name");

        CategoryWriteResult itself = await RunAsync((tree, ct) => tree.MoveAsync(a, a, ct));
        CategoryWriteResult intoChild = await RunAsync((tree, ct) => tree.MoveAsync(a, c, ct));
        CategoryWriteResult pastDepth = await RunAsync((tree, ct) => tree.MoveAsync(a, y, ct));
        CategoryWriteResult missingParent = await RunAsync((tree, ct) => tree.MoveAsync(a, EntityIds.New(), ct));
        CategoryWriteResult missingCategory = await RunAsync((tree, ct) => tree.MoveAsync(EntityIds.New(), null, ct));

        Assert.Equal(CategoryWriteOutcome.IntoOwnSubtree, itself.Outcome);
        Assert.Equal(CategoryWriteOutcome.IntoOwnSubtree, intoChild.Outcome);
        Assert.Equal(CategoryWriteOutcome.DepthExceeded, pastDepth.Outcome);
        Assert.Equal(CategoryWriteOutcome.ParentNotFound, missingParent.Outcome);
        Assert.Equal(CategoryWriteOutcome.NotFound, missingCategory.Outcome);
        Assert.Equal(before, await QueryAsync("SELECT name || ':' || level || ':' || coalesce(parent_category_id::text, '-') FROM categories ORDER BY name"));
    }

    [Fact]
    public async Task AMove_ShiftsTheWholeBranchTogether_ToTheRootAndUnderAnotherParent()
    {
        Guid a = await CreateAsync("A", null);
        Guid b = await CreateAsync("B", a);
        Guid c = await CreateAsync("C", b);
        Guid x = await CreateAsync("X", null);

        CategoryWriteResult toRoot = await RunAsync((tree, ct) => tree.MoveAsync(b, null, ct));
        Assert.Equal(CategoryWriteOutcome.Written, toRoot.Outcome);
        Assert.Equal(["A:1", "B:1", "C:2", "X:1"], await QueryAsync("SELECT name || ':' || level FROM categories ORDER BY name"));

        CategoryWriteResult under = await RunAsync((tree, ct) => tree.MoveAsync(b, x, ct));
        Assert.Equal(CategoryWriteOutcome.Written, under.Outcome);
        Assert.Equal(["A:1", "B:2", "C:3", "X:1"], await QueryAsync("SELECT name || ':' || level FROM categories ORDER BY name"));
        Assert.Equal(x, await ScalarAsync<Guid>($"SELECT parent_category_id FROM categories WHERE id = '{b}'"));
        Assert.Equal(b, await ScalarAsync<Guid>($"SELECT parent_category_id FROM categories WHERE id = '{c}'"));
    }

    [Fact]
    public async Task TwoOpposingMoves_AreInOrderedByTheTreeLock_SoOnlyOneHoldsAndNoCycleForms()
    {
        Guid a = await CreateAsync("A", null);
        Guid b = await CreateAsync("B", null);

        for (int round = 0; round < 5; round++)
        {
            Task<CategoryWriteResult> aUnderB = RunAsync((tree, ct) => tree.MoveAsync(a, b, ct));
            Task<CategoryWriteResult> bUnderA = RunAsync((tree, ct) => tree.MoveAsync(b, a, ct));
            CategoryWriteResult[] results = await Task.WhenAll(aUnderB, bUnderA);

            Assert.Equal(1, results.Count(r => r.Outcome == CategoryWriteOutcome.Written));
            Assert.Single(results, r => r.Outcome == CategoryWriteOutcome.IntoOwnSubtree);
            await AssertLevelInvariantAsync();
            Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM categories WHERE parent_category_id IS NULL"));

            await RunAsync((tree, ct) => tree.MoveAsync(a, null, ct));
            await RunAsync((tree, ct) => tree.MoveAsync(b, null, ct));
        }
    }

    [Fact]
    public async Task TheTreeWrites_NameTheCacheAreasTheyMakeStale()
    {
        List<CacheArea> created = await AreasOfAsync(async (tree, ct) => await tree.CreateAsync("Önbellek", null, ct));
        Guid id = await ScalarAsync<Guid>("SELECT id FROM categories WHERE name = 'Önbellek'");
        List<CacheArea> renamed = await AreasOfAsync(async (tree, ct) => await tree.RenameAsync(id, "Önbellek 2", ct));
        List<CacheArea> deleted = await AreasOfAsync(async (tree, ct) => await tree.DeleteAsync(id, ct));

        Assert.Equal([CacheArea.Menu, CacheArea.CategoryTree, CacheArea.Sitemap], created.Order());
        Assert.Equal([CacheArea.Menu, CacheArea.CategoryTree], renamed.Order());
        Assert.Equal([CacheArea.Menu, CacheArea.CategoryTree, CacheArea.Sitemap], deleted.Order());
    }

    // ---- KK7: the search reads the product name only ----

    [Fact]
    public async Task TheSearch_ReadsTheProductNameOnly_NotTheDescriptionTheCategoryNameOrAnOptionValue()
    {
        Guid ampulCategory = await CreateAsync("Ampul", null);
        Product bulb = await SeedProductAsync("Ampul 40W", Now);
        Product heater = await SeedProductAsync("Elektrikli Isıtıcı", Now.AddMinutes(1));
        Product byDescription = await SeedProductAsync("Kablo", Now.AddMinutes(2), description: "Ampul ve ısıtıcı için");
        Product byCategory = await SeedProductAsync("Priz", Now.AddMinutes(3));
        Product byOption = await SeedProductAsync("Anahtar", Now.AddMinutes(4));
        await SetLinksAsync(byCategory.Id, [ampulCategory], ampulCategory);
        await SeedOptionValueAsync(byOption.Id, "Ampul");
        await SeedOptionValueAsync(byOption.Id, "Isıtıcı", order: 2);

        Assert.Equal(["Ampul 40W"], (await SearchAsync("ampul")).Items.Select(i => i.Name));
        Assert.Equal(["Elektrikli Isıtıcı"], (await SearchAsync("ısıtıcı")).Items.Select(i => i.Name));
        Assert.DoesNotContain((await SearchAsync("ampul")).Items, i => i.Id == byDescription.Id || i.Id == byCategory.Id || i.Id == byOption.Id);
        Assert.Equal(bulb.Id, (await SearchAsync("ampul")).Items.Single().Id);
        Assert.Equal(heater.Id, (await SearchAsync("ısıtıcı")).Items.Single().Id);
    }

    [Fact]
    public async Task TheSearch_IsBlindToLetterCaseAndTurkishCharacters_AndMatchesAPartInTheMiddleOfTheName()
    {
        await SeedProductAsync("Ampul 40W", Now);
        await SeedProductAsync("Elektrikli Isıtıcı", Now.AddMinutes(1));

        foreach (string word in new[] { "AMPUL", "ampul", "Ampul", "aMpUl" })
        {
            Assert.Equal(["Ampul 40W"], (await SearchAsync(word)).Items.Select(i => i.Name));
        }

        foreach (string word in new[] { "isıtıcı", "ISITICI", "isitici", "ISITICI", "İSİTİCİ", "ısıtıcı" })
        {
            Assert.Equal(["Elektrikli Isıtıcı"], (await SearchAsync(word)).Items.Select(i => i.Name));
        }

        Assert.Equal(["Elektrikli Isıtıcı"], (await SearchAsync("trikli")).Items.Select(i => i.Name));
        Assert.Equal(["Ampul 40W"], (await SearchAsync("pul 4")).Items.Select(i => i.Name));
        Assert.Empty((await SearchAsync("zzz")).Items);
    }

    [Fact]
    public async Task TheSearch_TakesPercentAndUnderscoreAsLetters_AndAnEmptyWordMakesNoList()
    {
        await SeedProductAsync("Yüzde %50 Fırsat", Now);
        await SeedProductAsync("a_b kodu", Now.AddMinutes(1));
        await SeedProductAsync("Sıradan ürün", Now.AddMinutes(2));

        Assert.Equal(["Yüzde %50 Fırsat"], (await SearchAsync("%")).Items.Select(i => i.Name));
        Assert.Equal(["a_b kodu"], (await SearchAsync("_")).Items.Select(i => i.Name));
        Assert.Equal(["a_b kodu"], (await SearchAsync("a_b")).Items.Select(i => i.Name));
        Assert.Null(await SearchOrNullAsync(""));
        Assert.Null(await SearchOrNullAsync("   "));
        Assert.Null(await SearchOrNullAsync(null));
    }

    // ---- KK8: two filters, no sort, newest first ----

    [Fact]
    public async Task TheLists_HaveNoSortParameterNoHandOrderAndNoOptionValueFilter_AndExactlyTheTwoFilterAxes()
    {
        string[] listMethods = [nameof(ICatalogModule.ListCategoryProductsAsync), nameof(ICatalogModule.SearchProductsAsync)];
        foreach (string name in listMethods)
        {
            ParameterInfo[] parameters = typeof(ICatalogModule).GetMethod(name)!.GetParameters();
            Assert.DoesNotContain(parameters, p => p.Name!.Contains("sort", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("order", StringComparison.OrdinalIgnoreCase));
        }

        string[] axes = [.. typeof(ProductListFilter).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.Name != nameof(ProductListFilter.IsPriceRangeInverted)).Select(p => p.Name)];
        Assert.Equal(
            new[] { nameof(ProductListFilter.MinPrice), nameof(ProductListFilter.MaxPrice), nameof(ProductListFilter.InStockOnly) }.Order(StringComparer.Ordinal),
            axes.Order(StringComparer.Ordinal));

        List<string> columns = await QueryAsync(
            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name IN ('products', 'categories', 'product_categories') ORDER BY 1");
        Assert.DoesNotContain(columns, c => c.Contains("sort", StringComparison.Ordinal) || c.Contains("position", StringComparison.Ordinal) || c.Contains("rank", StringComparison.Ordinal));
        Assert.DoesNotContain(typeof(Product).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), p => p.Name.Contains("Sort", StringComparison.Ordinal));
        Assert.DoesNotContain(typeof(Category).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), p => p.Name.Contains("Sort", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TheLists_AreNewestFirst_EqualMomentsByTheKeyDescending_AndAnUnfilteredListShowsSoldOutProducts()
    {
        Guid cat = await CreateAsync("Sıra", null);
        Product oldest = await SeedProductAsync("Eski", Now);
        Product newest = await SeedProductAsync("Yeni", Now.AddDays(2));
        Product middle = await SeedProductAsync("Orta", Now.AddDays(1));
        Product tieA = await SeedProductAsync("Aynı A", Now.AddDays(1));
        Product tieB = await SeedProductAsync("Aynı B", Now.AddDays(1));
        foreach (Product product in new[] { oldest, newest, middle, tieA, tieB })
        {
            await SetLinksAsync(product.Id, [cat], cat);
            await SeedVariantAsync(product.Id, "SIRA-" + product.Slug, 100m, stock: 0, PublishStatus.Published);
        }

        ProductListPage page = await ListCategoryAsync("sira");

        // The database compares the key bytes, which is the ordinal order of the canonical text of the key.
        Guid[] tied = [.. new[] { middle.Id, tieA.Id, tieB.Id }.OrderByDescending(id => id.ToString("D"), StringComparer.Ordinal)];
        Assert.Equal([newest.Id, .. tied, oldest.Id], page.Items.Select(i => i.Id));
        Assert.All(page.Items, item => Assert.False(item.IsPurchasable));
        Assert.Equal(5, page.TotalCount);
    }

    [Fact]
    public async Task TheStockFilter_KeepsAProductWithOneBuyableVariant_AndDropsOneWhoseVariantsAreSoldOutReservedOrDraft()
    {
        Guid cat = await CreateAsync("Stok", null);
        Product oneBuyable = await SeedProductAsync("Bir satın alınabilir", Now);
        Product unlimited = await SeedProductAsync("Sınırsız", Now.AddMinutes(1), productType: ProductType.Digital);
        Product soldOut = await SeedProductAsync("Tükenmiş", Now.AddMinutes(2));
        Product reserved = await SeedProductAsync("Ayrılmış", Now.AddMinutes(3));
        Product draftOnly = await SeedProductAsync("Taslak varyant", Now.AddMinutes(4));
        foreach (Product product in new[] { oneBuyable, unlimited, soldOut, reserved, draftOnly })
        {
            await SetLinksAsync(product.Id, [cat], cat);
        }

        await SeedVariantAsync(oneBuyable.Id, "S-1A", 100m, stock: 0, PublishStatus.Published);
        await SeedVariantAsync(oneBuyable.Id, "S-1B", 100m, stock: 3, PublishStatus.Published);
        await SeedVariantAsync(unlimited.Id, "S-2", 100m, stock: null, PublishStatus.Published);
        await SeedVariantAsync(soldOut.Id, "S-3A", 100m, stock: 0, PublishStatus.Published);
        await SeedVariantAsync(soldOut.Id, "S-3B", 100m, stock: 0, PublishStatus.Published);
        Variant held = await SeedVariantAsync(reserved.Id, "S-4", 100m, stock: 5, PublishStatus.Published);
        await ExecuteAsync($"UPDATE variants SET reserved_quantity = 5 WHERE id = '{held.Id}'");
        await SeedVariantAsync(draftOnly.Id, "S-5", 100m, stock: 9, PublishStatus.Draft);

        ProductListPage all = await ListCategoryAsync("stok");
        ProductListPage inStock = await ListCategoryAsync("stok", new ProductListFilter(InStockOnly: true));

        Assert.Equal(5, all.TotalCount);
        AssertIds(inStock, oneBuyable.Id, unlimited.Id);
        Assert.True(all.Items.Single(i => i.Id == oneBuyable.Id).IsPurchasable);
        Assert.True(all.Items.Single(i => i.Id == unlimited.Id).IsPurchasable);
        Assert.False(all.Items.Single(i => i.Id == soldOut.Id).IsPurchasable);
        Assert.False(all.Items.Single(i => i.Id == reserved.Id).IsPurchasable);
        Assert.False(all.Items.Single(i => i.Id == draftOnly.Id).IsPurchasable);
    }

    [Fact]
    public async Task ThePriceFilter_ReadsThePublishedVariantsPrices_AndAnInvertedRangeIsNotApplied()
    {
        Guid cat = await CreateAsync("Fiyat", null);
        Product cheap = await SeedProductAsync("Ucuz", Now);
        Product pricey = await SeedProductAsync("Pahalı", Now.AddMinutes(1));
        Product both = await SeedProductAsync("İkisi de", Now.AddMinutes(2));
        foreach (Product product in new[] { cheap, pricey, both })
        {
            await SetLinksAsync(product.Id, [cat], cat);
        }

        await SeedVariantAsync(cheap.Id, "P-1", 50m, stock: 100, PublishStatus.Published);
        await SeedVariantAsync(pricey.Id, "P-2", 500m, stock: 100, PublishStatus.Published);
        await SeedVariantAsync(both.Id, "P-3A", 80m, stock: 100, PublishStatus.Published);
        await SeedVariantAsync(both.Id, "P-3B", 900m, stock: 100, PublishStatus.Published);
        await SeedVariantAsync(cheap.Id, "P-1D", 700m, stock: 100, PublishStatus.Draft);

        AssertIds(await ListCategoryAsync("fiyat", new ProductListFilter(MaxPrice: 100m)), cheap.Id, both.Id);
        AssertIds(await ListCategoryAsync("fiyat", new ProductListFilter(MinPrice: 400m)), pricey.Id, both.Id);
        AssertIds(await ListCategoryAsync("fiyat", new ProductListFilter(MinPrice: 400m, MaxPrice: 600m)), pricey.Id);
        AssertIds(await ListCategoryAsync("fiyat", new ProductListFilter(MinPrice: 50m, MaxPrice: 80m)), cheap.Id, both.Id);
        Assert.Equal(3, (await ListCategoryAsync("fiyat", new ProductListFilter(MinPrice: 600m, MaxPrice: 100m))).TotalCount);
        Assert.Equal(50m, (await ListCategoryAsync("fiyat")).Items.Single(i => i.Id == cheap.Id).LowestPrice);
    }

    [Fact]
    public async Task BothFilters_TogetherAskOneVariantToBeInRangeAndBuyable()
    {
        Guid cat = await CreateAsync("Birlikte", null);
        Product split = await SeedProductAsync("Bölünmüş", Now);
        Product whole = await SeedProductAsync("Bütün", Now.AddMinutes(1));
        await SetLinksAsync(split.Id, [cat], cat);
        await SetLinksAsync(whole.Id, [cat], cat);
        // The variant in range is sold out; the one in stock is out of range.
        await SeedVariantAsync(split.Id, "B-1A", 50m, stock: 0, PublishStatus.Published);
        await SeedVariantAsync(split.Id, "B-1B", 500m, stock: 4, PublishStatus.Published);
        await SeedVariantAsync(whole.Id, "B-2", 60m, stock: 4, PublishStatus.Published);

        ProductListPage both = await ListCategoryAsync("birlikte", new ProductListFilter(MaxPrice: 100m, InStockOnly: true));
        ProductListPage priceOnly = await ListCategoryAsync("birlikte", new ProductListFilter(MaxPrice: 100m));
        ProductListPage stockOnly = await ListCategoryAsync("birlikte", new ProductListFilter(InStockOnly: true));

        AssertIds(both, whole.Id);
        Assert.Equal(2, priceOnly.TotalCount);
        Assert.Equal(2, stockOnly.TotalCount);
    }

    [Fact]
    public async Task TheSearchPage_TakesTheSameFiltersAndTheSamePageSize_AsTheCategoryPage()
    {
        int size = PageSizes.StorefrontProducts;
        for (int i = 0; i < size + 3; i++)
        {
            Product product = await SeedProductAsync($"Ortak {i:00}", Now.AddMinutes(i));
            await SeedVariantAsync(product.Id, $"O-{i:00}", 10m + i, stock: i % 2 == 0 ? 0 : 5, PublishStatus.Published);
        }

        ProductListPage unfiltered = await SearchAsync("ortak");
        ProductListPage inStock = await SearchAsync("ortak", new ProductListFilter(InStockOnly: true));
        ProductListPage ranged = await SearchAsync("ortak", new ProductListFilter(MinPrice: 20m, MaxPrice: 22m));

        // Prices are 10 + index, so 20 to 22 holds the indexes 10, 11 and 12; the products with an odd index are in stock.
        Assert.Equal(size + 3, unfiltered.TotalCount);
        Assert.Equal(size, unfiltered.Items.Count);
        Assert.Equal(2, unfiltered.PageCount);
        Assert.Equal(Enumerable.Range(0, size + 3).Count(i => i % 2 == 1), inStock.TotalCount);
        Assert.Equal(3, ranged.TotalCount);
        Assert.Equal("Ortak 12", ranged.Items[0].Name);
        Assert.Equal($"Ortak {size + 2:00}", unfiltered.Items[0].Name);
    }

    // ---- page boundary: equal moments across the page edge (09 10.5 item 6, 19.2 item 5, 25) ----

    [Fact]
    public async Task ACategoryPage_WithEqualCreatedAtAcrossThePageBoundary_RepeatsNoProductAndLosesNone_TheLastPageIncluded()
    {
        Guid category = await CreateAsync("Sınır", null);
        int size = PageSizes.StorefrontProducts;
        int count = (2 * size) + 7;
        HashSet<Guid> seeded = [];
        for (int i = 0; i < count; i++)
        {
            Product product = await SeedProductAsync($"Eşit {i:00}", Now);
            await SeedVariantAsync(product.Id, $"E-{i:00}", 10m + i, stock: i % 2 == 0 ? 0 : 5, PublishStatus.Published);
            await SetLinksAsync(product.Id, [category], category);
            seeded.Add(product.Id);
        }

        await AssertWholeListIsWalkedOnceAsync(page => ListCategoryAsync("sinir", page: page), seeded, size);

        // A filtered list crosses the boundary too: the odd indexes are in stock, and their count is more than one page.
        int expected = Enumerable.Range(0, count).Count(i => i % 2 == 1);
        Assert.True(expected > size);
        ProductListFilter filter = new(InStockOnly: true);
        List<Guid> walked = [];
        ProductListPage first = await ListCategoryAsync("sinir", filter, 1);
        Assert.Equal(expected, first.TotalCount);
        for (int page = 1; page <= first.PageCount; page++)
        {
            walked.AddRange((await ListCategoryAsync("sinir", filter, page)).Items.Select(i => i.Id));
        }

        Assert.Equal(expected, walked.Count);
        Assert.Equal(expected, walked.Distinct().Count());
        Assert.All(walked, id => Assert.Contains(id, seeded));
    }

    [Fact]
    public async Task TheSearchPage_WithEqualCreatedAtAcrossThePageBoundary_RepeatsNoProductAndLosesNone_TheLastPageIncluded()
    {
        int size = PageSizes.StorefrontProducts;
        int count = (2 * size) + 7;
        HashSet<Guid> seeded = [];
        for (int i = 0; i < count; i++)
        {
            Product product = await SeedProductAsync($"Aynı an {i:00}", Now);
            await SeedVariantAsync(product.Id, $"A-{i:00}", 10m + i, stock: 7, PublishStatus.Published);
            seeded.Add(product.Id);
        }

        await AssertWholeListIsWalkedOnceAsync(page => SearchAsync("aynı an", page: page), seeded, size);

        // With a price filter the matching set is smaller than the list but still crosses the boundary: indexes 10 to 40 are 31 products.
        ProductListFilter filter = new(MinPrice: 20m, MaxPrice: 50m);
        ProductListPage firstFiltered = await SearchAsync("aynı an", filter, 1);
        Assert.Equal(31, firstFiltered.TotalCount);
        List<Guid> walked = [];
        for (int page = 1; page <= firstFiltered.PageCount; page++)
        {
            walked.AddRange((await SearchAsync("aynı an", filter, page)).Items.Select(i => i.Id));
        }

        Assert.Equal(31, walked.Count);
        Assert.Equal(31, walked.Distinct().Count());
    }

    [Fact]
    public async Task TheSearch_TakesABackslashAsALetter()
    {
        await SeedProductAsync(@"Yol a\b modeli", Now);
        await SeedProductAsync("Yol ab modeli", Now.AddMinutes(1));
        await SeedProductAsync("Yol a b modeli", Now.AddMinutes(2));

        Assert.Equal([@"Yol a\b modeli"], (await SearchAsync(@"a\b")).Items.Select(i => i.Name));
        Assert.Equal([@"Yol a\b modeli"], (await SearchAsync(@"\")).Items.Select(i => i.Name));
        Assert.Empty((await SearchAsync(@"a\\b")).Items);
    }

    // ---- helpers ----

    private static async Task AssertWholeListIsWalkedOnceAsync(Func<int, Task<ProductListPage>> readPage, HashSet<Guid> seeded, int size)
    {
        ProductListPage first = await readPage(1);
        Assert.Equal(seeded.Count, first.TotalCount);
        Assert.Equal((seeded.Count + size - 1) / size, first.PageCount);
        Assert.True(first.PageCount > 2);

        List<Guid> walked = [];
        for (int page = 1; page <= first.PageCount; page++)
        {
            ProductListPage read = await readPage(page);
            Assert.Equal(page < first.PageCount ? size : seeded.Count - (size * (first.PageCount - 1)), read.Items.Count);
            walked.AddRange(read.Items.Select(i => i.Id));
        }

        Assert.Equal(seeded.Count, walked.Count);
        Assert.Equal(seeded.Count, walked.Distinct().Count());
        Assert.True(seeded.SetEquals(walked));
    }

    private static void AssertIds(ProductListPage page, params Guid[] expected)
    {
        Assert.Equal(expected.Order(), page.Items.Select(i => i.Id).Order());
    }

    private async Task<Guid> CreateAsync(string name, Guid? parent)
    {
        CategoryWriteResult result = await RunAsync((tree, ct) => tree.CreateAsync(name, parent, ct));
        Assert.Equal(CategoryWriteOutcome.Written, result.Outcome);
        return result.CategoryId!.Value;
    }

    private async Task<IReadOnlyList<CategoryNode>> ReadTreeAsync()
    {
        await using AppDbContext context = NewContext();
        return await NewTree(context).ReadAsync(Ct);
    }

    private async Task<ProductCategoriesResult> SetLinksAsync(Guid productId, Guid[] categories, Guid? primary)
    {
        await using AppDbContext context = NewContext();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        ProductCategoriesResult result = await new ProductCategoryLinks(context, TimeProvider.System).SetAsync(productId, categories, primary, Ct);
        if (result.IsRejected)
        {
            await transaction.RollbackAsync(Ct);
            return result;
        }

        await context.SaveChangesAsync(Ct);
        await transaction.CommitAsync(Ct);
        return result;
    }

    private async Task<ProductListPage> ListCategoryAsync(string? slug, ProductListFilter? filter = null, int page = 1)
    {
        ProductListPage? result = await ListCategoryOrNullAsync(slug, filter, page);
        Assert.NotNull(result);
        return result;
    }

    private async Task<ProductListPage?> ListCategoryOrNullAsync(string? slug, ProductListFilter? filter = null, int page = 1)
    {
        await using AppDbContext context = NewContext();
        return await new ProductListReader(context).ListCategoryAsync(slug, filter, page, Ct);
    }

    private async Task<ProductListPage?> SearchOrNullAsync(string? word, ProductListFilter? filter = null, int page = 1)
    {
        await using AppDbContext context = NewContext();
        return await new ProductListReader(context).SearchAsync(word, filter, page, Ct);
    }

    private async Task<ProductListPage> SearchAsync(string? word, ProductListFilter? filter = null, int page = 1)
    {
        ProductListPage? result = await SearchOrNullAsync(word, filter, page);
        Assert.NotNull(result);
        return result;
    }

    private async Task<List<Guid>> PrimaryCategoriesAsync(Guid productId)
    {
        List<string> rows = await QueryAsync($"SELECT category_id::text FROM product_categories WHERE product_id = '{productId}' AND is_primary_category");
        return [.. rows.Select(Guid.Parse)];
    }

    private async Task AssertLevelInvariantAsync()
    {
        Assert.Equal(0L, await ScalarAsync<long>(
            "SELECT count(*) FROM categories c LEFT JOIN categories p ON p.id = c.parent_category_id WHERE c.level <> coalesce(p.level, 0) + 1"));
    }

    private static CategoryTree NewTree(AppDbContext context) => new(context, new AdvisoryLocks(context), new CacheInvalidations(context), TimeProvider.System);

    /// <summary>Runs a tree write the way the transaction runner does: one transaction, saved and committed unless the result is a refusal, which rolls it back.</summary>
    private async Task<CategoryWriteResult> RunAsync(Func<CategoryTree, CancellationToken, Task<CategoryWriteResult>> work)
    {
        await using AppDbContext context = NewContext();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        CategoryWriteResult result = await work(NewTree(context), Ct);
        if (result.IsRejected)
        {
            await transaction.RollbackAsync(Ct);
            return result;
        }

        await context.SaveChangesAsync(Ct);
        await transaction.CommitAsync(Ct);
        return result;
    }

    private async Task<List<CacheArea>> AreasOfAsync(Func<CategoryTree, CancellationToken, Task<CategoryWriteResult>> work)
    {
        await using AppDbContext context = NewContext();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        CacheInvalidations invalidations = new(context);
        CategoryTree tree = new(context, new AdvisoryLocks(context), invalidations, TimeProvider.System);
        CategoryWriteResult result = await work(tree, Ct);
        Assert.Equal(CategoryWriteOutcome.Written, result.Outcome);
        await context.SaveChangesAsync(Ct);
        await transaction.CommitAsync(Ct);
        return [.. invalidations.TakePendingAreas()];
    }

    private async Task<Product> SeedProductAsync(string name, DateTimeOffset createdAt, PublishStatus status = PublishStatus.Published, string? description = null, ProductType productType = ProductType.Physical)
    {
        string slug = Slug.From(name, "urun") + "-" + Guid.NewGuid().ToString("N")[..6];
        Product product = Product.Create(name, slug, productType, 20m, createdAt);
        product.PublishStatus = status;
        product.SetDescription(description);
        await SaveAsync(c => c.Products.Add(product));
        return product;
    }

    private async Task<Variant> SeedVariantAsync(Guid productId, string sku, decimal price, int? stock, PublishStatus status)
    {
        Variant variant = Variant.Create(productId, sku, price, Now);
        variant.PublishStatus = status;
        variant.StockQuantity = stock;

        // A product's variants differ by their option values (the unique key of a product's variants): the product's one option gets a value per variant.
        await using AppDbContext context = NewContext();
        Option? option = await context.Options.SingleOrDefaultAsync(o => o.ProductId == productId, Ct);
        if (option is null)
        {
            option = Option.Create(productId, "Seçenek", 1, Now);
            context.Options.Add(option);
        }

        int valueCount = await context.OptionValues.CountAsync(v => v.OptionId == option.Id, Ct);
        OptionValue optionValue = OptionValue.Create(option.Id, sku, valueCount + 1, Now);
        context.OptionValues.Add(optionValue);
        variant.FirstOptionValueId = optionValue.Id;
        context.Variants.Add(variant);
        await context.SaveChangesAsync(Ct);
        return variant;
    }

    private async Task SeedOptionValueAsync(Guid productId, string value, int order = 1)
    {
        Option option = Option.Create(productId, "Seçenek " + order, order, Now);
        OptionValue optionValue = OptionValue.Create(option.Id, value, 1, Now);
        await SaveAsync(c =>
        {
            c.Options.Add(option);
            c.OptionValues.Add(optionValue);
        });
    }

    /// <summary>Counts the commands one read sends, by a command interceptor on a fresh context.</summary>
    private async Task<int> CountCommandsAsync(Func<AppDbContext, Task<ProductListPage?>> read)
    {
        CommandCounter counter = new();
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn).AddInterceptors(counter);
        await using AppDbContext context = new(builder.Options);
        Assert.NotNull(await read(context));
        return counter.Count;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        await context.SaveChangesAsync(Ct);
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

    private async Task<List<string>> QueryAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        List<string> rows = [];
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(reader.GetString(0));
        }

        return rows;
    }

    /// <summary>Counts the commands (reads and writes) a context sends.</summary>
    private sealed class CommandCounter : Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public override ValueTask<Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader>> ReaderExecutingAsync(
            System.Data.Common.DbCommand command,
            Microsoft.EntityFrameworkCore.Diagnostics.CommandEventData eventData,
            Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _count);
            return ValueTask.FromResult(result);
        }
    }
}
