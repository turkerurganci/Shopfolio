using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Orders;
using Shopfolio.Pages.Payment;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// E-14 and the payment return on the opened application (T510 KK1-KK9; 07 UC-28…UC-30; 04 5.14, 3.5.8): the orders are placed through the payment step itself, so the
/// confirmation screen is reached the way a customer reaches it. The provider is the hand-written fake; the frame's address is under the registered form root.
/// </summary>
public sealed partial class CheckoutPageTests
{
    private const string FrameSourcePolicy = "frame-src 'self' ";

    private static Uri FrameAddress(string part) => new(PayTrStartForm.FrameAddressRoot + part);

    private WebApplicationFactory<Program> OpenWithGateway(FakePaymentGateway gateway) =>
        Open(salesOpen: true, cardOpen: true, extra: s =>
        {
            s.RemoveAll<IPaymentGateway>();
            s.AddSingleton<IPaymentGateway>(gateway);
        });

    private static async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string path, params string[] cookies)
    {
        using HttpRequestMessage request = new(method, path);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookies.Length > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies));
        }

        if (method == HttpMethod.Post)
        {
            request.Content = new FormUrlEncodedContent([]);
        }

        return await client.SendAsync(request, Ct);
    }

    private static HttpClient PlainClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false });

    private static string Policy(HttpResponseMessage response) => string.Join(" ", response.Headers.GetValues(ContentSecurityPolicy.HeaderName));

    // Places a guest's order through the step and gives the confirmation screen's address (the access-key root).
    private async Task<(string Screen, string Guest)> PlaceGuestOrderAsync(WebApplicationFactory<Program> factory, string method)
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = (await GetAsync(factory, GuestCart(guest))).Html;
        using HttpResponseMessage placed = await PostAsync(client, "PlaceOrder", [.. Complete(page, method: method)], GuestCart(guest));
        Assert.Equal(HttpStatusCode.SeeOther, placed.StatusCode);
        return (placed.Headers.Location!.OriginalString, guest);
    }

    private static string KeyOf(string screen) => screen.Split('/')[3];

    private Task<string> OrderStateAsync() => ScalarTextAsync("SELECT order_status || '+' || payment_status FROM orders ORDER BY created_at DESC LIMIT 1");

    // ---- KK1, KK3, KK4: the card's screen, the payment page and its frame permission ----

    [Fact]
    public async Task TheCardScreen_ShowsTheNumberAndTheButton_AndEveryOpeningOfThePaymentPageAsksANewAddress_KeptNowhere_T510_KK1_KK3_KK4()
    {
        FakePaymentGateway gateway = new();
        await using WebApplicationFactory<Program> factory = OpenWithGateway(gateway);
        (string screen, string guest) = await PlaceGuestOrderAsync(factory, "Card");
        Assert.Matches("^/siparis/erisim/[A-Za-z0-9_-]+/teyit$", screen);
        string number = await ScalarTextAsync("SELECT order_number FROM orders");

        using HttpClient plain = PlainClient(factory);
        using HttpResponseMessage get = await SendAsync(plain, HttpMethod.Get, screen);
        string html = await BodyOf(get);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Contains($"Sipariş {number}", html, StringComparison.Ordinal);
        Assert.Contains(ConfirmationPageModel.ReceivedText, html, StringComparison.Ordinal);
        Assert.Contains(ConfirmationPageModel.CardWaitingText, html, StringComparison.Ordinal);
        Assert.Contains($"action=\"{screen}?handler=StartPayment\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("IBAN", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<iframe", html, StringComparison.Ordinal);
        Assert.DoesNotContain("http-equiv=\"refresh\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<meta name=\"robots\" content=\"noindex\"", html, StringComparison.Ordinal);
        Assert.Contains("frame-src 'none'", Policy(get), StringComparison.Ordinal);
        Assert.Equal("no-store", get.Headers.CacheControl?.ToString());

        gateway.EnqueueStart(new PaymentStartResult.Started(FrameAddress("ilk-acilis")), new PaymentStartResult.Started(FrameAddress("ikinci-acilis")));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage first = await SendAsync(client, HttpMethod.Post, screen + "?handler=StartPayment");
        string firstHtml = await BodyOf(first);
        using HttpResponseMessage second = await SendAsync(client, HttpMethod.Post, screen + "?handler=StartPayment");
        string secondHtml = await BodyOf(second);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Matches(new Regex($"<iframe class=\"payment-page__frame\" src=\"{Regex.Escape(FrameAddress("ilk-acilis").AbsoluteUri)}\""), firstHtml);
        Assert.DoesNotContain("sandbox", firstHtml, StringComparison.Ordinal);
        Assert.Contains(FrameAddress("ikinci-acilis").AbsoluteUri, secondHtml, StringComparison.Ordinal);
        Assert.Equal(2, gateway.Calls.Count(c => c.Job == nameof(IPaymentGateway.StartAsync)));

        // KK4: the frame permission names the site and the provider's origin on the payment page only; form-action does not carry the provider.
        string policy = Policy(first);
        Assert.Contains(FrameSourcePolicy + PayTrStartForm.FrameOrigin, policy, StringComparison.Ordinal);
        Assert.Contains("frame-ancestors 'none'", policy, StringComparison.Ordinal);
        Assert.Contains("script-src 'self';", policy, StringComparison.Ordinal);
        Assert.Contains($"form-action 'self' {ContentSecurityPolicy.GoogleAuthorizationOrigin}", policy, StringComparison.Ordinal);
        Assert.DoesNotContain(PayTrStartForm.FrameOrigin, policy.Split("form-action")[1], StringComparison.Ordinal);
        Assert.Equal(SecurityHeaders.ReferrerPolicyKeyedPage, string.Join(" ", first.Headers.GetValues("Referrer-Policy")));

        // KK3: the address is kept nowhere - no column of the order carries it - and the order still waits for its payment.
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM orders o WHERE o::text LIKE '%acilis%'"));
        Assert.Equal("Placed+Pending", await OrderStateAsync());

        // The cart page and the screen's own GET keep the frame closed.
        using HttpResponseMessage cart = await SendAsync(plain, HttpMethod.Get, FramePaths.Cart, GuestCart(guest));
        Assert.Contains("frame-src 'none'", Policy(cart), StringComparison.Ordinal);
    }

    // ---- KK5: the provider cannot be reached ----

    [Fact]
    public async Task AnUnreachableProvider_CancelsTheOrderOutsideL7_MarksTheCart_AndTheCartTellsItOnce_T510_KK5()
    {
        FakePaymentGateway gateway = new();
        await using WebApplicationFactory<Program> factory = OpenWithGateway(gateway);
        (string screen, string guest) = await PlaceGuestOrderAsync(factory, "Card");
        gateway.EnqueueStart(new PaymentStartResult.ConnectionFailed(ExternalCallFailure.TimeoutReason));

        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage start = await SendAsync(client, HttpMethod.Post, screen + "?handler=StartPayment", GuestCart(guest));

        Assert.Equal(HttpStatusCode.SeeOther, start.StatusCode);
        Assert.Equal(FramePaths.Cart, start.Headers.Location?.OriginalString);
        Assert.Equal("Cancelled+Failed", await OrderStateAsync());
        // The system's cancellation writes the order's and the payment's records (O2, S3), both with the reason that L-7 does not count.
        Assert.Equal("O2,S3", await ScalarTextAsync("SELECT string_agg(transition_code, ',' ORDER BY transition_code) FROM transition_records WHERE transition_reason = 'PaymentPageNotOpened' AND actor_kind = 'System'"));
        Assert.DoesNotContain(Shopfolio.Modules.Ordering.Contracts.TransitionReason.PaymentPageNotOpened, Shopfolio.Modules.Ordering.UnpaidCancellationLimit.CountedReasons);
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM stock_reservations"));

        using HttpClient plain = PlainClient(factory);
        string told = await BodyOf(await SendAsync(plain, HttpMethod.Get, FramePaths.Cart, GuestCart(guest)));
        string again = await BodyOf(await SendAsync(plain, HttpMethod.Get, FramePaths.Cart, GuestCart(guest)));
        Assert.Equal(1, Regex.Count(told, Regex.Escape(CartTexts.PaymentUnavailable)));
        Assert.DoesNotContain(CartTexts.PaymentUnavailable, again, StringComparison.Ordinal);
        Assert.Equal(1, await ScalarAsync("SELECT count(*) FROM cart_items"));

        // The cancelled order's screen is the guest's 303 to the order tracking (K-1209 (1)).
        using HttpResponseMessage reopened = await SendAsync(plain, HttpMethod.Get, screen);
        Assert.Equal(HttpStatusCode.SeeOther, reopened.StatusCode);
        Assert.Equal(FramePaths.OrderTracking, reopened.Headers.Location?.OriginalString);
    }

    // ---- KK6: the second token refusal, the transfer and the order that moved on ----

    [Fact]
    public async Task TheSecondTokenRefusal_ReopensTheScreenWithoutACancellation_AndAPaidOrderGoesToTheOrderTracking_T510_KK6()
    {
        FakePaymentGateway gateway = new();
        await using WebApplicationFactory<Program> factory = OpenWithGateway(gateway);
        (string screen, _) = await PlaceGuestOrderAsync(factory, "Card");
        gateway.EnqueueStart(new PaymentStartResult.SecondTokenRejected("token-reuse"));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage refused = await SendAsync(client, HttpMethod.Post, screen + "?handler=StartPayment");
        Assert.Equal(HttpStatusCode.SeeOther, refused.StatusCode);
        Assert.Equal(screen, refused.Headers.Location?.OriginalString);
        Assert.Equal("Placed+Pending", await OrderStateAsync());

        await ExecuteAsync("UPDATE orders SET payment_status = 'Paid'");
        using HttpResponseMessage paid = await SendAsync(client, HttpMethod.Post, screen + "?handler=StartPayment");
        Assert.Equal(HttpStatusCode.SeeOther, paid.StatusCode);
        Assert.Equal(FramePaths.OrderTracking, paid.Headers.Location?.OriginalString);
        Assert.Equal(1, gateway.Calls.Count(c => c.Job == nameof(IPaymentGateway.StartAsync)));
    }

    [Fact]
    public async Task TheTransferScreen_ShowsTheFrozenIbanTotalLastDayAndTheWay_AndItsStartReopensIt_T510_KK1_KK6()
    {
        FakePaymentGateway gateway = new();
        await using WebApplicationFactory<Program> factory = OpenWithGateway(gateway);
        (string screen, _) = await PlaceGuestOrderAsync(factory, "BankTransfer");
        await ExecuteAsync("UPDATE company_settings SET bank_transfer_iban = 'TR120006200000000000000001'");
        using HttpClient plain = PlainClient(factory);

        string html = await BodyOf(await SendAsync(plain, HttpMethod.Get, screen));
        Assert.Contains("TR33 0006 1005 1978 6457 8413 26", html, StringComparison.Ordinal);
        Assert.Contains("Ödenecek toplam", html, StringComparison.Ordinal);
        Assert.Contains("Son ödeme günü", html, StringComparison.Ordinal);
        Assert.Contains(ConfirmationPageModel.GuestOrderWayText, html, StringComparison.Ordinal);
        Assert.Contains($"href=\"{FramePaths.OrderTracking}\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("handler=StartPayment", html, StringComparison.Ordinal);

        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage start = await SendAsync(client, HttpMethod.Post, screen + "?handler=StartPayment");
        Assert.Equal(HttpStatusCode.SeeOther, start.StatusCode);
        Assert.Equal(screen, start.Headers.Location?.OriginalString);
        Assert.Empty(gateway.Calls);
    }

    // ---- KK2: the two redirects and the 404 ----

    [Fact]
    public async Task TheMembersScreen_LinksTheOrderPage_GoesThereOnceTheOrderMovedOn_AndAWrongRootIs404_T510_KK2()
    {
        (string memberKey, Guid memberId) = await TestSessions.CustomerAsync(_database.Dsn);
        await SeedMemberCartAsync(memberId);
        FakePaymentGateway gateway = new();
        await using WebApplicationFactory<Program> factory = OpenWithGateway(gateway);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = (await GetAsync(factory, Customer(memberKey))).Html;
        using HttpResponseMessage placed = await PostAsync(client, "PlaceOrder", [.. Complete(page, guest: false)], Customer(memberKey));
        string screen = placed.Headers.Location!.OriginalString;
        string number = await ScalarTextAsync("SELECT order_number FROM orders");
        Assert.Equal($"/siparis/{number}/teyit", screen);

        using HttpClient plain = PlainClient(factory);
        string html = await BodyOf(await SendAsync(plain, HttpMethod.Get, screen, Customer(memberKey)));
        Assert.Contains($"href=\"/siparis/{number}\"", html, StringComparison.Ordinal);
        Assert.Contains(ConfirmationPageModel.MemberOrderLinkText, html, StringComparison.Ordinal);

        await ExecuteAsync("UPDATE orders SET order_status = 'Cancelled', payment_status = 'Failed'");
        using HttpResponseMessage moved = await SendAsync(plain, HttpMethod.Get, screen, Customer(memberKey));
        Assert.Equal(HttpStatusCode.SeeOther, moved.StatusCode);
        Assert.Equal($"/siparis/{number}", moved.Headers.Location?.OriginalString);

        (string otherKey, _) = await TestSessions.CustomerAsync(_database.Dsn);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(plain, HttpMethod.Get, screen, Customer(otherKey))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(plain, HttpMethod.Get, "/siparis/erisim/YokBoyleBirAnahtar/teyit")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(client, HttpMethod.Post, "/siparis/erisim/YokBoyleBirAnahtar/teyit?handler=StartPayment")).StatusCode);
        Assert.Empty(gateway.Calls);
    }

    // ---- KK7, KK8, KK9: the return's hand-over page ----

    [Fact]
    public async Task TheReturn_IsAOneSentenceHandOverWithoutTheFrame_WritesNothing_AndOnlyItMayBeFramedBySelf_T510_KK7_KK8_KK9()
    {
        FakePaymentGateway gateway = new();
        await using WebApplicationFactory<Program> factory = OpenWithGateway(gateway);
        (string screen, _) = await PlaceGuestOrderAsync(factory, "Card");
        string key = KeyOf(screen);
        string before = await ScalarTextAsync("SELECT o::text FROM orders o");
        long records = await ScalarAsync("SELECT count(*) FROM transition_records");
        using HttpClient plain = PlainClient(factory);

        // KK7: without any cookie, with the provider's own parameters added; the page is the same each time.
        using HttpResponseMessage back = await SendAsync(plain, HttpMethod.Get, $"{PayTrStartForm.ReturnPathRoot}{key}?merchant_oid=x&status=success");
        string html = await BodyOf(back);
        Assert.Equal(HttpStatusCode.OK, back.StatusCode);
        Assert.DoesNotContain("<header", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<footer", html, StringComparison.Ordinal);
        Assert.Contains($"{ReturnModel.SentenceText} <a href=\"/siparis/erisim/{key}\" target=\"_top\" data-js=\"payment-return\">{ReturnModel.LinkText}</a>.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("merchant_oid", html, StringComparison.Ordinal);
        Assert.Matches("<script type=\"module\" src=\"/statik/js/pages/payment/return\\.[^\"]+\\.js\"></script>", html);
        Assert.Contains("<meta name=\"robots\" content=\"noindex\"", html, StringComparison.Ordinal);
        Assert.Equal(before, await ScalarTextAsync("SELECT o::text FROM orders o"));
        Assert.Equal(records, await ScalarAsync("SELECT count(*) FROM transition_records"));
        Assert.Empty(gateway.Calls);

        // KK8: frame-ancestors 'self' on this response only; the order page it hands over to stays unframeable; no referrer, not stored.
        string policy = Policy(back);
        Assert.Contains("frame-ancestors 'self'", policy, StringComparison.Ordinal);
        Assert.Contains("frame-src 'none'", policy, StringComparison.Ordinal);
        Assert.Equal(SecurityHeaders.ReferrerPolicyKeyedPage, string.Join(" ", back.Headers.GetValues("Referrer-Policy")));
        Assert.Equal("no-store", back.Headers.CacheControl?.ToString());
        using HttpResponseMessage order = await SendAsync(plain, HttpMethod.Get, $"/siparis/erisim/{key}");
        Assert.Equal(HttpStatusCode.OK, order.StatusCode);
        Assert.Contains("frame-ancestors 'none'", Policy(order), StringComparison.Ordinal);

        // KK9: a value outside the key's form is 404 and is never written; a well-formed but unknown key gets the page (200) and the order page's own 404.
        foreach (string wrong in new[] { "a.b", "a%3Cb", "a%20b" })
        {
            using HttpResponseMessage refused = await SendAsync(plain, HttpMethod.Get, PayTrStartForm.ReturnPathRoot + wrong);
            Assert.Equal(HttpStatusCode.NotFound, refused.StatusCode);
            Assert.DoesNotContain("frame-ancestors 'self'", Policy(refused), StringComparison.Ordinal);
        }

        using HttpResponseMessage unknown = await SendAsync(plain, HttpMethod.Get, PayTrStartForm.ReturnPathRoot + "BilinmeyenAnahtar_1");
        Assert.Equal(HttpStatusCode.OK, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(plain, HttpMethod.Get, "/siparis/erisim/BilinmeyenAnahtar_1")).StatusCode);

        // The return has no POST arm (07 UC-30; DV-4): a POST writes nothing.
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage post = await SendAsync(client, HttpMethod.Post, PayTrStartForm.ReturnPathRoot + key);
        Assert.NotEqual(HttpStatusCode.OK, post.StatusCode);
        Assert.Equal(before, await ScalarTextAsync("SELECT o::text FROM orders o"));
    }
}
