using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.IntegrationTests.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The security headers and the no-store rule on the opened application and test-only sample pages (T209 criteria 7, 8, 9; KK7, KK8, KK9; 07 2.5.6;
/// 05 6.5.4, 6.8.1; 02 8.3.2, 8.3.3; 09 15.2 items 3-5): the machine endpoint, the error answer, the static file and a page all carry the four headers;
/// the nonce is new in every response; the policy as sent blocks an outside script, an inline script, an inline style without the nonce and a frame, and lets
/// the nonce block and the form to Google's authorization address through; the personal pages carry <c>no-store</c> and the public ones and the static file do
/// not; the application writes neither HSTS nor a frame-options header.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed partial class SecurityHeadersTests
{
    private static readonly string[] _securityHeaders = ["Content-Security-Policy", "X-Content-Type-Options", "Referrer-Policy", "Permissions-Policy"];

    private readonly CollectionDatabaseFixture _database;

    public SecurityHeadersTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex("'nonce-([^']+)'", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Nonce();

    private WebApplicationFactory<Program> Open()
    {
        ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        return factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddSingleton(new PatternRecordStore());
            s.AddSingleton(new AccessWriteLog());
            s.AddRazorPages().AddApplicationPart(typeof(PatternFormModel).Assembly);
        }));
    }

    private static async Task<HttpResponseMessage> GetAsync(HttpClient client, string path, string? cookie = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation(HeaderNames.Cookie, cookie);
        }

        return await client.SendAsync(request, Ct);
    }

    private static string Header(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out IEnumerable<string>? values) ? string.Join(", ", values) : string.Empty;

    // --- criterion 7; KK7: the headers on every answer ---

    [Fact]
    public async Task EveryAnswer_MachineEndpointPageErrorAndStaticFile_CarriesTheFourHeaders_AndNoHstsAndNoFrameOptions_KK7_KK9()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        string[] paths = ["/sistem/canlilik", "/urun/ornek", "/yok/boyle-bir-adres", "/statik/css/base.css", "/robots.txt", "/panel/ornek-form"];
        foreach (string path in paths)
        {
            using HttpResponseMessage response = await GetAsync(client, path, $"{CookieNames.AdminSession}={adminKey}");

            foreach (string name in _securityHeaders)
            {
                Assert.True(response.Headers.Contains(name), $"{path} has no {name} (status {(int)response.StatusCode})");
            }

            Assert.Equal("nosniff", Header(response, "X-Content-Type-Options"));
            Assert.Equal("camera=(), microphone=(), geolocation=()", Header(response, "Permissions-Policy"));
            Assert.Equal("strict-origin-when-cross-origin", Header(response, "Referrer-Policy"));
            Assert.False(response.Headers.Contains("Strict-Transport-Security"), $"{path} writes HSTS (05 6.8.1: the reverse proxy does)");
            Assert.False(response.Headers.Contains("X-Frame-Options"), $"{path} writes X-Frame-Options (the policy's frame-ancestors is the one)");
        }
    }

    [Fact]
    public async Task TheSameHolds_WhenTheRequestComesThroughTheReverseProxyAsHttps_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpRequestMessage request = new(HttpMethod.Get, "/sistem/canlilik");
        request.Headers.Add("X-Forwarded-Proto", "https");
        request.Headers.Add("X-Forwarded-For", "203.0.113.7");

        using HttpResponseMessage response = await client.SendAsync(request, Ct);

        Assert.False(response.Headers.Contains("Strict-Transport-Security"));
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
    }

    [Fact]
    public async Task TheNonce_IsNewInEveryResponse_AndIsInTheStylePolicyOnly_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage first = await GetAsync(client, "/sistem/canlilik");
        using HttpResponseMessage second = await GetAsync(client, "/sistem/canlilik");

        string firstPolicy = Header(first, "Content-Security-Policy");
        string secondPolicy = Header(second, "Content-Security-Policy");
        string firstNonce = Nonce().Match(firstPolicy).Groups[1].Value;
        string secondNonce = Nonce().Match(secondPolicy).Groups[1].Value;
        Assert.Equal(24, firstNonce.Length);
        Assert.NotEqual(firstNonce, secondNonce);
        Assert.Single(Nonce().Matches(firstPolicy));
        Assert.Equal(ContentSecurityPolicy.Build(firstNonce), firstPolicy);
    }

    [Fact]
    public async Task ThePolicyAsSent_BlocksAnOutsideScriptAnInlineScriptAnInlineStyleWithoutTheNonceAndAFrame_AndLetsTheNonceBlockAndGoogleThrough_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage response = await GetAsync(client, "/sistem/canlilik");
        string header = Header(response, "Content-Security-Policy");
        string nonce = Nonce().Match(header).Groups[1].Value;
        CspEvaluator policy = new(header, "https://localhost");

        Assert.False(policy.AllowsLoad("script-src", "https://evil.example/a.js"));
        Assert.False(policy.AllowsLoad("script-src", "https://cdn.example/lib.js"));
        Assert.True(policy.AllowsLoad("script-src", "https://localhost/statik/js/common.js"));
        Assert.False(policy.AllowsInline("script-src", nonce: null));
        Assert.False(policy.AllowsInline("script-src", nonce));
        Assert.False(policy.AllowsInline("style-src", nonce: null));
        Assert.False(policy.AllowsInline("style-src", "another-nonce"));
        Assert.True(policy.AllowsInline("style-src", nonce));
        Assert.False(policy.AllowsLoad("frame-src", "https://localhost/odeme"));
        Assert.False(policy.AllowsLoad("frame-src", "https://evil.example/f"));
        Assert.False(policy.AllowsLoad("frame-ancestors", "https://localhost"));
        Assert.False(policy.AllowsLoad("object-src", "https://localhost/o.swf"));
        Assert.False(policy.AllowsLoad("img-src", "https://evil.example/p.gif"));
        Assert.False(policy.AllowsLoad("connect-src", "https://evil.example/collect"));
        Assert.True(policy.AllowsLoad("form-action", "https://localhost/panel/ornek-form"));
        Assert.True(policy.AllowsLoad("form-action", "https://accounts.google.com/o/oauth2/v2/auth"));
        Assert.False(policy.AllowsLoad("form-action", "https://evil.example/steal"));
        Assert.False(policy.AllowsLoad("form-action", "https://accounts.google.com.evil.example/auth"));
        Assert.False(policy.AllowsLoad("base-uri", "https://evil.example/"));
    }

    [Fact]
    public void TheEvaluator_ReadsDefaultSrcAndSourceListsTheWayABrowserDoes_SoItsAnswersAboveMeanSomething()
    {
        CspEvaluator policy = new("default-src 'self'; script-src 'self' https://ok.example; style-src 'unsafe-inline'; img-src 'none'", "https://localhost");

        Assert.True(policy.AllowsLoad("script-src", "https://ok.example/x.js"));
        Assert.False(policy.AllowsLoad("script-src", "https://other.example/x.js"));
        Assert.True(policy.AllowsLoad("font-src", "https://localhost/f.woff2"));
        Assert.False(policy.AllowsLoad("font-src", "https://other.example/f.woff2"));
        Assert.True(policy.AllowsInline("style-src", nonce: null));
        Assert.False(policy.AllowsInline("script-src", nonce: null));
        Assert.False(policy.AllowsLoad("img-src", "https://localhost/a.png"));
    }

    [Fact]
    public async Task APageOfAKeyedLink_SendsNoReferrer_AndOtherPagesTheDefault_KK7()
    {
        (_, string number, string accessKey) = await TestSessions.OrderAsync(_database.Dsn, customerId: null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage link = await GetAsync(client, "/geri-al/abc123");
        using HttpResponseMessage order = await GetAsync(client, $"/siparis/erisim/{accessKey}");
        using HttpResponseMessage product = await GetAsync(client, "/urun/ornek");

        Assert.Equal("no-referrer", Header(link, "Referrer-Policy"));
        Assert.Equal("no-referrer", Header(order, "Referrer-Policy"));
        Assert.Equal("strict-origin-when-cross-origin", Header(product, "Referrer-Policy"));
        _ = number;
    }

    // --- criterion 8; KK8: no-store ---

    [Fact]
    public async Task PanelAccountGuestCartAccessKeyOrderAndKeyedLink_AreNotStored_AndTheProductPageAndTheStaticFileAre_KK8()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        (_, string memberNumber, _) = await TestSessions.OrderAsync(_database.Dsn, customerId);
        (_, _, string accessKey) = await TestSessions.OrderAsync(_database.Dsn, customerId: null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string admin = $"{CookieNames.AdminSession}={adminKey}";
        string customer = $"{CookieNames.Session}={customerKey}";

        (string Path, string? Cookie)[] personal =
        [
            ("/panel/ornek-form", admin),
            ("/hesap", customer),
            ("/hesap/adresler", customer),
            ("/sepet", null),
            ($"/siparis/erisim/{accessKey}", null),
            ($"/siparis/{memberNumber}", customer),
            ("/geri-al/abc123", null),
        ];
        foreach ((string path, string? cookie) in personal)
        {
            using HttpResponseMessage response = await GetAsync(client, path, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("no-store", Header(response, "Cache-Control"));
        }

        foreach (string path in new[] { "/urun/ornek", "/statik/css/base.css" })
        {
            using HttpResponseMessage response = await GetAsync(client, path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("no-store", Header(response, "Cache-Control"), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task EveryPageOfThePanelAndTheAccountFolder_CarriesTheNoStoreAttribute_ByTheFolderConvention_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        await GetAsync(client, "/sistem/canlilik");

        PageActionDescriptor[] pages = [.. factory.Services.GetServices<EndpointDataSource>()
            .SelectMany(d => d.Endpoints)
            .Select(e => e.Metadata.GetMetadata<PageActionDescriptor>())
            .OfType<PageActionDescriptor>()];
        PageActionDescriptor[] personal = [.. pages.Where(p => p.ViewEnginePath.StartsWith("/Panel/", StringComparison.Ordinal) || p.ViewEnginePath.StartsWith("/Account/", StringComparison.Ordinal))];

        Assert.True(personal.Length > 10);
        Assert.All(personal, p => Assert.Contains(p.FilterDescriptors, d => d.Filter is NoStoreAttribute));
        Assert.All(pages.Except(personal).Where(p => p.ViewEnginePath == "/Storefront/Product"), p => Assert.DoesNotContain(p.FilterDescriptors, d => d.Filter is NoStoreAttribute));
    }

    [Fact]
    public async Task ADeniedOrMissingAnswer_IsAlsoNotStored_SoNoHeaderSaysWhichDoorWasReached_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage unrouted = await GetAsync(client, "/yok/boyle-bir-adres");
        using HttpResponseMessage unknownKey = await GetAsync(client, $"/siparis/erisim/{SecretKey.Create()}");

        Assert.Equal(HttpStatusCode.NotFound, unrouted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknownKey.StatusCode);
        Assert.Equal("no-store", Header(unrouted, "Cache-Control"));
        Assert.Equal("no-store", Header(unknownKey, "Cache-Control"));

        // T213 KK6: the missing answer carries no code, in the body or in a header (07 2.7.3).
        foreach (HttpResponseMessage answer in new[] { unrouted, unknownKey })
        {
            ErrorCodeLeakCheck.AssertNone(answer, await answer.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        }
    }
}

/// <summary>
/// A small evaluator of a content security policy, the way a browser reads it for the cases the tests ask about: the directive's own source list or the
/// <c>default-src</c> list for the fetch directives, <c>'self'</c> as the site's origin, <c>'none'</c>, host sources by origin, <c>'unsafe-inline'</c> and the
/// nonce. The directives that do not fall back to <c>default-src</c> (<c>form-action</c>, <c>frame-ancestors</c>, <c>base-uri</c>) allow everything when absent.
/// </summary>
internal sealed class CspEvaluator
{
    private static readonly string[] _noFallback = ["form-action", "frame-ancestors", "base-uri"];

    private readonly Dictionary<string, string[]> _directives;
    private readonly string _selfOrigin;

    public CspEvaluator(string policy, string selfOrigin)
    {
        ArgumentException.ThrowIfNullOrEmpty(policy);
        _selfOrigin = selfOrigin;
        _directives = policy.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => d.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToDictionary(parts => parts[0], parts => parts[1..], StringComparer.Ordinal);
    }

    /// <summary>Whether a load (a script, a frame, a form target, ...) of <paramref name="url"/> is allowed by the directive <paramref name="directive"/>.</summary>
    public bool AllowsLoad(string directive, string url)
    {
        string[]? sources = SourcesOf(directive);
        if (sources is null)
        {
            return true;
        }

        Uri target = new(url);
        string origin = target.GetLeftPart(UriPartial.Authority);
        return sources.Any(s => s switch
        {
            "'none'" => false,
            "'self'" => string.Equals(origin, _selfOrigin, StringComparison.OrdinalIgnoreCase),
            "*" => true,
            _ => !s.StartsWith('\'') && string.Equals(origin, s, StringComparison.OrdinalIgnoreCase),
        });
    }

    /// <summary>Whether an inline block (or an inline style) with this nonce is allowed by the directive.</summary>
    public bool AllowsInline(string directive, string? nonce)
    {
        string[]? sources = SourcesOf(directive);
        if (sources is null)
        {
            return true;
        }

        return sources.Contains("'unsafe-inline'", StringComparer.Ordinal) || (nonce is not null && sources.Contains($"'nonce-{nonce}'", StringComparer.Ordinal));
    }

    private string[]? SourcesOf(string directive)
    {
        if (_directives.TryGetValue(directive, out string[]? own))
        {
            return own;
        }

        return !_noFallback.Contains(directive, StringComparer.Ordinal) && _directives.TryGetValue("default-src", out string[]? fallback) ? fallback : null;
    }
}
