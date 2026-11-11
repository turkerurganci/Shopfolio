using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
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
/// The customer's operations of E-17 and of the order page's IBAN field on a real PostgreSQL and a fake clock (T512 criteria 1-10; 07 UC-35, UC-38…UC-41):
/// <see cref="ICustomerOperations"/> draws the screen from the one operation decision and applies each write in the caller's transaction, so a refused request
/// rolls back whole. Every write runs in the transaction runner the way the page's <c>OperationRunner</c> does; what it wrote is read back from the tables.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class CustomerOperationsTests : IAsyncLifetime
{
    private const string ValidIban = "TR330006100519786457841326";

    private const string CorrectedIban = "TR320010009999901234567890";

    private const string BadCheckDigits = "TR330006100519786457841327";

    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset Late = DeliveryPeriodRules.LegalDeliveryLimitEnd(Start).AddHours(1);

    private readonly CollectionDatabaseFixture _collection;

    private readonly ManualTimeProvider _clock = new(Start);

    private ServiceProvider _provider = null!;

    private Admin? _admin;

    private int _counter;

    public CustomerOperationsTests(CollectionDatabaseFixture collection)
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

    // ---- KK4, KK5: the two addresses, the two operations of the cancellation address, the server's three values ----

    [Fact]
    public async Task TheCancellationAddress_OfAnUnpaidOrder_IsTheOrdersCancellation_ItsItemsReadOnly_AndNoIbanField_KK4_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2));

        CustomerOperationScreen? screen = await ScreenAsync(order.Id, CustomerScreen.Cancellation);

        Assert.NotNull(screen);
        Assert.Equal((CustomerScreenOperation.CancelOrder, false), (screen.Operation, screen.AsksIban));
        CustomerScreenItem item = Assert.Single(screen.Items);
        Assert.Equal((items[0].Id, 2, false), (item.OrderItemId, item.OpenQuantity, item.HasQuantityField));
    }

    [Fact]
    public async Task TheCancellationAddress_OfAPaidOrder_IsTheItemCancellation_WithTheServersThreeValues_TheQuantityFieldOnlyOnAnItemOfSeveralUnits_KK4_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 3), new Line(ProductType.Physical, 1), new Line(ProductType.Digital, 1));
        Assert.False((await RunAsync((u, t) => u.CancelItemAsync(new CancelItemRequest(order.Id, items[0].Id, 1), t))).IsRejected);

        CustomerOperationScreen? screen = await ScreenAsync(order.Id, CustomerScreen.Cancellation);

        // The open units, the default and the upper limit are the decision's (the whole open quantity for a cancellation); the digital item's cancellation is not the customer's.
        Assert.NotNull(screen);
        Assert.Equal((CustomerScreenOperation.CancelOrderItems, false, RefundPreview.RefundPeriodDays), (screen.Operation, screen.AsksIban, screen.RefundPeriodDays));
        Assert.Equal(
            [(items[0].Id, 2, 2, 2, true), (items[1].Id, 1, 1, 1, false)],
            screen.Items.OrderByDescending(i => i.OpenQuantity).Select(i => (i.OrderItemId, i.OpenQuantity, i.DefaultQuantity, i.MaxQuantity, i.HasQuantityField)));
        Assert.Equal(new Dictionary<Guid, int> { [items[0].Id] = 2, [items[1].Id] = 1 }, screen.OpenPhysicalUnits);
    }

    [Fact]
    public async Task TheIbanField_IsDrawnOnlyOnThePaidTransferOrder_KK5()
    {
        (Order transfer, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2));
        (Order card, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2));

        Assert.True((await ScreenAsync(transfer.Id, CustomerScreen.Cancellation))!.AsksIban);
        Assert.False((await ScreenAsync(card.Id, CustomerScreen.Cancellation))!.AsksIban);
    }

    [Fact]
    public async Task TheTerminationAddress_IsClosedBeforeTheLimit_AndThenListsTheUndeliveredPhysicalItemsOpenUnits_ReadOnly_KK4_KK6()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1, Delivered: true), new Line(ProductType.Digital, 1));

        Assert.Null(await ScreenAsync(order.Id, CustomerScreen.LateDeliveryTermination));
        _clock.SetUtcNow(Late);
        CustomerOperationScreen? screen = await ScreenAsync(order.Id, CustomerScreen.LateDeliveryTermination);

        Assert.NotNull(screen);
        Assert.Equal((CustomerScreenOperation.TerminateForLateDelivery, true), (screen.Operation, screen.AsksIban));
        Assert.Equal([(items[0].Id, 2, false)], screen.Items.Select(i => (i.OrderItemId, i.OpenQuantity, i.HasQuantityField)));
    }

    // ---- KK7: the order's cancellation ----

    [Fact]
    public async Task CancelOrder_OnPlacedAndPending_CancelsTheOrder_AndFreesTheReservations_ButIsClosedOnAPaidOrder_KK7()
    {
        Variant variant = await SeedVariantAsync(5);
        (Order order, _) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2, variant.Id));
        await SaveAsync(c => c.StockReservations.Add(StockReservation.ForVariant(order.Id, variant.Id, 2, Start)));
        await ExecuteAsync($"UPDATE variants SET reserved_quantity = 2 WHERE id = '{variant.Id}'");
        (Order paid, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1));

        CustomerOperationResult applied = await OperateAsync((o, t) => o.CancelOrderAsync(order.Id, t));
        CustomerOperationResult closed = await OperateAsync((o, t) => o.CancelOrderAsync(paid.Id, t));

        Assert.Equal(CustomerOperationOutcome.Applied, applied.Outcome);
        Assert.Equal(CustomerOperationOutcome.Closed, closed.Outcome);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Cancelled, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.False(await reader.StockReservations.AnyAsync(r => r.OrderId == order.Id, Ct));
        Assert.Equal(0, (await reader.Variants.SingleAsync(v => v.Id == variant.Id, Ct)).ReservedQuantity);
        Assert.Equal(OrderStatus.Preparing, (await reader.Orders.SingleAsync(o => o.Id == paid.Id, Ct)).OrderStatus);
    }

    // ---- KK8: the whole request is refused ----

    [Fact]
    public async Task CancelOrderItems_AQuantityAboveTheOpenUnits_OrAnOpenQuantityOtherThanTheShownOne_RefusesTheWholeRequest_KK8()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 2));

        CustomerOperationResult exceeds = await OperateAsync((o, t) => o.CancelOrderItemsAsync(
            order.Id, [new CancelOrderItemsLine(items[0].Id, 1, 2), new CancelOrderItemsLine(items[1].Id, 3, 2)], null, t));
        CustomerOperationResult stale = await OperateAsync((o, t) => o.CancelOrderItemsAsync(
            order.Id, [new CancelOrderItemsLine(items[0].Id, 1, 2), new CancelOrderItemsLine(items[2].Id, 1, 1)], null, t));

        Assert.Equal((CustomerOperationOutcome.QuantityExceedsOpen, true), (exceeds.Outcome, exceeds.Gate.IsRejected));
        Assert.Equal(CustomerOperationOutcome.QuantityExceedsOpen, stale.Outcome);
        await AssertNothingCancelledAsync(order.Id);
    }

    [Fact]
    public async Task CancelOrderItems_AnItemOfAnotherOrder_IsNotFound_AndAShippedOrder_IsClosed_KK6_KK8()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2));
        (Order other, List<OrderItem> otherItems) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2));
        (Order shipped, List<OrderItem> shippedItems) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2));

        CustomerOperationResult foreign = await OperateAsync((o, t) => o.CancelOrderItemsAsync(
            order.Id, [new CancelOrderItemsLine(items[0].Id, 1, 2), new CancelOrderItemsLine(otherItems[0].Id, 1, 2)], null, t));
        CustomerOperationResult closed = await OperateAsync((o, t) => o.CancelOrderItemsAsync(shipped.Id, [new CancelOrderItemsLine(shippedItems[0].Id, 1, 2)], null, t));

        Assert.Equal(CustomerOperationOutcome.NotFound, foreign.Outcome);
        Assert.Equal(CustomerOperationOutcome.Closed, closed.Outcome);
        await AssertNothingCancelledAsync(order.Id);
        await AssertNothingCancelledAsync(other.Id);
        await AssertNothingCancelledAsync(shipped.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(BadCheckDigits)]
    public async Task CancelOrderItems_OnThePaidTransfer_WithoutAValidIban_IsInvalidIban_AndNoLineIsWritten_KK8(string? iban)
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));

        CustomerOperationResult result = await OperateAsync((o, t) => o.CancelOrderItemsAsync(
            order.Id, [new CancelOrderItemsLine(items[0].Id, 1, 2), new CancelOrderItemsLine(items[1].Id, 1, 1)], iban, t));

        Assert.Equal((CustomerOperationOutcome.InvalidIban, true), (result.Outcome, result.Gate.IsRejected));
        await AssertNothingCancelledAsync(order.Id);
    }

    // ---- KK9: the two lines, the shipping fee ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CancelOrderItems_OnTheCard_StartsTheRefundItself_AndEveryPhysicalUnitCoveredPaysTheShippingFeeBack_KK9(bool reversed)
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));
        await ExecuteAsync($"UPDATE orders SET shipping_fee = 30, shipping_fee_vat_breakdown = '[]' WHERE id = '{order.Id}'");
        Dictionary<Guid, int> open = new() { [items[0].Id] = 2, [items[1].Id] = 1 };
        Dictionary<Guid, int> selected = new() { [items[0].Id] = 2, [items[1].Id] = 1 };

        List<CancelOrderItemsLine> lines = [new CancelOrderItemsLine(items[0].Id, 2, 2), new CancelOrderItemsLine(items[1].Id, 1, 1)];
        if (reversed)
        {
            lines.Reverse();
        }

        CustomerOperationResult result = await OperateAsync((o, t) => o.CancelOrderItemsAsync(order.Id, lines, null, t));

        // The screen said so before the last step (the rule home), and the refund line pays it: the units' amounts and the fee once.
        Assert.True(RefundPreview.CancellationRefundsShippingFee(open, selected));
        Assert.Equal(CustomerOperationOutcome.Applied, result.Outcome);
        await using AppDbContext reader = NewContext();
        List<Refund> refunds = await reader.Refunds.Where(r => r.OrderId == order.Id).ToListAsync(Ct);
        Assert.Equal(2, refunds.Count);
        Assert.All(refunds, r => Assert.Equal((PaymentMethod.Card, ActorKind.System), (r.PaymentMethod, r.ActorKind)));
        Assert.Equal(330m, refunds.Sum(r => r.Amount));
        Assert.Single(refunds, r => r.IsShippingFeeIncluded);

        // Of the records of one moment the fee goes to the smallest item id (K-1107), whichever order the lines came in.
        Guid smallest = items.Select(i => i.Id).OrderBy(id => id.ToString("N"), StringComparer.Ordinal).First();
        Assert.Equal(smallest, refunds.Single(r => r.IsShippingFeeIncluded).OrderItemId);
        foreach (Refund refund in refunds)
        {
            Assert.True(await reader.Jobs.AnyAsync(j => j.JobType == JobType.CardRefund && j.RelatedRecordId == refund.Id, Ct));
        }
    }

    [Fact]
    public async Task CancelOrderItems_OfSomeUnitsOnly_PaysNoShippingFee_KK9()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));
        await ExecuteAsync($"UPDATE orders SET shipping_fee = 30, shipping_fee_vat_breakdown = '[]' WHERE id = '{order.Id}'");

        CustomerOperationResult result = await OperateAsync((o, t) => o.CancelOrderItemsAsync(order.Id, [new CancelOrderItemsLine(items[0].Id, 2, 2)], null, t));

        Assert.False(RefundPreview.CancellationRefundsShippingFee(new Dictionary<Guid, int> { [items[0].Id] = 2, [items[1].Id] = 1 }, new Dictionary<Guid, int> { [items[0].Id] = 2 }));
        Assert.Equal(CustomerOperationOutcome.Applied, result.Outcome);
        await using AppDbContext reader = NewContext();
        Refund refund = await reader.Refunds.SingleAsync(r => r.OrderId == order.Id, Ct);
        Assert.Equal((200m, false), (refund.Amount, refund.IsShippingFeeIncluded));
    }

    [Fact]
    public async Task CancelOrderItems_OnTheTransfer_KeepsTheIbanOnTheRecords_ForTheFirmToProcess_KK9()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));

        CustomerOperationResult result = await OperateAsync((o, t) => o.CancelOrderItemsAsync(
            order.Id, [new CancelOrderItemsLine(items[0].Id, 1, 2), new CancelOrderItemsLine(items[1].Id, 1, 1)], "tr33 0006 1005 1978 6457 8413 26", t));

        Assert.Equal(CustomerOperationOutcome.Applied, result.Outcome);
        await using AppDbContext reader = NewContext();
        List<Cancellation> cancellations = await reader.Cancellations.Where(c => c.OrderItemId == items[0].Id || c.OrderItemId == items[1].Id).ToListAsync(Ct);
        Assert.Equal(2, cancellations.Count);
        foreach (Cancellation cancellation in cancellations)
        {
            Assert.Equal(ValidIban, (await reader.RefundIbans.SingleAsync(i => i.Id == cancellation.RefundIbanId, Ct)).Iban);
        }

        Assert.False(await reader.Refunds.AnyAsync(r => r.OrderId == order.Id, Ct));
    }

    [Fact]
    public async Task CancelOrderItems_TheLastUnitOfAPlacedPaidOrder_IsTheCustomersS3_TheOrderCancelled_ThePaymentAxisStays_D803()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 1));

        CustomerOperationResult result = await OperateAsync((o, t) => o.CancelOrderItemsAsync(order.Id, [new CancelOrderItemsLine(items[0].Id, 1, 1)], null, t));

        Assert.Equal(CustomerOperationOutcome.Applied, result.Outcome);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Paid), (saved.OrderStatus, saved.PaymentStatus));
        TransitionRecord s3 = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal(TransitionCode.S3, s3.TransitionCode);
        Cancellation cancellation = await reader.Cancellations.SingleAsync(c => c.OrderItemId == items[0].Id, Ct);
        Assert.Equal(s3.Id, cancellation.TransitionRecordId);
        Assert.Equal(1, await reader.DispatchRecords.CountAsync(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B7, Ct));
    }

    // ---- KK10: the termination ----

    [Fact]
    public async Task Terminate_CoversEveryOpenUnitOfTheUndeliveredPhysicalItems_AndPaysTheShippingFeeBack_KK10()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1), new Line(ProductType.Digital, 1));
        await ExecuteAsync($"UPDATE orders SET shipping_fee = 30, shipping_fee_vat_breakdown = '[]' WHERE id = '{order.Id}'");
        Assert.False((await RunAsync((u, t) => u.CancelItemAsync(new CancelItemRequest(order.Id, items[0].Id, 1), t))).IsRejected);
        _clock.SetUtcNow(Late);

        CustomerOperationResult result = await OperateAsync((o, t) => o.TerminateForLateDeliveryAsync(order.Id, null, t));

        Assert.Equal(CustomerOperationOutcome.Applied, result.Outcome);
        await using AppDbContext reader = NewContext();
        List<LateDeliveryTermination> terminations = await reader.LateDeliveryTerminations.Where(t => t.OrderItemId == items[0].Id || t.OrderItemId == items[1].Id).ToListAsync(Ct);
        Assert.Equal([(items[0].Id, 1), (items[1].Id, 1)], terminations.OrderBy(t => t.OrderItemId == items[1].Id).Select(t => (t.OrderItemId, t.Quantity)));
        Assert.False(await reader.LateDeliveryTerminations.AnyAsync(t => t.OrderItemId == items[2].Id, Ct));

        // Two refunds of the termination and one of the earlier cancellation; the termination always pays the fee back (K-796).
        List<Refund> refunds = await reader.Refunds.Where(r => r.OrderId == order.Id && r.RefundType == RefundType.LateDeliveryTermination).ToListAsync(Ct);
        Assert.Equal(230m, refunds.Sum(r => r.Amount));
        Assert.Single(refunds, r => r.IsShippingFeeIncluded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(BadCheckDigits)]
    public async Task Terminate_OnThePaidTransfer_WithoutAValidIban_IsInvalidIban_AndWritesNothing_KK10(string? iban)
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2));
        _clock.SetUtcNow(Late);

        CustomerOperationResult result = await OperateAsync((o, t) => o.TerminateForLateDeliveryAsync(order.Id, iban, t));

        Assert.Equal((CustomerOperationOutcome.InvalidIban, true), (result.Outcome, result.Gate.IsRejected));
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.LateDeliveryTerminations.AnyAsync(Ct));
        Assert.Equal(OrderStatus.Preparing, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
    }

    [Fact]
    public async Task Terminate_OnThePaidTransfer_KeepsTheCustomersIbanOnTheRecord_AndOpensNoIbanRequest_D873_KK10()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2));
        _clock.SetUtcNow(Late);

        CustomerOperationResult result = await OperateAsync((o, t) => o.TerminateForLateDeliveryAsync(order.Id, ValidIban, t));

        Assert.Equal(CustomerOperationOutcome.Applied, result.Outcome);
        await using AppDbContext reader = NewContext();
        LateDeliveryTermination termination = await reader.LateDeliveryTerminations.SingleAsync(t => t.OrderItemId == items[0].Id, Ct);
        Assert.Equal(2, termination.Quantity);
        Assert.Equal(ValidIban, (await reader.RefundIbans.SingleAsync(i => i.Id == termination.RefundIbanId, Ct)).Iban);
        Assert.False(await reader.RefundIbanRequests.AnyAsync(r => r.OrderItemId == items[0].Id, Ct));
    }

    [Fact]
    public async Task Terminate_BeforeTheLimit_IsClosed_AndWritesNothing_KK6()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2));

        CustomerOperationResult result = await OperateAsync((o, t) => o.TerminateForLateDeliveryAsync(order.Id, null, t));

        Assert.Equal(CustomerOperationOutcome.Closed, result.Outcome);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.LateDeliveryTerminations.AnyAsync(Ct));
    }

    // ---- KK1, KK2, KK3: the IBAN field ----

    [Fact]
    public async Task SaveRefundIban_CorrectsTheIbanOnFileInPlace_IgnoringBlanks_UntilTheRefundIsProcessed_KK1()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2));
        Assert.Equal(CustomerOperationOutcome.Applied, (await OperateAsync((o, t) => o.CancelOrderItemsAsync(order.Id, [new CancelOrderItemsLine(items[0].Id, 1, 2)], ValidIban, t))).Outcome);
        Guid ibanId;
        await using (AppDbContext before = NewContext())
        {
            ibanId = (await before.Cancellations.SingleAsync(c => c.OrderItemId == items[0].Id, Ct)).RefundIbanId!.Value;
        }

        RefundIbanFieldView field = Assert.Single(await FieldsAsync(order.Id));
        Assert.Equal((items[0].Id, ValidIban, true, false), (field.OrderItemId, field.Iban, field.IsCorrection, field.IsOptional));
        int dispatches = await CountDispatchesAsync(order.Id);

        CustomerOperationResult result = await OperateAsync((o, t) => o.SaveRefundIbanAsync(order.Id, items[0].Id, " tr32 0010 0099 9990 1234 5678 90 ", t));

        Assert.Equal(CustomerOperationOutcome.Applied, result.Outcome);
        await using AppDbContext reader = NewContext();
        Assert.Equal(CorrectedIban, (await reader.RefundIbans.SingleAsync(i => i.Id == ibanId, Ct)).Iban);
        Assert.Equal(dispatches, await CountDispatchesAsync(order.Id));
    }

    [Fact]
    public async Task SaveRefundIban_ABadIban_AClosedField_AndAForeignItem_AreRefused_AndWriteNothing_KK2()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2));
        (Order other, List<OrderItem> otherItems) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2));
        Assert.Equal(CustomerOperationOutcome.Applied, (await OperateAsync((o, t) => o.CancelOrderItemsAsync(order.Id, [new CancelOrderItemsLine(items[0].Id, 1, 2)], ValidIban, t))).Outcome);

        CustomerOperationResult invalid = await OperateAsync((o, t) => o.SaveRefundIbanAsync(order.Id, items[0].Id, BadCheckDigits, t));
        CustomerOperationResult closed = await OperateAsync((o, t) => o.SaveRefundIbanAsync(other.Id, otherItems[0].Id, CorrectedIban, t));
        CustomerOperationResult itemless = await OperateAsync((o, t) => o.SaveRefundIbanAsync(order.Id, null, CorrectedIban, t));
        CustomerOperationResult foreign = await OperateAsync((o, t) => o.SaveRefundIbanAsync(order.Id, otherItems[0].Id, CorrectedIban, t));

        Assert.Equal(
            [CustomerOperationOutcome.InvalidIban, CustomerOperationOutcome.Closed, CustomerOperationOutcome.Closed, CustomerOperationOutcome.NotFound],
            new[] { invalid.Outcome, closed.Outcome, itemless.Outcome, foreign.Outcome });
        await using AppDbContext reader = NewContext();
        Assert.Equal([ValidIban], await reader.RefundIbans.Select(i => i.Iban).ToListAsync(Ct));
    }

    [Fact]
    public async Task SaveRefundIban_AnswersTheFailedCardRefundsTransferPath_TheRefundTurnsToTheTransfer_AndNoEmailIsWritten_KK3()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));
        Assert.False((await RunAsync((u, t) => u.CancelItemByFirmAsync(
            new CancelItemByFirmRequest(order.Id, items[0].Id, 1, AdminCaller, 1, true, CancellationReason.OutOfStock, null, false), t))).IsRejected);
        Guid refundId = await FailLatestCardRequestAsync(order.Id);
        Assert.False((await RunAsync((u, t) => u.OpenRefundTransferPathAsync(new OpenRefundTransferPathRequest(order.Id, refundId, AdminCaller, 2), t))).IsRejected);
        RefundIbanFieldView field = Assert.Single(await FieldsAsync(order.Id));
        Assert.Equal((items[0].Id, (RefundIbanRequestReason?)RefundIbanRequestReason.CardRefundFailed, true), (field.OrderItemId, field.Reason, field.IsOptional));
        int dispatches = await CountDispatchesAsync(order.Id);

        CustomerOperationResult result = await OperateAsync((o, t) => o.SaveRefundIbanAsync(order.Id, items[0].Id, ValidIban, t));

        Assert.Equal(CustomerOperationOutcome.Applied, result.Outcome);
        await using AppDbContext reader = NewContext();
        Assert.Equal(PaymentMethod.BankTransfer, (await reader.Refunds.SingleAsync(r => r.Id == refundId, Ct)).PaymentMethod);
        Assert.Equal(dispatches, await CountDispatchesAsync(order.Id));
    }

    // ---- helpers ----

    private async Task<CustomerOperationScreen?> ScreenAsync(Guid orderId, CustomerScreen screen)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICustomerOperations>().ReadScreenAsync(orderId, screen, Ct);
    }

    private async Task<IReadOnlyList<RefundIbanFieldView>> FieldsAsync(Guid orderId)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICustomerOperations>().ReadRefundIbanFieldsAsync(orderId, Ct);
    }

    /// <summary>The customer's write in one transaction the way the page's runner runs it: a rejected result rolls everything back.</summary>
    private async Task<CustomerOperationResult> OperateAsync(Func<ICustomerOperations, CancellationToken, Task<CustomerOperationResult>> work)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ICustomerOperations operations = scope.ServiceProvider.GetRequiredService<ICustomerOperations>();
        CustomerOperationResult? outcome = null;
        await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                outcome = await work(operations, token);
                return outcome.Gate;
            },
            Ct);
        return outcome!;
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task AssertNothingCancelledAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        Guid[] itemIds = [.. await reader.OrderItems.Where(i => i.OrderId == orderId).Select(i => i.Id).ToListAsync(Ct)];
        Assert.False(await reader.Cancellations.AnyAsync(c => itemIds.Contains(c.OrderItemId), Ct));
        Assert.False(await reader.Refunds.AnyAsync(r => r.OrderId == orderId, Ct));
        Assert.False(await reader.RefundIbans.AnyAsync(i => i.OrderId == orderId, Ct));
    }

    private async Task<int> CountDispatchesAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.CountAsync(d => d.OrderId == orderId, Ct);
    }

    /// <summary>The order's system card refund gets a failed latest request (the job's outcome; 06 4.34) and its job is done.</summary>
    private async Task<Guid> FailLatestCardRequestAsync(Guid orderId)
    {
        Guid refundId;
        await using (AppDbContext reader = NewContext())
        {
            refundId = await reader.Refunds.Where(r => r.OrderId == orderId).Select(r => r.Id).SingleAsync(Ct);
        }

        _clock.Advance(TimeSpan.FromMinutes(1));
        RefundRecord record = RefundRecord.Sending(refundId, _clock.GetUtcNow());
        record.State = RefundRecordState.Failed;
        record.OutcomeAt = _clock.GetUtcNow();
        await SaveAsync(c => c.RefundRecords.Add(record));
        await ExecuteAsync($"UPDATE jobs SET job_state = 'Completed', completed_at = now() WHERE related_record_id = '{refundId}'");
        return refundId;
    }

    private async Task<Variant> SeedVariantAsync(int stock)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Ürün", $"musteri-islem-urun-{n}", ProductType.Physical, 20m, Start).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, $"MIS-{n}", 10m, Start, stock);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        return variant;
    }

    private async Task<(Order Order, List<OrderItem> Items)> SeedAsync(OrderStatus orderStatus, PaymentStatus paymentStatus, PaymentMethod method, params Line[] lines)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n % 100:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n + 512000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, true, "Ali@Example.com", 120m, 0m, Start.AddHours(1), Start);
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
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? ValidIban : null;
        List<OrderItem> items = [];
        foreach (Line line in lines)
        {
            OrderItem item = OrderItem.Create(order.Id, null, line.VariantId, line.Type, "Ürün", line.Quantity, 100m, 20m, 16.67m, Start);
            if (line.Type == ProductType.Digital)
            {
                item.DownloadAllowance = 5;
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

        // F-3 of the termination goes to the firm's contact e-mail (08 2.2.7): a fixed address here, without the Settings module's legal identity.
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

    /// <summary>One item of a seeded order: its type, units, variant and whether it carries the delivery mark.</summary>
    public sealed record Line(ProductType Type, int Quantity, Guid? VariantId = null, bool Delivered = false);

    private sealed class FixedCompanyContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>("firma-iletisim@example.com");
    }
}
