using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 D-1773 (07 2.5.4; 04 2.7.3.1; K-1627 (2)): the page shown again by the forgery check's 400 reads what its GET reads through the one hook (<see cref="IReadsOnForgeryRefusal"/>), and nothing is written.</summary>
public sealed partial class CategoriesPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheTreeWithItsNodes_AndTheMessage_AndWritesNothing_D1773()
    {
        await CreateAsync("Zeytin", null);
        await CreateAsync("Ayakkabı", null);
        long before = await CategoryCountAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpRequestMessage request = new(HttpMethod.Post, $"{Path}?handler=CreateCategory");
        request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        request.Headers.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={_key}");
        request.Content = new FormUrlEncodedContent([new("Input.Name", "Kasnak")]);

        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Zeytin", html, StringComparison.Ordinal);
        Assert.Contains("Ayakkabı", html, StringComparison.Ordinal);
        Assert.DoesNotContain(CategoriesModel.EmptySentence, html, StringComparison.Ordinal);

        // D-1844 (04 2.7.3.1): the action-level message is on the page, once, in place.
        Assert.Contains("id=\"categories-action-message\"", html, StringComparison.Ordinal);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, System.Text.RegularExpressions.Regex.Escape(ForgeryProtectionFilter.Message)));
        Assert.Equal(before, await CategoryCountAsync());
    }
}
