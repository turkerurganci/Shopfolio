using System.Globalization;
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

    private const string OtherRefundIban = "TR320010009999901234567890";

    private const string ThirdRefundIban = "TR290006400000112345678901";

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
        Assert.Contains(DisplayFormat.Amount(50m) + " tutarındaki", window, StringComparison.Ordinal);
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
    [InlineData("10,505", "Invalid")]
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

    // ---- UC-133: the form only while the processing step is open; an item that is not the order's is the item list's refusal ----

    [Fact]
    public async Task ProcessAmountBasedRefund_FormIsDrawnOnlyWhileProcessRefundIsOpen_AndAnItemNotOfTheOrderIs422WithTheItemListsMessage()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded noIban = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: false);
        Seeded seeded = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string awaiting) = await GetAsync(client, Path(noIban), key);
        (_, string open) = await GetAsync(client, Path(seeded), key);
        (HttpStatusCode foreign, string foreignHtml) = await PostAsync(
            client, Path(seeded, "ProcessAmountBasedRefund"), [("Input.Amount", "10"), ("Input.OrderItemId", noIban.ItemId.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true")], key);

        // Without an IBAN only the IBAN request is open (1.11.39 before 1.11.31): its form is drawn, the amount based refund's is not.
        Assert.Contains("id=\"request-refund-iban-form\"", awaiting, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"amount-based-refund-form\"", awaiting, StringComparison.Ordinal);
        Assert.Contains("id=\"amount-based-refund-form\"", open, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, foreign);
        Assert.Contains(OrderDetailModel.ItemNotOfOrderText, foreignHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(OrderDetailModel.ItemNotSelectedText, foreignHtml, StringComparison.Ordinal);
        await AssertNothingWrittenAsync(seeded.OrderId, 1);
    }

    // ---- D-878: the defect remedy's born transfer refund is sent from the page by the axis's one path ----

    [Fact]
    public async Task ProcessRefund_OnADefectRemedysBornTransferRefund_SendsIt_AndMovesTheAxis_D878()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded seeded = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: true, withCancellation: false);
        (DefectClaim claim, Refund remedy) = await SeedDefectRemedyRefundAsync(seeded);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string screen) = await GetAsync(client, Path(seeded), key);
        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, Path(seeded, "ProcessRefund"), [("Input.DefectClaimId", claim.Id.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true"), ("Input.ShownIban", ValidRefundIban)], key);

        Assert.Contains($"name=\"Input.DefectClaimId\" value=\"{claim.Id:D}\"", screen, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext after = NewContext();
        Refund sent = await after.Refunds.AsNoTracking().SingleAsync(r => r.Id == remedy.Id, Ct);
        Assert.NotNull(sent.SentAt);
        Assert.Equal(PaymentStatus.PartiallyRefunded, (await after.Orders.AsNoTracking().SingleAsync(o => o.Id == seeded.OrderId, Ct)).PaymentStatus);
        Assert.Equal(1, await after.DispatchRecords.CountAsync(d => d.OrderId == seeded.OrderId && d.DispatchType == DispatchType.B8, Ct));
    }

    // ---- KK6, D-912: the defect remedy's refund paid to the IBAN on the order's file - the screen and the step decide the same (D-907; 09 8.3 item 5) ----

    [Fact]
    public async Task MarkBankTransferRefundFailed_OnADefectRemedysRefundPaidToTheIbanOnFile_OnTheItemsRow_DeletesThatIban_AndOpensTheField_KK6_D912()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded seeded = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: true, withCancellation: false);
        (DefectClaim claim, Refund remedy) = await SeedDefectRemedyRefundAsync(seeded);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string screen) = await GetAsync(client, Path(seeded), key);
        (HttpStatusCode reported, _) = await PostForLocationAsync(
            client, Path(seeded, "MarkBankTransferRefundFailed"), [("Input.DefectClaimId", claim.Id.ToString("D")), ("Input.Version", "1")], key);
        (_, string after) = await GetAsync(client, Path(seeded), key);

        Assert.Contains("handler=MarkBankTransferRefundFailed", screen, StringComparison.Ordinal);

        // 04 9.9.6 (4), 9.9.21: the item's refund line and its steps stand on the item's row, next to its pending refund - not in the order's section.
        int row = screen.IndexOf($"id=\"kalem-{seeded.ItemId:D}\"", StringComparison.Ordinal);
        int line = screen.IndexOf($"id=\"geri-odeme-{claim.Id:D}\"", StringComparison.Ordinal);
        Assert.True(row >= 0 && line > row && line < screen.IndexOf("id=\"tutar\"", StringComparison.Ordinal));
        Assert.Equal(HttpStatusCode.SeeOther, reported);
        Assert.Contains(OrderDetailModel.IbanAwaitedText, after, StringComparison.Ordinal);
        Assert.DoesNotContain("handler=MarkBankTransferRefundFailed", after, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.False(await db.RefundIbans.AnyAsync(i => i.OrderId == seeded.OrderId, Ct));
        RefundIbanRequest request = await db.RefundIbanRequests.AsNoTracking().SingleAsync(r => r.OrderItemId == seeded.ItemId, Ct);
        Assert.Equal((RefundIbanRequestReason.TransferFailed, (DateTimeOffset?)null), (request.Reason, request.FulfilledAt));
        Assert.Null((await db.Refunds.AsNoTracking().SingleAsync(r => r.Id == remedy.Id, Ct)).SentAt);
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == seeded.OrderId && a.Operation == nameof(AuditOperation.ReportRefundTransferNotCompleted), Ct));
    }

    // ---- KK5: the binding to the item's open "mal dönmedi" closure; the later receipt bears it ----

    [Fact]
    public async Task ProcessAmountBasedRefund_IsBoundToTheChosenOpenClosure_AForeignOneIs422_AndTheLaterReceiptsRefundBearsTheBoundAmount_KK5()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded seeded = await SeedPaidAsync(PaymentMethod.Card, withIban: false, withdrawal: true);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateOnly today = DateOnly.FromDateTime(now.UtcDateTime);
        await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = now(), delivery_date = '{today.AddDays(-1):yyyy-MM-dd}' WHERE id = '{seeded.ItemId}'");
        NotReturnedClosure closure = NotReturnedClosure.Create(seeded.ItemId, seeded.CauseId, 1, adminId, "Yönetici", now);
        await using (AppDbContext db = NewContext())
        {
            db.NotReturnedClosures.Add(closure);
            await db.SaveChangesAsync(Ct);
        }

        // No job runner: the card refunds' jobs stay queued, so the order's version is the steps' alone.
        await using WebApplicationFactory<Program> factory = Open(withJobRunner: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        const decimal Bound = 30m;

        (_, string screen) = await GetAsync(client, Path(seeded), key);
        (HttpStatusCode foreign, string foreignHtml) = await PostAsync(
            client,
            Path(seeded, "ProcessAmountBasedRefund"),
            [("Input.Amount", "30"), ("Input.OrderItemId", seeded.ItemId.ToString("D")), ("Input.NotReturnedClosureId", Guid.NewGuid().ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true")],
            key);
        (HttpStatusCode noItem, _) = await PostAsync(
            client, Path(seeded, "ProcessAmountBasedRefund"), [("Input.Amount", "30"), ("Input.NotReturnedClosureId", closure.Id.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true")], key);

        Assert.Contains($"name=\"Input.NotReturnedClosureId\"", screen, StringComparison.Ordinal);
        Assert.Contains($"value=\"{closure.Id:D}\"", screen, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, foreign);
        Assert.Contains(OrderDetailModel.ClosureNotOpenText, foreignHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, noItem);
        await AssertNothingWrittenAsync(seeded.OrderId, 1);

        (HttpStatusCode done, _) = await PostForLocationAsync(
            client,
            Path(seeded, "ProcessAmountBasedRefund"),
            [("Input.Amount", "30"), ("Input.OrderItemId", seeded.ItemId.ToString("D")), ("Input.NotReturnedClosureId", closure.Id.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true")],
            key);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        Guid boundRefund;
        await using (AppDbContext db = NewContext())
        {
            Refund refund = await db.Refunds.AsNoTracking().SingleAsync(r => r.OrderId == seeded.OrderId, Ct);
            Assert.Equal((RefundType.AmountBased, Bound, (Guid?)closure.Id), (refund.RefundType, refund.Amount, refund.NotReturnedClosureId));
            boundRefund = refund.Id;

            // The goods come after all: the receipt reopens the closure (K-1024 (6)) and its refund is owed.
            NotReturnedClosure reopened = await db.NotReturnedClosures.SingleAsync(n => n.Id == closure.Id, Ct);
            reopened.ReopenedAt = now.AddMinutes(1);
            db.Returns.Add(Return.ForWithdrawal(seeded.ItemId, seeded.CauseId, 1, today, adminId, "Yönetici", now.AddMinutes(1)));
            await db.SaveChangesAsync(Ct);
        }

        Guid receiptId;
        int version;
        await using (AppDbContext db = NewContext())
        {
            receiptId = (await db.Returns.AsNoTracking().SingleAsync(r => r.OrderItemId == seeded.ItemId, Ct)).Id;
            version = (await db.Orders.AsNoTracking().SingleAsync(o => o.Id == seeded.OrderId, Ct)).Version;
        }

        (HttpStatusCode received, _) = await PostForLocationAsync(
            client, Path(seeded, "ProcessRefund"), [("Input.ReturnId", receiptId.ToString("D")), ("Input.Version", version.ToString(System.Globalization.CultureInfo.InvariantCulture)), ("Input.Confirmed", "true")], key);

        Assert.Equal(HttpStatusCode.SeeOther, received);
        await using AppDbContext after = NewContext();
        Refund receiptRefund = await after.Refunds.AsNoTracking().SingleAsync(r => r.OrderId == seeded.OrderId && r.Id != boundRefund, Ct);
        Order order = await after.Orders.AsNoTracking().SingleAsync(o => o.Id == seeded.OrderId, Ct);

        // K-1105: the receipt's 50 bears the bound 30 - the server deducts it; the fee, when the rule includes it, rides aside.
        Assert.Equal(50m - Bound + (receiptRefund.IsShippingFeeIncluded ? order.ShippingFee : 0m), receiptRefund.Amount);
        Assert.Equal(receiptId, receiptRefund.ReturnId);
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
    public async Task RetryCardRefund_AsksTheConfirmation_TheChargebackWarningStaysOnTheLineOnly_ThenQueuesANewRequest_AndWhileTheResultIsAwaitedIs422_KK9()
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
        Assert.Contains("data-js=\"confirm-window\"", window, StringComparison.Ordinal);

        // 12 12.13 K-1514 (3): U4 stays on the line under the window, and the window does not repeat it.
        int dialog = window.IndexOf("data-js=\"confirm-window\"", StringComparison.Ordinal);
        string dialogHtml = window[dialog..window.IndexOf("</dialog>", dialog, StringComparison.Ordinal)];
        Assert.DoesNotContain(OrderDetailModel.CardRefundNotRealizedWarning, dialogHtml, StringComparison.Ordinal);
        Assert.Equal(2, window.Split(OrderDetailModel.CardRefundNotRealizedWarning, StringSplitOptions.None).Length);
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
    public async Task SwitchRefundToBankTransfer_PutsTheOrderUnderTheIbanPendingMark_AndTheMarkGoesWhenTheRequestIsFulfilled_D874()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Seeded failed, Guid failedRefund) = await SeedCardRefundAsync(RefundRecordState.Failed);
        await using WebApplicationFactory<Program> factory = Open(withJobRunner: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string number;
        await using (AppDbContext db = NewContext())
        {
            number = (await db.Orders.AsNoTracking().SingleAsync(o => o.Id == failed.OrderId, Ct)).OrderNumber;
        }

        string marked = $"{OrdersPath}?search={Uri.EscapeDataString(number)}&pendingWork={nameof(PanelOrderWork.IbanPendingMark)}";
        (_, string before) = await GetAsync(client, marked, key);
        (HttpStatusCode done, _) = await PostForLocationAsync(client, Path(failed, "SwitchRefundToBankTransfer"), [("Input.RefundId", failedRefund.ToString("D")), ("Input.Version", "1")], key);
        (_, string pending) = await GetAsync(client, marked, key);

        // The customer's IBAN fulfils the refund's request (the request stays, its fulfilment closes the mark; K-1075, K-675).
        await ExecuteAsync($"UPDATE refund_iban_requests SET fulfilled_at = now() WHERE refund_id = '{failedRefund}'");
        (_, string fulfilled) = await GetAsync(client, marked, key);

        Assert.Equal(HttpStatusCode.SeeOther, done);
        // The row links the order by its key; the search box repeats the number, so the key is what tells the row is listed.
        string row = failed.OrderId.ToString("D");
        Assert.DoesNotContain(row, before, StringComparison.Ordinal);
        Assert.Contains(row, pending, StringComparison.Ordinal);
        Assert.DoesNotContain(row, fulfilled, StringComparison.Ordinal);
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

    // ---- Düzeltme 2: the itemless born transfer refund (04 9.9.21) and its own IBAN (02 7.4.5; D-907) - the line, the window and the steps name the same IBAN ----

    [Fact]
    public async Task ProcessRefund_OnTheLatePaymentsRefundTurnedToTheTransfer_ByThePaymentRecord_RepeatsItsOwnIbanNotTheItems_ThenSendsIt_KK1_KK4()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        TurnedRefunds seeded = await SeedTurnedRefundsAsync(adminId, latePayment: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string screen) = await GetAsync(client, Path(seeded.Order), key);
        (HttpStatusCode ask, string window) = await PostAsync(client, Path(seeded.Order, "ProcessRefund"), [("Input.PaymentRecordId", seeded.ItemlessKey.ToString("D")), ("Input.Version", "1")], key);
        (HttpStatusCode done, string? location) = await PostForLocationAsync(
            client, Path(seeded.Order, "ProcessRefund"), [("Input.PaymentRecordId", seeded.ItemlessKey.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true"), ("Input.ShownIban", OtherRefundIban)], key);

        // The line: the refund's own IBAN (B), not the first on the order's file (the item's A); the item's line keeps its own.
        Assert.Equal(DisplayFormat.Iban(OtherRefundIban), LineIbanOf(screen, seeded.ItemlessKey));
        Assert.Equal(DisplayFormat.Iban(ValidRefundIban), LineIbanOf(screen, seeded.ItemKey));
        Assert.Contains($"name=\"Input.PaymentRecordId\" value=\"{seeded.ItemlessKey:D}\"", screen, StringComparison.Ordinal);

        // The window (KK1): the amount and the same IBAN; the IBAN the conflict compares (KK4) is that one too.
        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Contains(DisplayFormat.Amount(20m) + " tutarındaki", window, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.ShownIban\" value=\"{OtherRefundIban}\"", window, StringComparison.Ordinal);
        Assert.DoesNotContain($"name=\"Input.ShownIban\" value=\"{ValidRefundIban}\"", window, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Equal(Path(seeded.Order), location);

        // Sent with the step; the IBAN's life then deletes B and keeps the item's pending A (02 7.4.5).
        await using AppDbContext db = NewContext();
        Assert.NotNull((await db.Refunds.AsNoTracking().SingleAsync(r => r.Id == seeded.ItemlessRefundId, Ct)).SentAt);
        Assert.Null((await db.Refunds.AsNoTracking().SingleAsync(r => r.Id == seeded.ItemRefundId, Ct)).SentAt);
        Assert.Equal(ValidRefundIban, Assert.Single(await db.RefundIbans.AsNoTracking().Where(i => i.OrderId == seeded.Order.OrderId).Select(i => i.Iban).ToListAsync(Ct)));
        Assert.Equal(1, await db.DispatchRecords.CountAsync(d => d.OrderId == seeded.Order.OrderId && d.DispatchType == DispatchType.B8, Ct));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == seeded.Order.OrderId && a.Operation == nameof(AuditOperation.RefundManually), Ct));
    }

    [Fact]
    public async Task ProcessRefund_OnAnItemlessAmountBasedRefundTurnedToTheTransfer_ByTheRefundId_RepeatsItsOwnIban_ThenSendsIt_KK1()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        TurnedRefunds seeded = await SeedTurnedRefundsAsync(adminId, latePayment: false);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string screen) = await GetAsync(client, Path(seeded.Order), key);
        (HttpStatusCode ask, string window) = await PostAsync(client, Path(seeded.Order, "ProcessRefund"), [("Input.RefundId", seeded.ItemlessKey.ToString("D")), ("Input.Version", "1")], key);
        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, Path(seeded.Order, "ProcessRefund"), [("Input.RefundId", seeded.ItemlessKey.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true"), ("Input.ShownIban", OtherRefundIban)], key);

        Assert.Equal(seeded.ItemlessRefundId, seeded.ItemlessKey);
        Assert.Equal(DisplayFormat.Iban(OtherRefundIban), LineIbanOf(screen, seeded.ItemlessKey));
        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Contains(DisplayFormat.Amount(20m) + " tutarındaki", window, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.ShownIban\" value=\"{OtherRefundIban}\"", window, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext db = NewContext();
        Assert.NotNull((await db.Refunds.AsNoTracking().SingleAsync(r => r.Id == seeded.ItemlessRefundId, Ct)).SentAt);
        Assert.Equal(PaymentStatus.PartiallyRefunded, (await db.Orders.AsNoTracking().SingleAsync(o => o.Id == seeded.Order.OrderId, Ct)).PaymentStatus);
    }

    [Fact]
    public async Task MarkBankTransferRefundFailed_OnTheLatePaymentsRefundTurnedToTheTransfer_OnACardOrder_DeletesItsOwnIban_AndOpensTheItemlessRequest_KK6_D1563()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        TurnedRefunds seeded = await SeedTurnedRefundsAsync(adminId, latePayment: true, withCancellation: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string screen) = await GetAsync(client, Path(seeded.Order), key);

        // The card order's owed record has no transfer to fail: the step is closed on it, whatever IBAN is on file (the screen draws nothing there either).
        (HttpStatusCode owed, _) = await PostAsync(
            client, Path(seeded.Order, "MarkBankTransferRefundFailed"), [("Input.CancellationId", seeded.Order.CauseId.ToString("D")), ("Input.Version", "1")], key);
        (HttpStatusCode reported, _) = await PostForLocationAsync(
            client, Path(seeded.Order, "MarkBankTransferRefundFailed"), [("Input.PaymentRecordId", seeded.ItemlessKey.ToString("D")), ("Input.Version", "1")], key);
        (_, string after) = await GetAsync(client, Path(seeded.Order), key);
        (HttpStatusCode again, _) = await PostAsync(
            client, Path(seeded.Order, "MarkBankTransferRefundFailed"), [("Input.PaymentRecordId", seeded.ItemlessKey.ToString("D")), ("Input.Version", "2")], key);

        int line = screen.IndexOf($"id=\"geri-odeme-{seeded.ItemlessKey:D}\"", StringComparison.Ordinal);
        int next = screen.IndexOf("id=\"geri-odeme-", line + 1, StringComparison.Ordinal);
        Assert.Contains("handler=MarkBankTransferRefundFailed", screen[line..(next > line ? next : screen.Length)], StringComparison.Ordinal);
        Assert.Equal(DisplayFormat.Iban(OtherRefundIban), LineIbanOf(screen, seeded.ItemlessKey));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, owed);
        Assert.Equal(HttpStatusCode.SeeOther, reported);

        // VAL-A912 (K-1131 (2)): the line's own IBAN (B) goes and the line waits for the new one - not the item's A, which stays on its own line; with no IBAN the step
        // is closed on the screen and in the POST alike. The new request is itemless, TransferFailed, on the same refund.
        int waiting = after.IndexOf($"id=\"geri-odeme-{seeded.ItemlessKey:D}\"", StringComparison.Ordinal);
        int beyond = after.IndexOf("id=\"geri-odeme-", waiting + 1, StringComparison.Ordinal);
        string awaited = after[waiting..(beyond > waiting ? beyond : after.Length)];
        Assert.Null(LineIbanOf(after, seeded.ItemlessKey));
        Assert.Contains(OrderDetailModel.IbanAwaitedText, awaited, StringComparison.Ordinal);
        Assert.DoesNotContain("handler=MarkBankTransferRefundFailed", awaited, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, again);
        Assert.Equal(DisplayFormat.Iban(ValidRefundIban), LineIbanOf(after, seeded.ItemKey));
        await using AppDbContext db = NewContext();
        Assert.Equal(ValidRefundIban, Assert.Single(await db.RefundIbans.AsNoTracking().Where(i => i.OrderId == seeded.Order.OrderId).Select(i => i.Iban).ToListAsync(Ct)));
        RefundIbanRequest request = await db.RefundIbanRequests.AsNoTracking().SingleAsync(r => r.RefundId == seeded.ItemlessRefundId && r.FulfilledAt == null && r.ClosedAt == null, Ct);
        Assert.Equal((RefundIbanRequestReason.TransferFailed, (Guid?)null), (request.Reason, request.OrderItemId));
        Assert.Null((await db.Refunds.AsNoTracking().SingleAsync(r => r.Id == seeded.ItemlessRefundId, Ct)).SentAt);
        Assert.Equal(1, await db.DispatchRecords.CountAsync(d => d.OrderId == seeded.Order.OrderId && d.DispatchType == DispatchType.B14, Ct));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == seeded.Order.OrderId && a.Operation == nameof(AuditOperation.ReportRefundTransferNotCompleted), Ct));
    }

    // ---- Düzeltme 3: one rule for the IBAN a refund is paid to (02 7.4.5; D-907; K-356) - the line, the window, the POSTs and the IBAN's life, with several IBANs on file ----

    [Fact]
    public async Task MarkBankTransferRefundFailed_OnTheItem_WhileAnotherRefundsIbanIsOnFile_TheItemsLineWaits_ItsStepsAre422_AndTheOtherIbanGoesWithItsOwnRefund_KK6()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        TurnedRefunds seeded = await SeedTurnedRefundsAsync(adminId, latePayment: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string screen) = await GetAsync(client, Path(seeded.Order), key);
        (HttpStatusCode reported, _) = await PostForLocationAsync(
            client, Path(seeded.Order, "MarkBankTransferRefundFailed"), [("Input.DefectClaimId", seeded.ItemKey.ToString("D")), ("Input.Version", "1")], key);
        (_, string after) = await GetAsync(client, Path(seeded.Order), key);
        (HttpStatusCode window, string refused) = await PostAsync(client, Path(seeded.Order, "ProcessRefund"), [("Input.DefectClaimId", seeded.ItemKey.ToString("D")), ("Input.Version", "2")], key);
        (HttpStatusCode again, _) = await PostAsync(
            client, Path(seeded.Order, "MarkBankTransferRefundFailed"), [("Input.DefectClaimId", seeded.ItemKey.ToString("D")), ("Input.Version", "2")], key);

        // Before: the item's refund is paid to A, the itemless one to its own B.
        Assert.Equal(DisplayFormat.Iban(ValidRefundIban), LineIbanOf(screen, seeded.ItemKey));
        Assert.Equal(DisplayFormat.Iban(OtherRefundIban), LineIbanOf(screen, seeded.ItemlessKey));
        Assert.Equal(HttpStatusCode.SeeOther, reported);

        // 02 7.4.5: the item is back to "IBAN bekleniyor" - not paid to B, the other refund's IBAN on file; no step on its line, the POSTs refuse alike.
        Assert.Null(LineIbanOf(after, seeded.ItemKey));
        Assert.Contains(OrderDetailModel.IbanAwaitedText, LineHtmlOf(after, seeded.ItemKey), StringComparison.Ordinal);
        Assert.DoesNotContain($"name=\"Input.DefectClaimId\" value=\"{seeded.ItemKey:D}\"", after, StringComparison.Ordinal);
        Assert.Equal(DisplayFormat.Iban(OtherRefundIban), LineIbanOf(after, seeded.ItemlessKey));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, window);
        Assert.Contains(OrderDetailModel.RefundIbanMissingText, refused, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, again);
        await using (AppDbContext db = NewContext())
        {
            Assert.Equal(OtherRefundIban, Assert.Single(await db.RefundIbans.AsNoTracking().Where(i => i.OrderId == seeded.Order.OrderId).Select(i => i.Iban).ToListAsync(Ct)));
            RefundIbanRequest request = await db.RefundIbanRequests.AsNoTracking().SingleAsync(r => r.OrderItemId == seeded.Order.ItemId && r.FulfilledAt == null && r.ClosedAt == null, Ct);
            Assert.Equal((RefundIbanRequestReason.TransferFailed, (Guid?)null), (request.Reason, request.RefundId));
        }

        // The IBAN's life: B goes with its own refund; the waiting item's refund keeps no IBAN.
        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, Path(seeded.Order, "ProcessRefund"), [("Input.PaymentRecordId", seeded.ItemlessKey.ToString("D")), ("Input.Version", "2"), ("Input.Confirmed", "true"), ("Input.ShownIban", OtherRefundIban)], key);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext later = NewContext();
        Assert.False(await later.RefundIbans.AnyAsync(i => i.OrderId == seeded.Order.OrderId, Ct));
        Assert.Null((await later.Refunds.AsNoTracking().SingleAsync(r => r.Id == seeded.ItemRefundId, Ct)).SentAt);
    }

    [Fact]
    public async Task ProcessRefund_WhileTheItemlessRefundWaitsForItsOwnIban_TheItemsRefundIsSentToItsIban_AndThatIbanIsDeleted_KK2_K356()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        TurnedRefunds seeded = await SeedTurnedRefundsAsync(adminId, latePayment: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode reported, _) = await PostForLocationAsync(
            client, Path(seeded.Order, "MarkBankTransferRefundFailed"), [("Input.PaymentRecordId", seeded.ItemlessKey.ToString("D")), ("Input.Version", "1")], key);
        (_, string screen) = await GetAsync(client, Path(seeded.Order), key);
        (HttpStatusCode waiting, _) = await PostAsync(client, Path(seeded.Order, "ProcessRefund"), [("Input.PaymentRecordId", seeded.ItemlessKey.ToString("D")), ("Input.Version", "2")], key);
        (HttpStatusCode ask, string window) = await PostAsync(client, Path(seeded.Order, "ProcessRefund"), [("Input.DefectClaimId", seeded.ItemKey.ToString("D")), ("Input.Version", "2")], key);
        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, Path(seeded.Order, "ProcessRefund"), [("Input.DefectClaimId", seeded.ItemKey.ToString("D")), ("Input.Version", "2"), ("Input.Confirmed", "true"), ("Input.ShownIban", ValidRefundIban)], key);
        (_, string after) = await GetAsync(client, Path(seeded.Order), key);

        // The itemless refund waits for its own IBAN (VAL-A912): no IBAN on its line, no step, 422; the item's line, window and POST name A.
        Assert.Equal(HttpStatusCode.SeeOther, reported);
        Assert.Null(LineIbanOf(screen, seeded.ItemlessKey));
        Assert.Contains(OrderDetailModel.IbanAwaitedText, LineHtmlOf(screen, seeded.ItemlessKey), StringComparison.Ordinal);
        Assert.DoesNotContain($"name=\"Input.PaymentRecordId\" value=\"{seeded.ItemlessKey:D}\"", screen, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, waiting);
        Assert.Equal(DisplayFormat.Iban(ValidRefundIban), LineIbanOf(screen, seeded.ItemKey));
        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Contains($"name=\"Input.ShownIban\" value=\"{ValidRefundIban}\"", window, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);

        // K-356: A goes with its last refund - the waiting refund is no IBAN's payee; the itemless line still waits, its request open.
        Assert.Null(LineIbanOf(after, seeded.ItemlessKey));
        Assert.Contains(OrderDetailModel.IbanAwaitedText, LineHtmlOf(after, seeded.ItemlessKey), StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.False(await db.RefundIbans.AnyAsync(i => i.OrderId == seeded.Order.OrderId, Ct));
        Assert.NotNull((await db.Refunds.AsNoTracking().SingleAsync(r => r.Id == seeded.ItemRefundId, Ct)).SentAt);
        Assert.Null((await db.Refunds.AsNoTracking().SingleAsync(r => r.Id == seeded.ItemlessRefundId, Ct)).SentAt);
        Assert.True(await db.RefundIbanRequests.AnyAsync(r => r.RefundId == seeded.ItemlessRefundId && r.FulfilledAt == null && r.ClosedAt == null, Ct));
    }

    [Fact]
    public async Task ProcessRefund_TwoCancellationsOfOneItemWithTwoIbans_EachLineAndWindowNamesItsRecordsIban_AndTheSentOnesIbanGoesWhileTheOtherStays_KK1_KK2()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded order = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: false, withCancellation: false);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        // The customer cancels the two units one by one through the transition unit, each with its own IBAN (AddCancellationIban writes one per cancellation): A, then B.
        // The last open unit closes the order (S4), as in the product.
        Guid first = await CancelOneUnitAsync(factory, order, ValidRefundIban);
        Guid second = await CancelOneUnitAsync(factory, order, OtherRefundIban);
        Seeded seeded = order with { CauseId = first };
        string version = (await VersionOfAsync(order.OrderId)).ToString(CultureInfo.InvariantCulture);

        (_, string screen) = await GetAsync(client, Path(seeded), key);
        (HttpStatusCode ask, string window) = await PostAsync(client, Path(seeded, "ProcessRefund"), [("Input.CancellationId", second.ToString("D")), ("Input.Version", version)], key);
        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, Path(seeded, "ProcessRefund"), [("Input.CancellationId", second.ToString("D")), ("Input.Version", version), ("Input.Confirmed", "true"), ("Input.ShownIban", OtherRefundIban)], key);

        Assert.Equal(DisplayFormat.Iban(ValidRefundIban), LineIbanOf(screen, seeded.CauseId));
        Assert.Equal(DisplayFormat.Iban(OtherRefundIban), LineIbanOf(screen, second));
        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Contains($"name=\"Input.ShownIban\" value=\"{OtherRefundIban}\"", window, StringComparison.Ordinal);
        Assert.DoesNotContain($"name=\"Input.ShownIban\" value=\"{ValidRefundIban}\"", window, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using (AppDbContext db = NewContext())
        {
            // B goes with its refund; A stays for the first cancellation's pending refund.
            Assert.Equal(ValidRefundIban, Assert.Single(await db.RefundIbans.AsNoTracking().Where(i => i.OrderId == seeded.OrderId).Select(i => i.Iban).ToListAsync(Ct)));
            Assert.NotNull((await db.Refunds.AsNoTracking().SingleAsync(r => r.CancellationId == second, Ct)).SentAt);
        }

        // The first cancellation's line and window still name its own A (the IBAN its record brought), the one the life rule kept for it.
        string next = (await VersionOfAsync(order.OrderId)).ToString(CultureInfo.InvariantCulture);
        (_, string after) = await GetAsync(client, Path(seeded), key);
        (HttpStatusCode again, string firstWindow) = await PostAsync(client, Path(seeded, "ProcessRefund"), [("Input.CancellationId", first.ToString("D")), ("Input.Version", next)], key);

        Assert.Equal(DisplayFormat.Iban(ValidRefundIban), LineIbanOf(after, first));
        Assert.Equal(HttpStatusCode.OK, again);
        Assert.Contains($"name=\"Input.ShownIban\" value=\"{ValidRefundIban}\"", firstWindow, StringComparison.Ordinal);

        // The second sending: the first cancellation's refund goes to A, and A - now used by no pending refund - goes with it (K-356).
        (HttpStatusCode secondDone, string? secondLocation) = await PostForLocationAsync(
            client, Path(seeded, "ProcessRefund"), [("Input.CancellationId", first.ToString("D")), ("Input.Version", next), ("Input.Confirmed", "true"), ("Input.ShownIban", ValidRefundIban)], key);
        Assert.Equal(HttpStatusCode.SeeOther, secondDone);
        Assert.Equal(Path(seeded), secondLocation);
        await using AppDbContext last = NewContext();
        Assert.False(await last.RefundIbans.AnyAsync(i => i.OrderId == seeded.OrderId, Ct));
        Assert.NotNull((await last.Refunds.AsNoTracking().SingleAsync(r => r.CancellationId == first, Ct)).SentAt);
    }

    // ---- Düzeltme 3b: "havale gerçekleşmedi" and the file step read the one rule too - another refund's own IBAN is neither deleted nor paid to ----

    [Fact]
    public async Task MarkBankTransferRefundFailed_OnOneOfTwoRefundsOfAnItem_DeletesOnlyTheIbanItIsPaidTo_TheOtherRefundsOwnIbanStaysOnItsLine_AndGoesWithItsOwnRefund_KK6()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        TurnedRefunds seeded = await SeedTurnedRefundsAsync(adminId, latePayment: true, withCancellation: true);
        Guid otherKey = await SeedCancellationsTransferRefundAsync(seeded, adminId, ThirdRefundIban);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string screen) = await GetAsync(client, Path(seeded.Order), key);
        (HttpStatusCode reported, _) = await PostForLocationAsync(
            client, Path(seeded.Order, "MarkBankTransferRefundFailed"), [("Input.DefectClaimId", seeded.ItemKey.ToString("D")), ("Input.Version", "1")], key);
        (_, string after) = await GetAsync(client, Path(seeded.Order), key);

        // Before: the item's two refunds are paid to their own IBANs A and C, the itemless one to its own B.
        Assert.Equal(DisplayFormat.Iban(ValidRefundIban), LineIbanOf(screen, seeded.ItemKey));
        Assert.Equal(DisplayFormat.Iban(ThirdRefundIban), LineIbanOf(screen, otherKey));
        Assert.Equal(DisplayFormat.Iban(OtherRefundIban), LineIbanOf(screen, seeded.ItemlessKey));
        Assert.Equal(HttpStatusCode.SeeOther, reported);

        // Only A - the IBAN the failed refund was paid to - goes: its line waits; the other refund of the same item keeps its own C on its line, with its step.
        Assert.Null(LineIbanOf(after, seeded.ItemKey));
        Assert.Contains(OrderDetailModel.IbanAwaitedText, LineHtmlOf(after, seeded.ItemKey), StringComparison.Ordinal);
        Assert.Equal(DisplayFormat.Iban(ThirdRefundIban), LineIbanOf(after, otherKey));
        Assert.Contains($"name=\"Input.CancellationId\" value=\"{otherKey:D}\"", after, StringComparison.Ordinal);
        await using (AppDbContext db = NewContext())
        {
            Assert.Equal(
                new[] { OtherRefundIban, ThirdRefundIban }.Order(StringComparer.Ordinal),
                (await db.RefundIbans.AsNoTracking().Where(i => i.OrderId == seeded.Order.OrderId).Select(i => i.Iban).ToListAsync(Ct)).Order(StringComparer.Ordinal));
        }

        // The IBAN's life: C goes with its own refund, sent to it by the window that named it; B stays for the itemless refund.
        (HttpStatusCode ask, string window) = await PostAsync(client, Path(seeded.Order, "ProcessRefund"), [("Input.CancellationId", otherKey.ToString("D")), ("Input.Version", "2")], key);
        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, Path(seeded.Order, "ProcessRefund"), [("Input.CancellationId", otherKey.ToString("D")), ("Input.Version", "2"), ("Input.Confirmed", "true"), ("Input.ShownIban", ThirdRefundIban)], key);
        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Contains($"name=\"Input.ShownIban\" value=\"{ThirdRefundIban}\"", window, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext later = NewContext();
        Assert.Equal(OtherRefundIban, Assert.Single(await later.RefundIbans.AsNoTracking().Where(i => i.OrderId == seeded.Order.OrderId).Select(i => i.Iban).ToListAsync(Ct)));
        Assert.NotNull((await later.Refunds.AsNoTracking().SingleAsync(r => r.CancellationId == otherKey, Ct)).SentAt);
        Assert.Null((await later.Refunds.AsNoTracking().SingleAsync(r => r.Id == seeded.ItemRefundId, Ct)).SentAt);
    }

    [Fact]
    public async Task ProcessRefund_ARefundWithNoIbanOfItsOwn_IsNotPaidToAnotherRefundsOwnIbanOnTheFile_ItWaits_AndThatIbanGoesWithItsOwnRefund_KK1_KK2()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        TurnedRefunds seeded = await SeedTurnedRefundsAsync(adminId, latePayment: true, withCancellation: true);

        // A third transfer refund of the item - the cancellation's card refund on the transfer way - with no record, request or item IBAN: the file holds only A and B, each another refund's own.
        Guid bare = await SeedCancellationsTransferRefundAsync(seeded, adminId, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string screen) = await GetAsync(client, Path(seeded.Order), key);
        (HttpStatusCode waiting, string refused) = await PostAsync(client, Path(seeded.Order, "ProcessRefund"), [("Input.CancellationId", bare.ToString("D")), ("Input.Version", "1")], key);
        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, Path(seeded.Order, "ProcessRefund"), [("Input.DefectClaimId", seeded.ItemKey.ToString("D")), ("Input.Version", "1"), ("Input.Confirmed", "true"), ("Input.ShownIban", ValidRefundIban)], key);
        (_, string after) = await GetAsync(client, Path(seeded.Order), key);

        // The screen: the bare refund waits ("IBAN bekleniyor", no step) - not paid to A, the first on the file; the POST refuses alike.
        Assert.Null(LineIbanOf(screen, bare));
        Assert.Contains(OrderDetailModel.IbanAwaitedText, LineHtmlOf(screen, bare), StringComparison.Ordinal);
        Assert.DoesNotContain($"name=\"Input.CancellationId\" value=\"{bare:D}\"", screen, StringComparison.Ordinal);
        Assert.Equal(DisplayFormat.Iban(ValidRefundIban), LineIbanOf(screen, seeded.ItemKey));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, waiting);
        Assert.Contains(OrderDetailModel.RefundIbanMissingText, refused, StringComparison.Ordinal);

        // The IBAN's life (K-356): A goes with its own refund - the waiting bare refund is not its payee; B stays for the itemless refund; the bare one still waits.
        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Null(LineIbanOf(after, bare));
        Assert.Contains(OrderDetailModel.IbanAwaitedText, LineHtmlOf(after, bare), StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(OtherRefundIban, Assert.Single(await db.RefundIbans.AsNoTracking().Where(i => i.OrderId == seeded.Order.OrderId).Select(i => i.Iban).ToListAsync(Ct)));
        Assert.NotNull((await db.Refunds.AsNoTracking().SingleAsync(r => r.Id == seeded.ItemRefundId, Ct)).SentAt);
        Assert.Null((await db.Refunds.AsNoTracking().SingleAsync(r => r.CancellationId == bare, Ct)).SentAt);
    }

    /// <summary>
    /// The seeded cancellation's card refund of the same item, on the transfer way (02 7.2.8): with <paramref name="ownIban"/> its own request (on the item, naming the
    /// refund) brought it - after A and B on the file -; without one it has no record, request or item IBAN. Returns the line's key, the cancellation.
    /// </summary>
    private async Task<Guid> SeedCancellationsTransferRefundAsync(TurnedRefunds seeded, Guid adminId, string? ownIban)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Refund refund = Refund.BySystem(seeded.Order.OrderId, RefundType.Cancellation, PaymentMethod.Card, 50m, now);
        refund.CancellationId = seeded.Order.CauseId;
        refund.OrderItemId = seeded.Order.ItemId;
        refund.Quantity = 1;
        refund.PaymentMethod = PaymentMethod.BankTransfer;
        await using AppDbContext db = NewContext();
        db.Refunds.Add(refund);
        if (ownIban is not null)
        {
            RefundIban iban = RefundIban.Create(seeded.Order.OrderId, ownIban, now);
            RefundIbanRequest own = RefundIbanRequest.ForItem(seeded.Order.ItemId, RefundIbanRequestReason.CardRefundFailed, refund.Id, adminId, "Yönetici", now.AddMinutes(-1));
            own.FulfilledAt = now;
            own.RefundIbanId = iban.Id;
            db.RefundIbans.Add(iban);
            db.RefundIbanRequests.Add(own);
        }

        await db.SaveChangesAsync(Ct);
        return seeded.Order.CauseId;
    }

    // The customer's cancellation of one unit with its IBAN, through the transition unit in its own transaction (02 7.2.8, 7.4.5); returns the cancellation.
    private async Task<Guid> CancelOneUnitAsync(WebApplicationFactory<Program> factory, Seeded order, string iban)
    {
        await using (AsyncServiceScope scope = factory.Services.CreateAsyncScope())
        {
            IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
            OrderTransitionResult cancelled = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>()
                .RunAsync(token => unit.CancelItemAsync(new CancelItemRequest(order.OrderId, order.ItemId, 1, iban), token), Ct);
            Assert.False(cancelled.IsRejected);
        }

        await using AppDbContext db = NewContext();
        RefundIban written = await db.RefundIbans.AsNoTracking().SingleAsync(i => i.OrderId == order.OrderId && i.Iban == iban, Ct);
        return await db.Cancellations.AsNoTracking().Where(c => c.RefundIbanId == written.Id).Select(c => c.Id).SingleAsync(Ct);
    }

    private async Task<int> VersionOfAsync(Guid orderId)
    {
        await using AppDbContext db = NewContext();
        return await db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct);
    }

    // The markup of one refund line, to its first closing tag or the next line.
    private static string LineHtmlOf(string html, Guid key)
    {
        int line = html.IndexOf($"id=\"geri-odeme-{key:D}\"", StringComparison.Ordinal);
        Assert.True(line >= 0);
        int next = html.IndexOf("id=\"geri-odeme-", line + 1, StringComparison.Ordinal);
        int close = html.IndexOf("</div>", line, StringComparison.Ordinal);
        int end = new[] { next, close, html.Length }.Where(i => i > line).Min();
        return html[line..end];
    }

    // ---- Seeding ----

    /// <summary>
    /// A paid card order whose two refunds the customer's IBANs turned to the transfer (02 7.2.8, 7.4.5): the defect remedy's on the item - IBAN A, entered first, on the item's request - and an
    /// itemless one - the late payment's (K-1075, K-1211) or an amount based one -, IBAN B on its own request. The keys are the lines' (07 UC-131).
    /// </summary>
    private async Task<TurnedRefunds> SeedTurnedRefundsAsync(Guid adminId, bool latePayment, bool withCancellation = false)
    {
        Seeded seeded = await SeedPaidAsync(PaymentMethod.Card, withIban: false, withCancellation: withCancellation);
        (DefectClaim claim, Refund remedy) = await SeedDefectRemedyRefundAsync(seeded);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        PaymentRecord payment = PaymentRecord.ForCardPayment(seeded.OrderId, "late-" + Guid.NewGuid().ToString("N"), null, now);
        Refund itemless = latePayment
            ? Refund.ForLatePayment(seeded.OrderId, payment.Id, 20m, now)
            : Refund.ByAdmin(seeded.OrderId, RefundType.AmountBased, PaymentMethod.Card, 20m, adminId, "Yönetici", now);
        itemless.PaymentMethod = PaymentMethod.BankTransfer;
        RefundIban first = RefundIban.Create(seeded.OrderId, ValidRefundIban, now.AddMinutes(-2));
        RefundIban second = RefundIban.Create(seeded.OrderId, OtherRefundIban, now.AddMinutes(-1));
        RefundIbanRequest onItem = RefundIbanRequest.ForItem(seeded.ItemId, RefundIbanRequestReason.CardRefundFailed, remedy.Id, adminId, "Yönetici", now.AddMinutes(-3));
        onItem.FulfilledAt = now.AddMinutes(-2);
        onItem.RefundIbanId = first.Id;
        RefundIbanRequest onRefund = RefundIbanRequest.ForRefund(itemless.Id, RefundIbanRequestReason.CardRefundFailed, adminId, "Yönetici", now.AddMinutes(-3));
        onRefund.FulfilledAt = now.AddMinutes(-1);
        onRefund.RefundIbanId = second.Id;
        await using AppDbContext db = NewContext();
        if (latePayment)
        {
            db.PaymentRecords.Add(payment);
        }

        db.Refunds.Add(itemless);
        db.RefundIbans.AddRange(first, second);
        db.RefundIbanRequests.AddRange(onItem, onRefund);
        await db.SaveChangesAsync(Ct);
        return new TurnedRefunds(seeded, claim.Id, remedy.Id, latePayment ? payment.Id : itemless.Id, itemless.Id);
    }

    // The IBAN a refund line shows (_OrderRefundLine: "· IBAN TR.." on its first row), or null when the line shows none.
    private static string? LineIbanOf(string html, Guid key)
    {
        int line = html.IndexOf($"id=\"geri-odeme-{key:D}\"", StringComparison.Ordinal);
        if (line < 0)
        {
            return null;
        }

        int next = html.IndexOf("id=\"geri-odeme-", line + 1, StringComparison.Ordinal);
        int limit = Math.Min(next > line ? next : html.Length, line + 800);
        int iban = html.IndexOf("IBAN TR", line, limit - line, StringComparison.Ordinal);
        return iban < 0 ? null : html[(iban + "IBAN ".Length)..html.IndexOf('<', iban)].Trim();
    }

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

    /// <summary>A defect claim's rescission refund born on the transfer line, with no record or request IBAN: it is paid to the IBAN on the order's file (D-878, D-907).</summary>
    private async Task<(DefectClaim Claim, Refund Remedy)> SeedDefectRemedyRefundAsync(Seeded seeded)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DefectClaim claim = DefectClaim.Create(seeded.ItemId, 1, "Kusurlu", null, now);
        Refund remedy = Refund.BySystem(seeded.OrderId, RefundType.DefectClaimRescission, PaymentMethod.BankTransfer, 50m, now);
        remedy.DefectClaimId = claim.Id;
        remedy.OrderItemId = seeded.ItemId;
        remedy.Quantity = 1;
        remedy.CouponShare = 0m;
        remedy.VatAmount = 8.33m;
        await using AppDbContext db = NewContext();
        db.DefectClaims.Add(claim);
        db.Refunds.Add(remedy);
        await db.SaveChangesAsync(Ct);
        return (claim, remedy);
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

    private sealed record TurnedRefunds(Seeded Order, Guid ItemKey, Guid ItemRefundId, Guid ItemlessKey, Guid ItemlessRefundId);
}
