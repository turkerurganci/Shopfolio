using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 D-1867 (IReadsOnForgeryRefusal): the panel's reset page, shown again by the forgery refusal, reads no query value - a used link with the completed code in the address is the invalid step; nothing is written.</summary>
public sealed partial class PanelAccessPagesTests
{
    [Fact]
    public async Task TheForgeryRefusal_OfTheReset_ReadsNoQueryValue_AUsedLinkWithTheCompletedCodeIsTheInvalidStep_AndNothingIsWritten_D1867()
    {
        (_, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        string used = await SeedLinkAsync(keys, adminId, Now.AddMinutes(30), Now, null);
        string address = $"{ResetPath}/{used}?{NoticeParameter.ParameterName}={NoticeCode.PasswordResetCompleted}";
        await using AppDbContext before = NewContext();
        DateTimeOffset? usedAt = await before.LinkRecords.Where(l => l.AdminId == adminId).Select(l => l.UsedAt).SingleAsync(Ct);

        (HttpStatusCode refused, string refusedHtml) = await ForgedPosts.SendAsync(client, address + "&handler=ResetPassword", [("Input.NewPassword", Replacement)]);

        Assert.Equal(HttpStatusCode.BadRequest, refused);
        Assert.Contains(PanelAccessTexts.ResetLinkInvalid, refusedHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(PanelAccessTexts.ResetCompleted, refusedHtml, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(usedAt, await db.LinkRecords.Where(l => l.AdminId == adminId).Select(l => l.UsedAt).SingleAsync(Ct));
        Assert.False(await db.DispatchRecords.AnyAsync(Ct));
    }
}
