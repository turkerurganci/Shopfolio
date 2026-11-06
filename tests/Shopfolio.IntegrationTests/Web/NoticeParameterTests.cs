using System.Net;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Notices;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The short-lived notice codes on the opened application and a test-only sample page (T214 criteria 1-4, 6; KK1-KK4, KK6; 07 2.2.3, 2.2.8, 2.6.6, 6.3; VAL-A553,
/// VAL-A557, VAL-A669): a successful POST is a 303 to the page with the code of the closed list in <c>notice</c>; the GET shows the text of a code of the asked
/// list and nothing for a value outside the list, without reflecting the value; the reload repeats the notice; and neither the POST nor the GET sets a cookie.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class NoticeParameterTests
{
    private readonly CollectionDatabaseFixture _database;

    public NoticeParameterTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(NoticeSampleModel).Assembly)));

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string notice) =>
        client.PostAsync(NoticeSampleModel.Path + "?handler=Save", new FormUrlEncodedContent([new KeyValuePair<string, string>("Notice", notice)]), Ct);

    private static Task<HttpResponseMessage> GetAsync(HttpClient client, string query) =>
        client.GetAsync(new Uri(NoticeSampleModel.Path + query, UriKind.Relative), Ct);

    public static TheoryData<NoticeCode> AllCodes => [.. Enum.GetValues<NoticeCode>()];

    [Theory]
    [MemberData(nameof(AllCodes))]
    public async Task ASuccessfulPost_Is303ToTheAddressWithTheCode_AndSetsNoCookie_KK3_KK6(NoticeCode code)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAsync(client, code.ToString());

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal($"{NoticeSampleModel.Path}?page=2&{NoticeParameter.ParameterName}={code}", response.Headers.Location?.OriginalString);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Theory]
    [MemberData(nameof(AllCodes))]
    public async Task TheGet_ShowsTheTextOfACode_InItsOwnListOnly_AndNeverTheName_KK1(NoticeCode code)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await GetAsync(client, $"?{NoticeParameter.ParameterName}={code}");
        string html = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string text = NoticeSampleModel.TextOf(code);
        foreach (NoticeKind kind in Enum.GetValues<NoticeKind>())
        {
            string slot = kind switch
            {
                NoticeKind.ShortLived => "short-lived",
                NoticeKind.Result => "result",
                NoticeKind.Reason => "reason",
                _ => throw new InvalidOperationException(),
            };
            Assert.Equal(NoticeKinds.Of(code) == kind, html.Contains($"<p class=\"{slot}\">{text}</p>", StringComparison.Ordinal));
        }

        Assert.DoesNotContain(code.ToString(), html, StringComparison.Ordinal);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Theory]
    [InlineData("Sepete%20eklendi")]
    [InlineData("saved")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("Saved,CartItemAdded")]
    [InlineData("%3Cscript%3Ealert(1)%3C%2Fscript%3E")]
    [InlineData("NotFound")]
    [InlineData("ayse%40example.com")]
    [InlineData("")]
    public async Task AValueOutsideTheList_IsIgnored_200_NoNotice_AndNotReflected_KK1_KK2(string value)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await GetAsync(client, $"?{NoticeParameter.ParameterName}={value}");
        string html = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("class=\"short-lived\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"result\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"reason\"", html, StringComparison.Ordinal);
        string decoded = Uri.UnescapeDataString(value);
        if (decoded.Length > 2)
        {
            Assert.DoesNotContain(decoded, WebUtility.HtmlDecode(html), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheNoticeRepeatsWhenThePageIsReloaded_AndTheCodeStaysInTheAddress_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage posted = await PostAsync(client, nameof(NoticeCode.Saved));
        string address = posted.Headers.Location!.OriginalString;

        string first = await (await client.GetAsync(new Uri(address, UriKind.Relative), Ct)).Content.ReadAsStringAsync(Ct);
        string reload = await (await client.GetAsync(new Uri(address, UriKind.Relative), Ct)).Content.ReadAsStringAsync(Ct);

        string expected = $"<p class=\"short-lived\">{NoticeSampleModel.TextOf(NoticeCode.Saved)}</p>";
        Assert.Contains(expected, first, StringComparison.Ordinal);
        Assert.Contains(expected, reload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePageWithoutANotice_ShowsNone()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        string html = await (await GetAsync(client, string.Empty)).Content.ReadAsStringAsync(Ct);

        Assert.DoesNotContain("class=\"short-lived\"", html, StringComparison.Ordinal);
    }
}
