using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 VAL-B001 (12; 02 3.2): a visitor buys the three product types - they are listed in the same catalog, enter the same cart and are confirmed in one order.</summary>
public sealed partial class CheckoutPageTests
{
    private sealed record ListedProduct(ProductType Type, string Slug, Guid VariantId);

    private async Task<ListedProduct> SeedListedAsync(ProductType type, string name)
    {
        int n = Interlocked.Increment(ref _counter);
        string slug = $"karma-{n:D4}";
        Product product = Product.Create(name, slug, type, 20m, Now.AddMinutes(n));
        if (type == ProductType.Service)
        {
            product.FulfilmentPeriod = 3;
        }
        else
        {
            product = product.WithSeededOrigin();
        }

        product.PublishStatus = PublishStatus.Published;
        Variant variant = Variant.Create(product.Id, $"SKU-B001-{n:D4}", 100m, Now.AddMinutes(n));
        variant.PublishStatus = PublishStatus.Published;
        variant.StockQuantity = type == ProductType.Physical ? 10 : null;
        await using AppDbContext db = NewContext();
        db.Products.Add(product);
        db.Variants.Add(variant);
        await db.SaveChangesAsync(Ct);
        return new ListedProduct(type, slug, variant.Id);
    }

    [Fact]
    public async Task APhysicalADigitalAndAServiceProduct_AreListedInOneCatalog_EnterOneCart_AndAreConfirmedInOneOrder_T604_VALB001()
    {
        ListedProduct[] products =
        [
            await SeedListedAsync(ProductType.Physical, "Karma fiziksel"),
            await SeedListedAsync(ProductType.Digital, "Karma dijital"),
            await SeedListedAsync(ProductType.Service, "Karma hizmet"),
        ];

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        // One catalog: the search lists the three, each with the address of its page.
        string listing = await BodyOf(await SendAsync(client, HttpMethod.Get, $"{FramePaths.Search}?search=karma"));
        foreach (ListedProduct product in products)
        {
            Assert.Contains($"href=\"/urun/{product.Slug}\"", listing, StringComparison.Ordinal);
        }

        // One cart: the three adds go to the cart the first add opened.
        string? cartCookie = null;
        foreach (ListedProduct product in products)
        {
            string[] cookies = cartCookie is null ? [] : [cartCookie];
            using HttpRequestMessage add = new(HttpMethod.Post, $"/urun/{product.Slug}?handler=AddCartItem");
            add.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
            if (cookies.Length > 0)
            {
                add.Headers.Add("Cookie", cookies);
            }

            add.Content = new FormUrlEncodedContent([new("VariantId", product.VariantId.ToString("D")), new("Quantity", "1")]);
            using HttpResponseMessage added = await client.SendAsync(add, Ct);
            Assert.Equal(HttpStatusCode.SeeOther, added.StatusCode);
            string? set = SetCookies(added).FirstOrDefault(c => c.StartsWith(CookieNames.Cart + "=", StringComparison.Ordinal));
            if (cartCookie is null)
            {
                Assert.NotNull(set);
                cartCookie = set[..set.IndexOf(';', StringComparison.Ordinal)];
            }
        }

        Assert.Equal(1, await ScalarAsync("SELECT count(*) FROM carts"));
        Assert.Equal(3, await ScalarAsync("SELECT count(*) FROM cart_items"));

        // One order: the confirmation places the three lines together.
        string page = (await GetAsync(factory, cartCookie!)).Html;
        using HttpResponseMessage placed = await PostAsync(client, "PlaceOrder", [.. Complete(page), ("AcceptDigital", "true"), ("AcceptService", "true")], cartCookie!);
        Assert.True(placed.StatusCode == HttpStatusCode.SeeOther, await BodyOf(placed));

        Assert.Equal(1, await ScalarAsync("SELECT count(*) FROM orders"));
        await using AppDbContext db = NewContext();
        var lines = await db.OrderItems.Select(i => new { i.VariantId, i.ProductType, i.Quantity }).ToListAsync(Ct);
        Assert.Equal(3, lines.Count);
        foreach (ListedProduct product in products)
        {
            Assert.Single(lines, l => l.VariantId == product.VariantId && l.ProductType == product.Type && l.Quantity == 1);
        }
    }
}
