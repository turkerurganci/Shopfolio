using System.Net;

using Microsoft.EntityFrameworkCore;
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
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The two axes' tie on a real PostgreSQL (T318 KK1-KK8; 03 1.4.1-1.4.4; 02 5.6, 4.3; 06 4.14): Ö1's simultaneous transitions by type and method, the
/// unpaid cancellation's freed reservation, the transfer mark's correction that takes both axes back, the refused corrections, the closure rule's S11
/// and its quantities, and the nine moments' effect on the stock, the service capacity and the coupon right through <see cref="OrderCounters"/>. Every
/// operation runs in the transaction runner like a page would call it; what it wrote is read back from the tables.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class AxesTieTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = DateOnly.FromDateTime(Start.UtcDateTime);

    private static readonly ItemAmounts Amounts = new(100m, 0m, 16.67m);

    private readonly CollectionDatabaseFixture _collection;

    private Admin? _admin;

    private int _counter;

    public AxesTieTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TransitionActor AdminActor => TransitionActor.Admin(_admin!.Id, _admin.Name);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, refund_records, refunds, payment_records, returns, return_rejections, withdrawals, late_delivery_terminations, "
            + "cancellations, internal_notes, transition_records, stock_reservations, order_items, orders, coupons, variants, products, admins, "
            + "legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        await SaveAsync(c => c.Admins.Add(_admin));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK4: Ö1's simultaneous transitions by the order's types ----

    [Fact]
    public async Task ADigitalOrdersCardPayment_WritesO1S2AndTheDeliveryMark_InOneOperation_KK4()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, null, new Line(ProductType.Digital, 1));

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-digital"), token));

        Assert.Equal([TransitionCode.O1, TransitionCode.S2], result.Applied);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Delivered, PaymentStatus.Paid, 2), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.NotNull((await reader.OrderItems.SingleAsync(i => i.Id == items[0].Id, Ct)).DeliveryMarkedAt);
        List<TransitionRecord> records = await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct);
        Assert.Equivalent(new[] { TransitionCode.O1, TransitionCode.S2 }, records.Select(r => r.TransitionCode));
        Assert.All(records, r => Assert.Equal(ActorKind.System, r.ActorKind));
    }

    [Fact]
    public async Task AMixedOrdersCardPayment_WritesO1S1_MarksOnlyTheDigitalItem_AndCommitsTheStockAndTheCoupon_KK4_KK8()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 5);
        Coupon coupon = await SeedCouponAsync(3);
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, coupon, new Line(ProductType.Physical, 2, variant.Id), new Line(ProductType.Digital, 1));

        // 1.4.4.1: the order's birth reserves the units and the right.
        Assert.True(await RunCountersAsync((counters, db, token) => counters.ReserveAsync(Load(db, order.Id), token)));
        Assert.Equal((5, 2), await VariantAsync(variant.Id));
        Assert.Equal((0, 1), await CouponAsync(coupon.Id));
        Assert.Equal(2, await ReservationRowsAsync(order.Id));

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-mixed"), token));

        // 1.4.4.2: the reserved units fall finally and the right counts as used, with Ö1 and S1.
        Assert.Equal([TransitionCode.O1, TransitionCode.S1], result.Applied);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Preparing, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.Null((await reader.OrderItems.SingleAsync(i => i.Id == items[0].Id, Ct)).DeliveryMarkedAt);
        Assert.NotNull((await reader.OrderItems.SingleAsync(i => i.Id == items[1].Id, Ct)).DeliveryMarkedAt);
        Assert.Equal((3, 0), await VariantAsync(variant.Id));
        Assert.Equal((1, 0), await CouponAsync(coupon.Id));
        Assert.Equal(0, await ReservationRowsAsync(order.Id));
    }

    [Fact]
    public async Task ADigitalOrdersTransferMark_ClosesWithS2_ByTheAdministrator_KK4()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, null, new Line(ProductType.Digital, 1));

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token));

        Assert.Equal([TransitionCode.O1, TransitionCode.S2], result.Applied);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Delivered, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        List<TransitionRecord> records = await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct);
        Assert.All(records, r => Assert.Equal((ActorKind.Admin, _admin!.Id), (r.ActorKind, r.AdminId!.Value)));
    }

    [Fact]
    public async Task ADigitalAndServiceOrdersCardPayment_StaysPlacedAndPaid_TheDigitalItemDelivered_TheServiceItemWaiting_KK4()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, null, new Line(ProductType.Digital, 1), new Line(ProductType.Service, 1));

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-ds"), token));

        Assert.Equal([TransitionCode.O1], result.Applied);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Placed, PaymentStatus.Paid), (saved.OrderStatus, saved.PaymentStatus));
        Assert.NotNull((await reader.OrderItems.SingleAsync(i => i.Id == items[0].Id, Ct)).DeliveryMarkedAt);
        Assert.Null((await reader.OrderItems.SingleAsync(i => i.Id == items[1].Id, Ct)).DeliveryMarkedAt);
    }

    // ---- KK8 (1.4.4.3): the unpaid cancellation frees the reservation with S3 + Ö2 ----

    [Fact]
    public async Task TheUnpaidCancellation_FreesTheReservedUnitsAndTheRight_InTheSameOperation_KK4_KK8()
    {
        Variant variant = await SeedVariantAsync(ProductType.Service, 4);
        Coupon coupon = await SeedCouponAsync(3);
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, coupon, new Line(ProductType.Service, 3, variant.Id));
        Assert.True(await RunCountersAsync((counters, db, token) => counters.ReserveAsync(Load(db, order.Id), token)));

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.ByCustomer(order.Id), token));

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Failed), (saved.OrderStatus, saved.PaymentStatus));
        Assert.Equal((4, 0), await VariantAsync(variant.Id));
        Assert.Equal((0, 0), await CouponAsync(coupon.Id));
        Assert.Equal(0, await ReservationRowsAsync(order.Id));
    }

    [Fact]
    public async Task TheReservation_WhenTheStockIsShort_IsFalse_AndWritesNoRow_KK8()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 1);
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, null, new Line(ProductType.Physical, 2, variant.Id));

        Assert.False(await RunCountersAsync((counters, db, token) => counters.ReserveAsync(Load(db, order.Id), token)));

        Assert.Equal((1, 0), await VariantAsync(variant.Id));
        Assert.Equal(0, await ReservationRowsAsync(order.Id));
    }

    // ---- KK2, KK3 and KK8 (1.4.4.8): the transfer mark's correction ----

    [Fact]
    public async Task TheTransferMarksCorrection_OnPreparing_TakesBothAxesBack_WithTwoRecords_AndKeepsTheReservation_KK3_KK8()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 5);
        Coupon coupon = await SeedCouponAsync(3);
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, coupon, new Line(ProductType.Physical, 2, variant.Id));
        Assert.True(await RunCountersAsync((counters, db, token) => counters.ReserveAsync(Load(db, order.Id), token)));
        PaymentTransitionResult marked = await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token));
        Assert.Equal([TransitionCode.O1, TransitionCode.S1], marked.Applied);
        Assert.Equal((3, 0), await VariantAsync(variant.Id));
        (Guid o1, Guid s1) = await RecordIdsAsync(order.Id);

        TransitionOutcome corrected = await RunCoreAsync((core, token) => core.CorrectAsync(
            new CorrectionRequest(order.Id, o1, CorrectionReason.WrongOrder, null, AdminActor, 2), null, token));

        Assert.False(corrected.IsRejected);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Placed, PaymentStatus.Pending, 3), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        List<TransitionRecord> corrections = await reader.TransitionRecords
            .Where(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.Correction).ToListAsync(Ct);
        Assert.Equivalent(new[] { o1, s1 }, corrections.Select(c => c.CorrectedTransitionRecordId!.Value));
        Assert.Equal((5, 2), await VariantAsync(variant.Id));
        Assert.Equal((0, 1), await CouponAsync(coupon.Id));
        Assert.Equal(2, await ReservationRowsAsync(order.Id));
    }

    [Fact]
    public async Task TheCardApprovalsCorrection_IsRefused_ThePaymentReturnsToPendingOnlyByTheTransferMark_KK3()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.Card, null, new Line(ProductType.Physical, 1));
        await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-card"), token));
        (Guid o1, Guid s1) = await RecordIdsAsync(order.Id);

        TransitionOutcome ofO1 = await RunCoreAsync((core, token) => core.CorrectAsync(
            new CorrectionRequest(order.Id, o1, CorrectionReason.WrongOrder, null, AdminActor, 2), null, token));
        TransitionOutcome ofS1 = await RunCoreAsync((core, token) => core.CorrectAsync(
            new CorrectionRequest(order.Id, s1, CorrectionReason.WrongOrder, null, AdminActor, 2), null, token));

        Assert.Equal(TransitionRejection.CorrectionNotPossible, ofO1.Rejection);
        Assert.Equal(TransitionRejection.CorrectionNotPossible, ofS1.Rejection);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid, 2), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
    }

    [Fact]
    public async Task TheTransferMarksCorrection_OfADeliveredOrder_IsRefusedByThePairRule_KK2()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, null, new Line(ProductType.Digital, 1));
        await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token));
        Guid o1 = (await RecordIdsAsync(order.Id)).O1;

        TransitionOutcome refused = await RunCoreAsync((core, token) => core.CorrectAsync(
            new CorrectionRequest(order.Id, o1, CorrectionReason.WrongOrder, null, AdminActor, 2), null, token));

        Assert.Equal(TransitionRejection.ForbiddenStatePair, refused.Rejection);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Delivered, PaymentStatus.Paid), (saved.OrderStatus, saved.PaymentStatus));
        Assert.Equal(0, await reader.TransitionRecords.CountAsync(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.Correction, Ct));
    }

    // ---- KK6, KK7: the closure rule by quantity, S11, and no second record ----

    [Fact]
    public async Task ADeliveryFailedOrderWithADeliveredItem_ClosesWithS11_WhenItsLastOpenUnitCloses_AndWaitsWhileOneIsOpen_KK6_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.DeliveryFailed, PaymentStatus.Paid, PaymentMethod.Card, null, new Line(ProductType.Physical, 2), new Line(ProductType.Digital, 1, null, true));

        ClosureResult first = await CloseWithAsync(order.Id, db => db.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));
        ClosureResult last = await CloseWithAsync(order.Id, db => db.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 1, Amounts, null, Start)));

        Assert.Null(first.Code);
        Assert.Equal(TransitionCode.S11, last.Code);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Delivered, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.DeliveryFailed)]
    public async Task AnOrderWithNoDeliveredItem_OnShippedOrDeliveryFailed_WaitsForTheShipmentsReturn_WhenItsLastUnitCloses_KK5_KK6(OrderStatus status)
    {
        (Order order, List<OrderItem> items) = await SeedAsync(status, PaymentStatus.Paid, PaymentMethod.Card, null, new Line(ProductType.Physical, 1));

        ClosureResult closure = await CloseWithAsync(
            order.Id, db => db.LateDeliveryTerminations.Add(LateDeliveryTermination.ByBuyer(items[0].Id, 1, Today, Amounts, null, Start)));

        Assert.Null(closure.Code);
        await using AppDbContext reader = NewContext();
        Assert.Equal(status, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
    }

    [Fact]
    public async Task TheClosure_ByATermination_OpensNoCancellationRecordAndNoSecondRefund_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, null, new Line(ProductType.Physical, 1));

        ClosureResult closure = await CloseWithAsync(
            order.Id, db => db.LateDeliveryTerminations.Add(LateDeliveryTermination.ByBuyer(items[0].Id, 1, Today, Amounts, null, Start)));

        Assert.Equal(TransitionCode.S4, closure.Code);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Cancelled, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.Equal(0, await reader.Cancellations.CountAsync(c => c.OrderItemId == items[0].Id, Ct));
        Assert.Equal(0, await reader.Refunds.CountAsync(r => r.OrderId == order.Id, Ct));
    }

    // ---- KK8 (1.4.4.4-1.4.4.6): the paid order's returned units and the coupon right ----

    [Fact]
    public async Task TheReturn_GivesTheUnitsBack_AndTheCouponOnlyWithTheOrdersLastUnit_AndOnlyOnce_KK8()
    {
        Variant first = await SeedVariantAsync(ProductType.Physical, 3);
        Variant second = await SeedVariantAsync(ProductType.Physical, 3);
        Coupon coupon = await SeedCouponAsync(3);
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, coupon, new Line(ProductType.Physical, 2, first.Id), new Line(ProductType.Physical, 1, second.Id));
        await ExecuteAsync($"UPDATE coupons SET used_count = 1 WHERE id = '{coupon.Id}'");

        // 1.4.4.4: the first item's cancellation gives its units back, the second item is still open - the right stays used.
        await ReturnWithAsync(order.Id, [new VariantQuantity(first.Id, 2)], db => db.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 2, Amounts, null, Start)));
        Assert.Equal((5, 0), await VariantAsync(first.Id));
        Assert.Equal((1, 0), await CouponAsync(coupon.Id));

        // 1.4.4.5: the last unit's termination before shipping brings every unit to its last state - the right goes back with it.
        await ReturnWithAsync(
            order.Id, [new VariantQuantity(second.Id, 1)], db => db.LateDeliveryTerminations.Add(LateDeliveryTermination.ByBuyer(items[1].Id, 1, Today, Amounts, null, Start)));
        Assert.Equal((4, 0), await VariantAsync(second.Id));
        Assert.Equal((0, 0), await CouponAsync(coupon.Id));

        // A later call finds the order already final: the right is not given back twice.
        Assert.False(await RunCountersAsync((counters, db, token) => counters.IsCouponReturnDueAsync(Load(db, order.Id), token)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task APhysicalWithdrawal_GivesTheCouponBackOnlyWithTheReceipt_AndNeverAfterARejection_KK8(bool rejected)
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 3);
        Coupon coupon = await SeedCouponAsync(3);
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Card, coupon, new Line(ProductType.Physical, 1, variant.Id, true));
        await ExecuteAsync($"UPDATE coupons SET used_count = 1 WHERE id = '{coupon.Id}'");
        Withdrawal withdrawal = Withdrawal.ByBuyer(items[0].Id, 1, Today, null, Amounts, null, Start);

        // The declaration closes the unit but the goods are on their way back: no stock, no right (1.4.4.6).
        await ReturnWithAsync(order.Id, [], db => db.Withdrawals.Add(withdrawal));
        Assert.Equal((1, 0), await CouponAsync(coupon.Id));

        // The receipt adds the received unit to stock and brings the order to its last state; a rejected unit does neither.
        await ReturnWithAsync(
            order.Id,
            rejected ? [] : [new VariantQuantity(variant.Id, 1)],
            db =>
            {
                if (rejected)
                {
                    db.ReturnRejections.Add(ReturnRejection.Create(items[0].Id, withdrawal.Id, 1, Today, _admin!.Id, _admin.Name, Start));
                }
                else
                {
                    db.Returns.Add(Return.ForWithdrawal(items[0].Id, withdrawal.Id, 1, Today, _admin!.Id, _admin.Name, Start));
                }
            });

        Assert.Equal(rejected ? (3, 0) : (4, 0), await VariantAsync(variant.Id));
        Assert.Equal(rejected ? (1, 0) : (0, 0), await CouponAsync(coupon.Id));
    }

    [Fact]
    public async Task AServiceWithdrawal_GivesTheCapacityBack_ByTheWithdrawnQuantity_KK8()
    {
        Variant variant = await SeedVariantAsync(ProductType.Service, 2);
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, PaymentMethod.Card, null, new Line(ProductType.Service, 3, variant.Id));

        await ReturnWithAsync(
            order.Id, [new VariantQuantity(variant.Id, 2)], db => db.Withdrawals.Add(Withdrawal.ByBuyer(items[0].Id, 2, Today, null, Amounts, null, Start)));

        Assert.Equal((4, 0), await VariantAsync(variant.Id));
    }

    // ---- KK8 (1.4.4.7, 1.4.4.9): the wrong cancellation's retake and the variant's deletion ----

    [Theory]
    [InlineData(5, true, 3)]
    [InlineData(1, false, 1)]
    public async Task TheRetake_TakesTheUnitsAgainOnlyIfEnoughIsLeft_AndCountsTheRightAgainEvenOverItsLimit_KK8(int stock, bool expected, int stockAfter)
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, stock);
        Coupon coupon = await SeedCouponAsync(1);
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Cancelled, PaymentStatus.Paid, PaymentMethod.Card, coupon, new Line(ProductType.Physical, 2, variant.Id));
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(items[0].Id, 2, Amounts, null, Start)));

        // Another order used the coupon's one right after this order's cancellation gave it back.
        await ExecuteAsync($"UPDATE coupons SET used_count = 1 WHERE id = '{coupon.Id}'");

        bool retaken = await RunCountersAsync((counters, db, token) => counters.RetakeAsync(Load(db, order.Id), [new VariantQuantity(variant.Id, 2)], token));

        Assert.Equal(expected, retaken);
        Assert.Equal((stockAfter, 0), await VariantAsync(variant.Id));
        Assert.Equal((expected ? 2 : 1, 0), await CouponAsync(coupon.Id));
    }

    [Fact]
    public async Task TheVariantsDeletion_DropsItsReservation_AndTheCorrectionsMovementsSkipIt_KK8()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 5);
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, null, new Line(ProductType.Physical, 2, variant.Id));
        Assert.True(await RunCountersAsync((counters, db, token) => counters.ReserveAsync(Load(db, order.Id), token)));

        await ExecuteAsync($"DELETE FROM variants WHERE id = '{variant.Id}'");

        Assert.Equal(0, await ReservationRowsAsync(order.Id));
        Assert.True(await RunCountersAsync((counters, db, token) => counters.RetakeAsync(Load(db, order.Id), [new VariantQuantity(variant.Id, 2)], token)));
    }

    // ---- helpers ----

    private static Order Load(AppDbContext db, Guid orderId) => db.Orders.Single(o => o.Id == orderId);

    private TransferPaidMark Mark(Guid orderId, int version) => new(orderId, _admin!.Id, _admin.Name, version, true);

    private async Task<(Guid O1, Guid S1)> RecordIdsAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        List<TransitionRecord> records = await reader.TransitionRecords.Where(t => t.OrderId == orderId).ToListAsync(Ct);
        return (records.Single(r => r.TransitionCode == TransitionCode.O1).Id, records.SingleOrDefault(r => r.TransitionCode == TransitionCode.S1)?.Id ?? Guid.Empty);
    }

    private async Task<(int? Stock, int Reserved)> VariantAsync(Guid variantId)
    {
        await using AppDbContext reader = NewContext();
        Variant variant = await reader.Variants.SingleAsync(v => v.Id == variantId, Ct);
        return (variant.StockQuantity, variant.ReservedQuantity);
    }

    private async Task<(int Used, int Reserved)> CouponAsync(Guid couponId)
    {
        await using AppDbContext reader = NewContext();
        Coupon coupon = await reader.Coupons.SingleAsync(c => c.Id == couponId, Ct);
        return (coupon.UsedCount, coupon.ReservedCount);
    }

    private async Task<int> ReservationRowsAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.StockReservations.CountAsync(r => r.OrderId == orderId, Ct);
    }

    /// <summary>Writes an item record and asks <see cref="OrderCounters.ReturnAsync"/> for the units it returns, in one transaction like the record's task.</summary>
    private Task<bool> ReturnWithAsync(Guid orderId, IReadOnlyList<VariantQuantity> units, Action<AppDbContext> record)
    {
        return RunCountersAsync(async (counters, db, token) =>
        {
            record(db);
            await counters.ReturnAsync(Load(db, orderId), units, token);
            return true;
        });
    }

    /// <summary>Writes an item record under the order's lock and evaluates the closure rule in the same transaction, like the record's task.</summary>
    private async Task<ClosureResult> CloseWithAsync(Guid orderId, Action<AppDbContext> record)
    {
        ClosureResult closure = ClosureResult.None;
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        OrderTransitions core = scope.ServiceProvider.GetRequiredService<OrderTransitions>();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        OrderLocks locks = scope.ServiceProvider.GetRequiredService<OrderLocks>();
        await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                await locks.LockAsync([orderId], token);
                Order locked = await db.Orders.SingleAsync(o => o.Id == orderId, token);
                record(db);
                locked.BumpVersion();
                closure = await core.EvaluateClosureAsync(locked, TransitionActor.Buyer, token);
                return new TransitionOutcome(null, [], closure);
            },
            Ct);
        return closure;
    }

    private async Task<T> RunCountersAsync<T>(Func<OrderCounters, AppDbContext, CancellationToken, Task<T>> work)
    {
        T value = default!;
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        OrderCounters counters = scope.ServiceProvider.GetRequiredService<OrderCounters>();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                value = await work(counters, db, token);
                return new TransitionOutcome(null, [], ClosureResult.None);
            },
            Ct);
        return value;
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<T> RunCoreAsync<T>(Func<OrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        OrderTransitions core = scope.ServiceProvider.GetRequiredService<OrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(core, token), Ct);
    }

    private async Task<Variant> SeedVariantAsync(ProductType type, int? stock)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Ürün", $"eksen-urun-{n}", type, 20m, Start);
        if (type == ProductType.Physical)
        {
            product.WithSeededOrigin();
        }

        Variant variant = Variant.Create(product.Id, $"EKS-{n}", 10m, Start, stock);
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
        Coupon coupon = Coupon.CreatePercentage($"EKSEN{n}", 10m, Today.AddDays(-7), Today.AddDays(30), limit, Start);
        await SaveAsync(c => c.Coupons.Add(coupon));
        return coupon;
    }

    private async Task<(Order Order, List<OrderItem> Items)> SeedAsync(
        OrderStatus orderStatus, PaymentStatus paymentStatus, PaymentMethod method, Coupon? coupon, params Line[] lines)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n % 100:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, true, "Ali@Example.com", 120m, 0m, Start.AddHours(1), Start);
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
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;
        if (coupon is not null)
        {
            order.CouponId = coupon.Id;
            order.CouponCode = $"EKSEN{n}";
        }

        List<OrderItem> items = [];
        foreach (Line line in lines)
        {
            OrderItem item = OrderItem.Create(order.Id, null, line.VariantId, line.Type, "Ürün", line.Quantity, 100m, 20m, 16.67m, Start);
            if (line.Type == ProductType.Digital)
            {
                item.DownloadAllowance = 5;
            }

            if (line.Type == ProductType.Service)
            {
                item.FulfilmentPeriod = 3;
            }

            if (line.Delivered)
            {
                item.DeliveryMarkedAt = Start;
                item.DeliveryDate = Today;
            }

            items.Add(item);
        }

        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.AddRange(items);
        });

        // The states are set around the entity (it takes them only through the unit); the pair rule of the database still applies.
        await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        return (order, items);
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
        services.AddNoticeDependencies();
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

    /// <summary>One item of a seeded order: its type, quantity, variant and whether it already carries the delivery mark.</summary>
    public sealed record Line(ProductType Type, int Quantity, Guid? VariantId = null, bool Delivered = false);
}
