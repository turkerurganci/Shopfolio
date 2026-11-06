using System.Net;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Npgsql;

using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The cookie rules on the opened application and a real database (T224 KK1…KK8; 05 6.7; 09 15.1 item 6): every cookie of the inventory is written
/// with Secure, HttpOnly, SameSite=Lax, the path <c>/</c>, no Domain and the <c>__Host-</c> prefix; the guest cart cookie lives 400 days and is
/// renewed at every visit; the checkout step's cookie is encrypted, lasts the browser session and is deleted on confirmation; and until the cookie
/// policy is published the storefront's responses carry no <c>Set-Cookie</c> while the panel's are left alone. Every test works on its own copy
/// of the template database (a test publishes the policy). The test endpoints write cookies through the application's own writer.
/// </summary>
public sealed class CookiePolicyTests : IAsyncLifetime
{
    private const string PolicyId = "01000000-0000-7000-8000-000000224001";

    private readonly TemplateDatabaseFixture _template;
    private string _databaseName = string.Empty;
    private string _dsn = string.Empty;

    public CookiePolicyTests(TemplateDatabaseFixture template)
    {
        _template = template;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        _databaseName = await _template.CreateDatabaseFromTemplateAsync(Ct);
        _dsn = _template.DsnFor(_databaseName);
    }

    public async ValueTask DisposeAsync()
    {
        using NpgsqlConnection pooled = new(_dsn);
        NpgsqlConnection.ClearPool(pooled);
        await _template.DropDatabaseAsync(_databaseName, CancellationToken.None);
    }

    private static WebApplicationFactory<Program> WithProbe(ShopfolioWebApplicationFactory factory) =>
        factory.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IStartupFilter, CookieProbeStartupFilter>()));

    private static HttpClient ClientOf(WebApplicationFactory<Program> factory) =>
        ForgeryTokenHandler.CreateClient(factory);

    private static async Task<HttpResponseMessage> GetAsync(HttpClient client, string path, string? cookieHeader = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        if (cookieHeader is not null)
        {
            request.Headers.Add(HeaderNames.Cookie, cookieHeader);
        }

        return await client.SendAsync(request, Ct);
    }

    private async Task PublishCookiePolicyAsync()
    {
        await using NpgsqlConnection connection = new(_dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(
            "INSERT INTO legal_text_versions (id, created_at, legal_text_type, version_number, published_at, text) "
            + $"VALUES ('{PolicyId}', now(), 'CookiePolicy', 1, now(), 'policy')",
            connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private static string RequestCookieOf(IEnumerable<SetCookieHeaderValue> headers) =>
        string.Join("; ", headers.Select(h => $"{h.Name}={h.Value}"));

    [Fact]
    public async Task AllCookies_AfterThePublication_CarryTheThreeAttributesThePrefixConditionsAndNothingOutsideTheInventory()
    {
        await PublishCookiePolicyAsync();
        await using ShopfolioWebApplicationFactory baseFactory = new(_dsn);
        using WebApplicationFactory<Program> factory = WithProbe(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await GetAsync(client, CookieProbeStartupFilter.AllPath);

        List<SetCookieHeaderValue> headers = CookieHeaders.Read(response);
        Assert.Empty(CookieHeaders.Violations(headers));
        string[] names = [.. headers.Select(h => CookieHeaders.InventoryName(h.Name.Value!)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        Assert.Equal(CookieNames.All.Order(StringComparer.Ordinal), names);
        Assert.Contains(headers, h => h.Name.Value!.StartsWith(CookieNames.Checkout + "C", StringComparison.Ordinal));
    }

    [Fact]
    public void Violations_OnACookieOutsideTheInventoryOrWithoutAnAttribute_AreFound()
    {
        List<SetCookieHeaderValue> headers =
        [
            SetCookieHeaderValue.Parse("_ga=GA1.2; path=/; secure; httponly; samesite=lax"),
            SetCookieHeaderValue.Parse("__Host-cart=x; path=/; secure; httponly; samesite=none"),
            SetCookieHeaderValue.Parse("__Host-session=x; path=/; domain=example.com; httponly; samesite=lax"),
        ];

        List<string> problems = CookieHeaders.Violations(headers);

        Assert.Contains("_ga: not in the cookie inventory", problems);
        Assert.Contains("_ga: no __Host- prefix", problems);
        Assert.Contains(problems, p => p.StartsWith("__Host-cart: SameSite", StringComparison.Ordinal));
        Assert.Contains("__Host-session: not Secure", problems);
        Assert.Contains("__Host-session: has a Domain", problems);
    }

    [Fact]
    public async Task GuestCartCookie_Lives400DaysAndIsWrittenAgainAtEveryVisit()
    {
        await PublishCookiePolicyAsync();
        await using ShopfolioWebApplicationFactory baseFactory = new(_dsn);
        using WebApplicationFactory<Program> factory = WithProbe(baseFactory);
        using HttpClient client = ClientOf(factory);

        for (int visit = 0; visit < 2; visit++)
        {
            using HttpResponseMessage response = await GetAsync(client, CookieProbeStartupFilter.CartPath, "__Host-cart=raw-cart-key");

            SetCookieHeaderValue cart = Assert.Single(CookieHeaders.Read(response));
            Assert.Equal(CookieNames.Cart, cart.Name.Value);
            Assert.Equal(TimeSpan.FromDays(400), cart.MaxAge);
            Assert.Equal(TimeSpan.FromDays(DisposalTimings.GuestCartIdleDays), BrowserCookies.GuestCartLifetime);
            Assert.Equal("raw-cart-key", cart.Value.Value);
        }
    }

    [Fact]
    public async Task CheckoutCookie_IsEncryptedLastsTheSessionIsReadBackAndIsDeletedOnConfirmation()
    {
        await PublishCookiePolicyAsync();
        await using ShopfolioWebApplicationFactory baseFactory = new(_dsn);
        using WebApplicationFactory<Program> factory = WithProbe(baseFactory);
        using HttpClient client = ClientOf(factory);
        const string payload = "buyer@example.com;Atatürk Cd. 1";

        using HttpResponseMessage written = await GetAsync(client, CookieProbeStartupFilter.CheckoutWritePath + "?payload=" + Uri.EscapeDataString(payload));
        SetCookieHeaderValue checkout = Assert.Single(CookieHeaders.Read(written));
        string cookie = RequestCookieOf([checkout]);

        Assert.Equal(CookieNames.Checkout, checkout.Name.Value);
        Assert.Null(checkout.MaxAge);
        Assert.Null(checkout.Expires);
        Assert.DoesNotContain("buyer", checkout.Value.Value, StringComparison.Ordinal);
        Assert.DoesNotContain("example", checkout.Value.Value, StringComparison.Ordinal);

        using HttpResponseMessage read = await GetAsync(client, CookieProbeStartupFilter.CheckoutReadPath, cookie);
        Assert.Equal("valid:" + payload, await read.Content.ReadAsStringAsync(Ct));

        using HttpResponseMessage tampered = await GetAsync(client, CookieProbeStartupFilter.CheckoutReadPath, cookie + "x");
        Assert.Equal("invalid", await tampered.Content.ReadAsStringAsync(Ct));

        using HttpResponseMessage confirmed = await GetAsync(client, CookieProbeStartupFilter.CheckoutConfirmPath, cookie);
        SetCookieHeaderValue deletion = Assert.Single(CookieHeaders.Read(confirmed));
        Assert.Equal(CookieNames.Checkout, deletion.Name.Value);
        Assert.Equal(string.Empty, deletion.Value.Value);
        Assert.True(deletion.Expires < DateTimeOffset.UtcNow);
        Assert.Empty(CookieHeaders.Violations([deletion]));
    }

    [Fact]
    public async Task CheckoutCookie_ThatIsSplitIntoChunks_IsReadBackAndEveryChunkIsDeleted()
    {
        await PublishCookiePolicyAsync();
        await using ShopfolioWebApplicationFactory baseFactory = new(_dsn);
        using WebApplicationFactory<Program> factory = WithProbe(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage written = await GetAsync(client, CookieProbeStartupFilter.CheckoutWritePath + "?payload=" + Uri.EscapeDataString(CookieProbeStartupFilter.LongPayload));
        List<SetCookieHeaderValue> chunks = CookieHeaders.Read(written);
        string cookie = RequestCookieOf(chunks);

        Assert.True(chunks.Count > 2, "The long payload should have been split into chunks.");
        Assert.Empty(CookieHeaders.Violations(chunks));

        using HttpResponseMessage read = await GetAsync(client, CookieProbeStartupFilter.CheckoutReadPath, cookie);
        Assert.Equal("valid:" + CookieProbeStartupFilter.LongPayload, await read.Content.ReadAsStringAsync(Ct));

        using HttpResponseMessage confirmed = await GetAsync(client, CookieProbeStartupFilter.CheckoutConfirmPath, cookie);
        List<SetCookieHeaderValue> deletions = CookieHeaders.Read(confirmed);
        Assert.Equal(chunks.Select(c => c.Name.Value).Order(StringComparer.Ordinal), deletions.Select(d => d.Name.Value).Order(StringComparer.Ordinal));
        Assert.All(deletions, d => Assert.True(d.Expires < DateTimeOffset.UtcNow));
        Assert.Empty(CookieHeaders.Violations(deletions));
    }

    [Fact]
    public async Task ExternalLoginFlowCookie_HasTheRootPathLastsTenMinutesAndIsUsedOnce()
    {
        await PublishCookiePolicyAsync();
        await using ShopfolioWebApplicationFactory baseFactory = new(_dsn);
        using WebApplicationFactory<Program> factory = WithProbe(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage written = await GetAsync(client, CookieProbeStartupFilter.FlowWritePath);
        SetCookieHeaderValue flow = Assert.Single(CookieHeaders.Read(written));

        Assert.Equal(CookieNames.ExternalLogin, flow.Name.Value);
        Assert.Equal("/", flow.Path.Value);
        Assert.Equal(TimeSpan.FromMinutes(10), flow.MaxAge);
        Assert.Equal(SameSiteMode.Lax, flow.SameSite);
        Assert.DoesNotContain("state", flow.Value.Value, StringComparison.Ordinal);

        using HttpResponseMessage consumed = await GetAsync(client, CookieProbeStartupFilter.FlowConsumePath, RequestCookieOf([flow]));
        Assert.Equal("valid:state=1;nonce=2", await consumed.Content.ReadAsStringAsync(Ct));
        SetCookieHeaderValue deletion = Assert.Single(CookieHeaders.Read(consumed));
        Assert.Equal(CookieNames.ExternalLogin, deletion.Name.Value);
        Assert.True(deletion.Expires < DateTimeOffset.UtcNow);
        Assert.Empty(CookieHeaders.Violations([deletion]));

        using HttpResponseMessage missing = await GetAsync(client, CookieProbeStartupFilter.FlowConsumePath);
        Assert.Equal("invalid", await missing.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task AntiforgeryCookie_OfTheFramework_TakesItsNameAndAttributesFromTheCookieRules()
    {
        await PublishCookiePolicyAsync();
        await using ShopfolioWebApplicationFactory baseFactory = new(_dsn);
        using WebApplicationFactory<Program> factory = WithProbe(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await GetAsync(client, CookieProbeStartupFilter.AllPath);

        SetCookieHeaderValue antiforgery = Assert.Single(CookieHeaders.Read(response), h => h.Name.Value == CookieNames.Antiforgery);
        Assert.Empty(CookieHeaders.Violations([antiforgery]));
        Assert.Null(antiforgery.MaxAge);
        Assert.Null(antiforgery.Expires);
    }

    [Fact]
    public async Task StorefrontResponse_BeforeThePublication_CarriesNoSetCookie_AndThePanelResponseIsLeftAlone()
    {
        await using ShopfolioWebApplicationFactory baseFactory = new(_dsn);
        using WebApplicationFactory<Program> factory = WithProbe(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage storefront = await GetAsync(client, CookieProbeStartupFilter.AllPath);
        using HttpResponseMessage cart = await GetAsync(client, CookieProbeStartupFilter.CartPath);
        using HttpResponseMessage panel = await GetAsync(client, CookieProbeStartupFilter.PanelPath);

        Assert.False(storefront.Headers.Contains(HeaderNames.SetCookie));
        Assert.False(cart.Headers.Contains(HeaderNames.SetCookie));
        List<SetCookieHeaderValue> panelHeaders = CookieHeaders.Read(panel);
        Assert.Empty(CookieHeaders.Violations(panelHeaders));
        string[] names = [.. panelHeaders.Select(h => CookieHeaders.InventoryName(h.Name.Value!)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        Assert.Equal(CookieNames.All.Order(StringComparer.Ordinal), names);
    }

    [Fact]
    public async Task StorefrontResponse_WhileOnlyTheDraftExists_CarriesNoSetCookie()
    {
        await using NpgsqlConnection connection = new(_dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand draft = new("SELECT count(*) FROM legal_text_versions WHERE legal_text_type = 'CookiePolicy' AND published_at IS NULL", connection);
        Assert.Equal(1L, await draft.ExecuteScalarAsync(Ct));
        await using ShopfolioWebApplicationFactory baseFactory = new(_dsn);
        using WebApplicationFactory<Program> factory = WithProbe(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await GetAsync(client, CookieProbeStartupFilter.AllPath);

        Assert.False(response.Headers.Contains(HeaderNames.SetCookie));
    }

    [Fact]
    public async Task StorefrontResponse_OpensAtTheFirstRequestAfterThePublication_AndStaysOpen()
    {
        await using ShopfolioWebApplicationFactory baseFactory = new(_dsn);
        using WebApplicationFactory<Program> factory = WithProbe(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage before = await GetAsync(client, CookieProbeStartupFilter.CartPath);
        await PublishCookiePolicyAsync();
        using HttpResponseMessage first = await GetAsync(client, CookieProbeStartupFilter.CartPath);
        using HttpResponseMessage second = await GetAsync(client, CookieProbeStartupFilter.CartPath);

        Assert.False(before.Headers.Contains(HeaderNames.SetCookie));
        Assert.Single(CookieHeaders.Read(first));
        Assert.Single(CookieHeaders.Read(second));
    }

    [Fact]
    public async Task MachineEndpoint_BeforeThePublication_StillAnswers_WithoutTheDatabase()
    {
        DsnBuilder unreachable = new(_dsn) { Port = 1 };
        await using ShopfolioWebApplicationFactory factory = new(unreachable.ConnectionString);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(new Uri("/sistem/canlilik", UriKind.Relative), Ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
