using Microsoft.AspNetCore.Http;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The recognised-browser mark's cookie (T377 criterion 8, KK8; 02 8.2.6, 12.2.5; 06 4.41): a required first-party cookie with the one set of attributes and no domain, a life of the
/// mark's 90 days, a random value that is not an e-mail address or a password, and a read that gives back exactly what was written.
/// </summary>
public sealed class KnownDeviceCookieTests
{
    private static readonly TimeSpan Life = TimeSpan.FromDays(ProductConstants.RecognizedBrowserMarkDays);

    private static string Header(HttpContext context) => context.Response.Headers.SetCookie.ToString();

    [Fact]
    public void WriteKnownDevice_SetsTheHostPrefixedSecureHttpOnlyLaxRootCookie_WithNoDomain_AndTheMarksLife_KK8()
    {
        var context = new DefaultHttpContext();

        BrowserCookies.WriteKnownDevice(context, "mark-value", Life);

        string header = Header(context);
        Assert.StartsWith(CookieNames.KnownDevice + "=", header, StringComparison.Ordinal);
        Assert.StartsWith(CookieNames.Prefix, CookieNames.KnownDevice, StringComparison.Ordinal);
        Assert.Contains("secure", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/", header, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("domain", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"max-age={(long)Life.TotalSeconds}", header, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheMarkValueTheCookieCarries_IsTheRandomOneTheStoreGave_NotAnEmailOrAPassword_KK8()
    {
        string first = SecretKey.Create();
        string second = SecretKey.Create();
        var context = new DefaultHttpContext();

        BrowserCookies.WriteKnownDevice(context, first, Life);

        Assert.NotEqual(first, second);
        string header = Header(context);
        Assert.DoesNotContain("@", header, StringComparison.Ordinal);
        Assert.DoesNotContain("example.com", header, StringComparison.Ordinal);
        Assert.Contains(first, header, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadKnownDevice_GivesBackTheWrittenValue_AndNullWhenTheCookieDidNotCome_KK8()
    {
        string value = SecretKey.Create();
        var writing = new DefaultHttpContext();
        BrowserCookies.WriteKnownDevice(writing, value, Life);

        var next = new DefaultHttpContext();
        next.Request.Headers.Cookie = Header(writing).Split(';')[0];

        Assert.Equal(value, BrowserCookies.ReadKnownDevice(next));
        Assert.Null(BrowserCookies.ReadKnownDevice(new DefaultHttpContext()));
    }

    [Fact]
    public void WriteKnownDevice_RefusesAnEmptyValue_KK8()
    {
        Assert.Throws<ArgumentException>(() => BrowserCookies.WriteKnownDevice(new DefaultHttpContext(), "", Life));
    }
}
