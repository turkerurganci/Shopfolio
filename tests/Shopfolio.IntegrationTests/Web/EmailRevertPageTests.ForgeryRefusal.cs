using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 D-1843 (07 2.5.4; 04 2.7.3.1; K-1627 (2)): the page the forgery check's 400 shows again reads the link as the GET does, through the one hook, and nothing is reverted or consumed.</summary>
public sealed partial class EmailRevertPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheRevertStepWithItsButton_AndRevertsAndConsumesNothing_D1843()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Seeded seeded = await SeedAsync(factory);

        using HttpRequestMessage request = new(HttpMethod.Post, $"{Path(seeded.Key)}?handler=RevertPendingEmailChange");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        request.Content = new FormUrlEncodedContent([]);
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(EmailRevertLandings.RevertStep, html, StringComparison.Ordinal);
        Assert.Contains($"{FramePaths.EmailRevert}/{seeded.Key}?handler=RevertPendingEmailChange", html, StringComparison.Ordinal);
        Assert.DoesNotContain(EmailRevertLandings.Invalid, html, StringComparison.Ordinal);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, System.Text.RegularExpressions.Regex.Escape(ForgeryProtectionFilter.Message)));
        await using AppDbContext db = NewContext();
        Assert.Equal(Current, await db.Customers.Where(c => c.Id == seeded.CustomerId).Select(c => c.Email).SingleAsync(Ct));
        Assert.Null(await db.LinkRecords.Where(l => l.Id == seeded.LinkId).Select(l => l.UsedAt).SingleAsync(Ct));
    }
}
