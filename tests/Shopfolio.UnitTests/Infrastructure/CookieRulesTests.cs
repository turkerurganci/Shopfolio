using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The cookie rules without a server (T224 KK1, KK2, KK3, KK4; 05 6.7; 09 15.1 item 6): the closed name list, the one set of attributes, the
/// refusal of a name outside the list, the 400-day life of the guest cart cookie, the scope of the header filter and the gate that opens once.
/// </summary>
public sealed class CookieRulesTests
{
    public static TheoryData<string> Names => [.. CookieNames.All];

    private static BrowserCookies NewWriter() => new(new ProtectedValues(new EphemeralDataProtectionProvider()));

    [Fact]
    public void CookieNames_AreSevenDistinctNamesAllWithTheHostPrefix()
    {
        Assert.Equal(7, CookieNames.All.Count);
        Assert.Equal(CookieNames.All.Count, CookieNames.All.Distinct(StringComparer.Ordinal).Count());
        Assert.All(CookieNames.All, n => Assert.StartsWith(CookieNames.Prefix, n, StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(Names))]
    public void Create_ForEachName_GivesSecureHttpOnlyLaxRootPathAndNoDomain_EvenOverPlainHttp(string name)
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";

        CookieOptions options = CookieDefaults.Create(name).Build(context);

        Assert.True(options.Secure);
        Assert.True(options.HttpOnly);
        Assert.Equal(SameSiteMode.Lax, options.SameSite);
        Assert.Equal("/", options.Path);
        Assert.Null(options.Domain);
        Assert.Null(options.MaxAge);
        Assert.Null(options.Expires);
    }

    [Fact]
    public void Apply_OnABuilderTheFrameworkOwns_OverwritesWhatWouldBreakThePrefix()
    {
        var builder = new CookieBuilder { Domain = "example.com", Path = "/shop", SameSite = SameSiteMode.None, HttpOnly = false, SecurePolicy = CookieSecurePolicy.SameAsRequest };

        CookieDefaults.Apply(builder);

        Assert.Null(builder.Domain);
        Assert.Equal("/", builder.Path);
        Assert.Equal(SameSiteMode.Lax, builder.SameSite);
        Assert.True(builder.HttpOnly);
        Assert.Equal(CookieSecurePolicy.Always, builder.SecurePolicy);
    }

    [Theory]
    [InlineData("_ga")]
    [InlineData("session")]
    [InlineData("__Host-tracker")]
    [InlineData("__Host-checkoutC1")]
    [InlineData("")]
    public void ANameOutsideTheList_IsAProgrammingError_ForTheBuilderTheWriterTheReaderAndTheDeletion(string name)
    {
        var context = new DefaultHttpContext();

        Assert.Throws<ArgumentException>(() => CookieDefaults.Create(name));
        Assert.Throws<ArgumentException>(() => BrowserCookies.Write(context, name, "value"));
        Assert.Throws<ArgumentException>(() => BrowserCookies.Read(context, name));
        Assert.Throws<ArgumentException>(() => BrowserCookies.Delete(context, name));
        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public void GuestCartCookie_Lives400Days_TheLifeOfTheServerSideCartThresholdToo()
    {
        var context = new DefaultHttpContext();

        BrowserCookies.WriteGuestCart(context, "raw-key");

        Assert.Equal(TimeSpan.FromDays(400), BrowserCookies.GuestCartLifetime);
        Assert.Equal(TimeSpan.FromDays(DisposalTimings.GuestCartIdleDays), BrowserCookies.GuestCartLifetime);
        string header = context.Response.Headers.SetCookie.ToString();
        Assert.Contains("max-age=34560000", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/", header, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("domain", header, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Checkout_WrittenWithTheCookieWriter_IsNotReadableAsPlainTextAndRoundTrips()
    {
        BrowserCookies writer = NewWriter();
        var context = new DefaultHttpContext();

        writer.WriteCheckout(context, "buyer@example.com");

        string header = context.Response.Headers.SetCookie.ToString();
        Assert.DoesNotContain("buyer", header, StringComparison.Ordinal);
        Assert.DoesNotContain("max-age", header, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("expires", header, StringComparison.OrdinalIgnoreCase);

        var next = new DefaultHttpContext();
        next.Request.Headers.Cookie = header.Split(';')[0];
        ProtectedValueResult read = writer.ReadCheckout(next);
        Assert.True(read.IsValid);
        Assert.Equal("buyer@example.com", read.Payload);
        Assert.False(writer.ReadCheckout(new DefaultHttpContext()).IsValid);
    }

    [Theory]
    [InlineData("/", true)]
    [InlineData("/sepet", true)]
    [InlineData("/odeme/adres", true)]
    [InlineData("/hesap", true)]
    [InlineData("/panelim", true)]
    [InlineData("/panel", false)]
    [InlineData("/panel/", false)]
    [InlineData("/panel/giris", false)]
    [InlineData("/PANEL/Siparisler", false)]
    [InlineData("/sistem/canlilik", false)]
    [InlineData("/sistem/durum", false)]
    [InlineData("/sistemler", true)]
    public void HeaderFilter_AppliesToTheStorefrontOnly(string path, bool applies)
    {
        Assert.Equal(applies, CookieHeaderFilterMiddleware.AppliesTo(new PathString(path)));
    }

    [Fact]
    public async Task Gate_OnceItHasSeenThePublication_StaysOpenAndAsksNoMore()
    {
        var gate = new CookiePolicyGate();
        var publication = new CountingPublication { Published = false };

        Assert.False(await gate.IsOpenAsync(publication, TestContext.Current.CancellationToken));
        Assert.False(await gate.IsOpenAsync(publication, TestContext.Current.CancellationToken));
        Assert.Equal(2, publication.Asked);

        publication.Published = true;
        Assert.True(await gate.IsOpenAsync(publication, TestContext.Current.CancellationToken));

        publication.Published = false;
        Assert.True(await gate.IsOpenAsync(publication, TestContext.Current.CancellationToken));
        Assert.True(await gate.IsOpenAsync(publication, TestContext.Current.CancellationToken));
        Assert.Equal(3, publication.Asked);
    }

    [Fact]
    public async Task HeaderFilter_BeforeThePublication_RemovesWhatAnyMiddlewareWroteForTheStorefront_AndLeavesThePanel()
    {
        var gate = new CookiePolicyGate();
        var publication = new CountingPublication { Published = false };

        string storefront = await SetCookieAfterAsync(gate, publication, "/sepet");
        string panel = await SetCookieAfterAsync(gate, publication, "/panel/giris");
        publication.Published = true;
        string opened = await SetCookieAfterAsync(gate, publication, "/sepet");

        Assert.Equal(string.Empty, storefront);
        Assert.Contains("__Host-cart=x", panel, StringComparison.Ordinal);
        Assert.Contains("__Host-cart=x", opened, StringComparison.Ordinal);
    }

    private static async Task<string> SetCookieAfterAsync(CookiePolicyGate gate, ICookiePolicyPublication publication, string path)
    {
        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(new CallbackResponseFeature());
        context.Request.Path = path;
        var middleware = new CookieHeaderFilterMiddleware(
            ctx =>
            {
                ctx.Response.Cookies.Append(CookieNames.Cart, "x");
                return ((CallbackResponseFeature)ctx.Features.Get<IHttpResponseFeature>()!).RunAsync();
            },
            gate);

        await middleware.InvokeAsync(context, publication);

        return context.Response.Headers.SetCookie.ToString();
    }

    /// <summary>The default response feature of a bare context never runs its OnStarting callbacks; this one runs them, last registered first, as the server does.</summary>
    private sealed class CallbackResponseFeature : HttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> _callbacks = [];

        public override void OnStarting(Func<object, Task> callback, object state) => _callbacks.Add((callback, state));

        public async Task RunAsync()
        {
            for (int i = _callbacks.Count - 1; i >= 0; i--)
            {
                await _callbacks[i].Callback(_callbacks[i].State);
            }
        }
    }

    private sealed class CountingPublication : ICookiePolicyPublication
    {
        public bool Published { get; set; }

        public int Asked { get; private set; }

        public Task<bool> IsPublishedAsync(CancellationToken cancellationToken)
        {
            Asked++;
            return Task.FromResult(Published);
        }
    }
}
