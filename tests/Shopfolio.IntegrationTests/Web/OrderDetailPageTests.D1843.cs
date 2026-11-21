using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1): the forgery check's 400 on the order's screen shows the order of the route's key through the one hook (<see cref="IReadsOnForgeryRefusal"/>) and writes
/// nothing; a key that is no order's is the one 404, as the GET and the 422 answer.
/// </summary>
public sealed partial class OrderDetailPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheOrderAndItsSteps_WritesNothing_AndAForeignKeyIs404_D1843()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, string number, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string cookie = $"{CookieNames.AdminSession}={key}";

        (HttpStatusCode status, string html) = await ForgedPosts.SendAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkPaid", [("Input.Version", "1"), ("Input.Confirmed", "true")], cookie);
        (HttpStatusCode foreign, _) = await ForgedPosts.SendAsync(client, $"{OrdersPath}/{Guid.NewGuid():D}?handler=MarkPaid", [("Input.Version", "1")], cookie);

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Contains($"<h1>{number}</h1>", html, StringComparison.Ordinal);
        Assert.Contains("id=\"mark-paid-form\"", html, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, foreign);
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);
        Assert.Equal(1, order.Version);
    }
}
