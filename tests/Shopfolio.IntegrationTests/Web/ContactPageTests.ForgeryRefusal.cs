using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1; K-1627 (2)): the contact page the forgery check's 400 shows again reads the identity block, the branches and the form's gate in the page's own wrapper and keeps the
/// sent values - the trap field empty - and nothing is sent, written or counted.
/// </summary>
public sealed partial class ContactPageTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TheForgeryRefusal_ShowsTheIdentityTheBranchesAndTheGate_KeepsTheSentValues_AndWritesNothing_D1843(bool open)
    {
        await SeedFirmAsync(open);
        await SeedBranchAsync("Merkez şube", 10);
        await using WebApplicationFactory<Program> factory = OpenForPost();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpRequestMessage request = new(HttpMethod.Post, "/iletisim?handler=SendContactRequest") { Content = new FormUrlEncodedContent(Fields(website: "tuzak")) };
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Örnek Ticaret", html, StringComparison.Ordinal);
        Assert.Contains("Merkez şube", html, StringComparison.Ordinal);
        Assert.Equal(!open, html.Contains("Bu form şu anda kullanıma kapalı.", StringComparison.Ordinal));
        Assert.Equal(open, html.Contains("value=\"Ayşe Yılmaz\"", StringComparison.Ordinal));
        Assert.DoesNotContain("tuzak", html, StringComparison.Ordinal);
        Assert.Contains("id=\"contact-action-message\"", html, StringComparison.Ordinal);
        Assert.Equal(0L, await CountAsync("contact_requests"));
        Assert.Equal(0L, await CountAsync("attempt_records"));
        Assert.Equal(0L, await CountAsync("dispatch_records"));
    }
}
