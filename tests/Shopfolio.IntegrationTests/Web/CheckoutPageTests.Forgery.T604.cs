using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 D-1190 (07 2.5.4; 04 2.7.3.1; K-1627 (2)): the payment step shown again by the forgery check's 400 has its summary and its form with the sent values, with the message, and no order is written.</summary>
public sealed partial class CheckoutPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheStepWithItsSummary_AndTheSentValues_AndTheMessage_AndPlacesNothing_D1190()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        (_, string page, _) = await GetAsync(factory, GuestCart(guest));
        long ordersBefore = await ScalarAsync("SELECT count(*) FROM orders");

        // The plain client sends no forgery token: the check answers before the handler.
        using HttpClient plain = PlainClient(factory);
        using HttpResponseMessage response = await PostAsync(plain, "PlaceOrder", [.. Fields(page, guest: true, email: "sahte@example.com")], GuestCart(guest));
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("id=\"checkout-summary\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"checkout-form\"", html, StringComparison.Ordinal);
        Assert.Contains("sahte@example.com", html, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, html, StringComparison.Ordinal);
        Assert.Equal(ordersBefore, await ScalarAsync("SELECT count(*) FROM orders"));
    }
}
