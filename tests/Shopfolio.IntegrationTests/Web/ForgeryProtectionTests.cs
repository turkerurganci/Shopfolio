using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages.Panel;
using Shopfolio.IntegrationTests.SignedEndpoints;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The protection against cross-site forgery on the opened application and the test-only sample page (T209 criteria 1, 4, 5, 6; KK1, KK4, KK5, KK6;
/// 07 2.5.1, 2.5.4, 2.5.5; 05 6.5.1; 09 15.2 items 1, 2): a POST without the token, with a wrong token, with a token of another pair or of another user is
/// refused with 400, writes nothing, runs no handler and shows the page again with what was typed and a new token; the token in the header and the token
/// in the form field both pass; a GET needs none; every page endpoint carries the protection and no writing endpoint is outside the closed list.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed partial class ForgeryProtectionTests
{
    private const string PagePath = "/panel/ornek-form";

    private readonly CollectionDatabaseFixture _database;
    private readonly string _adminKey;
    private ShopfolioWebApplicationFactory? _root;

    public ForgeryProtectionTests(CollectionDatabaseFixture database)
    {
        _database = database;
        _adminKey = TestSessions.AdminAsync(database.Dsn).GetAwaiter().GetResult().Key;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*?value=\"([^\"]+)\"", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FormToken();

    private WebApplicationFactory<Program> Open(out PatternRecordStore store, bool withUploadEndpoint = false)
    {
        PatternRecordStore records = new();
        store = records;
        ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        _root = factory;
        return factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddSingleton(records);
            s.AddRazorPages().AddApplicationPart(typeof(PatternFormModel).Assembly);
            if (withUploadEndpoint)
            {
                s.AddSingleton<IStartupFilter, UploadEndpointStartupFilter>();
            }
        }));
    }

    // The lines of one request in the application log file of the opened application (the same reading as the field validation tests').
    private List<JsonElement> LinesOf(string requestId)
    {
        List<JsonElement> lines = [];
        foreach (string path in Directory.GetFiles(_root!.LogDirectory, "app-*.jsonl"))
        {
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new(stream);
            while (reader.ReadLine() is { } text)
            {
                JsonElement line = JsonDocument.Parse(text).RootElement.Clone();
                if (line.TryGetProperty("requestId", out JsonElement id) && id.GetString() == requestId)
                {
                    lines.Add(line);
                }
            }
        }

        return lines;
    }

    private string AdminCookie => $"{CookieNames.AdminSession}={_adminKey}";

    private static string Decoded(string html) => WebUtility.HtmlDecode(html);

    private async Task<HttpResponseMessage> PostAddAsync(HttpClient client, string name, Action<HttpRequestMessage>? configure = null, string handler = "Add")
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{PagePath}?handler={handler}")
        {
            Content = new FormUrlEncodedContent([new KeyValuePair<string, string>("Input.Name", name)]),
        };
        request.Headers.TryAddWithoutValidation(HeaderNames.Cookie, AdminCookie);
        configure?.Invoke(request);
        return await client.SendAsync(request, Ct);
    }

    // A token pair as the test token page makes it: the request half and the cookie half ("name=value").
    private static async Task<(string Token, string Cookie)> MintAsync(HttpClient client, string scheme, string? cookie)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"{ForgeryTokenHandler.TokenPath}?scheme={scheme}");
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation(HeaderNames.Cookie, cookie);
        }

        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        string cookiePair = response.Headers.GetValues(HeaderNames.SetCookie).Select(c => c.Split(';', 2)[0]).First(c => c.StartsWith(CookieNames.Antiforgery + "=", StringComparison.Ordinal));
        return (await response.Content.ReadAsStringAsync(Ct), cookiePair);
    }

    // --- criterion 4, 5; KK4, KK5: the refused POST ---

    [Fact]
    public async Task Post_WithoutAToken_Is400_WritesNothing_ShowsTheSentValueAMessageAndANewToken_KK4_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAddAsync(client, "yazilan-ad", r => r.Headers.Add(ForgeryTokenHandler.SkipHeader, "1"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(store.Snapshot());
        string body = Decoded(await response.Content.ReadAsStringAsync(Ct));
        Assert.Contains("value=\"yazilan-ad\"", body, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, body, StringComparison.Ordinal);
        Assert.Matches(FormToken(), body);
        Assert.Contains(response.Headers.GetValues(HeaderNames.SetCookie), c => c.StartsWith(CookieNames.Antiforgery + "=", StringComparison.Ordinal));
    }

    // --- criterion 8; KK6, KK8 (T213): one rejection line with the code, and no code in the answer ---

    [Fact]
    public async Task Post_WithoutAToken_WritesOneRejectionLineWithTheCode_NoFieldsNoValue_AndNoCodeNameInTheAnswer_KK6_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string requestId = "t213-" + Guid.NewGuid().ToString("N");

        using HttpResponseMessage response = await PostAddAsync(client, "yazilan-ad", r =>
        {
            r.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
            r.Headers.Add("X-Request-Id", requestId);
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(store.Snapshot());
        JsonElement line = Assert.Single(LinesOf(requestId), l => l.TryGetProperty("event", out JsonElement e) && e.GetString() == LogEvents.ValidationRejected);
        Assert.DoesNotContain(LinesOf(requestId), l => l.TryGetProperty("code", out _) && l.GetProperty("category").GetString() != typeof(RejectionLog).FullName);
        Assert.Equal(LogEvents.ValidationRejected, line.GetProperty("event").GetString());
        Assert.Equal(nameof(ErrorCode.AntiforgeryValidationFailed), line.GetProperty("code").GetString());
        Assert.False(line.TryGetProperty("fields", out _));
        Assert.DoesNotContain("yazilan-ad", line.GetRawText(), StringComparison.Ordinal);

        ErrorCodeLeakCheck.AssertNone(response, await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task Post_WithAValidToken_WritesNoRejectionLine_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string requestId = "t213-" + Guid.NewGuid().ToString("N");

        using HttpResponseMessage response = await PostAddAsync(client, "ad", r => r.Headers.Add("X-Request-Id", requestId));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(["ad"], store.Snapshot());
        Assert.DoesNotContain(LinesOf(requestId), l => l.TryGetProperty("code", out _) || l.TryGetProperty("fields", out _));
    }

    [Fact]
    public async Task Post_WithoutAToken_ThatAlsoBreaksAFieldRule_ShowsNoRuleCode_AndKeepsTheSentValue_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string tooLong = new('x', 40);

        using HttpResponseMessage response = await PostAddAsync(client, tooLong, r => r.Headers.Add(ForgeryTokenHandler.SkipHeader, "1"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = Decoded(await response.Content.ReadAsStringAsync(Ct));
        Assert.Contains($"value=\"{tooLong}\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain("field-validation-error", body, StringComparison.Ordinal);
        Assert.DoesNotContain("StringLength", body, StringComparison.Ordinal);
        Assert.Empty(store.Snapshot());
    }

    [Fact]
    public async Task Post_WithAWrongToken_Is400_AndWritesNothing_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAddAsync(client, "ad", r => r.Headers.Add(CookieSetup.AntiforgeryHeaderName, "not-a-token"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(store.Snapshot());
    }

    [Fact]
    public async Task Post_WithAValidRequestTokenButNoCookieHalf_Is400_AndWritesNothing_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string token, _) = await MintAsync(client, "Admin", AdminCookie);

        using HttpResponseMessage response = await PostAddAsync(client, "ad", r => r.Headers.Add(CookieSetup.AntiforgeryHeaderName, token));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(store.Snapshot());
    }

    [Fact]
    public async Task Post_WithTheTokenOfOnePairAndTheCookieOfAnother_Is400_AndWritesNothing_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string firstToken, _) = await MintAsync(client, "Admin", AdminCookie);
        (_, string secondCookie) = await MintAsync(client, "Admin", AdminCookie);

        using HttpResponseMessage response = await PostAddAsync(client, "ad", r =>
        {
            r.Headers.Remove(HeaderNames.Cookie);
            r.Headers.TryAddWithoutValidation(HeaderNames.Cookie, $"{AdminCookie}; {secondCookie}");
            r.Headers.Add(CookieSetup.AntiforgeryHeaderName, firstToken);
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(store.Snapshot());
    }

    [Fact]
    public async Task Post_WithATokenMadeForAnotherUser_Is400_AndWritesNothing_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string token, string cookie) = await MintAsync(client, "Customer", null);

        using HttpResponseMessage response = await PostAddAsync(client, "ad", r =>
        {
            r.Headers.Remove(HeaderNames.Cookie);
            r.Headers.TryAddWithoutValidation(HeaderNames.Cookie, $"{AdminCookie}; {cookie}");
            r.Headers.Add(CookieSetup.AntiforgeryHeaderName, token);
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(store.Snapshot());
    }

    [Fact]
    public async Task Post_OfAHandlerThatAnswers200_WithoutAToken_RunsNoHandler_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAddAsync(client, "x", r => r.Headers.Add(ForgeryTokenHandler.SkipHeader, "1"), handler: "Export");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotEqual("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Empty(store.Snapshot());
    }

    // --- criterion 4: the two places of the value ---

    [Fact]
    public async Task Post_WithTheTokenInTheHeader_Passes_AndTheWriteHappens_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAddAsync(client, "ad");

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(["ad"], store.Snapshot());
    }

    [Fact]
    public async Task Post_WithTheTokenFieldOfTheFormThePageShowed_Passes_AndTheWriteHappens_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpRequestMessage get = new(HttpMethod.Get, PagePath);
        get.Headers.TryAddWithoutValidation(HeaderNames.Cookie, AdminCookie);
        using HttpResponseMessage page = await client.SendAsync(get, Ct);
        string formValue = FormToken().Match(await page.Content.ReadAsStringAsync(Ct)).Groups[1].Value;
        string cookie = page.Headers.GetValues(HeaderNames.SetCookie).Select(c => c.Split(';', 2)[0]).First(c => c.StartsWith(CookieNames.Antiforgery + "=", StringComparison.Ordinal));
        Assert.NotEmpty(formValue);

        using HttpRequestMessage post = new(HttpMethod.Post, $"{PagePath}?handler=Add")
        {
            Content = new FormUrlEncodedContent([new KeyValuePair<string, string>("Input.Name", "formdan"), new KeyValuePair<string, string>("__RequestVerificationToken", formValue)]),
        };
        post.Headers.TryAddWithoutValidation(HeaderNames.Cookie, $"{AdminCookie}; {cookie}");
        post.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        using HttpResponseMessage response = await client.SendAsync(post, Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(["formdan"], store.Snapshot());
    }

    [Fact]
    public async Task Get_NeedsNoToken_AndWritesNothing()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpRequestMessage get = new(HttpMethod.Get, PagePath);
        get.Headers.TryAddWithoutValidation(HeaderNames.Cookie, AdminCookie);

        using HttpResponseMessage response = await client.SendAsync(get, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(store.Snapshot());
    }

    // --- criterion 1; KK1: no request identity, a second send is a second effect where the list says so ---

    [Fact]
    public async Task TheSameFormSentTwice_WithTheListedOperationsOfCriterion1_WritesTwice_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage first = await PostAddAsync(client, "ayni");
        using HttpResponseMessage second = await PostAddAsync(client, "ayni");

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, second.StatusCode);
        Assert.Equal(["ayni", "ayni"], store.Snapshot());
        Assert.False(first.Headers.Contains("Idempotency-Key"));
    }

    // --- criterion 6; KK6: the closed list ---

    [Fact]
    public async Task EveryPageEndpointOfTheApplication_CarriesTheProtection_AndNoWritingEndpointIsOutsideTheClosedList_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        await client.GetAsync(PagePath, Ct);

        List<AuditedEndpoint> endpoints = WritingEndpointAudit.Of([.. factory.Services.GetServices<EndpointDataSource>().SelectMany(d => d.Endpoints)]);

        Assert.Contains(endpoints, e => e.IsPage && e.Pattern == PagePath && e.HasPolicy);
        Assert.True(endpoints.Count(e => e.IsPage) > 10);
        Assert.Empty(WritingEndpointAudit.Violations(endpoints));
    }

    [Fact]
    public async Task TheAudit_IsRedOnAWritingEndpointThatIsNotPageNorOnTheClosedList_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open(out _, withUploadEndpoint: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        await client.GetAsync(PagePath, Ct);

        List<string> violations = WritingEndpointAudit.Violations(WritingEndpointAudit.Of([.. factory.Services.GetServices<EndpointDataSource>().SelectMany(d => d.Endpoints)]));

        Assert.Equal(["/panel/yukleme-deneme: a writing endpoint outside the closed list"], violations);
    }

    [Fact]
    public void TheAudit_IsRedOnAPageWithoutTheMarkAndOnAPageWithTheIgnoreAttribute_KK6()
    {
        List<AuditedEndpoint> endpoints =
        [
            new("/a", IsPage: true, HasPolicy: false, HasIgnoreAttribute: false, AcceptsWrites: true, SwitchedOff: false),
            new("/b", IsPage: true, HasPolicy: true, HasIgnoreAttribute: true, AcceptsWrites: true, SwitchedOff: false),
            new("/c", IsPage: true, HasPolicy: true, HasIgnoreAttribute: false, AcceptsWrites: true, SwitchedOff: false),
        ];

        Assert.Equal(["/a: a page without the protection's policy marker", "/b: a page with the ignore attribute"], WritingEndpointAudit.Violations(endpoints));
    }

    [Fact]
    public void TheAudit_AcceptsTheSignedEndpointOnTheClosedList_AndIsRedOnOneOffTheListOrOnAListedOneWithTheCheckOn_KK6()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.Services.AddAntiforgery();
        WebApplication app = builder.Build();
        app.MapSignedEndpoint<DatabaseTestEndpoint, DatabaseTestNotice>(AntiforgeryExemptions.PaymentNotificationPath);
        app.MapSignedEndpoint<DatabaseTestEndpoint, DatabaseTestNotice>("/sistem/baska-bildirim");
        app.MapPost("/sistem/odeme-bildirimi-iki", () => Results.Ok());
        List<AuditedEndpoint> endpoints = WritingEndpointAudit.Of([.. ((IEndpointRouteBuilder)app).DataSources.SelectMany(d => d.Endpoints)]);

        List<string> violations = WritingEndpointAudit.Violations(endpoints);

        Assert.Equal(
            [
                "/sistem/baska-bildirim: a writing endpoint outside the closed list",
                "/sistem/baska-bildirim: its check is switched off but it is not on the closed list",
                "/sistem/odeme-bildirimi-iki: a writing endpoint outside the closed list",
            ],
            [.. violations.Order(StringComparer.Ordinal)]);
        Assert.DoesNotContain(violations, v => v.StartsWith(AntiforgeryExemptions.PaymentNotificationPath + ":", StringComparison.Ordinal));
        Assert.True(endpoints.Single(e => e.Pattern == AntiforgeryExemptions.PaymentNotificationPath).SwitchedOff);
    }

    [Fact]
    public void AListedPathThatIsMappedWithTheCheckOn_IsRedToo_KK6()
    {
        List<AuditedEndpoint> endpoints =
        [
            new(AntiforgeryExemptions.PaymentNotificationPath, IsPage: false, HasPolicy: false, HasIgnoreAttribute: false, AcceptsWrites: true, SwitchedOff: false),
        ];

        Assert.Equal(["/sistem/odeme-bildirimi: on the closed list but its check is not switched off"], WritingEndpointAudit.Violations(endpoints));
    }

    // --- D-131: the header-token list of the chunked upload (09 15.7 item 4) ---

    [Fact]
    public void TheAudit_AcceptsTheChunkedUploadsPostsWithTheHeaderCheck_AndAReadingEndpointOnTheSamePattern_D131()
    {
        List<AuditedEndpoint> endpoints =
        [
            new(AntiforgeryExemptions.UploadStartPath, IsPage: false, HasPolicy: false, HasIgnoreAttribute: false, AcceptsWrites: true, SwitchedOff: false, HasHeaderToken: true),
            new(AntiforgeryExemptions.UploadChunkPath, IsPage: false, HasPolicy: false, HasIgnoreAttribute: false, AcceptsWrites: true, SwitchedOff: false, HasHeaderToken: true),
            new(AntiforgeryExemptions.UploadChunkPath, IsPage: false, HasPolicy: false, HasIgnoreAttribute: false, AcceptsWrites: false, SwitchedOff: false),
        ];

        Assert.Empty(WritingEndpointAudit.Violations(endpoints));
    }

    [Fact]
    public void TheAudit_IsRedOnAHeaderTokenPathWithoutTheCheck_OnTheCheckOffTheList_OnAListedPathWithTheCheckSwitchedOff_AndOnAnyOtherWritingEndpoint_D131()
    {
        List<AuditedEndpoint> endpoints =
        [
            new(AntiforgeryExemptions.UploadStartPath, IsPage: false, HasPolicy: false, HasIgnoreAttribute: false, AcceptsWrites: true, SwitchedOff: false),
            new("/panel/baska-yukleme", IsPage: false, HasPolicy: false, HasIgnoreAttribute: false, AcceptsWrites: true, SwitchedOff: false, HasHeaderToken: true),
            new(AntiforgeryExemptions.UploadChunkPath, IsPage: false, HasPolicy: false, HasIgnoreAttribute: false, AcceptsWrites: true, SwitchedOff: true, HasHeaderToken: true),
            new("/panel/yukleme/{id}/iptal", IsPage: false, HasPolicy: false, HasIgnoreAttribute: false, AcceptsWrites: true, SwitchedOff: false),
        ];

        Assert.Equal(
            [
                "/panel/baska-yukleme: a writing endpoint outside the closed list",
                "/panel/baska-yukleme: carries the header token check but is not on the header token list",
                "/panel/yukleme: on the header token list but carries no header token check",
                "/panel/yukleme/{id}: on the header token list but its check is switched off",
                "/panel/yukleme/{id}/iptal: a writing endpoint outside the closed list",
            ],
            [.. WritingEndpointAudit.Violations(endpoints).Order(StringComparer.Ordinal)]);
    }
}

