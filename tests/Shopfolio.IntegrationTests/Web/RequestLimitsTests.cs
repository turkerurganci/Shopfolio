using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.IntegrationTests.SignedEndpoints;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The request limits on the opened application (T215 criteria 1 to 5; KK1 to KK5; D-112; 07 2.8; 09 15.4): the body ceiling per endpoint class answers
/// 413 before the endpoint runs, the general limit answers 429 on a page with the neutral message and no form and on a machine endpoint with no body, and
/// neither adds <c>Retry-After</c>; the static files and images are outside the limit; the payment notification endpoint is outside the limit while its
/// signature is right and a bad signature counts towards the form submission limit; the forwarded headers are believed from the proxy's inner network only.
/// The clock of the application is a fake one: the window moves by the test, nobody waits. The test endpoints and the peer address live in the test host only.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class RequestLimitsTests
{
    private const long Kilobyte = 1024;
    private const long Megabyte = 1024 * Kilobyte;

    private const string GeneralPath = "/iletisim/test-govde";
    private const string LongTextPath = "/iletisim/test-govde-uzun";
    private const string MachinePath = "/sistem/test-govde";
    private const string ProbePath = "/sistem/test-istemci";
    private const string PeerHeader = "X-Test-Peer";

    private readonly CollectionDatabaseFixture _database;

    public RequestLimitsTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private WebApplicationFactory<Program> Open(ManualTimeProvider time, int pages = 1000, int forms = 1000, EndpointCalls? calls = null, List<string>? logDirectory = null)
    {
        EndpointCalls seen = calls ?? new EndpointCalls();
        ShopfolioWebApplicationFactory factory = new(
            _database.Dsn,
            new Dictionary<string, string>
            {
                [RateLimitOptions.PageRequestsPerMinuteKey] = pages.ToString(System.Globalization.CultureInfo.InvariantCulture),
                [RateLimitOptions.FormSubmissionsPerMinuteKey] = forms.ToString(System.Globalization.CultureInfo.InvariantCulture),
            });
        logDirectory?.Add(factory.LogDirectory);
        return factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddSingleton<TimeProvider>(time);
            s.AddSingleton(seen);
            s.AddScoped<DatabaseTestEndpoint>();
            s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly);
            s.AddSingleton<IStartupFilter, LimitsStartupFilter>();
        }));
    }

    private static ManualTimeProvider Clock() => new(new DateTimeOffset(2026, 11, 6, 9, 0, 0, TimeSpan.Zero));

    private static HttpClient ClientOf(WebApplicationFactory<Program> factory, string baseAddress = "https://localhost")
        => factory.CreateDefaultClient(new Uri(baseAddress));

    private static async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string path, HttpContent? content = null, string? peer = null, string? forwardedFor = null, string? forwardedProto = null)
    {
        using HttpRequestMessage request = new(method, path) { Content = content };
        if (peer is not null)
        {
            request.Headers.Add(PeerHeader, peer);
        }

        if (forwardedFor is not null)
        {
            request.Headers.Add("X-Forwarded-For", forwardedFor);
        }

        if (forwardedProto is not null)
        {
            request.Headers.Add("X-Forwarded-Proto", forwardedProto);
        }

        return await client.SendAsync(request, Ct);
    }

    private static ByteArrayContent Bytes(long count) => new(new byte[count]);

    // --- KK1, KK2: the body ceiling per class ---

    [Theory]
    [InlineData(GeneralPath, 64 * Kilobyte)]
    [InlineData(LongTextPath, Megabyte)]
    public async Task ABodyAtTheClassCeiling_ReachesTheEndpoint_AndOneByteOverIsRefusedWith413BeforeTheEndpointRuns(string path, long ceiling)
    {
        EndpointCalls calls = new();
        await using WebApplicationFactory<Program> factory = Open(Clock(), calls: calls);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage within = await SendAsync(client, HttpMethod.Post, path, Bytes(ceiling));
        int callsAtCeiling = calls.Count;
        using HttpResponseMessage over = await SendAsync(client, HttpMethod.Post, path, Bytes(ceiling + 1));

        Assert.Equal(HttpStatusCode.OK, within.StatusCode);
        Assert.Equal(1, callsAtCeiling);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, over.StatusCode);
        Assert.Equal(callsAtCeiling, calls.Count);
    }

    [Fact]
    public async Task TheGeneralClass_RefusesWhatTheLongTextClassAccepts()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock());
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage general = await SendAsync(client, HttpMethod.Post, GeneralPath, Bytes(100 * Kilobyte));
        using HttpResponseMessage longText = await SendAsync(client, HttpMethod.Post, LongTextPath, Bytes(100 * Kilobyte));

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, general.StatusCode);
        Assert.Equal(HttpStatusCode.OK, longText.StatusCode);
    }

    [Fact]
    public async Task TheRefusal_OfAPage_IsAPlainPageWithNoFormAndNoRetryAfter_AndNothingWasRead()
    {
        EndpointCalls calls = new();
        await using WebApplicationFactory<Program> factory = Open(Clock(), calls: calls);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await SendAsync(client, HttpMethod.Post, GeneralPath, Bytes(64 * Kilobyte + 1));
        string html = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(WebUtility.HtmlEncode(RequestBodyLimitMiddleware.Message), html, StringComparison.Ordinal);
        Assert.DoesNotContain("<form", html, StringComparison.OrdinalIgnoreCase);
        Assert.False(response.Headers.Contains("Retry-After"));
        Assert.Equal(0, calls.Count);
    }

    [Fact]
    public async Task TheRefusal_OfAMachineEndpoint_HasNoBody()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock());
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await SendAsync(client, HttpMethod.Post, MachinePath, Bytes(64 * Kilobyte + 1));

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task ABodyWithNoDeclaredLength_IsMeasuredAsItIsRead_AndRefusedPastTheCeiling()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock());
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage within = await SendAsync(client, HttpMethod.Post, GeneralPath, new StreamContent(new MemoryStream(new byte[64 * Kilobyte])));
        using HttpResponseMessage over = await SendAsync(client, HttpMethod.Post, GeneralPath, new StreamContent(new MemoryStream(new byte[(64 * Kilobyte) + 1])));

        Assert.Equal(HttpStatusCode.OK, within.StatusCode);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, over.StatusCode);
    }

    // The same ceiling on the application's own pipeline behind a real Kestrel (a test server has no body size limit of its own and throws nothing): a body
    // with no declared length is read by the server, which throws past its limit, and the middleware turns that into the 413 (B1 of the validation round 1).
    [Theory]
    [InlineData(GeneralPath, 64 * Kilobyte, true)]
    [InlineData(GeneralPath, 64 * Kilobyte, false)]
    [InlineData(LongTextPath, Megabyte, true)]
    [InlineData(LongTextPath, Megabyte, false)]
    public async Task OnARealServer_ABodyBelowAtAndOverTheCeiling_IsAcceptedOrRefusedWith413_ChunkedOrWithDeclaredLength(string path, long ceiling, bool chunked)
    {
        List<string> logs = [];
        EndpointCalls calls = new();
        await using WebApplicationFactory<Program> factory = Open(Clock(), calls: calls, logDirectory: logs);
        factory.UseKestrel(0);
        factory.StartServer();
        using HttpClient client = factory.CreateClient();

        long[] accepted = [1000, ceiling - 1, ceiling];
        long[] refused = [ceiling + 1, ceiling + (4 * Megabyte), 40 * Megabyte];
        foreach (long size in accepted)
        {
            using HttpResponseMessage response = await client.SendAsync(RealRequest(path, size, chunked, "ok-" + size), Ct);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"{size} bytes, chunked={chunked}: {(int)response.StatusCode}");
            Assert.Equal(size.ToString(System.Globalization.CultureInfo.InvariantCulture), await response.Content.ReadAsStringAsync(Ct));
        }

        int callsBefore = calls.Count;
        foreach (long size in refused)
        {
            string id = "t215-" + Guid.NewGuid().ToString("N");
            using HttpRequestMessage request = RealRequest(path, size, chunked, id);
            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(request, Ct);
            }
            catch (HttpRequestException) when (size > ceiling + Megabyte)
            {
                // The server closes the connection after the 413 without reading the rest; a client that is still writing a large body may see the
                // connection reset before it reads the answer; the endpoint was not called either way.
                continue;
            }

            using HttpResponseMessage answered = response;
            string html = await answered.Content.ReadAsStringAsync(Ct);

            Assert.True(answered.StatusCode == HttpStatusCode.RequestEntityTooLarge, $"{size} bytes, chunked={chunked}: {(int)answered.StatusCode}");
            Assert.Contains(WebUtility.HtmlEncode(RequestBodyLimitMiddleware.Message), html, StringComparison.Ordinal);
            Assert.DoesNotContain("<form", html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(nameof(Shopfolio.Infrastructure.Errors.ErrorCode.PayloadTooLarge), html, StringComparison.Ordinal);
            JsonElement line = Assert.Single(LinesOf(logs[0], id), l => l.TryGetProperty("code", out _));
            Assert.Equal(nameof(Shopfolio.Infrastructure.Errors.ErrorCode.PayloadTooLarge), line.GetProperty("code").GetString());
        }

        Assert.Equal(callsBefore, calls.Count);
    }

    [Fact]
    public async Task OnARealServer_ARefusedChunkedBody_IsNotAnUnexpectedError_AndTheMachineEndpointAnswersWithNoBody()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock());
        factory.UseKestrel(0);
        factory.StartServer();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage machine = await client.SendAsync(RealRequest(MachinePath, (64 * Kilobyte) + 1, chunked: true, "m"), Ct);
        using HttpResponseMessage within = await client.SendAsync(RealRequest(MachinePath, (64 * Kilobyte) - 1, chunked: true, "m2"), Ct);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, machine.StatusCode);
        Assert.Equal(string.Empty, await machine.Content.ReadAsStringAsync(Ct));
        Assert.Equal(HttpStatusCode.OK, within.StatusCode);
    }

    private static HttpRequestMessage RealRequest(string path, long size, bool chunked, string requestId)
    {
        // A seekable stream would give the content a length; the header makes the client send the body chunked, with no Content-Length.
        HttpRequestMessage request = new(HttpMethod.Post, path)
        {
            Content = chunked ? new StreamContent(new MemoryStream(new byte[size])) : new ByteArrayContent(new byte[size]),
        };
        if (chunked)
        {
            request.Headers.TransferEncodingChunked = true;
        }

        request.Headers.Add("X-Request-Id", requestId);
        return request;
    }

    [Fact]
    public async Task AGetCarriesNoBody_AGetWithOneIsRefusedWith413()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock());
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage plain = await SendAsync(client, HttpMethod.Get, ProbePath);
        using HttpResponseMessage withBody = await SendAsync(client, HttpMethod.Get, ProbePath, new StringContent("x", Encoding.UTF8));

        Assert.Equal(HttpStatusCode.OK, plain.StatusCode);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, withBody.StatusCode);
    }

    // --- KK3, KK5: the general limit ---

    [Fact]
    public async Task APage_IsRefusedWith429AfterThePageLimit_NeutralMessageNoFormNoRetryAfter_AndTheNextWindowOpensIt()
    {
        ManualTimeProvider time = Clock();
        await using WebApplicationFactory<Program> factory = Open(time, pages: 3);
        using HttpClient client = ClientOf(factory);
        for (int i = 0; i < 3; i++)
        {
            using HttpResponseMessage ok = await SendAsync(client, HttpMethod.Get, "/urunler");
            Assert.NotEqual(HttpStatusCode.TooManyRequests, ok.StatusCode);
        }

        using HttpResponseMessage refused = await SendAsync(client, HttpMethod.Get, "/urunler");
        string html = await refused.Content.ReadAsStringAsync(Ct);
        time.Advance(RateLimitDefaults.Window);
        using HttpResponseMessage again = await SendAsync(client, HttpMethod.Get, "/urunler");

        Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
        Assert.Contains(WebUtility.HtmlEncode(NeutralMessages.LimitExceeded), html, StringComparison.Ordinal);
        Assert.DoesNotContain("<form", html, StringComparison.OrdinalIgnoreCase);
        Assert.False(refused.Headers.Contains("Retry-After"));
        Assert.Equal("no-store", refused.Headers.CacheControl?.ToString());
        Assert.NotEqual(HttpStatusCode.TooManyRequests, again.StatusCode);
    }

    [Fact]
    public async Task AMachineEndpoint_IsRefusedWith429AndNoBodyAndNoRetryAfter()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock(), pages: 2);
        using HttpClient client = ClientOf(factory);
        for (int i = 0; i < 2; i++)
        {
            using HttpResponseMessage ok = await SendAsync(client, HttpMethod.Get, "/sistem/canlilik");
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        using HttpResponseMessage refused = await SendAsync(client, HttpMethod.Get, "/sistem/canlilik");

        Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
        Assert.Equal(string.Empty, await refused.Content.ReadAsStringAsync(Ct));
        Assert.False(refused.Headers.Contains("Retry-After"));
    }

    [Fact]
    public async Task StaticFilesAndImages_AreOutsideTheLimit()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock(), pages: 2, forms: 1);
        using HttpClient client = ClientOf(factory);

        for (int i = 0; i < 10; i++)
        {
            using HttpResponseMessage file = await SendAsync(client, HttpMethod.Get, "/statik/yok.css");
            using HttpResponseMessage image = await SendAsync(client, HttpMethod.Get, "/gorsel/yok.webp");
            using HttpResponseMessage post = await SendAsync(client, HttpMethod.Post, "/statik/yok.css", new StringContent("x"));
            Assert.NotEqual(HttpStatusCode.TooManyRequests, file.StatusCode);
            Assert.NotEqual(HttpStatusCode.TooManyRequests, image.StatusCode);
            Assert.NotEqual(HttpStatusCode.TooManyRequests, post.StatusCode);
        }

        using HttpResponseMessage page = await SendAsync(client, HttpMethod.Get, "/urunler");
        Assert.NotEqual(HttpStatusCode.TooManyRequests, page.StatusCode);
    }

    [Fact]
    public async Task PostsAreRefusedWith429AfterTheFormLimit_AGetStillPasses_AndAnotherAddressIsFree()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock(), forms: 2);
        using HttpClient client = ClientOf(factory);
        for (int i = 0; i < 2; i++)
        {
            using HttpResponseMessage ok = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), peer: "198.51.100.7");
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        using HttpResponseMessage refused = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), peer: "198.51.100.7");
        using HttpResponseMessage get = await SendAsync(client, HttpMethod.Get, ProbePath, peer: "198.51.100.7");
        using HttpResponseMessage other = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), peer: "198.51.100.8");

        Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal(HttpStatusCode.OK, other.StatusCode);
    }

    [Fact]
    public async Task TwoAddressesOfOneV6Prefix_ShareOneCounter()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock(), forms: 2);
        using HttpClient client = ClientOf(factory);
        using HttpResponseMessage first = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), peer: "2001:db8:5:5:1::1");
        using HttpResponseMessage second = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), peer: "2001:db8:5:5:2::2");

        using HttpResponseMessage third = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), peer: "2001:db8:5:5:ffff::3");
        using HttpResponseMessage elsewhere = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), peer: "2001:db8:5:6:1::1");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
        Assert.Equal(HttpStatusCode.OK, elsewhere.StatusCode);
    }

    [Fact]
    public async Task TheChunkedUpload_IsUnderThePageLimitOnly_NotTheFormLimit()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock(), forms: 2);
        using HttpClient client = ClientOf(factory);

        for (int i = 0; i < 6; i++)
        {
            using HttpResponseMessage chunk = await SendAsync(client, HttpMethod.Post, "/panel/yukleme/parca", new StringContent("x"));
            Assert.NotEqual(HttpStatusCode.TooManyRequests, chunk.StatusCode);
        }

        using HttpResponseMessage form = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"));
        Assert.Equal(HttpStatusCode.OK, form.StatusCode);
    }

    // --- D-143, 09 8.2 items 3, 4 and 5: the refusals name no code and write their one line with it ---

    [Fact]
    public async Task The429_OfAPageAFormAndAMachineEndpoint_NamesNoErrorCode_InTheBodyTheHeadersOrTheContentHeaders()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock(), pages: 1, forms: 1);
        using HttpClient client = ClientOf(factory);
        using HttpResponseMessage pageOk = await SendAsync(client, HttpMethod.Get, "/urunler", peer: "198.51.100.40");
        using HttpResponseMessage machineOk = await SendAsync(client, HttpMethod.Get, "/sistem/canlilik", peer: "198.51.100.41");
        using HttpResponseMessage formOk = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), peer: "198.51.100.42");
        Assert.NotEqual(HttpStatusCode.TooManyRequests, pageOk.StatusCode);
        Assert.NotEqual(HttpStatusCode.TooManyRequests, machineOk.StatusCode);
        Assert.NotEqual(HttpStatusCode.TooManyRequests, formOk.StatusCode);

        using HttpResponseMessage page = await SendAsync(client, HttpMethod.Get, "/urunler", peer: "198.51.100.40");
        using HttpResponseMessage machine = await SendAsync(client, HttpMethod.Get, "/sistem/canlilik", peer: "198.51.100.41");
        using HttpResponseMessage form = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), peer: "198.51.100.42");

        foreach (HttpResponseMessage refused in new[] { page, machine, form })
        {
            Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
            ErrorCodeLeakCheck.AssertNone(refused, await refused.Content.ReadAsStringAsync(Ct));
        }

        // The page answer has a content type header of its own: the scan reads it (response.Content.Headers).
        Assert.Contains("Content-Type: text/html", ErrorCodeLeakCheck.HeaderText(page), StringComparison.Ordinal);
        Assert.NotEmpty(await page.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task The413_OfAPageAndAMachineEndpoint_NamesNoErrorCode_InTheBodyTheHeadersOrTheContentHeaders()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock());
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage page = await SendAsync(client, HttpMethod.Post, GeneralPath, Bytes(64 * Kilobyte + 1));
        using HttpResponseMessage machine = await SendAsync(client, HttpMethod.Post, MachinePath, Bytes(64 * Kilobyte + 1));

        foreach (HttpResponseMessage refused in new[] { page, machine })
        {
            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, refused.StatusCode);
            ErrorCodeLeakCheck.AssertNone(refused, await refused.Content.ReadAsStringAsync(Ct));
        }
    }

    [Fact]
    public void TheLeakCheck_FindsACodeNameInAContentHeader_AndInTheBody()
    {
        using HttpResponseMessage inContentHeader = new(HttpStatusCode.TooManyRequests) { Content = new StringContent("x") };
        inContentHeader.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = nameof(Shopfolio.Infrastructure.Errors.ErrorCode.RateLimitExceeded) };
        using HttpResponseMessage inResponseHeader = new(HttpStatusCode.TooManyRequests) { Content = new StringContent("x") };
        inResponseHeader.Headers.Add("X-Test", nameof(Shopfolio.Infrastructure.Errors.ErrorCode.PayloadTooLarge));
        using HttpResponseMessage clean = new(HttpStatusCode.TooManyRequests) { Content = new StringContent("x") };

        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ErrorCodeLeakCheck.AssertNone(inContentHeader, "x"));
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ErrorCodeLeakCheck.AssertNone(inResponseHeader, "x"));
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ErrorCodeLeakCheck.AssertNone(clean, "RateLimitExceeded"));
        ErrorCodeLeakCheck.AssertNone(clean, "x");
    }

    [Fact]
    public async Task EachRefusal_WritesOneRejectionLineWithItsCode_Through_TheRejectionLog()
    {
        List<string> logs = [];
        await using WebApplicationFactory<Program> factory = Open(Clock(), pages: 1, logDirectory: logs);
        using HttpClient client = ClientOf(factory);
        string limitedId = "t215-" + Guid.NewGuid().ToString("N");
        string largeId = "t215-" + Guid.NewGuid().ToString("N");

        using HttpResponseMessage first = await SendAsync(client, HttpMethod.Get, "/urunler", forwardedFor: null, peer: "198.51.100.50");
        using HttpResponseMessage limited = await SendWithIdAsync(client, HttpMethod.Get, "/urunler", limitedId, "198.51.100.50");
        using HttpResponseMessage large = await SendWithIdAsync(client, HttpMethod.Post, MachinePath, largeId, "198.51.100.51", Bytes(64 * Kilobyte + 1));

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, large.StatusCode);
        JsonElement limitedLine = Assert.Single(LinesOf(logs[0], limitedId));
        JsonElement largeLine = Assert.Single(LinesOf(logs[0], largeId));
        Assert.Equal(LogEvents.RateLimitExceeded, limitedLine.GetProperty("event").GetString());
        Assert.Equal(nameof(Shopfolio.Infrastructure.Errors.ErrorCode.RateLimitExceeded), limitedLine.GetProperty("code").GetString());
        Assert.Equal(LogEvents.ValidationRejected, largeLine.GetProperty("event").GetString());
        Assert.Equal(nameof(Shopfolio.Infrastructure.Errors.ErrorCode.PayloadTooLarge), largeLine.GetProperty("code").GetString());
        Assert.False(limitedLine.TryGetProperty("fields", out _));
        Assert.False(largeLine.TryGetProperty("fields", out _));
        Assert.Equal(typeof(Shopfolio.Infrastructure.Errors.RejectionLog).FullName, limitedLine.GetProperty("category").GetString());
    }

    private static async Task<HttpResponseMessage> SendWithIdAsync(HttpClient client, HttpMethod method, string path, string requestId, string peer, HttpContent? content = null)
    {
        using HttpRequestMessage request = new(method, path) { Content = content };
        request.Headers.Add(PeerHeader, peer);
        request.Headers.Add("X-Request-Id", requestId);
        return await client.SendAsync(request, Ct);
    }

    // The lines of one request in the application log file (the writer keeps its file open, so the reader allows its access).
    private static List<JsonElement> LinesOf(string logDirectory, string requestId)
    {
        List<JsonElement> lines = [];
        foreach (string path in Directory.GetFiles(logDirectory, "app-*.jsonl"))
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

    // --- KK4: the notification endpoint ---

    [Fact]
    public async Task ANotificationWithARightSignature_IsOutsideTheLimit_AndBadSignaturesCountTowardsTheFormLimitOfTheirAddress()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock(), forms: 3);
        using HttpClient client = ClientOf(factory);
        const string peer = "198.51.100.20";
        string path = AntiforgeryExemptions.PaymentNotificationPath;

        for (int i = 0; i < 8; i++)
        {
            using HttpResponseMessage good = await SendAsync(client, HttpMethod.Post, path, Notice(DatabaseTestEndpoint.Body()), peer);
            Assert.Equal(HttpStatusCode.OK, good.StatusCode);
        }

        using HttpResponseMessage formBefore = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), peer);
        Assert.Equal(HttpStatusCode.OK, formBefore.StatusCode);

        for (int i = 0; i < 3; i++)
        {
            using HttpResponseMessage bad = await SendAsync(client, HttpMethod.Post, path, Notice(DatabaseTestEndpoint.Body(signature: "bad")), peer);
            Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        }

        using HttpResponseMessage formAfter = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), peer);
        using HttpResponseMessage goodAfter = await SendAsync(client, HttpMethod.Post, path, Notice(DatabaseTestEndpoint.Body()), peer);
        using HttpResponseMessage otherAddress = await SendAsync(client, HttpMethod.Post, GeneralPath, new StringContent("x"), "198.51.100.21");

        Assert.Equal(HttpStatusCode.TooManyRequests, formAfter.StatusCode);
        Assert.Equal(HttpStatusCode.OK, goodAfter.StatusCode);
        Assert.Equal(HttpStatusCode.OK, otherAddress.StatusCode);
    }

    [Fact]
    public async Task ABadSignature_IsAnsweredByTheEndpointEvenWhenTheLimitIsReached()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock(), forms: 1);
        using HttpClient client = ClientOf(factory);
        string path = AntiforgeryExemptions.PaymentNotificationPath;

        for (int i = 0; i < 4; i++)
        {
            using HttpResponseMessage bad = await SendAsync(client, HttpMethod.Post, path, Notice(DatabaseTestEndpoint.Body(signature: "bad")), "198.51.100.30");
            Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        }
    }

    private static FormUrlEncodedContent Notice(string body)
        => new(body.Split('&').Select(pair => pair.Split('=', 2)).Select(p => new KeyValuePair<string, string>(p[0], Uri.UnescapeDataString(p[1]))));

    // --- KK3, D-112: the forwarded headers ---

    [Theory]
    [InlineData("172.30.1.5", "203.0.113.50", "https", "https|True|203.0.113.50")]
    [InlineData("172.30.1.5", "2001:db8:7:7:1:2:3:4", "https", "https|True|2001:db8:7:7::/64")]
    [InlineData("172.30.1.5", null, null, "https|True|172.30.1.5")]
    [InlineData("fd7a:5f0c:9e11:1::2", "203.0.113.50", "https", "https|True|203.0.113.50")]
    [InlineData("127.0.0.1", "203.0.113.50", "https", "https|True|127.0.0.1")]
    [InlineData("203.0.113.9", "198.51.100.1", "https", "https|True|203.0.113.9")]
    [InlineData("172.30.2.5", "203.0.113.50", "https", "https|True|172.30.2.5")]
    public async Task TheForwardedAddressAndScheme_AreBelievedFromTheProxysNetworkOnly(string peer, string? forwardedFor, string? forwardedProto, string expected)
    {
        // The test client talks https, so the scheme the request keeps is https either way; the next test uses an http client to show the header's own effect.
        await using WebApplicationFactory<Program> factory = Open(Clock());
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await SendAsync(client, HttpMethod.Get, ProbePath, peer: peer, forwardedFor: forwardedFor, forwardedProto: forwardedProto);

        Assert.Equal(expected, await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task OnAPlainHttpConnection_TheProtoHeaderOfTheProxyMakesTheRequestHttps_AndNoHeaderOrAForeignOneChangesNothing()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock());
        using HttpClient client = ClientOf(factory, "http://localhost");

        using HttpResponseMessage fromProxy = await SendAsync(client, HttpMethod.Get, ProbePath, peer: "172.30.1.5", forwardedFor: "203.0.113.50", forwardedProto: "https");
        using HttpResponseMessage noHeader = await SendAsync(client, HttpMethod.Get, ProbePath, peer: "172.30.1.5");
        using HttpResponseMessage foreign = await SendAsync(client, HttpMethod.Get, ProbePath, peer: "203.0.113.9", forwardedFor: "198.51.100.1", forwardedProto: "https");
        using HttpResponseMessage loopback = await SendAsync(client, HttpMethod.Get, ProbePath, peer: "127.0.0.1", forwardedProto: "https");

        Assert.Equal("https|True|203.0.113.50", await fromProxy.Content.ReadAsStringAsync(Ct));
        Assert.Equal("http|False|172.30.1.5", await noHeader.Content.ReadAsStringAsync(Ct));
        Assert.Equal("http|False|203.0.113.9", await foreign.Content.ReadAsStringAsync(Ct));
        Assert.Equal("http|False|127.0.0.1", await loopback.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task TheAntiforgeryCookie_IsWrittenSecure_WhenTheProxySaysHttps_OnAPlainHttpConnection()
    {
        await using WebApplicationFactory<Program> factory = Open(Clock());
        using HttpClient client = ClientOf(factory, "http://localhost");

        using HttpResponseMessage viaProxy = await SendAsync(client, HttpMethod.Get, ForgeryTokenHandler.TokenPath, peer: "172.30.1.5", forwardedFor: "203.0.113.50", forwardedProto: "https");

        string cookie = Assert.Single(viaProxy.Headers.GetValues("Set-Cookie"), c => c.StartsWith(CookieNames.Antiforgery + "=", StringComparison.Ordinal));
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Counts the calls of the test endpoints that ran (a refused request never reaches one).</summary>
    public sealed class EndpointCalls
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public void Add() => Interlocked.Increment(ref _count);
    }

    /// <summary>
    /// Before the application's pipeline: the peer address a test names in a header becomes the connection's address (a test server has none). After it: the
    /// test endpoints - two body classes, a machine path, and a probe that tells the scheme and the axis the request ended up with.
    /// </summary>
    private sealed class LimitsStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            ArgumentNullException.ThrowIfNull(next);
            return app =>
            {
                app.Use((context, forward) =>
                {
                    if (context.Request.Headers.TryGetValue(PeerHeader, out Microsoft.Extensions.Primitives.StringValues peer))
                    {
                        context.Connection.RemoteIpAddress = IPAddress.Parse(peer.ToString());
                    }

                    return forward(context);
                });
                next(app);
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapPost(GeneralPath, (Delegate)ReadBodyAsync).WithMetadata(new RequestBodyClassAttribute(RequestBodyClass.General));
                    endpoints.MapPost(LongTextPath, (Delegate)ReadBodyAsync).WithMetadata(new RequestBodyClassAttribute(RequestBodyClass.LongText));
                    endpoints.MapPost(MachinePath, (Delegate)ReadBodyAsync);
                    endpoints.MapGet(ProbePath, (HttpContext context) => $"{context.Request.Scheme}|{context.Request.IsHttps}|{ClientAddress.AxisOf(context)}");
                    endpoints.MapSignedEndpoint<DatabaseTestEndpoint, DatabaseTestNotice>(AntiforgeryExemptions.PaymentNotificationPath);
                });
            };
        }

        private static async Task<IResult> ReadBodyAsync(HttpContext context)
        {
            context.RequestServices.GetRequiredService<EndpointCalls>().Add();
            using MemoryStream copy = new();
            await context.Request.Body.CopyToAsync(copy, context.RequestAborted);
            return Results.Text(copy.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
