using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1; K-1627 (2)): the sign-in page the forgery check's 400 shows again takes the sent address and the local return path in the page's own wrapper - its GET reads the
/// database only for a resent link's address - and no session is opened and no attempt counted.
/// </summary>
public sealed partial class CustomerSignInPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_KeepsTheSentAddressAndTheReturnPath_NotThePassword_AndOpensNoSessionAndCountsNothing_D1843()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedCustomerAsync("ali@example.com", verified: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpRequestMessage request = new(HttpMethod.Post, $"{FramePaths.SignIn}?handler=SignIn");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        request.Content = new FormUrlEncodedContent([new("Input.Email", "ali@example.com"), new("Input.Password", Entered), new("Input.ReturnUrl", FramePaths.Cart)]);
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("value=\"ali@example.com\"", html, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.ReturnUrl\" value=\"{FramePaths.Cart}\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain(Entered, html, StringComparison.Ordinal);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, System.Text.RegularExpressions.Regex.Escape(ForgeryProtectionFilter.Message)));
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
        Assert.Equal(0, await db.AttemptRecords.CountAsync(Ct));
    }
}
