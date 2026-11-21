using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 D-1190 (07 2.5.4; 04 2.7.3.1; K-1627 (2)): the cart shown again by the forgery check's 400 is drawn with its lines and its total, with the message, and nothing is written.</summary>
public sealed partial class CartPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheCartWithItsLines_AndTheMessage_AndWritesNothing_D1190()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant variant = await SeedVariantAsync(price: 100m);
        Guid cartId = await SeedCartAsync(customerId, null, (variant, 2, 100m));
        Guid itemId = await ItemIdAsync(cartId, variant.Id);

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpRequestMessage request = new(HttpMethod.Post, $"{FramePaths.Cart}?handler=UpdateCartItem");
        request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        request.Headers.Add("Cookie", Customer(customerKey));
        request.Content = new FormUrlEncodedContent([new("CartItemId", itemId.ToString()), new("Quantity", "1")]);
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains($"id=\"line-{itemId:N}\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"cart-total\"", html, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, html, StringComparison.Ordinal);
        Assert.Equal((2, 100m), await QuantityAndFirstPriceAsync(itemId));
    }
}
