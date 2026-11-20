using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 D-1474 (07 UC-67; 12 VAL-A441): asking the link again with no pending change is 422 and writes nothing - no L-9 attempt line either; with a pending change it counts (on both axes).</summary>
public sealed partial class AccountProfilePageTests
{
    [Fact]
    public async Task TheLinkAskedForAgain_WithNothingPending_WritesNoAttemptLine_AndWithAPendingChangeCountsOne_VALA441_D1474()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        int before = await AttemptLinesAsync();

        using HttpResponseMessage nothing = await PostAsync(client, FramePaths.Account, "ResendEmailChangeLink", [], session);
        int afterNothing = await AttemptLinesAsync();
        using (await ChangeEmailAsync(client, session, "yeni@example.com"))
        {
        }

        int afterChange = await AttemptLinesAsync();
        using HttpResponseMessage resent = await PostAsync(client, FramePaths.Account, "ResendEmailChangeLink", [], session);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, nothing.StatusCode);
        Assert.Equal(before, afterNothing);
        Assert.Equal(HttpStatusCode.SeeOther, resent.StatusCode);
        Assert.True(await AttemptLinesAsync() > afterChange);
    }

    private async Task<int> AttemptLinesAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.AttemptRecords.CountAsync(Ct);
    }
}
