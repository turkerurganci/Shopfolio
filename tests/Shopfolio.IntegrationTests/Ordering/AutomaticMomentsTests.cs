using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The moments that run by themselves and the site's outage on a real PostgreSQL and a fake clock (T327 criteria 2-7, KK2-KK7; 03 4.2.8, 4.2.11,
/// 4.2.12; 05 5.4.3, MK-44): the return's first heartbeat records the outage and defers the transfer order whose window ran out in the gap; the scan
/// then runs the missed jobs in the order they were missed and neither the deferred cancellation nor the ended window's reminder runs; the deferred
/// order keeps its reservation, its page shows the new last day and no e-mail goes, and the cancellation runs at the deferred moment; the refund
/// IBAN's deletion day deletes the IBAN of a withdrawal whose goods did not arrive and keeps it while another refund uses it.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class AutomaticMomentsTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _clock = new(Start);
    private readonly List<string> _processed = [];
    private ServiceProvider _provider = null!;
    private int _counter;

    public AutomaticMomentsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private DateTimeOffset Now => _clock.GetUtcNow();

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE refunds, returns, withdrawals, cancellations, refund_ibans, disposal_records, stock_reservations, dispatch_records, jobs, payment_records, "
            + "internal_notes, transition_records, order_items, orders, coupons, variants, products, heartbeat_records, outage_records, legal_text_versions, provinces CASCADE");
        _provider = BuildProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK3, KK7: the return runs the missed jobs in their order; the deferred cancellation and the ended window's reminder do not run ----

    [Fact]
    public async Task Return_RecordsTheOutage_RunsTheMissedJobsInTheOrderTheyWereMissed_AndHoldsBackTheDeferredCancellationAndTheEndedReminder_KK3_KK7()
    {
        // The site was down for three hours (05 5.4.1: the last beat is three hours old).
        await SeedHeartbeatAsync(Start.AddHours(-3));
        Order deferred = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddHours(-2), reminderAt: Start.AddMinutes(-150));
        Order running = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddDays(2), reminderAt: Start.AddMinutes(-60));
        Order card = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-170));
        Order withdrawn = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddDays(5), orderStatus: OrderStatus.Delivered, paymentStatus: PaymentStatus.Paid);
        RefundIban iban = await SeedIbanWithWithdrawalAsync(withdrawn.Id, Start.AddMinutes(-120));

        await ReturnAsync();
        await _provider.GetRequiredService<SchedulerScan>().RunAsync(Ct);

        // KK7: the outage is the heartbeat's measure (05 MK-44): one record from the last beat to the return's first one.
        await using AppDbContext reader = NewContext();
        OutageRecord outage = await reader.OutageRecords.SingleAsync(Ct);
        Assert.Equal((Start.AddHours(-3), Start), (outage.StartedAt, outage.EndedAt));

        // The missed jobs ran by their missed moments: the card window (-170 min), the IBAN's day (-120 min), the running window's reminder (-60 min).
        Assert.Equal(["CardPaymentExpiry", "refund-iban-deletion-day", "TransferPaymentReminder"], _processed);
        Assert.Equal(1, await reader.Jobs.CountAsync(j => j.RelatedRecordId == card.Id && j.JobType == JobType.FinalPaymentQuery, Ct));
        Assert.False(await reader.RefundIbans.AnyAsync(i => i.Id == iban.Id, Ct));
        Assert.Equal([DispatchType.B3], await reader.DispatchRecords.Where(d => d.OrderId == running.Id).Select(d => d.DispatchType).ToListAsync(Ct));

        // The deferred cancellation and the ended window's reminder did not run (03 4.2.11: the two exceptions).
        Order after = await reader.Orders.SingleAsync(o => o.Id == deferred.Id, Ct);
        Assert.Equal((OrderStatus.Placed, PaymentStatus.Pending), (after.OrderStatus, after.PaymentStatus));
        Assert.Equal(TransferSchedule.DeferredWindowEnd(Start, BusinessCalendar.Product), after.PaymentWindowEndsAt);
        Assert.False(await reader.DispatchRecords.AnyAsync(d => d.OrderId == deferred.Id, Ct));
    }

    // ---- KK4, KK5: the deferral keeps the reservation, shows the new last day, sends nothing; the cancellation runs at the deferred moment ----

    [Fact]
    public async Task DeferredCancellation_KeepsTheReservation_ShowsTheNewLastDay_SendsNoEmail_AndRunsAtTheDeferredMoment_KK4_KK5()
    {
        await SeedHeartbeatAsync(Start.AddHours(-3));
        Order order = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddHours(-1), reminderAt: Start.AddHours(-2));
        Guid variantId = await SeedReservationAsync(order.Id, units: 2);

        await ReturnAsync();
        await _provider.GetRequiredService<SchedulerScan>().RunAsync(Ct);

        DateTimeOffset newEnd = TransferSchedule.DeferredWindowEnd(Start, BusinessCalendar.Product);
        Assert.Equal(1, await CountReservationsAsync(order.Id));
        Assert.Equal(0, await CountDispatchesAsync(order.Id));
        OrderPageSummary page = (await WithOrderingAsync(m => m.GetOrderPageSummaryAsync(order.Id, Ct)))!;
        Assert.Equal(newEnd, page.TransferPaymentEndsAt);

        // A minute before the deferred moment, with fresh heartbeats, nothing happens.
        await BeatAtAsync(newEnd.AddMinutes(-1));
        await _provider.GetRequiredService<SchedulerScan>().RunAsync(Ct);
        Assert.Equal((OrderStatus.Placed, 1), ((await ReadOrderAsync(order.Id)).OrderStatus, await CountReservationsAsync(order.Id)));

        // At the deferred moment, the payment not marked, the cancellation runs with its own notice (B-7) and the reservation is released.
        await BeatAtAsync(newEnd);
        await _provider.GetRequiredService<SchedulerScan>().RunAsync(Ct);
        Order cancelled = await ReadOrderAsync(order.Id);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Failed), (cancelled.OrderStatus, cancelled.PaymentStatus));
        Assert.Equal(0, await CountReservationsAsync(order.Id));
        await using AppDbContext reader = NewContext();
        Assert.Equal([DispatchType.B7], await reader.DispatchRecords.Where(d => d.OrderId == order.Id).Select(d => d.DispatchType).ToListAsync(Ct));
        Assert.Equal(0, (await reader.Variants.SingleAsync(v => v.Id == variantId, Ct)).ReservedQuantity);
        Assert.Null((await WithOrderingAsync(m => m.GetOrderPageSummaryAsync(order.Id, Ct)))!.TransferPaymentEndsAt);
    }

    [Fact]
    public async Task InTheDeferredGap_TheFirmCanStillMarkThePayment_AndTheDeferredMomentThenCancelsNothing_T353_KK7()
    {
        await SeedHeartbeatAsync(Start.AddHours(-3));
        Order order = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddHours(-1), reminderAt: Start.AddHours(-2));
        await SeedReservationAsync(order.Id, units: 1);
        await ReturnAsync();
        await _provider.GetRequiredService<SchedulerScan>().RunAsync(Ct);
        int version = (await ReadOrderAsync(order.Id)).Version;
        Admin admin = Admin.Create("Yönetici", $"yonetici-{Guid.NewGuid():N}@example.com", "digest-placeholder", Start);
        await using (AppDbContext db = NewContext())
        {
            db.Admins.Add(admin);
            await db.SaveChangesAsync(Ct);
        }

        // 02 3.21.8 (K-667): the window ran out in the outage, its cancellation is put off, and the "paid" mark is taken in the meantime.
        await using (AsyncServiceScope scope = _provider.CreateAsyncScope())
        {
            PaymentTransitionResult mark = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
                cancel => scope.ServiceProvider.GetRequiredService<IOrderTransitions>().MarkTransferPaidAsync(new TransferPaidMark(order.Id, admin.Id, admin.Name, version, true), cancel),
                Ct);
            Assert.False(mark.IsRejected);
        }

        await BeatAtAsync(TransferSchedule.DeferredWindowEnd(Start, BusinessCalendar.Product));
        await _provider.GetRequiredService<SchedulerScan>().RunAsync(Ct);

        Order paid = await ReadOrderAsync(order.Id);
        Assert.Equal(PaymentStatus.Paid, paid.PaymentStatus);
        Assert.NotEqual(OrderStatus.Cancelled, paid.OrderStatus);
    }

    // ---- KK6: Z-38, the refund IBAN's deletion day ----

    [Fact]
    public async Task DeletionDay_DeletesTheIbanOfAWithdrawalWhoseGoodsDidNotArrive_KeepsTheWithdrawal_AndWritesTheDisposalRecord_KK6()
    {
        Order order = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddDays(5), orderStatus: OrderStatus.Delivered, paymentStatus: PaymentStatus.Paid);
        RefundIban iban = await SeedIbanWithWithdrawalAsync(order.Id, Start.AddMinutes(-1));
        RefundIban notYet = await SeedIbanWithWithdrawalAsync(order.Id, Start.AddMinutes(1));

        Assert.Equal([iban.Id], (await FindDueAsync()).Select(d => d.Id));
        int versionBefore = (await ReadOrderAsync(order.Id)).Version;
        await ProcessAsync(iban.Id);

        await using AppDbContext reader = NewContext();
        Assert.Equal([notYet.Id], await reader.RefundIbans.Select(i => i.Id).ToListAsync(Ct));
        Withdrawal withdrawal = await reader.Withdrawals.SingleAsync(w => w.RefundIbanId == null, Ct);
        Assert.Equal(1, withdrawal.Quantity);
        Assert.Equal(2, await reader.Withdrawals.CountAsync(Ct));

        // The item is not closed and the order's status stays; the order's version is raised once for the record it lost (06 2.6.4).
        Order after = await ReadOrderAsync(order.Id);
        Assert.Equal((OrderStatus.Delivered, versionBefore + 1), (after.OrderStatus, after.Version));
        Assert.False(await reader.Cancellations.AnyAsync(Ct));
        Assert.False(await reader.NotReturnedClosures.AnyAsync(Ct));
        DisposalRecord record = await reader.DisposalRecords.SingleAsync(Ct);
        Assert.Equal((DisposalRecordType.RefundIban, 1), (record.DisposalRecordType, record.Count!.Value));
    }

    [Fact]
    public async Task DeletionDay_KeepsTheIbanWhileAnotherRefundUsesIt_AndEmptiesItsDay_ButDeletesItWhenThatRefundWasSent_KK6()
    {
        Order order = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddDays(5), orderStatus: OrderStatus.Delivered, paymentStatus: PaymentStatus.Paid);
        RefundIban kept = await SeedIbanWithWithdrawalAsync(order.Id, Start.AddMinutes(-2));
        await SeedCancellationRefundAsync(order.Id, kept.Id, sent: false);
        RefundIban deleted = await SeedIbanWithWithdrawalAsync(order.Id, Start.AddMinutes(-1));
        await SeedCancellationRefundAsync(order.Id, deleted.Id, sent: true);

        await ProcessAsync(kept.Id);
        await ProcessAsync(deleted.Id);

        await using AppDbContext reader = NewContext();
        RefundIban stays = await reader.RefundIbans.SingleAsync(Ct);
        Assert.Equal((kept.Id, (DateTimeOffset?)null), (stays.Id, stays.ScheduledDeletionAt));
        Assert.Empty(await FindDueAsync());
        Assert.Equal(1, await reader.DisposalRecords.CountAsync(Ct));
    }

    // ---- T412 KK3 (D-910; 03 4.2.8): the deletion day counts every refund that may still be paid to the IBAN ----

    [Fact]
    public async Task DeletionDay_KeepsTheIban_ForAWithdrawalBeforeTheDelivery_AServiceWithdrawal_AndARequestsRefund_WhileTheirRefundsArePending_T412_KK3()
    {
        (RefundIban beforeDelivery, RefundIban service, RefundIban request) = await SeedThreeSubjectsAsync(settled: false);

        await ProcessAsync(beforeDelivery.Id);
        await ProcessAsync(service.Id);
        await ProcessAsync(request.Id);

        // Each IBAN stays for the refund that still uses it and its day is emptied: the deletion that comes with that refund is the refund line's (03 4.2.8).
        await using AppDbContext reader = NewContext();
        List<RefundIban> kept = await reader.RefundIbans.OrderBy(i => i.Id).ToListAsync(Ct);
        Assert.Equal(new[] { beforeDelivery.Id, service.Id, request.Id }.Order(), kept.Select(i => i.Id));
        Assert.All(kept, i => Assert.Null(i.ScheduledDeletionAt));
        Assert.False(await reader.DisposalRecords.AnyAsync(Ct));
    }

    [Fact]
    public async Task DeletionDay_DeletesTheIban_OnceTheRefundsOfThoseThreeSubjectsWereSentOrVoided_T412_KK3()
    {
        (RefundIban beforeDelivery, RefundIban service, RefundIban request) = await SeedThreeSubjectsAsync(settled: true);

        await ProcessAsync(beforeDelivery.Id);
        await ProcessAsync(service.Id);
        await ProcessAsync(request.Id);

        await using AppDbContext reader = NewContext();
        Assert.False(await reader.RefundIbans.AnyAsync(Ct));
        Assert.Equal(3, await reader.DisposalRecords.CountAsync(r => r.DisposalRecordType == DisposalRecordType.RefundIban, Ct));
    }

    /// <summary>
    /// Three orders, each with an IBAN whose deletion day came for a withdrawal after the delivery whose goods did not arrive, and one more subject of the IBAN:
    /// a withdrawal before the delivery (its refund is born with the declaration; settled: sent), a service withdrawal (settled: voided), and an IBAN request
    /// fulfilled on a failed card refund that names the refund (settled: sent). Unsettled, the first has no refund row yet and the other two are not sent.
    /// </summary>
    private async Task<(RefundIban BeforeDelivery, RefundIban Service, RefundIban Request)> SeedThreeSubjectsAsync(bool settled)
    {
        Order first = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddDays(5), orderStatus: OrderStatus.Delivered, paymentStatus: PaymentStatus.Paid);
        RefundIban beforeDelivery = await SeedIbanWithWithdrawalAsync(first.Id, Start.AddMinutes(-3));
        Guid undelivered = await AddItemAsync(first.Id, ProductType.Physical);
        Guid declared = await SeedWithdrawalAsync(first.Id, undelivered, beforeDelivery.Id);
        if (settled)
        {
            await SeedWithdrawalRefundAsync(first.Id, undelivered, declared, PaymentMethod.BankTransfer, sent: true, voided: false);
        }

        Order second = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddDays(5), orderStatus: OrderStatus.Delivered, paymentStatus: PaymentStatus.Paid);
        RefundIban service = await SeedIbanWithWithdrawalAsync(second.Id, Start.AddMinutes(-2));
        Guid serviceItem = await AddItemAsync(second.Id, ProductType.Service);
        Guid withdrawn = await SeedWithdrawalAsync(second.Id, serviceItem, service.Id);
        await SeedWithdrawalRefundAsync(second.Id, serviceItem, withdrawn, PaymentMethod.Card, sent: false, voided: settled);

        Order third = await SeedOrderAsync(PaymentMethod.BankTransfer, Start.AddDays(5), orderStatus: OrderStatus.Delivered, paymentStatus: PaymentStatus.Paid);
        RefundIban request = await SeedIbanWithWithdrawalAsync(third.Id, Start.AddMinutes(-1));
        await SeedRequestRefundAsync(third.Id, request.Id, sent: settled);
        return (beforeDelivery, service, request);
    }

    /// <summary>A further item of the order, not delivered.</summary>
    private async Task<Guid> AddItemAsync(Guid orderId, ProductType productType)
    {
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.SingleAsync(o => o.Id == orderId, Ct);
        OrderItem item = OrderItem.Create(orderId, null, null, productType, "Ürün 2", 1, 100m, 20m, 16.67m, Start.AddSeconds(1));
        item.FulfilmentPeriod = productType == ProductType.Service ? 7 : null;
        db.OrderItems.Add(item);
        await SaveWithVersionAsync(db, order);
        return item.Id;
    }

    /// <summary>A withdrawal of one unit of the item linked to the IBAN, declared before any delivery of the item.</summary>
    private async Task<Guid> SeedWithdrawalAsync(Guid orderId, Guid itemId, Guid ibanId)
    {
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.SingleAsync(o => o.Id == orderId, Ct);
        Withdrawal withdrawal = Withdrawal.ByBuyer(itemId, 1, new DateOnly(2026, 10, 15), null, new ItemAmounts(100m, 10m, 15m), ibanId, Start.AddDays(-20));
        db.Withdrawals.Add(withdrawal);
        await SaveWithVersionAsync(db, order);
        return withdrawal.Id;
    }

    /// <summary>The declaration's refund of a withdrawal (no receipt carries it), sent, voided or still waiting.</summary>
    private async Task SeedWithdrawalRefundAsync(Guid orderId, Guid itemId, Guid withdrawalId, PaymentMethod method, bool sent, bool voided)
    {
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.SingleAsync(o => o.Id == orderId, Ct);
        Refund refund = Refund.BySystem(orderId, RefundType.Withdrawal, method, 100m, Start.AddDays(-20));
        (refund.WithdrawalId, refund.OrderItemId, refund.Quantity) = (withdrawalId, itemId, 1);
        refund.SentAt = sent ? Start.AddDays(-1) : null;
        refund.VoidedAt = voided ? Start.AddDays(-1) : null;
        db.Refunds.Add(refund);
        await SaveWithVersionAsync(db, order);
    }

    /// <summary>A failed card refund of a cancellation whose item's IBAN request was fulfilled with the IBAN: the request names the refund, sent or not.</summary>
    private async Task SeedRequestRefundAsync(Guid orderId, Guid ibanId, bool sent)
    {
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.SingleAsync(o => o.Id == orderId, Ct);
        Guid itemId = await db.OrderItems.Where(i => i.OrderId == orderId).Select(i => i.Id).FirstAsync(Ct);
        Cancellation cancellation = Cancellation.ByBuyer(itemId, 1, new ItemAmounts(100m, 10m, 15m), null, Start.AddDays(-20));
        db.Cancellations.Add(cancellation);
        await SaveWithVersionAsync(db, order);
        Refund refund = Refund.BySystem(orderId, RefundType.Cancellation, PaymentMethod.Card, 50m, Start.AddDays(-20));
        (refund.CancellationId, refund.OrderItemId, refund.Quantity) = (cancellation.Id, itemId, 1);
        refund.SentAt = sent ? Start.AddDays(-1) : null;
        db.Refunds.Add(refund);
        await SaveWithVersionAsync(db, order);
        // The administrator who opened the request has since been removed: the link is emptied (06 7.2), the request stays.
        RefundIbanRequest request = RefundIbanRequest.ForItem(itemId, RefundIbanRequestReason.CardRefundFailed, refund.Id, Guid.Empty, "Yönetici", Start.AddDays(-10));
        (request.AdminId, request.FulfilledAt, request.RefundIbanId) = (null, Start.AddDays(-9), ibanId);
        db.RefundIbanRequests.Add(request);
        await SaveWithVersionAsync(db, order);
    }

    // ---- helpers ----

    private async Task ReturnAsync()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        Assert.True(await OutageRecovery.ApplyAsync(scope.ServiceProvider, Now, Ct));
    }

    /// <summary>Moves the clock and writes a heartbeat of that moment directly: the beats every thirty seconds in between are not an outage.</summary>
    private async Task BeatAtAsync(DateTimeOffset at)
    {
        _clock.SetUtcNow(at);
        await ExecuteAsync($"UPDATE heartbeat_records SET last_heartbeat_at = '{at:O}'");
    }

    private async Task<IReadOnlyList<DueRecord>> FindDueAsync()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await new RefundIbanDeletionDay().FindDueAsync(scope.ServiceProvider, Now, Ct);
    }

    private async Task ProcessAsync(Guid ibanId)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        await new RefundIbanDeletionDay().ProcessAsync(scope.ServiceProvider, ibanId, Ct);
    }

    private async Task<T> WithOrderingAsync<T>(Func<IOrderingModule, Task<T>> work)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await work(scope.ServiceProvider.GetRequiredService<IOrderingModule>());
    }

    private async Task<int> CountReservationsAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.StockReservations.CountAsync(r => r.OrderId == orderId, Ct);
    }

    private async Task<int> CountDispatchesAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.CountAsync(d => d.OrderId == orderId, Ct);
    }

    private async Task<Order> ReadOrderAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
    }

    private async Task SeedHeartbeatAsync(DateTimeOffset lastBeat)
    {
        await using AppDbContext db = NewContext();
        HeartbeatRecord heartbeat = HeartbeatRecord.Create(lastBeat);
        heartbeat.LastHeartbeatAt = lastBeat;
        heartbeat.NightlyJobLastSucceededAt = Start;
        heartbeat.NightlyJobLastAttemptedAt = Start;
        db.HeartbeatRecords.Add(heartbeat);
        await db.SaveChangesAsync(Ct);
    }

    /// <summary>A refund IBAN with its deletion day, used by a withdrawal after delivery whose goods did not arrive (no receipt).</summary>
    private async Task<RefundIban> SeedIbanWithWithdrawalAsync(Guid orderId, DateTimeOffset deletionAt)
    {
        RefundIban iban = RefundIban.Create(orderId, "TR330006100519786457841326", Start.AddDays(-20));
        iban.ScheduledDeletionAt = deletionAt;
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.SingleAsync(o => o.Id == orderId, Ct);
        OrderItem item = await db.OrderItems.Where(i => i.OrderId == orderId).OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).FirstAsync(Ct);
        Guid itemId = item.Id;

        // Delivered before the declaration: the withdrawal's refund waits for the goods (Z-16), so it has no subject until a receipt (Z-38's case).
        (item.DeliveryDate, item.DeliveryMarkedAt) = (new DateOnly(2026, 10, 10), Start.AddDays(-30));
        db.RefundIbans.Add(iban);
        await SaveWithVersionAsync(db, order);
        db.Withdrawals.Add(Withdrawal.ByBuyer(itemId, 1, new DateOnly(2026, 10, 15), null, new ItemAmounts(100m, 10m, 15m), iban.Id, Start.AddDays(-20)));
        await SaveWithVersionAsync(db, order);
        return iban;
    }

    /// <summary>An order's record is written with the order's version raised by one (06 2.6.4): the seed follows the rule the guard enforces.</summary>
    private static async Task SaveWithVersionAsync(AppDbContext db, Order order)
    {
        order.BumpVersion();
        await db.SaveChangesAsync(Ct);
    }

    /// <summary>A cancellation whose refund goes to the IBAN, with its refund sent or not yet sent.</summary>
    private async Task SeedCancellationRefundAsync(Guid orderId, Guid ibanId, bool sent)
    {
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.SingleAsync(o => o.Id == orderId, Ct);
        Guid itemId = await db.OrderItems.Where(i => i.OrderId == orderId).Select(i => i.Id).FirstAsync(Ct);
        Cancellation cancellation = Cancellation.ByBuyer(itemId, 1, new ItemAmounts(100m, 10m, 15m), ibanId, Start.AddDays(-20));
        db.Cancellations.Add(cancellation);
        await SaveWithVersionAsync(db, order);
        Refund refund = Refund.BySystem(orderId, RefundType.Cancellation, PaymentMethod.Card, 50m, Start.AddDays(-20));
        (refund.CancellationId, refund.OrderItemId, refund.Quantity) = (cancellation.Id, itemId, 1);
        refund.SentAt = sent ? Start.AddDays(-1) : null;
        db.Refunds.Add(refund);
        await SaveWithVersionAsync(db, order);
    }

    /// <summary>A variant with five in stock and the order's reservation of <paramref name="units"/> units.</summary>
    private async Task<Guid> SeedReservationAsync(Guid orderId, int units)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Kapı", $"kapi-t327-{n}", ProductType.Physical, 20m, Start).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, $"KAPI-T327-{n}", 100m, Start, 100);
        variant.StockQuantity = 5;
        await using (AppDbContext db = NewContext())
        {
            db.Products.Add(product);
            db.Variants.Add(variant);
            db.StockReservations.Add(StockReservation.ForVariant(orderId, variant.Id, units, Start));
            await db.SaveChangesAsync(Ct);
        }

        await ExecuteAsync($"UPDATE variants SET reserved_quantity = {units} WHERE id = '{variant.Id}'");
        return variant.Id;
    }

    private async Task<Order> SeedOrderAsync(
        PaymentMethod method,
        DateTimeOffset windowEndsAt,
        OrderStatus orderStatus = OrderStatus.Placed,
        PaymentStatus paymentStatus = PaymentStatus.Pending,
        DateTimeOffset? reminderAt = null)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n + 327000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, true, "Ali@Example.com", 120m, 0m, windowEndsAt, Start.AddDays(-1));
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;
        order.PaymentReminderAt = reminderAt;
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
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Ürün", 2, 100m, 20m, 16.67m, Start);
        await using (AppDbContext db = NewContext())
        {
            db.Provinces.Add(province);
            db.LegalTextVersions.AddRange(form, contract, notice);
            db.Orders.Add(order);
            db.OrderItems.Add(item);
            await db.SaveChangesAsync(Ct);
        }

        if (orderStatus != OrderStatus.Placed || paymentStatus != PaymentStatus.Pending)
        {
            await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        }

        return order;
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddSingleton(NpgsqlDataSource.Create(_collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton<TimeProvider>(_clock);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<Shopfolio.Infrastructure.InstallationSettings.StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddNoticeDependencies();
        services.AddOrderingModule();
        services.AddPaymentModule();
        services.AddSingleton<NightlyJobRunner>();
        services.AddSingleton<SchedulerScan>();

        // Every due-record source is wrapped so the test sees the order the scan processed the records in (03 4.2.11: in the order they were missed).
        foreach (ServiceDescriptor source in services.Where(d => d.ServiceType == typeof(IDueRecordSource)).ToList())
        {
            Type inner = source.ImplementationType!;
            services.Remove(source);
            services.AddSingleton<IDueRecordSource>(provider => new RecordingSource((IDueRecordSource)ActivatorUtilities.CreateInstance(provider, inner), _processed));
        }

        return services.BuildServiceProvider();
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

    /// <summary>A source that writes its name to the list each time one of its records is processed, then lets the real source process it.</summary>
    private sealed class RecordingSource : IDueRecordSource
    {
        private readonly IDueRecordSource _inner;
        private readonly List<string> _processed;

        public RecordingSource(IDueRecordSource inner, List<string> processed)
        {
            _inner = inner;
            _processed = processed;
        }

        public string Name => _inner.Name;

        public bool WaitsForFreshHeartbeat => _inner.WaitsForFreshHeartbeat;

        public Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct) => _inner.FindDueAsync(services, now, ct);

        public Task ProcessAsync(IServiceProvider services, Guid recordId, CancellationToken ct)
        {
            _processed.Add(_inner.Name);
            return _inner.ProcessAsync(services, recordId, ct);
        }
    }
}
