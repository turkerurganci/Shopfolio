using System.Globalization;
using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604's items on E-37 (D-1544): a form value that does not bind is a 422 with the order on the screen - the validation filter's path loads the order too -, never a 500.</summary>
public sealed partial class OrderDetailPageTests
{
    [Fact]
    public async Task AFormValueThatDoesNotBind_Is422WithTheOrdersScreen_NotA500_AndAKeyThatIsNoOrder_Is404_D1544()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _) = await StepOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, (ProductType.Physical, false));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = $"{OrdersPath}/{orderId:D}";

        // A version that is not a number (T527's step, T530's corrections), a Guid and a bool that do not parse (T530): the filter answers before the handler.
        (HttpStatusCode version, string versionHtml) = await PostAsync(client, page + "?handler=MarkDelivered", [("Input.Version", "abc")], key);
        (HttpStatusCode carrier, string carrierHtml) = await PostAsync(client, page + "?handler=CorrectCarrier", [("Input.Version", "abc"), ("Input.IsOwnVehicleDelivery", "maybe")], key);
        (HttpStatusCode transition, _) = await PostAsync(client, page + "?handler=CorrectTransition", [("Input.Version", "1"), ("Input.TransitionRecordId", "abc")], key);
        (HttpStatusCode foreign, _) = await PostAsync(client, $"{OrdersPath}/{Guid.NewGuid():D}?handler=MarkDelivered", [("Input.Version", "abc")], key);

        Assert.Equal(
            [HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound],
            new[] { version, carrier, transition, foreign });
        Assert.Contains(page + "?handler=", versionHtml, StringComparison.Ordinal);
        Assert.Contains(page + "?handler=", carrierHtml, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.Orders.Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct));
    }

    // ---- D-892 (K-1629 (3)): the claim's goods receipt owes the rescission's refund; UC-131 pays it through the rescission's one writer ----

    [Fact]
    public async Task AClaimsGoodsReceipt_OwesTheRescissionsRefund_AndUc131PaysIt_WithTheTrailLine_AndNoSecondRefund_D892()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        Seeded seeded = await SeedPaidAsync(PaymentMethod.BankTransfer, withIban: true, withCancellation: false);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateOnly today = TurkeyTime.DateOf(now);
        DefectClaim claim = DefectClaim.Create(seeded.ItemId, 2, "Kusurlu", null, now);
        await using (AppDbContext db = NewContext())
        {
            db.DefectClaims.Add(claim);
            await db.SaveChangesAsync(Ct);
        }

        string delivered = today.AddDays(-2).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await ExecuteAsync($"UPDATE orders SET order_status = 'Delivered' WHERE id = '{seeded.OrderId}'");
        await ExecuteAsync($"UPDATE order_items SET delivery_date = '{delivered}', delivery_marked_at = now() WHERE id = '{seeded.ItemId}'");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        // UC-140 takes the claim's two defective units in (K-808: the rescission starts with the receipt; the step pays nothing).
        (HttpStatusCode received, _) = await PostForLocationAsync(
            client, Path(seeded, "ReceiveReturn"), [("Input.OrderItemId", seeded.ItemId.ToString("D")), ("Input.ArrivalDate", today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), ("Input.Version", "1")], key);
        Assert.Equal(HttpStatusCode.SeeOther, received);
        Guid receiptId;
        int version;
        await using (AppDbContext db = NewContext())
        {
            Return receipt = await db.Returns.AsNoTracking().SingleAsync(r => r.OrderItemId == seeded.ItemId, Ct);
            Assert.Equal(((Guid?)claim.Id, 2), (receipt.DefectClaimId, receipt.Quantity));
            Assert.False(await db.Refunds.AnyAsync(r => r.OrderId == seeded.OrderId, Ct));
            receiptId = receipt.Id;
            version = await db.Orders.Where(o => o.Id == seeded.OrderId).Select(o => o.Version).SingleAsync(Ct);
        }

        // The order screen's refund line carries the receipt as its key (07 UC-131); the confirmation repeats the rescission's 02 7.1.7 amount of the two units (2 x 50).
        string shown = version.ToString(CultureInfo.InvariantCulture);
        (_, string screen) = await GetAsync(client, Path(seeded), key);
        // The receipt's key is on three forms: "Stoğa ekle" (UC-141) and the refund line's two steps (UC-131, UC-132).
        string keyInput = $"name=\"Input.ReturnId\" value=\"{receiptId:D}\"";
        Assert.Equal(3, Occurrences(screen, keyInput));
        (HttpStatusCode ask, string window) = await PostAsync(client, Path(seeded, "ProcessRefund"), [("Input.ReturnId", receiptId.ToString("D")), ("Input.Version", shown)], key);
        Assert.Equal(HttpStatusCode.OK, ask);
        Assert.Contains(DisplayFormat.Amount(100m) + " tutarındaki", window, StringComparison.Ordinal);

        (HttpStatusCode done, _) = await PostForLocationAsync(
            client, Path(seeded, "ProcessRefund"), [("Input.ReturnId", receiptId.ToString("D")), ("Input.Version", shown), ("Input.Confirmed", "true"), ("Input.ShownIban", ValidRefundIban)], key);
        (HttpStatusCode again, _) = await PostAsync(
            client, Path(seeded, "ProcessRefund"), [("Input.ReturnId", receiptId.ToString("D")), ("Input.Version", (version + 1).ToString(CultureInfo.InvariantCulture)), ("Input.Confirmed", "true")], key);

        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.NotEqual(HttpStatusCode.SeeOther, again);
        await using AppDbContext after = NewContext();
        Refund refund = await after.Refunds.AsNoTracking().SingleAsync(r => r.OrderId == seeded.OrderId, Ct);
        Assert.Equal(
            (RefundType.DefectClaimRescission, PaymentMethod.BankTransfer, 100m, (Guid?)claim.Id, (Guid?)receiptId, (int?)2, (Guid?)seeded.ItemId),
            (refund.RefundType, refund.PaymentMethod, refund.Amount, refund.DefectClaimId, refund.ReturnId, refund.Quantity, refund.OrderItemId));
        Assert.NotNull(refund.SentAt);
        Order order = await after.Orders.AsNoTracking().SingleAsync(o => o.Id == seeded.OrderId, Ct);
        Assert.Equal(version + 1, order.Version);
        Assert.Equal(refund.Amount >= order.Total ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded, order.PaymentStatus);
        Assert.Equal(1, await after.AuditLogs.CountAsync(a => a.TargetRecordId == seeded.OrderId && a.Operation == nameof(AuditOperation.RefundManually), Ct));

        // The receipt owes nothing more: the line is the born, sent refund's and offers no step.
        (_, string paid) = await GetAsync(client, Path(seeded), key);
        Assert.Equal(1, Occurrences(paid, keyInput));
    }

    private static int Occurrences(string html, string text)
    {
        int count = 0;
        for (int at = html.IndexOf(text, StringComparison.Ordinal); at >= 0; at = html.IndexOf(text, at + text.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }
}
