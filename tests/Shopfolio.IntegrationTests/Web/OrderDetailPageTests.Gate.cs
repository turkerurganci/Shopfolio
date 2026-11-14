using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The re-check of E-37's line steps under the order's lock (T527 correction 1; 07 9.9; 09 8.3 items 3-4): a changed version is 409 before the operation's rule is asked - on the unconfirmed first POST
/// too -, a closed operation is 422 on both POSTs and writes nothing, the carrier data is refused with the form's message (K-799), the own-vehicle declaration is what the order keeps (VAL-C041), a returned
/// order whose physical items all closed by a termination and a withdrawal is not sent again (VAL-C046), and the transfer's last payment day is the list's (D-1436).
/// </summary>
public sealed partial class OrderDetailPageTests
{
    [Fact]
    public async Task AnUnconfirmedSendingWithTheOldVersion_AfterAnotherAdministratorsStep_Is409_NotTheClosedSteps422_KK9()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid paidId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        (Guid shippedId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        (Guid resentId, _) = await StepOrderAsync(OrderStatus.DeliveryFailed, PaymentStatus.Paid, (ProductType.Physical, false));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string, string)[] ownVehicle = [("Input.Version", "1"), ("Input.IsOwnVehicleDelivery", "true")];

        // Another administrator's step moves each order on (version 2) and closes the step the first screen still offers.
        Assert.Equal(HttpStatusCode.SeeOther, (await PostForLocationAsync(client, $"{OrdersPath}/{paidId:D}?handler=MarkPaid", [("Input.Version", "1"), ("Input.Confirmed", "true")], key)).Status);
        Assert.Equal(HttpStatusCode.SeeOther, (await PostForLocationAsync(client, $"{OrdersPath}/{shippedId:D}?handler=MarkShipped", [.. ownVehicle, ("Input.Confirmed", "true")], key)).Status);
        Assert.Equal(HttpStatusCode.SeeOther, (await PostForLocationAsync(client, $"{OrdersPath}/{resentId:D}?handler=MarkReshipped", [.. ownVehicle, ("Input.Confirmed", "true")], key)).Status);

        (HttpStatusCode paid, string paidHtml) = await PostAsync(client, $"{OrdersPath}/{paidId:D}?handler=MarkPaid", [("Input.Version", "1")], key);
        (HttpStatusCode shipped, string shippedHtml) = await PostAsync(client, $"{OrdersPath}/{shippedId:D}?handler=MarkShipped", ownVehicle, key);
        (HttpStatusCode resent, string resentHtml) = await PostAsync(client, $"{OrdersPath}/{resentId:D}?handler=MarkReshipped", ownVehicle, key);

        Assert.Equal(HttpStatusCode.Conflict, paid);
        Assert.Contains(ConflictControlsText, paidHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(OrderDetailModel.NotOpenText, paidHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Conflict, shipped);
        Assert.Contains(ConflictControlsText, shippedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Conflict, resent);
        Assert.Contains(ConflictControlsText, resentHtml, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        foreach (Guid orderId in new[] { paidId, shippedId, resentId })
        {
            Assert.Equal(2, await db.Orders.Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct));
            Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));
        }
    }

    [Fact]
    public async Task MarkPaid_OnACancelledTransferOrderAndOnACardOrder_Is422Unconfirmed_AndConfirmed_AndWritesNothing_KK5()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid cancelledId, _) = await StepOrderAsync(OrderStatus.Cancelled, PaymentStatus.Failed, (ProductType.Physical, false));
        (Guid cardId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using (AppDbContext db = NewContext())
        {
            await db.Orders.Where(o => o.Id == cardId)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.PaymentMethod, PaymentMethod.Card).SetProperty(o => o.BankTransferIban, (string?)null), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        foreach (Guid orderId in new[] { cancelledId, cardId })
        {
            (HttpStatusCode asked, string askedHtml) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkPaid", [("Input.Version", "1")], key);
            (HttpStatusCode confirmed, string confirmedHtml) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkPaid", [("Input.Version", "1"), ("Input.Confirmed", "true")], key);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, asked);
            Assert.Contains(OrderDetailModel.NotOpenText, askedHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("data-js=\"confirm-window\"", askedHtml, StringComparison.Ordinal);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, confirmed);
            Assert.Contains(OrderDetailModel.NotOpenText, confirmedHtml, StringComparison.Ordinal);
        }

        await using AppDbContext reader = NewContext();
        foreach (Guid orderId in new[] { cancelledId, cardId })
        {
            Assert.Equal(1, await reader.Orders.Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct));
            Assert.False(await reader.TransitionRecords.AnyAsync(t => t.OrderId == orderId, Ct));
            Assert.False(await reader.AuditLogs.AnyAsync(a => a.TargetRecordId == orderId, Ct));
        }
    }

    [Fact]
    public async Task MarkShipped_WithTheOwnVehicleDeclaration_KeepsTheDeclarationWithoutACarrierOrATrackingNumber_AndTheScreenShowsIt_VAL_C041()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode done, _) = await PostForLocationAsync(
            client,
            $"{OrdersPath}/{orderId:D}?handler=MarkShipped",
            [("Input.Version", "1"), ("Input.IsOwnVehicleDelivery", "true"), ("Input.Confirmed", "true")],
            key);
        (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);

        Assert.Equal(HttpStatusCode.SeeOther, done);
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((OrderStatus.Shipped, true, null, null, null), (order.OrderStatus, order.IsOwnVehicleDelivery, order.Carrier, order.OtherCarrierName, order.TrackingNumber));
        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("Kendi aracımızla teslim", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MarkReshipped_OnAReturnedOrderWhosePhysicalItemsClosedByATerminationAndAWithdrawal_Is422_AndChangesNothing_VAL_C046()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, Guid[] items) = await StepOrderAsync(OrderStatus.DeliveryFailed, PaymentStatus.Paid, (ProductType.Physical, false), (ProductType.Physical, false));
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateOnly today = TurkeyTime.DateOf(now);
        await using (AppDbContext db = NewContext())
        {
            db.LateDeliveryTerminations.Add(LateDeliveryTermination.ByBuyer(items[0], 1, today, StepAmounts, null, now));
            db.Withdrawals.Add(Withdrawal.ByBuyer(items[1], 1, today, null, StepAmounts, null, now));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string, string)[] ownVehicle = [("Input.Version", "1"), ("Input.IsOwnVehicleDelivery", "true")];

        (HttpStatusCode asked, string askedHtml) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkReshipped", ownVehicle, key);
        (HttpStatusCode confirmed, string confirmedHtml) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkReshipped", [.. ownVehicle, ("Input.Confirmed", "true")], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, asked);
        Assert.Contains(OrderDetailModel.NotOpenText, askedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, confirmed);
        Assert.Contains(OrderDetailModel.NotOpenText, confirmedHtml, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Order order = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((OrderStatus.DeliveryFailed, 1, false), (order.OrderStatus, order.Version, order.IsOwnVehicleDelivery));
        Assert.False(await reader.TransitionRecords.AnyAsync(t => t.OrderId == orderId, Ct));
        Assert.False(await reader.AuditLogs.AnyAsync(a => a.TargetRecordId == orderId, Ct));
    }

    [Fact]
    public async Task AnInvalidCarrierValue_Is422WithTheFormsMessageAndShipmentInfoMissing_OnBothSendings_K799()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        await using ShopfolioWebApplicationFactory root = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(Pages.PanelOrdersModel).Assembly)));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string run = Guid.NewGuid().ToString("N")[..8];
        (string Name, (string, string)[] Fields)[] cases =
        [
            ("both", [("Input.Carrier", "YurticiKargo"), ("Input.OtherCarrierName", "Başka Kargo"), ("Input.TrackingNumber", "TR-1")]),
            ("unlisted", [("Input.Carrier", "NoSuchCarrier"), ("Input.TrackingNumber", "TR-1")]),
            ("long", [("Input.Carrier", "YurticiKargo"), ("Input.TrackingNumber", new string('7', ShipmentLimits.TrackingNumberMaxLength + 1))]),
        ];

        foreach ((string name, (string, string)[] fields) in cases)
        {
            foreach (bool confirmed in new[] { false, true })
            {
                string requestId = $"t527-{name}-{confirmed}-{run}";
                List<(string, string)> sent = [("Input.Version", "1"), .. fields];
                if (confirmed)
                {
                    sent.Add(("Input.Confirmed", "true"));
                }

                (HttpStatusCode status, string html) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkShipped", sent, key, requestId);

                Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
                Assert.Contains(OrderDetailModel.ShipmentMissingText, html, StringComparison.Ordinal);
                Assert.DoesNotContain(OrderDetailModel.NotOpenText, html, StringComparison.Ordinal);
                Assert.DoesNotContain("data-js=\"confirm-window\"", html, StringComparison.Ordinal);
                Assert.Equal(nameof(ErrorCode.ShipmentInfoMissing), CodeOfRejection(root.LogDirectory, requestId));
            }
        }

        await using AppDbContext db = NewContext();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((OrderStatus.Preparing, 1), (order.OrderStatus, order.Version));
    }

    [Fact]
    public async Task TheTransfersLastPaymentDay_IsTheDayBeforeTheWindowsEnd_AsOnTheList_D1436()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        DateOnly lastDay = TurkeyTime.DateOf(DateTimeOffset.UtcNow).AddDays(2);
        await using (AppDbContext db = NewContext())
        {
            // A window ends at the 00:00 after its last day (TurkeyTime.EndOfPeriod).
            await db.Orders.Where(o => o.Id == orderId).ExecuteUpdateAsync(s => s.SetProperty(o => o.PaymentWindowEndsAt, TurkeyTime.StartOf(lastDay.AddDays(1))), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains($"Son ödeme günü: {DisplayFormat.Date(lastDay)}", html, StringComparison.Ordinal);
        Assert.DoesNotContain($"Son ödeme günü: {DisplayFormat.Date(lastDay.AddDays(1))}", html, StringComparison.Ordinal);
    }
}
