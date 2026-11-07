using System.Net;

using Microsoft.EntityFrameworkCore;
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

/// <summary>The collection of the publish gate and removal tests (T335): one database copied from the template.</summary>
[CollectionDefinition(ProductPublishGateDefinition.Name)]
public sealed class ProductPublishGateDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ProductPublishGate";
}

/// <summary>
/// The publish gate, the archive, the product address, the permanent deletion and the file removal on a real PostgreSQL (T335 KK1-KK9; 02 3.7.1-3.7.8, 5.1): the real gate of the
/// catalog is registered - no stand-in. The two statuses move apart, the archive keeps the record, the gate names what is unmet, a digital product needs its files, a file removal
/// that would leave a published variant without one is refused, the address answers by status and by who asks, a deletion leaves the old orders alone and takes the reservation
/// with it, and the last Published variant is kept three ways.
/// </summary>
[Collection(ProductPublishGateDefinition.Name)]
public sealed class ProductPublishGateTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private Admin? _admin;

    private int _counter;

    public ProductPublishGateTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE cart_items, carts, order_items, orders, audit_logs, variants, products, categories, admins, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(_admin));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the two statuses are apart ----

    [Fact]
    public async Task APublishedProduct_KeepsItsStatus_WhenOneOfItsVariantsGoesToDraft_KK1()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Published, [PublishStatus.Published, PublishStatus.Published]);

        PublishChangeResult move = await TransitionAsync((unit, token) => unit.ChangeVariantStatusAsync(seeded.Variants[0].Id, PublishStatus.Draft, seeded.Product.Version, token));

        Assert.True(move.IsChanged);
        Assert.Equal(PublishStatus.Published, await ProductStatusAsync(seeded.Product.Id));
        Assert.Equal([PublishStatus.Draft, PublishStatus.Published], await VariantStatusesAsync(seeded));
    }

    [Fact]
    public async Task ADraftProduct_CanHoldAPublishedVariant_AndTheProductGoesPublishedWithoutMovingTheVariants_KK1()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Draft, [PublishStatus.Published, PublishStatus.Draft]);

        PublishChangeResult move = await TransitionAsync((unit, token) => unit.ChangeProductStatusAsync(seeded.Product.Id, PublishStatus.Published, seeded.Product.Version, token));

        Assert.True(move.IsChanged);
        Assert.Equal(PublishStatus.Published, await ProductStatusAsync(seeded.Product.Id));
        Assert.Equal([PublishStatus.Published, PublishStatus.Draft], await VariantStatusesAsync(seeded));
    }

    // ---- KK2: the archive is not a deletion ----

    [Fact]
    public async Task AnArchivedProduct_KeepsItsRecordAndVariants_AndLeavesTheListAndTheOptions_KK2()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Published, [PublishStatus.Published, PublishStatus.Published]);
        Assert.Equal([seeded.Product.Id], (await ShowcaseAsync()).Items.Select(i => i.Id));

        PublishChangeResult move = await TransitionAsync((unit, token) => unit.ChangeProductStatusAsync(seeded.Product.Id, PublishStatus.Archived, seeded.Product.Version, token));

        Assert.True(move.IsChanged);
        Assert.Empty((await ShowcaseAsync()).Items);
        Assert.Empty((await ShowcaseAsync("masa")).Items);
        Assert.Empty(await WithCatalogAsync((catalog, token) => catalog.ListOptionVariantsAsync(seeded.Product.Id, token)));
        await using AppDbContext reader = NewContext();
        Product kept = await reader.Products.SingleAsync(p => p.Id == seeded.Product.Id, Ct);
        Assert.Equal(("Masa", PublishStatus.Archived), (kept.Name, kept.PublishStatus));
        Assert.Equal(2, await reader.Variants.CountAsync(v => v.ProductId == seeded.Product.Id, Ct));
    }

    // ---- KK3: the gate of a product ----

    [Fact]
    public async Task AProductWithoutNameCategoryAndPublishedVariant_IsRefused_WithTheThreeConditionsNamed_KK3()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Draft, [PublishStatus.Draft], mainCategory: false);
        await ExecuteAsync($"UPDATE products SET name = '   ' WHERE id = '{seeded.Product.Id}'");

        PublishChangeResult move = await TransitionAsync((unit, token) => unit.ChangeProductStatusAsync(seeded.Product.Id, PublishStatus.Published, seeded.Product.Version, token));

        Assert.Equal(ErrorCode.PublishGateIncomplete, move.Code);
        Assert.Equal([PublishCondition.Name, PublishCondition.MainCategory, PublishCondition.PublishedVariant], move.UnmetConditions);
        Assert.All(move.UnmetConditions, name => Assert.Contains(name, PublishCondition.All));
        Assert.Equal(PublishStatus.Draft, await ProductStatusAsync(seeded.Product.Id));
        await using AppDbContext reader = NewContext();
        Assert.Equal(seeded.Product.Version, (await reader.Products.SingleAsync(p => p.Id == seeded.Product.Id, Ct)).Version);
        Assert.Equal(0, await reader.AuditLogs.CountAsync(Ct));
    }

    [Fact]
    public async Task ACategoryThatIsNotTheMainOne_DoesNotOpenTheGate_AndTheMainOneDoes_KK3()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Draft, [PublishStatus.Published], mainCategory: false);
        Guid categoryId = await SeedCategoryAsync();
        await SaveAsync(c => c.ProductCategories.Add(ProductCategory.Create(seeded.Product.Id, categoryId, false, Now)));

        PublishChangeResult refused = await TransitionAsync((unit, token) => unit.ChangeProductStatusAsync(seeded.Product.Id, PublishStatus.Published, seeded.Product.Version, token));
        await ExecuteAsync($"UPDATE product_categories SET is_primary_category = true WHERE product_id = '{seeded.Product.Id}'");
        PublishChangeResult opened = await TransitionAsync((unit, token) => unit.ChangeProductStatusAsync(seeded.Product.Id, PublishStatus.Published, seeded.Product.Version, token));

        Assert.Equal([PublishCondition.MainCategory], refused.UnmetConditions);
        Assert.True(opened.IsChanged);
    }

    [Fact]
    public async Task AProductWithNameCategoryAndAPublishedVariant_GoesPublished_WithoutDescriptionOrImage_KK3()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Archived, [PublishStatus.Published]);

        PublishChangeResult move = await TransitionAsync((unit, token) => unit.ChangeProductStatusAsync(seeded.Product.Id, PublishStatus.Published, seeded.Product.Version, token));

        Assert.True(move.IsChanged);
        await using AppDbContext reader = NewContext();
        Product saved = await reader.Products.SingleAsync(p => p.Id == seeded.Product.Id, Ct);
        Assert.Null(saved.Description);
        Assert.Equal(0, await reader.ProductImages.CountAsync(i => i.ProductId == seeded.Product.Id, Ct));
        Assert.Equal(PublishStatus.Published, saved.PublishStatus);
    }

    // ---- KK4: the digital file and the service period ----

    [Fact]
    public async Task ADigitalProduct_WithAPublishedVariantWithoutAFile_IsRefused_AndAFileOpensTheGate_KK4()
    {
        Seeded seeded = await SeedAsync(ProductType.Digital, PublishStatus.Draft, [PublishStatus.Published]);

        PublishChangeResult refused = await TransitionAsync((unit, token) => unit.ChangeProductStatusAsync(seeded.Product.Id, PublishStatus.Published, seeded.Product.Version, token));
        IReadOnlyList<Guid> missing = await WithCatalogAsync((catalog, token) => catalog.ListVariantsWithoutFileAsync(seeded.Product.Id, token));
        await SeedFileAsync(variantId: seeded.Variants[0].Id);
        PublishChangeResult opened = await TransitionAsync((unit, token) => unit.ChangeProductStatusAsync(seeded.Product.Id, PublishStatus.Published, seeded.Product.Version, token));

        Assert.Equal([PublishCondition.DigitalFile], refused.UnmetConditions);
        Assert.Equal([seeded.Variants[0].Id], missing);
        Assert.True(opened.IsChanged);
    }

    [Fact]
    public async Task AProductLevelFile_ServesEveryVariant_ButADetachedOrOneVariantsFileDoesNotServeTheOther_KK4()
    {
        Seeded withProductFile = await SeedAsync(ProductType.Digital, PublishStatus.Draft, [PublishStatus.Published, PublishStatus.Published]);
        await SeedFileAsync(productId: withProductFile.Product.Id);
        Seeded withOneOwnFile = await SeedAsync(ProductType.Digital, PublishStatus.Draft, [PublishStatus.Published, PublishStatus.Published]);
        await SeedFileAsync(variantId: withOneOwnFile.Variants[0].Id);
        Seeded detachedOnly = await SeedAsync(ProductType.Digital, PublishStatus.Draft, [PublishStatus.Published]);
        await SeedFileAsync(productId: detachedOnly.Product.Id, detached: true);

        Assert.Empty(await WithCatalogAsync((catalog, token) => catalog.ListVariantsWithoutFileAsync(withProductFile.Product.Id, token)));
        Assert.Equal([withOneOwnFile.Variants[1].Id], await WithCatalogAsync((catalog, token) => catalog.ListVariantsWithoutFileAsync(withOneOwnFile.Product.Id, token)));
        Assert.Equal([detachedOnly.Variants[0].Id], await WithCatalogAsync((catalog, token) => catalog.ListVariantsWithoutFileAsync(detachedOnly.Product.Id, token)));
        Assert.True((await TransitionAsync((unit, token) => unit.ChangeProductStatusAsync(withProductFile.Product.Id, PublishStatus.Published, withProductFile.Product.Version, token))).IsChanged);
        Assert.Equal(
            [PublishCondition.DigitalFile],
            (await TransitionAsync((unit, token) => unit.ChangeProductStatusAsync(withOneOwnFile.Product.Id, PublishStatus.Published, withOneOwnFile.Product.Version, token))).UnmetConditions);
    }

    [Fact]
    public async Task ADigitalVariantWithoutAFile_CannotGoPublished_EvenUnderADraftProduct_KK4()
    {
        Seeded seeded = await SeedAsync(ProductType.Digital, PublishStatus.Draft, [PublishStatus.Draft, PublishStatus.Draft]);
        await SeedFileAsync(variantId: seeded.Variants[1].Id);

        PublishChangeResult refused = await TransitionAsync((unit, token) => unit.ChangeVariantStatusAsync(seeded.Variants[0].Id, PublishStatus.Published, seeded.Product.Version, token));
        PublishChangeResult opened = await TransitionAsync((unit, token) => unit.ChangeVariantStatusAsync(seeded.Variants[1].Id, PublishStatus.Published, seeded.Product.Version, token));

        Assert.Equal(ErrorCode.PublishGateIncomplete, refused.Code);
        Assert.Equal([PublishCondition.DigitalFile], refused.UnmetConditions);
        Assert.True(opened.IsChanged);
        Assert.Equal([PublishStatus.Draft, PublishStatus.Published], await VariantStatusesAsync(seeded));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(3, true)]
    public async Task AServiceProduct_NeedsItsFulfilmentPeriod_ToGoPublished_KK4(int? period, bool isOpen)
    {
        Seeded seeded = await SeedAsync(ProductType.Service, PublishStatus.Draft, [PublishStatus.Published], fulfilmentPeriod: period);

        PublishChangeResult move = await TransitionAsync((unit, token) => unit.ChangeProductStatusAsync(seeded.Product.Id, PublishStatus.Published, seeded.Product.Version, token));

        Assert.Equal(isOpen, move.IsChanged);
        string[] expected = isOpen ? [] : [PublishCondition.FulfilmentPeriod];
        Assert.Equal(expected, move.UnmetConditions);
    }

    // ---- KK5: the file removal ----

    [Fact]
    public async Task RemovingTheOnlyFileOfAPublishedDigitalVariant_IsRefused_AndNamesTheVariant_KK5()
    {
        Seeded seeded = await SeedAsync(ProductType.Digital, PublishStatus.Published, [PublishStatus.Published, PublishStatus.Draft]);
        Guid fileId = await SeedFileAsync(variantId: seeded.Variants[0].Id);

        RemovalResult result = await RemoveAsync((removal, token) => removal.RemoveDigitalFileAsync(fileId, token));

        Assert.Equal(ErrorCode.DigitalFileRequired, result.Code);
        Assert.Equal([seeded.Variants[0].Id], result.VariantsWithoutFile);
        Assert.Null(await DetachedAtAsync(fileId));
    }

    [Fact]
    public async Task RemovingAProductLevelFile_IsRefused_WhileAPublishedVariantHasNoOwnFile_AndAllowedWhenEachHasOne_KK5()
    {
        Seeded seeded = await SeedAsync(ProductType.Digital, PublishStatus.Published, [PublishStatus.Published, PublishStatus.Published]);
        Guid productFile = await SeedFileAsync(productId: seeded.Product.Id);
        Guid firstOwn = await SeedFileAsync(variantId: seeded.Variants[0].Id);

        RemovalResult refused = await RemoveAsync((removal, token) => removal.RemoveDigitalFileAsync(productFile, token));
        await SeedFileAsync(variantId: seeded.Variants[1].Id);
        RemovalResult allowed = await RemoveAsync((removal, token) => removal.RemoveDigitalFileAsync(productFile, token));

        Assert.Equal(ErrorCode.DigitalFileRequired, refused.Code);
        Assert.Equal([seeded.Variants[1].Id], refused.VariantsWithoutFile);
        Assert.True(allowed.IsDone);
        Assert.NotNull(await DetachedAtAsync(productFile));
        Assert.Null(await DetachedAtAsync(firstOwn));
    }

    [Fact]
    public async Task RemovingAVariantsOwnFile_IsAllowed_WhenTheProductCarriesAFile_AndTheRowStaysDetached_KK5()
    {
        Seeded seeded = await SeedAsync(ProductType.Digital, PublishStatus.Published, [PublishStatus.Published]);
        await SeedFileAsync(productId: seeded.Product.Id);
        Guid own = await SeedFileAsync(variantId: seeded.Variants[0].Id);

        RemovalResult result = await RemoveAsync((removal, token) => removal.RemoveDigitalFileAsync(own, token));

        Assert.True(result.IsDone);
        await using AppDbContext reader = NewContext();
        DigitalFile kept = await reader.DigitalFiles.SingleAsync(f => f.Id == own, Ct);
        Assert.NotNull(kept.DetachedAt);
        Assert.Equal(seeded.Variants[0].Id, kept.VariantId);
    }

    [Theory]
    [InlineData(PublishStatus.Draft)]
    [InlineData(PublishStatus.Archived)]
    public async Task RemovingAFile_OfAProductThatIsNotPublished_IsFree_AndARepeatChangesNothing_KK5(PublishStatus status)
    {
        Seeded seeded = await SeedAsync(ProductType.Digital, status, [PublishStatus.Published]);
        Guid fileId = await SeedFileAsync(variantId: seeded.Variants[0].Id);

        RemovalResult first = await RemoveAsync((removal, token) => removal.RemoveDigitalFileAsync(fileId, token));
        DateTimeOffset? detachedAt = await DetachedAtAsync(fileId);
        RemovalResult second = await RemoveAsync((removal, token) => removal.RemoveDigitalFileAsync(fileId, token));

        Assert.True(first.IsDone);
        Assert.NotNull(detachedAt);
        Assert.False(second.IsDone);
        Assert.False(second.IsRejected);
        Assert.Equal(detachedAt, await DetachedAtAsync(fileId));
    }

    [Fact]
    public async Task RemovingAFileThatIsGone_OrNeverAttached_IsNotFound_KK5()
    {
        Guid unattached = await SeedFileAsync(attached: false);

        RemovalResult missing = await RemoveAsync((removal, token) => removal.RemoveDigitalFileAsync(Guid.NewGuid(), token));
        RemovalResult pending = await RemoveAsync((removal, token) => removal.RemoveDigitalFileAsync(unattached, token));

        Assert.Equal(ErrorCode.NotFound, missing.Code);
        Assert.Equal(ErrorCode.NotFound, pending.Code);
        Assert.Null(await DetachedAtAsync(unattached));
    }

    // ---- KK6, KK7: the address ----

    [Fact]
    public async Task ADraftProductsAddress_IsNotFoundForAVisitor_AndThePreviewWithABandForTheAdministrator_KK6()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Draft, [PublishStatus.Published, PublishStatus.Draft, PublishStatus.Archived]);

        ProductPage visitor = await PageAsync(seeded.Product.Slug, false);
        ProductPage administrator = await PageAsync(seeded.Product.Slug, true);

        Assert.Equal(ProductPageKind.NotFound, visitor.Kind);
        Assert.Empty(visitor.Variants);
        Assert.Equal(ProductPageKind.DraftPreview, administrator.Kind);
        Assert.True(administrator.ShowsDraftBand);
        Assert.False(administrator.OffersCart);
        Assert.False(administrator.IsIndexable);
        Assert.Equal([seeded.Variants[0].Id, seeded.Variants[1].Id], administrator.Variants.Select(v => v.Id));
        Assert.Equal([PublishStatus.Published, PublishStatus.Draft], administrator.Variants.Select(v => v.Status));
    }

    [Fact]
    public async Task APublishedProductsAddress_ShowsTheVisitorOnlyThePublishedVariants_AndOffersTheCart_KK6()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Published, [PublishStatus.Published, PublishStatus.Draft, PublishStatus.Archived]);

        ProductPage visitor = await PageAsync(seeded.Product.Slug, false);
        ProductPage administrator = await PageAsync(seeded.Product.Slug, true);

        Assert.Equal(ProductPageKind.Published, visitor.Kind);
        Assert.False(visitor.ShowsDraftBand);
        Assert.True(visitor.OffersCart);
        Assert.True(visitor.ShowsPrice);
        Assert.Equal([seeded.Variants[0].Id], visitor.Variants.Select(v => v.Id));
        Assert.Equal([seeded.Variants[0].Id, seeded.Variants[1].Id], administrator.Variants.Select(v => v.Id));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AnArchivedProductsAddress_IsTheNoLongerSoldPage_WithoutVariantsPriceOrCart_KK7(bool isAdministrator)
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Archived, [PublishStatus.Published, PublishStatus.Published]);

        ProductPage page = await PageAsync(seeded.Product.Slug, isAdministrator);

        Assert.Equal(ProductPageKind.Archived, page.Kind);
        Assert.True(page.IsNoLongerSold);
        Assert.Equal("Masa", page.Name);
        Assert.Empty(page.Variants);
        Assert.False(page.ShowsPrice);
        Assert.False(page.OffersCart);
        Assert.False(page.IsIndexable);
        Assert.Empty((await ShowcaseAsync("masa")).Items);
    }

    [Fact]
    public async Task AnAddressThatHoldsNoProduct_IsNotFound_ForEveryone_AndSoIsADeletedOne_KK6()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Published, [PublishStatus.Published]);
        await RemoveAsync((removal, token) => removal.DeleteProductAsync(seeded.Product.Id, seeded.Product.Version, token));

        foreach (bool isAdministrator in new[] { false, true })
        {
            Assert.Equal(ProductPageKind.NotFound, (await PageAsync(seeded.Product.Slug, isAdministrator)).Kind);
            Assert.Equal(ProductPageKind.NotFound, (await PageAsync("hic-yok", isAdministrator)).Kind);
            Assert.Equal(ProductPageKind.NotFound, (await PageAsync(null, isAdministrator)).Kind);
            Assert.Equal(ProductPageKind.NotFound, (await PageAsync(string.Empty, isAdministrator)).Kind);
        }
    }

    // ---- KK8: the permanent deletion ----

    [Fact]
    public async Task ADeletedProduct_LeavesItsOldOrder_WithTheFrozenItemAndWithoutTheLink_KK8()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Draft, [PublishStatus.Published, PublishStatus.Draft]);
        Guid orderId = await SeedOrderAsync(seeded.Product.Id, seeded.Variants[0].Id, "Delivered");
        string before = await OrderFingerprintAsync(orderId);

        RemovalResult result = await RemoveAsync((removal, token) => removal.DeleteProductAsync(seeded.Product.Id, seeded.Product.Version, token));

        Assert.True(result.IsDone);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.Products.AnyAsync(p => p.Id == seeded.Product.Id, Ct));
        Assert.Equal(0, await reader.Variants.CountAsync(v => v.ProductId == seeded.Product.Id, Ct));
        OrderItem item = await reader.OrderItems.SingleAsync(i => i.OrderId == orderId, Ct);
        Assert.Null(item.ProductId);
        Assert.Null(item.VariantId);
        Assert.Equal(("Masa", 1, 100m, 20m, 16.67m), (item.ProductName, item.Quantity, item.UnitPrice, item.VatRate, item.VatAmount));
        Assert.Equal(before, await OrderFingerprintAsync(orderId));
        Assert.Equal(0, await reader.AuditLogs.CountAsync(Ct));
    }

    [Fact]
    public async Task AProductWithAnOpenOrder_CanBeDeleted_AndTheImpactCountsOnlyTheOpenOrders_KK8()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Published, [PublishStatus.Published, PublishStatus.Published]);
        await SeedOrderAsync(seeded.Product.Id, seeded.Variants[0].Id, "Preparing", reservedQuantity: 2);
        await SeedOrderAsync(seeded.Product.Id, seeded.Variants[1].Id, "Shipped", reservedQuantity: 3);
        await SeedOrderAsync(seeded.Product.Id, seeded.Variants[0].Id, "Delivered");
        await SeedOrderAsync(seeded.Product.Id, seeded.Variants[0].Id, "Cancelled");

        RemovalImpact? product = await RemovalReadAsync((removal, token) => removal.GetProductImpactAsync(seeded.Product.Id, token));
        RemovalImpact? firstVariant = await RemovalReadAsync((removal, token) => removal.GetVariantImpactAsync(seeded.Variants[0].Id, token));
        RemovalResult result = await RemoveAsync((removal, token) => removal.DeleteProductAsync(seeded.Product.Id, seeded.Product.Version, token));

        Assert.Equal(new RemovalImpact(2, 5, false), product);
        Assert.Equal(new RemovalImpact(1, 2, false), firstVariant);
        Assert.True(result.IsDone);
        await using AppDbContext reader = NewContext();
        Assert.Equal(4, await reader.Orders.CountAsync(Ct));
        Assert.Equal(0, await reader.StockReservations.CountAsync(Ct));
    }

    [Fact]
    public async Task ADeletedVariant_TakesItsReservationWithIt_AndMarksTheCartItemWithTheProductName_KK8()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Published, [PublishStatus.Published, PublishStatus.Published]);
        Guid orderId = await SeedOrderAsync(seeded.Product.Id, seeded.Variants[1].Id, "Preparing", reservedQuantity: 4);
        Guid cartId = await SeedCartAsync((seeded.Variants[1].Id, 2), (seeded.Variants[0].Id, 1));

        RemovalResult result = await RemoveAsync((removal, token) => removal.DeleteVariantAsync(seeded.Variants[1].Id, seeded.Product.Version, token));

        Assert.True(result.IsDone);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.Variants.AnyAsync(v => v.Id == seeded.Variants[1].Id, Ct));
        Assert.Equal(0, await reader.StockReservations.CountAsync(r => r.OrderId == orderId, Ct));
        OrderItem item = await reader.OrderItems.SingleAsync(i => i.OrderId == orderId, Ct);
        Assert.Equal(seeded.Product.Id, item.ProductId);
        Assert.Null(item.VariantId);
        List<CartItem> cartItems = await reader.CartItems.Where(i => i.CartId == cartId).ToListAsync(Ct);
        Assert.Equal("Masa", Assert.Single(cartItems, i => i.VariantId is null).RemovedProductName);
        Assert.Null(Assert.Single(cartItems, i => i.VariantId == seeded.Variants[0].Id).RemovedProductName);
    }

    [Fact]
    public async Task ADeletion_WithAStaleVersion_OrOfAGoneRecord_IsRefused_AndWritesNothing_KK8()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Draft, [PublishStatus.Draft, PublishStatus.Draft]);
        Guid cartId = await SeedCartAsync((seeded.Variants[0].Id, 1));

        RemovalResult product = await RemoveAsync((removal, token) => removal.DeleteProductAsync(seeded.Product.Id, seeded.Product.Version + 1, token));
        RemovalResult variant = await RemoveAsync((removal, token) => removal.DeleteVariantAsync(seeded.Variants[0].Id, seeded.Product.Version + 1, token));
        RemovalResult goneProduct = await RemoveAsync((removal, token) => removal.DeleteProductAsync(Guid.NewGuid(), 1, token));
        RemovalResult goneVariant = await RemoveAsync((removal, token) => removal.DeleteVariantAsync(Guid.NewGuid(), 1, token));

        Assert.Equal(
            [ErrorCode.RecordVersionConflict, ErrorCode.RecordVersionConflict, ErrorCode.NotFound, ErrorCode.NotFound],
            [product.Code, variant.Code, goneProduct.Code, goneVariant.Code]);
        Assert.Null(await RemovalReadAsync((removal, token) => removal.GetProductImpactAsync(Guid.NewGuid(), token)));
        await using AppDbContext reader = NewContext();
        Assert.Equal(2, await reader.Variants.CountAsync(v => v.ProductId == seeded.Product.Id, Ct));
        Assert.Equal(seeded.Product.Version, (await reader.Products.SingleAsync(p => p.Id == seeded.Product.Id, Ct)).Version);
        Assert.Null((await reader.CartItems.SingleAsync(i => i.CartId == cartId, Ct)).RemovedProductName);
    }

    [Fact]
    public async Task AWholePublishedProduct_WithOnlyOneVariant_CanBeDeleted_AsOneAct_KK8()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Published, [PublishStatus.Published]);

        RemovalResult result = await RemoveAsync((removal, token) => removal.DeleteProductAsync(seeded.Product.Id, seeded.Product.Version, token));

        Assert.True(result.IsDone);
        Assert.Empty((await ShowcaseAsync()).Items);
    }

    // ---- KK9: the last Published variant, three ways ----

    [Fact]
    public async Task TheLastPublishedVariant_IsKeptByTheDraftMove_TheArchiveMove_AndTheDeletion_KK9()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Published, [PublishStatus.Published, PublishStatus.Draft]);
        Guid last = seeded.Variants[0].Id;
        Guid cartId = await SeedCartAsync((last, 1));

        PublishChangeResult toDraft = await TransitionAsync((unit, token) => unit.ChangeVariantStatusAsync(last, PublishStatus.Draft, seeded.Product.Version, token));
        PublishChangeResult toArchive = await TransitionAsync((unit, token) => unit.ChangeVariantStatusAsync(last, PublishStatus.Archived, seeded.Product.Version, token));
        RemovalResult deletion = await RemoveAsync((removal, token) => removal.DeleteVariantAsync(last, seeded.Product.Version, token));
        RemovalImpact? impact = await RemovalReadAsync((removal, token) => removal.GetVariantImpactAsync(last, token));

        Assert.Equal([ErrorCode.LastPublishedVariant, ErrorCode.LastPublishedVariant, ErrorCode.LastPublishedVariant], [toDraft.Code, toArchive.Code, deletion.Code]);
        Assert.Equal([PublishStatus.Draft, PublishStatus.Archived], toDraft.SuggestedProductMoves);
        Assert.Equal(toDraft.SuggestedProductMoves, toArchive.SuggestedProductMoves);
        Assert.Equal(toDraft.SuggestedProductMoves, deletion.SuggestedProductMoves);
        Assert.True(impact!.IsLastPublishedVariant);
        Assert.Equal([PublishStatus.Published, PublishStatus.Draft], await VariantStatusesAsync(seeded));
        await using AppDbContext reader = NewContext();
        Assert.Null((await reader.CartItems.SingleAsync(i => i.CartId == cartId, Ct)).RemovedProductName);
        Assert.Equal(seeded.Product.Version, (await reader.Products.SingleAsync(p => p.Id == seeded.Product.Id, Ct)).Version);
    }

    [Fact]
    public async Task AVariant_IsFree_WhenAnotherVariantIsPublished_AndAMoveBackIsFree_KK9()
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, PublishStatus.Published, [PublishStatus.Published, PublishStatus.Published, PublishStatus.Published]);

        PublishChangeResult toDraft = await TransitionAsync((unit, token) => unit.ChangeVariantStatusAsync(seeded.Variants[0].Id, PublishStatus.Draft, seeded.Product.Version, token));
        PublishChangeResult toArchive = await TransitionAsync((unit, token) => unit.ChangeVariantStatusAsync(seeded.Variants[1].Id, PublishStatus.Archived, seeded.Product.Version + 1, token));
        RemovalImpact? impact = await RemovalReadAsync((removal, token) => removal.GetVariantImpactAsync(seeded.Variants[2].Id, token));

        Assert.True(toDraft.IsChanged);
        Assert.True(toArchive.IsChanged);
        Assert.True(impact!.IsLastPublishedVariant);
        Assert.Equal(ErrorCode.LastPublishedVariant, (await RemoveAsync((removal, token) => removal.DeleteVariantAsync(seeded.Variants[2].Id, seeded.Product.Version + 2, token))).Code);
        Assert.True((await TransitionAsync((unit, token) => unit.ChangeVariantStatusAsync(seeded.Variants[0].Id, PublishStatus.Published, seeded.Product.Version + 2, token))).IsChanged);
        Assert.True((await RemoveAsync((removal, token) => removal.DeleteVariantAsync(seeded.Variants[2].Id, seeded.Product.Version + 3, token))).IsDone);
    }

    [Theory]
    [InlineData(PublishStatus.Draft)]
    [InlineData(PublishStatus.Archived)]
    public async Task ThePublishedVariantOfAProductThatIsNotPublished_IsFree_ByEveryWay_KK9(PublishStatus productStatus)
    {
        Seeded seeded = await SeedAsync(ProductType.Physical, productStatus, [PublishStatus.Published, PublishStatus.Published]);
        await ExecuteAsync($"DELETE FROM variants WHERE id = '{seeded.Variants[1].Id}'");

        RemovalImpact? impact = await RemovalReadAsync((removal, token) => removal.GetVariantImpactAsync(seeded.Variants[0].Id, token));
        PublishChangeResult toDraft = await TransitionAsync((unit, token) => unit.ChangeVariantStatusAsync(seeded.Variants[0].Id, PublishStatus.Draft, seeded.Product.Version, token));
        PublishChangeResult toArchive = await TransitionAsync((unit, token) => unit.ChangeVariantStatusAsync(seeded.Variants[0].Id, PublishStatus.Archived, seeded.Product.Version + 1, token));
        RemovalResult deletion = await RemoveAsync((removal, token) => removal.DeleteVariantAsync(seeded.Variants[0].Id, seeded.Product.Version + 2, token));

        Assert.False(impact!.IsLastPublishedVariant);
        Assert.True(toDraft.IsChanged);
        Assert.True(toArchive.IsChanged);
        Assert.True(deletion.IsDone);
    }

    // ---- helpers ----

    private sealed record Seeded(Product Product, List<Variant> Variants);

    private Task<ShowcasePage> ShowcaseAsync(string? search = null) => WithCatalogAsync((catalog, token) => catalog.ListShowcaseAsync(search, 1, token));

    private Task<ProductPage> PageAsync(string? slug, bool isAdministrator) => WithCatalogAsync((catalog, token) => catalog.GetProductPageAsync(slug, isAdministrator, token));

    private async Task<T> WithCatalogAsync<T>(Func<ICatalogModule, CancellationToken, Task<T>> read)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await read(scope.ServiceProvider.GetRequiredService<ICatalogModule>(), Ct);
    }

    private async Task<T> RemovalReadAsync<T>(Func<IProductRemoval, CancellationToken, Task<T>> read)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await read(scope.ServiceProvider.GetRequiredService<IProductRemoval>(), Ct);
    }

    private async Task<PublishChangeResult> TransitionAsync(Func<IProductTransitions, CancellationToken, Task<PublishChangeResult>> work)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IProductTransitions unit = scope.ServiceProvider.GetRequiredService<IProductTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<RemovalResult> RemoveAsync(Func<IProductRemoval, CancellationToken, Task<RemovalResult>> work)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IProductRemoval unit = scope.ServiceProvider.GetRequiredService<IProductRemoval>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<PublishStatus> ProductStatusAsync(Guid productId)
    {
        await using AppDbContext reader = NewContext();
        return (await reader.Products.SingleAsync(p => p.Id == productId, Ct)).PublishStatus;
    }

    private async Task<List<PublishStatus>> VariantStatusesAsync(Seeded seeded)
    {
        await using AppDbContext reader = NewContext();
        Dictionary<Guid, PublishStatus> saved = await reader.Variants.Where(v => v.ProductId == seeded.Product.Id).ToDictionaryAsync(v => v.Id, v => v.PublishStatus, Ct);
        return [.. seeded.Variants.Select(v => saved[v.Id])];
    }

    private async Task<DateTimeOffset?> DetachedAtAsync(Guid fileId)
    {
        await using AppDbContext reader = NewContext();
        return (await reader.DigitalFiles.SingleAsync(f => f.Id == fileId, Ct)).DetachedAt;
    }

    private async Task<Guid> SeedCategoryAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Category category = Category.Create($"Kategori {n}", $"kategori-{n:D4}", null, 1, Now);
        await SaveAsync(c => c.Categories.Add(category));
        return category.Id;
    }

    private async Task<Seeded> SeedAsync(
        ProductType type,
        PublishStatus productStatus,
        PublishStatus[] variantStatuses,
        bool mainCategory = true,
        int? fulfilmentPeriod = null)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Masa", $"urun-{n:D4}", type, 20m, Now.AddMinutes(n));
        product.PublishStatus = productStatus;
        product.FulfilmentPeriod = fulfilmentPeriod;
        List<Variant> variants = [];
        foreach (PublishStatus status in variantStatuses)
        {
            Variant variant = Variant.Create(product.Id, $"SKU-{n:D4}-{variants.Count}", 100m, Now.AddMinutes(n).AddSeconds(variants.Count));
            variant.PublishStatus = status;
            variant.StockQuantity = type == ProductType.Physical ? 10 : null;
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

        Category? category = mainCategory ? Category.Create($"Kategori {n}", $"kategori-{n:D4}", null, 1, Now) : null;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            if (option is not null)
            {
                c.Options.Add(option);
                c.OptionValues.AddRange(values);
            }

            if (category is not null)
            {
                c.Categories.Add(category);
                c.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, true, Now));
            }

            c.Variants.AddRange(variants);
        });
        return new Seeded(product, variants);
    }

    private async Task<Guid> SeedFileAsync(Guid? productId = null, Guid? variantId = null, bool detached = false, bool attached = true)
    {
        int n = Interlocked.Increment(ref _counter);
        DigitalFile file = DigitalFile.Create(EntityIds.NewStorageKey(), $"kitap-{n}.pdf", 100, Now);
        file.UploadCompletedAt = Now;
        file.ProductId = productId;
        file.VariantId = variantId;
        if (attached)
        {
            file.AttachedAt = Now;
        }

        if (detached)
        {
            file.DetachedAt = Now.AddHours(1);
        }

        await SaveAsync(c => c.DigitalFiles.Add(file));
        return file.Id;
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

    private async Task<Guid> SeedOrderAsync(Guid productId, Guid variantId, string orderStatus, int reservedQuantity = 0)
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
            if (reservedQuantity > 0)
            {
                c.StockReservations.Add(StockReservation.ForVariant(order.Id, variantId, reservedQuantity, Now));
            }
        });
        await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = 'Paid' WHERE id = '{order.Id}'");
        if (reservedQuantity > 0)
        {
            await ExecuteAsync($"UPDATE variants SET reserved_quantity = reserved_quantity + {reservedQuantity} WHERE id = '{variantId}'");
        }

        return order.Id;
    }

    private async Task<string> OrderFingerprintAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        Order order = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        List<OrderItem> items = await reader.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).OrderBy(i => i.Id).ToListAsync(Ct);
        int records = await reader.TransitionRecords.CountAsync(t => t.OrderId == orderId, Ct);
        return $"{order.OrderStatus}|{order.PaymentStatus}|{order.Version}|{order.Total}|{records}|"
            + string.Join(';', items.Select(i => $"{i.Id}:{i.Quantity}:{i.ProductName}:{i.UnitPrice}"));
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
