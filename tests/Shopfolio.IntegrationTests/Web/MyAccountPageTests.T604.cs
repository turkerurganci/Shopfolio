using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 (02 3.13.11; K-786; 07 UC-67 and its panel twin): the administrator's address request answers a session without the permission with the 303 to the reauthentication before it tells
/// whether the address is another administrator's (D-1473), and asking the link again with nothing pending writes no attempt line (D-1474).
/// </summary>
public sealed partial class MyAccountPageTests
{
    [Fact]
    public async Task ATakenAddress_WithoutTheValue_Is303ToTheReauthentication_NotThe422OfTheTakenAddress_AndCountsNoAttempt_D1473()
    {
        Guid adminId = await SeedAdminAsync();
        string otherEmail = await EmailOfAsync(await SeedAdminAsync());
        (string key, _, _) = await SeedSessionAsync(adminId, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode none, string? noneLocation, _) = await PostAsync(client, $"{AccountPath}?handler=ChangeEmail", [("Input.NewEmail", otherEmail)], key);
        (HttpStatusCode wrong, string? wrongLocation, _) = await PostAsync(client, $"{AccountPath}?handler=ChangeEmail", [("Input.NewEmail", otherEmail), ("Input.ReauthenticationToken", "wrong")], key);

        Assert.Equal(HttpStatusCode.SeeOther, none);
        Assert.StartsWith(ReauthPath + "?reauthenticatedOperation=EmailChange", noneLocation, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, wrong);
        Assert.StartsWith(ReauthPath + "?reauthenticatedOperation=EmailChange", wrongLocation, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.AttemptRecords.CountAsync(Ct));
        Assert.False(await db.PendingEmailChanges.AnyAsync(p => p.AdminId == adminId, Ct));
    }

    [Fact]
    public async Task AskingTheLinkAgain_WithNothingPending_Is422_AndWritesNoAttemptLine_D1474()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, _, _) = await SeedSessionAsync(adminId, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode nothing, _) = await PostBodyAsync(client, $"{AccountPath}?handler=ResendEmailChangeLink", [], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, nothing);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.AttemptRecords.CountAsync(Ct));
    }
}
