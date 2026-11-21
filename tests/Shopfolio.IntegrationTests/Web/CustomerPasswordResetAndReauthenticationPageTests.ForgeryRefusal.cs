using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1; K-1627 (2)): the page the forgery check's 400 shows again reads what its GET reads in the page's own wrapper - the reset page the link's step or the sent address,
/// the re-authentication screen the operation and the account's ways - and nothing is consumed, sent, counted or granted.
/// </summary>
public sealed partial class CustomerPasswordResetAndReauthenticationPageTests
{
    [Fact]
    public async Task TheResetPagesForgeryRefusal_ShowsTheSettingStepOfAValidLink_AndKeepsTheSentAddressInTheRequestStep_D1843()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        Guid ali = await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        string valid = await SeedLinkAsync(factory, ali, DateTimeOffset.UtcNow.AddMinutes(30));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        await using AppDbContext before = NewContext();
        int linksBefore = await before.LinkRecords.CountAsync(Ct);

        using HttpResponseMessage setting = await PostWithoutTokenAsync(client, $"{FramePaths.PasswordReset}/{valid}", "ResetPassword", [("Input.NewPassword", NewPassword)], null);
        using HttpResponseMessage request = await PostWithoutTokenAsync(client, FramePaths.PasswordReset, "RequestPasswordReset", [("Input.Email", "ali@example.com")], null);
        string settingHtml = await BodyOf(setting);
        string requestHtml = await BodyOf(request);

        Assert.Equal(HttpStatusCode.BadRequest, setting.StatusCode);
        Assert.Contains(PasswordResetTexts.SetButton, settingHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(PasswordResetTexts.LinkInvalid, settingHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(NewPassword, settingHtml, StringComparison.Ordinal);
        Assert.Contains("id=\"password-reset-action-message\"", settingHtml, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, requestHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.BadRequest, request.StatusCode);
        Assert.Contains("value=\"ali@example.com\"", requestHtml, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.LinkRecords.CountAsync(l => l.UsedAt != null, Ct));
        Assert.Equal(linksBefore, await db.LinkRecords.CountAsync(Ct));
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
    }

    [Fact]
    public async Task TheReauthenticationScreensForgeryRefusal_ShowsTheSentOperationWithThePasswordField_AndGrantsNothing_D1843()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        using HttpResponseMessage response = await PostWithoutTokenAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.Password", Entered), ("Input.ReauthenticatedOperation", "EmailChange")], session);
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("name=\"Input.Password\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.ReauthenticatedOperation\" value=\"EmailChange\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain(Entered, html, StringComparison.Ordinal);
        Assert.Contains("id=\"reauth-action-message\"", html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Session row = await db.Sessions.AsNoTracking().SingleAsync(Ct);
        Assert.Null(row.ReauthenticatedOperation);
    }

    private static async Task<HttpResponseMessage> PostWithoutTokenAsync(HttpClient client, string path, string handler, IEnumerable<(string Name, string Value)> fields, string? cookie)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{path}?handler={handler}");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        request.Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        return await client.SendAsync(request, Ct);
    }
}
