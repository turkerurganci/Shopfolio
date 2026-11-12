using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the administrator's own account page tests (T523): one database copied from the template.</summary>
[CollectionDefinition(MyAccountPageTestsDefinition.Name)]
public sealed class MyAccountPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "MyAccountPageTests";
}

/// <summary>
/// The administrator's own account page (E-50; 07 UC-104, UC-178 to UC-180, UC-188) on the opened application (T523 KK1-KK9): what the address draws for each query, what each handler answers (303, 422,
/// 429) and what it writes. Accounts, sessions, links and pending changes are real rows of the collection database.
/// </summary>
[Collection(MyAccountPageTestsDefinition.Name)]
public sealed class MyAccountPageTests : IAsyncLifetime
{
    private const string Strong = "a-long-enough-password";
    private const string AccountPath = "/panel/hesabim";
    private const string ReauthPath = "/panel/yeniden-dogrulama";

    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static readonly Lazy<string> StrongDigest = new(() => new PasswordService().Hash(Strong));

    private readonly CollectionDatabaseFixture _database;

    public MyAccountPageTests(CollectionDatabaseFixture database)
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

    // ---- KK1: the permission and the summary ----

    [Theory]
    [InlineData("PasswordChange", "change-password-form", "change-email-link")]
    [InlineData("EmailChange", "change-email-form", "change-password-link")]
    public async Task TheForm_Opens_OnlyForTheOperationAndValueTheSessionHolds_T523_KK1(string operation, string openForm, string closedLink)
    {
        Guid adminId = await SeedAdminAsync();
        (string key, Guid sessionId, string held) = await SeedSessionAsync(adminId, Enum.Parse<ReauthenticationOperation>(operation));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, $"{AccountPath}?reauthenticatedOperation={operation}&reauthenticationToken={Uri.EscapeDataString(held)}", key);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains($"id=\"{openForm}\"", html, StringComparison.Ordinal);
        Assert.Contains($"id=\"{closedLink}\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.ReauthenticationToken\"", html, StringComparison.Ordinal);
        Assert.Contains($"value=\"{held}\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"change-name-form\"", html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.NotNull((await db.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct)).ReauthenticationTokenHash);
    }

    [Theory]
    [InlineData("&reauthenticationToken=not-the-value")]
    [InlineData("")]
    public async Task AValueThatDoesNotMatch_OrNoParameters_KeepTheFormsClosed_AndDropThePermission_T523_KK1(string tail)
    {
        Guid adminId = await SeedAdminAsync();
        (string key, Guid sessionId, _) = await SeedSessionAsync(adminId, ReauthenticationOperation.PasswordChange);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html) = await GetAsync(client, AccountPath + (tail.Length == 0 ? string.Empty : "?reauthenticatedOperation=PasswordChange" + tail), key);

        Assert.DoesNotContain("id=\"change-password-form\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"change-email-form\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"change-password-link\"", html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Session session = await db.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct);
        Assert.Null(session.ReauthenticatedOperation);
        Assert.Null(session.ReauthenticationTokenHash);
    }

    // ---- KK2: the pending change and the open revert link ----

    [Fact]
    public async Task ThePendingChange_ShowsTheNewAddress_AndTheResendButton_T523_KK2()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, _, _) = await SeedSessionAsync(adminId, null);
        await SeedPendingAsync(adminId, "new-address@example.com");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html) = await GetAsync(client, AccountPath, key);

        Assert.Contains("new-address@example.com adresi doğrulanmayı bekliyor", html, StringComparison.Ordinal);
        Assert.Contains("id=\"resend-email-link-form\"", html, StringComparison.Ordinal);
        Assert.Contains(MyAccountTexts.EmailNote, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnOpenRevertLink_ClosesTheAddressChange_EvenWithThePermission_AndSaysUntilWhen_T523_KK2()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, _, string held) = await SeedSessionAsync(adminId, ReauthenticationOperation.EmailChange);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        DateTimeOffset ends = await SeedRevertAsync(factory.Services.GetRequiredService<LinkKeys>(), adminId);

        (_, string html) = await GetAsync(client, $"{AccountPath}?reauthenticatedOperation=EmailChange&reauthenticationToken={Uri.EscapeDataString(held)}", key);

        Assert.Contains(MyAccountTexts.RevertOpen(ends), html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"change-email-form\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"change-email-link\"", html, StringComparison.Ordinal);
    }

    // ---- KK3: the name ----

    [Fact]
    public async Task ChangingTheName_Is303WithTheNotice_AndAnEmptyName422LeavesTheOldOne_T523_KK3()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, _, _) = await SeedSessionAsync(adminId, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode empty, _, _) = await PostAsync(client, $"{AccountPath}?handler=ChangeName", [("Input.Name", " ")], key);
        (HttpStatusCode done, string? location, _) = await PostAsync(client, $"{AccountPath}?handler=ChangeName", [("Input.Name", "Yeni Ad")], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Equal("/panel/hesabim?notice=Saved", location);
        await using AppDbContext db = NewContext();
        Assert.Equal("Yeni Ad", await db.Admins.Where(a => a.Id == adminId).Select(a => a.Name).SingleAsync(Ct));
    }

    // ---- KK4, KK5, KK6: the password ----

    [Fact]
    public async Task ChangingThePassword_UsesTheValueOnce_EndsTheOtherSessions_AndWritesNoTrailRow_T523_KK4_KK6()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, Guid sessionId, string held) = await SeedSessionAsync(adminId, ReauthenticationOperation.PasswordChange);
        (_, Guid otherSessionId, _) = await SeedSessionAsync(adminId, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string before = await PasswordHashOfAsync(adminId);

        (HttpStatusCode done, string? location, List<string> cookies) = await PostAsync(client, $"{AccountPath}?handler=ChangePassword", [("Input.NewPassword", "another-strong-password"), ("Input.ReauthenticationToken", held)], key);
        (HttpStatusCode again, string? againLocation, _) = await PostAsync(client, $"{AccountPath}?handler=ChangePassword", [("Input.NewPassword", "third-strong-password"), ("Input.ReauthenticationToken", held)], key);

        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Equal("/panel/hesabim?notice=PasswordChanged", location);
        Assert.NotEmpty(cookies);
        Assert.Equal(HttpStatusCode.SeeOther, again);
        Assert.StartsWith(ReauthPath + "?reauthenticatedOperation=PasswordChange", againLocation, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.NotEqual(before, await PasswordHashOfAsync(adminId));
        Assert.True(await db.Sessions.AnyAsync(s => s.Id == sessionId, Ct));
        Assert.False(await db.Sessions.AnyAsync(s => s.Id == otherSessionId, Ct));
        Assert.Null((await db.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct)).ReauthenticationTokenHash);
        Assert.Equal(0, await db.AuditLogs.CountAsync(Ct));
        (_, string html) = await GetAsync(client, location!, key);
        Assert.Contains(MyAccountTexts.PasswordChanged, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task APasswordChange_WithoutTheValue_Is303ToTheReauthentication_AndAWeakPassword422KeepsThePermission_T523_KK5()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, Guid sessionId, string held) = await SeedSessionAsync(adminId, ReauthenticationOperation.PasswordChange);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string before = await PasswordHashOfAsync(adminId);

        (HttpStatusCode none, string? noneLocation, _) = await PostAsync(client, $"{AccountPath}?handler=ChangePassword", [("Input.NewPassword", "another-strong-password"), ("Input.ReauthenticationToken", "wrong")], key);
        (HttpStatusCode weak, string html) = await PostBodyAsync(client, $"{AccountPath}?handler=ChangePassword", [("Input.NewPassword", "short"), ("Input.ReauthenticationToken", held)], key);

        Assert.Equal(HttpStatusCode.SeeOther, none);
        Assert.StartsWith(ReauthPath + "?reauthenticatedOperation=PasswordChange", noneLocation, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, weak);
        Assert.Contains("id=\"change-password-form\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("value=\"short\"", html, StringComparison.Ordinal);
        Assert.Equal(before, await PasswordHashOfAsync(adminId));
        await using AppDbContext db = NewContext();
        Assert.NotNull((await db.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct)).ReauthenticationTokenHash);
    }

    // ---- KK7, KK8: the address ----

    [Fact]
    public async Task ChangingTheAddress_WritesThePendingChange_TheLink_AndTheMail_AndTheOldAddressKeepsWorking_T523_KK7()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, _, string held) = await SeedSessionAsync(adminId, ReauthenticationOperation.EmailChange);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string old = await EmailOfAsync(adminId);

        (HttpStatusCode status, string? location, _) = await PostAsync(client, $"{AccountPath}?handler=ChangeEmail", [("Input.NewEmail", "New.Address@Example.com"), ("Input.ReauthenticationToken", held)], key);

        Assert.Equal(HttpStatusCode.SeeOther, status);
        Assert.Equal(AccountPath, location);
        await using AppDbContext db = NewContext();
        Assert.Equal("New.Address@Example.com", await db.PendingEmailChanges.Where(p => p.AdminId == adminId).Select(p => p.NewEmail).SingleAsync(Ct));
        Assert.Equal(1, await db.LinkRecords.CountAsync(l => l.AdminId == adminId && l.LinkType == LinkType.NewEmailVerification && l.UsedAt == null, Ct));
        Assert.Equal(1, await db.DispatchRecords.CountAsync(Ct));
        Assert.Equal(old, await EmailOfAsync(adminId));
    }

    [Fact]
    public async Task AnAddressRequest_WithoutTheValue_Is303_AndTheRefusedOnes422_WithoutSpendingTheValue_T523_KK8()
    {
        Guid adminId = await SeedAdminAsync();
        Guid otherAdminId = await SeedAdminAsync();
        string otherEmail = await EmailOfAsync(otherAdminId);
        (string key, Guid sessionId, string held) = await SeedSessionAsync(adminId, ReauthenticationOperation.EmailChange);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode none, string? noneLocation, _) = await PostAsync(client, $"{AccountPath}?handler=ChangeEmail", [("Input.NewEmail", "x@example.com"), ("Input.ReauthenticationToken", "wrong")], key);
        (HttpStatusCode malformed, _) = await PostBodyAsync(client, $"{AccountPath}?handler=ChangeEmail", [("Input.NewEmail", "not-an-address"), ("Input.ReauthenticationToken", held)], key);
        (HttpStatusCode taken, string takenHtml) = await PostBodyAsync(client, $"{AccountPath}?handler=ChangeEmail", [("Input.NewEmail", otherEmail), ("Input.ReauthenticationToken", held)], key);

        Assert.Equal(HttpStatusCode.SeeOther, none);
        Assert.StartsWith(ReauthPath + "?reauthenticatedOperation=EmailChange", noneLocation, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, malformed);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, taken);
        Assert.Contains(AdminAccountsModel.AdminEmailTakenText, takenHtml, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.False(await db.PendingEmailChanges.AnyAsync(p => p.AdminId == adminId, Ct));
        Assert.NotNull((await db.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct)).ReauthenticationTokenHash);
    }

    [Fact]
    public async Task AnAddressRequest_WhileARevertLinkIsOpen_Is422WithTheSentence_AndSpendsNothing_T523_KK8()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, Guid sessionId, string held) = await SeedSessionAsync(adminId, ReauthenticationOperation.EmailChange);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        DateTimeOffset ends = await SeedRevertAsync(factory.Services.GetRequiredService<LinkKeys>(), adminId);

        (HttpStatusCode status, string html) = await PostBodyAsync(client, $"{AccountPath}?handler=ChangeEmail", [("Input.NewEmail", "free@example.com"), ("Input.ReauthenticationToken", held)], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains(MyAccountTexts.RevertOpen(ends), html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.False(await db.PendingEmailChanges.AnyAsync(p => p.AdminId == adminId, Ct));
        Assert.NotNull((await db.Sessions.AsNoTracking().SingleAsync(s => s.Id == sessionId, Ct)).ReauthenticationTokenHash);
    }

    // ---- KK9: the link asked again ----

    [Fact]
    public async Task AskingTheLinkAgain_IssuesANewLink_ReplacesTheEarlierOnes_KeepsTheLastDay_AndNothingPendingIs422_T523_KK9()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, _, _) = await SeedSessionAsync(adminId, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode nothing, string nothingHtml) = await PostBodyAsync(client, $"{AccountPath}?handler=ResendEmailChangeLink", [], key);
        await SeedPendingAsync(adminId, "pending@example.com");
        await SeedVerificationLinkAsync(factory.Services.GetRequiredService<LinkKeys>(), adminId);
        (HttpStatusCode done, string? location, _) = await PostAsync(client, $"{AccountPath}?handler=ResendEmailChangeLink", [], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, nothing);
        Assert.Contains(MyAccountTexts.NoPendingChange, nothingHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Equal("/panel/hesabim?notice=LinkResent", location);
        await using AppDbContext db = NewContext();
        List<LinkRecord> links = await db.LinkRecords.AsNoTracking().Where(l => l.AdminId == adminId && l.LinkType == LinkType.NewEmailVerification).OrderBy(l => l.CreatedAt).ToListAsync(Ct);
        Assert.Equal(2, links.Count);
        Assert.NotNull(links[0].SupersededAt);
        Assert.Null(links[1].SupersededAt);
        DateTimeOffset createdAt = await db.PendingEmailChanges.Where(p => p.AdminId == adminId).Select(p => p.CreatedAt).SingleAsync(Ct);
        Assert.Equal(createdAt.AddHours(ProductConstants.EmailVerificationLinkHours), links[1].ExpiresAt);
        (_, string html) = await GetAsync(client, location!, key);
        Assert.Contains("Yeni bir bağlantı gönderdik", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AskingTheLinkAgain_IsLimited429_OnlyAtL9_AndTheEarlierLinkStaysValid_T523_KK9()
    {
        Guid adminId = await SeedAdminAsync();
        (string key, _, _) = await SeedSessionAsync(adminId, null);
        await SeedPendingAsync(adminId, "pending@example.com");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        HttpStatusCode last = HttpStatusCode.OK;
        int sent = 0;
        for (int i = 0; i < 30 && last != HttpStatusCode.TooManyRequests; i++)
        {
            (last, _, _) = await PostAsync(client, $"{AccountPath}?handler=ResendEmailChangeLink", [], key);
            sent++;
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last);
        Assert.True(sent > 1);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.LinkRecords.CountAsync(l => l.AdminId == adminId && l.LinkType == LinkType.NewEmailVerification && l.SupersededAt == null, Ct));
    }

    // ---- helpers ----

    private static string AdminCookie(string key) => $"{CookieNames.AdminSession}={key}";

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path, string sessionKey)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        request.Headers.Add("Cookie", AdminCookie(sessionKey));
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<(HttpStatusCode Status, string? Location, List<string> SetCookies)> PostAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields, string sessionKey)
    {
        using HttpResponseMessage response = await SendPostAsync(client, path, fields, sessionKey);
        return (response.StatusCode, response.Headers.Location?.OriginalString, response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values) ? [.. values] : []);
    }

    private static async Task<(HttpStatusCode Status, string Html)> PostBodyAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields, string sessionKey)
    {
        using HttpResponseMessage response = await SendPostAsync(client, path, fields, sessionKey);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<HttpResponseMessage> SendPostAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields, string sessionKey)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))) };
        request.Headers.Add("Cookie", AdminCookie(sessionKey));
        return await client.SendAsync(request, Ct);
    }

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(Pages.PanelOrdersModel).Assembly)));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<string> PasswordHashOfAsync(Guid adminId)
    {
        await using AppDbContext db = NewContext();
        return await db.Admins.Where(a => a.Id == adminId).Select(a => a.PasswordHash ?? string.Empty).SingleAsync(Ct);
    }

    private async Task<string> EmailOfAsync(Guid adminId)
    {
        await using AppDbContext db = NewContext();
        return await db.Admins.Where(a => a.Id == adminId).Select(a => a.Email).SingleAsync(Ct);
    }

    private async Task<Guid> SeedAdminAsync()
    {
        Admin admin = Admin.Create("Test yonetici", $"admin-{Guid.NewGuid():N}@example.com", StrongDigest.Value, Now);
        await using AppDbContext db = NewContext();
        db.Admins.Add(admin);
        await db.SaveChangesAsync(Ct);
        return admin.Id;
    }

    // The session, with the permission of the operation bound to a value this test knows.
    private async Task<(string Key, Guid SessionId, string HeldValue)> SeedSessionAsync(Guid adminId, ReauthenticationOperation? grantedOperation)
    {
        string key = SecretKey.Create();
        string held = SecretKey.Create();
        Session session = Session.ForAdmin(adminId, SecretKey.Hash(key), false, DateTimeOffset.UtcNow);
        if (grantedOperation is { } operation)
        {
            session.GrantReauthentication(operation, SecretKey.Hash(held));
        }

        await using AppDbContext db = NewContext();
        db.Sessions.Add(session);
        await db.SaveChangesAsync(Ct);
        return (key, session.Id, held);
    }

    private async Task SeedPendingAsync(Guid adminId, string newEmail)
    {
        await using AppDbContext db = NewContext();
        db.PendingEmailChanges.Add(PendingEmailChange.ForAdmin(adminId, newEmail, DateTimeOffset.UtcNow.AddMinutes(-5)));
        await db.SaveChangesAsync(Ct);
    }

    private async Task SeedVerificationLinkAsync(LinkKeys keys, Guid adminId)
    {
        LinkRecord link = LinkRecord.ForAdmin(keys, LinkType.NewEmailVerification, adminId, DateTimeOffset.UtcNow.AddHours(ProductConstants.EmailVerificationLinkHours), DateTimeOffset.UtcNow.AddMinutes(-5));
        await using AppDbContext db = NewContext();
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
    }

    private async Task<DateTimeOffset> SeedRevertAsync(LinkKeys keys, Guid adminId)
    {
        DateTimeOffset ends = DateTimeOffset.UtcNow.AddDays(5);
        LinkRecord link = LinkRecord.ForAdmin(keys, LinkType.EmailChangeRevert, adminId, ends, DateTimeOffset.UtcNow.AddMinutes(-5));
        link.PreviousEmail = "previous@example.com";
        await using AppDbContext db = NewContext();
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return ends;
    }
}
