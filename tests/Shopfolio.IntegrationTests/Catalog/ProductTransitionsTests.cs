using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Catalog;

/// <summary>The collection of the publish status transitions' tests (T321): one database copied from the template.</summary>
[CollectionDefinition(ProductTransitionsDefinition.Name)]
public sealed class ProductTransitionsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ProductTransitions";
}

/// <summary>
/// The publish status transitions on a real PostgreSQL (T321 KK1-KK9; 03 1.8; 02 3.7.1, 5.1): the two statuses apart from each other, the free moves, the trail row and
/// no notification, what the visitor sees, the gate and its refusal, the last Published variant, the cart effect told once, the open order that is not touched, and the sold-out
/// record that stays listed. The gate is a stand-in here; its conditions are T335's.
/// </summary>
[Collection(ProductTransitionsDefinition.Name)]
public sealed class ProductTransitionsTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly PublishStatus[] AllStatuses = Enum.GetValues<PublishStatus>();

    private readonly CollectionDatabaseFixture _collection;

    private readonly StandInGate _gate = new();

    private Admin? _admin;

    private int _counter;

    public ProductTransitionsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static TheoryData<PublishStatus, PublishStatus> EveryMove()
    {
        TheoryData<PublishStatus, PublishStatus> data = [];
        foreach (PublishStatus from in AllStatuses)
        {
            foreach (PublishStatus to in AllStatuses.Where(s => s != from))
            {
                data.Add(from, to);
            }
        }

        return data;
    }

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE cart_items, carts, order_items, orders, audit_logs, variants, products, admins, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(_admin));
        _gate.Reset();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the two statuses are apart ----

    [Fact]
    public async Task APublishedProduct_CanHaveADraftVariant_AndTheOtherWayRound_KK1()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published, PublishStatus.Published);

        PublishChangeResult variantMove = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, PublishStatus.Draft, product.Version, token));

        Assert.True(variantMove.IsChanged);
        Assert.Equal((PublishStatus.Published, PublishStatus.Draft, PublishStatus.Published), await StatusesAsync(product.Id, variants));

        PublishChangeResult productMove = await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Draft, product.Version + 1, token));

        Assert.True(productMove.IsChanged);
        (PublishStatus productStatus, PublishStatus first, PublishStatus second) = await StatusesAsync(product.Id, variants);
        Assert.Equal((PublishStatus.Draft, PublishStatus.Draft, PublishStatus.Published), (productStatus, first, second));
    }

    [Theory]
    [MemberData(nameof(EveryMove))]
    public async Task EveryMoveBetweenTheThreeStatuses_IsFree_ForTheProductAndForTheVariant_KK1(PublishStatus from, PublishStatus to)
    {
        (Product product, List<Variant> variants) = await SeedAsync(from, from, from == PublishStatus.Published ? PublishStatus.Published : PublishStatus.Draft);

        PublishChangeResult productMove = await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, to, product.Version, token));
        PublishChangeResult variantMove = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, to, product.Version + 1, token));

        Assert.True(productMove.IsChanged);
        Assert.True(variantMove.IsChanged);
        (PublishStatus productStatus, PublishStatus variantStatus, _) = await StatusesAsync(product.Id, variants);
        Assert.Equal((to, to), (productStatus, variantStatus));
    }

    [Fact]
    public async Task AMoveToTheStatusTheRecordHolds_ChangesNothing_AndWritesNoRow()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Archived, PublishStatus.Draft);

        PublishChangeResult productMove = await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Archived, product.Version, token));
        PublishChangeResult variantMove = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, PublishStatus.Draft, product.Version, token));

        Assert.Equal((PublishChangeResult.Unchanged, PublishChangeResult.Unchanged), (productMove, variantMove));
        await using AppDbContext reader = NewContext();
        Assert.Equal(product.Version, (await reader.Products.SingleAsync(p => p.Id == product.Id, Ct)).Version);
        Assert.Equal(0, await reader.AuditLogs.CountAsync(Ct));
    }

    // ---- KK2: no notification, one trail row ----

    [Fact]
    public async Task AMove_WritesOneTrailRowOfThePublishStatus_AndNoJobAndNoDispatch_KK2()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Draft, PublishStatus.Draft);

        await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Published, product.Version, token));
        await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, PublishStatus.Published, product.Version + 1, token));

        await using AppDbContext reader = NewContext();
        List<AuditLog> rows = await reader.AuditLogs.OrderBy(l => l.CreatedAt).ThenBy(l => l.Id).ToListAsync(Ct);
        Assert.Equal(2, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Equal(nameof(AuditOperation.ChangePublishStatus), row.Operation);
            Assert.Equal(_admin!.Id, row.AdminId);
        });
        AuditLog productRow = Assert.Single(rows, r => r.TargetRecordType == nameof(Product));
        AuditLog variantRow = Assert.Single(rows, r => r.TargetRecordType == nameof(Variant));
        Assert.Equal(product.Id, productRow.TargetRecordId);
        Assert.Equal(variants[0].Id, variantRow.TargetRecordId);
        AuditValueChange productChange = Assert.Single(productRow.Details!.Changes);
        Assert.Equal(("PublishStatus", "Draft", "Published"), (productChange.Field, productChange.OldValue, productChange.NewValue));
        Assert.Equal("Masa", productRow.Details.DisplayName);
        Assert.Equal(0, await reader.Jobs.CountAsync(Ct));
        Assert.Equal(0, await reader.DispatchRecords.CountAsync(Ct));
    }

    [Fact]
    public async Task AMoveWhoseTrailRowCannotBeWritten_IsRolledBackWithItsCartEffect_KK2_KK7()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published, PublishStatus.Published);
        Guid cartId = await SeedCartAsync((variants[0].Id, 2));
        _admin = null;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Archived, product.Version, token)));

        Assert.Equal((PublishStatus.Published, PublishStatus.Published, PublishStatus.Published), await StatusesAsync(product.Id, variants));
        await using AppDbContext reader = NewContext();
        Assert.Equal(product.Version, (await reader.Products.SingleAsync(p => p.Id == product.Id, Ct)).Version);
        Assert.Null((await reader.CartItems.SingleAsync(i => i.CartId == cartId, Ct)).RemovedProductName);
    }

    // ---- KK3, KK4, KK9: what the visitor sees ----

    [Fact]
    public async Task TheList_AndTheSearch_ShowOnlyThePublishedProducts_AndKeepTheArchivedRecord_KK3_KK4()
    {
        List<Product> products = [];
        foreach (PublishStatus status in AllStatuses)
        {
            (Product product, _) = await SeedAsync(status, status, name: $"Masa {status}");
            products.Add(product);
        }

        ShowcasePage list = await ShowcaseAsync(null);
        ShowcasePage search = await ShowcaseAsync("masa");

        Product published = products.Single(p => p.PublishStatus == PublishStatus.Published);
        Assert.Equal([published.Id], list.Items.Select(i => i.Id));
        Assert.Equal([published.Id], search.Items.Select(i => i.Id));
        Assert.Empty((await ShowcaseAsync("Masa Draft")).Items);
        Assert.Empty((await ShowcaseAsync("Masa Archived")).Items);

        // The archived record is kept and readable: it only does not show.
        Product archived = products.Single(p => p.PublishStatus == PublishStatus.Archived);
        await using AppDbContext reader = NewContext();
        Product kept = await reader.Products.SingleAsync(p => p.Id == archived.Id, Ct);
        Assert.Equal(("Masa Archived", PublishStatus.Archived), (kept.Name, kept.PublishStatus));
    }

    [Fact]
    public async Task TheOptionList_ShowsOnlyThePublishedVariants_OfAPublishedProduct_KK3_KK4()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, [.. AllStatuses]);
        (Product draftProduct, List<Variant> underDraft) = await SeedAsync(PublishStatus.Draft, [.. AllStatuses]);

        IReadOnlyList<ShowcaseVariant> options = await OptionsAsync(product.Id);

        Assert.Equal([variants.Single(v => v.PublishStatus == PublishStatus.Published).Id], options.Select(o => o.Id));
        Assert.Empty(await OptionsAsync(draftProduct.Id));
        Assert.Equal(AllStatuses.Length, underDraft.Count);
    }

    [Fact]
    public async Task AVariantArchivedUnderAPublishedProduct_LeavesTheOptionList_AndAMoveBackBringsItBack_KK4()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published, PublishStatus.Published);

        await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[1].Id, PublishStatus.Archived, product.Version, token));

        Assert.Equal([variants[0].Id], (await OptionsAsync(product.Id)).Select(o => o.Id));
        await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[1].Id, PublishStatus.Published, product.Version + 1, token));
        Assert.Equal(variants.Select(v => v.Id).Order(), (await OptionsAsync(product.Id)).Select(o => o.Id).Order());
    }

    [Fact]
    public async Task ASoldOutPublishedProduct_IsInTheSearch_AndInTheList_MarkedSoldOut_KK9()
    {
        (Product soldOut, List<Variant> soldOutVariants) = await SeedAsync(PublishStatus.Published, [PublishStatus.Published], name: "Tükenmiş masa");
        (Product available, _) = await SeedAsync(PublishStatus.Published, [PublishStatus.Published], name: "Masa var");
        await ExecuteAsync($"UPDATE variants SET stock_quantity = 0 WHERE id = '{soldOutVariants[0].Id}'");

        ShowcasePage search = await ShowcaseAsync("masa");

        Assert.Equal(new[] { soldOut.Id, available.Id }.Order(), search.Items.Select(i => i.Id).Order());
        Assert.True(search.Items.Single(i => i.Id == soldOut.Id).IsSoldOut);
        Assert.False(search.Items.Single(i => i.Id == available.Id).IsSoldOut);
        ShowcaseVariant option = Assert.Single(await OptionsAsync(soldOut.Id));
        Assert.True(option.IsSoldOut);
        Assert.Equal(PublishStatus.Published, (await StatusesAsync(soldOut.Id, soldOutVariants)).Product);
    }

    [Fact]
    public async Task AWordThatMakesNoPattern_MakesNoList()
    {
        await SeedAsync(PublishStatus.Published, PublishStatus.Published);

        ShowcasePage page = await ShowcaseAsync("   ");

        Assert.Empty(page.Items);
        Assert.Equal((1, 0), (page.PageNumber, page.TotalCount));
    }

    // ---- the showcase list's paging (09 19.2 item 5, 10.5 item 6) ----

    [Fact]
    public async Task TheShowcaseList_WithProductsOfTheSameCreatedAt_NeitherRepeatsNorLosesOneAtThePageBorders()
    {
        const int Total = (PageSizes.StorefrontProducts * 2) + 1;
        List<Product> seeded = await SeedSameMomentAsync(Total);
        await SeedAsync(PublishStatus.Draft, PublishStatus.Draft, name: "Taslak");

        ShowcasePage first = await ShowcaseAsync(null, 1);
        ShowcasePage second = await ShowcaseAsync(null, 2);
        ShowcasePage third = await ShowcaseAsync(null, 3);

        Assert.Equal([PageSizes.StorefrontProducts, PageSizes.StorefrontProducts, 1], [first.Items.Count, second.Items.Count, third.Items.Count]);
        Assert.All([first, second, third], p => Assert.Equal((Total, 3, PageSizes.StorefrontProducts), (p.TotalCount, p.PageCount, p.PageSize)));
        Guid[] shown = [.. first.Items.Concat(second.Items).Concat(third.Items).Select(i => i.Id)];
        Assert.Equal(Total, shown.Distinct().Count());
        Assert.Equal(seeded.Select(p => p.Id).Order(), shown.Order());
    }

    [Theory]
    [InlineData(4)]
    [InlineData(int.MaxValue)]
    public async Task AShowcasePage_PastTheLast_IsTheLastPage_AndOneBelowTheFirst_IsTheFirst(int asked)
    {
        await SeedSameMomentAsync((PageSizes.StorefrontProducts * 2) + 1);

        ShowcasePage past = await ShowcaseAsync(null, asked);
        ShowcasePage below = await ShowcaseAsync(null, 0);

        Assert.Equal((3, 1), (past.PageNumber, past.Items.Count));
        Assert.Equal((1, PageSizes.StorefrontProducts), (below.PageNumber, below.Items.Count));
    }

    [Fact]
    public async Task AShowcaseListOfOnePage_HasPageCountOne()
    {
        await SeedSameMomentAsync(3);

        ShowcasePage page = await ShowcaseAsync(null, 1);

        Assert.Equal((3, 3, 1, 1), (page.Items.Count, page.TotalCount, page.PageCount, page.PageNumber));
    }

    // ---- KK5: the gate ----

    [Fact]
    public async Task AMoveToPublished_AsksTheGate_AndAnOpenGateLetsItThrough_KK5()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Draft, PublishStatus.Draft);

        PublishChangeResult product1 = await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Published, product.Version, token));
        PublishChangeResult variant1 = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, PublishStatus.Published, product.Version + 1, token));

        Assert.False(product1.IsRejected || variant1.IsRejected);
        Assert.Equal([product.Id], _gate.ProductQuestions);
        Assert.Equal([variants[0].Id], _gate.VariantQuestions);
    }

    [Theory]
    [InlineData(PublishStatus.Draft)]
    [InlineData(PublishStatus.Archived)]
    public async Task AGateThatRefuses_LeavesTheStatusAsItWas_NoRowAndNoVersionRise_KK5(PublishStatus from)
    {
        (Product product, List<Variant> variants) = await SeedAsync(from, from);
        _gate.Refuse("name", "variant");

        PublishChangeResult productMove = await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Published, product.Version, token));
        PublishChangeResult variantMove = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, PublishStatus.Published, product.Version, token));

        foreach (PublishChangeResult result in new[] { productMove, variantMove })
        {
            Assert.Equal(ErrorCode.PublishGateIncomplete, result.Code);
            Assert.Equal(["name", "variant"], result.UnmetConditions);
        }

        Assert.Equal((from, from, from), await StatusesAsync(product.Id, variants));
        await using AppDbContext reader = NewContext();
        Assert.Equal(product.Version, (await reader.Products.SingleAsync(p => p.Id == product.Id, Ct)).Version);
        Assert.Equal(0, await reader.AuditLogs.CountAsync(Ct));
        Assert.Null((await reader.Products.SingleAsync(p => p.Id == product.Id, Ct)).SlugFixedAt);
    }

    [Fact]
    public async Task TheGateIsNotAskedForAMoveOutOfPublished_AndRefusingItDoesNotStopThatMove_KK5()
    {
        (Product product, _) = await SeedAsync(PublishStatus.Published, PublishStatus.Published);
        _gate.Refuse("name");

        PublishChangeResult move = await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Archived, product.Version, token));

        Assert.True(move.IsChanged);
        Assert.Empty(_gate.ProductQuestions);
    }

    [Fact]
    public async Task TheFirstPublication_FixesTheAddress_AndTheFirstSaleOfThePublishedVariants_OnceOnly()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Draft, PublishStatus.Published, PublishStatus.Draft);

        await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Published, product.Version, token));
        await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[1].Id, PublishStatus.Published, product.Version + 1, token));
        await using (AppDbContext first = NewContext())
        {
            Product fixedProduct = await first.Products.SingleAsync(p => p.Id == product.Id, Ct);
            List<Variant> saved = await first.Variants.Where(v => v.ProductId == product.Id).OrderBy(v => v.Id).ToListAsync(Ct);
            Assert.NotNull(fixedProduct.SlugFixedAt);
            Assert.NotNull(saved.Single(v => v.Id == variants[0].Id).FirstOnSaleAt);
            Assert.NotNull(saved.Single(v => v.Id == variants[1].Id).FirstOnSaleAt);
        }

        DateTimeOffset? slugFixed;
        DateTimeOffset? firstSale;
        await using (AppDbContext before = NewContext())
        {
            slugFixed = (await before.Products.SingleAsync(p => p.Id == product.Id, Ct)).SlugFixedAt;
            firstSale = (await before.Variants.SingleAsync(v => v.Id == variants[0].Id, Ct)).FirstOnSaleAt;
        }

        await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Archived, product.Version + 2, token));
        await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Published, product.Version + 3, token));

        await using AppDbContext after = NewContext();
        Assert.Equal(slugFixed, (await after.Products.SingleAsync(p => p.Id == product.Id, Ct)).SlugFixedAt);
        Assert.Equal(firstSale, (await after.Variants.SingleAsync(v => v.Id == variants[0].Id, Ct)).FirstOnSaleAt);
    }

    // ---- KK6: the last Published variant ----

    [Theory]
    [InlineData(PublishStatus.Draft)]
    [InlineData(PublishStatus.Archived)]
    public async Task TheLastPublishedVariantOfAPublishedProduct_CannotLeavePublished_KK6(PublishStatus target)
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published, PublishStatus.Draft, PublishStatus.Archived);

        PublishChangeResult result = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, target, product.Version, token));

        Assert.Equal(ErrorCode.LastPublishedVariant, result.Code);
        Assert.Equal((PublishStatus.Published, PublishStatus.Published, PublishStatus.Draft), await StatusesAsync(product.Id, variants));
        await using AppDbContext reader = NewContext();
        Assert.Equal(0, await reader.AuditLogs.CountAsync(Ct));
        Assert.Equal(product.Version, (await reader.Products.SingleAsync(p => p.Id == product.Id, Ct)).Version);
    }

    [Fact]
    public async Task APublishedVariantMayLeave_WhenAnotherPublishedVariantStays_OrWhenTheProductIsNotPublished_KK6()
    {
        (Product published, List<Variant> two) = await SeedAsync(PublishStatus.Published, PublishStatus.Published, PublishStatus.Published);
        (Product draft, List<Variant> one) = await SeedAsync(PublishStatus.Draft, PublishStatus.Published);

        PublishChangeResult withAnother = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(two[0].Id, PublishStatus.Archived, published.Version, token));
        PublishChangeResult underDraft = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(one[0].Id, PublishStatus.Archived, draft.Version, token));

        Assert.True(withAnother.IsChanged);
        Assert.True(underDraft.IsChanged);
        // The second of the two is now the last: it is held.
        PublishChangeResult last = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(two[1].Id, PublishStatus.Draft, published.Version + 1, token));
        Assert.Equal(ErrorCode.LastPublishedVariant, last.Code);
    }

    [Fact]
    public async Task TheProductItself_MayLeavePublished_WhateverItsVariantsAre_KK6()
    {
        (Product product, _) = await SeedAsync(PublishStatus.Published, PublishStatus.Published);

        PublishChangeResult toDraft = await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Draft, product.Version, token));

        Assert.True(toDraft.IsChanged);
    }

    // ---- stale screen and missing record ----

    [Fact]
    public async Task AScreenOfAnOlderVersion_IsRefused_AndNothingIsWritten()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Draft, PublishStatus.Draft);

        PublishChangeResult product1 = await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Published, product.Version + 4, token));
        PublishChangeResult variant1 = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, PublishStatus.Published, product.Version - 1, token));

        Assert.Equal((ErrorCode.RecordVersionConflict, ErrorCode.RecordVersionConflict), (product1.Code, variant1.Code));
        Assert.Equal((PublishStatus.Draft, PublishStatus.Draft, PublishStatus.Draft), await StatusesAsync(product.Id, variants));
        await using AppDbContext reader = NewContext();
        Assert.Equal(0, await reader.AuditLogs.CountAsync(Ct));
    }

    [Fact]
    public async Task ARecordThatIsGone_IsNotFound()
    {
        PublishChangeResult product = await RunAsync((unit, token) => unit.ChangeProductStatusAsync(Guid.NewGuid(), PublishStatus.Published, 1, token));
        PublishChangeResult variant = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(Guid.NewGuid(), PublishStatus.Published, 1, token));

        Assert.Equal((ErrorCode.NotFound, ErrorCode.NotFound), (product.Code, variant.Code));
    }

    [Fact]
    public async Task AMoveOutsideATransaction_IsAProgrammingError()
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IProductTransitions unit = scope.ServiceProvider.GetRequiredService<IProductTransitions>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => unit.ChangeProductStatusAsync(Guid.NewGuid(), PublishStatus.Draft, 1, Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => unit.ChangeVariantStatusAsync(Guid.NewGuid(), PublishStatus.Draft, 1, Ct));
    }

    // ---- KK7: the cart ----

    [Fact]
    public async Task TwoRequestsTakingTheNoticesTogether_TellThemOnce_AndNeitherThrows_KK7()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published, PublishStatus.Published);
        Guid cartId = await SeedCartAsync((variants[0].Id, 1), (variants[1].Id, 2));
        await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Archived, product.Version, token));

        IReadOnlyList<string>[] taken = await Task.WhenAll(TakeNoticesAsync(cartId), TakeNoticesAsync(cartId));

        Assert.Equal(2, taken.Sum(t => t.Count));
        Assert.Empty(await TakeNoticesAsync(cartId));
    }

    [Theory]
    [InlineData(PublishStatus.Archived)]
    [InlineData(PublishStatus.Draft)]
    public async Task AProductTakenOffTheShowcase_LeavesTheCarts_AndIsToldOnce_KK7(PublishStatus target)
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published, PublishStatus.Published);
        (_, List<Variant> otherVariants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published, name: "Sandalye");
        Guid cartId = await SeedCartAsync((variants[0].Id, 1), (variants[1].Id, 3), (otherVariants[0].Id, 2));

        PublishChangeResult move = await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, target, product.Version, token));

        Assert.True(move.IsChanged);
        await using (AppDbContext reader = NewContext())
        {
            List<CartItem> items = await reader.CartItems.Where(i => i.CartId == cartId).ToListAsync(Ct);
            Assert.Equal(3, items.Count);
            Assert.All(items.Where(i => variants.Any(v => v.Id == i.VariantId)), i => Assert.Equal("Masa", i.RemovedProductName));
            Assert.Equal(variants.Count, items.Count(i => i.RemovedProductName == "Masa"));
            Assert.Single(items, i => i.RemovedProductName is null && i.VariantId == otherVariants[0].Id);
        }

        IReadOnlyList<string> first = await TakeNoticesAsync(cartId);
        IReadOnlyList<string> second = await TakeNoticesAsync(cartId);

        Assert.Equal(variants.Select(_ => "Masa"), first);
        Assert.Empty(second);
        await using AppDbContext after = NewContext();
        Assert.Equal([otherVariants[0].Id], await after.CartItems.Where(i => i.CartId == cartId).Select(i => i.VariantId!.Value).ToListAsync(Ct));
    }

    [Theory]
    [InlineData(PublishStatus.Archived)]
    [InlineData(PublishStatus.Draft)]
    public async Task AVariantTakenOffTheShowcase_LeavesTheCarts_AndTheProductsOtherVariantsStay_KK7(PublishStatus target)
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published, PublishStatus.Published);
        Guid cartId = await SeedCartAsync((variants[0].Id, 1), (variants[1].Id, 1));

        PublishChangeResult move = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, target, product.Version, token));

        Assert.True(move.IsChanged);
        Assert.Equal(["Masa"], await TakeNoticesAsync(cartId));
        await using AppDbContext reader = NewContext();
        Assert.Equal([variants[1].Id], await reader.CartItems.Where(i => i.CartId == cartId).Select(i => i.VariantId!.Value).ToListAsync(Ct));
    }

    [Fact]
    public async Task TheCartMethods_DoNotSaveOnTheirOwn_TheTransactionOwnerDoes_KK7()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published, PublishStatus.Published);
        Guid cartId = await SeedCartAsync((variants[0].Id, 1));

        // The transaction is committed without any save: a module method that saved itself would leave its change behind (09 6 item 3).
        await using (ServiceProvider provider = BuildProvider())
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(Ct);
            await scope.ServiceProvider.GetRequiredService<IOrderingModule>().RemoveFromCartsAsync([variants[0].Id], product.Name, Ct);
            await transaction.CommitAsync(Ct);
        }

        await using (AppDbContext reader = NewContext())
        {
            Assert.Null((await reader.CartItems.SingleAsync(i => i.CartId == cartId, Ct)).RemovedProductName);
        }

        await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, PublishStatus.Draft, product.Version, token));
        await using (ServiceProvider provider = BuildProvider())
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(Ct);
            IReadOnlyList<string> notices = await scope.ServiceProvider.GetRequiredService<IOrderingModule>().TakeRemovedItemNoticesAsync(cartId, Ct);
            await transaction.CommitAsync(Ct);
            Assert.Equal(["Masa"], notices);
        }

        await using AppDbContext after = NewContext();
        Assert.Equal(1, await after.CartItems.CountAsync(i => i.CartId == cartId && i.RemovedProductName != null, Ct));
    }

    [Fact]
    public async Task AMoveThatDoesNotTakeAnythingOffTheShowcase_LeavesTheCartsAlone_KK7()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Draft, PublishStatus.Published, PublishStatus.Published);
        Guid cartId = await SeedCartAsync((variants[0].Id, 1));

        await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Archived, product.Version, token));
        await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Published, product.Version + 1, token));

        Assert.Empty(await TakeNoticesAsync(cartId));
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.CartItems.CountAsync(i => i.CartId == cartId, Ct));
    }

    [Fact]
    public async Task ARefusedMove_LeavesTheCartsAlone_KK7()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published);
        Guid cartId = await SeedCartAsync((variants[0].Id, 1));

        PublishChangeResult refused = await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, PublishStatus.Archived, product.Version, token));

        Assert.Equal(ErrorCode.LastPublishedVariant, refused.Code);
        Assert.Empty(await TakeNoticesAsync(cartId));
        await using AppDbContext reader = NewContext();
        Assert.Null((await reader.CartItems.SingleAsync(i => i.CartId == cartId, Ct)).RemovedProductName);
    }

    // ---- KK8: the order and the sales switch ----

    [Fact]
    public async Task AnOpenOrder_IsNotTouchedByAnyMoveOfItsProductOrVariant_KK8()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published, PublishStatus.Published);
        Guid orderId = await SeedOrderAsync(product.Id, variants[0].Id);
        string before = await OrderFingerprintAsync(orderId);

        await RunAsync((unit, token) => unit.ChangeVariantStatusAsync(variants[0].Id, PublishStatus.Archived, product.Version, token));
        await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Draft, product.Version + 1, token));
        await RunAsync((unit, token) => unit.ChangeProductStatusAsync(product.Id, PublishStatus.Archived, product.Version + 2, token));

        Assert.Equal(before, await OrderFingerprintAsync(orderId));
        await using AppDbContext reader = NewContext();
        OrderItem item = await reader.OrderItems.SingleAsync(i => i.OrderId == orderId, Ct);
        Assert.Equal((product.Id, variants[0].Id), (item.ProductId, item.VariantId));
    }

    [Fact]
    public async Task TemporarilyClosingTheSales_ChangesNoPublishStatus_AndTheListStays_KK8()
    {
        (Product product, List<Variant> variants) = await SeedAsync(PublishStatus.Published, PublishStatus.Published);

        await ExecuteAsync("UPDATE company_settings SET is_sales_temporarily_closed = true");

        Assert.Equal((PublishStatus.Published, PublishStatus.Published, PublishStatus.Published), await StatusesAsync(product.Id, variants));
        Assert.Equal([product.Id], (await ShowcaseAsync(null)).Items.Select(i => i.Id));
        Assert.Single(await OptionsAsync(product.Id));
    }

    // ---- helpers ----

    private Task<ShowcasePage> ShowcaseAsync(string? search, int page = 1) => WithCatalogAsync((catalog, token) => catalog.ListShowcaseAsync(search, page, token));

    /// <summary>Seeds Published products that all carry one <c>CreatedAt</c>, so only the id tells them apart in the list's order.</summary>
    private async Task<List<Product>> SeedSameMomentAsync(int count)
    {
        int n = Interlocked.Increment(ref _counter);
        List<Product> products = [.. Enumerable.Range(0, count).Select(i =>
        {
            Product product = Product.Create($"Masa {i}", $"ayni-an-{n:D4}-{i:D3}", ProductType.Physical, 20m, Now.AddDays(1));
            product.PublishStatus = PublishStatus.Published;
            return product;
        })];
        await SaveAsync(c => c.Products.AddRange(products));
        return products;
    }

    private Task<IReadOnlyList<ShowcaseVariant>> OptionsAsync(Guid productId) => WithCatalogAsync((catalog, token) => catalog.ListOptionVariantsAsync(productId, token));

    private async Task<T> WithCatalogAsync<T>(Func<ICatalogModule, CancellationToken, Task<T>> read)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await read(scope.ServiceProvider.GetRequiredService<ICatalogModule>(), Ct);
    }

    private async Task<IReadOnlyList<string>> TakeNoticesAsync(Guid cartId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        // The call joins the transaction owner's transaction and takes the cart's lock inside it; the owner saves (09 6 item 3, 9.1 item 3, 10.2 item 5).
        IOrderingModule ordering = scope.ServiceProvider.GetRequiredService<IOrderingModule>();
        TakenNotices taken = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>()
            .RunAsync(async token => new TakenNotices(await ordering.TakeRemovedItemNoticesAsync(cartId, token)), Ct);
        return taken.Names;
    }

    private sealed record TakenNotices(IReadOnlyList<string> Names) : IOperationResult
    {
        public bool IsRejected => false;
    }

    private async Task<PublishChangeResult> RunAsync(Func<IProductTransitions, CancellationToken, Task<PublishChangeResult>> work)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IProductTransitions unit = scope.ServiceProvider.GetRequiredService<IProductTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<(PublishStatus Product, PublishStatus First, PublishStatus Second)> StatusesAsync(Guid productId, List<Variant> variants)
    {
        await using AppDbContext reader = NewContext();
        PublishStatus product = (await reader.Products.SingleAsync(p => p.Id == productId, Ct)).PublishStatus;
        Dictionary<Guid, PublishStatus> saved = await reader.Variants.Where(v => v.ProductId == productId).ToDictionaryAsync(v => v.Id, v => v.PublishStatus, Ct);
        return (product, saved[variants[0].Id], variants.Count > 1 ? saved[variants[1].Id] : saved[variants[0].Id]);
    }

    private Task<(Product Product, List<Variant> Variants)> SeedAsync(PublishStatus productStatus, PublishStatus first, PublishStatus? second = null, string name = "Masa")
    {
        List<PublishStatus> statuses = [first];
        if (second is { } s)
        {
            statuses.Add(s);
        }

        return SeedAsync(productStatus, [.. statuses], name);
    }

    private Task<(Product Product, List<Variant> Variants)> SeedAsync(PublishStatus productStatus, PublishStatus first, PublishStatus second, PublishStatus third)
        => SeedAsync(productStatus, [first, second, third], "Masa");

    private async Task<(Product Product, List<Variant> Variants)> SeedAsync(PublishStatus productStatus, PublishStatus[] variantStatuses, string name = "Masa")
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create(name, $"urun-{n:D4}", ProductType.Physical, 20m, Now.AddMinutes(n));
        product.PublishStatus = productStatus;
        List<Variant> variants = [];
        foreach (PublishStatus status in variantStatuses)
        {
            Variant variant = Variant.Create(product.Id, $"SKU-{n:D4}-{variants.Count}", 100m, Now.AddMinutes(n));
            variant.PublishStatus = status;
            variants.Add(variant);
        }

        // Several variants of one product are told apart by their option values; the seed gives each its own value of one option (06 4.6).
        Option? option = variants.Count > 1 ? Option.Create(product.Id, "Boy", 1, Now) : null;
        List<OptionValue> values = [];
        if (option is not null)
        {
            for (int i = 0; i < variants.Count; i++)
            {
                OptionValue value = OptionValue.Create(option.Id, $"Değer {i}", i + 1, Now);
                variants[i].FirstOptionValueId = value.Id;
                values.Add(value);
            }
        }

        await SaveAsync(c =>
        {
            c.Products.Add(product);
            if (option is not null)
            {
                c.Options.Add(option);
                c.OptionValues.AddRange(values);
            }

            c.Variants.AddRange(variants);
        });
        return (product, variants);
    }

    private async Task<Guid> SeedCartAsync(params (Guid VariantId, int Quantity)[] items)
    {
        int n = Interlocked.Increment(ref _counter);
        Cart cart = Cart.ForGuest($"guest-key-digest-{n}", Now);
        await SaveAsync(c =>
        {
            c.Carts.Add(cart);
            foreach ((Guid variantId, int quantity) in items)
            {
                c.CartItems.Add(CartItem.Create(cart.Id, variantId, quantity, 100m, Now));
            }
        });
        return cart.Id;
    }

    private async Task<Guid> SeedOrderAsync(Guid productId, Guid variantId)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Now, "metin", Now).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Now.AddHours(1), Now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = province.Id;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = province.Id;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        OrderItem item = OrderItem.Create(order.Id, productId, variantId, ProductType.Physical, "Masa", 1, 100m, 20m, 16.67m, Now);
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });
        await ExecuteAsync($"UPDATE orders SET order_status = 'Preparing', payment_status = 'Paid' WHERE id = '{order.Id}'");
        return order.Id;
    }

    private async Task<string> OrderFingerprintAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        Order order = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        List<OrderItem> items = await reader.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).OrderBy(i => i.Id).ToListAsync(Ct);
        int records = await reader.TransitionRecords.CountAsync(t => t.OrderId == orderId, Ct);
        return $"{order.OrderStatus}|{order.PaymentStatus}|{order.Version}|{order.Total}|{records}|"
            + string.Join(';', items.Select(i => $"{i.Id}:{i.Quantity}:{i.ProductId}:{i.VariantId}"));
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<Shopfolio.Infrastructure.InstallationSettings.StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddOrderingModule();
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin));
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddScoped<IPublishGate>(_ => _gate);
        return services.BuildServiceProvider();
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

    /// <summary>The gate of the tests: open until told to refuse with the named conditions; it records the questions it was asked.</summary>
    private sealed class StandInGate : IPublishGate
    {
        private IReadOnlyList<string> _unmet = [];

        public List<Guid> ProductQuestions { get; } = [];

        public List<Guid> VariantQuestions { get; } = [];

        public void Refuse(params string[] unmet) => _unmet = unmet;

        public void Reset()
        {
            _unmet = [];
            ProductQuestions.Clear();
            VariantQuestions.Clear();
        }

        public Task<PublishGateVerdict> CheckProductAsync(Guid productId, CancellationToken ct)
        {
            ProductQuestions.Add(productId);
            return Task.FromResult(new PublishGateVerdict(_unmet));
        }

        public Task<PublishGateVerdict> CheckVariantAsync(Guid variantId, CancellationToken ct)
        {
            VariantQuestions.Add(variantId);
            return Task.FromResult(new PublishGateVerdict(_unmet));
        }
    }

    private sealed class FixedActor : IAuditActorSource
    {
        private readonly Admin? _admin;

        public FixedActor(Admin? admin)
        {
            _admin = admin;
        }

        public Task<AuditActor?> CurrentAsync(CancellationToken ct)
            => Task.FromResult(_admin is null ? null : new AuditActor(_admin.Id, _admin.Name, _admin.Email));
    }
}
