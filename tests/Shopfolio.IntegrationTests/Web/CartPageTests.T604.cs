using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
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

    [Fact]
    public async Task TheLinesImage_HasTheAlternativeTextOf3114_TheGalleryTextOfTheMainImage_TheFirmsText_OrTheNameAndOptionValuesOfAnOwnImage_D1819()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        (Variant variant, Guid mainImageId) = await SeedVariantWithImageAndOptionAsync("Kırmızı");
        Guid cartId = await SeedCartAsync(customerId, null, (variant, 1, 100m));
        Guid itemId = await ItemIdAsync(cartId, variant.Id);
        string name;
        await using (AppDbContext db = NewContext())
        {
            name = await db.Products.Where(p => p.Id == variant.ProductId).Select(p => p.Name).SingleAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);

        // The product's main image, no text written: the name and the place in the gallery.
        Assert.Contains($"src=\"/gorsel/{mainImageId:D}\" alt=\"{name} — görsel 1 / 1\"", await LineAsync(factory, customerKey, itemId), StringComparison.Ordinal);

        // The firm's text for the image wins.
        await using (AppDbContext db = NewContext())
        {
            await db.ProductImages.Where(p => p.ImageId == mainImageId).ExecuteUpdateAsync(p => p.SetProperty(x => x.AltText, "Kırmızı kumaşın yakın çekimi"), Ct);
        }

        Assert.Contains($"src=\"/gorsel/{mainImageId:D}\" alt=\"Kırmızı kumaşın yakın çekimi\"", await LineAsync(factory, customerKey, itemId), StringComparison.Ordinal);

        // A variant's own image without a text of its own: the name and the variant's option values.
        Guid ownImageId;
        await using (AppDbContext db = NewContext())
        {
            Image own = Image.Create($"sepet-{Guid.NewGuid():N}", ImageFormat.Png, 1, 1, DateTimeOffset.UtcNow.AddDays(-3));
            db.Images.Add(own);
            await db.SaveChangesAsync(Ct);
            ownImageId = own.Id;
            await db.Variants.Where(v => v.Id == variant.Id).ExecuteUpdateAsync(v => v.SetProperty(x => x.ImageId, ownImageId), Ct);
        }

        Assert.Contains($"src=\"/gorsel/{ownImageId:D}\" alt=\"{name} — Kırmızı\"", await LineAsync(factory, customerKey, itemId), StringComparison.Ordinal);
    }

    private static async Task<string> LineAsync(WebApplicationFactory<Program> factory, string customerKey, Guid itemId) =>
        WebUtility.HtmlDecode(LineOf((await GetAsync(factory, Customer(customerKey))).Html, itemId));
}
