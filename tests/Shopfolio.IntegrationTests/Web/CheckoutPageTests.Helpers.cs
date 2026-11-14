using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The helpers of the payment step's page tests (T509): the opened application, the requests, the form's values and the seeded rows.</summary>
public sealed partial class CheckoutPageTests
{
    private const string Phone = "5321234567";

    /// <summary>The client address of the test requests (a test server's connection has none; behind the proxy every request has one, 09 15.1 item 8).</summary>
    private static readonly IPAddress ClientIp = IPAddress.Parse("203.0.113.9");

    private WebApplicationFactory<Program> Open(bool salesOpen, bool confirmationFails = false, bool hasAddress = true, bool cardOpen = false, Action<IServiceCollection>? extra = null) =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            if (cardOpen)
            {
                s.RemoveAll<ICardPaymentCondition>();
                s.AddScoped<ICardPaymentCondition>(_ => new OpenCardCondition());
            }

            if (hasAddress)
            {
                s.AddSingleton<IStartupFilter>(new ClientAddressFilter());
            }

            // The sample pages give the forgery token page; their frame sample at the step's own address is put after the real step.
            s.AddRazorPages(o => o.Conventions.AddPageRouteModelConvention(
                "/Storefront/FrameCheckoutSample",
                model =>
                {
                    foreach (SelectorModel selector in model.Selectors)
                    {
                        selector.AttributeRouteModel!.Order = 1;
                    }
                })).AddApplicationPart(typeof(FrameSampleModel).Assembly);
            s.RemoveAll<ISettingsModule>();
            s.AddScoped(sp => GateSettings.Create(ActivatorUtilities.CreateInstance<SettingsModule>(sp), salesOpen));
            if (confirmationFails)
            {
                s.RemoveAll<ICheckout>();
                s.AddScoped<ICheckout>(sp => new FailingConfirmation(ActivatorUtilities.CreateInstance<Checkout>(sp)));
            }

            if (extra is not null)
            {
                extra(s);
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

    private static async Task<(HttpStatusCode Status, string Html, IReadOnlyList<string> SetCookies)> GetAsync(WebApplicationFactory<Program> factory, params string[] cookies)
    {
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false });
        using HttpRequestMessage request = new(HttpMethod.Get, FramePaths.Checkout);
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
        using HttpRequestMessage request = new(HttpMethod.Post, $"{FramePaths.Checkout}?handler={handler}");
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

    // The checkout cookie as the browser sends it back: every part of it (the framework's chunked cookie manager may split it).
    private static string CheckoutCookieOf(HttpResponseMessage response)
    {
        string[] parts = [.. SetCookies(response).Where(c => c.StartsWith(CookieNames.Checkout, StringComparison.Ordinal) && !IsDeletion(c)).Select(c => c[..c.IndexOf(';', StringComparison.Ordinal)])];
        Assert.NotEmpty(parts);
        return string.Join("; ", parts);
    }

    // The value of a hidden field of the page.
    private static string Hidden(string html, string name)
    {
        Match tag = Regex.Match(html, $"<input[^>]*name=\"{Regex.Escape(name)}\"[^>]*>");
        Assert.True(tag.Success, $"The page has no field {name}.");
        Match value = Regex.Match(tag.Value, "value=\"([^\"]*)\"");
        return value.Success ? value.Groups[1].Value : string.Empty;
    }

    // The text of a legal text's scrolling box, labelled by its heading (04 5.13.9 (c)-(d)).
    private static string LegalBox(string html, string headingId)
    {
        Assert.Contains($"<h3 id=\"{headingId}\">", html, StringComparison.Ordinal);
        Match box = Regex.Match(html, $"<div class=\"checkout__legal-box\" role=\"region\" aria-labelledby=\"{Regex.Escape(headingId)}\" tabindex=\"0\"><pre class=\"legal-text__plain\">(.*?)</pre></div>", RegexOptions.Singleline);
        Assert.True(box.Success, $"The page has no open box for {headingId}.");
        return box.Groups[1].Value;
    }

    // The values the page carries back - the key, the fingerprint, the versions - and, for a guest's form, the e-mail field.
    private static IEnumerable<(string Name, string Value)> Fields(string page, bool guest, string email = "")
    {
        yield return ("ConfirmationKey", Hidden(page, "ConfirmationKey"));
        yield return (nameof(PlaceOrderInput.SummaryFingerprint), Hidden(page, nameof(PlaceOrderInput.SummaryFingerprint)));
        yield return (nameof(PlaceOrderInput.PreInformationFormVersionId), Hidden(page, nameof(PlaceOrderInput.PreInformationFormVersionId)));
        yield return (nameof(PlaceOrderInput.DistanceSalesContractVersionId), Hidden(page, nameof(PlaceOrderInput.DistanceSalesContractVersionId)));
        if (guest)
        {
            yield return (nameof(PlaceOrderInput.ContactEmail), email);
        }
    }

    // A form the confirmation accepts: the shipping address, billing the same, the transfer, the two boxes ticked.
    private IEnumerable<(string Name, string Value)> Complete(string page, Guid? province = null, string method = "BankTransfer", bool guest = true)
    {
        foreach ((string, string) field in Fields(page, guest, guest ? "alici@example.com" : string.Empty))
        {
            yield return field;
        }

        yield return (nameof(PlaceOrderInput.ShippingRecipientName), "Ayşe Yılmaz");
        yield return (nameof(PlaceOrderInput.ShippingProvinceId), (province ?? _provinceId).ToString());
        yield return (nameof(PlaceOrderInput.ShippingDistrict), "Çankaya");
        yield return (nameof(PlaceOrderInput.ShippingAddressLine), "Kızılay Mah. 1");
        yield return (nameof(PlaceOrderInput.ShippingPhone), Phone);
        yield return (nameof(PlaceOrderInput.IsBillingSameAsShipping), "true");
        yield return (nameof(PlaceOrderInput.PaymentMethod), method);
        yield return (nameof(PlaceOrderInput.AcceptPreInformation), "true");
        yield return (nameof(PlaceOrderInput.AcceptDistanceSales), "true");
    }

    /// <summary>The submissions of the step that sends a request away from it: the GET and the three POSTs that show the step again.</summary>
    private static readonly string[] AwayHandlers = ["ApplyCoupon", "RemoveCoupon", "Recalculate"];

    /// <summary>What the step answers to its GET and its three re-showing POSTs, as "request status location" lines (the GET's line has its status only).</summary>
    private static async Task<IReadOnlyList<string>> AnswersOfTheStepAsync(WebApplicationFactory<Program> factory, string guest)
    {
        (HttpStatusCode status, _, _) = await GetAsync(factory, GuestCart(guest));
        List<string> answers = [$"GET {(int)status}"];
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        foreach (string handler in AwayHandlers)
        {
            using HttpResponseMessage response = await PostAsync(client, handler, [("ContactEmail", string.Empty), ("CouponCode", "KOD")], GuestCart(guest));
            answers.Add($"{handler} {(int)response.StatusCode} {response.Headers.Location?.OriginalString}");
        }

        return answers;
    }

    /// <summary>The answers of <see cref="AnswersOfTheStepAsync"/> when every one goes to the cart.</summary>
    private static IReadOnlyList<string> AllToTheCart()
    {
        return [$"GET {(int)HttpStatusCode.SeeOther}", .. AwayHandlers.Select(h => $"{h} {(int)HttpStatusCode.SeeOther} {FramePaths.Cart}")];
    }

    private async Task<Guid> SeedVariantAsync(ProductType type, decimal price, int stock)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create($"Ürün {n}", $"urun-t509-{Guid.NewGuid():N}", type, 20m, Now.AddMinutes(n));
        if (type == ProductType.Service)
        {
            product.FulfilmentPeriod = 3;
        }
        else
        {
            product = product.WithSeededOrigin();
        }

        product.PublishStatus = PublishStatus.Published;
        Variant variant = Variant.Create(product.Id, $"SKU-{Guid.NewGuid():N}"[..20], price, Now.AddMinutes(n));
        variant.PublishStatus = PublishStatus.Published;
        variant.StockQuantity = type == ProductType.Physical ? stock : null;
        await using AppDbContext db = NewContext();
        db.Products.Add(product);
        db.Variants.Add(variant);
        await db.SaveChangesAsync(Ct);
        return variant.Id;
    }

    private async Task<string> SeedGuestCartAsync(ProductType type, decimal price = 100m, int stock = 10)
    {
        Guid variantId = await SeedVariantAsync(type, price, stock);
        string key = $"guest-{Guid.NewGuid():N}";
        Cart cart = Cart.ForGuest(SecretKey.Hash(key), DateTimeOffset.UtcNow);
        await using AppDbContext db = NewContext();
        db.Carts.Add(cart);
        db.CartItems.Add(CartItem.Create(cart.Id, variantId, 1, price, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(Ct);
        return key;
    }

    private async Task SeedMemberCartAsync(Guid customerId)
    {
        Guid variantId = await SeedVariantAsync(ProductType.Physical, 100m, 10);
        Cart cart = Cart.ForCustomer(customerId, DateTimeOffset.UtcNow);
        await using AppDbContext db = NewContext();
        db.Carts.Add(cart);
        db.CartItems.Add(CartItem.Create(cart.Id, variantId, 1, 100m, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(Ct);
    }

    private async Task SeedCouponAsync(string code, decimal? minimumBasket)
    {
        Coupon coupon = Coupon.CreatePercentage(code, 10m, new DateOnly(2026, 1, 1), new DateOnly(2030, 12, 31), 10, Now);
        coupon.MinimumBasketAmount = minimumBasket;
        await using AppDbContext db = NewContext();
        db.Coupons.Add(coupon);
        await db.SaveChangesAsync(Ct);
    }

    private Task SetMinimumAsync(decimal minimum) => ExecuteAsync($"UPDATE company_settings SET minimum_order_amount = {minimum.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<long> ScalarAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return Convert.ToInt64(await command.ExecuteScalarAsync(Ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    private async Task<string> ScalarTextAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return Convert.ToString(await command.ExecuteScalarAsync(Ct), System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>The settings module with the sales gate a test chooses - open, or closed by the temporary closure; every other call goes to the real module.</summary>
    public class GateSettings : System.Reflection.DispatchProxy
    {
        private ISettingsModule _inner = null!;
        private bool _open;

        public static ISettingsModule Create(ISettingsModule inner, bool open)
        {
            ISettingsModule proxy = Create<ISettingsModule, GateSettings>();
            ((GateSettings)(object)proxy)._inner = inner;
            ((GateSettings)(object)proxy)._open = open;
            return proxy;
        }

        protected override object? Invoke(System.Reflection.MethodInfo? targetMethod, object?[]? args)
        {
            ArgumentNullException.ThrowIfNull(targetMethod);
            if (targetMethod.Name == nameof(ISettingsModule.GetSalesGateAsync))
            {
                return Task.FromResult(_open ? new SalesGate([], []) : new SalesGate([SalesGateCondition.TemporaryClosure], []));
            }

            try
            {
                return targetMethod.Invoke(_inner, args);
            }
            catch (System.Reflection.TargetInvocationException e) when (e.InnerException is not null)
            {
                throw e.InnerException;
            }
        }
    }

    /// <summary>Gives every request of the test server the client address <see cref="ClientIp"/> before the application's pipeline.</summary>
    private sealed class ClientAddressFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            ArgumentNullException.ThrowIfNull(next);
            return app =>
            {
                app.Use((context, forward) =>
                {
                    context.Connection.RemoteIpAddress = ClientIp;
                    return forward(context);
                });
                next(app);
            };
        }
    }

    /// <summary>The real step whose confirmation fails after its work, inside the open transaction: the error the step did not expect (09 8.1 item 4; 07 6.4.2).</summary>
    private sealed class FailingConfirmation(ICheckout inner) : ICheckout
    {
        public Task<CheckoutStep> ReadAsync(CheckoutInput input, bool appliesCoupon, CancellationToken ct) => inner.ReadAsync(input, appliesCoupon, ct);

        public async Task<PlaceOrderResult> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct)
        {
            await inner.PlaceOrderAsync(request, ct);
            throw new InvalidOperationException("The confirmation failed after its work.");
        }

        public Task MarkPaymentUnavailableAsync(CartOwner owner, CancellationToken ct) => inner.MarkPaymentUnavailableAsync(owner, ct);

        public Task<bool> ClosesBankTransferAsync(CartOwner owner, IPAddress? address, CancellationToken ct) => inner.ClosesBankTransferAsync(owner, address, ct);
    }

    /// <summary>The card's line open: the provider's keys defined at installation (02 3.21.2); the test installation has none.</summary>
    private sealed class OpenCardCondition : ICardPaymentCondition
    {
        public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(true);
    }
}
