using System.Globalization;
using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The line steps of E-37 through the page (T527 KK4, KK6-KK8, KK10; 07 UC-92, UC-124...UC-128): the defect claims' section and the undelivered e-mails, the shipment's two ends, the delivery date's
/// three states, the returned shipment with and without a delivered item, the closing and the audit row of each step.
/// </summary>
public sealed partial class OrderDetailPageTests
{
    private static readonly ItemAmounts StepAmounts = new(100m, 0m, 16.67m);

    [Fact]
    public async Task TheScreen_ShowsTheDefectClaimsSection_WithTheClaimsMark_AndTheUndeliveredEmailsUnderTheOrdersMark_KK4()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Physical, true));
        Guid claimId;
        string provinceName;
        await using (AppDbContext db = NewContext())
        {
            Province province = await db.Provinces.AsNoTracking().FirstAsync(Ct);
            provinceName = province.Name;
            DefectClaim claim = DefectClaim.Create(items[0], 1, "Kutusu ezik geldi", new ReturnAddress("İade Deposu", province.Id, "Yenimahalle", "Depo Sokak 3", null), DateTimeOffset.UtcNow.AddDays(-1));
            db.DefectClaims.Add(claim);
            DispatchRecord row = DispatchRecord.Queue(DispatchType.B9, "ulasmayan@example.com", orderId, DispatchRelatedRecordType.DefectClaim, claim.Id, DateTimeOffset.UtcNow.AddHours(-2));
            row.DispatchState = DispatchState.Undelivered;
            row.LastOutcome = DispatchOutcome.RecipientError;
            row.AttemptCount = 3;
            db.DispatchRecords.Add(row);
            await db.SaveChangesAsync(Ct);
            claimId = claim.Id;
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);

        Assert.Equal(HttpStatusCode.OK, status);
        int section = html.IndexOf("id=\"ayip-talepleri\"", StringComparison.Ordinal);
        Assert.True(section > 0);
        string claimRow = html[html.IndexOf($"id=\"ayip-talebi-{claimId:D}\"", StringComparison.Ordinal)..];
        claimRow = claimRow[..claimRow.IndexOf("</li>", StringComparison.Ordinal)];
        Assert.Contains("Ayıplı adet: 1", claimRow, StringComparison.Ordinal);
        Assert.Contains("Kutusu ezik geldi", claimRow, StringComparison.Ordinal);
        Assert.Contains(StatusBadges.For(DefectClaimStatus.Open).Text, claimRow, StringComparison.Ordinal);
        Assert.Contains($"İade adresi: İade Deposu · Depo Sokak 3 · Yenimahalle / {provinceName}", claimRow, StringComparison.Ordinal);
        Assert.Contains(PanelMarks.TextOf(PanelMark.EmailNotDelivered), claimRow, StringComparison.Ordinal);
        Assert.DoesNotContain("Yeniden açılış", claimRow, StringComparison.Ordinal);
        // The order's mark carries the undelivered e-mails one by one: the notice's name, the first sending and the address (04 9.9.4, 2.5.3.1), each with its re-send button (UC-153; T531).
        string marks = html[html.IndexOf("id=\"isaretler\"", StringComparison.Ordinal)..html.IndexOf("id=\"siradaki-adim\"", StringComparison.Ordinal)];
        Assert.Contains("B-9", marks, StringComparison.Ordinal);
        Assert.Contains("Gittiği adres: ulasmayan@example.com", marks, StringComparison.Ordinal);
        Assert.Contains(UndeliveredEmailList.WrongAddressReminder, marks, StringComparison.Ordinal);
        Assert.Contains($">{UndeliveredEmailList.ResendLabel}</button>", marks, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheScreen_WithoutAClaim_HasNoClaimsSection_KK4()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.DoesNotContain("id=\"ayip-talepleri\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MarkShipped_Unconfirmed_Is200AndWritesNothing_Confirmed_Is303_SendsOnlyTheOpenPhysicalItem_AndWritesTheAuditRow_KK6_KK10()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false), (ProductType.Physical, false), (ProductType.Digital, false));
        await using (AppDbContext db = NewContext())
        {
            db.Cancellations.Add(Cancellation.ByBuyer(items[1], 1, StepAmounts, null, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string, string)[] shipment = [("Input.Version", "1"), ("Input.Carrier", "YurticiKargo"), ("Input.TrackingNumber", "TR-1001")];

        (HttpStatusCode asked, string askedHtml) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkShipped", shipment, key);
        await using (AppDbContext db = NewContext())
        {
            Assert.True(await db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => new { o.OrderStatus, o.Version }).SingleAsync(Ct) is { OrderStatus: OrderStatus.Preparing, Version: 1 });
        }

        (HttpStatusCode done, string? location) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkShipped", [.. shipment, ("Input.Confirmed", "true")], key);

        Assert.Equal(HttpStatusCode.OK, asked);
        Assert.Contains("data-js=\"confirm-window\"", askedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Equal($"{OrdersPath}/{orderId:D}", location);
        await using AppDbContext reader = NewContext();
        Order order = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((OrderStatus.Shipped, "YurticiKargo", "TR-1001"), (order.OrderStatus, order.Carrier, order.TrackingNumber));
        TransitionRecord record = await reader.TransitionRecords.AsNoTracking().SingleAsync(t => t.OrderId == orderId, Ct);
        Assert.Equal(TransitionCode.S5, record.TransitionCode);
        AuditLog audit = await reader.AuditLogs.AsNoTracking().SingleAsync(a => a.TargetRecordId == orderId, Ct);
        Assert.Equal(nameof(AuditOperation.MarkShipped), audit.Operation);
    }

    [Fact]
    public async Task MarkReshipped_AfterTheReturn_Is303_AndWritesTheAuditRow_AndMarkShippedWithNoOpenPhysicalItem_Is422_KK6_KK10()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid returnedId, _) = await StepOrderAsync(OrderStatus.DeliveryFailed, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid digitalId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Digital, false));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode resent, _) = await PostForLocationAsync(
            client,
            $"{OrdersPath}/{returnedId:D}?handler=MarkReshipped",
            [("Input.Version", "1"), ("Input.IsOwnVehicleDelivery", "true"), ("Input.Confirmed", "true")],
            key);
        (HttpStatusCode noPhysical, string noPhysicalHtml) = await PostAsync(
            client,
            $"{OrdersPath}/{digitalId:D}?handler=MarkShipped",
            [("Input.Version", "1"), ("Input.IsOwnVehicleDelivery", "true"), ("Input.Confirmed", "true")],
            key);

        Assert.Equal(HttpStatusCode.SeeOther, resent);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, noPhysical);
        Assert.Contains(OrderDetailModel.NotOpenText, noPhysicalHtml, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Shipped, await reader.Orders.Where(o => o.Id == returnedId).Select(o => o.OrderStatus).SingleAsync(Ct));
        Assert.Equal(TransitionCode.S8, await reader.TransitionRecords.Where(t => t.OrderId == returnedId).Select(t => t.TransitionCode).SingleAsync(Ct));
        Assert.Equal(nameof(AuditOperation.MarkShipped), await reader.AuditLogs.Where(a => a.TargetRecordId == returnedId).Select(a => a.Operation).SingleAsync(Ct));
        Assert.Equal(0, await reader.AuditLogs.CountAsync(a => a.TargetRecordId == digitalId, Ct));
    }

    [Fact]
    public async Task MarkDelivered_AFutureDateAndOneBeforeTheShipment_Are422_ADeclarationsDayAsksTheOrder_AndTheAnsweredDateIsWritten_KK7_KK10()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false), (ProductType.Physical, false));
        DateOnly today = TurkeyTime.DateOf(DateTimeOffset.UtcNow);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (HttpStatusCode shipped, _) = await PostForLocationAsync(
            client,
            $"{OrdersPath}/{orderId:D}?handler=MarkShipped",
            [("Input.Version", "1"), ("Input.IsOwnVehicleDelivery", "true"), ("Input.Confirmed", "true")],
            key);
        Assert.Equal(HttpStatusCode.SeeOther, shipped);
        await using (AppDbContext db = NewContext())
        {
            db.Withdrawals.Add(Withdrawal.ByBuyer(items[0], 1, today, null, StepAmounts, null, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync(Ct);
        }

        string path = $"{OrdersPath}/{orderId:D}?handler=MarkDelivered";
        (HttpStatusCode future, string futureHtml) = await PostAsync(client, path, [("Input.Version", "2"), ("Input.DeliveryDate", DateText(today.AddDays(1)))], key);
        (HttpStatusCode before, string beforeHtml) = await PostAsync(client, path, [("Input.Version", "2"), ("Input.DeliveryDate", DateText(today.AddDays(-1)))], key);
        (HttpStatusCode question, string questionHtml) = await PostAsync(client, path, [("Input.Version", "2"), ("Input.DeliveryDate", DateText(today))], key);
        await using (AppDbContext db = NewContext())
        {
            Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId && a.Operation == nameof(AuditOperation.MarkDelivered), Ct));
        }

        (HttpStatusCode answered, _) = await PostForLocationAsync(
            client,
            path,
            [("Input.Version", "2"), ("Input.DeliveryDate", DateText(today)), ("Input.IsDeliveredBeforeDeclaration", "true")],
            key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, future);
        Assert.Contains(OrderDetailModel.DeliveryDateFutureText, futureHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, before);
        Assert.Contains(OrderDetailModel.DeliveryDateBeforeShipmentText, beforeHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, question);
        Assert.Contains(OrderDetailModel.DeliveryOrderQuestionText, questionHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, answered);
        await using AppDbContext reader = NewContext();
        Assert.Equal(today, await reader.OrderItems.Where(i => i.Id == items[1]).Select(i => i.DeliveryDate).SingleAsync(Ct));
        Assert.Equal(OrderStatus.Delivered, await reader.Orders.Where(o => o.Id == orderId).Select(o => o.OrderStatus).SingleAsync(Ct));
        List<string> operations = await reader.AuditLogs.Where(a => a.TargetRecordId == orderId).Select(a => a.Operation).ToListAsync(Ct);
        Assert.Contains(nameof(AuditOperation.MarkDelivered), operations);
        Assert.Contains(nameof(AuditOperation.SetDeliveryOrderAgainstWithdrawal), operations);
    }

    [Fact]
    public async Task MarkDeliveryFailed_WithADeliveredItemAndNoOpenOne_EndsInDelivered_WithoutOne_StaysReturned_KK8_KK10()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid mixedId, Guid[] mixedItems) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false), (ProductType.Digital, true));
        await using (AppDbContext db = NewContext())
        {
            db.Cancellations.Add(Cancellation.ByBuyer(mixedItems[0], 1, StepAmounts, null, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync(Ct);
        }

        (Guid plainId, _) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode mixed, _) = await PostForLocationAsync(client, $"{OrdersPath}/{mixedId:D}?handler=MarkDeliveryFailed", [("Input.Version", "1")], key);
        (HttpStatusCode plain, _) = await PostForLocationAsync(client, $"{OrdersPath}/{plainId:D}?handler=MarkDeliveryFailed", [("Input.Version", "1")], key);

        Assert.Equal(HttpStatusCode.SeeOther, mixed);
        Assert.Equal(HttpStatusCode.SeeOther, plain);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Delivered, await reader.Orders.Where(o => o.Id == mixedId).Select(o => o.OrderStatus).SingleAsync(Ct));
        Assert.Equivalent(new[] { TransitionCode.S7, TransitionCode.S11 }, await reader.TransitionRecords.Where(t => t.OrderId == mixedId).Select(t => t.TransitionCode).ToListAsync(Ct));
        Assert.Equal(OrderStatus.DeliveryFailed, await reader.Orders.Where(o => o.Id == plainId).Select(o => o.OrderStatus).SingleAsync(Ct));
        Assert.Equal([TransitionCode.S7], await reader.TransitionRecords.Where(t => t.OrderId == plainId).Select(t => t.TransitionCode).ToListAsync(Ct));
        Assert.Equal(nameof(AuditOperation.MarkDeliveryFailed), await reader.AuditLogs.Where(a => a.TargetRecordId == mixedId).Select(a => a.Operation).SingleAsync(Ct));
        Assert.Equal(nameof(AuditOperation.MarkDeliveryFailed), await reader.AuditLogs.Where(a => a.TargetRecordId == plainId).Select(a => a.Operation).SingleAsync(Ct));
    }

    [Fact]
    public async Task CloseDeliveryFailed_ClosesTheReturnedOrder_WithoutACancellationRecordOrARefund_AndWritesTheAuditRow_KK8_KK10()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false));
        await using (AppDbContext db = NewContext())
        {
            db.Cancellations.Add(Cancellation.ByBuyer(items[0], 1, StepAmounts, null, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (HttpStatusCode returned, _) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkDeliveryFailed", [("Input.Version", "1")], key);
        Assert.Equal(HttpStatusCode.SeeOther, returned);
        int cancellations;
        int refunds;
        await using (AppDbContext db = NewContext())
        {
            cancellations = await db.Cancellations.CountAsync(c => c.OrderItemId == items[0], Ct);
            refunds = await db.Refunds.CountAsync(r => r.OrderId == orderId, Ct);
        }

        (HttpStatusCode closed, _) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=CloseDeliveryFailed", [("Input.Version", "2")], key);

        Assert.Equal(HttpStatusCode.SeeOther, closed);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Cancelled, await reader.Orders.Where(o => o.Id == orderId).Select(o => o.OrderStatus).SingleAsync(Ct));
        Assert.Equal(cancellations, await reader.Cancellations.CountAsync(c => c.OrderItemId == items[0], Ct));
        Assert.Equal(refunds, await reader.Refunds.CountAsync(r => r.OrderId == orderId, Ct));
        Assert.DoesNotContain(DispatchType.B7, await reader.DispatchRecords.Where(d => d.OrderId == orderId).Select(d => d.DispatchType).ToListAsync(Ct));
        Assert.Equal(2, await reader.AuditLogs.CountAsync(a => a.TargetRecordId == orderId && a.Operation == nameof(AuditOperation.MarkDeliveryFailed), Ct));
    }

    [Fact]
    public async Task TheScreen_OnAShippedOrderWhosePhysicalItemsAllClosed_OffersNoDeliveryMark_OnlyTheDeliveryFailure_AndADirectPostIs422_KK3()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid withdrawnId, Guid[] withdrawnItems) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid terminatedId, Guid[] terminatedItems) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false), (ProductType.Digital, true));
        await using (AppDbContext db = NewContext())
        {
            DateOnly today = TurkeyTime.DateOf(DateTimeOffset.UtcNow);
            db.Withdrawals.Add(Withdrawal.ByBuyer(withdrawnItems[0], 1, today, null, StepAmounts, null, DateTimeOffset.UtcNow));
            db.LateDeliveryTerminations.Add(LateDeliveryTermination.ByBuyer(terminatedItems[0], 1, today, StepAmounts, null, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        foreach (Guid orderId in new[] { withdrawnId, terminatedId })
        {
            (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);

            Assert.Equal(HttpStatusCode.OK, status);
            string next = SectionOf(html, "siradaki-adim");
            Assert.DoesNotContain("<form", next, StringComparison.Ordinal);
            Assert.DoesNotContain("button--primary", next, StringComparison.Ordinal);
            // The delivery mark is not open with no open physical item (the writer refuses S6): no form posts it anywhere on the screen.
            Assert.DoesNotContain("handler=MarkDelivered\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain($">{OrderDetailModel.MarkDeliveredLabel}</button>", html, StringComparison.Ordinal);
            Assert.Contains("id=\"mark-delivery-failed-form\"", SectionOf(html, "siparis-islemleri"), StringComparison.Ordinal);

            // The POST asks the same rule as the screen: a direct one is refused before the writer and changes nothing.
            int version;
            await using (AppDbContext db = NewContext())
            {
                version = await db.Orders.Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct);
            }

            (HttpStatusCode posted, string postedHtml) = await PostAsync(
                client,
                $"{OrdersPath}/{orderId:D}?handler=MarkDelivered",
                [("Input.Version", version.ToString(CultureInfo.InvariantCulture)), ("Input.DeliveryDate", DateText(TurkeyTime.DateOf(DateTimeOffset.UtcNow))), ("Input.IsDeliveredBeforeDeclaration", "true")],
                key);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, posted);
            Assert.Contains(OrderDetailModel.NotOpenText, postedHtml, StringComparison.Ordinal);
            await using AppDbContext reader = NewContext();
            Assert.Equal(OrderStatus.Shipped, await reader.Orders.Where(o => o.Id == orderId).Select(o => o.OrderStatus).SingleAsync(Ct));
            Assert.Equal(0, await reader.AuditLogs.CountAsync(a => a.TargetRecordId == orderId && a.Operation == nameof(AuditOperation.MarkDelivered), Ct));
        }
    }

    [Fact]
    public async Task TheScreen_WithAnOpenPhysicalItem_KeepsTheLinesStepAsThePrimaryButton_OnAPreparingAndOnAShippedOrder_KK3()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid preparingId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid shippedId, Guid[] items) = await StepOrderAsync(OrderStatus.Shipped, PaymentStatus.Paid, (ProductType.Physical, false), (ProductType.Physical, false));
        await using (AppDbContext db = NewContext())
        {
            db.Withdrawals.Add(Withdrawal.ByBuyer(items[0], 1, TurkeyTime.DateOf(DateTimeOffset.UtcNow), null, StepAmounts, null, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode preparingStatus, string preparingHtml) = await GetAsync(client, $"{OrdersPath}/{preparingId:D}", key);
        (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{shippedId:D}", key);

        Assert.Equal(HttpStatusCode.OK, preparingStatus);
        string preparingNext = SectionOf(preparingHtml, "siradaki-adim");
        Assert.Contains("id=\"mark-shipped-form\"", preparingNext, StringComparison.Ordinal);
        Assert.Contains($"class=\"button button--primary\" data-busy-label=\"{LoadingViews.BusyLabelOf(OrderDetailModel.ShipLabel)}\">{OrderDetailModel.ShipLabel}</button>", preparingNext, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, status);
        string next = SectionOf(html, "siradaki-adim");
        Assert.Contains("id=\"mark-delivered-form\"", next, StringComparison.Ordinal);
        Assert.Contains($"class=\"button button--primary\" data-busy-label=\"{LoadingViews.BusyLabelOf(OrderDetailModel.MarkDeliveredLabel)}\">{OrderDetailModel.MarkDeliveredLabel}</button>", next, StringComparison.Ordinal);
        Assert.Equal(2, html.Split("handler=MarkDelivered\"", StringSplitOptions.None).Length); // the primary form only: no second delivery mark among the operations
        Assert.Contains("id=\"mark-delivery-failed-form\"", SectionOf(html, "siparis-islemleri"), StringComparison.Ordinal);
    }

    // The section of the screen with the id, up to its closing tag (the screen's sections do not nest).
    private static string SectionOf(string html, string id)
    {
        int start = html.IndexOf($"id=\"{id}\"", StringComparison.Ordinal);
        Assert.True(start >= 0, $"The screen has no section {id}.");
        return html[start..html.IndexOf("</section>", start, StringComparison.Ordinal)];
    }

    private static string DateText(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>A transfer order of the collection with the given items, moved to the given states around the entity (the pair rule of the database still applies).</summary>
    private async Task<(Guid OrderId, Guid[] Items)> StepOrderAsync(OrderStatus orderStatus, PaymentStatus paymentStatus, params (ProductType Type, bool Delivered)[] specs)
    {
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        List<OrderItem> items = [];
        foreach ((ProductType type, bool delivered) in specs)
        {
            OrderItem item = OrderItem.Create(orderId, null, null, type, "Ürün", 1, 100m, 20m, 16.67m, now);
            if (type == ProductType.Service)
            {
                item.FulfilmentPeriod = 5; // a service item carries its fulfilment period (T531)
            }

            if (type == ProductType.Digital)
            {
                item.DownloadAllowance = 5;
            }

            if (delivered)
            {
                item.DeliveryMarkedAt = now;
                item.DeliveryDate = TurkeyTime.DateOf(now);
            }

            items.Add(item);
        }

        await using AppDbContext db = NewContext();
        db.OrderItems.AddRange(items);
        await db.SaveChangesAsync(Ct);
        await db.Orders.Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.OrderStatus, orderStatus).SetProperty(o => o.PaymentStatus, paymentStatus), Ct);
        return (orderId, [.. items.Select(i => i.Id)]);
    }
}
