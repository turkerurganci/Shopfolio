using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T531 on E-37 through the page (KK1-KK9; 07 UC-139, UC-143, UC-152...UC-154): the service item's mark - on the row and as the primary button's fourth arm (D-1491) - and its closure, the download
/// right's renewal, the internal note with its shown text, and the re-sending of the undelivered e-mails one by one and all at once. The re-sending tests open the application without the job runner, so
/// a queued row stays queued and the next drawing is the test's to set.
/// </summary>
public sealed partial class OrderDetailPageTests
{
    // ---- UC-139: the service item's mark ----

    [Fact]
    public async Task CompleteServiceItem_MarksTheWholeOpenQuantity_TheWithdrawalFalls_TheDefectChannelOpens_AndTheLastItemClosesTheOrder_KK1()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Service, false));
        (Guid mixedId, Guid[] mixedItems) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false), (ProductType.Service, false));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        // No order-level step is left on the service order: the first open service item's mark is the primary button (04 9.9.5; D-1491); the row carries it too (04 9.9.26).
        (HttpStatusCode shown, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        Assert.Equal(HttpStatusCode.OK, shown);
        string next = SectionOf(html, "siradaki-adim");
        Assert.Contains("id=\"complete-service-primary-form\"", next, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.OrderItemId\" value=\"{items[0]:D}\"", next, StringComparison.Ordinal);
        Assert.Contains($"class=\"button button--primary\" data-busy-label=\"{LoadingViews.BusyLabelOf(OrderDetailModel.CompleteServiceLabel)}\">{OrderDetailModel.CompleteServiceLabel}</button>", next, StringComparison.Ordinal);
        Assert.Contains($"id=\"complete-service-{items[0]:D}\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Input.Confirmed", html, StringComparison.Ordinal);
        // The mixed order still has its shipment as the line's next step: the service mark is on the row only.
        (_, string mixedHtml) = await GetAsync(client, $"{OrdersPath}/{mixedId:D}", key);
        Assert.Contains("id=\"mark-shipped-form\"", SectionOf(mixedHtml, "siradaki-adim"), StringComparison.Ordinal);
        Assert.DoesNotContain("complete-service-primary-form", mixedHtml, StringComparison.Ordinal);
        Assert.Contains($"id=\"complete-service-{mixedItems[1]:D}\"", mixedHtml, StringComparison.Ordinal);

        OrderOperationsView before = await BuyerOperationsAsync(factory, orderId);
        (HttpStatusCode done, string? location) = await PostForLocationAsync(
            client, $"{OrdersPath}/{orderId:D}?handler=CompleteServiceItem", [("Input.Version", "1"), ("Input.OrderItemId", items[0].ToString("D"))], key);
        (HttpStatusCode mixedDone, _) = await PostForLocationAsync(
            client, $"{OrdersPath}/{mixedId:D}?handler=CompleteServiceItem", [("Input.Version", "1"), ("Input.OrderItemId", mixedItems[1].ToString("D"))], key);

        // No confirmation: the first POST without a confirmation mark is the operation (07 UC-139).
        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Equal($"{OrdersPath}/{orderId:D}", location);
        Assert.Equal(HttpStatusCode.SeeOther, mixedDone);
        await using AppDbContext db = NewContext();
        OrderItem item = await db.OrderItems.AsNoTracking().SingleAsync(i => i.Id == items[0], Ct);
        Assert.NotNull(item.DeliveryMarkedAt);
        Assert.NotNull(item.DeliveryDate);
        // The last open item ends the line (S10 from Preparing, the administrator's event; 03 1.4.3); the order's version moved once.
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal(OrderStatus.Delivered, order.OrderStatus);
        Assert.Equal(TransitionCode.S10, await db.TransitionRecords.Where(t => t.OrderId == orderId).Select(t => t.TransitionCode).SingleAsync(Ct));
        AuditLog audit = await db.AuditLogs.AsNoTracking().SingleAsync(a => a.TargetRecordId == orderId, Ct);
        Assert.Equal(nameof(AuditOperation.CompleteService), audit.Operation);
        Assert.Contains(audit.Details!.Values, v => v.Name == "ItemId" && v.Value == items[0].ToString("D")); // non-null: the step writes its values
        Assert.Contains(audit.Details!.Values, v => v.Name == "TransitionCode" && v.Value == nameof(TransitionCode.S10));
        // A service item that is not the last leaves the order where it was.
        Assert.Equal(OrderStatus.Preparing, await db.Orders.Where(o => o.Id == mixedId).Select(o => o.OrderStatus).SingleAsync(Ct));
        Assert.NotNull(await db.OrderItems.Where(i => i.Id == mixedItems[1]).Select(i => i.DeliveryMarkedAt).SingleAsync(Ct));
        Assert.Null(await db.OrderItems.Where(i => i.Id == mixedItems[0]).Select(i => i.DeliveryMarkedAt).SingleAsync(Ct));

        // The customer's side: the withdrawal right of the service falls with the mark and "sorun bildir" opens (03 2.5.3.6).
        OrderOperationsView after = await BuyerOperationsAsync(factory, orderId);
        Assert.True(before.IsOpen(OrderOperation.WithdrawServiceItem, items[0]));
        Assert.False(before.IsOpen(OrderOperation.OpenDefectClaim, items[0]));
        Assert.False(after.IsOpen(OrderOperation.WithdrawServiceItem, items[0]));
        Assert.True(after.IsOpen(OrderOperation.OpenDefectClaim, items[0]));
    }

    [Fact]
    public async Task CompleteServiceItem_OnAnUnpaidOrder_AndOnAClosedItem_Is422_AndWritesNothing_KK2()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid unpaidId, Guid[] unpaidItems) = await StepOrderAsync(OrderStatus.Placed, PaymentStatus.Pending, (ProductType.Service, false));
        (Guid markedId, Guid[] markedItems) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Service, true), (ProductType.Physical, false));
        await using ShopfolioWebApplicationFactory root = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(Pages.PanelOrdersModel).Assembly)));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string run = Guid.NewGuid().ToString("N")[..8];

        // An unpaid order does not offer the mark (K-536).
        (_, string unpaidHtml) = await GetAsync(client, $"{OrdersPath}/{unpaidId:D}", key);
        Assert.DoesNotContain(OrderDetailModel.CompleteServiceLabel, unpaidHtml, StringComparison.Ordinal);
        (HttpStatusCode unpaid, string unpaidPost) = await PostAsync(
            client, $"{OrdersPath}/{unpaidId:D}?handler=CompleteServiceItem", [("Input.Version", "1"), ("Input.OrderItemId", unpaidItems[0].ToString("D"))], key, $"t531-unpaid-{run}");
        (HttpStatusCode closed, string closedPost) = await PostAsync(
            client, $"{OrdersPath}/{markedId:D}?handler=CompleteServiceItem", [("Input.Version", "1"), ("Input.OrderItemId", markedItems[0].ToString("D"))], key, $"t531-closed-{run}");
        // A physical item is no service item: the decision's row is about service items only.
        (HttpStatusCode physical, _) = await PostAsync(
            client, $"{OrdersPath}/{markedId:D}?handler=CompleteServiceItem", [("Input.Version", "1"), ("Input.OrderItemId", markedItems[1].ToString("D"))], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, unpaid);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closed);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, physical);
        // The message stands on the item's row (04 9.9.41), not at the line's step.
        Assert.Contains($"id=\"order-item-message-{unpaidItems[0]:D}\"", unpaidPost, StringComparison.Ordinal);
        Assert.Contains(OrderDetailModel.NotOpenText, SectionOf(closedPost, "kalemler"), StringComparison.Ordinal);
        Assert.Equal(nameof(ErrorCode.OperationNotAvailable), CodeOfRejection(root.LogDirectory, $"t531-unpaid-{run}"));
        Assert.Equal(nameof(ErrorCode.OperationNotAvailable), CodeOfRejection(root.LogDirectory, $"t531-closed-{run}"));
        await using AppDbContext db = NewContext();
        Assert.Null(await db.OrderItems.Where(i => i.Id == unpaidItems[0]).Select(i => i.DeliveryMarkedAt).SingleAsync(Ct));
        Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.TargetRecordId == unpaidId || a.TargetRecordId == markedId, Ct));
        Assert.All(await db.Orders.Where(o => o.Id == unpaidId || o.Id == markedId).Select(o => o.Version).ToArrayAsync(Ct), version => Assert.Equal(1, version));
    }

    // ---- UC-143: the download right's renewal ----

    [Fact]
    public async Task ResetDownloadCount_PutsTheCounterAtZero_SendsNoNotice_AndAnItemThatIsNotDigital_Is422_KK3()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Digital, true), (ProductType.Physical, true));
        await using (AppDbContext setup = NewContext())
        {
            await setup.OrderItems.Where(i => i.Id == items[0]).ExecuteUpdateAsync(s => s.SetProperty(i => i.DownloadCount, 5), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (_, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        Assert.Contains($"id=\"reset-download-{items[0]:D}\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain($"id=\"reset-download-{items[1]:D}\"", html, StringComparison.Ordinal);

        (HttpStatusCode renewed, _) = await PostForLocationAsync(
            client, $"{OrdersPath}/{orderId:D}?handler=ResetDownloadCount", [("Input.Version", "1"), ("Input.OrderItemId", items[0].ToString("D"))], key);
        (HttpStatusCode physical, string physicalHtml) = await PostAsync(
            client, $"{OrdersPath}/{orderId:D}?handler=ResetDownloadCount", [("Input.Version", "2"), ("Input.OrderItemId", items[1].ToString("D"))], key);
        // The version the screen showed before the renewal is stale now (07 UC-143: idempotency by the version).
        (HttpStatusCode again, _) = await PostAsync(
            client, $"{OrdersPath}/{orderId:D}?handler=ResetDownloadCount", [("Input.Version", "1"), ("Input.OrderItemId", items[0].ToString("D"))], key);

        Assert.Equal(HttpStatusCode.SeeOther, renewed);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, physical);
        Assert.Contains(OrderDetailModel.NotOpenText, physicalHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Conflict, again);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.OrderItems.Where(i => i.Id == items[0]).Select(i => i.DownloadCount).SingleAsync(Ct));
        // No notice goes (K-683) and the renewal is not written to the trail (02 3.12.6).
        Assert.Equal(0, await db.DispatchRecords.CountAsync(d => d.OrderId == orderId, Ct));
        Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));
    }

    // ---- UC-152: the internal note ----

    [Fact]
    public async Task SaveInternalNote_WritesChangesAndEmptiesTheNote_TheVersionStays_AndTheTrailHasNoText_KK4()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (_, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        string noteSection = SectionOf(html, "ic-not");
        Assert.Contains("id=\"internal-note-form\"", noteSection, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.OriginalText\" value=\"\"", noteSection, StringComparison.Ordinal);

        (HttpStatusCode written, _) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=SaveInternalNote", [("Input.Text", "Müşteri öğleden sonra aranacak"), ("Input.OriginalText", "")], key);
        (_, string shown) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        (HttpStatusCode changed, _) = await PostForLocationAsync(
            client, $"{OrdersPath}/{orderId:D}?handler=SaveInternalNote", [("Input.Text", "Kargo adresi teyit edildi"), ("Input.OriginalText", "Müşteri öğleden sonra aranacak")], key);
        string? changedText;
        await using (AppDbContext middle = NewContext())
        {
            changedText = await middle.InternalNotes.Where(n => n.OrderId == orderId).Select(n => n.Text).SingleAsync(Ct);
        }

        (HttpStatusCode emptied, _) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=SaveInternalNote", [("Input.Text", "   "), ("Input.OriginalText", "Kargo adresi teyit edildi")], key);

        Assert.Equal(HttpStatusCode.SeeOther, written);
        Assert.Equal(HttpStatusCode.SeeOther, changed);
        Assert.Equal(HttpStatusCode.SeeOther, emptied);
        Assert.Contains("name=\"Input.OriginalText\" value=\"Müşteri öğleden sonra aranacak\"", SectionOf(shown, "ic-not"), StringComparison.Ordinal);
        Assert.Equal("Kargo adresi teyit edildi", changedText);
        await using AppDbContext db = NewContext();
        Assert.False(await db.InternalNotes.AnyAsync(n => n.OrderId == orderId, Ct)); // the emptied note is deleted (K-1051)
        Assert.Equal(1, await db.Orders.Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct)); // the note is outside the order's version
        List<AuditLog> trail = await db.AuditLogs.AsNoTracking().Where(a => a.TargetRecordId == orderId).ToListAsync(Ct);
        Assert.Equal(3, trail.Count);
        Assert.All(trail, a => Assert.Equal(nameof(AuditOperation.SaveInternalNote), a.Operation));
        string details = JsonSerializer.Serialize(trail.Select(a => a.Details));
        Assert.DoesNotContain("aranacak", details, StringComparison.Ordinal);
        Assert.DoesNotContain("teyit", details, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveInternalNote_AgainstAChangedNote_Is409WithTheCurrentNote_AndADisposedOrder_Is422WithTheFieldClosed_KK5()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        (Guid disposedId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using (AppDbContext setup = NewContext())
        {
            setup.InternalNotes.Add(InternalNote.Create(orderId, "Birinci yöneticinin notu", adminId, "Yönetici Bir", DateTimeOffset.UtcNow));
            await setup.SaveChangesAsync(Ct);
            // The disposal's emptying of the order's personal data (06 9.2.1), as OrderCorrectionTests writes it.
            await setup.Database.ExecuteSqlAsync(
                $"""
                UPDATE orders SET contact_email = NULL, normalized_contact_email = NULL, order_access_key = NULL, ip_address = NULL, cart_id = NULL, customer_id = NULL,
                billing_recipient_name = NULL, billing_province_id = NULL, billing_district = NULL, billing_address_line = NULL, shipping_recipient_name = NULL, shipping_phone = NULL,
                shipping_province_id = NULL, shipping_district = NULL, shipping_address_line = NULL WHERE id = {disposedId}
                """,
                Ct);
        }

        await using ShopfolioWebApplicationFactory root = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(Pages.PanelOrdersModel).Assembly)));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string run = Guid.NewGuid().ToString("N")[..8];

        // The form showed no note, another administrator wrote one meanwhile: 409, the order notice and the current note (04 2.10.2; K-1051); the note stays as it was.
        (HttpStatusCode conflict, string conflictHtml) = await PostAsync(
            client, $"{OrdersPath}/{orderId:D}?handler=SaveInternalNote", [("Input.Text", "İkinci yöneticinin notu"), ("Input.OriginalText", "")], key, $"t531-note-{run}");
        (_, string disposedPage) = await GetAsync(client, $"{OrdersPath}/{disposedId:D}", key);
        (HttpStatusCode disposed, string disposedHtml) = await PostAsync(
            client, $"{OrdersPath}/{disposedId:D}?handler=SaveInternalNote", [("Input.Text", "Not"), ("Input.OriginalText", "")], key, $"t531-disposed-{run}");

        Assert.Equal(HttpStatusCode.Conflict, conflict);
        string conflictNote = SectionOf(conflictHtml, "ic-not");
        Assert.Contains(ConflictControls.OrderMessage, conflictNote, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.OriginalText\" value=\"Birinci yöneticinin notu\"", conflictNote, StringComparison.Ordinal);
        Assert.Equal(nameof(ErrorCode.DisplayedValueConflict), CodeOfRejection(root.LogDirectory, $"t531-note-{run}"));
        // The disposed order's field is closed and shows no note (04 9.9.11; K-1095); a sent note is 422.
        Assert.DoesNotContain("internal-note-form", SectionOf(disposedPage, "ic-not"), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, disposed);
        Assert.Contains(OrderDetailModel.NotOpenText, SectionOf(disposedHtml, "ic-not"), StringComparison.Ordinal);
        Assert.Equal(nameof(ErrorCode.OperationNotAvailable), CodeOfRejection(root.LogDirectory, $"t531-disposed-{run}"));
        await using AppDbContext db = NewContext();
        Assert.Equal("Birinci yöneticinin notu", await db.InternalNotes.Where(n => n.OrderId == orderId).Select(n => n.Text).SingleAsync(Ct));
        Assert.False(await db.InternalNotes.AnyAsync(n => n.OrderId == disposedId, Ct));
        Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId || a.TargetRecordId == disposedId, Ct));
    }

    // ---- UC-153, UC-154: the re-sending ----

    [Fact]
    public async Task ResendNotification_WritesANewQueuedRowInTheGroup_AndTheResultIsReadAtTheNextDrawing_KK6()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        Guid first = await UndeliveredAsync(orderId, DispatchType.B4, "alici@example.com");
        await using WebApplicationFactory<Program> factory = Open(new StorefrontSearchAndProductTests.CommandCounter());
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (_, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        string marks = SectionOf(html, "isaretler");
        Assert.Contains($"name=\"Input.DispatchRecordId\" value=\"{first:D}\"", marks, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Version\"", marks, StringComparison.Ordinal);
        Assert.DoesNotContain(UndeliveredEmailList.ResendAllLabel, marks, StringComparison.Ordinal); // one row: no "Hepsini yeniden gönder"

        (HttpStatusCode sent, string? location) = await PostForLocationAsync(
            client, $"{OrdersPath}/{orderId:D}?handler=ResendNotification", [("Input.Version", "1"), ("Input.DispatchRecordId", first.ToString("D"))], key);

        Assert.Equal(HttpStatusCode.SeeOther, sent);
        Assert.Equal($"{OrdersPath}/{orderId:D}", location);
        Guid again;
        await using (AppDbContext db = NewContext())
        {
            DispatchRecord row = await db.DispatchRecords.AsNoTracking().SingleAsync(d => d.OrderId == orderId && d.Id != first, Ct);
            Assert.Equal(first, row.OriginalDispatchId); // the group's first row (K-1262)
            Assert.Equal(DispatchState.Queued, row.DispatchState);
            Assert.Equal(DispatchType.B4, row.DispatchType);
            Assert.Equal(1, await db.Orders.Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct)); // the order's version does not move
            Assert.Equal(nameof(AuditOperation.ResendEmail), await db.AuditLogs.Where(a => a.TargetRecordId == orderId).Select(a => a.Operation).SingleAsync(Ct));
            again = row.Id;

            // The queue's three tries fail: the row is "undelivered" again and the next drawing says so in place (04 9.9.33, 2.5.4).
            await db.DispatchRecords.Where(d => d.Id == again).ExecuteUpdateAsync(
                s => s.SetProperty(d => d.DispatchState, DispatchState.Undelivered).SetProperty(d => d.AttemptCount, 3).SetProperty(d => d.NextAttemptAt, (DateTimeOffset?)null)
                    .SetProperty(d => d.LastOutcome, DispatchOutcome.RecipientError),
                Ct);
        }

        (_, string failedHtml) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        Assert.Contains(UndeliveredEmailList.ResendFailedMessage, SectionOf(failedHtml, "isaretler"), StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.DispatchRecordId\" value=\"{again:D}\"", SectionOf(failedHtml, "isaretler"), StringComparison.Ordinal);

        // A later try goes: the row falls and, being the last, the mark with it.
        await using (AppDbContext db = NewContext())
        {
            await db.DispatchRecords.Where(d => d.Id == again).ExecuteUpdateAsync(
                s => s.SetProperty(d => d.DispatchState, DispatchState.Sent).SetProperty(d => d.SentAt, DateTimeOffset.UtcNow).SetProperty(d => d.NextAttemptAt, (DateTimeOffset?)null),
                Ct);
        }

        (_, string sentHtml) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        Assert.DoesNotContain(UndeliveredEmailList.ResendLabel, sentHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(PanelMarks.TextOf(PanelMark.EmailNotDelivered), SectionOf(sentHtml, "isaretler"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResendNotification_TwoConcurrentRequests_WriteOneRow_AndTheSecondSending_Is422NotA409_KK7_KK8()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        Guid first = await UndeliveredAsync(orderId, DispatchType.B1, "alici@example.com");
        await using WebApplicationFactory<Program> factory = Open(new StorefrontSearchAndProductTests.CommandCounter());
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpClient other = ForgeryTokenHandler.CreateClient(factory);
        (string Name, string Value)[] fields = [("Input.Version", "1"), ("Input.DispatchRecordId", first.ToString("D"))];

        // The condition is read under the order's row lock: the second request waits, then finds the group's latest row queued (05 4.4.3; K-1066, K-1084).
        Task<(HttpStatusCode, string?)> one = PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=ResendNotification", fields, key);
        Task<(HttpStatusCode, string?)> two = PostForLocationAsync(other, $"{OrdersPath}/{orderId:D}?handler=ResendNotification", fields, key);
        (HttpStatusCode, string?)[] answers = await Task.WhenAll(one, two);
        // The same form sent once more later: still 422, the version did not move so it is no 409 (07 UC-153).
        (HttpStatusCode third, string thirdHtml) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=ResendNotification", fields, key);

        Assert.Equal(new[] { HttpStatusCode.SeeOther, HttpStatusCode.UnprocessableEntity }, answers.Select(a => a.Item1).Order().ToArray());
        Assert.Equal(HttpStatusCode.UnprocessableEntity, third);
        Assert.Contains(OrderDetailModel.NotOpenText, thirdHtml, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        DispatchRecord row = await db.DispatchRecords.AsNoTracking().SingleAsync(d => d.OrderId == orderId && d.Id != first, Ct);
        Assert.Equal(first, row.OriginalDispatchId);
        // B-1 goes again as B-1 of the same group, with the group's first e-mail moment; its body is made from the order's frozen texts when the queue sends it (K-1115; the sending itself:
        // DispatchRetryAndMarkTests.EveryCustomerNotice_IsMadeFromTheOrder_SentAgainThroughTheQueue_...).
        Assert.Equal(DispatchType.B1, row.DispatchType);
        Assert.NotNull(CustomerNoticeValues.ReadFirstEmailSentAt(row.EventValues));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));
    }

    [Fact]
    public async Task ResendAllNotifications_WritesARowForEveryMarkedNotice_InOneOrderOnly_AndIsAllOrNone_KK9()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid otherOrderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid partialId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        Guid b1 = await UndeliveredAsync(orderId, DispatchType.B1, "alici@example.com");
        Guid b4 = await UndeliveredAsync(orderId, DispatchType.B4, "alici@example.com");
        Guid foreign = await UndeliveredAsync(otherOrderId, DispatchType.B4, "alici@example.com");
        // A B-15 row goes to the address its own row names; with that address gone it cannot be re-sent, so the order's "all" refuses as a whole.
        await UndeliveredAsync(partialId, DispatchType.B4, "alici@example.com", -3);
        Guid noAddress = await UndeliveredAsync(partialId, DispatchType.B15, "eski@example.com", -2);
        await using (AppDbContext setup = NewContext())
        {
            await setup.DispatchRecords.Where(d => d.Id == noAddress).ExecuteUpdateAsync(s => s.SetProperty(d => d.RecipientEmail, (string?)null), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open(new StorefrontSearchAndProductTests.CommandCounter());
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (_, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);
        Assert.Contains($">{UndeliveredEmailList.ResendAllLabel}</button>", SectionOf(html, "isaretler"), StringComparison.Ordinal);

        (HttpStatusCode all, _) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=ResendAllNotifications", [("Input.Version", "1")], key);
        (HttpStatusCode partial, _) = await PostAsync(client, $"{OrdersPath}/{partialId:D}?handler=ResendAllNotifications", [("Input.Version", "1")], key);
        (HttpStatusCode again, _) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=ResendAllNotifications", [("Input.Version", "1")], key);

        Assert.Equal(HttpStatusCode.SeeOther, all);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, partial);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, again); // nothing is marked any more: the second sending writes nothing
        await using AppDbContext db = NewContext();
        List<Guid?> roots = await db.DispatchRecords.Where(d => d.OrderId == orderId && d.OriginalDispatchId != null).Select(d => d.OriginalDispatchId).ToListAsync(Ct);
        Assert.Equal(new Guid?[] { b1, b4 }.Order(), roots.Order());
        // No re-sending across orders (K-761), and the refused order wrote none of its rows - not even the one that could go.
        Assert.Equal(foreign, await db.DispatchRecords.Where(d => d.OrderId == otherOrderId).Select(d => d.Id).SingleAsync(Ct));
        Assert.Equal(2, await db.DispatchRecords.CountAsync(d => d.OrderId == partialId, Ct));
        Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.TargetRecordId == partialId || a.TargetRecordId == otherOrderId, Ct));
        Assert.Equal(2, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId && a.Operation == nameof(AuditOperation.ResendEmail), Ct));
    }

    // A customer notice of the order whose three tries failed (Z-26): the "e-posta ulaşmadı" mark of the order (02 9.1.6).
    private async Task<Guid> UndeliveredAsync(Guid orderId, DispatchType type, string recipient, int hoursAgo = -2)
    {
        await using AppDbContext db = NewContext();
        DispatchRecord row = DispatchRecord.Queue(type, recipient, orderId, null, null, DateTimeOffset.UtcNow.AddHours(hoursAgo));
        row.DispatchState = DispatchState.Undelivered;
        row.LastOutcome = DispatchOutcome.RecipientError;
        row.AttemptCount = 3;
        db.DispatchRecords.Add(row);
        await db.SaveChangesAsync(Ct);
        return row.Id;
    }

    private static async Task<OrderOperationsView> BuyerOperationsAsync(WebApplicationFactory<Program> factory, Guid orderId)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        return (await scope.ServiceProvider.GetRequiredService<IOrderOperations>().ReadAsync(orderId, ActorKind.Buyer, Ct))!; // non-null: the order exists
    }
}
