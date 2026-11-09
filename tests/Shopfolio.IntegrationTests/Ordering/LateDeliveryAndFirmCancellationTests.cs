using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
/// The late delivery termination and the firm's cancellation on a real PostgreSQL and a fake clock (T368 criteria 1-9, KK1-KK9; 03 1.11.8, 1.11.21, 1.11.41,
/// 1.4.4.4, 1.4.4.5; 02 7.2.3, 7.2.4, 8.3.8, 10.4.10): <see cref="IOrderTransitions.TerminateForLateDeliveryAsync"/> opens thirty days after the order reached the
/// firm, takes every undelivered physical item's open units - no choice, digital and service items go on - writes a record type of its own, closes a preparing order
/// by the closure rule without B-7 and leaves a shipped one in its status; the firm's record of a notice from another channel carries the day the notice arrived.
/// <see cref="IOrderTransitions.CancelItemByFirmAsync"/> takes a reason from the closed list, the note on "Other", the owner's confirmation on "müşteriyle
/// anlaşıldı", the irreversible-action confirmation, and sends B-7; the whole-order cancellation of a paid order writes the items' records too (D-305). The panel's
/// legal interest warning marks every termination and the cancellations after the promise. Every operation runs in the transaction runner like a page would call it.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class LateDeliveryAndFirmCancellationTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset Late = DeliveryPeriodRules.LegalDeliveryLimitEnd(Start).AddHours(1);

    private readonly CollectionDatabaseFixture _collection;

    private readonly ManualTimeProvider _clock = new(Start);

    private ServiceProvider _provider = null!;

    private Admin? _admin;

    private int _counter;

    public LateDeliveryAndFirmCancellationTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TransitionAdmin AdminCaller => new(_admin!.Id, _admin.Name);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, refund_records, refund_iban_requests, refunds, payment_records, returns, return_rejections, defect_claims, withdrawals, "
            + "late_delivery_terminations, item_removals, cancellations, internal_notes, transition_records, stock_reservations, order_items, orders, coupons, "
            + "variants, products, admins, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        await SaveAsync(c => c.Admins.Add(_admin));
        _clock.SetUtcNow(Start);
        _provider = BuildProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK1, KK3, KK4: the customer's termination of a preparing order ----

    [Fact]
    public async Task Termination_OfAPreparingOrder_TakesEveryPhysicalItemsOpenUnits_ClosesWithTheClosureS4_WithoutB7_AndGivesTheStockBack_KK1_KK3_KK4()
    {
        Variant first = await SeedVariantAsync(5);
        Variant second = await SeedVariantAsync(5);
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, new Line(ProductType.Physical, 3, first.Id), new Line(ProductType.Physical, 1, second.Id));

        // One unit of the first item was cancelled by the customer before: the termination takes the two units left, never a chosen number.
        Assert.False((await RunAsync((unit, token) => unit.CancelItemAsync(new CancelItemRequest(order.Id, items[0].Id, 1), token))).IsRejected);
        _clock.SetUtcNow(Late);

        OrderTransitionResult result = await TerminateAsync(TerminateForLateDeliveryRequest.ByCustomer(order.Id));

        Assert.False(result.IsRejected);
        Assert.Equal(2, result.ItemRecordIds.Count);
        await using AppDbContext reader = NewContext();
        List<LateDeliveryTermination> terminations = await reader.LateDeliveryTerminations.OrderBy(t => t.Quantity).ToListAsync(Ct);
        Assert.Equal(
            [(items[1].Id, 1, ActorKind.Buyer, (Guid?)null, TurkeyTime.DateOf(Late)), (items[0].Id, 2, ActorKind.Buyer, (Guid?)null, TurkeyTime.DateOf(Late))],
            terminations.Select(t => (t.OrderItemId, t.Quantity, t.ActorKind, t.AdminId, t.DeclarationDate)));
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal(OrderStatus.Cancelled, saved.OrderStatus);
        TransitionRecord closure = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((TransitionCode.S4, TransitionReason.Closure, (CancellationReason?)null), (closure.TransitionCode, closure.TransitionReason, closure.CancellationReason));
        Assert.Equal([closure.Id], result.TransitionRecordIds);

        // B-7 went only with the customer's earlier cancellation; the termination sends B-9 to the customer and F-3 to the firm.
        Assert.Equal(1, await CountAsync(order.Id, DispatchType.B7));
        Assert.Equal(1, await CountAsync(order.Id, DispatchType.B9));
        Assert.Equal(1, await CountF3Async(result.ItemRecordIds));
        Assert.Equal(8, (await VariantAsync(first.Id)).Stock);
        Assert.Equal(6, (await VariantAsync(second.Id)).Stock);
    }

    [Fact]
    public async Task Termination_LeavesTheDigitalAndTheServiceItemOut_AndTheOrderGoesOnAlongThem_KK3()
    {
        Variant variant = await SeedVariantAsync(5);
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, new Line(ProductType.Physical, 2, variant.Id), new Line(ProductType.Digital, 1), new Line(ProductType.Service, 1));
        _clock.SetUtcNow(Late);

        OrderTransitionResult result = await TerminateAsync(TerminateForLateDeliveryRequest.ByCustomer(order.Id));

        Assert.False(result.IsRejected);
        Assert.Equal([items[0].Id], result.ItemIds);
        Assert.Empty(result.TransitionRecordIds);
        await using AppDbContext reader = NewContext();
        LateDeliveryTermination termination = await reader.LateDeliveryTerminations.SingleAsync(Ct);
        Assert.Equal((items[0].Id, 2), (termination.OrderItemId, termination.Quantity));
        Assert.Equal((OrderStatus.Preparing, 2), await OrderAsync(order.Id));
        Assert.Equal(7, (await VariantAsync(variant.Id)).Stock);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.DeliveryFailed)]
    public async Task Termination_OfAShippedOrder_KeepsItsStatusAndTheStock_AndTheTerminatedItemIsNoLongerTheFirmsToCancel_KK1_KK4(OrderStatus status)
    {
        Variant variant = await SeedVariantAsync(5);
        (Order order, List<OrderItem> items) = await SeedAsync(status, new Line(ProductType.Physical, 2, variant.Id));
        _clock.SetUtcNow(Late);

        OrderTransitionResult result = await TerminateAsync(TerminateForLateDeliveryRequest.ByCustomer(order.Id));
        OrderTransitionResult firm = await CancelByFirmAsync(order.Id, items[0].Id, 1, 2, CancellationReason.OutOfStock);

        Assert.False(result.IsRejected);
        Assert.Empty(result.TransitionRecordIds);
        Assert.True(firm.IsRejected);
        Assert.Equal((status, 2), await OrderAsync(order.Id));
        Assert.Equal(5, (await VariantAsync(variant.Id)).Stock);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.Cancellations.AnyAsync(Ct));
        Assert.False(await reader.TransitionRecords.AnyAsync(t => t.OrderId == order.Id, Ct));
        Assert.Equal(0, await CountAsync(order.Id, DispatchType.B7));
    }

    // ---- KK2: the button's conditions ----

    [Fact]
    public async Task Termination_OpensExactlyThirtyDaysAfterTheOrderReachedTheFirm_AndBeforeItWritesNothing_KK2()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, new Line(ProductType.Physical, 1));
        DateTimeOffset limit = DeliveryPeriodRules.LegalDeliveryLimitEnd(order.CreatedAt);
        _clock.SetUtcNow(limit.AddTicks(-1));

        OrderTransitionResult early = await TerminateAsync(TerminateForLateDeliveryRequest.ByCustomer(order.Id));

        Assert.Equal(TransitionRefusal.ConditionNotMet, early.Refusal);
        await AssertNothingTerminatedAsync(order.Id, OrderStatus.Preparing);

        _clock.SetUtcNow(limit);
        Assert.False((await TerminateAsync(TerminateForLateDeliveryRequest.ByCustomer(order.Id))).IsRejected);
    }

    [Fact]
    public async Task Termination_CoversTheItemExemptFromWithdrawal_TheCustomMadeOneToo_KK2()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, new Line(ProductType.Physical, 1, Exemption: WithdrawalExemptionReason.MadeToOrder));
        _clock.SetUtcNow(Late);

        OrderTransitionResult result = await TerminateAsync(TerminateForLateDeliveryRequest.ByCustomer(order.Id));

        Assert.False(result.IsRejected);
        Assert.Equal([items[0].Id], result.ItemIds);
    }

    [Fact]
    public async Task Termination_DoesNotOpen_OnAnItemWithADeliveryDate_OnADigitalOnlyOrder_OrOnAnUnpaidOne_KK2()
    {
        (Order dated, _) = await SeedAsync(OrderStatus.Shipped, new Line(ProductType.Physical, 1, Delivered: true));
        (Order digital, _) = await SeedAsync(OrderStatus.Preparing, new Line(ProductType.Digital, 1));
        (Order unpaid, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, new Line(ProductType.Physical, 1));
        _clock.SetUtcNow(Late);

        foreach ((Order order, OrderStatus status) in new[] { (dated, OrderStatus.Shipped), (digital, OrderStatus.Preparing), (unpaid, OrderStatus.Placed) })
        {
            OrderTransitionResult result = await TerminateAsync(TerminateForLateDeliveryRequest.ByCustomer(order.Id));

            Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
            await AssertNothingTerminatedAsync(order.Id, status);
        }
    }

    // ---- KK5: the notice from another channel ----

    [Fact]
    public async Task ANoticeFromAnotherChannel_IsRecordedLikeTheButton_DatedTheDayItArrived_WithTheAdministrator_AndWithoutF3_KK5()
    {
        Variant variant = await SeedVariantAsync(5);
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, new Line(ProductType.Physical, 2, variant.Id));
        _clock.SetUtcNow(Late);
        DateOnly arrival = TurkeyTime.DateOf(Late).AddDays(-2);

        OrderTransitionResult result = await TerminateAsync(TerminateForLateDeliveryRequest.FromOtherChannel(order.Id, AdminCaller, 1, true, arrival));

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        LateDeliveryTermination termination = await reader.LateDeliveryTerminations.SingleAsync(Ct);
        Assert.Equal(
            (items[0].Id, 2, ActorKind.Admin, (Guid?)_admin!.Id, arrival),
            (termination.OrderItemId, termination.Quantity, termination.ActorKind, termination.AdminId, termination.DeclarationDate));
        Assert.Equal(OrderStatus.Cancelled, (await OrderAsync(order.Id)).Status);
        Assert.Equal(1, await CountAsync(order.Id, DispatchType.B9));
        Assert.Equal(0, await CountF3Async(result.ItemRecordIds));
        Assert.Equal(0, await CountAsync(order.Id, DispatchType.B7));
        Assert.Equal(7, (await VariantAsync(variant.Id)).Stock);
    }

    [Fact]
    public async Task ANoticeFromAnotherChannel_DatedInTheFuture_Unconfirmed_OrOnAnOldScreen_IsRefused_AndTheButtonTakesNoDate_KK5()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, new Line(ProductType.Physical, 1));
        _clock.SetUtcNow(Late);
        DateOnly today = TurkeyTime.DateOf(Late);

        Assert.Equal(
            TransitionRefusal.InvalidInput,
            (await TerminateAsync(TerminateForLateDeliveryRequest.FromOtherChannel(order.Id, AdminCaller, 1, true, today.AddDays(1)))).Refusal);
        Assert.Equal(
            TransitionRefusal.ConfirmationRequired,
            (await TerminateAsync(TerminateForLateDeliveryRequest.FromOtherChannel(order.Id, AdminCaller, 1, false, today))).Refusal);
        Assert.Equal(
            TransitionRefusal.VersionConflict,
            (await TerminateAsync(TerminateForLateDeliveryRequest.FromOtherChannel(order.Id, AdminCaller, 7, true, today))).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await TerminateAsync(new TerminateForLateDeliveryRequest(order.Id, null, null, false, today))).Refusal);
        await AssertNothingTerminatedAsync(order.Id, OrderStatus.Preparing);
    }

    // ---- KK6: the legal interest warning ----

    [Fact]
    public async Task TheLegalInterestWarning_MarksEveryTermination_AndTheCancellationsAfterThePromise_NotTheOnesBefore_KK6()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, new Line(ProductType.Physical, 1), new Line(ProductType.Physical, 2));
        await ApproveAsync(order.Id, shippingPeriod: 2);
        DateTimeOffset promiseEnd = DeliveryPeriodRules.ShippingPeriodEnd(Start, 2, BusinessCalendar.Product);

        _clock.SetUtcNow(promiseEnd.AddMinutes(-1));
        OrderTransitionResult before = await CancelByFirmAsync(order.Id, items[0].Id, 1, 1, CancellationReason.DamagedOrUnsellable);
        _clock.SetUtcNow(promiseEnd.AddHours(1));
        OrderTransitionResult after = await CancelByFirmAsync(order.Id, items[1].Id, 1, 2, CancellationReason.OutOfStock);
        _clock.SetUtcNow(Late);
        OrderTransitionResult termination = await TerminateAsync(TerminateForLateDeliveryRequest.ByCustomer(order.Id));

        Assert.False(before.IsRejected || after.IsRejected || termination.IsRejected);
        IReadOnlySet<Guid> warned = await PeriodsAsync(p => p.LegalInterestWarningsAsync(order.Id, Ct));
        Assert.Equal(new HashSet<Guid> { after.ItemRecordId!.Value, Assert.Single(termination.ItemRecordIds) }, warned.ToHashSet());
    }

    // ---- KK7, KK8: the firm's item cancellation ----

    [Fact]
    public async Task TheFirmsItemCancellation_CarriesTheReasonAndTheNote_SendsB7_AndTheLastUnitsAreTheFirmsS4_LinkedToTheRecord_KK7()
    {
        Variant variant = await SeedVariantAsync(5);
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, new Line(ProductType.Physical, 2, variant.Id));

        OrderTransitionResult partial = await CancelByFirmAsync(order.Id, items[0].Id, 1, 1, CancellationReason.Other, "Tedarikçi üretimi durdurdu");
        _clock.Advance(TimeSpan.FromHours(1));
        OrderTransitionResult last = await CancelByFirmAsync(order.Id, items[0].Id, 1, 2, CancellationReason.OutOfStock);

        Assert.False(partial.IsRejected);
        Assert.Empty(partial.TransitionRecordIds);
        Guid s4 = Assert.Single(last.TransitionRecordIds);
        await using AppDbContext reader = NewContext();
        List<Cancellation> cancellations = await reader.Cancellations.OrderBy(c => c.CreatedAt).ToListAsync(Ct);
        Assert.Equal(
            [
                (partial.ItemRecordId!.Value, ActorKind.Admin, (Guid?)_admin!.Id, (CancellationReason?)CancellationReason.Other, "Tedarikçi üretimi durdurdu", (Guid?)null),
                (last.ItemRecordId!.Value, ActorKind.Admin, (Guid?)_admin.Id, (CancellationReason?)CancellationReason.OutOfStock, (string?)null, (Guid?)s4),
            ],
            cancellations.Select(c => (c.Id, c.ActorKind, c.AdminId, c.CancellationReason, c.ReasonNote, c.TransitionRecordId)));
        TransitionRecord transition = await reader.TransitionRecords.SingleAsync(t => t.Id == s4, Ct);
        Assert.Equal(
            (TransitionCode.S4, ActorKind.Admin, (CancellationReason?)CancellationReason.OutOfStock),
            (transition.TransitionCode, transition.ActorKind, transition.CancellationReason));
        Assert.Equal(OrderStatus.Cancelled, (await OrderAsync(order.Id)).Status);
        Assert.Equal(2, await CountAsync(order.Id, DispatchType.B7));
        Assert.Equal(7, (await VariantAsync(variant.Id)).Stock);
    }

    [Fact]
    public async Task TheFirmsCancellation_OfAReturnedShipmentsLastUnits_IsTheFirmsS9_WithItsReason_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.DeliveryFailed, new Line(ProductType.Physical, 1));

        OrderTransitionResult result = await CancelByFirmAsync(order.Id, items[0].Id, 1, 1, CancellationReason.UndeliveredBuyerCaused);

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        TransitionRecord s9 = await reader.TransitionRecords.SingleAsync(t => t.Id == Assert.Single(result.TransitionRecordIds), Ct);
        Assert.Equal((TransitionCode.S9, (CancellationReason?)CancellationReason.UndeliveredBuyerCaused), (s9.TransitionCode, s9.CancellationReason));
        Assert.Equal(s9.Id, (await reader.Cancellations.SingleAsync(Ct)).TransitionRecordId);
        Assert.Equal(OrderStatus.Cancelled, (await OrderAsync(order.Id)).Status);
    }

    [Fact]
    public async Task TheFirmsItemCancellation_WithoutTheNoteOnOther_WithANoteElsewhere_OrWithoutTheConfirmation_WritesNothing_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, new Line(ProductType.Physical, 2));

        Assert.Equal(TransitionRefusal.InvalidInput, (await CancelByFirmAsync(order.Id, items[0].Id, 1, 1, CancellationReason.Other, " ")).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await CancelByFirmAsync(order.Id, items[0].Id, 1, 1, CancellationReason.OutOfStock, "not")).Refusal);
        Assert.Equal(
            TransitionRefusal.ConfirmationRequired,
            (await RunAsync((unit, token) => unit.CancelItemByFirmAsync(
                new CancelItemByFirmRequest(order.Id, items[0].Id, 1, AdminCaller, 1, false, CancellationReason.OutOfStock, null, false), token))).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await CancelByFirmAsync(order.Id, items[0].Id, 3, 1, CancellationReason.OutOfStock)).Refusal);
        await AssertNoFirmCancellationAsync(order.Id, OrderStatus.Preparing);
    }

    [Fact]
    public async Task TheFirmsItemCancellation_IsClosedOnADigitalItem_AndOnAShippedOrder_KK7()
    {
        (Order digital, List<OrderItem> digitalItems) = await SeedAsync(OrderStatus.Preparing, new Line(ProductType.Digital, 1), new Line(ProductType.Physical, 1));
        (Order shipped, List<OrderItem> shippedItems) = await SeedAsync(OrderStatus.Shipped, new Line(ProductType.Physical, 1));

        Assert.Equal(TransitionRefusal.ConditionNotMet, (await CancelByFirmAsync(digital.Id, digitalItems[0].Id, 1, 1, CancellationReason.OutOfStock)).Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await CancelByFirmAsync(shipped.Id, shippedItems[0].Id, 1, 1, CancellationReason.OutOfStock)).Refusal);
        await AssertNoFirmCancellationAsync(digital.Id, OrderStatus.Preparing);
        await AssertNoFirmCancellationAsync(shipped.Id, OrderStatus.Shipped);
    }

    // ---- KK9: "müşteriyle anlaşıldı" ----

    [Fact]
    public async Task AgreedWithTheCustomer_WithoutTheFirmsConfirmationOfTheOwner_IsNoCancellation_ItemOrWholeOrder_AndWithItIsApplied_KK9()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, new Line(ProductType.Physical, 2));

        OrderTransitionResult item = await CancelByFirmAsync(order.Id, items[0].Id, 1, 1, CancellationReason.AgreedWithBuyer);
        OrderTransitionResult whole = await RunAsync((unit, token) =>
            unit.CancelOrderAsync(CancelOrderRequest.ByAdministrator(order.Id, AdminCaller, 1, true, CancellationReason.AgreedWithBuyer), token));

        Assert.Equal(TransitionRefusal.InvalidInput, item.Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, whole.Refusal);
        await AssertNoFirmCancellationAsync(order.Id, OrderStatus.Preparing);

        OrderTransitionResult confirmed = await CancelByFirmAsync(order.Id, items[0].Id, 1, 1, CancellationReason.AgreedWithBuyer, ownerConfirmed: true);
        Assert.False(confirmed.IsRejected);
        await using AppDbContext reader = NewContext();
        Assert.Equal(CancellationReason.AgreedWithBuyer, (await reader.Cancellations.SingleAsync(Ct)).CancellationReason);
    }

    // ---- D-305: the firm's whole-order cancellation of a paid order ----

    [Fact]
    public async Task TheFirmsWholeOrderCancellation_OfAPaidOrder_WritesEveryOpenItemsRecord_LinkedToTheS4_AndGivesTheUnitsBack_OneB7()
    {
        Variant first = await SeedVariantAsync(5);
        Variant second = await SeedVariantAsync(5);
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, new Line(ProductType.Physical, 3, first.Id), new Line(ProductType.Physical, 1, second.Id));
        Assert.False((await RunAsync((unit, token) => unit.CancelItemAsync(new CancelItemRequest(order.Id, items[0].Id, 1), token))).IsRejected);

        OrderTransitionResult result = await RunAsync((unit, token) =>
            unit.CancelOrderAsync(CancelOrderRequest.ByAdministrator(order.Id, AdminCaller, 2, true, CancellationReason.CannotShipToAddress), token));

        Assert.False(result.IsRejected);
        Guid s4 = Assert.Single(result.TransitionRecordIds);
        await using AppDbContext reader = NewContext();
        List<Cancellation> firm = await reader.Cancellations.Where(c => c.ActorKind == ActorKind.Admin).OrderBy(c => c.Quantity).ToListAsync(Ct);
        Assert.Equal(
            [(items[1].Id, 1, (Guid?)s4), (items[0].Id, 2, (Guid?)s4)],
            firm.Select(c => (c.OrderItemId, c.Quantity, c.TransitionRecordId)));
        Assert.All(firm, c => Assert.Equal(CancellationReason.CannotShipToAddress, c.CancellationReason));
        Assert.Equal(OrderStatus.Cancelled, (await OrderAsync(order.Id)).Status);
        Assert.Equal(2, await CountAsync(order.Id, DispatchType.B7));
        Assert.Equal(8, (await VariantAsync(first.Id)).Stock);
        Assert.Equal(6, (await VariantAsync(second.Id)).Stock);
    }

    // ---- helpers ----

    private Task<OrderTransitionResult> TerminateAsync(TerminateForLateDeliveryRequest request) =>
        RunAsync((unit, token) => unit.TerminateForLateDeliveryAsync(request, token));

    private Task<OrderTransitionResult> CancelByFirmAsync(
        Guid orderId, Guid itemId, int quantity, int version, CancellationReason reason, string? note = null, bool ownerConfirmed = false) =>
        RunAsync((unit, token) => unit.CancelItemByFirmAsync(
            new CancelItemByFirmRequest(orderId, itemId, quantity, AdminCaller, version, true, reason, note, ownerConfirmed), token));

    /// <summary>A refused termination writes nothing: the status and the version stay, no record, no transition, no notice.</summary>
    private async Task AssertNothingTerminatedAsync(Guid orderId, OrderStatus status)
    {
        Assert.Equal((status, 1), await OrderAsync(orderId));
        await using AppDbContext reader = NewContext();
        List<Guid> itemIds = await reader.OrderItems.Where(i => i.OrderId == orderId).Select(i => i.Id).ToListAsync(Ct);
        Assert.False(await reader.LateDeliveryTerminations.AnyAsync(t => itemIds.Contains(t.OrderItemId), Ct));
        Assert.False(await reader.TransitionRecords.AnyAsync(t => t.OrderId == orderId && t.TransitionCode != TransitionCode.O1, Ct));
        Assert.Equal(0, await CountAsync(orderId, DispatchType.B9));
    }

    /// <summary>A refused firm cancellation writes nothing: the status and the version stay, no record, no transition and no B-7.</summary>
    private async Task AssertNoFirmCancellationAsync(Guid orderId, OrderStatus status)
    {
        Assert.Equal((status, 1), await OrderAsync(orderId));
        await using AppDbContext reader = NewContext();
        List<Guid> itemIds = await reader.OrderItems.Where(i => i.OrderId == orderId).Select(i => i.Id).ToListAsync(Ct);
        Assert.False(await reader.Cancellations.AnyAsync(c => itemIds.Contains(c.OrderItemId), Ct));
        Assert.False(await reader.TransitionRecords.AnyAsync(t => t.OrderId == orderId, Ct));
        Assert.Equal(0, await CountAsync(orderId, DispatchType.B7));
    }

    private async Task<(OrderStatus Status, int Version)> OrderAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == orderId, Ct);
        return (saved.OrderStatus, saved.Version);
    }

    private async Task<int> CountAsync(Guid orderId, DispatchType type)
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.CountAsync(d => d.OrderId == orderId && d.DispatchType == type, Ct);
    }

    /// <summary>The payment's approval (Ö1) at <see cref="Start"/> and the order's frozen shipping promise: the legal interest warning reads both.</summary>
    private async Task ApproveAsync(Guid orderId, int shippingPeriod)
    {
        await SaveAsync(c => c.TransitionRecords.Add(TransitionRecord.Create(orderId, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, Start)));
        await ExecuteAsync($"UPDATE orders SET shipping_period = {shippingPeriod} WHERE id = '{orderId}'");
    }

    private async Task<T> PeriodsAsync<T>(Func<OrderPeriods, Task<T>> read)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await read(scope.ServiceProvider.GetRequiredService<OrderPeriods>());
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<(int? Stock, int Reserved)> VariantAsync(Guid variantId)
    {
        await using AppDbContext reader = NewContext();
        Variant variant = await reader.Variants.SingleAsync(v => v.Id == variantId, Ct);
        return (variant.StockQuantity, variant.ReservedQuantity);
    }

    private async Task<Variant> SeedVariantAsync(int stock)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Ürün", $"fesih-urun-{n}", ProductType.Physical, 20m, Start);
        product.WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, $"FSH-{n}", 10m, Start, stock);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        return variant;
    }

    private Task<(Order Order, List<OrderItem> Items)> SeedAsync(OrderStatus orderStatus, params Line[] lines) =>
        SeedAsync(orderStatus, PaymentStatus.Paid, lines);

    private async Task<(Order Order, List<OrderItem> Items)> SeedAsync(OrderStatus orderStatus, PaymentStatus paymentStatus, params Line[] lines)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n % 100:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n + 368000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Start.AddHours(1), Start);
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

        List<OrderItem> items = [];
        foreach (Line line in lines)
        {
            OrderItem item = OrderItem.Create(order.Id, null, line.VariantId, line.Type, "Ürün", line.Quantity, 100m, 20m, 16.67m, Start);
            item.WithdrawalExemptionReason = line.Exemption;
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
                item.DeliveryDate = TurkeyTime.DateOf(Start);
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
        services.AddPaymentModule();
        services.AddOrderingModule();

        // F-3 goes to the firm's contact e-mail (08 2.2.7): a fixed address here, so the firm notice is written without the Settings module's legal identity.
        services.AddScoped<ICompanyContactEmail, FixedCompanyContactEmail>();
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

    /// <summary>One item of a seeded order: its type, units, variant, whether it carries the delivery mark and its withdrawal exemption.</summary>
    public sealed record Line(ProductType Type, int Quantity, Guid? VariantId = null, bool Delivered = false, WithdrawalExemptionReason? Exemption = null);

    /// <summary>F-3 carries no order link (08 2.2.7): it is found by the termination record it names.</summary>
    private async Task<int> CountF3Async(IReadOnlyList<Guid> terminationIds)
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.CountAsync(
            d => d.DispatchType == DispatchType.F3
                && d.RelatedRecordType == DispatchRelatedRecordType.LateDeliveryTermination
                && d.RelatedRecordId != null
                && terminationIds.Contains(d.RelatedRecordId.Value),
            Ct);
    }

    private sealed class FixedCompanyContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>("firma@example.com");
    }
}
