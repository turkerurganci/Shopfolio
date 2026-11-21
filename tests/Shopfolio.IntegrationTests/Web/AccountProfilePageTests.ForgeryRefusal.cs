using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1; K-1627 (2)): the account page the forgery check's 400 shows again reads the account in the page's own wrapper, keeps the sent values and the open form with the
/// value the refusal did not spend, and nothing is written - the permission is neither spent nor dropped.
/// </summary>
public sealed partial class AccountProfilePageTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheAccount_TheSentAddressAndTheOpenFormWithItsValue_AndSpendsAndWritesNothing_D1843()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        string permit = TokenOf(await ReauthenticateAsync(client, session, "EmailChange"));

        using HttpRequestMessage request = new(HttpMethod.Post, $"{FramePaths.Account}?handler=ChangeEmail");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        request.Headers.TryAddWithoutValidation("Cookie", session);
        request.Content = new FormUrlEncodedContent([new("Input.NewEmail", "kasnak@example.com"), new("Input.ReauthenticationToken", permit)]);
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("ali@example.com", html, StringComparison.Ordinal);
        Assert.Contains("value=\"kasnak@example.com\"", html, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.ReauthenticationToken\" type=\"hidden\" value=\"{permit}\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"account-action-message\"", html, StringComparison.Ordinal);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, System.Text.RegularExpressions.Regex.Escape(ForgeryProtectionFilter.Message)));
        Assert.NotNull((await ReadSessionAsync("ali@example.com")).ReauthenticationTokenHash);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.PendingEmailChanges.CountAsync(Ct));
        Assert.Equal("ali@example.com", await db.Customers.Select(c => c.Email).SingleAsync(Ct));
    }
}
