using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the panel home and E-54 tests (T521): one database copied from the template.</summary>
[CollectionDefinition(PanelHomeAndVerificationPagesTestsDefinition.Name)]
public sealed class PanelHomeAndVerificationPagesTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "PanelHomeAndVerificationPagesTests";
}

/// <summary>
/// The panel home (E-31; 07 UC-86) and the administrator account's verification screens (E-54; 07 UC-108, UC-185 to UC-187) on the opened application (T521 KK1-KK10): what each address draws, what each
/// handler answers (303, 200, 422, 429) and what it writes. Accounts, sessions, links and pending changes are real rows of the collection database.
/// </summary>
[Collection(PanelHomeAndVerificationPagesTestsDefinition.Name)]
public sealed class PanelHomeAndVerificationPagesTests : IAsyncLifetime
{
    private const string Strong = "a-long-enough-password";
    private const string HomePath = "/panel";
    private const string ReauthPath = "/panel/yeniden-dogrulama";
    private const string VerifyPath = "/panel/dogrulama";
    private const string RevertPath = "/panel/geri-al";
    private const string SignInPath = "/panel/giris";

    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static readonly Lazy<string> StrongDigest = new(() => new PasswordService().Hash(Strong));

    private readonly CollectionDatabaseFixture _database;

    public PanelHomeAndVerificationPagesTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE attempt_records, login_records, dispatch_records", Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1, KK2: the home ----

    [Fact]
    public async Task TheHome_DrawsTheSixCounters_ZeroIncluded_AndLeavesAnEmptySectionOut_T521_KK1()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html, _) = await GetAsync(client, HomePath, AdminCookie(key));

        Assert.Equal(HttpStatusCode.OK, status);
        foreach (string name in new[] { "Ödeme onayı bekleyen", "Kargoya verilecek", "Teslim işareti bekleyen", "Tamamlanmayı bekleyen hizmet", "Açık talep", "İade ve geri ödeme bekleyen" })
        {
            Assert.Contains(name, html, StringComparison.Ordinal);
        }

        Assert.Equal(6, Regex.Count(html, "pending-counters__item", RegexOptions.None, TimeSpan.FromSeconds(5)));
        Assert.DoesNotContain("Müşteriden beklenenler", html, StringComparison.Ordinal);
        Assert.DoesNotContain("home-warning-email-undelivered", html, StringComparison.Ordinal);
        Assert.DoesNotContain("home-warning-card-refund", html, StringComparison.Ordinal);

        // The empty installation lacks both legal texts: the line about the closed entrances stands once, under the notice item (04 9.3.4; K-850).
        Assert.Equal(1, Regex.Count(html, Regex.Escape(PanelHomeTexts.ClosedEntrances), RegexOptions.None, TimeSpan.FromSeconds(5)));

        // The order of the sections: warnings, then the checklist, then the counters.
        Assert.True(html.IndexOf("home-warning-sales", StringComparison.Ordinal) < html.IndexOf("setup-checklist-title", StringComparison.Ordinal));
        Assert.True(html.IndexOf("setup-checklist-title", StringComparison.Ordinal) < html.IndexOf("pending-counters-title", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ACounter_ChangesWhenTheRecordsChange_AndIsNotStored_T521_KK2()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string before, _) = await GetAsync(client, HomePath, AdminCookie(key));
        await TestSessions.OrderAsync(_database.Dsn, null);
        (_, string after, _) = await GetAsync(client, HomePath, AdminCookie(key));

        Assert.Equal(CounterOf(before, "Ödeme onayı bekleyen") + 1, CounterOf(after, "Ödeme onayı bekleyen"));
        Assert.Contains("href=\"/panel/siparisler?pendingWork=AwaitingPayment\"", after, StringComparison.Ordinal);
    }

    // ---- KK4, KK5: the re-verification ----

    [Theory]
    [InlineData("")]
    [InlineData("?reauthenticatedOperation=AccountDeletion")]
    [InlineData("?reauthenticatedOperation=Nothing")]
    public async Task TheReauthenticationPage_Redirects_ToTheAccountPage_ForNoOperationOrOneOutsideTheList_T521_KK4(string query)
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, _, string? location) = await GetWithLocationAsync(client, ReauthPath + query, AdminCookie(key));

        Assert.Equal(HttpStatusCode.SeeOther, status);
        Assert.Equal("/panel/hesabim", location);
    }

    [Theory]
    [InlineData("PasswordChange", "Şifrenizi değiştirmek için")]
    [InlineData("EmailChange", "E-posta adresinizi değiştirmek için")]
    public async Task TheReauthenticationPage_NamesTheOperation_AsksThePassword_HasNoGoogle_AndDropsTheEarlierPermission_T521_KK4(string operation, string line)
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        (string sessionKey, Guid sessionId) = await SeedSessionAsync(adminId, grantedOperation: ReauthenticationOperation.PasswordChange);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html, _) = await GetAsync(client, $"{ReauthPath}?reauthenticatedOperation={operation}", AdminCookie(sessionKey));

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(line, html, StringComparison.Ordinal);
        Assert.Contains("type=\"password\"", html, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.ReauthenticatedOperation\" value=\"{operation}\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Google", html, StringComparison.Ordinal);
        Assert.DoesNotContain(email, html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Session session = await db.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct);
        Assert.Null(session.ReauthenticatedOperation);
        Assert.Null(session.ReauthenticationTokenHash);
    }

    [Fact]
    public async Task TheRightPassword_GrantsOneSingleUseValue_AndTheOperationOutsideTheListIsRefused_T521_KK5()
    {
        (_, Guid adminId) = await SeedAdminAsync();
        (string sessionKey, Guid sessionId) = await SeedSessionAsync(adminId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode done, string? location, _) = await PostAsync(client, $"{ReauthPath}?handler=Reauthenticate", [("Input.Password", Strong), ("Input.ReauthenticatedOperation", "EmailChange")], AdminCookie(sessionKey));
        (HttpStatusCode outside, string? outsideLocation, _) = await PostAsync(client, $"{ReauthPath}?handler=Reauthenticate", [("Input.Password", Strong), ("Input.ReauthenticatedOperation", "AccountDeletion")], AdminCookie(sessionKey));

        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.StartsWith("/panel/hesabim?reauthenticatedOperation=EmailChange&reauthenticationToken=", location, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, outside);
        Assert.Equal("/panel/hesabim", outsideLocation);
        await using AppDbContext db = NewContext();
        Session session = await db.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct);
        Assert.Equal(ReauthenticationOperation.EmailChange, session.ReauthenticatedOperation);
        Assert.NotNull(session.ReauthenticationTokenHash);
    }

    [Fact]
    public async Task AWrongOrEmptyPassword_Is422_CountsForTheLimit_AndAtTheLimit429DoesNothing_T521_KK5()
    {
        (_, Guid adminId) = await SeedAdminAsync();
        (string sessionKey, Guid sessionId) = await SeedSessionAsync(adminId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string[] fields = ["Input.ReauthenticatedOperation", "PasswordChange"];

        (HttpStatusCode empty, string emptyHtml) = await PostBodyAsync(client, $"{ReauthPath}?handler=Reauthenticate", [("Input.Password", ""), (fields[0], fields[1])], AdminCookie(sessionKey));
        (HttpStatusCode wrong, string wrongHtml) = await PostBodyAsync(client, $"{ReauthPath}?handler=Reauthenticate", [("Input.Password", "not-the-password"), (fields[0], fields[1])], AdminCookie(sessionKey));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, wrong);
        Assert.Contains("Şifrenizi değiştirmek için", emptyHtml, StringComparison.Ordinal);
        Assert.Contains("Şifrenizi değiştirmek için", wrongHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("not-the-password", wrongHtml, StringComparison.Ordinal);

        HttpStatusCode last = wrong;
        for (int i = 0; i < ProductConstants.SignInAttemptsPerEmail + 1 && last != HttpStatusCode.TooManyRequests; i++)
        {
            (last, _) = await PostBodyAsync(client, $"{ReauthPath}?handler=Reauthenticate", [("Input.Password", "not-the-password"), (fields[0], fields[1])], AdminCookie(sessionKey));
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last);
        (HttpStatusCode limited, _) = await PostBodyAsync(client, $"{ReauthPath}?handler=Reauthenticate", [("Input.Password", Strong), (fields[0], fields[1])], AdminCookie(sessionKey));
        Assert.Equal(HttpStatusCode.TooManyRequests, limited);
        await using AppDbContext db = NewContext();
        Assert.Null((await db.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct)).ReauthenticatedOperation);
    }

    // ---- KK6, KK7, KK8: the new address's landing ----

    [Fact]
    public async Task TheVerificationLanding_HasThreeStates_ReadsFromTheLinkRecord_AndOpeningChangesNothing_T521_KK6()
    {
        (string oldEmail, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        string valid = await SeedVerificationAsync(keys, adminId, "new-" + oldEmail, usedAt: null, expiresAt: DateTimeOffset.UtcNow.AddHours(1));
        string used = await SeedVerificationAsync(keys, adminId, "other-" + oldEmail, usedAt: DateTimeOffset.UtcNow.AddMinutes(-1), expiresAt: DateTimeOffset.UtcNow.AddHours(1));

        (HttpStatusCode validStatus, string validHtml, _) = await GetAsync(client, $"{VerifyPath}/{valid}");
        (_, string usedWithCode, _) = await GetAsync(client, $"{VerifyPath}/{used}?{NoticeParameter.ParameterName}={NoticeCode.EmailVerified}");
        (_, string usedNoCode, _) = await GetAsync(client, $"{VerifyPath}/{used}");
        (_, string validWithCode, _) = await GetAsync(client, $"{VerifyPath}/{valid}?{NoticeParameter.ParameterName}={NoticeCode.EmailVerified}");
        (HttpStatusCode unknownStatus, string unknown, _) = await GetAsync(client, $"{VerifyPath}/not-a-link-key");

        Assert.Equal(HttpStatusCode.OK, validStatus);
        Assert.Contains(PanelAccessTexts.VerifyEmailValid, validHtml, StringComparison.Ordinal);
        Assert.Contains("handler=VerifyEmail", validHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("panel-top", validHtml, StringComparison.Ordinal);
        Assert.Contains(PanelAccessTexts.EmailVerified, usedWithCode, StringComparison.Ordinal);
        Assert.Contains(PanelAccessTexts.VerifyEmailInvalid, usedNoCode, StringComparison.Ordinal);
        Assert.DoesNotContain(PanelAccessTexts.EmailVerified, validWithCode, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, unknownStatus);
        Assert.Contains(PanelAccessTexts.VerifyEmailInvalid, unknown, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(oldEmail, await db.Admins.Where(a => a.Id == adminId).Select(a => a.NormalizedEmail).SingleAsync(Ct));
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
    }

    [Fact]
    public async Task TheVerificationButton_ConsumesTheLinkOnce_ChangesTheAddress_NotifiesTheOldOne_AndWritesTheTrail_T521_KK7()
    {
        (string oldEmail, Guid adminId) = await SeedAdminAsync();
        (string sessionKey, _) = await SeedSessionAsync(adminId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string newEmail = "new-" + oldEmail;
        string key = await SeedVerificationAsync(factory.Services.GetRequiredService<LinkKeys>(), adminId, newEmail, null, DateTimeOffset.UtcNow.AddHours(1));

        (HttpStatusCode first, string? location, List<string> cookies) = await PostAsync(client, $"{VerifyPath}/{key}?handler=VerifyEmail", []);
        (HttpStatusCode second, string secondHtml) = await PostBodyAsync(client, $"{VerifyPath}/{key}?handler=VerifyEmail", []);

        Assert.Equal(HttpStatusCode.SeeOther, first);
        Assert.Equal($"{VerifyPath}/{key}?{NoticeParameter.ParameterName}={NoticeCode.EmailVerified}", location);
        Assert.DoesNotContain(cookies, c => c.StartsWith(CookieNames.AdminSession + "=", StringComparison.Ordinal));
        Assert.Equal(HttpStatusCode.OK, second);
        Assert.Contains(PanelAccessTexts.VerifyEmailInvalid, secondHtml, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(newEmail, await db.Admins.Where(a => a.Id == adminId).Select(a => a.NormalizedEmail).SingleAsync(Ct));
        Assert.False(await db.PendingEmailChanges.AnyAsync(p => p.AdminId == adminId, Ct));
        Assert.True(await db.Sessions.AnyAsync(s => s.TokenHash == SecretKey.Hash(sessionKey), Ct));
        DispatchRecord notice = await db.DispatchRecords.AsNoTracking().SingleAsync(d => d.DispatchType == DispatchType.EmailChangeNotice, Ct);
        Assert.Equal(oldEmail, notice.RecipientEmail);
        Assert.Equal(1, await db.LinkRecords.CountAsync(l => l.AdminId == adminId && l.LinkType == LinkType.EmailChangeRevert, Ct));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.Operation == "ChangeAdminEmail" && a.AdminId == adminId, Ct));
    }

    [Fact]
    public async Task TheVerificationButton_Is422_WhenTheNewAddressBecameAnotherAdministrators_AndTheLinkIsNotConsumed_T521_KK8()
    {
        (string oldEmail, Guid adminId) = await SeedAdminAsync();
        (string otherEmail, _) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string key = await SeedVerificationAsync(factory.Services.GetRequiredService<LinkKeys>(), adminId, otherEmail, null, DateTimeOffset.UtcNow.AddHours(1));

        (HttpStatusCode status, string html) = await PostBodyAsync(client, $"{VerifyPath}/{key}?handler=VerifyEmail", []);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains(PanelAccessTexts.VerifyEmailTaken, html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(oldEmail, await db.Admins.Where(a => a.Id == adminId).Select(a => a.NormalizedEmail).SingleAsync(Ct));
        Assert.Null(await db.LinkRecords.Where(l => l.AdminId == adminId && l.LinkType == LinkType.NewEmailVerification).Select(l => l.UsedAt).SingleAsync(Ct));
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
    }

    [Fact]
    public async Task TheVerificationButton_WritesNothing_WhenTheLinkIsSupersededWhileItWaitsForTheAccountLock_T521_KK8()
    {
        (string oldEmail, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string key = await SeedVerificationAsync(factory.Services.GetRequiredService<LinkKeys>(), adminId, "new-" + oldEmail, null, DateTimeOffset.UtcNow.AddHours(1));

        (HttpStatusCode status, string html) = await RaceTheVerificationAsync(client, key, adminId, async holder =>
            await holder.LinkRecords.Where(l => l.AdminId == adminId && l.LinkType == LinkType.NewEmailVerification).ExecuteUpdateAsync(set => set.SetProperty(l => l.SupersededAt, DateTimeOffset.UtcNow), Ct));

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(PanelAccessTexts.VerifyEmailInvalid, html, StringComparison.Ordinal);
        await AssertNothingWrittenAsync(adminId, oldEmail);
    }

    [Fact]
    public async Task TheVerificationButton_WritesNothing_WhenThePendingChangeGoesWhileItWaitsForTheAccountLock_T521_KK8()
    {
        (string oldEmail, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string key = await SeedVerificationAsync(factory.Services.GetRequiredService<LinkKeys>(), adminId, "new-" + oldEmail, null, DateTimeOffset.UtcNow.AddHours(1));

        (HttpStatusCode status, string html) = await RaceTheVerificationAsync(client, key, adminId, async holder =>
            await holder.PendingEmailChanges.Where(p => p.AdminId == adminId).ExecuteDeleteAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(PanelAccessTexts.VerifyEmailInvalid, html, StringComparison.Ordinal);
        await AssertNothingWrittenAsync(adminId, oldEmail);
    }

    // Holds the administrator's row lock, lets the button's POST read the link and block on the lock, changes the records under the lock, then releases it.
    private async Task<(HttpStatusCode Status, string Html)> RaceTheVerificationAsync(HttpClient client, string key, Guid adminId, Func<AppDbContext, Task> changeUnderTheLock)
    {
        await using AppDbContext holder = NewContext();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await holder.Database.BeginTransactionAsync(Ct);
        await holder.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM admins WHERE id = {adminId} FOR UPDATE", Ct);

        Task<(HttpStatusCode Status, string Html)> post = PostBodyAsync(client, $"{VerifyPath}/{key}?handler=VerifyEmail", []);
        await using AppDbContext probe = NewContext();
        int waiting = 0;
        for (int i = 0; i < 400 && waiting == 0; i++)
        {
            await Task.Delay(50, Ct);
            waiting = await probe.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM pg_stat_activity WHERE wait_event_type = 'Lock' AND query ILIKE '%FOR UPDATE%' AND datname = current_database()").SingleAsync(Ct);
        }

        Assert.True(waiting > 0, "The button's request never waited for the account lock.");
        await changeUnderTheLock(holder);
        await transaction.CommitAsync(Ct);
        return await post;
    }

    private async Task AssertNothingWrittenAsync(Guid adminId, string oldEmail)
    {
        await using AppDbContext db = NewContext();
        Assert.Equal(oldEmail, await db.Admins.Where(a => a.Id == adminId).Select(a => a.NormalizedEmail).SingleAsync(Ct));
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
        Assert.Equal(0, await db.LinkRecords.CountAsync(l => l.AdminId == adminId && l.LinkType == LinkType.EmailChangeRevert, Ct));
        Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.Operation == "ChangeAdminEmail" && a.AdminId == adminId, Ct));
        Assert.Null(await db.LinkRecords.Where(l => l.AdminId == adminId && l.LinkType == LinkType.NewEmailVerification).Select(l => l.UsedAt).SingleAsync(Ct));
    }

    // ---- KK6, KK9, KK10: the revert landing ----

    [Fact]
    public async Task TheRevertLanding_HasThreeStates_AndOpeningChangesNothing_T521_KK6()
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        string valid = await SeedRevertAsync(keys, adminId, "previous-" + email, null);
        string used = await SeedRevertAsync(keys, adminId, "previous2-" + email, DateTimeOffset.UtcNow.AddMinutes(-1));

        (_, string validHtml, _) = await GetAsync(client, $"{RevertPath}/{valid}");
        (_, string done, _) = await GetAsync(client, $"{RevertPath}/{used}?{NoticeParameter.ParameterName}={NoticeCode.EmailChangeReverted}");
        (_, string usedNoCode, _) = await GetAsync(client, $"{RevertPath}/{used}");
        (_, string unknown, _) = await GetAsync(client, $"{RevertPath}/not-a-link-key");

        Assert.Contains(PanelAccessTexts.RevertValid, validHtml, StringComparison.Ordinal);
        Assert.Contains("handler=RevertPendingEmailChange", validHtml, StringComparison.Ordinal);
        Assert.Contains(PanelAccessTexts.EmailReverted, done, StringComparison.Ordinal);
        Assert.Contains(PanelAccessTexts.RevertInvalid, usedNoCode, StringComparison.Ordinal);
        Assert.Contains(PanelAccessTexts.RevertInvalid, unknown, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(email, await db.Admins.Where(a => a.Id == adminId).Select(a => a.NormalizedEmail).SingleAsync(Ct));
    }

    [Fact]
    public async Task TheRevertLanding_ShowsTheNextStepInEveryState_ToMyAccountWhenASessionIsOpenElseToTheSignIn_T521_KK6()
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        (string otherEmail, Guid otherId) = await SeedAdminAsync();
        (string otherSession, _) = await SeedSessionAsync(otherId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        string used = await SeedRevertAsync(keys, adminId, "previous-" + email, DateTimeOffset.UtcNow.AddMinutes(-1));
        string taken = await SeedRevertAsync(keys, adminId, otherEmail, null);
        string signIn = $"href=\"{PanelFramePaths.SignIn}\"";
        string myAccount = $"href=\"{PanelFramePaths.MyAccount}\"";

        (_, string invalidClosed, _) = await GetAsync(client, $"{RevertPath}/{used}");
        (_, string invalidOpen, _) = await GetAsync(client, $"{RevertPath}/{used}", AdminCookie(otherSession));
        (_, string doneClosed, _) = await GetAsync(client, $"{RevertPath}/{used}?{NoticeParameter.ParameterName}={NoticeCode.EmailChangeReverted}");
        (_, string doneOpen, _) = await GetAsync(client, $"{RevertPath}/{used}?{NoticeParameter.ParameterName}={NoticeCode.EmailChangeReverted}", AdminCookie(otherSession));
        (HttpStatusCode notAppliedStatus, string notAppliedClosed) = await PostBodyAsync(client, $"{RevertPath}/{taken}?handler=RevertPendingEmailChange", []);
        (_, string notAppliedOpen) = await PostBodyAsync(client, $"{RevertPath}/{taken}?handler=RevertPendingEmailChange", [], AdminCookie(otherSession));

        Assert.Contains(PanelAccessTexts.RevertInvalid, invalidClosed, StringComparison.Ordinal);
        Assert.Contains(signIn, invalidClosed, StringComparison.Ordinal);
        Assert.Contains(myAccount, invalidOpen, StringComparison.Ordinal);
        Assert.Contains(signIn, doneClosed, StringComparison.Ordinal);
        Assert.Contains(myAccount, doneOpen, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, notAppliedStatus);
        Assert.Contains(PanelAccessTexts.RevertNotApplied, notAppliedClosed, StringComparison.Ordinal);
        Assert.Contains(signIn, notAppliedClosed, StringComparison.Ordinal);
        Assert.Contains(PanelAccessTexts.RevertNotApplied, notAppliedOpen, StringComparison.Ordinal);
        Assert.Contains(myAccount, notAppliedOpen, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheRevertButton_DeletesTheBrowsersCookieOnlyWhenItIsTheRevertedAccountsOwnSession_T521_KK9()
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        _ = await SeedSessionAsync(adminId);
        (_, Guid otherId) = await SeedAdminAsync();
        (string otherSession, _) = await SeedSessionAsync(otherId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        string byOther = await SeedRevertAsync(keys, adminId, "previous-" + email, null);

        // Another administrator's browser opens the link and presses the button: the revert happens, that administrator's cookie and session stay.
        (HttpStatusCode first, _, List<string> otherCookies) = await PostAsync(client, $"{RevertPath}/{byOther}?handler=RevertPendingEmailChange", [], AdminCookie(otherSession));
        Assert.Equal(HttpStatusCode.SeeOther, first);
        Assert.DoesNotContain(otherCookies, c => c.StartsWith(CookieNames.AdminSession + "=", StringComparison.Ordinal));
        await using AppDbContext db = NewContext();
        Assert.True(await db.Sessions.AnyAsync(s => s.AdminId == otherId, Ct));
        Assert.False(await db.Sessions.AnyAsync(s => s.AdminId == adminId, Ct));

        // The landing after the revert leads that administrator to "Hesabım".
        (_, string landing, _) = await GetAsync(client, $"{RevertPath}/{byOther}?{NoticeParameter.ParameterName}={NoticeCode.EmailChangeReverted}", AdminCookie(otherSession));
        Assert.Contains($"href=\"{PanelFramePaths.MyAccount}\"", landing, StringComparison.Ordinal);

        // The reverted account's own browser: its session ended with the revert and its cookie goes.
        (string ownEmail, Guid ownId) = await SeedAdminAsync();
        (string ownKey, _) = await SeedSessionAsync(ownId);
        string byOwner = await SeedRevertAsync(keys, ownId, "previous-" + ownEmail, null);
        (HttpStatusCode second, _, List<string> ownCookies) = await PostAsync(client, $"{RevertPath}/{byOwner}?handler=RevertPendingEmailChange", [], AdminCookie(ownKey));
        Assert.Equal(HttpStatusCode.SeeOther, second);
        Assert.Contains(ownCookies, c => c.StartsWith(CookieNames.AdminSession + "=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TheRevertButton_ReturnsTheAddress_EndsEverySession_InvalidatesThePassword_SendsTheResetLink_AndWritesTheTrail_T521_KK9()
    {
        (string currentEmail, Guid adminId) = await SeedAdminAsync();
        (string sessionKey, _) = await SeedSessionAsync(adminId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string previous = "previous-" + currentEmail;
        string key = await SeedRevertAsync(factory.Services.GetRequiredService<LinkKeys>(), adminId, previous, null);

        (HttpStatusCode first, string? location, _) = await PostAsync(client, $"{RevertPath}/{key}?handler=RevertPendingEmailChange", [], AdminCookie(sessionKey));
        (HttpStatusCode second, string secondHtml) = await PostBodyAsync(client, $"{RevertPath}/{key}?handler=RevertPendingEmailChange", []);
        (HttpStatusCode oldPassword, _) = await PostBodyAsync(client, $"{SignInPath}?handler=SignIn", [("Input.Email", previous), ("Input.Password", Strong)]);

        Assert.Equal(HttpStatusCode.SeeOther, first);
        Assert.Equal($"{RevertPath}/{key}?{NoticeParameter.ParameterName}={NoticeCode.EmailChangeReverted}", location);
        Assert.Equal(HttpStatusCode.OK, second);
        Assert.Contains(PanelAccessTexts.RevertInvalid, secondHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, oldPassword);
        await using AppDbContext db = NewContext();
        Assert.Equal(previous, await db.Admins.Where(a => a.Id == adminId).Select(a => a.NormalizedEmail).SingleAsync(Ct));
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.AdminId == adminId, Ct));
        DispatchRecord reset = await db.DispatchRecords.AsNoTracking().SingleAsync(d => d.DispatchType == DispatchType.PasswordResetLink, Ct);
        Assert.Equal(previous, reset.RecipientEmail);
        Assert.Equal(1, await db.LinkRecords.CountAsync(l => l.AdminId == adminId && l.LinkType == LinkType.PasswordReset && l.UsedAt == null, Ct));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.Operation == "ChangeAdminEmail" && a.AdminId == adminId, Ct));
    }

    [Fact]
    public async Task TheRevertButton_Is422_WhenTheEarlierAddressBecameAnotherAdministrators_AndTheConsumptionRollsBack_T521_KK10()
    {
        (string currentEmail, Guid adminId) = await SeedAdminAsync();
        (string otherEmail, _) = await SeedAdminAsync();
        (string sessionKey, _) = await SeedSessionAsync(adminId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string key = await SeedRevertAsync(factory.Services.GetRequiredService<LinkKeys>(), adminId, otherEmail, null);

        (HttpStatusCode status, string html) = await PostBodyAsync(client, $"{RevertPath}/{key}?handler=RevertPendingEmailChange", [], AdminCookie(sessionKey));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains(PanelAccessTexts.RevertNotApplied, html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(currentEmail, await db.Admins.Where(a => a.Id == adminId).Select(a => a.NormalizedEmail).SingleAsync(Ct));
        Assert.Null(await db.LinkRecords.Where(l => l.AdminId == adminId && l.LinkType == LinkType.EmailChangeRevert).Select(l => l.UsedAt).SingleAsync(Ct));
        Assert.True(await db.Sessions.AnyAsync(s => s.AdminId == adminId, Ct));
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
    }

    // ---- helpers ----

    private static string AdminCookie(string key) => $"{CookieNames.AdminSession}={key}";

    private static int CounterOf(string html, string name)
    {
        Match match = Regex.Match(html, @"pending-counters__count"">([\d\.]+)</span>\s*<span class=""pending-counters__name"">" + Regex.Escape(name), RegexOptions.None, TimeSpan.FromSeconds(5));
        Assert.True(match.Success, $"The counter '{name}' is not on the page.");
        return int.Parse(match.Groups[1].Value.Replace(".", string.Empty, StringComparison.Ordinal), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task<(HttpStatusCode Status, string Html, List<string> SetCookies)> GetAsync(HttpClient client, string path, params string[] cookies)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        if (cookies.Length > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies));
        }

        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)), SetCookiesOf(response));
    }

    private static async Task<(HttpStatusCode Status, string Html, string? Location)> GetWithLocationAsync(HttpClient client, string path, params string[] cookies)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        if (cookies.Length > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies));
        }

        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)), response.Headers.Location?.OriginalString);
    }

    private static async Task<(HttpStatusCode Status, string? Location, List<string> SetCookies)> PostAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields, params string[] cookies)
    {
        using HttpResponseMessage response = await SendPostAsync(client, path, fields, cookies);
        return (response.StatusCode, response.Headers.Location?.OriginalString, SetCookiesOf(response));
    }

    private static async Task<(HttpStatusCode Status, string Html)> PostBodyAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields, params string[] cookies)
    {
        using HttpResponseMessage response = await SendPostAsync(client, path, fields, cookies);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<HttpResponseMessage> SendPostAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields, string[] cookies)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))) };
        if (cookies.Length > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies));
        }

        return await client.SendAsync(request, Ct);
    }

    private static List<string> SetCookiesOf(HttpResponseMessage response) => response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values) ? [.. values] : [];

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(Pages.PanelOrdersModel).Assembly)));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<(string Email, Guid AdminId)> SeedAdminAsync()
    {
        string email = $"admin-{Guid.NewGuid():N}@example.com";
        Admin admin = Admin.Create("Test yonetici", email, StrongDigest.Value, Now);
        await using AppDbContext db = NewContext();
        db.Admins.Add(admin);
        await db.SaveChangesAsync(Ct);
        return (email, admin.Id);
    }

    private async Task<(string Key, Guid SessionId)> SeedSessionAsync(Guid adminId, ReauthenticationOperation? grantedOperation = null)
    {
        string key = SecretKey.Create();
        Session session = Session.ForAdmin(adminId, SecretKey.Hash(key), false, DateTimeOffset.UtcNow);
        if (grantedOperation is { } operation)
        {
            session.GrantReauthentication(operation, SecretKey.Hash(SecretKey.Create()));
        }

        await using AppDbContext db = NewContext();
        db.Sessions.Add(session);
        await db.SaveChangesAsync(Ct);
        return (key, session.Id);
    }

    // The pending change and the new address's link, the way the request leaves them (06 4.40, 4.41).
    private async Task<string> SeedVerificationAsync(LinkKeys keys, Guid adminId, string newEmail, DateTimeOffset? usedAt, DateTimeOffset expiresAt)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        LinkRecord link = LinkRecord.ForAdmin(keys, LinkType.NewEmailVerification, adminId, expiresAt, now.AddMinutes(-5));
        link.UsedAt = usedAt;
        await using AppDbContext db = NewContext();
        if (!await db.PendingEmailChanges.AnyAsync(p => p.AdminId == adminId, Ct))
        {
            db.PendingEmailChanges.Add(PendingEmailChange.ForAdmin(adminId, newEmail, now.AddMinutes(-5)));
        }

        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return keys.Derive(link.Id);
    }

    private async Task<string> SeedRevertAsync(LinkKeys keys, Guid adminId, string previousEmail, DateTimeOffset? usedAt)
    {
        LinkRecord link = LinkRecord.ForAdmin(keys, LinkType.EmailChangeRevert, adminId, DateTimeOffset.UtcNow.AddDays(5), DateTimeOffset.UtcNow.AddMinutes(-5));
        link.PreviousEmail = previousEmail;
        link.UsedAt = usedAt;
        await using AppDbContext db = NewContext();
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return keys.Derive(link.Id);
    }
}
