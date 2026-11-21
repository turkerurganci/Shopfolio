using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1): the forgery check's 400 on a content record's form shows the record of the route's type and key and the sent entries through the one hook
/// (<see cref="IReadsOnForgeryRefusal"/>) and writes nothing; a key that is no record of the type is the one 404, as the GET and the 422 answer.
/// </summary>
public sealed partial class ContentPanelPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheRecordsFormWithTheSentName_WritesNothing_AndAForeignKeyIs404_D1843()
    {
        Guid offering = await SeedOfferingAsync("Eski hizmet", 1);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await ForgedPosts.SendAsync(client, $"{ContentRoutes.FormPath(PanelContentType.Offering, offering)}?handler=SaveContent", [("Input.Name", "Sahte hizmet"), ("Input.Version", "1")]);
        (HttpStatusCode foreign, _) = await ForgedPosts.SendAsync(client, $"{ContentRoutes.FormPath(PanelContentType.Offering, Guid.NewGuid())}?handler=SaveContent", [("Input.Name", "Sahte hizmet")]);

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Contains("value=\"Sahte hizmet\"", html, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, foreign);
        Assert.Equal("Eski hizmet", await ScalarAsync<string>($"SELECT name FROM offerings WHERE id = '{offering}'"));
    }
}
