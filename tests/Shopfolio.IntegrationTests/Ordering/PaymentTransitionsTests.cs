using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
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

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The payment axis on a real PostgreSQL (T317 KK1-KK8; 03 1.3; 02 5.5; 05 4.5.3, 4.6): the card payment's one path, the transfer's "paid" mark, the
/// unpaid cancellation's two axes together, the refund's sent money moving Ö3-Ö5, the notices B-4, B-7, B-8 and F-4 and their recipients, the refund
/// whose outcome failed or is unknown, and the payment that comes to a cancelled order. The expected transitions are read from <see cref="TransitionTable"/>.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class PaymentTransitionsTests : IAsyncLifetime
{
    private const string ContactEmail = "Ali@Example.com";

    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly ItemAmounts Amounts = new(100m, 0m, 16.67m);

    private readonly CollectionDatabaseFixture _collection;

    private Admin? _admin;

    private int _counter;

    public PaymentTransitionsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, refund_records, refunds, payment_records, cancellations, internal_notes, transition_records, "
            + "order_items, orders, admins, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(_admin));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the white list and the terminal states ----

    [Theory]
    [InlineData(OrderStatus.Cancelled, PaymentStatus.Failed)]
    [InlineData(OrderStatus.Preparing, PaymentStatus.Paid)]
    public async Task MarkTransferPaid_OnAnOrderThatIsNotPending_IsRefused_AndWritesNothing_KK1(OrderStatus orderStatus, PaymentStatus paymentStatus)
    {
        (Order order, _) = await SeedOrderAsync(orderStatus, paymentStatus, PaymentMethod.BankTransfer);

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token));

        Assert.True(result.IsNotOpen);
        await AssertStateAsync(order.Id, orderStatus, paymentStatus, 1);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.PaymentRecords.AnyAsync(p => p.OrderId == order.Id, Ct));
    }

    [Fact]
    public async Task RefundSent_OnARefundedOrder_DoesNotLeaveTheTerminalState_KK1()
    {
        (Order order, OrderItem item) = await SeedOrderAsync(OrderStatus.Cancelled, PaymentStatus.Refunded);
        (_, Guid recordId) = await SeedCardRefundAsync(order, item, 40m, byAdmin: false);

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(order.Id, recordId, RefundRecordState.Succeeded), token));

        Assert.False(result.IsRejected);
        Assert.Empty(result.Applied);
        Assert.True(TransitionTable.IsTerminal(PaymentStatus.Refunded));
        await AssertStateAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Refunded, 2);
    }

    // ---- KK2: the card payment's one path ----

    [Fact]
    public async Task CardPayment_FromTheNotificationAndFromTheFinalQuery_AppliesO1AndS1ByTheSystem_TheRecordAndB4_KK2()
    {
        foreach (string source in new[] { "notification", "final-query" })
        {
            (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
            string transactionId = $"trx-{source}-{order.Id:N}";

            PaymentTransitionResult result = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, transactionId), token));

            Assert.Equal([TransitionCode.O1, TransitionCode.S1], result.Applied);
            PaymentTransition o1 = TransitionTable.FindPayment(TransitionCode.O1)!;
            ShipmentTransition s1 = TransitionTable.FindShipment(TransitionCode.S1)!;
            await AssertStateAsync(order.Id, s1.To, o1.To, 2);
            await using AppDbContext reader = NewContext();
            List<TransitionRecord> records = await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct);
            Assert.All(records, r => Assert.Equal(ActorKind.System, r.ActorKind));
            PaymentRecord payment = await reader.PaymentRecords.SingleAsync(p => p.OrderId == order.Id, Ct);
            Assert.Equal((transactionId, records.Single(r => r.TransitionCode == TransitionCode.O1).Id), (payment.ProviderTransactionId, payment.TransitionRecordId!.Value));
            DispatchRecord notice = await reader.DispatchRecords.SingleAsync(d => d.OrderId == order.Id, Ct);
            Assert.Equal((DispatchType.B4, ContactEmail), (notice.DispatchType, notice.RecipientEmail));
        }
    }

    [Fact]
    public async Task CardPayment_ThatCameAlready_DoesNothing_KK2()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-once"), token));

        PaymentTransitionResult again = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-once"), token));

        Assert.True(again.IsAlreadyApplied);
        Assert.False(again.IsRejected);
        await AssertStateAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid, 2);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.PaymentRecords.CountAsync(p => p.OrderId == order.Id, Ct));
        Assert.Equal(1, await reader.DispatchRecords.CountAsync(d => d.OrderId == order.Id, Ct));
    }

    [Fact]
    public async Task CardPayment_OfAnOrderWithoutAPhysicalItem_AppliesNoS1_AndATransferOrderIsRefused_KK2()
    {
        // No S1 without a physical item; the digital order's delivery mark ends its line with S2 in the same approval (T318; 03 1.4.2.1).
        (Order digital, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending, productType: ProductType.Digital);
        (Order transfer, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer);

        PaymentTransitionResult applied = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(digital.Id, "trx-digital"), token));
        PaymentTransitionResult refused = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(transfer.Id, "trx-transfer"), token));

        Assert.Equal([TransitionCode.O1, TransitionCode.S2], applied.Applied);
        await AssertStateAsync(digital.Id, OrderStatus.Delivered, PaymentStatus.Paid, 2);
        Assert.True(refused.IsNotOpen);
        await AssertStateAsync(transfer.Id, OrderStatus.Placed, PaymentStatus.Pending, 1);
    }

    // ---- KK3: the transfer's "paid" mark ----

    [Fact]
    public async Task MarkTransferPaid_WithoutTheConfirmation_IsRefused_AndWritesNothing_KK3()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer);

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1) with { IsIrreversibleConfirmed = false }, token));

        Assert.True(result.IsConfirmationRequired);
        await AssertStateAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending, 1);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.DispatchRecords.AnyAsync(d => d.OrderId == order.Id, Ct));
    }

    [Fact]
    public async Task MarkTransferPaid_Confirmed_IsO1ByTheAdministratorAndS1ByTheSystem_OneVersionRaise_TheRecordAndB4_KK3()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer);

        PaymentTransitionResult stale = await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 2), token));
        PaymentTransitionResult result = await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token));

        Assert.True(stale.IsVersionConflict);
        Assert.Equal([TransitionCode.O1, TransitionCode.S1], result.Applied);
        await AssertStateAsync(order.Id, TransitionTable.FindShipment(TransitionCode.S1)!.To, TransitionTable.FindPayment(TransitionCode.O1)!.To, 2);
        await using AppDbContext reader = NewContext();
        TransitionRecord o1 = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.O1, Ct);
        TransitionRecord s1 = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.S1, Ct);
        Assert.Equal((ActorKind.Admin, _admin!.Id, _admin.Name), (o1.ActorKind, o1.AdminId!.Value, o1.AdminName!));
        Assert.Equal((ActorKind.System, (Guid?)null), (s1.ActorKind, s1.AdminId));
        PaymentRecord payment = await reader.PaymentRecords.SingleAsync(p => p.OrderId == order.Id, Ct);
        Assert.Equal(((string?)null, (Guid?)o1.Id), (payment.ProviderTransactionId, payment.TransitionRecordId));
        DispatchRecord notice = await reader.DispatchRecords.SingleAsync(d => d.OrderId == order.Id, Ct);
        Assert.Equal((DispatchType.B4, ContactEmail), (notice.DispatchType, notice.RecipientEmail));
    }

    [Fact]
    public async Task MarkTransferPaid_OnACardOrder_IsRefused_KK3()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token));

        Assert.True(result.IsNotOpen);
        await AssertStateAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending, 1);
    }

    // ---- KK4: the unpaid cancellation, through the one cancellation of the unit (09 24 item 4) ----

    [Theory]
    [InlineData(ActorKind.Buyer)]
    [InlineData(ActorKind.Admin)]
    [InlineData(ActorKind.System)]
    public async Task CancelOrder_OfAnUnpaidOrder_ByAnyone_IsS3AndO2InOneOperation_WithTheReason_AndB7_KK4(ActorKind actor)
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(WholeCancellation(order.Id, actor), token));

        Assert.False(result.IsRejected);
        await AssertStateAsync(order.Id, TransitionTable.FindShipment(TransitionCode.S3)!.To, TransitionTable.FindPayment(TransitionCode.O2)!.To, 2);
        await using AppDbContext reader = NewContext();
        List<TransitionRecord> records = await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct);
        Assert.Equal(new[] { TransitionCode.S3, TransitionCode.O2 }.Order(), records.Select(r => r.TransitionCode).Order());
        Assert.Equal(records.Select(r => r.Id).Order(), result.TransitionRecordIds.Order());
        CancellationReason? firmReason = actor == ActorKind.Admin ? CancellationReason.OutOfStock : null;
        TransitionReason? systemReason = actor == ActorKind.System ? TransitionReason.PaymentWindowExpired : null;
        Assert.All(records, r => Assert.Equal((actor, firmReason, systemReason), (r.ActorKind, r.CancellationReason, r.TransitionReason)));
        DispatchRecord notice = await reader.DispatchRecords.SingleAsync(d => d.OrderId == order.Id, Ct);
        Assert.Equal((DispatchType.B7, ContactEmail), (notice.DispatchType, notice.RecipientEmail));
    }

    [Theory]
    [InlineData(ActorKind.Buyer)]
    [InlineData(ActorKind.Admin)]
    [InlineData(ActorKind.System)]
    public async Task CancelOrder_OfAPaidOrder_DoesNotApplyO2_KK4(ActorKind actor)
    {
        // The customer and the system cancel only an unpaid order as a whole; the firm's whole cancellation of a paid order is S3 alone - the payment
        // axis stays Ödendi and its refund is the cancellation task's (02 5.5).
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Paid);

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(WholeCancellation(order.Id, actor), token));

        bool firm = actor == ActorKind.Admin;
        Assert.Equal(firm ? null : TransitionRefusal.ConditionNotMet, result.Refusal);
        await AssertStateAsync(order.Id, firm ? OrderStatus.Cancelled : OrderStatus.Placed, PaymentStatus.Paid, firm ? 2 : 1);
        await using AppDbContext reader = NewContext();
        TransitionCode[] expected = firm ? [TransitionCode.S3] : [];
        Assert.Equal(expected, await reader.TransitionRecords.Where(t => t.OrderId == order.Id).Select(t => t.TransitionCode).ToListAsync(Ct));
    }

    [Fact]
    public async Task CancelOrder_OfAnUnpaidOrder_ByTheAdministratorWithoutTheConfirmation_IsRefused_AndWritesNothing_KK4()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(
            CancelOrderRequest.ByAdministrator(order.Id, AdminCaller(), 1, false, CancellationReason.OutOfStock), token));

        Assert.Equal(TransitionRefusal.ConfirmationRequired, result.Refusal);
        await AssertNothingWrittenAsync(order.Id);
    }

    [Theory]
    [InlineData(CancellationReason.OutOfStock, "not for this reason")]
    [InlineData(CancellationReason.Other, null)]
    [InlineData(CancellationReason.Other, "   ")]
    public async Task CancelOrder_OfAnUnpaidOrder_ByTheAdministrator_WithANoteOutsideOther_OrOtherWithoutANote_IsRefused_KK4(CancellationReason reason, string? note)
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);

        OrderTransitionResult result = await RunAsync((unit, token) => unit.CancelOrderAsync(
            CancelOrderRequest.ByAdministrator(order.Id, AdminCaller(), 1, true, reason, note), token));

        Assert.Equal(TransitionRefusal.InvalidInput, result.Refusal);
        await AssertNothingWrittenAsync(order.Id);
    }

    [Fact]
    public async Task CancelOrder_OfAnUnpaidOrder_ByTheAdministratorWithoutTheVersion_OrAStaleOne_OrByTheSystemAsAClosure_IsRefused_KK4()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);

        OrderTransitionResult noVersion = await RunAsync((unit, token) => unit.CancelOrderAsync(WholeCancellation(order.Id, ActorKind.Admin) with { ExpectedVersion = null }, token));
        OrderTransitionResult staleScreen = await RunAsync((unit, token) => unit.CancelOrderAsync(WholeCancellation(order.Id, ActorKind.Admin) with { ExpectedVersion = 2 }, token));
        OrderTransitionResult closure = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.BySystem(order.Id, TransitionReason.Closure), token));

        Assert.Equal(TransitionRefusal.InvalidInput, noVersion.Refusal);
        Assert.Equal(TransitionRefusal.VersionConflict, staleScreen.Refusal);
        Assert.Equal(TransitionRefusal.ReasonNotAllowed, closure.Refusal);
        await AssertNothingWrittenAsync(order.Id);
    }

    // ---- KK5: the refund's sent money ----

    [Theory]
    [InlineData(40, PaymentStatus.PartiallyRefunded)]
    [InlineData(120, PaymentStatus.Refunded)]
    public async Task CardRefundSucceeded_OnAPaidOrder_IsO3ForAPartAndO4ForTheWhole_KK5(int amount, PaymentStatus expected)
    {
        (Order order, OrderItem item) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        (_, Guid recordId) = await SeedCardRefundAsync(order, item, amount, byAdmin: false);

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(order.Id, recordId, RefundRecordState.Succeeded), token));

        PaymentTransition row = TransitionTable.FindPayment(PaymentStatus.Paid, expected)!;
        Assert.Equal([row.Code], result.Applied);
        await AssertStateAsync(order.Id, OrderStatus.Preparing, expected, 2);
        await using AppDbContext reader = NewContext();
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((row.Code, ActorKind.System), (record.TransitionCode, record.ActorKind));
        RefundRecord saved = await reader.RefundRecords.SingleAsync(r => r.Id == recordId, Ct);
        Assert.Equal(RefundRecordState.Succeeded, saved.State);
        Assert.NotNull(saved.OutcomeAt);
    }

    [Theory]
    [InlineData(80, PaymentStatus.Refunded)]
    [InlineData(30, PaymentStatus.PartiallyRefunded)]
    public async Task CardRefundSucceeded_OnAPartlyRefundedOrder_IsO5ForTheRest_AndNoTransitionBelowIt_KK5(int amount, PaymentStatus expected)
    {
        (Order order, OrderItem item) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.PartiallyRefunded);
        await SeedTransferRefundAsync(order, 40m, sent: true);
        (_, Guid recordId) = await SeedCardRefundAsync(order, item, amount, byAdmin: false);

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(order.Id, recordId, RefundRecordState.Succeeded), token));

        PaymentTransition? row = TransitionTable.FindPayment(PaymentStatus.PartiallyRefunded, expected);
        TransitionCode[] expectedCodes = row is null ? [] : [row.Code];
        Assert.Equal(expectedCodes, result.Applied);
        await AssertStateAsync(order.Id, OrderStatus.Preparing, expected, 2, records: row is null ? 0 : 1);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.DispatchRecords.CountAsync(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B8, Ct));
    }

    [Fact]
    public async Task ProcessTransferRefund_IsTheAdministratorsO3_WithTheConfirmationAndTheVersion_AndB8WithoutF4_KK5()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer);
        Guid refundId = await SeedTransferRefundAsync(order, 50m, sent: false);

        PaymentTransitionResult unconfirmed = await RunAsync((unit, token) => unit.ProcessTransferRefundAsync(Processing(order.Id, refundId, 1) with { IsIrreversibleConfirmed = false }, token));
        PaymentTransitionResult stale = await RunAsync((unit, token) => unit.ProcessTransferRefundAsync(Processing(order.Id, refundId, 2), token));
        PaymentTransitionResult result = await RunAsync((unit, token) => unit.ProcessTransferRefundAsync(Processing(order.Id, refundId, 1), token));
        PaymentTransitionResult again = await RunAsync((unit, token) => unit.ProcessTransferRefundAsync(Processing(order.Id, refundId, 2), token));

        Assert.True(unconfirmed.IsConfirmationRequired);
        Assert.True(stale.IsVersionConflict);
        Assert.Equal([TransitionTable.FindPayment(PaymentStatus.Paid, PaymentStatus.PartiallyRefunded)!.Code], result.Applied);
        Assert.True(again.IsNotOpen);
        await AssertStateAsync(order.Id, OrderStatus.Preparing, PaymentStatus.PartiallyRefunded, 2);
        await using AppDbContext reader = NewContext();
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((ActorKind.Admin, (Guid?)_admin!.Id), (record.ActorKind, record.AdminId));
        Assert.NotNull((await reader.Refunds.SingleAsync(r => r.Id == refundId, Ct)).SentAt);
        DispatchRecord notice = await reader.DispatchRecords.SingleAsync(Ct);
        Assert.Equal((DispatchType.B8, ContactEmail), (notice.DispatchType, notice.RecipientEmail));
    }

    // ---- KK6: the recipients of B-8 and F-4 ----

    [Fact]
    public async Task SystemCardRefundSucceeded_WritesB8ToTheCustomer_AndF4ToTheFirm_KK6()
    {
        (Order order, OrderItem item) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        (Guid refundId, Guid recordId) = await SeedCardRefundAsync(order, item, 40m, byAdmin: false);

        await RunAsync((unit, token) => unit.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(order.Id, recordId, RefundRecordState.Succeeded), token));

        await using AppDbContext reader = NewContext();
        DispatchRecord customer = await reader.DispatchRecords.SingleAsync(d => d.DispatchType == DispatchType.B8, Ct);
        DispatchRecord firm = await reader.DispatchRecords.SingleAsync(d => d.DispatchType == DispatchType.F4, Ct);
        Assert.Equal((ContactEmail, (Guid?)order.Id), (customer.RecipientEmail, customer.OrderId));
        Assert.Equal((OrderingTestServices.CompanyEmail, (DispatchRelatedRecordType?)DispatchRelatedRecordType.Refund, (Guid?)refundId), (firm.RecipientEmail, firm.RelatedRecordType, firm.RelatedRecordId));
    }

    [Fact]
    public async Task FirmRetryOrAdministratorsCardRefundSucceeded_WritesB8Only_KK6()
    {
        (Order retried, OrderItem retriedItem) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        (_, Guid retryRecordId) = await SeedCardRefundAsync(retried, retriedItem, 40m, byAdmin: false, failedBefore: true);
        (Order byAdmin, OrderItem byAdminItem) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        (_, Guid adminRecordId) = await SeedCardRefundAsync(byAdmin, byAdminItem, 40m, byAdmin: true);

        await RunAsync((unit, token) => unit.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(retried.Id, retryRecordId, RefundRecordState.Succeeded), token));
        PaymentTransitionResult adminResult = await RunAsync((unit, token) => unit.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(byAdmin.Id, adminRecordId, RefundRecordState.Succeeded), token));

        await using AppDbContext reader = NewContext();
        Assert.Equal([DispatchType.B8, DispatchType.B8], await reader.DispatchRecords.Select(d => d.DispatchType).ToListAsync(Ct));
        Assert.Equal([TransitionCode.O3], adminResult.Applied);
        TransitionRecord adminRecord = await reader.TransitionRecords.SingleAsync(t => t.OrderId == byAdmin.Id, Ct);
        Assert.Equal((ActorKind.Admin, (Guid?)_admin!.Id), (adminRecord.ActorKind, adminRecord.AdminId));
    }

    // ---- KK7: the refund whose outcome failed or is unknown ----

    [Theory]
    [InlineData(RefundRecordState.Failed)]
    [InlineData(RefundRecordState.OutcomePending)]
    public async Task CardRefundNotSentOrUnknown_LeavesTheAxis_SendsNoNotice_AndTheMarkFalls_KK7(RefundRecordState outcome)
    {
        (Order order, OrderItem item) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        (Guid refundId, Guid recordId) = await SeedCardRefundAsync(order, item, 40m, byAdmin: false);

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(order.Id, recordId, outcome), token));
        RefundMarks marks = await ReadMarksAsync(order.Id);

        Assert.False(result.IsRejected);
        Assert.Empty(result.Applied);
        await AssertStateAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid, 2, records: 0);
        Assert.Equal(new RefundMarks(outcome == RefundRecordState.Failed, outcome == RefundRecordState.OutcomePending), marks);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.DispatchRecords.AnyAsync(Ct));
        Refund refund = await reader.Refunds.SingleAsync(r => r.Id == refundId, Ct);
        Assert.Null(refund.SentAt);
        Assert.Equal(outcome, (await reader.RefundRecords.SingleAsync(r => r.Id == recordId, Ct)).State);
    }

    [Fact]
    public async Task CardRefundOutcome_OfARecordThatIsNotOpen_IsRefused()
    {
        (Order order, OrderItem item) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        (_, Guid recordId) = await SeedCardRefundAsync(order, item, 40m, byAdmin: false);
        await RunAsync((unit, token) => unit.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(order.Id, recordId, RefundRecordState.Failed), token));

        PaymentTransitionResult again = await RunAsync((unit, token) => unit.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(order.Id, recordId, RefundRecordState.Succeeded), token));

        Assert.True(again.IsNotOpen);
        await AssertStateAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid, 2, records: 0);
    }

    // ---- KK8: the payment that must not stay ----

    [Theory]
    [InlineData(OrderStatus.Cancelled, PaymentStatus.Failed)]
    [InlineData(OrderStatus.Preparing, PaymentStatus.Paid)]
    public async Task CardPayment_ToACancelledOrder_OrASecondOneToAPaidOrder_IsRecordedAndRefunded_TheAxesStay_KK8(OrderStatus orderStatus, PaymentStatus paymentStatus)
    {
        (Order order, _) = await SeedOrderAsync(orderStatus, paymentStatus);

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-late"), token));
        PaymentTransitionResult again = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-late"), token));

        Assert.Empty(result.Applied);
        Assert.True(again.IsAlreadyApplied);
        await AssertStateAsync(order.Id, orderStatus, paymentStatus, 2, records: 0);
        await using AppDbContext reader = NewContext();
        PaymentRecord payment = await reader.PaymentRecords.SingleAsync(p => p.OrderId == order.Id, Ct);
        Assert.Equal(("trx-late", (Guid?)null), (payment.ProviderTransactionId!, payment.TransitionRecordId));
        Refund refund = await reader.Refunds.SingleAsync(r => r.OrderId == order.Id, Ct);
        Assert.Equal((refund.Id, RefundType.LatePayment, order.Total, PaymentMethod.Card, ActorKind.System, (Guid?)payment.Id), (result.LatePaymentRefundId!.Value, refund.RefundType, refund.Amount, refund.PaymentMethod, refund.ActorKind, refund.PaymentRecordId));
        Job job = await reader.Jobs.SingleAsync(Ct);
        Assert.Equal((JobType.CardRefund, (Guid?)refund.Id), (job.JobType, job.RelatedRecordId));
    }

    [Fact]
    public async Task LatePaymentRefundSucceeded_MovesNoAxis_AndWritesB8AndF4_KK8()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Cancelled, PaymentStatus.Failed);
        PaymentTransitionResult late = await RunAsync((unit, token) => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(order.Id, "trx-late-refund"), token));
        RefundRecord sending = RefundRecord.Sending(late.LatePaymentRefundId!.Value, Now);
        await SaveAsync(c => c.RefundRecords.Add(sending));

        PaymentTransitionResult result = await RunAsync((unit, token) => unit.ApplyCardRefundOutcomeAsync(new CardRefundOutcome(order.Id, sending.Id, RefundRecordState.Succeeded), token));

        Assert.Empty(result.Applied);
        await AssertStateAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Failed, 3, records: 0);
        await using AppDbContext reader = NewContext();
        Assert.Equal([DispatchType.B8, DispatchType.F4], (await reader.DispatchRecords.Select(d => d.DispatchType).ToListAsync(Ct)).Order());
    }

    // ---- Helpers ----

    private TransferPaidMark Mark(Guid orderId, int version) => new(orderId, _admin!.Id, _admin.Name, version, true);

    private TransferRefundProcessing Processing(Guid orderId, Guid refundId, int version) => new(orderId, refundId, _admin!.Id, _admin.Name, version, true);

    private TransitionAdmin AdminCaller() => new(_admin!.Id, _admin.Name);

    /// <summary>The whole-order cancellation of each actor: the customer's without a reason, the firm's confirmed with a closed-list reason, the system's with its kind.</summary>
    private CancelOrderRequest WholeCancellation(Guid orderId, ActorKind actor) => actor switch
    {
        ActorKind.Admin => CancelOrderRequest.ByAdministrator(orderId, AdminCaller(), 1, true, CancellationReason.OutOfStock),
        ActorKind.System => CancelOrderRequest.BySystem(orderId, TransitionReason.PaymentWindowExpired),
        _ => CancelOrderRequest.ByCustomer(orderId),
    };

    /// <summary>A refused unpaid cancellation leaves the order as seeded: no axis moved, no version raise, no record and no notice.</summary>
    private async Task AssertNothingWrittenAsync(Guid orderId)
    {
        await AssertStateAsync(orderId, OrderStatus.Placed, PaymentStatus.Pending, 1, records: 0);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.DispatchRecords.AnyAsync(d => d.OrderId == orderId, Ct));
    }

    private async Task AssertStateAsync(Guid orderId, OrderStatus orderStatus, PaymentStatus paymentStatus, int version, int? records = null)
    {
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((orderStatus, paymentStatus, version), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        if (records is { } expected)
        {
            Assert.Equal(expected, await reader.TransitionRecords.CountAsync(t => t.OrderId == orderId, Ct));
        }
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<RefundMarks> ReadMarksAsync(Guid orderId)
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IPaymentModule>().ReadRefundMarksAsync(orderId, Ct);
    }

    /// <summary>
    /// A card refund with an open request: the system's refund of a customer's item cancellation, or the administrator's amount-based refund on the
    /// card. <paramref name="failedBefore"/> writes a failed request first, so the open one is the firm's retry.
    /// </summary>
    private async Task<(Guid RefundId, Guid RecordId)> SeedCardRefundAsync(Order order, OrderItem item, decimal amount, bool byAdmin, bool failedBefore = false)
    {
        Refund refund;
        Cancellation? cancellation = null;
        if (byAdmin)
        {
            refund = Refund.ByAdmin(order.Id, RefundType.AmountBased, PaymentMethod.Card, amount, _admin!.Id, _admin.Name, Now);
        }
        else
        {
            cancellation = Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now);
            refund = Refund.BySystem(order.Id, RefundType.Cancellation, PaymentMethod.Card, amount, Now);
            refund.CancellationId = cancellation.Id;
            refund.OrderItemId = item.Id;
            refund.Quantity = 1;
        }

        List<RefundRecord> records = [];
        if (failedBefore)
        {
            RefundRecord failed = RefundRecord.Sending(refund.Id, Now.AddMinutes(-10));
            failed.State = RefundRecordState.Failed;
            failed.OutcomeAt = Now.AddMinutes(-9);
            records.Add(failed);
        }

        RefundRecord open = RefundRecord.Sending(refund.Id, Now);
        records.Add(open);
        await SaveAsync(c =>
        {
            if (cancellation is not null)
            {
                c.Cancellations.Add(cancellation);
            }

            c.Refunds.Add(refund);
            c.RefundRecords.AddRange(records);
        });
        return (refund.Id, open.Id);
    }

    /// <summary>The administrator's amount-based refund to the customer's IBAN; <paramref name="sent"/> writes it as sent already.</summary>
    private async Task<Guid> SeedTransferRefundAsync(Order order, decimal amount, bool sent)
    {
        Refund refund = Refund.ByAdmin(order.Id, RefundType.AmountBased, PaymentMethod.BankTransfer, amount, _admin!.Id, _admin.Name, Now);
        refund.SentAt = sent ? Now : null;
        await SaveAsync(c => c.Refunds.Add(refund));
        return refund.Id;
    }

    private async Task<(Order Order, OrderItem Item)> SeedOrderAsync(
        OrderStatus orderStatus,
        PaymentStatus paymentStatus,
        PaymentMethod method = PaymentMethod.Card,
        ProductType productType = ProductType.Physical)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Now, "metin", Now).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, true, ContactEmail, 120m, 0m, Now.AddHours(1), Now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;
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
        // A digital item is bought once (the database's quantity rule), a physical one any number of times.
        int quantity = productType == ProductType.Digital ? 1 : 3;
        OrderItem item = OrderItem.Create(order.Id, null, null, productType, "Ürün", quantity, 100m, 20m, 16.67m, Now);
        if (productType == ProductType.Digital)
        {
            item.DownloadAllowance = 5;
        }

        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });

        // The states are set around the entity (it takes them only through the unit), the pair rule of the database still applies.
        await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        return (order, item);
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
}
