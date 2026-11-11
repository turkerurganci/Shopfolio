using System.Net;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The two identity schemes and the closed doors on the opened application with test-only sample pages (T207 criteria 1-9; KK1-KK9; 07 2.3; 05 3.5, 6.1, 6.2;
/// 09 15.1 item 2): the session cookie of each scheme, the panel and the account closed by folder, the open pages, the 302 and 303 to the sign-in page with the
/// return address, the sign-out that is not sent to the sign-in page, the one 404 of a denied access, the guest order that opens only with its access key
/// and the administrator session that is no customer. Sessions, accounts and orders are real rows of the collection database; the pages live in the test host only.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class SessionAccessTests
{
    private const string Evil = "https://evil.example";

    private readonly CollectionDatabaseFixture _database;

    public SessionAccessTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static string CustomerCookie(string key) => $"{CookieNames.Session}={key}";

    private static string AdminCookie(string key) => $"{CookieNames.AdminSession}={key}";

    private WebApplicationFactory<Program> Open(out AccessWriteLog log, bool withUploadEndpoint = false)
    {
        AccessWriteLog writes = new();
        log = writes;
        ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        return factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddSingleton(writes);
            s.AddRazorPages().AddApplicationPart(typeof(PanelOrdersModel).Assembly);
            if (withUploadEndpoint)
            {
                s.AddSingleton<IStartupFilter, UploadEndpointStartupFilter>();
            }
        }));
    }

    private static HttpClient ClientOf(WebApplicationFactory<Program> factory) =>
        ForgeryTokenHandler.CreateClient(factory);

    private static async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string path, string? cookie = null, HttpContent? content = null)
    {
        using HttpRequestMessage request = new(method, path) { Content = content };
        if (cookie is not null)
        {
            request.Headers.Add("Cookie", cookie);
        }

        return await client.SendAsync(request, Ct);
    }

    private static Task<HttpResponseMessage> GetAsync(HttpClient client, string path, string? cookie = null) => SendAsync(client, HttpMethod.Get, path, cookie);

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string? cookie = null, params (string Name, string Value)[] fields) =>
        SendAsync(client, HttpMethod.Post, path, cookie, new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))));

    private static async Task<string> BodyOf(HttpResponseMessage response) => await response.Content.ReadAsStringAsync(Ct);

    // The 404 answer is the E-06 page: the reason shows in no byte of its content. The frame around it follows the viewer (account area, sign-in return address, nonce), so the
    // comparison is of the page's own content region.
    private static async Task<string> PageOf(HttpResponseMessage response)
    {
        System.Text.RegularExpressions.Match content = System.Text.RegularExpressions.Regex.Match(
            await BodyOf(response),
            "<main .*?</main>",
            System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(5));
        Assert.True(content.Success);
        return content.Value;
    }

    private static string Escaped(string path) => Uri.EscapeDataString(path);

    // --- criterion 1, KK1: two schemes, two cookies ---

    [Fact]
    public async Task EachScheme_ReadsItsOwnCookieAndTheOtherAccountTypesSessionIsNoSession()
    {
        (string customerKey, _) = await TestSessions.CustomerAsync(_database.Dsn);
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        Assert.NotEqual(CookieNames.Session, CookieNames.AdminSession);
        using (HttpResponseMessage own = await GetAsync(client, "/hesap", CustomerCookie(customerKey)))
        {
            Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        }

        using (HttpResponseMessage panel = await GetAsync(client, "/panel/siparisler", AdminCookie(adminKey)))
        {
            Assert.Equal(HttpStatusCode.OK, panel.StatusCode);
        }

        // A customer's key in the panel's cookie, an administrator's key in the customer's cookie, and each cookie on the other side: no session.
        foreach ((string path, string cookie) in new[]
        {
            ("/panel/siparisler", AdminCookie(customerKey)),
            ("/hesap", CustomerCookie(adminKey)),
            ("/panel/siparisler", CustomerCookie(customerKey)),
            ("/hesap", AdminCookie(adminKey)),
        })
        {
            using HttpResponseMessage response = await GetAsync(client, path, cookie);
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("unknown-key-that-is-not-a-session")]
    public async Task ACookieThatHoldsNoSession_IsNoSession(string value)
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await GetAsync(client, "/hesap", CustomerCookie(value));

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }

    // --- criterion 2, KK2: the panel is closed by default ---

    [Fact]
    public async Task Panel_WithoutASession_GoesToTheSignInPage_AndTheOpenPagesDoNot()
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using (HttpResponseMessage closed = await GetAsync(client, "/panel/siparisler"))
        {
            Assert.Equal(HttpStatusCode.Found, closed.StatusCode);
            Assert.Equal($"/panel/giris?returnUrl={Escaped("/panel/siparisler")}", closed.Headers.Location?.OriginalString);
        }

        foreach (string open in new[] { "/panel/giris", "/panel/sifre-sifirlama", "/panel/sifre-sifirlama/abc" })
        {
            using HttpResponseMessage response = await GetAsync(client, open);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // A record page of the panel is closed too: the sign-in page comes first and the record's existence is not told (07 2.3.3).
        using HttpResponseMessage record = await GetAsync(client, $"/panel/kayit/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Found, record.StatusCode);
    }

    [Fact]
    public async Task Panel_WithAnAdministratorSession_OpensItsPages()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await GetAsync(client, "/panel/siparisler", AdminCookie(adminKey));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("panel-orders", await BodyOf(response));
    }

    [Fact]
    public async Task TheChunkedUploadEndpointOfThePanel_AsksForThePanelsSession()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        (string customerKey, _) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open(out _, withUploadEndpoint: true);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage anonymous = await PostAsync(client, "/panel/yukleme-deneme");
        using HttpResponseMessage customer = await PostAsync(client, "/panel/yukleme-deneme", CustomerCookie(customerKey));
        using HttpResponseMessage admin = await PostAsync(client, "/panel/yukleme-deneme", AdminCookie(adminKey));

        Assert.Equal(HttpStatusCode.SeeOther, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, customer.StatusCode);
        Assert.Equal(HttpStatusCode.OK, admin.StatusCode);
    }

    // --- criterion 3, KK3: /hesap is closed, the rest of the storefront and the machine endpoints are open ---

    [Fact]
    public async Task Account_WithoutASession_GoesToTheSignInPage_AndTheDeletedStateAndTheOpenStorefrontDoNot()
    {
        (_, _, string accessKey) = await TestSessions.OrderAsync(_database.Dsn, customerId: null);
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        foreach (string closed in new[] { "/hesap", "/hesap/adresler" })
        {
            using HttpResponseMessage response = await GetAsync(client, closed);
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal($"/giris?returnUrl={Escaped(closed)}", response.Headers.Location?.OriginalString);
        }

        foreach (string open in new[] { "/hesap/silindi", "/sss/urun-ornegi/ahsap-masa", "/sayfa/hakkinda", "/sistem/canlilik", $"/siparis/erisim/{accessKey}" })
        {
            using HttpResponseMessage response = await GetAsync(client, open);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    // --- criterion 4, KK4: 302 with the return address ---

    [Fact]
    public async Task AGetThatNeedsASession_Is302WithTheAddressAndTheQueryInReturnUrl()
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage panel = await GetAsync(client, "/panel/siparisler?page=2");
        using HttpResponseMessage account = await GetAsync(client, "/hesap/adresler?x=1&y=a%20b");

        Assert.Equal(HttpStatusCode.Found, panel.StatusCode);
        Assert.Equal($"/panel/giris?returnUrl={Escaped("/panel/siparisler?page=2")}", panel.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.Found, account.StatusCode);
        Assert.Equal($"/giris?returnUrl={Escaped("/hesap/adresler?x=1&y=a%20b")}", account.Headers.Location?.OriginalString);
    }

    // --- criterion 5, KK5: a POST without a session writes nothing and goes to the sign-in page with 303 ---

    [Fact]
    public async Task APostWithoutASession_Is303ToTheSignInPageWithTheFormsGetAddress_AndWritesNothing()
    {
        await using WebApplicationFactory<Program> factory = Open(out AccessWriteLog log);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage panel = await PostAsync(client, "/panel/siparisler?handler=Save");
        using HttpResponseMessage account = await PostAsync(client, "/hesap/adresler?handler=Save&page=2");

        Assert.Equal(HttpStatusCode.SeeOther, panel.StatusCode);
        Assert.Equal($"/panel/giris?returnUrl={Escaped("/panel/siparisler")}&{NoticeParameter.ParameterName}={NoticeCode.SessionExpired}", panel.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.SeeOther, account.StatusCode);
        Assert.Equal($"/giris?returnUrl={Escaped("/hesap/adresler?page=2")}&{NoticeParameter.ParameterName}={NoticeCode.SessionExpired}", account.Headers.Location?.OriginalString);
        Assert.Empty(log.Snapshot());
    }

    // D-121 (07 2.3.2, 6.3.2): the 303 of a POST whose session ended says so with SessionExpired; the 302 of a GET and the sign-out do not.
    [Fact]
    public async Task OnlyAPostsRedirectToTheSignInPage_CarriesTheSessionExpiredNotice_D121()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);
        await using (AppDbContext db = NewContext())
        {
            await db.Sessions.Where(s => s.CustomerId == customerId).ExecuteDeleteAsync(Ct);
        }

        using HttpResponseMessage ended = await PostAsync(client, "/hesap/adresler?handler=Save", CustomerCookie(customerKey));
        using HttpResponseMessage get = await GetAsync(client, "/hesap/adresler", CustomerCookie(customerKey));
        using HttpResponseMessage signOut = await PostAsync(client, "/panel/cikis?handler=Save");

        Assert.Equal($"/giris?returnUrl={Escaped("/hesap/adresler")}&{NoticeParameter.ParameterName}={NoticeCode.SessionExpired}", ended.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.Found, get.StatusCode);
        Assert.Equal($"/giris?returnUrl={Escaped("/hesap/adresler")}", get.Headers.Location?.OriginalString);
        Assert.DoesNotContain(NoticeParameter.ParameterName, signOut.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.Equal(NoticeKind.Result, NoticeKinds.Of(NoticeCode.SessionExpired));
    }

    [Fact]
    public async Task APostWithAnEndedSession_WritesNothing_AndGoesToTheSignInPage()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open(out AccessWriteLog log);
        using HttpClient client = ClientOf(factory);
        await using (AppDbContext db = NewContext())
        {
            await db.Sessions.Where(s => s.CustomerId == customerId).ExecuteDeleteAsync(Ct);
        }

        using HttpResponseMessage response = await PostAsync(client, "/hesap/adresler?handler=Save", CustomerCookie(customerKey));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.StartsWith("/giris?returnUrl=", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.Empty(log.Snapshot());
    }

    [Fact]
    public async Task APostWithASession_Writes()
    {
        (string customerKey, _) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open(out AccessWriteLog log);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await PostAsync(client, "/hesap/adresler?handler=Save", CustomerCookie(customerKey));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(["account-addresses"], log.Snapshot());
    }

    [Fact]
    public async Task TheSignOutWithoutASession_WritesNothing_AndGetsTheSignOutsRedirectNotTheSignInPages()
    {
        await using WebApplicationFactory<Program> factory = Open(out AccessWriteLog log);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage panel = await PostAsync(client, "/panel/cikis?handler=Save");
        Assert.Equal(HttpStatusCode.SeeOther, panel.StatusCode);
        Assert.Equal("/panel/giris", panel.Headers.Location?.OriginalString);

        // The storefront's sign-out goes back to the page it came from when that page needs no session, else to the home page (07 4.34.1).
        foreach ((string returnUrl, string expected) in new[]
        {
            ("/urun/ahsap-masa", "/urun/ahsap-masa"),
            ("/hesap/silindi", "/hesap/silindi"),
            ("/hesap/adresler", "/"),
            ("/hesap", "/"),
            ("/siparis/2026-ABC123", "/"),
            ("/siparis/erisim/abc-DEF_123", "/siparis/erisim/abc-DEF_123"),
            ("/panel/siparisler", "/"),
            ("//evil.example", "/"),
            ("/\\evil.example", "/"),
            (Evil, "/"),
            ("/\t/evil.example", "/"),
        })
        {
            using HttpResponseMessage response = await PostAsync(client, "/hesap/test-cikis?handler=Save", null, ("returnUrl", returnUrl));
            Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
            Assert.Equal(expected ?? returnUrl, response.Headers.Location?.OriginalString);
        }

        using HttpResponseMessage none = await PostAsync(client, "/hesap/test-cikis?handler=Save");
        Assert.Equal("/", none.Headers.Location?.OriginalString);
        Assert.Empty(log.Snapshot());
    }

    [Fact]
    public async Task TheSignOutWithASession_RunsTheHandler()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open(out AccessWriteLog log);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await PostAsync(client, "/panel/cikis?handler=Save", AdminCookie(adminKey));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(["panel-signout"], log.Snapshot());
    }

    // --- criterion 6, KK6: the return address is the site's own path ---

    [Theory]
    [InlineData("//evil.example")]
    [InlineData("/\\evil.example")]
    [InlineData("https://evil.example")]
    [InlineData("%2F%2Fevil.example")]
    [InlineData("javascript:alert(1)")]
    [InlineData("evil")]
    [InlineData("/\t/evil.example")]
    [InlineData("")]
    public async Task TheSignInPage_IgnoresAReturnAddressThatIsNotASitePath_AndGoesToTheAreasHomePage(string returnUrl)
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage storefront = await GetAsync(client, $"/giris?returnUrl={Uri.EscapeDataString(returnUrl)}");
        using HttpResponseMessage panel = await GetAsync(client, $"/panel/giris?returnUrl={Uri.EscapeDataString(returnUrl)}");

        // The value is sent encoded once, so the page receives it decoded once: "%2F%2Fevil.example" arrives as the literal text and is no path.
        Assert.Equal("/", await BodyOf(storefront));
        Assert.Equal("/panel", await BodyOf(panel));
    }

    [Fact]
    public async Task TheSignInPage_KeepsASitePath_AndANetworkPathSentInTheQueryEncodedOnceIsIgnored()
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage kept = await GetAsync(client, "/giris?returnUrl=%2Fhesap%2Fadresler%3Fpage%3D2");
        using HttpResponseMessage panelKept = await GetAsync(client, "/panel/giris?returnUrl=%2Fpanel%2Fsiparisler%3Fpage%3D2");
        using HttpResponseMessage encodedNetworkPath = await GetAsync(client, "/giris?returnUrl=%2F%2Fdis.example");
        using HttpResponseMessage controlCharacter = await GetAsync(client, "/giris?returnUrl=%2F%09%2Fdis.example");

        Assert.Equal("/hesap/adresler?page=2", await BodyOf(kept));
        Assert.Equal("/panel/siparisler?page=2", await BodyOf(panelKept));
        Assert.Equal("/", await BodyOf(encodedNetworkPath));
        Assert.Equal("/", await BodyOf(controlCharacter));
    }

    // --- criterion 7, KK7: the one 404 ---

    [Fact]
    public async Task AnotherCustomersOrderAndAnOrderThatDoesNotExist_GetTheSame404_AndAPostWritesNothing()
    {
        (string aliKey, Guid aliId) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, Guid veliId) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, string aliNumber, _) = await TestSessions.OrderAsync(_database.Dsn, aliId);
        (_, string veliNumber, _) = await TestSessions.OrderAsync(_database.Dsn, veliId);
        (_, string guestNumber, _) = await TestSessions.OrderAsync(_database.Dsn, customerId: null);
        await using WebApplicationFactory<Program> factory = Open(out AccessWriteLog log);
        using HttpClient client = ClientOf(factory);
        string cookie = CustomerCookie(aliKey);

        using HttpResponseMessage own = await GetAsync(client, $"/siparis/{aliNumber}", cookie);
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Contains($"Sipariş {aliNumber}", await BodyOf(own), StringComparison.Ordinal);

        using HttpResponseMessage others = await GetAsync(client, $"/siparis/{veliNumber}", cookie);
        using HttpResponseMessage missing = await GetAsync(client, "/siparis/2026-ZZZZZZZZ", cookie);
        using HttpResponseMessage guest = await GetAsync(client, $"/siparis/{guestNumber}", cookie);
        Assert.Equal(HttpStatusCode.NotFound, others.StatusCode);
        foreach (HttpResponseMessage denied in new[] { missing, guest })
        {
            Assert.Equal(others.StatusCode, denied.StatusCode);
            Assert.Equal(await PageOf(others), await PageOf(denied));
            Assert.Equal(others.Content.Headers.ContentType, denied.Content.Headers.ContentType);
            Assert.Equal(HeaderNames(others), HeaderNames(denied));
        }

        // The page's one write (UC-36) finds the order by the root first: another account's order and a missing number are the same 404, and nothing is written.
        using HttpResponseMessage postOthers = await DownloadAsync(client, $"/siparis/{veliNumber}", cookie);
        using HttpResponseMessage postMissing = await DownloadAsync(client, "/siparis/2026-ZZZZZZZZ", cookie);
        Assert.Equal(HttpStatusCode.NotFound, postOthers.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, postMissing.StatusCode);
        Assert.Equal(await PageOf(others), await PageOf(postOthers));
        Assert.Empty(log.Snapshot());
    }

    /// <summary>The order page's download form for an item that is not on the order: the root decides first, then the item (07 4.16.3).</summary>
    private static Task<HttpResponseMessage> DownloadAsync(HttpClient client, string root, string? cookie) =>
        PostAsync(client, $"{root}?handler=DownloadDigitalFile", cookie, ("OrderItemId", Guid.NewGuid().ToString()));

    [Fact]
    public async Task AWrongOrRenewedAccessKeyAndAMissingPanelRecord_GetTheSame404_AsAnotherCustomersOrder()
    {
        (string aliKey, Guid aliId) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, Guid veliId) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, string veliNumber, _) = await TestSessions.OrderAsync(_database.Dsn, veliId);
        (Guid guestOrder, _, string oldKey) = await TestSessions.OrderAsync(_database.Dsn, customerId: null);
        string newKey = SecretKey.Create();
        await using (AppDbContext db = NewContext())
        {
            await db.Orders.Where(o => o.Id == guestOrder).ExecuteUpdateAsync(set => set.SetProperty(o => o.OrderAccessKey, newKey), Ct);
        }

        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open(out AccessWriteLog log);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage others = await GetAsync(client, $"/siparis/{veliNumber}", CustomerCookie(aliKey));
        using HttpResponseMessage renewed = await GetAsync(client, $"/siparis/erisim/{oldKey}");
        using HttpResponseMessage unknown = await GetAsync(client, $"/siparis/erisim/{SecretKey.Create()}");
        using HttpResponseMessage malformed = await GetAsync(client, "/siparis/erisim/not+a+key");
        using HttpResponseMessage panelRecord = await GetAsync(client, $"/panel/kayit/{Guid.NewGuid()}", AdminCookie(adminKey));
        using HttpResponseMessage panelPost = await PostAsync(client, $"/panel/kayit/{Guid.NewGuid()}?handler=Save", AdminCookie(adminKey));
        using HttpResponseMessage current = await GetAsync(client, $"/siparis/erisim/{newKey}");

        foreach (HttpResponseMessage denied in new[] { renewed, unknown, malformed, panelRecord, panelPost })
        {
            Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
            Assert.Equal(await PageOf(others), await PageOf(denied));
            Assert.Equal(HeaderNames(others), HeaderNames(denied));
        }

        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, others.StatusCode);
        Assert.Empty(log.Snapshot());
        _ = aliId;
    }

    private static string[] HeaderNames(HttpResponseMessage response) =>
        [.. response.Headers.Select(h => h.Key).Concat(response.Content.Headers.Select(h => h.Key)).Where(n => n is not ("Date" or "Pragma" or "Expires" or "Cache-Control")).Order(StringComparer.Ordinal)];

    // --- criterion 8, KK8: a guest order opens only with its access key ---

    [Fact]
    public async Task AGuestOrder_OpensWithItsAccessKeyOnly()
    {
        (string customerKey, _) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, string number, string accessKey) = await TestSessions.OrderAsync(_database.Dsn, customerId: null);
        await using WebApplicationFactory<Program> factory = Open(out AccessWriteLog log);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage byKey = await GetAsync(client, $"/siparis/erisim/{accessKey}");
        using HttpResponseMessage byNumberAnonymous = await GetAsync(client, $"/siparis/{number}");
        using HttpResponseMessage byNumberAsCustomer = await GetAsync(client, $"/siparis/{number}", CustomerCookie(customerKey));
        using HttpResponseMessage postByNumber = await DownloadAsync(client, $"/siparis/{number}", CustomerCookie(customerKey));

        Assert.Equal(HttpStatusCode.OK, byKey.StatusCode);
        Assert.Contains($"Sipariş {number}", await BodyOf(byKey), StringComparison.Ordinal);

        // The member's root without any session asks for the sign-in (07 2.3.2; UC-33); with a customer's session a guest order is the one 404.
        Assert.Equal(HttpStatusCode.Found, byNumberAnonymous.StatusCode);
        Assert.Equal($"/giris?returnUrl={Escaped($"/siparis/{number}")}", byNumberAnonymous.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.NotFound, byNumberAsCustomer.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, postByNumber.StatusCode);
        Assert.Empty(log.Snapshot());
    }

    [Fact]
    public async Task AMembersOrder_AlsoOpensWithItsAccessKey()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, string number, string accessKey) = await TestSessions.OrderAsync(_database.Dsn, customerId);
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage anonymous = await GetAsync(client, $"/siparis/erisim/{accessKey}");
        using HttpResponseMessage signedIn = await GetAsync(client, $"/siparis/erisim/{accessKey}", CustomerCookie(customerKey));

        foreach (HttpResponseMessage response in new[] { anonymous, signedIn })
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains($"Sipariş {number}", await BodyOf(response), StringComparison.Ordinal);
        }
    }

    // --- criterion 9, KK9: an administrator session is no customer ---

    [Fact]
    public async Task AnAdministratorSession_OpensNeitherTheAccountNorAMembersOrder_AndOnlyTheStripReadsIt()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, string number, _) = await TestSessions.OrderAsync(_database.Dsn, customerId);
        await using WebApplicationFactory<Program> factory = Open(out AccessWriteLog log);
        using HttpClient client = ClientOf(factory);
        string admin = AdminCookie(adminKey);

        using HttpResponseMessage account = await GetAsync(client, "/hesap", admin);
        using HttpResponseMessage accountPost = await PostAsync(client, "/hesap/adresler?handler=Save", admin);
        using HttpResponseMessage order = await GetAsync(client, $"/siparis/{number}", admin);
        using HttpResponseMessage strip = await GetAsync(client, "/sayfa/yonetici-seridi", admin);
        using HttpResponseMessage both = await GetAsync(client, "/sayfa/yonetici-seridi", $"{admin}; {CustomerCookie(customerKey)}");
        using HttpResponseMessage nobody = await GetAsync(client, "/sayfa/yonetici-seridi");

        Assert.Equal(HttpStatusCode.Found, account.StatusCode);
        Assert.Equal($"/giris?returnUrl={Escaped("/hesap")}", account.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.SeeOther, accountPost.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, order.StatusCode);
        Assert.Equal("customer:False|admin:True", await BodyOf(strip));
        Assert.Equal("customer:True|admin:True", await BodyOf(both));
        Assert.Equal("customer:False|admin:False", await BodyOf(nobody));
        Assert.Empty(log.Snapshot());
    }

    // --- 09 15.1 item 2: the lifetimes, the last activity and the long session's cookie ---

    [Theory]
    [InlineData(false, 119, true)]
    [InlineData(false, 121, false)]
    [InlineData(true, 121, true)]
    public async Task AShortSession_LivesTwoHoursFromItsLastActivity_AndALongOneIsNotCutAtTwoHours(bool longLived, int minutesIdle, bool open)
    {
        (string key, _) = await TestSessions.CustomerAsync(_database.Dsn, longLived, DateTimeOffset.UtcNow.AddMinutes(-minutesIdle));
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await GetAsync(client, "/hesap", CustomerCookie(key));

        Assert.Equal(open ? HttpStatusCode.OK : HttpStatusCode.Found, response.StatusCode);
    }

    [Theory]
    [InlineData(29, true)]
    [InlineData(31, false)]
    public async Task ALongSession_LivesThirtyDaysFromItsLastActivity(int daysIdle, bool open)
    {
        (string key, _) = await TestSessions.CustomerAsync(_database.Dsn, longLived: true, DateTimeOffset.UtcNow.AddDays(-daysIdle));
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await GetAsync(client, "/hesap", CustomerCookie(key));

        Assert.Equal(open ? HttpStatusCode.OK : HttpStatusCode.Found, response.StatusCode);
    }

    [Fact]
    public async Task TheLastActivity_IsWrittenAtMostOnceAMinute()
    {
        (string key, _, Guid sessionId) = await TestSessions.CustomerWithSessionAsync(_database.Dsn, lastActivity: DateTimeOffset.UtcNow.AddMinutes(-30));
        (string freshKey, _, Guid freshId) = await TestSessions.CustomerWithSessionAsync(_database.Dsn, lastActivity: DateTimeOffset.UtcNow.AddSeconds(-20));
        DateTimeOffset freshBefore = await TestSessions.LastActivityAsync(_database.Dsn, freshId);
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);
        DateTimeOffset before = DateTimeOffset.UtcNow;

        using HttpResponseMessage first = await GetAsync(client, "/hesap", CustomerCookie(key));
        DateTimeOffset afterFirst = await TestSessions.LastActivityAsync(_database.Dsn, sessionId);
        using HttpResponseMessage second = await GetAsync(client, "/hesap", CustomerCookie(key));
        DateTimeOffset afterSecond = await TestSessions.LastActivityAsync(_database.Dsn, sessionId);
        using HttpResponseMessage fresh = await GetAsync(client, "/hesap", CustomerCookie(freshKey));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.True(afterFirst >= before.AddSeconds(-1), "The first request wrote the last activity.");
        Assert.Equal(afterFirst, afterSecond);
        Assert.Equal(HttpStatusCode.OK, fresh.StatusCode);
        Assert.Equal(freshBefore, await TestSessions.LastActivityAsync(_database.Dsn, freshId));
    }

    [Fact]
    public async Task ALongSessionsCookie_IsWrittenAgainWithTheSameLifetimeWhenTheActivityIsWritten_AndAShortOneIsNot()
    {
        (string longKey, _) = await TestSessions.AdminAsync(_database.Dsn, longLived: true, DateTimeOffset.UtcNow.AddMinutes(-30));
        (string shortKey, _) = await TestSessions.AdminAsync(_database.Dsn, longLived: false, DateTimeOffset.UtcNow.AddMinutes(-30));
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage longResponse = await GetAsync(client, "/panel/siparisler", AdminCookie(longKey));
        using HttpResponseMessage shortResponse = await GetAsync(client, "/panel/siparisler", AdminCookie(shortKey));

        Assert.Equal(HttpStatusCode.OK, longResponse.StatusCode);
        SetCookieHeaderValue cookie = Assert.Single(SetCookieHeaderValue.ParseList([.. longResponse.Headers.GetValues("Set-Cookie")]));
        Assert.Equal(CookieNames.AdminSession, cookie.Name.Value);
        Assert.Equal(longKey, cookie.Value.Value);
        Assert.Equal(TimeSpan.FromDays(30), cookie.MaxAge);
        Assert.True(cookie.Secure);
        Assert.True(cookie.HttpOnly);
        Assert.Equal(HttpStatusCode.OK, shortResponse.StatusCode);
        Assert.False(shortResponse.Headers.Contains("Set-Cookie"));
    }

    // --- 09 15.1 item 2 architecture test: every page of the two folders carries the authorization metadata, but the open ones ---

    [Fact]
    public async Task EveryEndpointUnderThePanelAndTheAccount_IsClosed_ExceptTheOpenPagesOfTheClosedList()
    {
        await using WebApplicationFactory<Program> factory = Open(out _, withUploadEndpoint: true);
        using HttpClient client = ClientOf(factory);
        using HttpResponseMessage warm = await GetAsync(client, "/panel/giris");
        List<RouteEndpoint> endpoints = [.. factory.Services.GetServices<EndpointDataSource>().SelectMany(d => d.Endpoints).OfType<RouteEndpoint>()];

        List<string> open = [];
        List<string> unguarded = [];
        foreach (RouteEndpoint endpoint in endpoints)
        {
            string address = "/" + (endpoint.RoutePattern.RawText ?? string.Empty).TrimStart('/');
            string first = address.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
            if (!first.Equals(SessionSchemes.PanelSegment, StringComparison.OrdinalIgnoreCase) && !first.Equals(SessionSchemes.AccountSegment, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            {
                open.AddRange(AccessConvention.Variants(address));
            }
            else if (!endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Any())
            {
                unguarded.Add(address);
            }
        }

        Assert.Empty(unguarded);
        Assert.Contains(endpoints, e => e.Metadata.GetMetadata<IAuthorizeData>() is not null);
        Assert.Equal(
            ["/hesap/silindi", "/panel/davet/{}", "/panel/giris", "/panel/sifre-sifirlama", "/panel/sifre-sifirlama/{}"], // /panel/davet/{key} is the test project's bare-layout sample (T540); the product's invitation page takes its place
            open.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray());
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }
}

/// <summary>A test-only machine endpoint under <c>/panel</c> that asks for the panel's session, the way the chunked upload does (07 4.31.2; K-947).</summary>
internal sealed class UploadEndpointStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        return app =>
        {
            next(app);
            app.UseEndpoints(endpoints => endpoints.MapPost("/panel/yukleme-deneme", () => Results.Ok()).RequireAdminSession());
        };
    }
}
