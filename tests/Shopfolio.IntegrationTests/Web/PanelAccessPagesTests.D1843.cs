using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1): the forgery check's 400 on the panel's sign-in, reset and invitation pages shows what the GET shows through the one hook (<see cref="IReadsOnForgeryRefusal"/>) -
/// the sent values, the link's step read without using the link - and writes nothing: no session, no consumed link, no account.
/// </summary>
public sealed partial class PanelAccessPagesTests
{
    [Fact]
    public async Task TheForgeryRefusal_OfTheSignIn_ShowsTheSentAddressAndOnlyALocalReturnAddress_AndOpensNoSession_D1843()
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode local, string localHtml) = await ForgedPosts.SendAsync(client, $"{SignInPath}?handler=SignIn", Credentials(email, Strong, ("Input.ReturnUrl", "/panel/siparisler?page=2")));
        (HttpStatusCode foreign, string foreignHtml) = await ForgedPosts.SendAsync(client, $"{SignInPath}?handler=SignIn", Credentials(email, Strong, ("Input.ReturnUrl", "https://evil.example")));

        Assert.Equal(HttpStatusCode.BadRequest, local);
        Assert.Contains($"value=\"{email}\"", localHtml, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.ReturnUrl\" value=\"/panel/siparisler?page=2\"", localHtml, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, localHtml, StringComparison.Ordinal);
        Assert.DoesNotContain($"value=\"{Strong}\"", localHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.BadRequest, foreign);
        Assert.Contains($"value=\"{email}\"", foreignHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("evil.example", foreignHtml, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.False(await db.Sessions.AnyAsync(s => s.AdminId == adminId, Ct));
    }

    [Fact]
    public async Task TheForgeryRefusal_OfTheResetAndTheInvitation_ShowsTheLinksStepAndTheSentValues_AndConsumesNoLink_D1843()
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        string reset = await SeedLinkAsync(keys, adminId, Now.AddMinutes(30), null, null);
        (string invitation, string invited) = await SeedInvitationAsync(keys, Now.AddHours(1), null);

        (HttpStatusCode request, string requestHtml) = await ForgedPosts.SendAsync(client, $"{ResetPath}?handler=RequestPasswordReset", [("Input.Email", email)]);
        (HttpStatusCode setting, string settingHtml) = await ForgedPosts.SendAsync(client, $"{ResetPath}/{reset}?handler=ResetPassword", [("Input.NewPassword", Replacement)]);
        (HttpStatusCode acceptance, string acceptanceHtml) = await ForgedPosts.SendAsync(client, $"{InvitePath}/{invitation}?handler=AcceptAdminInvitation", [("Input.Name", "Yeni Yönetici"), ("Input.Password", Strong)]);

        Assert.Equal(HttpStatusCode.BadRequest, request);
        Assert.Contains($"value=\"{email}\"", requestHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.BadRequest, setting);
        Assert.Contains("name=\"Input.NewPassword\"", settingHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(PanelAccessTexts.ResetLinkInvalid, settingHtml, StringComparison.Ordinal);
        Assert.Contains("id=\"reset-forgery-message\"", settingHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.BadRequest, acceptance);
        Assert.Contains($"value=\"{invited}\"", acceptanceHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"Yeni Yönetici\"", acceptanceHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(PanelAccessTexts.InvitationInvalid, acceptanceHtml, StringComparison.Ordinal);
        Assert.Contains("id=\"invite-forgery-message\"", acceptanceHtml, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, requestHtml, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Null(await db.LinkRecords.Where(l => l.AdminId == adminId).Select(l => l.UsedAt).SingleAsync(Ct));
        Guid invitationId = await db.AdminInvitations.Where(i => i.Email == invited).Select(i => i.Id).SingleAsync(Ct);
        Assert.Null(await db.LinkRecords.Where(l => l.AdminInvitationId == invitationId).Select(l => l.UsedAt).SingleAsync(Ct));
        Assert.False(await db.Admins.AnyAsync(a => a.Email == invited, Ct));
        Assert.False(await db.DispatchRecords.AnyAsync(Ct));
    }
}
