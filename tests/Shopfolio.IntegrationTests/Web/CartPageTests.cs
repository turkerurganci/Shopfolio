using System.Globalization;
using System.Net;
using System.Reflection;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the cart page's tests (T508): one database copied from the template.</summary>
[CollectionDefinition(CartPageTestsDefinition.Name)]
public sealed class CartPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CartPageTests";
}

/// <summary>
/// The cart page on the opened application (T508 KK1-KK10; 07 UC-19, UC-20, UC-21; 04 E-12): what <c>GET /sepet</c> draws is the module's computation, the once-told messages are taken by the
/// read that tells them, the cookies follow 05 6.7.7 and 6.7.8, and the two writing ends answer 303, 422 and 404 as UC-20 and UC-21 say. Carts, lines, products and the firm's shipping terms are
/// real rows of the collection database. The sales gate is the real one except where a test opens it: the template's firm does not meet the gate's conditions, and the gate's own rules are
/// T130's tests.
/// </summary>
[Collection(CartPageTestsDefinition.Name)]
public sealed class CartPageTests : IAsyncLifetime
{
    private const string CookiePolicyId = "01000000-0000-7000-8000-000000508001";

    private static readonly DateTimeOffset Now = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    private int _counter;

    public CartPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        db.CartItems.RemoveRange(await db.CartItems.ToListAsync(Ct));
        db.Carts.RemoveRange(await db.Carts.ToListAsync(Ct));
        await db.SaveChangesAsync(Ct);
        await db.CompanySettings.ExecuteUpdateAsync(
            s => s.SetProperty(c => c.ShippingFee, 0m).SetProperty(c => c.FreeShippingThreshold, (decimal?)null).SetProperty(c => c.MinimumOrderAmount, (decimal?)null).SetProperty(c => c.IsSalesTemporarilyClosed, false),
            Ct);

        // Until the cookie policy is published the storefront writes no cookie at all (05 6.7; T224): the cart's cookies are observed with the policy published.
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO legal_text_versions (id, created_at, legal_text_type, version_number, published_at, text) "
            + $"VALUES ('{CookiePolicyId}', now(), 'CookiePolicy', 1, now(), 'policy') ON CONFLICT (id) DO NOTHING",
            Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: every line of the breakdown is the server's computation ----

    [Fact]
    public async Task TheBreakdown_TheFreeShippingMessage_AndTheButton_AreTheModulesComputation_T508_KK1()
    {
        await SetShippingAsync(fee: 30m, threshold: 500m, minimum: 50m);
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant first = await SeedVariantAsync(price: 100m);
        Variant second = await SeedVariantAsync(price: 45.5m);
        await SeedCartAsync(customerId, null, (first, 2, 100m), (second, 1, 45.5m));

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        CartView expected = await ReadViewAsync(factory, CartOwner.ForMember(customerId));
        (HttpStatusCode status, string html, _) = await GetAsync(factory, Customer(customerKey));

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(245.5m, expected.OrderableSubtotal);
        Assert.Contains($"id=\"cart-subtotal\">{DisplayFormat.Amount(expected.OrderableSubtotal)}<", html, StringComparison.Ordinal);
        Assert.Contains($"id=\"cart-shipping\">{DisplayFormat.Amount(expected.Shipping.Fee)}<", html, StringComparison.Ordinal);
        Assert.Contains($"id=\"cart-total\">{DisplayFormat.Amount(expected.OrderableSubtotal + expected.Shipping.Fee)}<", html, StringComparison.Ordinal);
        Assert.False(string.IsNullOrEmpty(expected.Shipping.FreeShippingMessage));
        Assert.Contains(expected.Shipping.FreeShippingMessage, html, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Amount(200m), html, StringComparison.Ordinal);
        Assert.Contains(CartTexts.ProceedLabel, ProceedOf(html), StringComparison.Ordinal);
        Assert.Contains($"href=\"{FramePaths.Checkout}\"", ProceedOf(html), StringComparison.Ordinal);
    }

    // ---- KK2: the line is the product as it stands; the line's messages stay while their condition does ----

    [Fact]
    public async Task APriceChange_ShowsTheCurrentPrice_AndItsMessageStaysAtEveryRead_WhileTheFirstPriceIsKept_T508_KK2()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant changed = await SeedVariantAsync(price: 120m);
        Variant soldOut = await SeedVariantAsync(price: 50m, stock: 0);
        Guid cartId = await SeedCartAsync(customerId, null, (changed, 1, 100m), (soldOut, 1, 50m));

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        string first = (await GetAsync(factory, Customer(customerKey))).Html;
        string second = (await GetAsync(factory, Customer(customerKey))).Html;

        foreach (string html in new[] { first, second })
        {
            string line = LineOf(html, await ItemIdAsync(cartId, changed.Id));
            Assert.Contains(DisplayFormat.Amount(120m), line, StringComparison.Ordinal);
            Assert.Contains(CartTexts.PriceChanged, line, StringComparison.Ordinal);
            Assert.Contains(CartTexts.SoldOut, LineOf(html, await ItemIdAsync(cartId, soldOut.Id)), StringComparison.Ordinal);

            // The sold-out line does not enter the amount (04 5.12.6).
            Assert.Contains($"id=\"cart-subtotal\">{DisplayFormat.Amount(120m)}<", html, StringComparison.Ordinal);
        }

        await using AppDbContext db = NewContext();
        Assert.Equal(100m, await db.CartItems.Where(i => i.CartId == cartId && i.VariantId == changed.Id).Select(i => i.FirstAddedUnitPrice).SingleAsync(Ct));
    }

    // ---- KK3: the three reasons the payment step stays closed, each in its own test ----

    [Fact]
    public async Task NoLineThatEntersTheOrder_ClosesTheStepWithItsSentence_T508_KK3()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant soldOut = await SeedVariantAsync(price: 50m, stock: 0);
        await SeedCartAsync(customerId, null, (soldOut, 1, 50m));

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        string proceed = ProceedOf((await GetAsync(factory, Customer(customerKey))).Html);

        Assert.Contains(CartTexts.NoOrderableItem, proceed, StringComparison.Ordinal);
        Assert.DoesNotContain(FramePaths.Checkout, proceed, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AMissingMinimumAmount_ClosesTheStepWithTheMissingAmount_T508_KK3()
    {
        await SetShippingAsync(fee: 0m, threshold: null, minimum: 250m);
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant variant = await SeedVariantAsync(price: 100m);
        await SeedCartAsync(customerId, null, (variant, 1, 100m));

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        CartView expected = await ReadViewAsync(factory, CartOwner.ForMember(customerId));
        string proceed = ProceedOf((await GetAsync(factory, Customer(customerKey))).Html);

        Assert.Equal(CartPaymentBlock.MinimumOrderAmountNotMet, expected.PaymentBlock);
        Assert.False(string.IsNullOrEmpty(expected.Shipping.MinimumOrderMessage));
        Assert.Contains(expected.Shipping.MinimumOrderMessage, proceed, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Amount(150m), proceed, StringComparison.Ordinal);
        Assert.DoesNotContain(FramePaths.Checkout, proceed, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AClosedSale_ClosesTheStepInTheButtonsPlace_AndTheCartStaysVisible_T508_KK3()
    {
        await SetShippingAsync(fee: 0m, threshold: null, minimum: 250m);
        await using (AppDbContext db = NewContext())
        {
            await db.CompanySettings.ExecuteUpdateAsync(s => s.SetProperty(c => c.IsSalesTemporarilyClosed, true), Ct);
        }

        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant variant = await SeedVariantAsync(price: 100m);
        Guid cartId = await SeedCartAsync(customerId, null, (variant, 1, 100m));

        // The real gate: the switch is on (and the template's firm misses other conditions too).
        await using WebApplicationFactory<Program> factory = Open(salesOpen: false);
        string html = (await GetAsync(factory, Customer(customerKey))).Html;
        string proceed = ProceedOf(html);

        Assert.Contains(SalesClosedMessages.StorefrontText, proceed, StringComparison.Ordinal);
        Assert.DoesNotContain(FramePaths.Checkout, proceed, StringComparison.Ordinal);

        // The closed sale is told first, even with the minimum amount missing; the lines and the breakdown stay.
        Assert.DoesNotContain("Sipariş verebilmek için", proceed, StringComparison.Ordinal);
        Assert.Contains($"id=\"line-{await ItemIdAsync(cartId, variant.Id):N}\"", html, StringComparison.Ordinal);
        Assert.Contains($"id=\"cart-subtotal\">{DisplayFormat.Amount(100m)}<", html, StringComparison.Ordinal);
    }

    // ---- KK4: the once-told messages: told at the head of the content, then gone ----

    [Fact]
    public async Task TheOnceToldMessages_AreToldOnceAtTheHead_TheMarkedLineIsDeleted_AndTheMarksAreCleared_T508_KK4()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant merged = await SeedVariantAsync(price: 100m);
        Variant kept = await SeedVariantAsync(price: 100m);
        Variant removed = await SeedVariantAsync(price: 100m);
        Guid cartId = await SeedCartAsync(customerId, null, (merged, 1, 100m), (kept, 1, 100m), (removed, 1, 100m));
        string mergedName;
        await using (AppDbContext db = NewContext())
        {
            mergedName = await db.Products.Where(p => p.Id == merged.ProductId).Select(p => p.Name).SingleAsync(Ct);
            await db.CartItems.Where(i => i.CartId == cartId && i.VariantId == merged.Id).ExecuteUpdateAsync(i => i.SetProperty(x => x.IsMergeNoticePending, true), Ct);
            await db.CartItems.Where(i => i.CartId == cartId && i.VariantId == removed.Id)
                .ExecuteUpdateAsync(i => i.SetProperty(x => x.VariantId, (Guid?)null).SetProperty(x => x.RemovedProductName, "Kalkan Ürün"), Ct);
            await db.Carts.Where(c => c.Id == cartId).ExecuteUpdateAsync(c => c.SetProperty(x => x.IsPaymentUnavailableNoticePending, true), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        string first = (await GetAsync(factory, Customer(customerKey))).Html;
        string second = (await GetAsync(factory, Customer(customerKey))).Html;

        string mergeText = CartTexts.MergeAdded([mergedName]);
        string removedText = CartTexts.RemovedFromShowcase("Kalkan Ürün");
        int head = first.IndexOf("data-js=\"cart\"", StringComparison.Ordinal);
        Assert.True(head > 0);
        foreach (string text in new[] { mergeText, removedText, CartTexts.PaymentUnavailable })
        {
            int at = first.IndexOf(text, StringComparison.Ordinal);
            Assert.InRange(at, 0, head);
            Assert.Equal(at, first.LastIndexOf(text, StringComparison.Ordinal));
            Assert.DoesNotContain(text, second, StringComparison.Ordinal);
        }

        await using AppDbContext reader = NewContext();
        Assert.Equal(2, await reader.CartItems.CountAsync(i => i.CartId == cartId, Ct));
        Assert.False(await reader.CartItems.AnyAsync(i => i.CartId == cartId && (i.IsMergeNoticePending || i.RemovedProductName != null), Ct));
        Assert.False(await reader.Carts.Where(c => c.Id == cartId).Select(c => c.IsPaymentUnavailableNoticePending).SingleAsync(Ct));
    }

    // ---- KK5: the guest's cart and its cookie ----

    [Fact]
    public async Task AGuestWithoutTheCartCookie_HasAnEmptyCart_AndTheGetOpensNoCookie_T508_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        int cartsBefore = await CountCartsAsync();

        (HttpStatusCode status, string html, IReadOnlyList<string> setCookies) = await GetAsync(factory);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(CartTexts.EmptySentence, html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"cart-proceed\"", html, StringComparison.Ordinal);
        Assert.Empty(setCookies);
        Assert.Equal(cartsBefore, await CountCartsAsync());
    }

    [Fact]
    public async Task TheCart_IsAClosedScreen_WithTheScreensNameInTheTabTitle_AndOnlyTheRobotsMetaTag_D693()
    {
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);

        (HttpStatusCode status, string html, _) = await GetAsync(factory);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains($"<title>{CartTexts.ScreenName}{SearchText.TitleSeparator}", html, StringComparison.Ordinal);
        Assert.Contains("<meta name=\"robots\" content=\"noindex\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"description\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("og:title", html, StringComparison.Ordinal);
        Assert.Single(html.Split("<title>"), part => part.Contains("</title>", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AGuestWithTheCartCookie_HasTheLastAccessWritten_AndTheCookieRenewed_T508_KK5()
    {
        string guestKey = NewGuestKey();
        Variant variant = await SeedVariantAsync(price: 100m);
        DateTimeOffset seeded = DateTimeOffset.UtcNow.AddDays(-2);
        Guid cartId = await SeedCartAsync(null, guestKey, (variant, 1, 100m));
        await using (AppDbContext db = NewContext())
        {
            await db.Carts.Where(c => c.Id == cartId).ExecuteUpdateAsync(c => c.SetProperty(x => x.LastAccessedAt, seeded), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        (HttpStatusCode status, string html, IReadOnlyList<string> setCookies) = await GetAsync(factory, GuestCart(guestKey));

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains($"id=\"line-{await ItemIdAsync(cartId, variant.Id):N}\"", html, StringComparison.Ordinal);
        string renewed = Assert.Single(setCookies, c => c.StartsWith($"{CookieNames.Cart}=", StringComparison.Ordinal));
        Assert.StartsWith($"{CookieNames.Cart}={guestKey};", renewed, StringComparison.Ordinal);
        Assert.Contains("max-age=", renewed, StringComparison.OrdinalIgnoreCase);
        await using AppDbContext reader = NewContext();
        Assert.True(await reader.Carts.Where(c => c.Id == cartId).Select(c => c.LastAccessedAt).SingleAsync(Ct) > seeded.AddDays(1));
    }

    // ---- KK6: an empty cart deletes the checkout step's cookie ----

    [Fact]
    public async Task RemovingTheLastLine_DeletesTheCheckoutCookie_AndAnEmptyCartsGetDeletesItToo_T508_KK6()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant first = await SeedVariantAsync(price: 100m);
        Variant second = await SeedVariantAsync(price: 100m);
        Guid cartId = await SeedCartAsync(customerId, null, (first, 1, 100m), (second, 1, 100m));
        Guid firstId = await ItemIdAsync(cartId, first.Id);
        Guid secondId = await ItemIdAsync(cartId, second.Id);
        string checkout = $"{CookieNames.Checkout}=held";

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage notLast = await PostAsync(client, "RemoveCartItem", [("CartItemId", firstId.ToString())], Customer(customerKey), checkout);
        using HttpResponseMessage last = await PostAsync(client, "RemoveCartItem", [("CartItemId", secondId.ToString())], Customer(customerKey), checkout);
        (_, _, IReadOnlyList<string> emptyGet) = await GetAsync(factory, Customer(customerKey), checkout);

        Assert.Equal(HttpStatusCode.SeeOther, notLast.StatusCode);
        Assert.DoesNotContain(SetCookies(notLast), c => c.StartsWith($"{CookieNames.Checkout}=", StringComparison.Ordinal));
        Assert.Equal(HttpStatusCode.SeeOther, last.StatusCode);
        Assert.True(IsDeletion(Assert.Single(SetCookies(last), c => c.StartsWith($"{CookieNames.Checkout}=", StringComparison.Ordinal))));
        Assert.True(IsDeletion(Assert.Single(emptyGet, c => c.StartsWith($"{CookieNames.Checkout}=", StringComparison.Ordinal))));
    }

    // ---- KK7: the quantity is absolute; a repeat gives the same result ----

    [Fact]
    public async Task TheQuantity_IsWrittenAsAnAbsoluteValue_TheRepeatGivesTheSameResult_AndTheFirstPriceStays_T508_KK7()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant variant = await SeedVariantAsync(price: 130m);
        Guid cartId = await SeedCartAsync(customerId, null, (variant, 1, 100m));
        Guid itemId = await ItemIdAsync(cartId, variant.Id);

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        foreach (string quantity in new[] { "3", "3", "2" })
        {
            using HttpResponseMessage response = await PostAsync(client, "UpdateCartItem", [("CartItemId", itemId.ToString()), ("Quantity", quantity)], Customer(customerKey));
            Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
            Assert.Equal(FramePaths.Cart, response.Headers.Location?.OriginalString);
            Assert.Equal((int.Parse(quantity, CultureInfo.InvariantCulture), 100m), await QuantityAndFirstPriceAsync(itemId));
        }
    }

    [Fact]
    public async Task TheQuantityForm_WorksWithoutScript_ItsOwnButtonSendsIt_AndTheScriptIsThePagesOwnModule_T539_KK1_KK2_KK4()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant variant = await SeedVariantAsync(price: 130m);
        Guid cartId = await SeedCartAsync(customerId, null, (variant, 1, 100m));
        Guid itemId = await ItemIdAsync(cartId, variant.Id);

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        string html = (await GetAsync(factory, Customer(customerKey))).Html;

        // Without the script the quantity form's own button sends UC-20 (the script only sends the same form with that button when the number changes); the form is a one-time form.
        string line = LineOf(html, itemId);
        Assert.Contains("name=\"Quantity\"", line, StringComparison.Ordinal);
        Assert.Contains("handler=UpdateCartItem", line, StringComparison.Ordinal);
        Assert.Contains("data-js=\"submit-once\"", line, StringComparison.Ordinal);
        Assert.Contains("<button type=\"submit\"", line, StringComparison.Ordinal);
        Assert.Contains("js/pages/storefront/cart", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ADigitalLine_HasNoQuantityField_AndAQuantityOtherThanOneIsRefused_T508_KK7()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant digital = await SeedVariantAsync(price: 80m, type: ProductType.Digital);
        Guid cartId = await SeedCartAsync(customerId, null, (digital, 1, 80m));
        Guid itemId = await ItemIdAsync(cartId, digital.Id);

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        string html = (await GetAsync(factory, Customer(customerKey))).Html;
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage refused = await PostAsync(client, "UpdateCartItem", [("CartItemId", itemId.ToString()), ("Quantity", "2")], Customer(customerKey));

        string line = LineOf(html, itemId);
        Assert.DoesNotContain("name=\"Quantity\"", line, StringComparison.Ordinal);
        Assert.Contains("handler=RemoveCartItem", line, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Equal((1, 80m), await QuantityAndFirstPriceAsync(itemId));
    }

    // ---- KK8: the two 422 reasons and the 404 ----

    [Theory]
    [InlineData("0")]
    [InlineData("1.5")]
    [InlineData("")]
    public async Task AQuantityUnderOneOrNotWhole_Is422_AndNothingIsWritten_T508_KK8(string quantity)
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant variant = await SeedVariantAsync(price: 100m);
        Guid cartId = await SeedCartAsync(customerId, null, (variant, 2, 100m));
        Guid itemId = await ItemIdAsync(cartId, variant.Id);

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage response = await PostAsync(client, "UpdateCartItem", [("CartItemId", itemId.ToString()), ("Quantity", quantity)], Customer(customerKey));

        // The validation filter answers without the handler; the page it shows again is the framed cart with the refused line (T508 moved the frame's filter outside it).
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string line = LineOf(await BodyOf(response), itemId);
        Assert.Contains("aria-invalid=\"true\"", line, StringComparison.Ordinal);
        Assert.Contains("Adet", line, StringComparison.Ordinal);
        Assert.Equal((2, 100m), await QuantityAndFirstPriceAsync(itemId));
    }

    [Fact]
    public async Task ARaiseOverTheStock_Is422WithTheLinesMessage_AndNothingIsWritten_T508_KK8()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant variant = await SeedVariantAsync(price: 100m, stock: 3);
        Guid cartId = await SeedCartAsync(customerId, null, (variant, 2, 100m));
        Guid itemId = await ItemIdAsync(cartId, variant.Id);

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage response = await PostAsync(client, "UpdateCartItem", [("CartItemId", itemId.ToString()), ("Quantity", "4")], Customer(customerKey));
        string line = LineOf(await BodyOf(response), itemId);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(CartTexts.NoStock, line, StringComparison.Ordinal);
        Assert.Contains("value=\"4\"", line, StringComparison.Ordinal);
        Assert.Equal((2, 100m), await QuantityAndFirstPriceAsync(itemId));

        // A lower quantity is always written.
        using HttpResponseMessage lower = await PostAsync(client, "UpdateCartItem", [("CartItemId", itemId.ToString()), ("Quantity", "1")], Customer(customerKey));
        Assert.Equal(HttpStatusCode.SeeOther, lower.StatusCode);
        Assert.Equal((1, 100m), await QuantityAndFirstPriceAsync(itemId));
    }

    [Fact]
    public async Task ARaiseOverTheProductsLimit_CountingItsOtherLines_Is422WithTheLimit_AndNothingIsWritten_T508_KK8()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        (Variant first, Variant second) = await SeedTwoVariantsOfOneProductAsync(limit: 3);
        Guid cartId = await SeedCartAsync(customerId, null, (first, 1, 100m), (second, 1, 100m));
        Guid itemId = await ItemIdAsync(cartId, first.Id);

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage refused = await PostAsync(client, "UpdateCartItem", [("CartItemId", itemId.ToString()), ("Quantity", "3")], Customer(customerKey));
        string line = LineOf(await BodyOf(refused), itemId);
        using HttpResponseMessage held = await PostAsync(client, "UpdateCartItem", [("CartItemId", itemId.ToString()), ("Quantity", "2")], Customer(customerKey));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Contains(CartTexts.OverOrderLimit(3), line, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, held.StatusCode);
        Assert.Equal((2, 100m), await QuantityAndFirstPriceAsync(itemId));
    }

    [Fact]
    public async Task AWriteWithoutTheForgeryToken_Is400InTheFrame_AndNothingIsWritten_T508_KK8()
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
        Assert.Contains("<h1>Sepet</h1>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("error-page", html, StringComparison.Ordinal);
        Assert.Equal((2, 100m), await QuantityAndFirstPriceAsync(itemId));
    }

    [Fact]
    public async Task ALineOfAnotherCart_OrOneThatNeverWas_Is404_AndNothingIsWritten_T508_KK8()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        (string _, Guid otherId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant variant = await SeedVariantAsync(price: 100m);
        await SeedCartAsync(customerId, null, (variant, 1, 100m));
        Guid foreignCart = await SeedCartAsync(otherId, null, (variant, 2, 100m));
        Guid foreignItem = await ItemIdAsync(foreignCart, variant.Id);

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage foreign = await PostAsync(client, "UpdateCartItem", [("CartItemId", foreignItem.ToString()), ("Quantity", "1")], Customer(customerKey));
        using HttpResponseMessage unknown = await PostAsync(client, "UpdateCartItem", [("CartItemId", Guid.NewGuid().ToString()), ("Quantity", "1")], Customer(customerKey));
        using HttpResponseMessage noCookie = await PostAsync(client, "UpdateCartItem", [("CartItemId", foreignItem.ToString()), ("Quantity", "1")]);

        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, noCookie.StatusCode);
        Assert.Equal((2, 100m), await QuantityAndFirstPriceAsync(foreignItem));
    }

    // ---- KK9: a line taken off the showcase keeps its quantity; the read tells it and deletes it ----

    [Fact]
    public async Task ALineTakenOffTheShowcase_KeepsItsQuantity_AndTheNextReadTellsItOnceAndDeletesIt_T508_KK9()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant variant = await SeedVariantAsync(price: 100m);
        Guid cartId = await SeedCartAsync(customerId, null, (variant, 2, 100m));
        Guid itemId = await ItemIdAsync(cartId, variant.Id);
        await using (AppDbContext db = NewContext())
        {
            await db.CartItems.Where(i => i.Id == itemId).ExecuteUpdateAsync(i => i.SetProperty(x => x.VariantId, (Guid?)null).SetProperty(x => x.RemovedProductName, "Kalkan Ürün"), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage response = await PostAsync(client, "UpdateCartItem", [("CartItemId", itemId.ToString()), ("Quantity", "5")], Customer(customerKey));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(FramePaths.Cart, response.Headers.Location?.OriginalString);
        Assert.Equal((2, 100m), await QuantityAndFirstPriceAsync(itemId));

        string html = (await GetAsync(factory, Customer(customerKey))).Html;
        Assert.Contains(CartTexts.RemovedFromShowcase("Kalkan Ürün"), html, StringComparison.Ordinal);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.CartItems.AnyAsync(i => i.Id == itemId, Ct));
    }

    // ---- KK10: the removal, the marked line too, and the second 404 ----

    [Fact]
    public async Task TheRemoval_Is303_TheSecondRemovalIs404_AndAForeignLineStays_T508_KK10()
    {
        string guestKey = NewGuestKey();
        (string _, Guid otherId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant first = await SeedVariantAsync(price: 100m);
        Variant second = await SeedVariantAsync(price: 100m);
        Guid cartId = await SeedCartAsync(null, guestKey, (first, 1, 100m), (second, 1, 100m));
        Guid itemId = await ItemIdAsync(cartId, first.Id);
        Guid foreignItem = await ItemIdAsync(await SeedCartAsync(otherId, null, (first, 1, 100m)), first.Id);

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage removed = await PostAsync(client, "RemoveCartItem", [("CartItemId", itemId.ToString())], GuestCart(guestKey));
        using HttpResponseMessage again = await PostAsync(client, "RemoveCartItem", [("CartItemId", itemId.ToString())], GuestCart(guestKey));
        using HttpResponseMessage foreign = await PostAsync(client, "RemoveCartItem", [("CartItemId", foreignItem.ToString())], GuestCart(guestKey));
        using HttpResponseMessage noCookie = await PostAsync(client, "RemoveCartItem", [("CartItemId", foreignItem.ToString())]);

        Assert.Equal(HttpStatusCode.SeeOther, removed.StatusCode);
        Assert.Equal(FramePaths.Cart, removed.Headers.Location?.OriginalString);
        Assert.StartsWith($"{CookieNames.Cart}={guestKey};", Assert.Single(SetCookies(removed), c => c.StartsWith($"{CookieNames.Cart}=", StringComparison.Ordinal)), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, noCookie.StatusCode);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.CartItems.AnyAsync(i => i.Id == itemId, Ct));
        Assert.True(await reader.CartItems.AnyAsync(i => i.Id == foreignItem, Ct));
        Assert.Equal(1, await reader.CartItems.CountAsync(i => i.CartId == cartId, Ct));
    }

    [Fact]
    public async Task AMarkedLine_IsRemovedWhateverItsShowcaseState_T508_KK10()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        Variant variant = await SeedVariantAsync(price: 100m);
        Guid cartId = await SeedCartAsync(customerId, null, (variant, 1, 100m));
        Guid itemId = await ItemIdAsync(cartId, variant.Id);
        await using (AppDbContext db = NewContext())
        {
            await db.CartItems.Where(i => i.Id == itemId).ExecuteUpdateAsync(i => i.SetProperty(x => x.VariantId, (Guid?)null).SetProperty(x => x.RemovedProductName, "Kalkan Ürün"), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage removed = await PostAsync(client, "RemoveCartItem", [("CartItemId", itemId.ToString())], Customer(customerKey), $"{CookieNames.Checkout}=held");
        using HttpResponseMessage again = await PostAsync(client, "RemoveCartItem", [("CartItemId", itemId.ToString())], Customer(customerKey));

        Assert.Equal(HttpStatusCode.SeeOther, removed.StatusCode);
        Assert.True(IsDeletion(Assert.Single(SetCookies(removed), c => c.StartsWith($"{CookieNames.Checkout}=", StringComparison.Ordinal))));
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.CartItems.AnyAsync(i => i.Id == itemId, Ct));
    }

    // ---- helpers ----

    private WebApplicationFactory<Program> Open(bool salesOpen) =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddRazorPages().AddApplicationPart(typeof(FrameSampleModel).Assembly);
            if (salesOpen)
            {
                s.RemoveAll<ISettingsModule>();
                s.AddScoped(sp => OpenGateSettings.Create(ActivatorUtilities.CreateInstance<SettingsModule>(sp)));
            }
        }));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private static string Customer(string key) => $"{CookieNames.Session}={key}";

    private static string GuestCart(string key) => $"{CookieNames.Cart}={key}";

    private static string NewGuestKey() => $"guest-{Guid.NewGuid():N}";

    private static async Task<(HttpStatusCode Status, string Html, IReadOnlyList<string> SetCookies)> GetAsync(WebApplicationFactory<Program> factory, params string[] cookies)
    {
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false });
        using HttpRequestMessage request = new(HttpMethod.Get, FramePaths.Cart);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookies.Length > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies));
        }

        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, await BodyOf(response), SetCookies(response));
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string handler, IEnumerable<(string Name, string Value)> fields, params string[] cookies)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{FramePaths.Cart}?handler={handler}");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookies.Length > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies));
        }

        request.Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        return await client.SendAsync(request, Ct);
    }

    // The default encoder writes Turkish letters as character references; the texts are compared as the browser reads them.
    private static async Task<string> BodyOf(HttpResponseMessage response) => WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

    private static List<string> SetCookies(HttpResponseMessage response) => response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values) ? [.. values] : [];

    private static bool IsDeletion(string setCookie) => setCookie.Contains("expires=Thu, 01 Jan 1970", StringComparison.OrdinalIgnoreCase);

    private static string ProceedOf(string html)
    {
        int start = html.IndexOf("id=\"cart-proceed\"", StringComparison.Ordinal);
        Assert.True(start > 0, "The cart has no place for the payment step's button.");
        int end = html.IndexOf("</section>", start, StringComparison.Ordinal);
        return html[start..end];
    }

    private static string LineOf(string html, Guid itemId)
    {
        int start = html.IndexOf($"id=\"line-{itemId:N}\"", StringComparison.Ordinal);
        Assert.True(start > 0, "The line is not on the page: " + html[Math.Max(0, html.IndexOf("<main", StringComparison.Ordinal))..]);
        int end = html.IndexOf("</li>", start, StringComparison.Ordinal);
        return html[start..end];
    }

    private static async Task<CartView> ReadViewAsync(WebApplicationFactory<Program> factory, CartOwner owner)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        ShippingFeeSection terms = await scope.ServiceProvider.GetRequiredService<ISettingsModule>().GetShippingFeeAsync(Ct);

        // The read takes the cart's lock, which lives in a transaction; nothing of this read is kept.
        await using IDbContextTransaction transaction = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.BeginTransactionAsync(Ct);
        CartView view = await scope.ServiceProvider.GetRequiredService<IOrderingModule>().OpenCartAsync(owner, terms, Ct);
        await transaction.RollbackAsync(Ct);
        return view;
    }

    private async Task SetShippingAsync(decimal fee, decimal? threshold, decimal? minimum)
    {
        await using AppDbContext db = NewContext();
        await db.CompanySettings.ExecuteUpdateAsync(
            s => s.SetProperty(c => c.ShippingFee, fee).SetProperty(c => c.FreeShippingThreshold, threshold).SetProperty(c => c.MinimumOrderAmount, minimum),
            Ct);
    }

    private async Task<int> CountCartsAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.Carts.CountAsync(Ct);
    }

    private async Task<Guid> ItemIdAsync(Guid cartId, Guid variantId)
    {
        await using AppDbContext db = NewContext();
        return await db.CartItems.Where(i => i.CartId == cartId && i.VariantId == variantId).Select(i => i.Id).SingleAsync(Ct);
    }

    private async Task<(int Quantity, decimal FirstPrice)> QuantityAndFirstPriceAsync(Guid itemId)
    {
        await using AppDbContext db = NewContext();
        var row = await db.CartItems.Where(i => i.Id == itemId).Select(i => new { i.Quantity, i.FirstAddedUnitPrice }).SingleAsync(Ct);
        return (row.Quantity, row.FirstAddedUnitPrice);
    }

    private async Task<Variant> SeedVariantAsync(decimal price, int stock = 10, ProductType type = ProductType.Physical)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create($"Urun {n}", $"urun-{Guid.NewGuid():N}", type, 20m, Now.AddMinutes(n)).WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        Variant variant = Variant.Create(product.Id, $"SKU-{Guid.NewGuid():N}"[..20], price, Now.AddMinutes(n));
        variant.PublishStatus = PublishStatus.Published;
        variant.StockQuantity = type == ProductType.Physical ? stock : null;
        await using AppDbContext db = NewContext();
        db.Products.Add(product);
        db.Variants.Add(variant);
        await db.SaveChangesAsync(Ct);
        return variant;
    }

    private async Task<(Variant First, Variant Second)> SeedTwoVariantsOfOneProductAsync(int limit)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create($"Lamba {n}", $"lamba-{Guid.NewGuid():N}", ProductType.Physical, 20m, Now.AddMinutes(n)).WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        Option option = Option.Create(product.Id, "Boy", 1, Now);
        List<Variant> variants = [];
        List<OptionValue> values = [];
        for (int i = 0; i < 2; i++)
        {
            Variant variant = Variant.Create(product.Id, $"SKU-{Guid.NewGuid():N}"[..20], 100m, Now.AddMinutes(n));
            variant.PublishStatus = PublishStatus.Published;
            variant.StockQuantity = 10;
            OptionValue value = OptionValue.Create(option.Id, $"Değer {i}", i + 1, Now);
            variant.FirstOptionValueId = value.Id;
            variants.Add(variant);
            values.Add(value);
        }

        await using AppDbContext db = NewContext();
        db.Products.Add(product);
        db.Options.Add(option);
        db.OptionValues.AddRange(values);
        db.Variants.AddRange(variants);
        await db.SaveChangesAsync(Ct);
        await db.Products.Where(p => p.Id == product.Id).ExecuteUpdateAsync(p => p.SetProperty(x => x.MaxQuantityPerOrder, limit), Ct);
        return (variants[0], variants[1]);
    }

    private async Task<Guid> SeedCartAsync(Guid? customerId, string? guestKey, params (Variant Variant, int Quantity, decimal FirstPrice)[] lines)
    {
        Cart cart = customerId is Guid id ? Cart.ForCustomer(id, DateTimeOffset.UtcNow) : Cart.ForGuest(SecretKey.Hash(guestKey!), DateTimeOffset.UtcNow);
        await using AppDbContext db = NewContext();
        db.Carts.Add(cart);
        int order = 0;
        foreach ((Variant variant, int quantity, decimal firstPrice) in lines)
        {
            db.CartItems.Add(CartItem.Create(cart.Id, variant.Id, quantity, firstPrice, Now.AddSeconds(order++)));
        }

        await db.SaveChangesAsync(Ct);
        return cart.Id;
    }

    /// <summary>The settings module with an open sales gate: every other call goes to the real module (the shipping terms are read from the database).</summary>
    public class OpenGateSettings : DispatchProxy
    {
        private ISettingsModule _inner = null!;

        public static ISettingsModule Create(ISettingsModule inner)
        {
            ISettingsModule proxy = Create<ISettingsModule, OpenGateSettings>();
            ((OpenGateSettings)(object)proxy)._inner = inner;
            return proxy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            ArgumentNullException.ThrowIfNull(targetMethod);
            if (targetMethod.Name == nameof(ISettingsModule.GetSalesGateAsync))
            {
                return Task.FromResult(new SalesGate([], []));
            }

            try
            {
                return targetMethod.Invoke(_inner, args);
            }
            catch (TargetInvocationException e) when (e.InnerException is not null)
            {
                throw e.InnerException;
            }
        }
    }
}
