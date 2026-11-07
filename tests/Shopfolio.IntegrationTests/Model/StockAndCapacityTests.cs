using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Variants;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the stock and capacity tests (T334): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(StockAndCapacityDefinition.Name)]
public sealed class StockAndCapacityDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "StockAndCapacity";
}

/// <summary>
/// Stock and service capacity through the real Catalog and Ordering code on a real PostgreSQL (T334 criteria 1-8; KK1-KK8; 02 3.6.1-3.6.7, 3.17.3, 7.4.8; 10 KD-9, KD-33): the stock is a
/// whole number the type requires (physical), forbids (digital) or leaves optional (service, empty is unlimited); a sold-out variant is marked by the available units after the
/// reserved ones and the product stays on the shop window; the visitor is given no number; a cancelled unit returns to the stock and a refunded one does not by itself; the panel's
/// stock write never goes below the reserved quantity, names it and the orders that hold it, and lets an increase and an emptied capacity through; and two confirmations of the last
/// unit reserve it once.
/// </summary>
[Collection(StockAndCapacityDefinition.Name)]
public sealed class StockAndCapacityTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private int _counter;

    public StockAndCapacityTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE stock_reservations, order_items, orders, variants, option_values, options, products, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1, KK2: the stock by type ----

    [Fact]
    public async Task APhysicalVariant_NeedsAStockNumber_AndZeroIsANumber_KK1_KK2()
    {
        Product product = await SeedProductAsync(ProductType.Physical);

        await Assert.ThrowsAsync<InvalidOperationException>(() => SaveAsync(c => c.Variants.Add(Variant.Create(product.Id, Sku(), 10m, Now))));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM variants WHERE product_id = '{product.Id}'"));
        await SeedVariantAsync(ProductType.Physical, 0);
        await SeedVariantAsync(ProductType.Physical, 8);

        Assert.Equal(["0", "8"], (await QueryAsync("SELECT stock_quantity::text FROM variants ORDER BY stock_quantity")));
    }

    [Fact]
    public async Task ADigitalVariant_HasNoStockField_AndIsRefusedOneWithANumber_KK2()
    {
        Product product = await SeedProductAsync(ProductType.Digital);

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => SaveAsync(c => c.Variants.Add(Variant.Create(product.Id, Sku(), 10m, Now, 5))));
        Assert.Contains("no stock field", error.Message, StringComparison.Ordinal);
        await SaveAsync(c => c.Variants.Add(Variant.Create(product.Id, Sku(), 10m, Now)));
    }

    [Fact]
    public async Task AServiceVariant_TakesACapacityOrNone_EmptyIsUnlimited_KK2()
    {
        await SeedVariantAsync(ProductType.Service, null);
        await SeedVariantAsync(ProductType.Service, 12);

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM variants WHERE stock_quantity IS NULL"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM variants WHERE stock_quantity = 12"));
    }

    [Fact]
    public async Task TheStock_NeverGoesBelowZero_ByTheGuardAndByTheDatabase_KK1()
    {
        Product product = await SeedProductAsync(ProductType.Physical);
        await Assert.ThrowsAsync<InvalidOperationException>(() => SaveAsync(c => c.Variants.Add(Variant.Create(product.Id, Sku(), 10m, Now, -1))));

        Variant variant = await SeedVariantAsync(ProductType.Physical, 3);
        PostgresException violation = await Assert.ThrowsAsync<PostgresException>(
            () => ExecuteAsync($"UPDATE variants SET stock_quantity = -1 WHERE id = '{variant.Id}'"));
        Assert.Equal("ck_variants_stock_not_below_reserved", violation.ConstraintName);
    }

    [Fact]
    public async Task ChangingAProductsType_UnderVariantsWhoseStockWouldNotFit_IsRefused_KK2()
    {
        Variant physical = await SeedVariantAsync(ProductType.Physical, 4);
        Variant service = await SeedVariantAsync(ProductType.Service, null);

        // Physical to digital leaves a number on a variant that has no field; service to physical leaves a physical variant without a number.
        await Assert.ThrowsAsync<InvalidOperationException>(() => ChangeTypeAsync(physical.ProductId, ProductType.Digital));
        await Assert.ThrowsAsync<InvalidOperationException>(() => ChangeTypeAsync(service.ProductId, ProductType.Physical));

        // Service to digital with no capacity fits; the same service product may become digital.
        await ChangeTypeAsync(service.ProductId, ProductType.Digital);
        Assert.Equal("Digital", await ScalarAsync<string>($"SELECT product_type FROM products WHERE id = '{service.ProductId}'"));
        Assert.Equal("Physical", await ScalarAsync<string>($"SELECT product_type FROM products WHERE id = '{physical.ProductId}'"));
    }

    [Fact]
    public async Task ThePanelsWrite_FollowsTheType_APhysicalNeedsANumber_ADigitalHasNoField_KK2()
    {
        Variant physical = await SeedVariantAsync(ProductType.Physical, 5);
        Variant digital = await SeedVariantAsync(ProductType.Digital, null);

        StockQuantityWriteResult emptied = await SaveStockAsync(physical.Id, 5, null);
        StockQuantityWriteResult given = await SaveStockAsync(digital.Id, null, 3);
        StockQuantityWriteResult same = await SaveStockAsync(digital.Id, null, null);

        Assert.True(emptied.IsStockRequired);
        Assert.True(given.IsNotTracked);
        Assert.True(same.IsWritten);
        await AssertStateAsync(physical.Id, 5, 0);
        await AssertStateAsync(digital.Id, null, 0);
    }

    [Fact]
    public async Task ThePanelsWrite_RefusesANegativeNumber_AndTheStockStaysAWholeNumber_KK1()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 5);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => SaveStockAsync(variant.Id, 5, -1));
        await AssertStateAsync(variant.Id, 5, 0);
    }

    // ---- KK3, KK4, KK5: sold out, shown and marked, with no number ----

    [Fact]
    public async Task AVariantWhoseLastUnitIsReserved_IsSoldOut_AndComesBackWhenTheReservationIsFreed_KK3()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 2, published: true);

        Assert.False(await SoldOutAsync(variant));
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(variant.Id, 2), null, Ct));
        Assert.True(await SoldOutAsync(variant));
        await InTransactionAsync(catalog => catalog.ReleaseAsync(Claim(variant.Id, 2), Ct));
        Assert.False(await SoldOutAsync(variant));

        // Paid: the stock itself goes down and the variant stays sold out for good.
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(variant.Id, 2), null, Ct));
        await InTransactionAsync(catalog => catalog.CommitAsync(Claim(variant.Id, 2), Ct));
        Assert.True(await SoldOutAsync(variant));
        await AssertStateAsync(variant.Id, 0, 0);
    }

    [Fact]
    public async Task ASoldOutVariant_IsListedWithTheMark_NotHidden_AndADraftVariantIsNotListedAtAll_KK3()
    {
        Product product = await SeedProductAsync(ProductType.Physical, published: true);
        Variant[] variants = await SeedOptionVariantsAsync(product, (0, true), (6, true), (0, false));
        (Variant soldOut, Variant onSale) = (variants[0], variants[1]);

        ProductAvailability availability = await AvailabilityAsync(product.Id);

        Assert.Equal([(soldOut.Id, true), (onSale.Id, false)], availability.Variants.Select(v => (v.VariantId, v.IsSoldOut)));
        Assert.False(availability.IsSoldOut);
    }

    [Fact]
    public async Task AProductWithEveryVariantSoldOut_StaysPublished_AndIsMarkedSoldOut_NoNotifyRecordIsKept_KK4()
    {
        Product product = await SeedProductAsync(ProductType.Physical, published: true);
        Variant first = (await SeedOptionVariantsAsync(product, (1, true), (0, true)))[0];
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(first.Id, 1), null, Ct));

        ProductAvailability availability = await AvailabilityAsync(product.Id);

        Assert.True(availability.IsSoldOut);
        Assert.All(availability.Variants, v => Assert.True(v.IsSoldOut));
        Assert.Equal("Published", await ScalarAsync<string>($"SELECT publish_status FROM products WHERE id = '{product.Id}'"));
        Assert.Equal(2L, await ScalarAsync<long>($"SELECT count(*) FROM variants WHERE product_id = '{product.Id}' AND publish_status = 'Published'"));
    }

    [Fact]
    public async Task NoTableOrColumn_KeepsANotifyMeRecord_ALowStockThreshold_OrAWaitingList_KK4_KK5()
    {
        List<string> names = await QueryAsync(
            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_schema = 'public' "
            + "UNION SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY 1");

        string[] forbidden =
        [
            "notify", "waitlist", "wait_list", "back_in_stock", "stock_alert", "stock_notification", "restock", "low_stock", "stock_threshold", "stock_warning",
            "units_left", "remaining_stock", "remaining_units",
        ];
        Assert.DoesNotContain(names, n => forbidden.Any(f => n.Contains(f, StringComparison.Ordinal)));
        Assert.Contains("variants.stock_quantity", names);
    }

    [Fact]
    public async Task APhysicalVariantsAvailability_IsOnlyTheFlag_NeverACount_KK5()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 2, published: true);

        ProductAvailability availability = await AvailabilityAsync(variant.ProductId);

        // The flag is the whole answer: stock 2 and stock 1 of 1 reserved look alike except for the flag; nothing else in the type could carry a number.
        Assert.Single(availability.Variants);
        Assert.False(availability.Variants[0].IsSoldOut);
        Assert.DoesNotContain(typeof(VariantAvailability).GetProperties().Concat(typeof(ProductAvailability).GetProperties()),
            p => p.PropertyType == typeof(int) || p.PropertyType == typeof(int?) || p.PropertyType == typeof(string));
    }

    [Fact]
    public async Task ADigitalVariantAndAServiceWithoutCapacity_AreNeverSoldOut_KK2_KK3()
    {
        Variant digital = await SeedVariantAsync(ProductType.Digital, null, published: true);
        Variant unlimited = await SeedVariantAsync(ProductType.Service, null, published: true);
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(unlimited.Id, 40), null, Ct));

        Assert.False(await SoldOutAsync(digital));
        Assert.False(await SoldOutAsync(unlimited));
        await AssertStateAsync(unlimited.Id, null, 40);
    }

    [Fact]
    public async Task AServiceWithACapacity_IsSoldOutWhenTheCapacityIsFull_LikeAPhysicalStock_KK2_KK3()
    {
        Variant service = await SeedVariantAsync(ProductType.Service, 3, published: true);

        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(service.Id, 3), null, Ct));

        Assert.True(await SoldOutAsync(service));
        Assert.False(await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(service.Id, 1), null, Ct)));
    }

    // ---- KK6: what returns ----

    [Fact]
    public async Task ACancelledUnpaidOrder_ReturnsItsUnitsToTheAvailable_ThroughTheRelease_KK6()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 10);
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(variant.Id, 4), null, Ct));
        await AssertStateAsync(variant.Id, 10, 4);

        await InTransactionAsync(catalog => catalog.ReleaseAsync(Claim(variant.Id, 4), Ct));

        Assert.Equal(new VariantStockState(10, 0), await StateAsync(variant.Id));
    }

    [Fact]
    public async Task ACancelledPaidOrder_ReturnsOnlyTheCancelledUnits_APartialCancelAddsThatMany_KK6()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 10);
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(variant.Id, 3), null, Ct));
        await InTransactionAsync(catalog => catalog.CommitAsync(Claim(variant.Id, 3), Ct));
        await AssertStateAsync(variant.Id, 7, 0);

        await InTransactionAsync(catalog => catalog.ReturnAsync(Claim(variant.Id, 2), Ct));

        Assert.Equal(new VariantStockState(9, 0), await StateAsync(variant.Id));
    }

    [Fact]
    public async Task AServicesWithdrawal_ReturnsTheWithdrawnUnitsToThePool_AnUnlimitedOneHasNothingToReturn_KK6()
    {
        Variant limited = await SeedVariantAsync(ProductType.Service, 5);
        Variant unlimited = await SeedVariantAsync(ProductType.Service, null);
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(limited.Id, 4), null, Ct));
        await InTransactionAsync(catalog => catalog.CommitAsync(Claim(limited.Id, 4), Ct));
        await AssertStateAsync(limited.Id, 1, 0);

        await InTransactionAsync(catalog => catalog.ReturnAsync(Claim(limited.Id, 3), Ct));
        await InTransactionAsync(catalog => catalog.ReturnAsync(Claim(unlimited.Id, 3), Ct));

        Assert.Equal(new VariantStockState(4, 0), await StateAsync(limited.Id));
        Assert.Equal(new VariantStockState(null, 0), await StateAsync(unlimited.Id));
    }

    [Fact]
    public async Task ARefundedPhysicalUnit_IsNotReturnedByItself_TheAdminAddsItAfterChecking_KK6()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 10);
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(variant.Id, 2), null, Ct));
        await InTransactionAsync(catalog => catalog.CommitAsync(Claim(variant.Id, 2), Ct));

        // The refund is money (Payment) and a record (AfterSales); no stock movement belongs to it. The units are back only when the admin writes them from the panel.
        await AssertStateAsync(variant.Id, 8, 0);
        StockQuantityWriteResult added = await SaveStockAsync(variant.Id, 8, 10);

        Assert.True(added.IsWritten);
        await AssertStateAsync(variant.Id, 10, 0);
    }

    // ---- KK7: the reserved quantity as a floor ----

    [Fact]
    public async Task ALoweringBelowTheReservedQuantity_IsRefused_TellsTheReservedQuantity_AndNamesTheHoldingOrders_KK7()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 10);
        (Order first, _) = await SeedOrderAsync();
        (Order second, _) = await SeedOrderAsync();
        await ReserveForOrderAsync(first, variant, 3, Now);
        await ReserveForOrderAsync(second, variant, 2, Now.AddMinutes(5));

        StockQuantityWriteResult below = await SaveStockAsync(variant.Id, 10, 4);
        VariantStockState? state = await StateAsync(variant.Id);
        IReadOnlyList<ReservationHolder> holders = await HoldersAsync(variant.Id);

        Assert.True(below.IsRejected);
        Assert.True(below.IsBelowReserved);
        Assert.Equal(5, below.ReservedQuantity);
        Assert.Equal(new VariantStockState(10, 5), state);
        Assert.Equal([(second.OrderNumber, 2), (first.OrderNumber, 3)], holders.Select(h => (h.OrderNumber, h.Quantity)));
        Assert.Equal(below.ReservedQuantity, holders.Sum(h => h.Quantity));
        await AssertStateAsync(variant.Id, 10, 5);
    }

    [Fact]
    public async Task TheHoldersOfAVariant_AreOnlyTheOrdersThatReserveIt_AndNoneOnceTheReservationIsGone_KK7()
    {
        Variant held = await SeedVariantAsync(ProductType.Physical, 10);
        Variant other = await SeedVariantAsync(ProductType.Physical, 10);
        (Order order, _) = await SeedOrderAsync();
        await ReserveForOrderAsync(order, held, 2, Now);

        Assert.Single(await HoldersAsync(held.Id));
        Assert.Empty(await HoldersAsync(other.Id));

        await InTransactionAsync(async catalog =>
        {
            await catalog.ReleaseAsync(Claim(held.Id, 2), Ct);
            await ExecuteAsync($"DELETE FROM stock_reservations WHERE order_id = '{order.Id}'");
        });
        Assert.Empty(await HoldersAsync(held.Id));
    }

    [Fact]
    public async Task ALoweringToExactlyTheReservedQuantity_IsAccepted_AndOneBelowIsNot_KK7()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 10);
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(variant.Id, 4), null, Ct));

        StockQuantityWriteResult below = await SaveStockAsync(variant.Id, 10, 3);
        StockQuantityWriteResult exact = await SaveStockAsync(variant.Id, 10, 4);

        Assert.True(below.IsBelowReserved);
        Assert.True(exact.IsWritten);
        await AssertStateAsync(variant.Id, 4, 4);
    }

    [Fact]
    public async Task IncreasingTheStock_AndEmptyingTheCapacity_AreFree_EvenWithUnitsReserved_KK7()
    {
        Variant physical = await SeedVariantAsync(ProductType.Physical, 6);
        Variant service = await SeedVariantAsync(ProductType.Service, 6, published: true);
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(physical.Id, 6), null, Ct));
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(service.Id, 5), null, Ct));

        StockQuantityWriteResult raised = await SaveStockAsync(physical.Id, 6, 50);
        StockQuantityWriteResult emptied = await SaveStockAsync(service.Id, 6, null);

        Assert.True(raised.IsWritten);
        Assert.True(emptied.IsWritten);
        await AssertStateAsync(physical.Id, 50, 6);
        await AssertStateAsync(service.Id, null, 5);
        Assert.False(await SoldOutAsync(service));
    }

    [Fact]
    public async Task ACapacityEnteredForAnUnlimitedService_IsRefusedBelowWhatIsAlreadyReserved_KK7()
    {
        Variant service = await SeedVariantAsync(ProductType.Service, null);
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(service.Id, 5), null, Ct));

        StockQuantityWriteResult below = await SaveStockAsync(service.Id, null, 4);
        StockQuantityWriteResult fits = await SaveStockAsync(service.Id, null, 5);

        Assert.True(below.IsBelowReserved);
        Assert.Equal(5, below.ReservedQuantity);
        Assert.True(fits.IsWritten);
    }

    [Fact]
    public async Task AValueThatChangedUnderTheForm_IsReportedAsChanged_NotAsBelowReserved_KK7()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 10);
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(variant.Id, 4), null, Ct));

        StockQuantityWriteResult changed = await SaveStockAsync(variant.Id, 9, 1);

        Assert.True(changed.IsShownValueChanged);
        Assert.False(changed.IsBelowReserved);
        Assert.Null(changed.ReservedQuantity);
    }

    [Fact]
    public async Task TheStockStateOfAVariantThatIsGone_IsNull_AndAWriteToItReportsAChangedValue_KK7()
    {
        Assert.Null(await StateAsync(Guid.NewGuid()));
        Assert.True((await SaveStockAsync(Guid.NewGuid(), 1, 2)).IsShownValueChanged);
    }

    // ---- KK8: the last unit, two confirmations ----

    [Fact]
    public async Task TwoConfirmationsOfTheLastUnit_AtTheSameTime_ReserveItOnce_KK8()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 1);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope first = provider.CreateAsyncScope();
        await using AsyncServiceScope second = provider.CreateAsyncScope();
        AppDbContext firstDb = first.ServiceProvider.GetRequiredService<AppDbContext>();
        AppDbContext secondDb = second.ServiceProvider.GetRequiredService<AppDbContext>();

        await using IDbContextTransaction firstTransaction = await firstDb.Database.BeginTransactionAsync(Ct);
        await using IDbContextTransaction secondTransaction = await secondDb.Database.BeginTransactionAsync(Ct);
        Task<bool> a = first.ServiceProvider.GetRequiredService<ICatalogModule>().ReserveAsync(Claim(variant.Id, 1), null, Ct);
        Task<bool> b = second.ServiceProvider.GetRequiredService<ICatalogModule>().ReserveAsync(Claim(variant.Id, 1), null, Ct);

        // One of the two holds the row lock; the other waits for it. Whichever finished first commits, then the other gets its answer from the committed row.
        Task<bool> winner = await Task.WhenAny(a, b);
        bool winnerReserved = await winner;
        if (winner == a)
        {
            await firstTransaction.CommitAsync(Ct);
        }
        else
        {
            await secondTransaction.CommitAsync(Ct);
        }

        Task<bool> loser = winner == a ? b : a;
        bool loserReserved = await loser.WaitAsync(TimeSpan.FromSeconds(30), Ct);

        Assert.True(winnerReserved);
        Assert.False(loserReserved);
        await (winner == a ? secondTransaction : firstTransaction).RollbackAsync(Ct);
        await AssertStateAsync(variant.Id, 1, 1);
    }

    [Fact]
    public async Task AConfirmationThatTakesMoreThanIsLeft_ReservesNothing_AndTheCountsStayAsTheyWere_KK8()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 3);
        await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(variant.Id, 2), null, Ct));

        bool reserved = await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(variant.Id, 2), null, Ct));

        Assert.False(reserved);
        await AssertStateAsync(variant.Id, 3, 2);
    }

    // ---- Helpers ----

    private static StockClaim Claim(Guid variantId, int quantity) => new([new VariantQuantity(variantId, quantity)], null);

    private string Sku() => $"STK-{Interlocked.Increment(ref _counter)}";

    private async Task<Product> SeedProductAsync(ProductType type, bool published = false)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Ürün", $"urun-{n}", type, 20m, Now);
        if (published)
        {
            product.PublishStatus = PublishStatus.Published;
            product.FulfilmentPeriod = type == ProductType.Service ? 3 : null;
        }

        await SaveAsync(c => c.Products.Add(product));
        return product;
    }

    private async Task<Variant> SeedVariantAsync(ProductType type, int? stock, bool published = false)
    {
        Product product = await SeedProductAsync(type, published);
        Variant variant = Variant.Create(product.Id, Sku(), 10m, Now, stock);
        if (published)
        {
            variant.PublishStatus = PublishStatus.Published;
        }

        await SaveAsync(c => c.Variants.Add(variant));
        return variant;
    }

    /// <summary>Gives the product one option and one variant per value, in the order given: the stock and whether the variant is published.</summary>
    private async Task<Variant[]> SeedOptionVariantsAsync(Product product, params (int? Stock, bool Published)[] parts)
    {
        Option option = Option.Create(product.Id, "Renk", 1, Now);
        OptionValue[] values = [.. parts.Select((_, i) => OptionValue.Create(option.Id, $"Değer {i + 1}", i + 1, Now))];
        Variant[] variants = [.. parts.Select((p, i) => Variant.Create(product.Id, Sku(), 10m, Now.AddMinutes(i), p.Stock))];
        for (int i = 0; i < variants.Length; i++)
        {
            variants[i].FirstOptionValueId = values[i].Id;
            variants[i].PublishStatus = parts[i].Published ? PublishStatus.Published : PublishStatus.Draft;
        }

        await SaveAsync(c =>
        {
            c.Options.Add(option);
            c.OptionValues.AddRange(values);
            c.Variants.AddRange(variants);
        });
        return variants;
    }

    private async Task ChangeTypeAsync(Guid productId, ProductType type)
    {
        await using AppDbContext context = NewContext();
        Product product = await context.Products.SingleAsync(p => p.Id == productId, Ct);
        product.ProductType = type;
        product.BumpVersion();
        await context.SaveChangesAsync(Ct);
    }

    private Task<StockQuantityWriteResult> SaveStockAsync(Guid variantId, int? shown, int? value)
    {
        return RunAsync((services, token) => services.GetRequiredService<ICatalogModule>().SaveStockQuantityAsync(variantId, shown, value, token));
    }

    private async Task<bool> SoldOutAsync(Variant variant)
    {
        ProductAvailability availability = await AvailabilityAsync(variant.ProductId);
        return availability.Variants.Single(v => v.VariantId == variant.Id).IsSoldOut;
    }

    private async Task<ProductAvailability> AvailabilityAsync(Guid productId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICatalogModule>().GetAvailabilityAsync(productId, Ct);
    }

    private async Task<VariantStockState?> StateAsync(Guid variantId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICatalogModule>().GetStockStateAsync(variantId, Ct);
    }

    private async Task<IReadOnlyList<ReservationHolder>> HoldersAsync(Guid variantId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IOrderingModule>().ListReservationHoldersAsync(variantId, Ct);
    }

    private async Task AssertStateAsync(Guid variantId, int? stock, int reserved)
    {
        VariantStockState? state = await StateAsync(variantId);
        Assert.Equal(new VariantStockState(stock, reserved), state);
    }

    /// <summary>Reserves through the counters and writes the order's reservation row, as the confirmation will (06 4.14 path 1).</summary>
    private async Task ReserveForOrderAsync(Order order, Variant variant, int quantity, DateTimeOffset at)
    {
        Assert.True(await InTransactionAsync(catalog => catalog.ReserveAsync(Claim(variant.Id, quantity), null, Ct)));
        await SaveAsync(c => c.StockReservations.Add(StockReservation.ForVariant(order.Id, variant.Id, quantity, at)));
    }

    private async Task<T> InTransactionAsync<T>(Func<ICatalogModule, Task<T>> work)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(Ct);
        T result = await work(scope.ServiceProvider.GetRequiredService<ICatalogModule>());
        await transaction.CommitAsync(Ct);
        return result;
    }

    private async Task InTransactionAsync(Func<ICatalogModule, Task> work)
    {
        await InTransactionAsync(async catalog =>
        {
            await work(catalog);
            return true;
        });
    }

    private async Task<T> RunAsync<T>(Func<IServiceProvider, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(scope.ServiceProvider, token), Ct);
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
        services.AddPaymentModule();
        services.AddOrderingModule();
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

    private async Task<(Order Order, OrderItem Item)> SeedOrderAsync()
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
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Ürün", 1, 100m, 20m, 16.67m, Now);
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });
        return (order, item);
    }
}
