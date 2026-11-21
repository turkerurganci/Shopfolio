using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 (05 6.1.5; 09 8.2 item 3): the customer's reauthentication permission drops at the first page request that does not show its form - any storefront or account page,
/// not only the account page and the screen (D-1454) - and stays while only the form is shown; the reset page's spent link writes its rejection line (D-1317, UC-59).
/// </summary>
public sealed partial class CustomerPasswordResetAndReauthenticationPageTests
{
    [Theory]
    [InlineData(FramePaths.Home)]
    [InlineData(FramePaths.Cart)]
    [InlineData(FramePaths.AccountAddresses)]
    public async Task AnotherPageRequest_DropsThePermission_AndTheFormDoesNotOpenWithItsValue_D1454(string otherPage)
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        using HttpResponseMessage granted = await PostAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.Password", Entered), ("Input.ReauthenticatedOperation", "EmailChange")], session);
        string formAddress = granted.Headers.Location!.OriginalString;

        (HttpStatusCode other, _, _) = await GetAsync(client, otherPage, session);
        Session dropped = await SessionAsync();
        (_, string accountHtml, _) = await GetAsync(client, formAddress, session);

        Assert.Equal(HttpStatusCode.SeeOther, granted.StatusCode);
        Assert.Equal(HttpStatusCode.OK, other);
        Assert.Null(dropped.ReauthenticatedOperation);
        Assert.Null(dropped.ReauthenticationTokenHash);
        Assert.DoesNotContain(ReauthenticationValues.FieldName, accountHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WithNoOtherPageRequest_TheFormOpensWithItsValue_AndThePermissionStaysUntilThePost_D1454()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        using HttpResponseMessage granted = await PostAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.Password", Entered), ("Input.ReauthenticatedOperation", "EmailChange")], session);

        (_, string accountHtml, _) = await GetAsync(client, granted.Headers.Location!.OriginalString, session);

        Assert.Contains(ReauthenticationValues.FieldName, accountHtml, StringComparison.Ordinal);
        Assert.Equal(ReauthenticationOperation.EmailChange, (await SessionAsync()).ReauthenticatedOperation);
    }

    [Fact]
    public async Task ALinkSpentBetweenThePageAndTheButton_WritesTheLinkNoLongerValidLine_D1317()
    {
        CollectingLoggerProvider logs = new();
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false, logs);
        Guid ali = await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        string spent = await SeedLinkAsync(factory, ali, DateTimeOffset.UtcNow.AddMinutes(30), usedAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAsync(client, $"{FramePaths.PasswordReset}/{spent}", "ResetPassword", [("Input.NewPassword", NewPassword)], null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, logs.Count(ErrorCode.LinkNoLongerValid));
    }

    private async Task<Session> SessionAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.Sessions.AsNoTracking().SingleAsync(Ct);
    }
}
