using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1): the forgery check's 400 on a contact request's screen shows the request of the route's key through the one hook (<see cref="IReadsOnForgeryRefusal"/>) and writes
/// nothing; a key that is no request's is the one 404, as the GET and both POSTs answer.
/// </summary>
public sealed partial class RequestsPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheRequestAndItsOneButton_WritesNothing_AndAForeignKeyIs404_D1843()
    {
        ContactRequest request = await AddAsync("Ayşe", "ayse-d1843@example.com", ContactTopic.GeneralQuestion, "Kargo ne zaman gelir?", DateTimeOffset.UtcNow.AddHours(-1));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await ForgedPosts.SendAsync(client, $"{ListPath}/{request.Id:D}?handler=CloseContactRequest", []);
        (HttpStatusCode foreign, _) = await ForgedPosts.SendAsync(client, $"{ListPath}/{Guid.NewGuid():D}?handler=CloseContactRequest", []);

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Contains("Kargo ne zaman gelir?", html, StringComparison.Ordinal);
        Assert.Contains("ayse-d1843@example.com", html, StringComparison.Ordinal);
        Assert.Contains("handler=CloseContactRequest", html, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, foreign);
        Assert.Equal("Open", await StatusOfAsync(request.Id));
    }
}
