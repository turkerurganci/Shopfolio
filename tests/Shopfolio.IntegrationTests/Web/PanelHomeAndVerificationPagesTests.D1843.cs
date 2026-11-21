using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1; 07 UC-108): the forgery check's 400 on the panel's two address-link landings shows the link's state through the one hook (<see cref="IReadsOnForgeryRefusal"/>) -
/// a read of the link's record that does not use it - and writes nothing: the link stays unused, the address stays.
/// </summary>
public sealed partial class PanelHomeAndVerificationPagesTests
{
    [Fact]
    public async Task TheForgeryRefusal_OfTheVerificationAndTheRevert_ShowsTheValidLink_AndConsumesNothing_D1843()
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        (string sessionKey, _) = await SeedSessionAsync(adminId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        string verification = await SeedVerificationAsync(keys, adminId, $"new-{Guid.NewGuid():N}@example.com", null, DateTimeOffset.UtcNow.AddHours(1));
        string revert = await SeedRevertAsync(keys, adminId, $"old-{Guid.NewGuid():N}@example.com", null);

        (HttpStatusCode verify, string verifyHtml) = await ForgedPosts.SendAsync(client, $"{VerifyPath}/{verification}?handler=VerifyEmail", []);
        (HttpStatusCode back, string backHtml) = await ForgedPosts.SendAsync(client, $"{RevertPath}/{revert}?handler=RevertPendingEmailChange", [], AdminCookie(sessionKey));

        Assert.Equal(HttpStatusCode.BadRequest, verify);
        Assert.Contains("id=\"verify-email-valid\"", verifyHtml, StringComparison.Ordinal);
        Assert.Contains($"{VerifyPath}/{verification}?handler=VerifyEmail", verifyHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.BadRequest, back);
        Assert.Contains("id=\"revert-valid\"", backHtml, StringComparison.Ordinal);
        Assert.Contains("id=\"verify-email-forgery-message\"", verifyHtml, StringComparison.Ordinal);
        Assert.Contains("id=\"revert-forgery-message\"", backHtml, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.False(await db.LinkRecords.AnyAsync(l => l.AdminId == adminId && l.UsedAt != null, Ct));
        Assert.Equal(email, await db.Admins.Where(a => a.Id == adminId).Select(a => a.Email).SingleAsync(Ct));
        Assert.True(await db.PendingEmailChanges.AnyAsync(p => p.AdminId == adminId, Ct));
    }
}
