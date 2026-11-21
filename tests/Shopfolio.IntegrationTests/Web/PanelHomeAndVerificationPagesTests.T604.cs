using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 (09 8.2 item 3; 05 6.1.5): the panel's link pages write their refusals' rejection lines (D-1317, UC-186, UC-187), and a panel page request that does not show the permission's form
/// drops the reauthentication permission while a storefront request leaves it (D-1389, D-1454).
/// </summary>
public sealed partial class PanelHomeAndVerificationPagesTests
{
    [Fact]
    public async Task ThePanelLinkPagesRefusals_WriteTheirRejectionLines_TakenAddress422_AndLinkNoLongerValid200_D1317()
    {
        (_, Guid adminId) = await SeedAdminAsync();
        (string otherEmail, _) = await SeedAdminAsync();
        (string sessionKey, _) = await SeedSessionAsync(adminId);
        CollectingLoggerProvider logs = new();
        await using WebApplicationFactory<Program> factory = Open().Collecting(logs);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        string taken = await SeedVerificationAsync(keys, adminId, otherEmail, null, DateTimeOffset.UtcNow.AddHours(1));
        string spent = await SeedVerificationAsync(keys, adminId, otherEmail, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));
        string revertTaken = await SeedRevertAsync(keys, adminId, otherEmail, null);

        (HttpStatusCode verifyTaken, _) = await PostBodyAsync(client, $"{VerifyPath}/{taken}?handler=VerifyEmail", []);
        (HttpStatusCode verifySpent, _) = await PostBodyAsync(client, $"{VerifyPath}/{spent}?handler=VerifyEmail", []);
        (HttpStatusCode revert, _) = await PostBodyAsync(client, $"{RevertPath}/{revertTaken}?handler=RevertPendingEmailChange", [], AdminCookie(sessionKey));

        Assert.Equal([HttpStatusCode.UnprocessableEntity, HttpStatusCode.OK, HttpStatusCode.UnprocessableEntity], [verifyTaken, verifySpent, revert]);
        Assert.Equal(2, logs.Count(ErrorCode.EmailTakenByOtherAccount));
        Assert.Equal(1, logs.Count(ErrorCode.LinkNoLongerValid));
    }

    [Fact]
    public async Task APanelPageRequest_DropsTheHeldPermission_AndAStorefrontRequestLeavesIt_D1389_D1454()
    {
        (_, Guid adminId) = await SeedAdminAsync();
        (string sessionKey, Guid sessionId) = await SeedSessionAsync(adminId, ReauthenticationOperation.EmailChange);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode storefront, _, _) = await GetAsync(client, FramePaths.Home, AdminCookie(sessionKey));
        ReauthenticationOperation? afterStorefront = await HeldAsync(sessionId);
        (HttpStatusCode home, _, _) = await GetAsync(client, HomePath, AdminCookie(sessionKey));

        Assert.Equal(HttpStatusCode.OK, storefront);
        Assert.Equal(ReauthenticationOperation.EmailChange, afterStorefront);
        Assert.Equal(HttpStatusCode.OK, home);
        Assert.Null(await HeldAsync(sessionId));
        await using AppDbContext db = NewContext();
        Assert.Null(await db.Sessions.Where(s => s.Id == sessionId).Select(s => s.ReauthenticationTokenHash).SingleAsync(Ct));
    }

    private async Task<ReauthenticationOperation?> HeldAsync(Guid sessionId)
    {
        await using AppDbContext db = NewContext();
        return await db.Sessions.Where(s => s.Id == sessionId).Select(s => s.ReauthenticatedOperation).SingleAsync(Ct);
    }
}
