using System.Net;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Security;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.RateLimiting;

/// <summary>
/// The general limit's chain and what it counts (T215 KK3, KK4, KK5; 07 2.8.2 to 2.8.4; 09 15.4 items 1 to 4; K-941, K-947): the page limit and the form
/// submission limit per client address axis, the exempt areas, the chunked upload under the page limit only, the notification endpoint outside both unless
/// its signature failed. The window moves with the fake clock.
/// </summary>
public sealed class RequestRateLimitingTests : IDisposable
{
    private static readonly DateTimeOffset Start = new(2026, 11, 6, 9, 0, 0, TimeSpan.Zero);

    private readonly ManualTimeProvider _time = new(Start);
    private readonly RateLimitOptions _settings = new();
    private readonly ServiceProvider _services;
    private readonly RateLimiterOptions _options = new();

    public RequestRateLimitingTests()
    {
        RequestRateLimiting.Configure(_options, _settings, _time);
        _services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IOptions<RateLimiterOptions>>(Options.Create(_options))
            .BuildServiceProvider();
    }

    public void Dispose() => _services.Dispose();

    private DefaultHttpContext Request(string method, string path, string address = "192.0.2.10")
    {
        DefaultHttpContext context = new() { RequestServices = _services };
        context.Request.Method = method;
        context.Request.Path = path;
        context.Connection.RemoteIpAddress = IPAddress.Parse(address);
        return context;
    }

    private bool Allowed(string method, string path, string address = "192.0.2.10")
    {
        using RateLimitLease lease = _options.GlobalLimiter!.AttemptAcquire(Request(method, path, address));
        return lease.IsAcquired;
    }

    private void Fill(string method, string path, int count, string address = "192.0.2.10")
    {
        for (int i = 0; i < count; i++)
        {
            Assert.True(Allowed(method, path, address), $"request {i + 1} of {count}");
        }
    }

    [Fact]
    public void TheDefaults_AreThreeHundredPagesAndThirtyFormsAMinute()
    {
        Assert.Equal(300, _settings.PageRequestsPerMinute);
        Assert.Equal(30, _settings.FormSubmissionsPerMinute);
        Assert.Equal(StatusCodes.Status429TooManyRequests, _options.RejectionStatusCode);
    }

    [Fact]
    public void Pages_AreRefusedAfterThePageLimit_AndTheNextWindowOpensThemAgain()
    {
        Fill("GET", "/urunler", _settings.PageRequestsPerMinute);
        Assert.False(Allowed("GET", "/urunler"));

        _time.Advance(RateLimitDefaults.Window);

        Assert.True(Allowed("GET", "/urunler"));
    }

    [Fact]
    public void ASegmentLeavingTheWindow_ReleasesOnlyItsOwnRequests()
    {
        Fill("GET", "/", 100);
        _time.Advance(TimeSpan.FromSeconds(10));
        Fill("GET", "/", _settings.PageRequestsPerMinute - 100);
        Assert.False(Allowed("GET", "/"));

        // After sixty seconds from the first request only the first segment's hundred are out.
        _time.Advance(TimeSpan.FromSeconds(50));

        Fill("GET", "/", 100);
        Assert.False(Allowed("GET", "/"));
    }

    [Fact]
    public void Posts_AreRefusedAfterTheFormLimit_WhileAGetStillPasses()
    {
        Fill("POST", "/iletisim", _settings.FormSubmissionsPerMinute);

        Assert.False(Allowed("POST", "/iletisim"));
        Assert.True(Allowed("GET", "/iletisim"));
    }

    [Fact]
    public void APost_CountsInBothLimits()
    {
        Fill("POST", "/iletisim", _settings.FormSubmissionsPerMinute);
        Fill("GET", "/", _settings.PageRequestsPerMinute - _settings.FormSubmissionsPerMinute);

        Assert.False(Allowed("GET", "/"));
    }

    [Theory]
    [InlineData("/statik/css/site.css")]
    [InlineData("/gorsel/urun/abc.webp")]
    [InlineData("/STATIK/app.js")]
    public void StaticFilesAndImages_AreOutsideBothLimits(string path)
    {
        Fill("GET", "/", _settings.PageRequestsPerMinute);

        Fill("GET", path, 1000);
        Fill("POST", path, 1000);
        Assert.False(Allowed("GET", "/"));
    }

    [Theory]
    [InlineData("/statikx")]
    [InlineData("/gorselx/a")]
    [InlineData("/urunler/statik")]
    public void ThePathComparison_IsOnWholeSegments_SoALookAlikeIsNotExempt(string path)
    {
        Fill("GET", path, _settings.PageRequestsPerMinute);

        Assert.False(Allowed("GET", path));
    }

    [Fact]
    public void TheChunkedUpload_IsUnderThePageLimitOnly()
    {
        const int chunks = 100;
        Fill("POST", "/panel/yukleme/parca", chunks);

        // No chunk counted as a form submission: the whole form limit is still there, and the chunks count in the page limit.
        Fill("POST", "/iletisim", _settings.FormSubmissionsPerMinute);
        Assert.False(Allowed("POST", "/iletisim"));
        // The refused form took its page permit before the form limiter refused it (the chain does not give it back): one more is used.
        Fill("GET", "/", _settings.PageRequestsPerMinute - chunks - _settings.FormSubmissionsPerMinute - 1);
        Assert.False(Allowed("GET", "/"));
        Assert.False(RateLimitScope.IsFormLimited(Request("POST", "/panel/yukleme/parca")));
        Assert.True(RateLimitScope.IsPageLimited(Request("POST", "/panel/yukleme/parca")));
    }

    [Theory]
    [InlineData("/sistem/canlilik")]
    [InlineData("/sistem/durum")]
    [InlineData("/sitemap.xml")]
    [InlineData("/robots.txt")]
    [InlineData("/indir/abc")]
    public void ProbesSitemapRobotsAndDownload_AreGetsUnderThePageLimitOnly(string path)
    {
        Fill("GET", path, _settings.PageRequestsPerMinute);

        Assert.False(Allowed("GET", path));
        Assert.False(RateLimitScope.IsFormLimited(Request("GET", path)));
        Assert.True(RateLimitScope.IsPageLimited(Request("GET", path)));
    }

    [Fact]
    public void TheNotificationEndpoint_IsOutsideBothLimits_WhenItsSignatureIsRight()
    {
        Fill("GET", "/", _settings.PageRequestsPerMinute);

        Fill("POST", AntiforgeryExemptions.PaymentNotificationPath, 500);

        Assert.False(RateLimitScope.IsPageLimited(Request("POST", AntiforgeryExemptions.PaymentNotificationPath)));
        Assert.False(RateLimitScope.IsFormLimited(Request("POST", AntiforgeryExemptions.PaymentNotificationPath)));
    }

    [Fact]
    public void AFailedSignature_CountsOnceTowardsTheFormLimit_OfTheSameAddressAndNotTheirPageLimit()
    {
        for (int i = 0; i < _settings.FormSubmissionsPerMinute; i++)
        {
            HttpContext failed = Request("POST", AntiforgeryExemptions.PaymentNotificationPath);
            RateLimitScope.CountSignatureFailure(failed);
            Assert.True(failed.Items.ContainsKey(RateLimitScope.SignatureFailureItemKey));
        }

        // The same address's next form is refused; its pages are not; another address is untouched.
        Assert.False(Allowed("POST", "/iletisim"));
        Assert.True(Allowed("GET", "/"));
        Assert.True(Allowed("POST", "/iletisim", "198.51.100.7"));
    }

    [Fact]
    public void ARefusedFailedSignature_AnswersTheEndpointNotTheLimit_AndALaterNotificationWithAGoodSignatureStaysFree()
    {
        for (int i = 0; i < _settings.FormSubmissionsPerMinute + 5; i++)
        {
            RateLimitScope.CountSignatureFailure(Request("POST", AntiforgeryExemptions.PaymentNotificationPath));
        }

        Assert.True(Allowed("POST", AntiforgeryExemptions.PaymentNotificationPath));
        Assert.False(Allowed("POST", "/iletisim"));
    }

    [Fact]
    public void AFailedSignatureMarkedRequest_IsFormLimitedEvenOnTheNotificationPath()
    {
        HttpContext context = Request("POST", AntiforgeryExemptions.PaymentNotificationPath);
        Assert.False(RateLimitScope.IsFormLimited(context));

        RateLimitScope.CountSignatureFailure(context);

        Assert.True(RateLimitScope.IsFormLimited(context));
    }

    [Fact]
    public void CountSignatureFailure_WithoutTheLimiterConfigured_OnlyMarksTheRequest()
    {
        DefaultHttpContext context = new() { RequestServices = new ServiceCollection().BuildServiceProvider() };

        RateLimitScope.CountSignatureFailure(context);

        Assert.True(context.Items.ContainsKey(RateLimitScope.SignatureFailureItemKey));
    }

    [Fact]
    public void TwoAddressesOfTheSameV6Prefix_ShareOneCounter_AndAnotherPrefixHasItsOwn()
    {
        Fill("GET", "/", _settings.PageRequestsPerMinute, "2001:db8:1:2:aaaa::1");

        Assert.False(Allowed("GET", "/", "2001:db8:1:2:bbbb::9"));
        Assert.True(Allowed("GET", "/", "2001:db8:1:3:aaaa::1"));
    }

    [Fact]
    public void ADifferentV4Address_HasItsOwnCounter()
    {
        Fill("GET", "/", _settings.PageRequestsPerMinute);

        Assert.False(Allowed("GET", "/"));
        Assert.True(Allowed("GET", "/", "192.0.2.11"));
    }

    [Fact]
    public void IsMachineEndpoint_IsTheProbesTheDownloadAndTheChunkedUpload()
    {
        Assert.True(RateLimitScope.IsMachineEndpoint(Request("GET", "/sistem/canlilik")));
        Assert.True(RateLimitScope.IsMachineEndpoint(Request("GET", "/sistem/durum")));
        Assert.True(RateLimitScope.IsMachineEndpoint(Request("GET", "/indir/x")));
        Assert.True(RateLimitScope.IsMachineEndpoint(Request("POST", "/panel/yukleme/parca")));
        Assert.False(RateLimitScope.IsMachineEndpoint(Request("GET", "/urunler")));
        Assert.False(RateLimitScope.IsMachineEndpoint(Request("POST", "/sistemx")));
    }

    [Fact]
    public async Task TheRejection_OfAPage_IsA429WithTheNeutralMessage_NoFormAndNoRetryAfter()
    {
        HttpContext context = Request("GET", "/urunler");
        MemoryStream body = new();
        context.Response.Body = body;

        await _options.OnRejected!(new OnRejectedContext { HttpContext = context, Lease = new Refused() }, CancellationToken.None);

        string html = System.Text.Encoding.UTF8.GetString(body.ToArray());
        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.False(context.Response.Headers.ContainsKey("Retry-After"));
        Assert.Equal("no-store", context.Response.Headers.CacheControl.ToString());
        Assert.Contains(System.Net.WebUtility.HtmlEncode(NeutralMessages.LimitExceeded), html, StringComparison.Ordinal);
        Assert.DoesNotContain("<form", html, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("GET", "/sistem/canlilik")]
    [InlineData("GET", "/indir/abc")]
    [InlineData("POST", "/panel/yukleme/parca")]
    public async Task TheRejection_OfAMachineEndpoint_IsA429WithNoBodyAndNoRetryAfter(string method, string path)
    {
        HttpContext context = Request(method, path);
        MemoryStream body = new();
        context.Response.Body = body;

        await _options.OnRejected!(new OnRejectedContext { HttpContext = context, Lease = new Refused() }, CancellationToken.None);

        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.Equal(0, body.Length);
        Assert.False(context.Response.Headers.ContainsKey("Retry-After"));
        Assert.Null(context.Response.ContentType);
    }

    private sealed class Refused : RateLimitLease
    {
        public override bool IsAcquired => false;

        public override IEnumerable<string> MetadataNames => [];

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            metadata = null;
            return false;
        }
    }
}
