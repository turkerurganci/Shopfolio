using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1; K-1627 (2)): the page the forgery check's 400 shows again reads what its GET reads - the landing page the link through the one hook, the registration page the gate
/// and the sent values in its own wrapper - and nothing is written or consumed.
/// </summary>
public sealed partial class RegistrationPageTests
{
    [Fact]
    public async Task TheLandingPagesForgeryRefusal_ShowsTheVerificationStepWithItsButton_AndVerifiesAndConsumesNothing_D1843()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (Guid customerId, string key) = await SeedLinkedCustomerAsync(factory, "ayse@example.com", LinkType.EmailVerification, verified: false);

        using HttpResponseMessage response = await PostWithoutTokenAsync(client, $"{FramePaths.Verification}/{key}", "VerifyEmail", []);
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("E-postamı doğrula", html, StringComparison.Ordinal);
        Assert.Contains($"{FramePaths.Verification}/{key}?handler=VerifyEmail", html, StringComparison.Ordinal);
        Assert.DoesNotContain(VerificationLandings.Unrecognized.Sentence, html, StringComparison.Ordinal);
        Assert.Contains("id=\"verification-action-message\"", html, StringComparison.Ordinal);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, System.Text.RegularExpressions.Regex.Escape(ForgeryProtectionFilter.Message)));
        await using AppDbContext db = NewContext();
        Assert.Null(await db.Customers.Where(c => c.Id == customerId).Select(c => c.EmailVerifiedAt).SingleAsync(Ct));
        Assert.Null(await db.LinkRecords.Select(l => l.UsedAt).SingleAsync(Ct));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TheRegistrationsForgeryRefusal_ReadsTheGate_KeepsTheSentValues_AndOpensNoAccount_D1843(bool gateOpen)
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostWithoutTokenAsync(client, FramePaths.Register, "Register", Form("kasnak@example.com"));
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(!gateOpen, html.Contains(SalesClosedMessages.DataEntryText, StringComparison.Ordinal));
        Assert.Equal(gateOpen, html.Contains("value=\"kasnak@example.com\"", StringComparison.Ordinal));
        Assert.DoesNotContain(Entered, html, StringComparison.Ordinal);
        Assert.Equal(gateOpen, html.Contains(ForgeryProtectionFilter.Message, StringComparison.Ordinal));
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Customers.CountAsync(Ct));
    }

    private static async Task<HttpResponseMessage> PostWithoutTokenAsync(HttpClient client, string path, string handler, IEnumerable<(string Name, string Value)> fields)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{path}?handler={handler}");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        request.Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        return await client.SendAsync(request, Ct);
    }
}
