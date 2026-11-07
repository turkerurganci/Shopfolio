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
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Concurrency;

/// <summary>The collection of the concurrency tests (T228): one database copied from the template.</summary>
[CollectionDefinition(ConcurrencyDefinition.Name)]
public sealed class ConcurrencyDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "Concurrency";
}

/// <summary>
/// The concurrency control of T228 on a real PostgreSQL (09 10.2; 05 7.3; 06 2.6): the transaction runner, the record version (KK1, KK2),
/// the order's version under its row lock (KK3, KK4), the counters' conditional updates (KK5, KK6), the lock order (KK7, KK8) and the lock
/// takers outside a transaction. A second transaction's wait is read from <c>pg_stat_activity</c> (its backend waits on a lock), not timed.
/// The operations that will use these pieces (the order operations, the product form) are later tasks; where a test needs one, a test-local
/// stand-in follows 09 10.2 items 2 and 3.
/// </summary>
[Collection(ConcurrencyDefinition.Name)]
public sealed class ConcurrencyTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private static readonly ItemAmounts Amounts = new(100m, 10m, 15m);

    private readonly CollectionDatabaseFixture _collection;

    private int _counter;

    public ConcurrencyTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE cancellations, internal_notes, transition_records, order_items, orders, admins, coupons, variants, products, faq_items, "
            + "legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- The transaction runner (09 9.1 item 3) ----

    [Fact]
    public async Task Runner_ANotRefusedResult_IsSavedAndCommitted()
    {
        Guid faqId = Guid.Empty;
        await using ServiceProvider provider = BuildProvider();
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Outcome outcome = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
                token =>
                {
                    FaqItem faq = FaqItem.Create("Soru?", 1, Now);
                    db.FaqItems.Add(faq);
                    faqId = faq.Id;
                    return Task.FromResult(Outcome.Done);
                },
                Ct);

            Assert.False(outcome.IsRejected);
            Assert.Null(db.Database.CurrentTransaction);
        }

        await using AppDbContext reader = NewContext();
        Assert.True(await reader.FaqItems.AnyAsync(f => f.Id == faqId, Ct));
    }

    [Fact]
    public async Task Runner_ARefusedResult_RollsBackTheTrackedChangesAndTheStatementsAlreadySent()
    {
        Variant variant = await SeedVariantAsync(stock: 5);
        Guid faqId = Guid.Empty;
        await using ServiceProvider provider = BuildProvider();
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ICatalogModule catalog = scope.ServiceProvider.GetRequiredService<ICatalogModule>();
            Outcome outcome = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
                async token =>
                {
                    Assert.True(await catalog.ReserveAsync(Claim(variant.Id, 2), null, token));
                    FaqItem faq = FaqItem.Create("Soru?", 1, Now);
                    db.FaqItems.Add(faq);
                    faqId = faq.Id;
                    return Outcome.Refused;
                },
                Ct);

            Assert.True(outcome.IsRejected);
            Assert.Empty(db.ChangeTracker.Entries());
        }

        await using AppDbContext reader = NewContext();
        Assert.False(await reader.FaqItems.AnyAsync(f => f.Id == faqId, Ct));
        Assert.Equal(0, (await reader.Variants.SingleAsync(v => v.Id == variant.Id, Ct)).ReservedQuantity);
    }

    [Fact]
    public async Task Runner_AnException_RollsBackAndIsThrownAgain()
    {
        Variant variant = await SeedVariantAsync(stock: 5);
        await using ServiceProvider provider = BuildProvider();
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            ICatalogModule catalog = scope.ServiceProvider.GetRequiredService<ICatalogModule>();
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync<Outcome>(
                    async token =>
                    {
                        await catalog.ReserveAsync(Claim(variant.Id, 2), null, token);
                        throw new InvalidOperationException("planned failure");
                    },
                    Ct));

            Assert.Equal("planned failure", error.Message);
        }

        await using AppDbContext reader = NewContext();
        Assert.Equal(0, (await reader.Variants.SingleAsync(v => v.Id == variant.Id, Ct)).ReservedQuantity);
    }

    [Fact]
    public async Task Runner_CalledInsideARunningTransaction_Throws()
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runner.RunAsync(token => runner.RunAsync(inner => Task.FromResult(Outcome.Done), token), Ct));

        Assert.Contains("already open", error.Message, StringComparison.Ordinal);
    }

    // ---- KK1, KK2: the record version (05 7.3.1; 09 10.2 items 1, 2) ----

    [Fact]
    public async Task RecordSave_IsOneConditionalUpdate_AndOfTwoSavesOfTheSameVersionOneFails_KK1()
    {
        Product product = await SeedProductAsync();
        CommandRecorder recorder = new();
        await using AppDbContext first = NewContext(recorder);
        await using AppDbContext second = NewContext();
        Product firstCopy = await first.Products.SingleAsync(p => p.Id == product.Id, Ct);
        Product secondCopy = await second.Products.SingleAsync(p => p.Id == product.Id, Ct);
        firstCopy.Description = "ilk";
        firstCopy.BumpVersion();
        secondCopy.Description = "ikinci";
        secondCopy.BumpVersion();

        recorder.Commands.Clear();
        await first.SaveChangesAsync(Ct);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync(Ct));

        string update = Assert.Single(recorder.Commands);
        Assert.StartsWith("UPDATE products SET", update, StringComparison.Ordinal);
        Assert.Contains("version = @", update[update.IndexOf("WHERE", StringComparison.Ordinal)..], StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Product saved = await reader.Products.SingleAsync(p => p.Id == product.Id, Ct);
        Assert.Equal(product.Version + 1, saved.Version);
        Assert.Equal("ilk", saved.Description);
    }

    [Fact]
    public async Task RecordUpdate_WithAStaleVersion_AffectsNoRow_KK1()
    {
        Product product = await SeedProductAsync();

        int written = await ExecuteCountAsync(
            "UPDATE products SET description = 'eski', version = version + 1 WHERE id = @id AND version = @version",
            ("id", product.Id),
            ("version", product.Version - 1));

        Assert.Equal(0, written);
    }

    [Fact]
    public async Task RecordSave_TakesNoLock_AnotherSaveDoesNotWaitForAnOpenTransactionThatLoadedIt_KK2()
    {
        Product product = await SeedProductAsync();
        await using AppDbContext holder = NewContext();
        await using IDbContextTransaction open = await holder.Database.BeginTransactionAsync(Ct);
        Product held = await holder.Products.SingleAsync(p => p.Id == product.Id, Ct);

        await using (AppDbContext other = NewContext())
        {
            Product copy = await other.Products.SingleAsync(p => p.Id == product.Id, Ct);
            copy.Description = "başka";
            copy.BumpVersion();
            await other.SaveChangesAsync(Ct).WaitAsync(TimeSpan.FromSeconds(10), Ct);
        }

        held.Description = "geç";
        held.BumpVersion();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => holder.SaveChangesAsync(Ct));
    }

    [Fact]
    public async Task VariantChange_RaisesTheProductsVersion_AndWithoutTheRaiseIsRefused_KK2()
    {
        Variant variant = await SeedVariantAsync(stock: 5);
        await using (AppDbContext refused = NewContext())
        {
            await refused.Products.SingleAsync(p => p.Id == variant.ProductId, Ct);
            Variant copy = await refused.Variants.SingleAsync(v => v.Id == variant.Id, Ct);
            copy.Price = 120m;

            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => refused.SaveChangesAsync(Ct));
            Assert.Contains("BumpVersion", error.Message, StringComparison.Ordinal);
        }

        await using (AppDbContext form = NewContext())
        {
            Product product = await form.Products.SingleAsync(p => p.Id == variant.ProductId, Ct);
            Variant copy = await form.Variants.SingleAsync(v => v.Id == variant.Id, Ct);
            copy.Price = 120m;
            product.BumpVersion();
            await form.SaveChangesAsync(Ct);
        }

        await using AppDbContext reader = NewContext();
        Assert.Equal(2, (await reader.Products.SingleAsync(p => p.Id == variant.ProductId, Ct)).Version);
        Assert.Equal(120m, (await reader.Variants.SingleAsync(v => v.Id == variant.Id, Ct)).Price);
    }

    [Fact]
    public async Task ReserveCommitReturnAndThePanelsStockWrite_DoNotRaiseTheProductsOrTheCouponsVersion_KK2()
    {
        Variant variant = await SeedVariantAsync(stock: 10);
        Coupon coupon = await SeedCouponAsync(limit: 5);
        StockClaim claim = new([new VariantQuantity(variant.Id, 3)], coupon.Id);

        await RunAsync(async (services, token) =>
        {
            ICatalogModule catalog = services.GetRequiredService<ICatalogModule>();
            Assert.True(await catalog.ReserveAsync(claim, null, token));
            return Outcome.Done;
        });
        await AssertCountersAsync(variant.Id, stock: 10, reserved: 3);
        await AssertCouponAsync(coupon.Id, used: 0, reserved: 1);

        await RunAsync(async (services, token) =>
        {
            await services.GetRequiredService<ICatalogModule>().CommitAsync(claim, token);
            return Outcome.Done;
        });
        await AssertCountersAsync(variant.Id, stock: 7, reserved: 0);
        await AssertCouponAsync(coupon.Id, used: 1, reserved: 0);

        await RunAsync(async (services, token) =>
        {
            await services.GetRequiredService<ICatalogModule>().ReturnAsync(claim, token);
            return Outcome.Done;
        });
        await AssertCountersAsync(variant.Id, stock: 10, reserved: 0);
        await AssertCouponAsync(coupon.Id, used: 0, reserved: 0);

        StockQuantityWriteResult written = await RunAsync((services, token) =>
            services.GetRequiredService<ICatalogModule>().SaveStockQuantityAsync(variant.Id, 10, 12, token));
        Assert.True(written.IsWritten);

        await using AppDbContext reader = NewContext();
        Assert.Equal(1, (await reader.Products.SingleAsync(p => p.Id == variant.ProductId, Ct)).Version);
        Assert.Equal(1, (await reader.Coupons.SingleAsync(c => c.Id == coupon.Id, Ct)).Version);
        Assert.Equal(12, (await reader.Variants.SingleAsync(v => v.Id == variant.Id, Ct)).StockQuantity);
    }

    [Fact]
    public async Task Save_ThatWritesTheStockField_IsRefused_ItIsWrittenOnlyByItsConditionalUpdate()
    {
        Variant variant = await SeedVariantAsync(stock: 5);
        await using AppDbContext form = NewContext();
        Product product = await form.Products.SingleAsync(p => p.Id == variant.ProductId, Ct);
        Variant copy = await form.Variants.SingleAsync(v => v.Id == variant.Id, Ct);
        copy.StockQuantity = 50;
        product.BumpVersion();

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => form.SaveChangesAsync(Ct));

        Assert.Contains("StockQuantity", error.Message, StringComparison.Ordinal);
        await AssertCountersAsync(variant.Id, stock: 5, reserved: 0);
    }

    // ---- KK3: the order's version covers the order side (05 7.3.2; 06 2.6.4) ----

    [Fact]
    public async Task ItemRecordUnderTheOrderLock_WithoutTheRaise_IsRefused_WithTheRaise_RaisesTheVersionByOne_KK3()
    {
        (Order order, OrderItem item) = await SeedOrderAsync();

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(async (services, token) =>
        {
            AppDbContext db = services.GetRequiredService<AppDbContext>();
            await services.GetRequiredService<OrderLocks>().LockAsync([order.Id], token);
            await db.Orders.SingleAsync(o => o.Id == order.Id, token);
            db.Cancellations.Add(Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now));
            return Outcome.Done;
        }));
        Assert.Contains("BumpVersion", error.Message, StringComparison.Ordinal);

        await RunAsync(async (services, token) =>
        {
            AppDbContext db = services.GetRequiredService<AppDbContext>();
            await services.GetRequiredService<OrderLocks>().LockAsync([order.Id], token);
            Order locked = await db.Orders.SingleAsync(o => o.Id == order.Id, token);
            db.Cancellations.Add(Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now));
            locked.BumpVersion();
            return Outcome.Done;
        });

        await using AppDbContext reader = NewContext();
        Assert.Equal(2, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).Version);
        Assert.Equal(1, await reader.Cancellations.CountAsync(c => c.OrderItemId == item.Id, Ct));
    }

    [Fact]
    public async Task OrderChangeUnderTheLock_WithoutTheRaise_IsRefused_KK3()
    {
        (Order order, _) = await SeedOrderAsync();

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(async (services, token) =>
        {
            AppDbContext db = services.GetRequiredService<AppDbContext>();
            await services.GetRequiredService<OrderLocks>().LockAsync([order.Id], token);
            Order locked = await db.Orders.SingleAsync(o => o.Id == order.Id, token);
            locked.BillingDistrict = "Kızılay";
            return Outcome.Done;
        }));

        Assert.Contains("BumpVersion", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InternalNoteUnderTheOrderLock_DoesNotRaiseTheVersion_KK3()
    {
        (Order order, _) = await SeedOrderAsync();
        Admin admin = Admin.Create("Veli", "veli@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(admin));

        await RunAsync(async (services, token) =>
        {
            AppDbContext db = services.GetRequiredService<AppDbContext>();
            await services.GetRequiredService<OrderLocks>().LockAsync([order.Id], token);
            await db.Orders.SingleAsync(o => o.Id == order.Id, token);
            db.InternalNotes.Add(InternalNote.Create(order.Id, "Kargo gecikti.", admin.Id, admin.Name, Now));
            return Outcome.Done;
        });

        await using AppDbContext reader = NewContext();
        Assert.Equal(1, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).Version);
        Assert.True(await reader.InternalNotes.AnyAsync(n => n.OrderId == order.Id, Ct));
    }

    // ---- KK4: the order's row lock (05 7.3.2; MK-21; K-1202) ----

    [Fact]
    public async Task OrderLock_TheSecondOrderOperationWaitsForTheFirst_KK4()
    {
        (Order order, _) = await SeedOrderAsync();

        (bool waitedWhileHeld, bool gotItAfterRelease) = await SecondWaitsForTheFirstAsync((services, token) =>
            services.GetRequiredService<OrderLocks>().LockAsync([order.Id], token));

        Assert.True(waitedWhileHeld);
        Assert.True(gotItAfterRelease);
    }

    [Fact]
    public async Task AdministratorOperation_ThatSawAnOlderVersion_IsNotApplied_EvenWhenItWaitedForTheLock_KK4()
    {
        (Order order, _) = await SeedOrderAsync();
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope first = provider.CreateAsyncScope();
        await using AsyncServiceScope second = provider.CreateAsyncScope();
        AppDbContext firstDb = first.ServiceProvider.GetRequiredService<AppDbContext>();
        AppDbContext secondDb = second.ServiceProvider.GetRequiredService<AppDbContext>();

        await using IDbContextTransaction firstTransaction = await firstDb.Database.BeginTransactionAsync(Ct);
        Assert.False((await AdministratorOperationAsync(first.ServiceProvider, order.Id, shownVersion: 1, "Kızılay", Ct)).IsRejected);
        await firstDb.SaveChangesAsync(Ct);

        // The connection stays open, so the runner's transaction runs on the backend whose wait is watched.
        await secondDb.Database.OpenConnectionAsync(Ct);
        int pid = await BackendPidAsync(secondDb);
        Task<Outcome> waiting = second.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            inner => AdministratorOperationAsync(second.ServiceProvider, order.Id, shownVersion: 1, "Ulus", inner),
            Ct);
        await WaitUntilBlockedAsync(pid);
        Assert.False(waiting.IsCompleted);
        await firstTransaction.CommitAsync(Ct);

        Assert.True((await waiting.WaitAsync(TimeSpan.FromSeconds(30), Ct)).IsRejected);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal(2, saved.Version);
        Assert.Equal("Kızılay", saved.BillingDistrict);
    }

    [Fact]
    public async Task ItemCancellation_WhoseShownOpenQuantityChanged_IsNotApplied_KK4()
    {
        (Order order, OrderItem item) = await SeedOrderAsync();
        Assert.False((await RunAsync((services, token) => CustomerItemCancellationAsync(services, order.Id, item.Id, shownOpen: 1, token))).IsRejected);

        Outcome second = await RunAsync((services, token) => CustomerItemCancellationAsync(services, order.Id, item.Id, shownOpen: 1, token));

        Assert.True(second.IsRejected);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.Cancellations.CountAsync(c => c.OrderItemId == item.Id, Ct));
        Assert.Equal(2, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).Version);
    }

    // ---- KK5, KK6: the counters' conditional updates (05 7.3.3) ----

    [Fact]
    public async Task LastUnit_TwoConcurrentReservations_OnlyOneGetsIt_KK5()
    {
        Variant variant = await SeedVariantAsync(stock: 1);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope first = provider.CreateAsyncScope();
        await using AsyncServiceScope second = provider.CreateAsyncScope();
        AppDbContext firstDb = first.ServiceProvider.GetRequiredService<AppDbContext>();
        AppDbContext secondDb = second.ServiceProvider.GetRequiredService<AppDbContext>();

        await using IDbContextTransaction firstTransaction = await firstDb.Database.BeginTransactionAsync(Ct);
        Assert.True(await first.ServiceProvider.GetRequiredService<ICatalogModule>().ReserveAsync(Claim(variant.Id, 1), null, Ct));

        await using IDbContextTransaction secondTransaction = await secondDb.Database.BeginTransactionAsync(Ct);
        int pid = await BackendPidAsync(secondDb);
        Task<bool> waiting = second.ServiceProvider.GetRequiredService<ICatalogModule>().ReserveAsync(Claim(variant.Id, 1), null, Ct);
        await WaitUntilBlockedAsync(pid);
        await firstTransaction.CommitAsync(Ct);

        Assert.False(await waiting.WaitAsync(TimeSpan.FromSeconds(30), Ct));
        await secondTransaction.RollbackAsync(Ct);
        await AssertCountersAsync(variant.Id, stock: 1, reserved: 1);
    }

    [Fact]
    public async Task Reservation_ThatDoesNotFit_WritesNothing_KK5()
    {
        Variant variant = await SeedVariantAsync(stock: 2);
        Coupon coupon = await SeedCouponAsync(limit: 1);

        bool reserved = true;
        await RunAsync(async (services, token) =>
        {
            reserved = await services.GetRequiredService<ICatalogModule>().ReserveAsync(Claim(variant.Id, 3), null, token);
            return Outcome.Done;
        });
        Assert.False(reserved);
        await AssertCountersAsync(variant.Id, stock: 2, reserved: 0);

        StockClaim withCoupon = new([new VariantQuantity(variant.Id, 1)], coupon.Id);
        Outcome first = await RunAsync(async (services, token) =>
            await services.GetRequiredService<ICatalogModule>().ReserveAsync(withCoupon, null, token) ? Outcome.Done : Outcome.Refused);
        Outcome second = await RunAsync(async (services, token) =>
            await services.GetRequiredService<ICatalogModule>().ReserveAsync(withCoupon, null, token) ? Outcome.Done : Outcome.Refused);

        // The coupon's last right went to the first; the second's variant row, written before the coupon, falls with its refusal.
        Assert.False(first.IsRejected);
        Assert.True(second.IsRejected);
        await AssertCountersAsync(variant.Id, stock: 2, reserved: 1);
        await AssertCouponAsync(coupon.Id, used: 0, reserved: 1);
    }

    [Fact]
    public async Task OneTransaction_ReservingTwice_AsksAVariantLockAfterTheCouponLock_AndIsCaught_KK7()
    {
        Variant variant = await SeedVariantAsync(stock: 2);
        Coupon coupon = await SeedCouponAsync(limit: 5);
        StockClaim withCoupon = new([new VariantQuantity(variant.Id, 1)], coupon.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(async (services, token) =>
        {
            ICatalogModule catalog = services.GetRequiredService<ICatalogModule>();
            await catalog.ReserveAsync(withCoupon, null, token);
            await catalog.ReserveAsync(withCoupon, null, token);
            return Outcome.Done;
        }));

        await AssertCountersAsync(variant.Id, stock: 2, reserved: 0);
        await AssertCouponAsync(coupon.Id, used: 0, reserved: 0);
    }

    [Fact]
    public async Task PanelStockWrite_NeverGoesBelowTheReservedQuantity_AndRefusesAChangedShownValue_KK6()
    {
        Variant variant = await SeedVariantAsync(stock: 10);
        await RunAsync(async (services, token) =>
        {
            Assert.True(await services.GetRequiredService<ICatalogModule>().ReserveAsync(Claim(variant.Id, 4), null, token));
            return Outcome.Done;
        });

        StockQuantityWriteResult changed = await SaveStockAsync(variant.Id, shown: 9, value: 20);
        StockQuantityWriteResult below = await SaveStockAsync(variant.Id, shown: 10, value: 3);
        StockQuantityWriteResult atReserved = await SaveStockAsync(variant.Id, shown: 10, value: 4);

        Assert.True(changed.IsShownValueChanged);
        Assert.True(below.IsRejected);
        Assert.False(below.IsShownValueChanged);
        Assert.True(atReserved.IsWritten);
        await AssertCountersAsync(variant.Id, stock: 4, reserved: 4);
    }

    // ---- KK7, KK8: the lock order (05 7.3.5) ----

    [Fact]
    public async Task ReversedLockOrder_IsCaughtBeforeTheStatement_KK7()
    {
        (Order order, _) = await SeedOrderAsync();
        (Order other, _) = await SeedOrderAsync();
        Variant variant = await SeedVariantAsync(stock: 5);
        Guid[] ids = [.. new[] { order.Id, other.Id }.Order(Comparer<Guid>.Create(LockSequence.CompareKeys))];

        Func<IServiceProvider, CancellationToken, Task>[] reversed =
        [
            async (s, t) =>
            {
                await s.GetRequiredService<ICatalogModule>().ReserveAsync(Claim(variant.Id, 1), null, t);
                await s.GetRequiredService<OrderLocks>().LockAsync([order.Id], t);
            },
            async (s, t) =>
            {
                await s.GetRequiredService<OrderLocks>().LockAsync([order.Id], t);
                await s.GetRequiredService<AdvisoryLocks>().LockAsync(AdvisoryLockScope.Cart, EntityIds.New(), t);
            },
            async (s, t) =>
            {
                await s.GetRequiredService<OrderLocks>().LockAsync([ids[1]], t);
                await s.GetRequiredService<OrderLocks>().LockAsync([ids[0]], t);
            },
            async (s, t) =>
            {
                await s.GetRequiredService<AdvisoryLocks>().LockAsync(AdvisoryLockScope.CategoryTree, null, t);
                await s.GetRequiredService<OrderLocks>().LockAsync([order.Id], t);
            },
            async (s, t) =>
            {
                await s.GetRequiredService<AdvisoryLocks>().LockAsync(AdvisoryLockScope.AdminSet, null, t);
                await s.GetRequiredService<ICatalogModule>().ReserveAsync(Claim(variant.Id, 1), null, t);
            },
        ];

        foreach (Func<IServiceProvider, CancellationToken, Task> locks in reversed)
        {
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(async (services, token) =>
            {
                await locks(services, token);
                return Outcome.Done;
            }));
            Assert.Contains("7.3.5)", error.Message, StringComparison.Ordinal);
        }

        await AssertCountersAsync(variant.Id, stock: 5, reserved: 0);
    }

    [Fact]
    public async Task ConfirmationOrder_CartThenEarlierOrderThenItsFreeingAndTheNewReservation_IsAllowed_KK8()
    {
        (Order earlier, _) = await SeedOrderAsync();
        Variant variant = await SeedVariantAsync(stock: 3);
        await RunAsync(async (services, token) =>
        {
            Assert.True(await services.GetRequiredService<ICatalogModule>().ReserveAsync(Claim(variant.Id, 2), null, token));
            return Outcome.Done;
        });

        await RunAsync(async (services, token) =>
        {
            await services.GetRequiredService<AdvisoryLocks>().LockAsync(AdvisoryLockScope.Cart, EntityIds.New(), token);
            await services.GetRequiredService<OrderLocks>().LockAsync([earlier.Id], token);
            Assert.True(await services.GetRequiredService<ICatalogModule>().ReserveAsync(Claim(variant.Id, 3), Claim(variant.Id, 2), token));
            return Outcome.Done;
        });

        await AssertCountersAsync(variant.Id, stock: 3, reserved: 3);
    }

    [Fact]
    public async Task SameCart_TheSecondConfirmationWaitsForTheFirst_KK8()
    {
        Guid cartId = EntityIds.New();

        (bool waitedWhileHeld, bool gotItAfterRelease) = await SecondWaitsForTheFirstAsync((services, token) =>
            services.GetRequiredService<AdvisoryLocks>().LockAsync(AdvisoryLockScope.Cart, cartId, token));

        Assert.True(waitedWhileHeld);
        Assert.True(gotItAfterRelease);
    }

    [Fact]
    public async Task CategoryTreeLock_TheSecondTransactionWaitsForTheFirst()
    {
        (bool waitedWhileHeld, bool gotItAfterRelease) = await SecondWaitsForTheFirstAsync((services, token) =>
            services.GetRequiredService<AdvisoryLocks>().LockAsync(AdvisoryLockScope.CategoryTree, null, token));

        Assert.True(waitedWhileHeld);
        Assert.True(gotItAfterRelease);
    }

    [Fact]
    public async Task LockTakers_OutsideATransaction_Throw()
    {
        Variant variant = await SeedVariantAsync(stock: 5);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;

        await Assert.ThrowsAsync<InvalidOperationException>(() => services.GetRequiredService<AdvisoryLocks>().LockAsync(AdvisoryLockScope.Cart, EntityIds.New(), Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => services.GetRequiredService<OrderLocks>().LockAsync([EntityIds.New()], Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => services.GetRequiredService<ICatalogModule>().ReserveAsync(Claim(variant.Id, 1), null, Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => services.GetRequiredService<ICatalogModule>().SaveStockQuantityAsync(variant.Id, 5, 6, Ct));
        await AssertCountersAsync(variant.Id, stock: 5, reserved: 0);
    }

    [Fact]
    public async Task PostgresUuidOrder_IsTheOrderOfCompareKeys()
    {
        Guid[] keys = [.. Enumerable.Range(0, 64).Select(_ => EntityIds.New()), new("0fffffff-ffff-ffff-ffff-ffffffffffff"), new("f0000000-0000-0000-0000-000000000000")];
        await using AppDbContext context = NewContext();

        List<Guid> byDatabase = await context.Database.SqlQuery<Guid>($"SELECT k AS \"Value\" FROM unnest({keys}) AS k ORDER BY k").ToListAsync(Ct);

        Assert.Equal(byDatabase, keys.Order(Comparer<Guid>.Create(LockSequence.CompareKeys)));
    }

    // ---- Stand-ins of later operations (09 10.2 item 3) ----

    /// <summary>The administrator's order operation: the lock, the load after it, the shown version compared, then the write and the raise.</summary>
    private static async Task<Outcome> AdministratorOperationAsync(IServiceProvider services, Guid orderId, int shownVersion, string district, CancellationToken ct)
    {
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        await services.GetRequiredService<OrderLocks>().LockAsync([orderId], ct);
        Order order = await db.Orders.SingleAsync(o => o.Id == orderId, ct);
        if (order.Version != shownVersion)
        {
            return Outcome.Refused;
        }

        order.BillingDistrict = district;
        order.BumpVersion();
        return Outcome.Done;
    }

    /// <summary>The customer's item cancellation: the lock, the open quantity as it is now against the shown one, then the record and the raise.</summary>
    private static async Task<Outcome> CustomerItemCancellationAsync(IServiceProvider services, Guid orderId, Guid itemId, int shownOpen, CancellationToken ct)
    {
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        await services.GetRequiredService<OrderLocks>().LockAsync([orderId], ct);
        Order order = await db.Orders.SingleAsync(o => o.Id == orderId, ct);
        OrderItem item = await db.OrderItems.SingleAsync(i => i.Id == itemId, ct);
        int cancelled = await db.Cancellations.Where(c => c.OrderItemId == itemId).SumAsync(c => c.Quantity, ct);
        if (!ShownOpenQuantities.Match(new Dictionary<Guid, int> { [itemId] = shownOpen }, new Dictionary<Guid, int> { [itemId] = item.Quantity - cancelled }))
        {
            return Outcome.Refused;
        }

        db.Cancellations.Add(Cancellation.ByBuyer(itemId, shownOpen, Amounts, null, Now));
        order.BumpVersion();
        return Outcome.Done;
    }

    // ---- Helpers ----

    private static StockClaim Claim(Guid variantId, int quantity) => new([new VariantQuantity(variantId, quantity)], null);

    /// <summary>Takes the lock in two transactions; returns whether the second waited while the first held it and got it once the first committed.</summary>
    private async Task<(bool WaitedWhileHeld, bool GotItAfterRelease)> SecondWaitsForTheFirstAsync(Func<IServiceProvider, CancellationToken, Task> takeLock)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope first = provider.CreateAsyncScope();
        await using AsyncServiceScope second = provider.CreateAsyncScope();
        AppDbContext firstDb = first.ServiceProvider.GetRequiredService<AppDbContext>();
        AppDbContext secondDb = second.ServiceProvider.GetRequiredService<AppDbContext>();

        await using IDbContextTransaction firstTransaction = await firstDb.Database.BeginTransactionAsync(Ct);
        await takeLock(first.ServiceProvider, Ct);
        await using IDbContextTransaction secondTransaction = await secondDb.Database.BeginTransactionAsync(Ct);
        int pid = await BackendPidAsync(secondDb);
        Task waiting = takeLock(second.ServiceProvider, Ct);
        await WaitUntilBlockedAsync(pid);

        bool waitedWhileHeld = !waiting.IsCompleted;
        await firstTransaction.CommitAsync(Ct);
        await waiting.WaitAsync(TimeSpan.FromSeconds(30), Ct);
        bool gotItAfterRelease = waiting.IsCompletedSuccessfully;
        await secondTransaction.CommitAsync(Ct);
        return (waitedWhileHeld, gotItAfterRelease);
    }

    private static async Task<int> BackendPidAsync(AppDbContext context)
    {
        return (await context.Database.SqlQuery<int>($"SELECT pg_backend_pid() AS \"Value\"").ToListAsync(Ct)).Single();
    }

    /// <summary>Waits until the backend waits on a lock (pg_stat_activity), at most about ten seconds.</summary>
    private async Task WaitUntilBlockedAsync(int pid)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        for (int attempt = 0; attempt < 400; attempt++)
        {
            await using NpgsqlCommand command = new("SELECT wait_event_type FROM pg_stat_activity WHERE pid = @pid", connection);
            command.Parameters.AddWithValue("pid", pid);
            if (await command.ExecuteScalarAsync(Ct) is string type && type == "Lock")
            {
                return;
            }

            await Task.Delay(25, Ct);
        }

        throw new Xunit.Sdk.XunitException($"Backend {pid} never waited on a lock.");
    }

    private async Task<T> RunAsync<T>(Func<IServiceProvider, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(scope.ServiceProvider, token), Ct);
    }

    private Task<StockQuantityWriteResult> SaveStockAsync(Guid variantId, int? shown, int? value)
    {
        return RunAsync((services, token) => services.GetRequiredService<ICatalogModule>().SaveStockQuantityAsync(variantId, shown, value, token));
    }

    private async Task AssertCountersAsync(Guid variantId, int? stock, int reserved)
    {
        await using AppDbContext reader = NewContext();
        Variant variant = await reader.Variants.SingleAsync(v => v.Id == variantId, Ct);
        Assert.Equal((stock, reserved), (variant.StockQuantity, variant.ReservedQuantity));
    }

    private async Task AssertCouponAsync(Guid couponId, int used, int reserved)
    {
        await using AppDbContext reader = NewContext();
        Coupon coupon = await reader.Coupons.SingleAsync(c => c.Id == couponId, Ct);
        Assert.Equal((used, reserved), (coupon.UsedCount, coupon.ReservedCount));
    }

    private async Task<Product> SeedProductAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Kapı", $"kapi-{n}", ProductType.Physical, 20m, Now);
        await SaveAsync(c => c.Products.Add(product));
        return product;
    }

    private async Task<Variant> SeedVariantAsync(int? stock)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Kapı", $"kapi-{n}", ProductType.Physical, 20m, Now);
        Variant variant = Variant.Create(product.Id, $"KAPI-{n}", 100m, Now);
        variant.StockQuantity = stock;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        return variant;
    }

    private async Task<Coupon> SeedCouponAsync(int limit)
    {
        int n = Interlocked.Increment(ref _counter);
        Coupon coupon = Coupon.CreatePercentage($"YAZ{n}", 10m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), limit, Now);
        await SaveAsync(c => c.Coupons.Add(coupon));
        return coupon;
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

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();

        // What the Catalog module's image upload stands on (the application registers them in Program.cs; the stock tests never upload).
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<Shopfolio.Infrastructure.InstallationSettings.StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddOrderingModule();
        return services.BuildServiceProvider();
    }

    private AppDbContext NewContext(CommandRecorder? recorder = null)
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        if (recorder is not null)
        {
            builder.AddInterceptors(recorder);
        }

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
        await ExecuteCountAsync(sql);
    }

    private async Task<int> ExecuteCountAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return await command.ExecuteNonQueryAsync(Ct);
    }

    /// <summary>A result of the stand-in operations: done or refused.</summary>
    private sealed class Outcome : IOperationResult
    {
        private Outcome(bool isRejected)
        {
            IsRejected = isRejected;
        }

        public static Outcome Done { get; } = new(false);

        public static Outcome Refused { get; } = new(true);

        public bool IsRejected { get; }
    }

    /// <summary>Records the text of every command the context sends.</summary>
    private sealed class CommandRecorder : Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        public List<string> Commands { get; } = [];

        public override ValueTask<Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader>> ReaderExecutingAsync(
            System.Data.Common.DbCommand command,
            Microsoft.EntityFrameworkCore.Diagnostics.CommandEventData eventData,
            Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Commands.Add(command.CommandText.Trim());
            return ValueTask.FromResult(result);
        }

        public override ValueTask<Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int>> NonQueryExecutingAsync(
            System.Data.Common.DbCommand command,
            Microsoft.EntityFrameworkCore.Diagnostics.CommandEventData eventData,
            Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Commands.Add(command.CommandText.Trim());
            return ValueTask.FromResult(result);
        }
    }
}
