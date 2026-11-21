using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 D-1867 (IReadsOnForgeryRefusal): the forgery refusal reads no query value - a used link with the completed code in the address is drawn as the invalid step, where the GET draws the completed one; nothing is written.</summary>
public sealed partial class CustomerPasswordResetAndReauthenticationPageTests
{
    [Fact]
    public async Task TheResetPagesForgeryRefusal_ReadsNoQueryValue_AUsedLinkWithTheCompletedCodeIsTheInvalidStep_AndNothingIsWritten_D1867()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        Guid ali = await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        string used = await SeedLinkAsync(factory, ali, DateTimeOffset.UtcNow.AddMinutes(30), usedAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string address = $"{FramePaths.PasswordReset}/{used}?{NoticeParameter.ParameterName}={NoticeCode.PasswordResetCompleted}";
        await using AppDbContext before = NewContext();
        DateTimeOffset? usedAt = await before.LinkRecords.Select(l => l.UsedAt).SingleAsync(Ct);

        // The GET reads the query value: the completed step. The refused POST to the same address does not.
        using HttpRequestMessage get = new(HttpMethod.Get, address);
        get.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        using HttpResponseMessage got = await client.SendAsync(get, Ct);
        using HttpResponseMessage refused = await PostWithoutTokenAsync(client, address.Split('?')[0], "ResetPassword&" + address.Split('?')[1], [("Input.NewPassword", NewPassword)], null);
        string gotHtml = await BodyOf(got);
        string refusedHtml = await BodyOf(refused);

        Assert.Contains(PasswordResetTexts.Completed, gotHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Contains(PasswordResetTexts.LinkInvalid, refusedHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(PasswordResetTexts.Completed, refusedHtml, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(usedAt, await db.LinkRecords.Select(l => l.UsedAt).SingleAsync(Ct));
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
    }
}
