using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the customer password reset and re-authentication pages' tests (T516): one database copied from the template.</summary>
[CollectionDefinition(CustomerPasswordResetAndReauthenticationPageTestsDefinition.Name)]
public sealed class CustomerPasswordResetAndReauthenticationPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CustomerPasswordResetAndReauthenticationPageTests";
}

/// <summary>
/// The customer password reset (/sifre-sifirlama) and the re-authentication screen (/hesap/yeniden-dogrulama) on the opened application (T516 KK1-KK10; 07 UC-56 to UC-62, UC-51; 04 E-23, E-24).
/// Accounts, links, sessions and rows are real rows of the collection database; Google is a stub that answers what a test queues; the flow's cookie and state are made by the application's own flow.
/// </summary>
[Collection(CustomerPasswordResetAndReauthenticationPageTestsDefinition.Name)]
public sealed class CustomerPasswordResetAndReauthenticationPageTests : IAsyncLifetime
{
    private const string Entered = "orchid-lantern-4471-violet";
    private const string NewPassword = "amber-harbor-9023-quartz";

    private readonly CollectionDatabaseFixture _database;

    public CustomerPasswordResetAndReauthenticationPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE customers, link_records, dispatch_records, attempt_records, disposal_records, orders CASCADE", Ct);
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO legal_text_versions (id, created_at, legal_text_type, version_number, published_at, text) "
            + "VALUES ('01000000-0000-7000-8000-000000516001', now(), 'CookiePolicy', 1, now(), 'policy') ON CONFLICT DO NOTHING",
            Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: a signed-in member sees the own address in the field; the address carries nothing ----

    [Fact]
    public async Task ASignedInMember_SeesTheOwnAddressInTheField_AndTheAddressCarriesNoEmail_T516_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        (HttpStatusCode status, string signedIn, _) = await GetAsync(client, FramePaths.PasswordReset, session);
        (_, string guest, _) = await GetAsync(client, FramePaths.PasswordReset, null);
        using HttpResponseMessage posted = await PostAsync(client, FramePaths.PasswordReset, "RequestPasswordReset", [("Input.Email", "ali@example.com")], session);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("value=\"ali@example.com\"", signedIn, StringComparison.Ordinal);
        Assert.DoesNotContain("ali@example.com", guest, StringComparison.Ordinal);
        Assert.Equal($"{FramePaths.PasswordReset}?notice={NoticeCode.PasswordResetRequested}", posted.Headers.Location!.OriginalString);
    }

    // ---- KK2: the answer is the same for a registered and an unregistered address ----

    [Fact]
    public async Task TheRequest_GivesTheSameAnswerForARegisteredAndAnUnregisteredAddress_T516_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage known = await PostAsync(client, FramePaths.PasswordReset, "RequestPasswordReset", [("Input.Email", "ali@example.com")], null);
        using HttpResponseMessage unknown = await PostAsync(client, FramePaths.PasswordReset, "RequestPasswordReset", [("Input.Email", "yok@example.com")], null);
        (_, string html, _) = await GetAsync(client, known.Headers.Location!.OriginalString, null);

        Assert.Equal(HttpStatusCode.SeeOther, known.StatusCode);
        Assert.Equal(known.StatusCode, unknown.StatusCode);
        Assert.Equal(known.Headers.Location!.OriginalString, unknown.Headers.Location!.OriginalString);
        Assert.Contains(PasswordResetTexts.Requested, html, StringComparison.Ordinal);
    }

    // ---- KK3: the link and the e-mail row only for a verified account, in the three connection states ----

    [Fact]
    public async Task TheRequest_QueuesTheEmailOnlyForAVerifiedAccount_AWithoutPasswordIncluded_T516_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        await SeedAsync("gul@example.com", hasPassword: false, googleLinked: true);
        await SeedAsync("bekleyen@example.com", hasPassword: true, googleLinked: false, verified: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        foreach (string email in new[] { "ali@example.com", "gul@example.com", "bekleyen@example.com", "yok@example.com" })
        {
            using HttpResponseMessage response = await PostAsync(client, FramePaths.PasswordReset, "RequestPasswordReset", [("Input.Email", email)], null);
            Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        }

        await using AppDbContext db = NewContext();
        List<DispatchRecord> rows = await db.DispatchRecords.AsNoTracking().ToListAsync(Ct);
        Assert.Equal(["ali@example.com", "gul@example.com"], rows.Select(r => r.RecipientEmail!).Order().ToArray());
        Assert.All(rows, r => Assert.Equal(DispatchType.PasswordResetLink, r.DispatchType));
        Assert.Equal(2, await db.LinkRecords.CountAsync(l => l.LinkType == LinkType.PasswordReset, Ct));
    }

    // ---- KK4: 422 for a broken field, 429 over the limit, every request counted ----

    [Theory]
    [InlineData("")]
    [InlineData("not-an-address")]
    public async Task ABrokenAddress_Gives422_AndWritesNothing_T516_KK4(string email)
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAsync(client, FramePaths.PasswordReset, "RequestPasswordReset", [("Input.Email", email)], null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
        Assert.Equal(0, await db.AttemptRecords.CountAsync(Ct));
    }

    [Fact]
    public async Task TooManyRequestsOfOneAddress_Give429_AndTheRequestsAreCounted_T516_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        HttpStatusCode last = HttpStatusCode.OK;
        bool retryAfter = false;
        for (int i = 0; i <= ProductConstants.PasswordResetAttemptsPerEmail + 1 && last != HttpStatusCode.TooManyRequests; i++)
        {
            using HttpResponseMessage attempt = await PostAsync(client, FramePaths.PasswordReset, "RequestPasswordReset", [("Input.Email", "ali@example.com")], null);
            last = attempt.StatusCode;
            retryAfter = attempt.Headers.Contains("Retry-After");
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last);
        Assert.False(retryAfter);
        await using AppDbContext db = NewContext();
        Assert.Equal(ProductConstants.PasswordResetAttemptsPerEmail + 1, await db.AttemptRecords.CountAsync(a => a.AttemptLimit == AttemptLimit.L2 && a.AttemptAxis == AttemptAxis.Email, Ct));
    }

    [Fact]
    public async Task TheBlockOfTheResetRequest_LiftsWhenTheAttemptsLeaveTheWindow_D572_T516_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        HttpStatusCode blocked = HttpStatusCode.OK;
        for (int i = 0; i <= ProductConstants.PasswordResetAttemptsPerEmail + 1 && blocked != HttpStatusCode.TooManyRequests; i++)
        {
            using HttpResponseMessage attempt = await PostAsync(client, FramePaths.PasswordReset, "RequestPasswordReset", [("Input.Email", "ali@example.com")], null);
            blocked = attempt.StatusCode;
        }

        await using (AppDbContext db = NewContext())
        {
            await db.Database.ExecuteSqlRawAsync("UPDATE attempt_records SET created_at = created_at - interval '2 hours'", Ct);
        }

        using HttpResponseMessage after = await PostAsync(client, FramePaths.PasswordReset, "RequestPasswordReset", [("Input.Email", "ali@example.com")], null);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked);
        Assert.Equal(HttpStatusCode.SeeOther, after.StatusCode);
    }

    // ---- KK5: the five states of a link ----

    [Fact]
    public async Task AValidLink_OpensTheSettingStep_AndEveryOtherStateOpensTheInvalidOne_T516_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        Guid ali = await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        Guid gul = await SeedAsync("gul@example.com", hasPassword: false, googleLinked: true);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string valid = await SeedLinkAsync(factory, ali, now.AddMinutes(30));
        string googleOnlyLink = await SeedLinkAsync(factory, gul, now.AddMinutes(30));
        string used = await SeedLinkAsync(factory, ali, now.AddMinutes(30), usedAt: now.AddMinutes(-1));
        string ended = await SeedLinkAsync(factory, ali, now.AddMinutes(-1));
        string renewed = await SeedLinkAsync(factory, ali, now.AddMinutes(30), supersededAt: now.AddMinutes(-1));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode okStatus, string okHtml, _) = await GetAsync(client, $"{FramePaths.PasswordReset}/{valid}", null);
        (_, string googleOnlyHtml, _) = await GetAsync(client, $"{FramePaths.PasswordReset}/{googleOnlyLink}", null);

        Assert.Equal(HttpStatusCode.OK, okStatus);
        Assert.Contains(PasswordResetTexts.SetButton, okHtml, StringComparison.Ordinal);
        Assert.Contains(PasswordResetTexts.SetPasswordButton, googleOnlyHtml, StringComparison.Ordinal);
        foreach (string key in new[] { used, ended, renewed, "not-a-known-key" })
        {
            (HttpStatusCode status, string html, _) = await GetAsync(client, $"{FramePaths.PasswordReset}/{key}", null);
            Assert.Equal(HttpStatusCode.OK, status);
            Assert.Contains(PasswordResetTexts.LinkInvalid, html, StringComparison.Ordinal);
            Assert.DoesNotContain(PasswordResetTexts.SetButton, html, StringComparison.Ordinal);
        }
    }

    // ---- KK6: the new password closes every session, opens none, answers 303 ----

    [Fact]
    public async Task TheNewPassword_ClosesEverySession_OpensNone_AndTheOldPasswordStopsWorking_T516_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        Guid ali = await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        await SignInAsync(client, "ali@example.com");
        await SignInAsync(client, "ali@example.com");
        string key = await SeedLinkAsync(factory, ali, DateTimeOffset.UtcNow.AddMinutes(30));

        using HttpResponseMessage response = await PostAsync(client, $"{FramePaths.PasswordReset}/{key}", "ResetPassword", [("Input.NewPassword", NewPassword)], null);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal($"{FramePaths.PasswordReset}/{key}?notice={NoticeCode.PasswordResetCompleted}", response.Headers.Location!.OriginalString);
        Assert.DoesNotContain(SetCookies(response), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal));
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
        (_, string completed, _) = await GetAsync(client, response.Headers.Location!.OriginalString, null);
        Assert.Contains(PasswordResetTexts.Completed, completed, StringComparison.Ordinal);
        using HttpResponseMessage old = await PostSignInAsync(client, "ali@example.com", Entered);
        using HttpResponseMessage fresh = await PostSignInAsync(client, "ali@example.com", NewPassword);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, old.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, fresh.StatusCode);
    }

    // ---- KK7: a weak or common password does not consume the link; a spent link is the 200 invalid state ----

    [Theory]
    [InlineData("short")]
    [InlineData("1234567890")]
    public async Task AWeakOrCommonPassword_Gives422_AndTheLinkIsNotConsumed_T516_KK7(string password)
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        Guid ali = await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        string key = await SeedLinkAsync(factory, ali, DateTimeOffset.UtcNow.AddMinutes(30));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAsync(client, $"{FramePaths.PasswordReset}/{key}", "ResetPassword", [("Input.NewPassword", password)], null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.Null((await db.LinkRecords.AsNoTracking().SingleAsync(Ct)).UsedAt);
        (HttpStatusCode again, _, _) = await GetAsync(client, $"{FramePaths.PasswordReset}/{key}", null);
        Assert.Equal(HttpStatusCode.OK, again);
    }

    [Fact]
    public async Task ALinkSpentBetweenThePageAndTheButton_Gives200WithTheInvalidState_AndChangesNothing_T516_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        Guid ali = await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        string spent = await SeedLinkAsync(factory, ali, DateTimeOffset.UtcNow.AddMinutes(30), usedAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string before;
        await using (AppDbContext read = NewContext())
        {
            before = (await read.Customers.AsNoTracking().SingleAsync(Ct)).PasswordHash!;
        }

        using HttpResponseMessage response = await PostAsync(client, $"{FramePaths.PasswordReset}/{spent}", "ResetPassword", [("Input.NewPassword", NewPassword)], null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(PasswordResetTexts.LinkInvalid, await BodyOf(response), StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(before, (await db.Customers.AsNoTracking().SingleAsync(Ct)).PasswordHash);
    }

    // ---- KK8: the screen's three operations, the two field conditions, the closed list, the earlier permission ----

    [Theory]
    [InlineData("PasswordChange")]
    [InlineData("EmailChange")]
    [InlineData("AccountDeletion")]
    public async Task TheScreen_OpensForEachOfTheThreeOperations_WithThePasswordFieldOfAnAccountWithPassword_T516_KK8(string operation)
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        (HttpStatusCode status, string html, _) = await GetAsync(client, $"{FramePaths.Reauthentication}?reauthenticatedOperation={operation}", session);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(ReauthenticationTexts.OperationLine(Enum.Parse<ReauthenticationOperation>(operation)), html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Password\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain(ReauthenticationTexts.GoogleButton, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheTwoFieldConditions_PasswordFieldNeedsAPassword_GoogleButtonNeedsALinkAndTheOpenGate_T516_KK8()
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: true);
        await SeedAsync("both@example.com", hasPassword: true, googleLinked: true);
        await SeedAsync("gul@example.com", hasPassword: false, googleLinked: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string both = await SignInAsync(client, "both@example.com");
        string gul = await GoogleSignInAsync(factory, gateway, "gul@example.com");
        string address = $"{FramePaths.Reauthentication}?reauthenticatedOperation=PasswordChange";

        (_, string bothHtml, _) = await GetAsync(client, address, both);
        (_, string gulHtml, _) = await GetAsync(client, address, gul);

        Assert.Contains("name=\"Input.Password\"", bothHtml, StringComparison.Ordinal);
        Assert.Contains(ReauthenticationTexts.GoogleButton, bothHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.Password\"", gulHtml, StringComparison.Ordinal);
        Assert.Contains(ReauthenticationTexts.GoogleButton, gulHtml, StringComparison.Ordinal);
        Assert.Contains(ReauthenticationTexts.SetPasswordLink, gulHtml, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("passwordchange")]
    [InlineData("0")]
    [InlineData("Other")]
    public async Task AnOperationOutsideTheClosedList_IsA303ToTheAccountPage_T516_KK8(string operation)
    {
        CollectingLoggerProvider logs = new();
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false, logs);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        (HttpStatusCode status, HttpResponseMessage response) = await SendAsync(client, HttpMethod.Get, $"{FramePaths.Reauthentication}?reauthenticatedOperation={Uri.EscapeDataString(operation)}", session);
        using HttpResponseMessage posted = await PostAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.Password", Entered), ("Input.ReauthenticatedOperation", operation)], session);

        Assert.Equal(HttpStatusCode.SeeOther, status);
        Assert.Equal(FramePaths.Account, response.Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.SeeOther, posted.StatusCode);
        Assert.Equal(FramePaths.Account, posted.Headers.Location!.OriginalString);
        Assert.Contains(logs.Lines, l => l.Contains("RejectionLog|", StringComparison.Ordinal) && l.EndsWith(" " + ErrorCode.ReauthenticationOperationInvalid, StringComparison.Ordinal));
        response.Dispose();
    }

    [Fact]
    public async Task OpeningTheScreen_DropsTheEarlierPermission_T516_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        using HttpResponseMessage granted = await PostAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.Password", Entered), ("Input.ReauthenticatedOperation", "EmailChange")], session);
        await using (AppDbContext before = NewContext())
        {
            Assert.NotNull((await before.Sessions.AsNoTracking().SingleAsync(Ct)).ReauthenticatedOperation);
        }

        await GetAsync(client, $"{FramePaths.Reauthentication}?reauthenticatedOperation=PasswordChange", session);

        await using AppDbContext db = NewContext();
        Session row = await db.Sessions.AsNoTracking().SingleAsync(Ct);
        Assert.Null(row.ReauthenticatedOperation);
        Assert.Null(row.ReauthenticationTokenHash);
        Assert.Equal(HttpStatusCode.SeeOther, granted.StatusCode);
    }

    [Fact]
    public async Task WithoutASession_TheScreenSendsThePersonToTheSignIn_T516_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, HttpResponseMessage response) = await SendAsync(client, HttpMethod.Get, $"{FramePaths.Reauthentication}?reauthenticatedOperation=PasswordChange", null);

        Assert.Equal(HttpStatusCode.Found, status);
        Assert.StartsWith(FramePaths.SignIn, response.Headers.Location!.OriginalString, StringComparison.Ordinal);
        response.Dispose();
    }

    // ---- KK9: the right password gives one permission; the wrong one is 422 and counts for L-1 ----

    [Fact]
    public async Task TheRightPassword_GivesOnePermission_AndA303ToTheAccountPageWithTheValue_T516_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        using HttpResponseMessage first = await PostAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.Password", Entered), ("Input.ReauthenticatedOperation", "PasswordChange")], session);
        using HttpResponseMessage second = await PostAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.Password", Entered), ("Input.ReauthenticatedOperation", "AccountDeletion")], session);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.StartsWith($"{FramePaths.Account}?reauthenticatedOperation=PasswordChange&reauthenticationToken=", first.Headers.Location!.OriginalString, StringComparison.Ordinal);
        Assert.StartsWith($"{FramePaths.Account}?reauthenticatedOperation=AccountDeletion&reauthenticationToken=", second.Headers.Location!.OriginalString, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Session row = await db.Sessions.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(ReauthenticationOperation.AccountDeletion, row.ReauthenticatedOperation);
        Assert.NotNull(row.ReauthenticationTokenHash);
    }

    [Fact]
    public async Task AWrongPassword_Gives422WithTheFieldMessage_CountsForL1_AndGivesNoPermission_T516_KK9()
    {
        CollectingLoggerProvider logs = new();
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false, logs);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        await using AppDbContext counted = NewContext();
        int before = await counted.AttemptRecords.CountAsync(a => a.AttemptLimit == AttemptLimit.L1, Ct);

        using HttpResponseMessage response = await PostAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.Password", Entered + "x"), ("Input.ReauthenticatedOperation", "PasswordChange")], session);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(ReauthenticationTexts.PasswordWrong, await BodyOf(response), StringComparison.Ordinal);
        Assert.True(await counted.AttemptRecords.CountAsync(a => a.AttemptLimit == AttemptLimit.L1, Ct) > before);
        Assert.Null((await counted.Sessions.AsNoTracking().SingleAsync(Ct)).ReauthenticatedOperation);
        Assert.Contains(logs.Lines, l => l.Contains("RejectionLog|", StringComparison.Ordinal) && l.EndsWith(" " + ErrorCode.ReauthenticationFailed, StringComparison.Ordinal));
    }

    [Fact]
    public async Task TooManyWrongPasswords_Give429_AndTheRightOneIsThenNotAccepted_T516_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        HttpStatusCode last = HttpStatusCode.OK;
        for (int i = 0; i <= ProductConstants.SignInAttemptsPerEmail + 2 && last != HttpStatusCode.TooManyRequests; i++)
        {
            using HttpResponseMessage attempt = await PostAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.Password", Entered + "x"), ("Input.ReauthenticatedOperation", "PasswordChange")], session);
            last = attempt.StatusCode;
        }

        using HttpResponseMessage right = await PostAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.Password", Entered), ("Input.ReauthenticatedOperation", "PasswordChange")], session);

        Assert.Equal(HttpStatusCode.TooManyRequests, last);
        Assert.Equal(HttpStatusCode.TooManyRequests, right.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.Null((await db.Sessions.AsNoTracking().SingleAsync(Ct)).ReauthenticatedOperation);
    }

    [Fact]
    public async Task AnEmptyPassword_Gives422_T516_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        using HttpResponseMessage response = await PostAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.Password", string.Empty), ("Input.ReauthenticatedOperation", "PasswordChange")], session);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(ReauthenticationTexts.OperationLine(ReauthenticationOperation.PasswordChange), await BodyOf(response), StringComparison.Ordinal);
    }

    // ---- KK10: the Google route ----

    [Fact]
    public async Task TheGoogleRoute_RedirectsToGoogleAskingForAFreshSignIn_AndTheReturnGrantsTheOperation_T516_KK10()
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: true);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        await using AppDbContext counted = NewContext();
        int attemptsBefore = await counted.AttemptRecords.CountAsync(Ct);

        (string flowCookie, string state, string nonce, Uri google) = await StartGoogleAsync(client, session, "EmailChange");
        gateway.Enqueue(new GoogleIdentityResult.Succeeded("ali@example.com", true, "Ali", null, DateTimeOffset.UtcNow.AddMinutes(5), nonce));
        (_, HttpResponseMessage back) = await ReturnAsync(factory, $"{session}; {flowCookie}", $"code=c1&state={Uri.EscapeDataString(state)}");

        Assert.Contains("auth_time", Uri.UnescapeDataString(google.Query), StringComparison.Ordinal);
        Assert.StartsWith($"{FramePaths.Account}?reauthenticatedOperation=EmailChange&reauthenticationToken=", back.Headers.Location!.OriginalString, StringComparison.Ordinal);
        Assert.Equal(ReauthenticationOperation.EmailChange, (await counted.Sessions.AsNoTracking().SingleAsync(Ct)).ReauthenticatedOperation);
        Assert.Equal(attemptsBefore, await counted.AttemptRecords.CountAsync(Ct));
        back.Dispose();
    }

    [Theory]
    [InlineData("old-sign-in")]
    [InlineData("no-moment")]
    [InlineData("other-address")]
    [InlineData("unverified")]
    public async Task AReturnWithAnOldOrMissingAuthTimeAnotherAddressOrAnUnverifiedOne_GrantsNothing_AndComesBackWithTheCode_T516_KK10(string kind)
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: true);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        (string flowCookie, string state, string nonce, _) = await StartGoogleAsync(client, session, "PasswordChange");
        DateTimeOffset future = DateTimeOffset.UtcNow.AddMinutes(5);
        gateway.Enqueue(kind switch
        {
            "old-sign-in" => new GoogleIdentityResult.Succeeded("ali@example.com", true, "Ali", null, DateTimeOffset.UtcNow.AddHours(-1), nonce),
            "no-moment" => new GoogleIdentityResult.Succeeded("ali@example.com", true, "Ali", null, null, nonce),
            "other-address" => new GoogleIdentityResult.Succeeded("veli@example.com", true, "Veli", null, future, nonce),
            _ => new GoogleIdentityResult.Succeeded("ali@example.com", false, "Ali", null, future, nonce),
        });

        (_, HttpResponseMessage back) = await ReturnAsync(factory, $"{session}; {flowCookie}", $"code=c1&state={Uri.EscapeDataString(state)}");

        Assert.Equal(
            $"{FramePaths.Reauthentication}?reauthenticatedOperation=PasswordChange&notice={NoticeCode.SocialReauthenticationFailed}",
            back.Headers.Location!.OriginalString);
        await using AppDbContext db = NewContext();
        Assert.Null((await db.Sessions.AsNoTracking().SingleAsync(Ct)).ReauthenticatedOperation);
        back.Dispose();
    }

    [Theory]
    [InlineData("failed", NoticeCode.SocialReauthenticationFailed)]
    [InlineData("unavailable", NoticeCode.SocialReauthenticationUnavailable)]
    public async Task AFailedOrUnreachableGoogle_ComesBackWithItsOwnCode_T516_KK10(string answer, NoticeCode expected)
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: true);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        (string flowCookie, string state, _, _) = await StartGoogleAsync(client, session, "AccountDeletion");
        gateway.Enqueue(answer == "failed" ? new GoogleIdentityResult.Failed("invalid") : new GoogleIdentityResult.Unavailable("timeout"));

        (_, HttpResponseMessage back) = await ReturnAsync(factory, $"{session}; {flowCookie}", $"code=c1&state={Uri.EscapeDataString(state)}");
        (_, string html, _) = await GetAsync(client, back.Headers.Location!.OriginalString, session);

        Assert.Equal($"{FramePaths.Reauthentication}?reauthenticatedOperation=AccountDeletion&notice={expected}", back.Headers.Location!.OriginalString);
        Assert.Contains(expected == NoticeCode.SocialReauthenticationFailed ? ReauthenticationTexts.GoogleFailed : ReauthenticationTexts.GoogleUnavailable, html, StringComparison.Ordinal);
        back.Dispose();
    }

    [Fact]
    public async Task TheGoogleRoute_IsRefused422ForAnAccountNotLinkedToGoogle_AndCountsNothingForL1_T516_KK10()
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: true);
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        await using AppDbContext db = NewContext();
        int before = await db.AttemptRecords.CountAsync(Ct);

        using HttpResponseMessage response = await PostAsync(client, FramePaths.Reauthentication, "StartSocialReauthentication", [("Input.ReauthenticatedOperation", "PasswordChange")], session);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(ReauthenticationTexts.GoogleNotOffered, await BodyOf(response), StringComparison.Ordinal);
        Assert.Equal(before, await db.AttemptRecords.CountAsync(Ct));
        Assert.Empty(gateway.Calls);
    }

    // ---- helpers ----

    private WebApplicationFactory<Program> Open(StubGateway? google, bool gateOpen, CollectingLoggerProvider? logs = null) =>
        new ShopfolioWebApplicationFactory(_database.Dsn, loggerProvider: logs).WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddRazorPages().AddApplicationPart(typeof(FrameSampleModel).Assembly);
            s.RemoveAll<IReadOnlyList<ConditionalLine>>();
            s.AddSingleton<IReadOnlyList<ConditionalLine>>([new ConditionalLine(ConditionalLine.GoogleSignIn, google is null ? ["Google:ClientId"] : [])]);
            if (google is not null)
            {
                s.Configure<GoogleOptions>(o => o.ClientId = "client-x");
                s.RemoveAll<IGoogleIdentityGateway>();
                s.AddSingleton<IGoogleIdentityGateway>(google);
            }

            if (gateOpen)
            {
                s.RemoveAll<ISettingsModule>();
                s.AddScoped(sp => CartPageTests.OpenGateSettings.Create(ActivatorUtilities.CreateInstance<Shopfolio.Modules.Settings.SettingsModule>(sp)));
            }
        }));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<Guid> SeedAsync(string email, bool hasPassword, bool googleLinked, bool verified = true)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Customer customer = Customer.Create("Ali", email, hasPassword ? new PasswordService().Hash(Entered) : null, verified ? now : null, googleLinked ? now : null, now);
        await using AppDbContext db = NewContext();
        db.Customers.Add(customer);
        await db.SaveChangesAsync(Ct);
        return customer.Id;
    }

    // A link made with the application's own key derivation; the returned key is what the address carries.
    private async Task<string> SeedLinkAsync(WebApplicationFactory<Program> factory, Guid customerId, DateTimeOffset expiresAt, DateTimeOffset? usedAt = null, DateTimeOffset? supersededAt = null)
    {
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        LinkRecord link = LinkRecord.ForCustomer(keys, LinkType.PasswordReset, customerId, expiresAt, now.AddMinutes(-5));
        link.UsedAt = usedAt;
        link.SupersededAt = supersededAt;
        await using AppDbContext db = NewContext();
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return keys.Derive(link.Id);
    }

    private static async Task<string> SignInAsync(HttpClient client, string email)
    {
        using HttpResponseMessage response = await PostSignInAsync(client, email, Entered);
        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        return ValueOf(Assert.Single(SetCookies(response), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal)));
    }

    // A Google-only account has no password; its session comes from a Google sign-in return (UC-51).
    private static async Task<string> GoogleSignInAsync(WebApplicationFactory<Program> factory, StubGateway gateway, string email)
    {
        GoogleSignInFlow flow = factory.Services.GetRequiredService<GoogleSignInFlow>();
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        Uri address = flow.Start(context, GoogleFlowKind.SignInOrRegistration, null, null);
        Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(address.Query);
        gateway.Enqueue(new GoogleIdentityResult.Succeeded(email, true, "Gul", null, DateTimeOffset.UtcNow, query["nonce"].ToString()));
        (_, HttpResponseMessage back) = await ReturnAsync(factory, context.Response.Headers.SetCookie.ToString().Split(';')[0], $"code=g1&state={Uri.EscapeDataString(query["state"].ToString())}");
        using (back)
        {
            Assert.Equal(HttpStatusCode.SeeOther, back.StatusCode);
            return ValueOf(Assert.Single(SetCookies(back), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal)));
        }
    }

    private static async Task<(string FlowCookie, string State, string Nonce, Uri Google)> StartGoogleAsync(HttpClient client, string session, string operation)
    {
        using HttpResponseMessage response = await PostAsync(client, FramePaths.Reauthentication, "StartSocialReauthentication", [("Input.ReauthenticatedOperation", operation)], session);
        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Uri google = response.Headers.Location!;
        Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(google.Query);
        string flowCookie = ValueOf(Assert.Single(SetCookies(response), c => !c.StartsWith(CookieNames.Antiforgery + "=", StringComparison.Ordinal)));
        return (flowCookie, query["state"].ToString(), query["nonce"].ToString(), google);
    }

    private static async Task<(HttpStatusCode Status, HttpResponseMessage Response)> ReturnAsync(WebApplicationFactory<Program> factory, string? cookie, string query)
    {
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false });
        using HttpRequestMessage request = new(HttpMethod.Get, $"/giris/google?{query}");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, response);
    }

    private static async Task<(HttpStatusCode Status, string Html, IReadOnlyList<string> SetCookies)> GetAsync(HttpClient client, string path, string? cookie)
    {
        (HttpStatusCode status, HttpResponseMessage response) = await SendAsync(client, HttpMethod.Get, path, cookie);
        using (response)
        {
            return (status, await BodyOf(response), SetCookies(response));
        }
    }

    private static async Task<(HttpStatusCode Status, HttpResponseMessage Response)> SendAsync(HttpClient client, HttpMethod method, string path, string? cookie)
    {
        using HttpRequestMessage request = new(method, path);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, response);
    }

    private static Task<HttpResponseMessage> PostSignInAsync(HttpClient client, string email, string password) =>
        PostAsync(client, FramePaths.SignIn, "SignIn", [("Input.Email", email), ("Input.Password", password)], null);

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string handler, IEnumerable<(string Name, string Value)> fields, string? cookie)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{path}?handler={handler}");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        request.Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        return await client.SendAsync(request, Ct);
    }

    private static string ValueOf(string setCookie) => setCookie.Split(';')[0];

    // The default encoder writes Turkish letters as character references; the texts are compared as the browser reads them.
    private static async Task<string> BodyOf(HttpResponseMessage response) => WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

    private static List<string> SetCookies(HttpResponseMessage response) => response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values) ? [.. values] : [];

    // Google as a test queues it: the next answer, or a failure when none is queued; every call is kept.
    private sealed class StubGateway : IGoogleIdentityGateway
    {
        private readonly Queue<GoogleIdentityResult> _results = new();
        private readonly List<IdentityCodeExchange> _calls = [];

        public IReadOnlyList<IdentityCodeExchange> Calls
        {
            get
            {
                lock (_calls)
                {
                    return [.. _calls];
                }
            }
        }

        public void Enqueue(GoogleIdentityResult result)
        {
            lock (_calls)
            {
                _results.Enqueue(result);
            }
        }

        public Task<GoogleIdentityResult> VerifyIdentityAsync(IdentityCodeExchange exchange, CancellationToken ct)
        {
            lock (_calls)
            {
                _calls.Add(exchange);
                return Task.FromResult(_results.Count > 0 ? _results.Dequeue() : new GoogleIdentityResult.Failed("not-queued"));
            }
        }
    }
}
