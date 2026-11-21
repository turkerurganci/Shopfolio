using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1; 05 6.1.5): the forgery check's 400 on the account page and on the reauthentication page shows what the GET shows through the one hook
/// (<see cref="IReadsOnForgeryRefusal"/>). The single-use value is checked, never used; the permission follows the page's own rule (05 6.1.5): the account page shown with the operation's form
/// keeps it, shown without it drops it (as its GET and its 422), and the reauthentication screen's refused POST leaves the earlier one (its GET's drop is not made).
/// </summary>
public sealed partial class MyAccountPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_OfTheAccountPage_ShowsTheAccountAndTheOpenForm_UsesNoValue_AndChangesNothing_D1843()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, Guid sessionId, string held) = await SeedSessionAsync(adminId, ReauthenticationOperation.PasswordChange);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string before = await PasswordHashOfAsync(adminId);
        string email = await EmailOfAsync(adminId);

        (HttpStatusCode password, string passwordHtml) = await ForgedPosts.SendAsync(client, $"{AccountPath}?handler=ChangePassword", [("Input.NewPassword", "another-strong-password"), ("Input.ReauthenticationToken", held)], AdminCookie(key));
        string? permissionAfterFirst;
        await using (AppDbContext db = NewContext())
        {
            permissionAfterFirst = (await db.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct)).ReauthenticationTokenHash;
        }

        (HttpStatusCode name, string nameHtml) = await ForgedPosts.SendAsync(client, $"{AccountPath}?handler=ChangeName", [("Input.Name", "Yeni Ad")], AdminCookie(key));

        Assert.Equal(HttpStatusCode.BadRequest, password);
        Assert.Contains("id=\"change-password-form\"", passwordHtml, StringComparison.Ordinal);
        Assert.Contains(email, passwordHtml, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, passwordHtml, StringComparison.Ordinal);
        Assert.NotNull(permissionAfterFirst);
        Assert.Equal(HttpStatusCode.BadRequest, name);
        Assert.Contains("id=\"change-name-form\"", nameHtml, StringComparison.Ordinal);
        Assert.Contains(email, nameHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"change-password-form\"", nameHtml, StringComparison.Ordinal);
        Assert.Equal(before, await PasswordHashOfAsync(adminId));
        await using AppDbContext after = NewContext();
        Assert.Equal("Test yonetici", await after.Admins.Where(a => a.Id == adminId).Select(a => a.Name).SingleAsync(Ct));
        Assert.Null((await after.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct)).ReauthenticationTokenHash);
    }

    [Fact]
    public async Task TheForgeryRefusal_OfTheReauthentication_ShowsTheSentOperationsForm_AndKeepsTheEarlierPermission_D1843()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, Guid sessionId, _) = await SeedSessionAsync(adminId, ReauthenticationOperation.PasswordChange);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await ForgedPosts.SendAsync(client, $"{ReauthPath}?handler=Reauthenticate", [("Input.Password", Strong), ("Input.ReauthenticatedOperation", "EmailChange")], AdminCookie(key));

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Contains("name=\"Input.ReauthenticatedOperation\" value=\"EmailChange\"", html, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Session session = await db.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct);
        Assert.NotNull(session.ReauthenticationTokenHash);
        Assert.Equal(ReauthenticationOperation.PasswordChange, session.ReauthenticatedOperation);
    }
}
