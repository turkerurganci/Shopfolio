using System.Net;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The path patterns of the record address and the two order roots on the framework's real routing (T206 criteria 1, 3, 5; KK1, KK3, KK5; 07 2.1.1,
/// 2.1.3, 2.1.7, 2.1.11). A small host with the test server answers each pattern with the route values it received, so the match, the letter case and
/// the trailing slash are the framework's own; no database and no page is needed. The pages of the order screens are T511's.
/// </summary>
public sealed class AddressRoutingTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        _app = builder.Build();
        _app.UseRouting();
        foreach (string prefix in AddressRules.RecordPrefixes)
        {
            _app.MapGet($"/{prefix}/{{slug}}", (string slug) => $"record:{prefix}:{slug}").WithName("record-" + prefix);
        }

        _app.MapGet(OrderRoutes.MemberRoot, (string orderNumber) => $"member:{orderNumber}").WithName("member");
        _app.MapGet(OrderRoutes.AccessRoot, (string accessKey) => $"access:{accessKey}").WithName("access");
        foreach (string screen in OrderRoutes.Screens)
        {
            _app.MapGet(OrderRoutes.MemberScreen(screen), (string orderNumber) => $"member:{orderNumber}:{screen}");
            _app.MapGet(OrderRoutes.AccessScreen(screen), (string accessKey) => $"access:{accessKey}:{screen}");
        }

        await _app.StartAsync(TestContext.Current.CancellationToken);
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<(HttpStatusCode Status, string Body, bool Redirected)> GetAsync(string path)
    {
        using HttpResponseMessage response = await _client.GetAsync(path, Ct);
        return (response.StatusCode, await response.Content.ReadAsStringAsync(Ct), response.Headers.Location is not null);
    }

    // --- The record address: /{prefix}/{slug}, KK1 ---

    [Theory]
    [InlineData("urun")]
    [InlineData("kategori")]
    [InlineData("hizmet")]
    [InlineData("referans")]
    [InlineData("sayfa")]
    public async Task RecordAddress_OfEachPrefix_AnswersThePageWithTheSlug(string prefix)
    {
        (HttpStatusCode status, string body, bool redirected) = await GetAsync(AddressRules.RecordAddress(prefix, "ahsap-masa"));

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal($"record:{prefix}:ahsap-masa", body);
        Assert.False(redirected);
    }

    [Theory]
    [InlineData("/ahsap-masa")]
    [InlineData("/product/ahsap-masa")]
    [InlineData("/urun")]
    [InlineData("/urun/ahsap-masa/detay")]
    [InlineData("/tr/urun/ahsap-masa")]
    public async Task RecordAddress_WithoutPrefixOrWithAnotherShape_Is404(string path)
    {
        (HttpStatusCode status, _, _) = await GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, status);
    }

    // --- Letter case and the trailing slash: the same page, no redirect, KK5 ---

    [Theory]
    [InlineData("/urun/ahsap-masa")]
    [InlineData("/URUN/ahsap-masa")]
    [InlineData("/Urun/ahsap-masa")]
    [InlineData("/urun/ahsap-masa/")]
    [InlineData("/URUN/ahsap-masa/")]
    public async Task Path_WithAnotherLetterCaseOrATrailingSlash_AnswersTheSamePageWithoutARedirect(string path)
    {
        (HttpStatusCode status, string body, bool redirected) = await GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal("record:urun:ahsap-masa", body);
        Assert.False(redirected);
    }

    [Fact]
    public async Task SlugInTheUpperCase_ArrivesAsTypedAndTheLookupFoldsIt()
    {
        (_, string body, _) = await GetAsync("/urun/AHSAP-MASA");

        Assert.Equal("record:urun:AHSAP-MASA", body);
        Assert.Equal("ahsap-masa", AddressRules.SlugForLookup(body["record:urun:".Length..]));
    }

    // --- The order roots: two patterns, /siparis/erisim is not an order number, KK3 ---

    [Theory]
    [InlineData("/siparis/2026-7K4M9P", "member:2026-7K4M9P")]
    [InlineData("/siparis/2026-7k4m9p", "member:2026-7k4m9p")]
    [InlineData("/SIPARIS/2026-7K4M9P/", "member:2026-7K4M9P")]
    public async Task MemberRoot_OnAnOrderNumber_Matches_WithoutRegardToLetterCase(string path, string expected)
    {
        (HttpStatusCode status, string body, _) = await GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(expected, body);
    }

    [Theory]
    [InlineData("/siparis/erisim")]
    [InlineData("/siparis/erisim/")]
    [InlineData("/siparis/ERISIM")]
    [InlineData("/siparis/abc")]
    [InlineData("/siparis/26-7K4M9P")]
    [InlineData("/siparis/2026-")]
    [InlineData("/siparis/2026-7K4 M9P")]
    [InlineData("/siparis")]
    public async Task MemberRoot_OnAnythingThatIsNotAnOrderNumber_Is404_AndErisimIsNeverANumber(string path)
    {
        (HttpStatusCode status, _, _) = await GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, status);
    }

    [Theory]
    [InlineData("/siparis/erisim/AbC_-9xyZ", "access:AbC_-9xyZ")]
    [InlineData("/siparis/erisim/abc_-9xyz", "access:abc_-9xyz")]
    [InlineData("/SIPARIS/ERISIM/AbC_-9xyZ/", "access:AbC_-9xyZ")]
    public async Task AccessRoot_KeepsTheLettersOfTheKeyAsTyped(string path, string expected)
    {
        (HttpStatusCode status, string body, _) = await GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(expected, body);
    }

    [Fact]
    public async Task AccessKey_WithAChangedLetterCase_IsAnotherValue()
    {
        (_, string original, _) = await GetAsync("/siparis/erisim/AbCdEf");
        (_, string changed, _) = await GetAsync("/siparis/erisim/abcdef");

        Assert.NotEqual(original, changed);
        Assert.False(string.Equals(original["access:".Length..], changed["access:".Length..], StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("/siparis/erisim/abc=")]
    [InlineData("/siparis/erisim/ab%20c")]
    [InlineData("/siparis/erisim/ab+c")]
    public async Task AccessRoot_OnACharacterThatIsNotBase64Url_Is404(string path)
    {
        (HttpStatusCode status, _, _) = await GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, status);
    }

    [Fact]
    public async Task OrderScreens_SitUnderTheRootTheyWereReachedFrom()
    {
        foreach (string screen in OrderRoutes.Screens)
        {
            (HttpStatusCode memberStatus, string member, _) = await GetAsync($"/siparis/2026-7K4M9P/{screen}");
            (HttpStatusCode accessStatus, string access, _) = await GetAsync($"/siparis/erisim/AbCdEf/{screen}");

            Assert.Equal(HttpStatusCode.OK, memberStatus);
            Assert.Equal($"member:2026-7K4M9P:{screen}", member);
            Assert.Equal(HttpStatusCode.OK, accessStatus);
            Assert.Equal($"access:AbCdEf:{screen}", access);
        }
    }

    [Fact]
    public async Task OrderScreen_UnderTheAccessRootWithoutAKey_IsNotTheMembersScreen()
    {
        (HttpStatusCode status, _, _) = await GetAsync("/siparis/erisim/teyit");

        // The key 'teyit' is a valid key value: the access root answers it, and the member's root does not.
        Assert.Equal(HttpStatusCode.OK, status);
        (_, string body, _) = await GetAsync("/siparis/erisim/teyit");
        Assert.Equal("access:teyit", body);
    }

    [Fact]
    public async Task OrderScreen_OfAnotherName_Is404()
    {
        (HttpStatusCode member, _, _) = await GetAsync("/siparis/2026-7K4M9P/kargoya-ver");
        (HttpStatusCode access, _, _) = await GetAsync("/siparis/erisim/AbCdEf/kargoya-ver");

        Assert.Equal(HttpStatusCode.NotFound, member);
        Assert.Equal(HttpStatusCode.NotFound, access);
    }

    // --- The generated address keeps the lower case literals and the letters of a key ---

    [Fact]
    public void GeneratedPath_IsLowerCaseWithoutATrailingSlash_AndKeepsTheLettersOfTheKey()
    {
        LinkGenerator links = _app.Services.GetRequiredService<LinkGenerator>();

        string? access = links.GetPathByName("access", new RouteValueDictionary { ["accessKey"] = "AbC_-9xyZ" });
        string? member = links.GetPathByName("member", new RouteValueDictionary { ["orderNumber"] = "2026-7K4M9P" });
        string? record = links.GetPathByName("record-urun", new RouteValueDictionary { ["slug"] = "ahsap-masa" });

        Assert.Equal("/siparis/erisim/AbC_-9xyZ", access);
        Assert.Equal("/siparis/2026-7K4M9P", member);
        Assert.Equal("/urun/ahsap-masa", record);
    }
}
