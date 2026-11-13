using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the account screen's tests (T517): one database copied from the template.</summary>
[CollectionDefinition(AccountProfilePageTestsDefinition.Name)]
public sealed class AccountProfilePageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "AccountProfilePageTests";
}

/// <summary>
/// The account screen (/hesap) and the deletion's result state (/hesap/silindi) on the opened application (T517 KK1-KK10; 07 UC-63 to UC-69; 04 E-25). Accounts, sessions, links and rows are real rows of the
/// collection database; the re-authentication is made through the application's own screen, so the single-use value is the one the application gave.
/// </summary>
[Collection(AccountProfilePageTestsDefinition.Name)]
public sealed class AccountProfilePageTests : IAsyncLifetime
{
    private const string Entered = "orchid-lantern-4471-violet";
    private const string NewPassword = "amber-harbor-9023-quartz";

    private readonly CollectionDatabaseFixture _database;

    public AccountProfilePageTests(CollectionDatabaseFixture database)
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
            + "VALUES ('01000000-0000-7000-8000-000000517001', now(), 'CookiePolicy', 1, now(), 'policy') ON CONFLICT DO NOTHING",
            Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the five sections; no session is 302 to the sign-in ----

    [Fact]
    public async Task TheScreen_GivesTheFiveSections_AndNoSessionGoesToTheSignIn_T517_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        (HttpStatusCode status, string html, _) = await GetAsync(client, FramePaths.Account, session);
        (HttpStatusCode anonymous, _, HttpResponseMessage redirect) = await SendAsync(client, HttpMethod.Get, FramePaths.Account, null);

        Assert.Equal(HttpStatusCode.OK, status);
        foreach (string section in new[] { "account-name", "account-email", "account-password", "account-methods", "account-delete" })
        {
            Assert.Contains($"id=\"{section}\"", html, StringComparison.Ordinal);
        }

        Assert.Contains("ali@example.com", html, StringComparison.Ordinal);
        Assert.Contains(AccountTexts.PasswordMethod(true), html, StringComparison.Ordinal);
        Assert.Contains(AccountTexts.GoogleMethod(true), html, StringComparison.Ordinal);
        Assert.Contains($"href=\"{FramePaths.AccountAddresses}\"", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Found, anonymous);
        Assert.Equal($"{FramePaths.SignIn}?returnUrl={Uri.EscapeDataString(FramePaths.Account)}", redirect.Headers.Location!.OriginalString);
        redirect.Dispose();
    }

    // ---- KK2: the form opens only under the session's own permission and value; a page request outside the form drops it ----

    [Fact]
    public async Task TheFormOpens_OnlyForTheOperationAndTheValueTheSessionHolds_AndEveryOtherParameterIsIgnored_T517_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        string address = await ReauthenticateAsync(client, session, "PasswordChange");
        string permit = TokenOf(address);

        (_, string closedOther, _) = await GetAsync(client, $"{FramePaths.Account}?reauthenticatedOperation=EmailChange&reauthenticationToken={Uri.EscapeDataString(permit)}", session);
        (_, string closedWrong, _) = await GetAsync(client, $"{FramePaths.Account}?reauthenticatedOperation=PasswordChange&reauthenticationToken=not-the-value", session);
        (_, string opened, _) = await GetAsync(client, address, session);
        (_, string reloaded, _) = await GetAsync(client, address, session);

        Assert.DoesNotContain("name=\"Input.NewPassword\"", closedOther, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.NewEmail\"", closedOther, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.NewPassword\"", closedWrong, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.NewPassword\"", opened, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.ReauthenticationToken\" value=\"{permit}\"", opened, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.NewPassword\"", reloaded, StringComparison.Ordinal);
    }

    [Fact]
    public async Task APageRequestOutsideTheForm_DropsThePermission_AndTheFormsDisplayDoesNot_T517_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        string address = await ReauthenticateAsync(client, session, "PasswordChange");

        await GetAsync(client, address, session);
        Assert.NotNull((await ReadSessionAsync()).ReauthenticatedOperation);
        await GetAsync(client, FramePaths.Account, session);

        Assert.Null((await ReadSessionAsync()).ReauthenticatedOperation);
        using HttpResponseMessage spent = await PostAsync(client, FramePaths.Account, "ChangePassword", [("Input.NewPassword", NewPassword), ("Input.ReauthenticationToken", TokenOf(address))], session);
        Assert.Equal(HttpStatusCode.SeeOther, spent.StatusCode);
        Assert.Equal(ReauthenticationAddresses.Screen(ReauthenticationOperation.PasswordChange), spent.Headers.Location!.OriginalString);
    }

    [Theory]
    [InlineData("ChangePassword", "Input.NewPassword", "PasswordChange")]
    [InlineData("ChangeEmail", "Input.NewEmail", "EmailChange")]
    public async Task ThePosts_WithoutTheValue_Go303ToTheReauthenticationAndWriteNothing_T517_KK2(string handler, string field, string operation)
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        string value = field == "Input.NewEmail" ? "yeni@example.com" : NewPassword;

        using HttpResponseMessage none = await PostAsync(client, FramePaths.Account, handler, [(field, value)], session);
        using HttpResponseMessage wrong = await PostAsync(client, FramePaths.Account, handler, [(field, value), ("Input.ReauthenticationToken", "not-the-value")], session);

        string screen = $"{FramePaths.Reauthentication}?reauthenticatedOperation={operation}";
        Assert.Equal(screen, none.Headers.Location!.OriginalString);
        Assert.Equal(screen, wrong.Headers.Location!.OriginalString);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.PendingEmailChanges.CountAsync(Ct));
        Assert.True(new PasswordService().Verify((await db.Customers.AsNoTracking().SingleAsync(Ct)).PasswordHash!, Entered).Matched);
    }

    // ---- KK3: the revert link closes the e-mail change and the deletion; the account without a password is shown the password-setting ----

    [Fact]
    public async Task WhileTheRevertLinkIsOpen_TheEmailChangeAndTheDeletionAreClosed_WithTheReasonAndTheDate_T517_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        Guid id = await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        DateTimeOffset ends = await SeedRevertLinkAsync(factory, id);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        string emailAddress = await ReauthenticateAsync(client, session, "EmailChange");
        (_, string html, _) = await GetAsync(client, emailAddress, session);
        string deletionAddress = await ReauthenticateAsync(client, session, "AccountDeletion");
        (_, string deletionHtml, _) = await GetAsync(client, deletionAddress, session);

        Assert.Contains(AccountTexts.RevertOpen(ends), html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"account-email-change\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.NewEmail\"", html, StringComparison.Ordinal);
        Assert.Contains(AccountTexts.RevertOpen(ends), deletionHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"account-delete-start\"", deletionHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(AccountTexts.DeleteConfirmLabel, deletionHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheEmailChangeAndTheDeletion_OfAClosedAccount_Give422AndSpendNothing_T517_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        Guid id = await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        await SeedRevertLinkAsync(factory, id);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        string emailPermit = TokenOf(await ReauthenticateAsync(client, session, "EmailChange"));

        using HttpResponseMessage email = await PostAsync(client, FramePaths.Account, "ChangeEmail", [("Input.NewEmail", "yeni@example.com"), ("Input.ReauthenticationToken", emailPermit)], session);
        string deletionPermit = TokenOf(await ReauthenticateAsync(client, session, "AccountDeletion"));
        using HttpResponseMessage deletion = await PostAsync(client, FramePaths.Account, "DeleteAccount", [("Input.ReauthenticationToken", deletionPermit), ("Input.Confirmed", "true")], session);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, email.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, deletion.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.PendingEmailChanges.CountAsync(Ct));
        Assert.Equal(1, await db.Customers.CountAsync(Ct));
        Assert.NotNull((await ReadSessionAsync()).ReauthenticationTokenHash);
    }

    [Fact]
    public async Task AnAccountWithoutAPassword_IsShownThePasswordSetting_AndNoPasswordChange_T517_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        Guid id = await SeedAsync("gul@example.com", hasPassword: false, googleLinked: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SeedGoogleSessionAsync(id);

        (_, string html, _) = await GetAsync(client, FramePaths.Account, session);
        using HttpResponseMessage post = await PostAsync(client, FramePaths.Account, "ChangePassword", [("Input.NewPassword", NewPassword)], session);

        Assert.Contains($"id=\"account-password-set\" class=\"button button--secondary\" href=\"{FramePaths.PasswordReset}\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"account-password-change\"", html, StringComparison.Ordinal);
        Assert.Contains(AccountTexts.PasswordMethod(false), html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, post.StatusCode);
        Assert.Contains(AccountTexts.NoPassword, await BodyOf(post), StringComparison.Ordinal);
    }

    // ---- KK4: the name ----

    [Fact]
    public async Task TheName_IsWritten_With303AndTheSavedNotice_AndAnEmptyNameIs422_T517_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        using HttpResponseMessage empty = await PostAsync(client, FramePaths.Account, "ChangeName", [("Input.Name", "   ")], session);
        using HttpResponseMessage saved = await PostAsync(client, FramePaths.Account, "ChangeName", [("Input.Name", "  Yeni Ad ")], session);
        (_, string html, _) = await GetAsync(client, saved.Headers.Location!.OriginalString, session);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Equal($"{FramePaths.Account}?notice={NoticeCode.Saved}", saved.Headers.Location!.OriginalString);
        Assert.Contains("value=\"Yeni Ad\"", html, StringComparison.Ordinal);
        Assert.Contains(NoticeTexts.Saved, html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal("Yeni Ad", (await db.Customers.AsNoTracking().SingleAsync(Ct)).Name);
        Assert.Equal(0, await db.PendingEmailChanges.CountAsync(Ct));
    }

    // ---- KK5, KK6: the password ----

    [Fact]
    public async Task ThePasswordChange_TakesTheValueOnce_EndsTheOtherSessions_AndKeepsThisOne_T517_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        string other = await SignInAsync(client, "ali@example.com");
        string permit = TokenOf(await ReauthenticateAsync(client, session, "PasswordChange"));

        using HttpResponseMessage changed = await PostAsync(client, FramePaths.Account, "ChangePassword", [("Input.NewPassword", NewPassword), ("Input.ReauthenticationToken", permit)], session);
        using HttpResponseMessage again = await PostAsync(client, FramePaths.Account, "ChangePassword", [("Input.NewPassword", "other-harbor-1234-fox"), ("Input.ReauthenticationToken", permit)], session);
        (_, string html, _) = await GetAsync(client, changed.Headers.Location!.OriginalString, session);
        (HttpStatusCode keptStatus, _, _) = await GetAsync(client, FramePaths.Account, session);
        (HttpStatusCode otherStatus, _, HttpResponseMessage otherResponse) = await SendAsync(client, HttpMethod.Get, FramePaths.Account, other);
        using HttpResponseMessage oldSignIn = await PostAsync(client, FramePaths.SignIn, "SignIn", [("Input.Email", "ali@example.com"), ("Input.Password", Entered)], null);
        using HttpResponseMessage newSignIn = await PostAsync(client, FramePaths.SignIn, "SignIn", [("Input.Email", "ali@example.com"), ("Input.Password", NewPassword)], null);

        Assert.Equal($"{FramePaths.Account}?notice={NoticeCode.PasswordChanged}", changed.Headers.Location!.OriginalString);
        Assert.Contains(AccountTexts.PasswordChanged, html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, again.StatusCode);
        Assert.Equal(ReauthenticationAddresses.Screen(ReauthenticationOperation.PasswordChange), again.Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.OK, keptStatus);
        Assert.Equal(HttpStatusCode.Found, otherStatus);
        Assert.NotEqual(HttpStatusCode.SeeOther, oldSignIn.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, newSignIn.StatusCode);
        Assert.Contains(changed.Headers.GetValues("Set-Cookie"), c => c.StartsWith(CookieNames.KnownDevice + "=", StringComparison.Ordinal));
        otherResponse.Dispose();
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567890")]
    public async Task AWeakOrCommonPassword_Gives422_AndTheValueStaysUnspent_T517_KK6(string weak)
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        string permit = TokenOf(await ReauthenticateAsync(client, session, "PasswordChange"));

        using HttpResponseMessage refused = await PostAsync(client, FramePaths.Account, "ChangePassword", [("Input.NewPassword", weak), ("Input.ReauthenticationToken", permit)], session);
        string body = await BodyOf(refused);
        using HttpResponseMessage accepted = await PostAsync(client, FramePaths.Account, "ChangePassword", [("Input.NewPassword", NewPassword), ("Input.ReauthenticationToken", permit)], session);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Contains($"name=\"Input.ReauthenticationToken\" value=\"{permit}\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain(weak, body.Replace("1234567890", string.Empty, StringComparison.Ordinal).Replace("short", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, accepted.StatusCode);
        Assert.Equal($"{FramePaths.Account}?notice={NoticeCode.PasswordChanged}", accepted.Headers.Location!.OriginalString);
    }

    // ---- KK7: the e-mail change writes the pending change and sends the link to the new address ----

    [Fact]
    public async Task TheEmailChange_WritesThePendingChange_SendsTheLinkToTheNewAddress_AndTheAccountKeepsTheOldOne_T517_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        using HttpResponseMessage first = await ChangeEmailAsync(client, session, "yeni@example.com");
        (_, string html, _) = await GetAsync(client, first.Headers.Location!.OriginalString, session);
        using HttpResponseMessage second = await ChangeEmailAsync(client, session, "baska@example.com");

        Assert.Equal(FramePaths.Account, first.Headers.Location!.OriginalString);
        Assert.Equal(FramePaths.Account, second.Headers.Location!.OriginalString);
        Assert.Contains("yeni@example.com", html, StringComparison.Ordinal);
        Assert.Contains("id=\"account-email-pending\"", html, StringComparison.Ordinal);
        Assert.Contains(AccountTexts.ResendLinkButton, html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Customer account = await db.Customers.AsNoTracking().SingleAsync(Ct);
        PendingEmailChange pending = await db.PendingEmailChanges.AsNoTracking().SingleAsync(Ct);
        Assert.Equal("ali@example.com", account.Email);
        Assert.Equal("baska@example.com", pending.NewEmail);
        List<LinkRecord> links = await db.LinkRecords.AsNoTracking().Where(l => l.LinkType == LinkType.NewEmailVerification).OrderBy(l => l.CreatedAt).ToListAsync(Ct);
        Assert.Equal(2, links.Count);
        Assert.NotNull(links[0].SupersededAt);
        Assert.Null(links[1].SupersededAt);
        Assert.Equal(["baska@example.com", "yeni@example.com"], (await db.DispatchRecords.AsNoTracking().Where(d => d.DispatchType == DispatchType.NewEmailVerificationLink).ToListAsync(Ct)).Select(d => d.RecipientEmail!).Order().ToArray());
    }

    // ---- KK8: three 422 reasons, L-9 only when replacing, the value is not spent by a refusal ----

    [Fact]
    public async Task TheEmailChange_RefusesABrokenAddress_AVerifiedOthersAddress_ButNotAnUnverifiedOnes_AndSpendsNoValue_T517_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        await SeedAsync("tutulu@example.com", hasPassword: true, googleLinked: false);
        await SeedAsync("bekleyen@example.com", hasPassword: true, googleLinked: false, verified: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        string permit = TokenOf(await ReauthenticateAsync(client, session, "EmailChange"));

        using HttpResponseMessage broken = await PostAsync(client, FramePaths.Account, "ChangeEmail", [("Input.NewEmail", "not-an-address"), ("Input.ReauthenticationToken", permit)], session);
        using HttpResponseMessage taken = await PostAsync(client, FramePaths.Account, "ChangeEmail", [("Input.NewEmail", "Tutulu@Example.com"), ("Input.ReauthenticationToken", permit)], session);
        string takenBody = await BodyOf(taken);
        Assert.NotNull((await ReadSessionAsync("ali@example.com")).ReauthenticationTokenHash);
        using HttpResponseMessage unverified = await PostAsync(client, FramePaths.Account, "ChangeEmail", [("Input.NewEmail", "bekleyen@example.com"), ("Input.ReauthenticationToken", permit)], session);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, broken.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, taken.StatusCode);
        Assert.Contains(AccountTexts.EmailTaken, takenBody, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.ReauthenticationToken\" value=\"{permit}\"", takenBody, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, unverified.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.Equal("bekleyen@example.com", (await db.PendingEmailChanges.AsNoTracking().SingleAsync(Ct)).NewEmail);
    }

    [Fact]
    public async Task TheEmailChange_Gives429OnlyWhenItReplacesAPendingChange_WithTheNeutralSentence_AndTheValueStaysUnspent_T517_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        using (HttpResponseMessage first = await ChangeEmailAsync(client, session, "bir@example.com"))
        {
            Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        }

        await using (AppDbContext db = NewContext())
        {
            Assert.Equal(0, await db.AttemptRecords.CountAsync(a => a.AttemptLimit == AttemptLimit.L9, Ct));
        }

        HttpStatusCode last = HttpStatusCode.OK;
        string body = string.Empty;
        string permit = string.Empty;
        for (int i = 0; i <= ProductConstants.VerificationResendAttemptsPerEmail + 1 && last != HttpStatusCode.TooManyRequests; i++)
        {
            permit = TokenOf(await ReauthenticateAsync(client, session, "EmailChange"));
            using HttpResponseMessage attempt = await PostAsync(client, FramePaths.Account, "ChangeEmail", [("Input.NewEmail", $"x{i}@example.com"), ("Input.ReauthenticationToken", permit)], session);
            last = attempt.StatusCode;
            body = await BodyOf(attempt);
            Assert.False(attempt.Headers.Contains("Retry-After"));
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last);
        Assert.Contains(NeutralMessages.LimitExceeded, body, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.ReauthenticationToken\" value=\"{permit}\"", body, StringComparison.Ordinal);
        Assert.NotNull((await ReadSessionAsync()).ReauthenticationTokenHash);
    }

    // ---- KK9: the link asked for again ----

    [Fact]
    public async Task TheLinkAskedForAgain_IsNew_TheEarlierOnesAreSuperseded_AndTheLifeIsNotLengthened_T517_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        using (await ChangeEmailAsync(client, session, "yeni@example.com"))
        {
        }

        await using AppDbContext before = NewContext();
        DateTimeOffset firstEnd = (await before.LinkRecords.AsNoTracking().SingleAsync(l => l.LinkType == LinkType.NewEmailVerification, Ct)).ExpiresAt;

        using HttpResponseMessage resent = await PostAsync(client, FramePaths.Account, "ResendEmailChangeLink", [], session);
        (_, string html, _) = await GetAsync(client, resent.Headers.Location!.OriginalString, session);

        Assert.Equal($"{FramePaths.Account}?notice={NoticeCode.LinkResent}", resent.Headers.Location!.OriginalString);
        Assert.Contains(RegistrationTexts.LinkResent(firstEnd), html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        List<LinkRecord> links = await db.LinkRecords.AsNoTracking().Where(l => l.LinkType == LinkType.NewEmailVerification).OrderBy(l => l.CreatedAt).ToListAsync(Ct);
        Assert.Equal(2, links.Count);
        Assert.NotNull(links[0].SupersededAt);
        Assert.Null(links[1].SupersededAt);
        Assert.Equal(firstEnd, links[1].ExpiresAt);
    }

    [Fact]
    public async Task TheLinkAskedForAgain_WithNothingPendingIs422_AndTooManyAskingsGive429_T517_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        using HttpResponseMessage nothing = await PostAsync(client, FramePaths.Account, "ResendEmailChangeLink", [], session);
        using (await ChangeEmailAsync(client, session, "yeni@example.com"))
        {
        }

        HttpStatusCode last = HttpStatusCode.OK;
        for (int i = 0; i <= ProductConstants.VerificationResendAttemptsPerEmail + 2 && last != HttpStatusCode.TooManyRequests; i++)
        {
            using HttpResponseMessage again = await PostAsync(client, FramePaths.Account, "ResendEmailChangeLink", [], session);
            last = again.StatusCode;
        }

        Assert.Equal(HttpStatusCode.UnprocessableEntity, nothing.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, last);
    }

    // ---- KK10: the deletion ----

    [Fact]
    public async Task TheDeletion_WithoutTheMark_IsTheLastStep200_WritesNothing_AndSpendsNoValue_T517_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        string address = await ReauthenticateAsync(client, session, "AccountDeletion");
        string permit = TokenOf(address);

        (_, string opened, _) = await GetAsync(client, address, session);
        using HttpResponseMessage unconfirmed = await PostAsync(client, FramePaths.Account, "DeleteAccount", [("Input.ReauthenticationToken", permit)], session);
        string body = await BodyOf(unconfirmed);

        foreach (string html in new[] { opened, body })
        {
            Assert.Contains(AccountTexts.DeleteSentence, html, StringComparison.Ordinal);
            Assert.Contains("Bu işlem geri alınamaz.", html, StringComparison.Ordinal);
            Assert.Contains(AccountTexts.DeleteConfirmLabel, html, StringComparison.Ordinal);
            Assert.Contains(ConfirmView.CancelLabel, html, StringComparison.Ordinal);
            Assert.Contains("name=\"Input.Confirmed\" value=\"true\"", html, StringComparison.Ordinal);
        }

        Assert.Equal(HttpStatusCode.OK, unconfirmed.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Customers.CountAsync(Ct));
        Assert.NotNull((await ReadSessionAsync()).ReauthenticationTokenHash);
    }

    [Fact]
    public async Task TheConfirmedDeletion_DeletesInOneTransaction_MarksTheOrder_Writes303_AndTheResultStateShowsOnce_T517_KK10()
    {
        List<AuditLine> lines = [];
        await using WebApplicationFactory<Program> factory = Open(lines);
        Guid id = await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");
        string permit = TokenOf(await ReauthenticateAsync(client, session, "AccountDeletion"));

        using HttpResponseMessage deleted = await PostAsync(client, FramePaths.Account, "DeleteAccount", [("Input.ReauthenticationToken", permit), ("Input.Confirmed", "true")], session);
        using HttpResponseMessage again = await PostAsync(client, FramePaths.Account, "DeleteAccount", [("Input.ReauthenticationToken", permit), ("Input.Confirmed", "true")], session);
        (HttpStatusCode shown, string html, IReadOnlyList<string> shownCookies) = await GetAsync(client, FramePaths.AccountDeleted, session);
        (HttpStatusCode reloaded, _, _) = await GetAsync(client, FramePaths.AccountDeleted, null);

        Assert.Equal(FramePaths.AccountDeleted, deleted.Headers.Location!.OriginalString);
        Assert.DoesNotContain(deleted.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? set) ? set : [], c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal));
        Assert.Equal(HttpStatusCode.Found, again.StatusCode);
        Assert.Equal(HttpStatusCode.OK, shown);
        Assert.Contains(AccountDeletedTexts.Deleted, html, StringComparison.Ordinal);
        Assert.Contains(AccountDeletedTexts.OrdersGoOn, html, StringComparison.Ordinal);
        Assert.Contains(shownCookies, c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal) && c.Contains("expires=", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(HttpStatusCode.SeeOther, reloaded);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Customers.CountAsync(Ct));
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
        Assert.Equal(1, await db.DispatchRecords.CountAsync(d => d.DispatchType == DispatchType.AccountDeletedNotice, Ct));
        AuditLine line = Assert.Single(lines);
        Assert.Equal(InstantDeletionKinds.CustomerAccount, line.Name);
        Assert.Equal(id, line.Id);
    }

    [Fact]
    public async Task TheResultState_GoesHome_WithoutTheSignOrWithALiveSession_T517_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        (HttpStatusCode withoutCookie, _, _) = await GetAsync(client, FramePaths.AccountDeleted, null);
        (HttpStatusCode liveSession, _, _) = await GetAsync(client, FramePaths.AccountDeleted, session);
        (HttpStatusCode unknownCookie, string html, _) = await GetAsync(client, FramePaths.AccountDeleted, $"{CookieNames.Session}=a-key-that-points-to-no-session");

        Assert.Equal(HttpStatusCode.SeeOther, withoutCookie);
        Assert.Equal(HttpStatusCode.SeeOther, liveSession);
        Assert.Equal(HttpStatusCode.OK, unknownCookie);
        Assert.Contains(AccountDeletedTexts.EmailSent, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheDeletion_NeedsTheValue_AndAMissingOneGoesToTheReauthentication_T517_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAsync("ali@example.com", hasPassword: true, googleLinked: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string session = await SignInAsync(client, "ali@example.com");

        using HttpResponseMessage none = await PostAsync(client, FramePaths.Account, "DeleteAccount", [("Input.Confirmed", "true")], session);

        Assert.Equal(ReauthenticationAddresses.Screen(ReauthenticationOperation.AccountDeletion), none.Headers.Location!.OriginalString);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Customers.CountAsync(Ct));
    }

    // ---- helpers ----

    private WebApplicationFactory<Program> Open(List<AuditLine>? audit = null) =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddRazorPages().AddApplicationPart(typeof(Shopfolio.IntegrationTests.Pages.FrameSampleModel).Assembly);
            if (audit is not null)
            {
                s.Replace(ServiceDescriptor.Singleton<IAuditLineWriter>(new SpyWriter(audit)));
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

    // An open revert link of an earlier e-mail change (Z-45): the e-mail change and the deletion are closed until it ends.
    private async Task<DateTimeOffset> SeedRevertLinkAsync(WebApplicationFactory<Program> factory, Guid customerId)
    {
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset ends = now.AddDays(3);
        LinkRecord link = LinkRecord.ForCustomer(keys, LinkType.EmailChangeRevert, customerId, ends, now.AddDays(-1));
        link.PreviousEmail = "eski@example.com";
        await using AppDbContext db = NewContext();
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return ends;
    }

    // A session row of an account without a password, as a Google sign-in leaves it (UC-51): the raw key is made here and only its digest is stored.
    private async Task<string> SeedGoogleSessionAsync(Guid customerId)
    {
        string key = SecretKey.Create();
        await using AppDbContext db = NewContext();
        db.Sessions.Add(Session.ForCustomer(customerId, SecretKey.Hash(key), false, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(Ct);
        return $"{CookieNames.Session}={key}";
    }

    private async Task<Session> ReadSessionAsync(string? email = null)
    {
        await using AppDbContext db = NewContext();
        return await db.Sessions.AsNoTracking().OrderByDescending(s => s.CreatedAt).FirstAsync(Ct);
    }

    private static async Task<string> SignInAsync(HttpClient client, string email)
    {
        using HttpResponseMessage response = await PostAsync(client, FramePaths.SignIn, "SignIn", [("Input.Email", email), ("Input.Password", Entered)], null);
        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        return ValueOf(Assert.Single(SetCookies(response), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal)));
    }

    // The re-authentication through the application's own screen: the address it answers carries the operation and the single-use value.
    private static async Task<string> ReauthenticateAsync(HttpClient client, string session, string operation)
    {
        using HttpResponseMessage response = await PostAsync(client, FramePaths.Reauthentication, "Reauthenticate", [("Input.ReauthenticatedOperation", operation), ("Input.Password", Entered)], session);
        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        return response.Headers.Location!.OriginalString;
    }

    private static string TokenOf(string address) => QueryHelpers.ParseQuery(new Uri("https://localhost" + address).Query)[ReauthenticationAddresses.TokenParameter].ToString();

    private static async Task<HttpResponseMessage> ChangeEmailAsync(HttpClient client, string session, string newEmail)
    {
        string permit = TokenOf(await ReauthenticateAsync(client, session, "EmailChange"));
        return await PostAsync(client, FramePaths.Account, "ChangeEmail", [("Input.NewEmail", newEmail), ("Input.ReauthenticationToken", permit)], session);
    }

    private static async Task<(HttpStatusCode Status, string Html, IReadOnlyList<string> SetCookies)> GetAsync(HttpClient client, string path, string? cookie)
    {
        (HttpStatusCode status, _, HttpResponseMessage response) = await SendAsync(client, HttpMethod.Get, path, cookie);
        using (response)
        {
            return (status, await BodyOf(response), SetCookies(response));
        }
    }

    private static async Task<(HttpStatusCode Status, string Html, HttpResponseMessage Response)> SendAsync(HttpClient client, HttpMethod method, string path, string? cookie)
    {
        using HttpRequestMessage request = new(method, path);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, string.Empty, response);
    }

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

    private sealed class SpyWriter(List<AuditLine> lines) : IAuditLineWriter
    {
        public void Write(AuditLine line)
        {
            lock (lines)
            {
                lines.Add(line);
            }
        }
    }
}
