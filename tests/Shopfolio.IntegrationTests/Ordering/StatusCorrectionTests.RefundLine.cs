using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The refund line on the paid-order harness of the correction (T370; 02 7.2.8, 7.4.5; 06 4.29, 4.33, 4.34): the cancellation's card refund is started by the
/// system with its job and no IBAN field; the firm's cancellation on the transfer line opens the IBAN request with B-14; the customer's cancellation brings a
/// checked IBAN; the IBAN is corrected until processed and deleted with its disposal record when its last refund goes; "havale gerçekleşmedi" deletes it and
/// opens the field again with B-14; a failed card refund opens the retry and the transfer path until an IBAN is entered, a pending result closes both.
/// </summary>
public sealed partial class StatusCorrectionTests
{
    private const string ValidIban = "TR330006100519786457841326";

    [Fact]
    public async Task TheFirmsCancellation_OnTheCard_StartsTheRefundWithItsJob_AndOpensNoIbanField_KK1_KK2()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));

        OrderTransitionResult result = await CancelItemByFirmAsync(order.Id, items[0].Id, 1, 1);

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        Refund refund = await reader.Refunds.SingleAsync(r => r.OrderId == order.Id, Ct);
        Assert.Equal(
            (RefundType.Cancellation, PaymentMethod.Card, ActorKind.System, 100m, (Guid?)items[0].Id, (int?)1, result.ItemRecordId),
            (refund.RefundType, refund.PaymentMethod, refund.ActorKind, refund.Amount, refund.OrderItemId, refund.Quantity, refund.CancellationId));
        Assert.True(await reader.Jobs.AnyAsync(j => j.JobType == JobType.CardRefund && j.RelatedRecordId == refund.Id, Ct));
        Assert.False(await reader.RefundIbanRequests.AnyAsync(r => r.OrderItemId == items[0].Id, Ct));
        Assert.Equal(0, await CountDispatchesAsync(order.Id, DispatchType.B14));
    }

    [Fact]
    public async Task TheFirmsCancellation_OnTheTransfer_OpensTheIbanRequest_WithB14_AndWritesNoRefundYet_KK6()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));

        Assert.False((await CancelItemByFirmAsync(order.Id, items[0].Id, 1, 1)).IsRejected);
        Assert.False((await CancelItemByFirmAsync(order.Id, items[0].Id, 1, 2)).IsRejected);

        await using AppDbContext reader = NewContext();
        Assert.False(await reader.Refunds.AnyAsync(r => r.OrderId == order.Id, Ct));
        RefundIbanRequest request = await reader.RefundIbanRequests.SingleAsync(r => r.OrderItemId == items[0].Id, Ct);
        Assert.Equal((RefundIbanRequestReason.CancellationOrRemoval, (DateTimeOffset?)null, (DateTimeOffset?)null), (request.Reason, request.FulfilledAt, request.ClosedAt));

        // A second event on the item uses the open request: one B-14.
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B14));
    }

    [Fact]
    public async Task TheCustomersCancellation_OnTheTransfer_TakesACheckedIban_KK4()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));

        Assert.Equal(TransitionRefusal.InvalidInput, (await CancelItemAsync(order.Id, items[0].Id, null)).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await CancelItemAsync(order.Id, items[0].Id, "TR330006100519786457841327")).Refusal);

        OrderTransitionResult result = await CancelItemAsync(order.Id, items[0].Id, "tr33 0006 1005 1978 6457 8413 26");

        Assert.False(result.IsRejected);
        await using AppDbContext reader = NewContext();
        Cancellation cancellation = await reader.Cancellations.SingleAsync(c => c.Id == result.ItemRecordId, Ct);
        RefundIban iban = await reader.RefundIbans.SingleAsync(i => i.Id == cancellation.RefundIbanId, Ct);
        Assert.Equal(ValidIban, iban.Iban);
        Assert.False(await reader.Refunds.AnyAsync(r => r.OrderId == order.Id, Ct));
        Assert.Equal(0, await CountDispatchesAsync(order.Id, DispatchType.B14));
    }

    [Fact]
    public async Task TheIban_IsEnteredAndCorrected_UntilProcessed_ThenDeletedWithItsDisposalRecord_KK4_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));
        Assert.False((await CancelItemByFirmAsync(order.Id, items[0].Id, 1, 1)).IsRejected);
        Guid requestId = await OpenRequestIdAsync(items[0].Id);

        // The firm never enters it; the customer's text is checked for format and check digits.
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.EnterRefundIbanAsync(new EnterRefundIbanRequest(order.Id, requestId, "TR00"), t))).Refusal);
        Assert.False((await RunAsync((u, t) => u.EnterRefundIbanAsync(new EnterRefundIbanRequest(order.Id, requestId, ValidIban), t))).IsRejected);
        Guid ibanId;
        await using (AppDbContext reader = NewContext())
        {
            ibanId = (await reader.RefundIbanRequests.SingleAsync(r => r.Id == requestId, Ct)).RefundIbanId!.Value;
        }

        const string Corrected = "TR320010009999901234567890";
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.ChangeRefundIbanAsync(new ChangeRefundIbanRequest(order.Id, ibanId, "TR32001000999990123456789X"), t))).Refusal);
        OrderTransitionResult corrected = await RunAsync((u, t) => u.ChangeRefundIbanAsync(new ChangeRefundIbanRequest(order.Id, ibanId, Corrected), t));
        Assert.False(corrected.IsRejected);

        // The firm processes the refund to the IBAN in the field: the IBAN no other pending refund uses goes, with its disposal record.
        Guid cancellationId = await CancellationIdAsync(items[0].Id);
        Refund refund = Refund.ByAdmin(order.Id, RefundType.Cancellation, PaymentMethod.BankTransfer, 80m, _admin!.Id, _admin.Name, Start);
        refund.CancellationId = cancellationId;
        refund.OrderItemId = items[0].Id;
        refund.Quantity = 1;
        await SaveAsync(c => c.Refunds.Add(refund));
        int disposals = await CountRefundIbanDisposalsAsync();
        PaymentTransitionResult processed = await RunAsync((u, t) => u.ProcessTransferRefundAsync(
            new TransferRefundProcessing(order.Id, refund.Id, _admin.Id, _admin.Name, 4, true), t));

        Assert.False(processed.IsRejected);
        await using AppDbContext after = NewContext();
        Assert.False(await after.RefundIbans.AnyAsync(i => i.Id == ibanId, Ct));
        Assert.Equal(disposals + 1, await CountRefundIbanDisposalsAsync());
    }

    [Fact]
    public async Task TheIban_SharedByAPendingRefund_StaysUntilTheLastOneGoes_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));
        OrderTransitionResult first = await CancelItemAsync(order.Id, items[0].Id, ValidIban);
        Assert.False(first.IsRejected);
        Guid ibanId;
        await using (AppDbContext reader = NewContext())
        {
            ibanId = (await reader.Cancellations.SingleAsync(c => c.Id == first.ItemRecordId, Ct)).RefundIbanId!.Value;
        }

        // A second cancellation uses the same IBAN (seeded link): its refund is still pending when the first one is processed.
        OrderTransitionResult second = await CancelItemAsync(order.Id, items[0].Id, ValidIban);
        Assert.False(second.IsRejected);
        await ExecuteAsync($"UPDATE cancellations SET refund_iban_id = '{ibanId}' WHERE id = '{second.ItemRecordId}'");
        await ExecuteAsync($"DELETE FROM refund_ibans WHERE order_id = '{order.Id}' AND id <> '{ibanId}'");

        Refund refund = Refund.ByAdmin(order.Id, RefundType.Cancellation, PaymentMethod.BankTransfer, 80m, _admin!.Id, _admin.Name, Start);
        refund.CancellationId = first.ItemRecordId;
        refund.OrderItemId = items[0].Id;
        refund.Quantity = 1;
        await SaveAsync(c => c.Refunds.Add(refund));
        int version = await VersionAsync(order.Id);
        PaymentTransitionResult processed = await RunAsync((u, t) => u.ProcessTransferRefundAsync(
            new TransferRefundProcessing(order.Id, refund.Id, _admin.Id, _admin.Name, version, true), t));

        Assert.False(processed.IsRejected);
        await using AppDbContext after = NewContext();
        Assert.True(await after.RefundIbans.AnyAsync(i => i.Id == ibanId, Ct));
    }

    [Fact]
    public async Task TransferFailed_DeletesTheIban_OpensTheFieldAgain_AndSendsB14_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));
        OrderTransitionResult cancelled = await CancelItemAsync(order.Id, items[0].Id, ValidIban);
        Assert.False(cancelled.IsRejected);
        int disposals = await CountRefundIbanDisposalsAsync();

        OrderTransitionResult reported = await RunAsync((u, t) => u.ReportTransferFailedAsync(
            new ReportTransferFailedRequest(order.Id, items[0].Id, null, AdminCaller, 2), t));

        Assert.False(reported.IsRejected);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.RefundIbans.AnyAsync(i => i.OrderId == order.Id, Ct));
        Assert.Equal(disposals + 1, await CountRefundIbanDisposalsAsync());
        RefundIbanRequest request = await reader.RefundIbanRequests.SingleAsync(r => r.OrderItemId == items[0].Id && r.FulfilledAt == null && r.ClosedAt == null, Ct);
        Assert.Equal(RefundIbanRequestReason.TransferFailed, request.Reason);
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B14));
        Assert.Equal(PaymentStatus.Paid, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).PaymentStatus);
    }

    [Fact]
    public async Task AFailedCardRefund_IsRetried_OrTurnedToTheTransfer_AndAfterTheIbanTheRetryIsClosed_KK8()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));
        Assert.False((await CancelItemByFirmAsync(order.Id, items[0].Id, 1, 1)).IsRejected);
        Guid refundId = await SeedLatestCardRequestAsync(order.Id, RefundRecordState.Failed);

        Assert.False((await RunAsync((u, t) => u.RetryCardRefundAsync(new RetryCardRefundRequest(order.Id, refundId, AdminCaller, 2), t))).IsRejected);
        await using (AppDbContext reader = NewContext())
        {
            Assert.Equal(2, await reader.Jobs.CountAsync(j => j.JobType == JobType.CardRefund && j.RelatedRecordId == refundId, Ct));
        }

        // The retry's request failed too: the firm opens the transfer path (B-14); the payment axis stays.
        await SeedLatestCardRequestAsync(order.Id, RefundRecordState.Failed);
        Assert.False((await RunAsync((u, t) => u.OpenRefundTransferPathAsync(new OpenRefundTransferPathRequest(order.Id, refundId, AdminCaller, 3), t))).IsRejected);
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B14));
        Guid requestId;
        await using (AppDbContext reader = NewContext())
        {
            RefundIbanRequest request = await reader.RefundIbanRequests.SingleAsync(r => r.RefundId == refundId, Ct);
            Assert.Equal(RefundIbanRequestReason.CardRefundFailed, request.Reason);
            requestId = request.Id;
        }

        Assert.False((await RunAsync((u, t) => u.EnterRefundIbanAsync(new EnterRefundIbanRequest(order.Id, requestId, ValidIban), t))).IsRejected);

        await using AppDbContext after = NewContext();
        Assert.Equal(PaymentMethod.BankTransfer, (await after.Refunds.SingleAsync(r => r.Id == refundId, Ct)).PaymentMethod);
        Assert.Equal(PaymentStatus.Paid, (await after.Orders.SingleAsync(o => o.Id == order.Id, Ct)).PaymentStatus);
        Assert.Equal(
            TransitionRefusal.ConditionNotMet,
            (await RunAsync((u, t) => u.RetryCardRefundAsync(new RetryCardRefundRequest(order.Id, refundId, AdminCaller, 5), t))).Refusal);
    }

    [Fact]
    public async Task APendingCardResult_ClosesTheRetryAndTheTransferPath_KK9()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.Card, new Line(ProductType.Physical, 2), new Line(ProductType.Physical, 1));
        Assert.False((await CancelItemByFirmAsync(order.Id, items[0].Id, 1, 1)).IsRejected);
        Guid refundId = await SeedLatestCardRequestAsync(order.Id, RefundRecordState.OutcomePending);

        Assert.Equal(
            TransitionRefusal.ConditionNotMet,
            (await RunAsync((u, t) => u.RetryCardRefundAsync(new RetryCardRefundRequest(order.Id, refundId, AdminCaller, 2), t))).Refusal);
        Assert.Equal(
            TransitionRefusal.ConditionNotMet,
            (await RunAsync((u, t) => u.OpenRefundTransferPathAsync(new OpenRefundTransferPathRequest(order.Id, refundId, AdminCaller, 2), t))).Refusal);
        Assert.Equal(0, await CountDispatchesAsync(order.Id, DispatchType.B14));
    }

    private Task<OrderTransitionResult> CancelItemByFirmAsync(Guid orderId, Guid itemId, int quantity, int version) =>
        RunAsync((unit, token) => unit.CancelItemByFirmAsync(
            new CancelItemByFirmRequest(orderId, itemId, quantity, AdminCaller, version, true, CancellationReason.OutOfStock, null, false), token));

    private Task<OrderTransitionResult> CancelItemAsync(Guid orderId, Guid itemId, string? iban) =>
        RunAsync((unit, token) => unit.CancelItemAsync(new CancelItemRequest(orderId, itemId, 1, iban), token));

    private async Task<Guid> OpenRequestIdAsync(Guid itemId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.RefundIbanRequests.Where(r => r.OrderItemId == itemId && r.FulfilledAt == null && r.ClosedAt == null).Select(r => r.Id).SingleAsync(Ct);
    }

    private async Task<Guid> CancellationIdAsync(Guid itemId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Cancellations.Where(c => c.OrderItemId == itemId).Select(c => c.Id).SingleAsync(Ct);
    }

    private async Task<int> CountRefundIbanDisposalsAsync()
    {
        await using AppDbContext reader = NewContext();
        return await reader.DisposalRecords.CountAsync(d => d.DisposalRecordType == DisposalRecordType.RefundIban, Ct);
    }

    /// <summary>The order's system card refund gets a new latest request in the given state (the job's outcome; 06 4.34).</summary>
    private async Task<Guid> SeedLatestCardRequestAsync(Guid orderId, RefundRecordState state)
    {
        Guid refundId;
        await using (AppDbContext reader = NewContext())
        {
            refundId = await reader.Refunds.Where(r => r.OrderId == orderId).Select(r => r.Id).SingleAsync(Ct);
        }

        _clock.Advance(TimeSpan.FromMinutes(1));
        RefundRecord record = RefundRecord.Sending(refundId, _clock.GetUtcNow());
        record.State = state;
        record.OutcomeAt = state == RefundRecordState.Failed ? _clock.GetUtcNow() : null;
        await SaveAsync(c => c.RefundRecords.Add(record));

        // The job that wrote the request is done (its handler completes the row); a retry enqueues a new one.
        await ExecuteAsync($"UPDATE jobs SET job_state = 'Completed', completed_at = now() WHERE related_record_id = '{refundId}'");
        return refundId;
    }
}
