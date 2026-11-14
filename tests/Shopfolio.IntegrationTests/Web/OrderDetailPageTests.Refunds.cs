using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// E-37's refund steps (T529 KK1-KK10; 07 UC-131...UC-137) on the opened application: the record that causes the refund is the key, the amount and the IBAN are the server's, the transfer is sent with the
/// step and the card's job goes in the same transaction, the two 409s, the named 422s, the IBAN request's server reason, the card refund's two steps and the late success's warning.
/// </summary>
public sealed partial class OrderDetailPageTests
{
    private const string ValidRefundIban = "TR330006100519786457841326";

    // ---- KK1, KK2: the key, the confirmation, the transfer sent with the step, the IBAN gone; 404 ----

    [Fact]
    public async Task ProcessRefund_OnTheTransfer_RepeatsTheAmountAndTheIban_ThenSendsIt_MovesTheAxis_AndDeletesTheIban_KK1_KK2()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded seeded = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode screen, string html) = await GetAsync(client, Path(seeded), key);
        (HttpStatusCode ask, string window) = await PostAsync(client, Path(seeded, "ProcessRefund"), [("Input.CancellationId", seeded.CauseId.ToString("D")), ("Input.Version", "1")], key);
        (HttpStatusCode foreign, _) = await PostAsync(client, Path(seeded, "ProcessRefund"), [("Input.CancellationId", Guid.NewGuid().ToString("D")), ("Input.Version", "1")], key);
        (HttpStatusCode done, string? location) = await PostForLocationAsync(
            client, Path(seeded, "ProcessRefund"), [("Input.CancellationId", seeded.CauseId.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true"), ("Input.ShownIban", ValidRefundIban)], key);

        Assert.Equal(HttpStatusCode.OK, screen);
        Assert.Contains($">{OrderDetailModel.ProcessRefundLabel}</button>", html, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.CancellationId\" value=\"{seeded.CauseId:D}\"", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Contains("data-js=\"confirm-window\"", window, StringComparison.Ordinal);
        Assert.Contains("50,00", window, StringComparison.Ordinal);
        Assert.Contains("TR33 0006 1005 1978 6457 8413 26", window, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, foreign);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Equal(Path(seeded), location);

        await using AppDbContext db = NewContext();
        Refund refund = await db.Refunds.AsNoTracking().SingleAsync(r => r.OrderId == seeded.OrderId, Ct);
        Assert.Equal((RefundType.Cancellation, (Guid?)seeded.CauseId, PaymentMethod.BankTransfer, 50m, ActorKind.Admin), (refund.RefundType, refund.CancellationId, refund.PaymentMethod, refund.Amount, refund.ActorKind));
        Assert.NotNull(refund.SentAt);
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == seeded.OrderId, Ct);
        Assert.Equal((PaymentStatus.PartiallyRefunded, 2), (order.PaymentStatus, order.Version));
        Assert.False(await db.RefundIbans.AnyAsync(i => i.OrderId == seeded.OrderId, Ct));
        Assert.Equal(1, await db.DispatchRecords.CountAsync(d => d.OrderId == seeded.OrderId && d.DispatchType == DispatchType.B8, Ct));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == seeded.OrderId && a.Operation == nameof(AuditOperation.RefundManually), Ct));
        Assert.DoesNotContain(ValidRefundIban, System.Text.Json.JsonSerializer.Serialize((await db.AuditLogs.AsNoTracking().SingleAsync(a => a.TargetRecordId == seeded.OrderId, Ct)).Details), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProcessRefund_OnTheTransferWithoutAnIban_Is422RefundIbanMissing_AndTheScreenSaysTheIbanIsAwaited_KK2()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded seeded = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: false);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html) = await GetAsync(client, Path(seeded), key);
        (HttpStatusCode status, string answer) = await PostAsync(
            client, Path(seeded, "ProcessRefund"), [("Input.CancellationId", seeded.CauseId.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true")], key);

        Assert.Contains(OrderDetailModel.IbanAwaitedText, html, StringComparison.Ordinal);
        Assert.DoesNotContain($">{OrderDetailModel.ProcessRefundLabel}</button>", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains(OrderDetailModel.RefundIbanMissingText, answer, StringComparison.Ordinal);
        await AssertNothingWrittenAsync(seeded.OrderId, 1);
    }

    // ---- KK3: the card line - the refund row, its job, the audit row and the version in one transaction; no provider call ----

    [Fact]
    public async Task ProcessRefund_OnTheCard_WritesTheRefundItsJobTheAuditRowAndTheVersion_AndLeavesTheAxisToTheProvidersResult_KK3()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded seeded = await SeedPaidAsync(PaymentMethod.Card, withIban: false, withdrawal: true);
        await using WebApplicationFactory<Program> factory = Open(withJobRunner: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, Path(seeded, "ProcessRefund"), [("Input.WithdrawalId", seeded.CauseId.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true")], key);

        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext db = NewContext();
        Refund refund = await db.Refunds.AsNoTracking().SingleAsync(r => r.OrderId == seeded.OrderId, Ct);
        Assert.Equal((RefundType.Withdrawal, (Guid?)seeded.CauseId, PaymentMethod.Card, (DateTimeOffset?)null), (refund.RefundType, refund.WithdrawalId, refund.PaymentMethod, refund.SentAt));
        Assert.Equal(1, await db.Jobs.CountAsync(j => j.RelatedRecordId == refund.Id, Ct));
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == seeded.OrderId, Ct);
        Assert.Equal((PaymentStatus.Paid, 2), (order.PaymentStatus, order.Version));
        Assert.Equal(0, await db.DispatchRecords.CountAsync(d => d.OrderId == seeded.OrderId && d.DispatchType == DispatchType.B8, Ct));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == seeded.OrderId && a.Operation == nameof(AuditOperation.RefundManually), Ct));
    }

    // ---- KK4: the second sending and the IBAN corrected while the window was open are 409 ----

    [Fact]
    public async Task ProcessRefund_SentTwice_IsTheVersionsConflict_AndAnIbanCorrectedMeanwhile_Is409WithTheCurrentIban_KK4()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded twice = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: true);
        Seeded corrected = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string Name, string Value)[] confirmed = [("Input.CancellationId", twice.CauseId.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true"), ("Input.ShownIban", ValidRefundIban)];

        (HttpStatusCode first, _) = await PostForLocationAsync(client, Path(twice, "ProcessRefund"), confirmed, key);
        (HttpStatusCode second, _) = await PostAsync(client, Path(twice, "ProcessRefund"), confirmed, key);

        // The customer corrects the IBAN while the window is open: the correction raises the version (T370), the IBAN is the new one.
        const string NewIban = "TR320010009999901234567890";
        await ExecuteAsync($"UPDATE refund_ibans SET iban = '{NewIban}' WHERE order_id = '{corrected.OrderId}'; UPDATE orders SET version = version + 1 WHERE id = '{corrected.OrderId}'");
        (HttpStatusCode changed, string html) = await PostAsync(
            client, Path(corrected, "ProcessRefund"), [("Input.CancellationId", corrected.CauseId.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true"), ("Input.ShownIban", ValidRefundIban)], key);

        Assert.Equal(HttpStatusCode.SeeOther, first);
        Assert.Equal(HttpStatusCode.Conflict, second);
        Assert.Equal(HttpStatusCode.Conflict, changed);
        Assert.Contains(OrderDetailModel.RefundIbanChangedText, html, StringComparison.Ordinal);
        Assert.Contains("TR32 0010 0099 9990 1234 5678 90", html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Refunds.CountAsync(r => r.OrderId == twice.OrderId, Ct));
        Assert.False(await db.Refunds.AnyAsync(r => r.OrderId == corrected.OrderId, Ct));
    }

    // ---- KK6: "havale gerçekleşmedi" deletes the IBAN and opens the field; on a processed refund 422 ----

    [Fact]
    public async Task MarkBankTransferRefundFailed_DeletesTheIban_OpensTheField_AndOnAProcessedRefundIs422_KK6()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded failed = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: true);
        Seeded processed = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode reported, _) = await PostForLocationAsync(client, Path(failed, "MarkBankTransferRefundFailed"), [("Input.CancellationId", failed.CauseId.ToString("D")), ("Input.Version", "1")], key);
        (_, string screen) = await GetAsync(client, Path(failed), key);
        await PostForLocationAsync(client, Path(processed, "ProcessRefund"), [("Input.CancellationId", processed.CauseId.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true")], key);
        (HttpStatusCode late, _) = await PostAsync(client, Path(processed, "MarkBankTransferRefundFailed"), [("Input.CancellationId", processed.CauseId.ToString("D")), ("Input.Version", "2")], key);

        Assert.Equal(HttpStatusCode.SeeOther, reported);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, late);
        Assert.Contains(OrderDetailModel.IbanAwaitedText, screen, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.False(await db.RefundIbans.AnyAsync(i => i.OrderId == failed.OrderId, Ct));
        RefundIbanRequest request = await db.RefundIbanRequests.AsNoTracking().SingleAsync(r => r.OrderItemId == failed.ItemId, Ct);
        Assert.Equal((RefundIbanRequestReason.TransferFailed, (DateTimeOffset?)null), (request.Reason, request.FulfilledAt));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == failed.OrderId && a.Operation == nameof(AuditOperation.ReportRefundTransferNotCompleted), Ct));
    }

    // ---- KK7: the four 422s of the amount based refund; the closed item stays closed ----

    [Theory]
    [InlineData("", "FieldRequired")]
    [InlineData("0", "Exceeded")]
    [InlineData("50,01", "Exceeded")]
    [InlineData("abc", "Invalid")]
    public async Task ProcessAmountBasedRefund_AnEmptyZeroOrExceedingAmount_Is422_AndWritesNothing_KK7(string amount, string reason)
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded seeded = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        // Paid 100, the cancellation's 50 is owed: 50 is left, 50,01 is over it (K-1078; D-906).
        (HttpStatusCode status, string html) = await PostAsync(
            client, Path(seeded, "ProcessAmountBasedRefund"), [("Input.Amount", amount), ("Input.Version", "1"), ("Input.Confirmed", "true")], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains(
            reason switch { "FieldRequired" => OrderDetailModel.AmountMissingText, "Invalid" => OrderDetailModel.AmountInvalidText, _ => OrderDetailModel.AmountExceededText },
            html,
            StringComparison.Ordinal);
        await AssertNothingWrittenAsync(seeded.OrderId, 1);
    }

    [Fact]
    public async Task ProcessAmountBasedRefund_OnTheTransferWithoutAnIban_Is422_AndAValidOneIsSentWithTheStep_AndTheClosedItemStaysClosed_KK7()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded noIban = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: false);
        Seeded seeded = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode missing, string missingHtml) = await PostAsync(client, Path(noIban, "ProcessAmountBasedRefund"), [("Input.Amount", "10"), ("Input.Version", "1")], key);
        (HttpStatusCode ask, string window) = await PostAsync(client, Path(seeded, "ProcessAmountBasedRefund"), [("Input.Amount", "10,50"), ("Input.OrderItemId", seeded.ItemId.ToString("D")), ("Input.Version", "1")], key);
        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, Path(seeded, "ProcessAmountBasedRefund"), [("Input.Amount", "10,50"), ("Input.OrderItemId", seeded.ItemId.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true")], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, missing);
        Assert.Contains(OrderDetailModel.RefundIbanMissingText, missingHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Contains("10,50", window, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext db = NewContext();
        Refund refund = await db.Refunds.AsNoTracking().SingleAsync(r => r.OrderId == seeded.OrderId, Ct);
        Assert.Equal((RefundType.AmountBased, 10.50m, (Guid?)seeded.ItemId), (refund.RefundType, refund.Amount, refund.OrderItemId));
        Assert.NotNull(refund.SentAt);
        Assert.Equal(1, await db.Cancellations.CountAsync(c => c.OrderItemId == seeded.ItemId, Ct));
        Assert.Equal(PaymentStatus.PartiallyRefunded, (await db.Orders.AsNoTracking().SingleAsync(o => o.Id == seeded.OrderId, Ct)).PaymentStatus);
    }

    // ---- KK8: the IBAN request's reason is the server's; no item or not the transfer line is 422 ----

    [Fact]
    public async Task RequestRefundIban_WritesTheServersReasonAndB14_AndWithoutAnItemOrOnTheCardIs422_KK8()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded seeded = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: false, withCancellation: false);
        Seeded card = await SeedPaidAsync(PaymentMethod.Card, withIban: false, withCancellation: false);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode noItem, string noItemHtml) = await PostAsync(client, Path(seeded, "RequestRefundIban"), [("Input.Version", "1")], key);
        (HttpStatusCode onCard, _) = await PostAsync(client, Path(card, "RequestRefundIban"), [("Input.OrderItemId", card.ItemId.ToString("D")), ("Input.Version", "1")], key);
        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, Path(seeded, "RequestRefundIban"), [("Input.OrderItemId", seeded.ItemId.ToString("D")), ("Input.Version", "1"), ("Input.Reason", "CardRefundFailed")], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, noItem);
        Assert.Contains(OrderDetailModel.ItemNotSelectedText, noItemHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, onCard);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext db = NewContext();
        RefundIbanRequest request = await db.RefundIbanRequests.AsNoTracking().SingleAsync(r => r.OrderItemId == seeded.ItemId, Ct);
        Assert.Equal(RefundIbanRequestReason.AdminRequest, request.Reason);
        Assert.Equal(1, await db.DispatchRecords.CountAsync(d => d.OrderId == seeded.OrderId && d.DispatchType == DispatchType.B14, Ct));
        Assert.False(await db.RefundIbans.AnyAsync(i => i.OrderId == seeded.OrderId, Ct));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == seeded.OrderId && a.Operation == nameof(AuditOperation.OpenRefundIbanRequest), Ct));
    }

    // ---- KK9, KK10: the card refund's two steps ----

    [Fact]
    public async Task RetryCardRefund_AsksTheConfirmationWithTheChargebackWarning_ThenQueuesANewRequest_AndWhileTheResultIsAwaitedIs422_KK9()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Seeded failed, Guid failedRefund) = await SeedCardRefundAsync(RefundRecordState.Failed);
        (Seeded pending, Guid pendingRefund) = await SeedCardRefundAsync(RefundRecordState.OutcomePending);
        await using WebApplicationFactory<Program> factory = Open(withJobRunner: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string screen) = await GetAsync(client, Path(failed), key);
        (HttpStatusCode ask, string window) = await PostAsync(client, Path(failed, "RetryCardRefund"), [("Input.RefundId", failedRefund.ToString("D")), ("Input.Version", "1")], key);
        (HttpStatusCode done, _) = await PostForLocationAsync(client, Path(failed, "RetryCardRefund"), [("Input.RefundId", failedRefund.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true")], key);
        (HttpStatusCode closed, _) = await PostAsync(client, Path(pending, "RetryCardRefund"), [("Input.RefundId", pendingRefund.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true")], key);

        Assert.Contains(OrderDetailModel.CardRefundNotRealizedWarning, screen, StringComparison.Ordinal);
        Assert.Contains($">{OrderDetailModel.RetryCardRefundLabel}</button>", screen, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Contains(OrderDetailModel.CardRefundNotRealizedWarning, window, StringComparison.Ordinal);
        Assert.Contains("data-js=\"confirm-window\"", window, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closed);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Jobs.CountAsync(j => j.RelatedRecordId == failedRefund && j.JobState == JobState.Pending, Ct));
        Assert.Equal(0, await db.Jobs.CountAsync(j => j.RelatedRecordId == pendingRefund && j.JobState == JobState.Pending, Ct));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == failed.OrderId && a.Operation == nameof(AuditOperation.RetryCardRefund), Ct));
    }

    [Fact]
    public async Task SwitchRefundToBankTransfer_OpensTheCardRefundFailedRequestWithB14_AndWhileTheResultIsAwaitedIs422_KK10()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Seeded failed, Guid failedRefund) = await SeedCardRefundAsync(RefundRecordState.Failed);
        (Seeded pending, Guid pendingRefund) = await SeedCardRefundAsync(RefundRecordState.OutcomePending);
        await using WebApplicationFactory<Program> factory = Open(withJobRunner: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode done, _) = await PostForLocationAsync(client, Path(failed, "SwitchRefundToBankTransfer"), [("Input.RefundId", failedRefund.ToString("D")), ("Input.Version", "1")], key);
        (HttpStatusCode closed, _) = await PostAsync(client, Path(pending, "SwitchRefundToBankTransfer"), [("Input.RefundId", pendingRefund.ToString("D")), ("Input.Version", "1")], key);

        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closed);
        await using AppDbContext db = NewContext();
        RefundIbanRequest request = await db.RefundIbanRequests.AsNoTracking().SingleAsync(r => r.RefundId == failedRefund, Ct);
        Assert.Equal(RefundIbanRequestReason.CardRefundFailed, request.Reason);
        Assert.Equal(1, await db.DispatchRecords.CountAsync(d => d.OrderId == failed.OrderId && d.DispatchType == DispatchType.B14, Ct));
        Assert.False(await db.RefundIbanRequests.AnyAsync(r => r.RefundId == pendingRefund, Ct));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == failed.OrderId && a.Operation == nameof(AuditOperation.RefundByBankTransfer), Ct));
    }

    [Fact]
    public async Task MarkCardRefundFailed_TurnsTheAwaitedRequestFailed_SendsNoNotice_AndWhenNothingIsAwaitedIs422_AndALateSuccessShowsTheWarning_KK10()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Seeded pending, Guid pendingRefund) = await SeedCardRefundAsync(RefundRecordState.OutcomePending);
        (Seeded failed, Guid failedRefund) = await SeedCardRefundAsync(RefundRecordState.Failed);
        await using WebApplicationFactory<Program> factory = Open(withJobRunner: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string screen) = await GetAsync(client, Path(pending), key);
        (HttpStatusCode done, _) = await PostForLocationAsync(client, Path(pending, "MarkCardRefundFailed"), [("Input.RefundId", pendingRefund.ToString("D")), ("Input.Version", "1")], key);
        (HttpStatusCode closed, _) = await PostAsync(client, Path(failed, "MarkCardRefundFailed"), [("Input.RefundId", failedRefund.ToString("D")), ("Input.Version", "1")], key);

        Assert.Contains($">{OrderDetailModel.MarkCardRefundFailedLabel}</button>", screen, StringComparison.Ordinal);
        Assert.DoesNotContain($">{OrderDetailModel.RetryCardRefundLabel}</button>", screen, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closed);
        await using (AppDbContext db = NewContext())
        {
            RefundRecord record = await db.RefundRecords.AsNoTracking().SingleAsync(r => r.RefundId == pendingRefund, Ct);
            Assert.Equal((RefundRecordState.Failed, true), (record.State, record.IsMarkedFailedByAdmin));
            Assert.NotNull(record.OutcomeAt);
            Assert.Equal(0, await db.DispatchRecords.CountAsync(d => d.OrderId == pending.OrderId, Ct));
            Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == pending.OrderId && a.Operation == nameof(AuditOperation.MarkCardRefundNotCompleted), Ct));
        }

        // The provider's success after the mark is not applied (T405) and stays on the refund's line as a lasting warning (04 9.9.8; K-978).
        await ExecuteAsync($"UPDATE refund_records SET late_success_at = now() WHERE refund_id = '{pendingRefund}'");
        (_, string warned) = await GetAsync(client, Path(pending), key);
        Assert.Contains(OrderDetailModel.LateRefundSuccessWarning, warned, StringComparison.Ordinal);
    }

    // ---- Seeding ----

    /// <summary>A paid order (Preparing) of one physical item of two units at 50 TL, with - unless told otherwise - a buyer's cancellation of one unit (or a withdrawal) whose refund is owed.</summary>
    private async Task<Seeded> SeedPaidAsync(PaymentMethod method, bool withIban, bool withdrawal = false, bool withCancellation = true)
    {
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        OrderItem item = OrderItem.Create(orderId, null, null, ProductType.Physical, "Kapı", 2, 50m, 20m, 16.67m, now);
        Guid causeId = Guid.Empty;
        await using (AppDbContext db = NewContext())
        {
            db.OrderItems.Add(item);
            RefundIban? iban = withIban ? RefundIban.Create(orderId, ValidRefundIban, now) : null;
            if (iban is not null)
            {
                db.RefundIbans.Add(iban);
            }

            if (withdrawal)
            {
                Withdrawal declared = Withdrawal.ByBuyer(item.Id, 1, DateOnly.FromDateTime(now.UtcDateTime), null, new ItemAmounts(50m, 0m, 8.33m), iban?.Id, now);
                db.Withdrawals.Add(declared);
                causeId = declared.Id;
            }
            else if (withCancellation)
            {
                Cancellation cancellation = Cancellation.ByBuyer(item.Id, 1, new ItemAmounts(50m, 0m, 8.33m), iban?.Id, now);
                db.Cancellations.Add(cancellation);
                causeId = cancellation.Id;
            }

            await db.SaveChangesAsync(Ct);
        }

        string card = method == PaymentMethod.Card ? ", payment_method = 'Card', bank_transfer_iban = NULL" : string.Empty;
        await ExecuteAsync($"UPDATE orders SET order_status = 'Preparing', payment_status = 'Paid'{card} WHERE id = '{orderId}'");
        return new Seeded(orderId, item.Id, causeId);
    }

    /// <summary>A paid card order whose system refund of a cancellation has a latest request in the given state (06 4.34).</summary>
    private async Task<(Seeded Seeded, Guid RefundId)> SeedCardRefundAsync(RefundRecordState state)
    {
        Seeded seeded = await SeedPaidAsync(PaymentMethod.Card, withIban: false);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Refund refund = Refund.BySystem(seeded.OrderId, RefundType.Cancellation, PaymentMethod.Card, 50m, now);
        refund.CancellationId = seeded.CauseId;
        refund.OrderItemId = seeded.ItemId;
        refund.Quantity = 1;
        RefundRecord record = RefundRecord.Sending(refund.Id, now);
        record.State = state;
        record.OutcomeAt = state == RefundRecordState.Failed ? now : null;
        await using AppDbContext db = NewContext();
        db.Refunds.Add(refund);
        db.RefundRecords.Add(record);
        await db.SaveChangesAsync(Ct);
        return (seeded, refund.Id);
    }

    private async Task AssertNothingWrittenAsync(Guid orderId, int version)
    {
        await using AppDbContext db = NewContext();
        Assert.False(await db.Refunds.AnyAsync(r => r.OrderId == orderId, Ct));
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((PaymentStatus.Paid, version), (order.PaymentStatus, order.Version));
        Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private static string Path(Seeded seeded, string? handler = null) => handler is null ? $"{OrdersPath}/{seeded.OrderId:D}" : $"{OrdersPath}/{seeded.OrderId:D}?handler={handler}";

    private WebApplicationFactory<Program> Open(bool withJobRunner) =>
        new ShopfolioWebApplicationFactory(_database.Dsn, withJobRunner: withJobRunner).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(Pages.PanelOrdersModel).Assembly)));

    private sealed record Seeded(Guid OrderId, Guid ItemId, Guid CauseId);
}
