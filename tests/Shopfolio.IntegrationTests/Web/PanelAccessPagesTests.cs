using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the panel access pages' tests (T520): one database copied from the template.</summary>
[CollectionDefinition(PanelAccessPagesTestsDefinition.Name)]
public sealed class PanelAccessPagesTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "PanelAccessPagesTests";
}

/// <summary>
/// The panel's sign-in, password reset and invitation pages on the opened application (T520 KK1-KK10; 07 9.1, 9.2; 04 E-29, E-30): what each address draws, what each handler answers (303, 422, 429, 200) and
/// what it writes. Administrators, sessions, links and invitations are real rows of the collection database; the e-mail is the queued row of the real notification writer.
/// </summary>
[Collection(PanelAccessPagesTestsDefinition.Name)]
public sealed class PanelAccessPagesTests : IAsyncLifetime
{
    private const string Strong = "a-long-enough-password";
    private const string Replacement = "another-long-enough-one";
    private const string Weak = "short";
    private const string SignInPath = "/panel/giris";
    private const string ResetPath = "/panel/sifre-sifirlama";
    private const string InvitePath = "/panel/davet";

    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static readonly Lazy<string> StrongDigest = new(() => new PasswordService().Hash(Strong));

    private readonly CollectionDatabaseFixture _database;

    public PanelAccessPagesTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // The counters are per e-mail and per address; the test host has one address, so every test starts with empty counters.
    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE attempt_records, login_records, dispatch_records", Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the sign-in page ----

    [Fact]
    public async Task TheSignInPage_HasTheRememberMeBoxUnticked_AndTheClosedSessionMessageOnlyWithItsCode_T520_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode plainStatus, string plain, _) = await GetAsync(client, SignInPath);
        (_, string expired, _) = await GetAsync(client, $"{SignInPath}?{NoticeParameter.ParameterName}={NoticeCode.SessionExpired}");

        Assert.Equal(HttpStatusCode.OK, plainStatus);
        Assert.Contains("name=\"Input.RememberMe\"", plain, StringComparison.Ordinal);
        Assert.DoesNotContain("checked", RememberBoxOf(plain), StringComparison.Ordinal);
        Assert.DoesNotContain(PanelAccessTexts.SessionClosed, plain, StringComparison.Ordinal);
        Assert.Contains(PanelAccessTexts.SessionClosed, expired, StringComparison.Ordinal);
        Assert.DoesNotContain("panel-top", plain, StringComparison.Ordinal);
        Assert.Contains("type=\"password\"", plain, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheAcceptedInvitationValue_FillsTheEmail_OnlyWhenItUnprotectsAndNamesAnAccount_T520_KK1()
    {
        (string email, _) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        ProtectedValues protectedValues = factory.Services.GetRequiredService<ProtectedValues>();
        string known = protectedValues.Protect(DataProtectionPurposes.AcceptedInvitation, email);
        string unknown = protectedValues.Protect(DataProtectionPurposes.AcceptedInvitation, $"nobody-{Guid.NewGuid():N}@example.com");
        string code = $"{NoticeParameter.ParameterName}={NoticeCode.AdminAccountCreated}";

        (_, string filled, _) = await GetAsync(client, $"{SignInPath}?acceptedInvitation={Uri.EscapeDataString(known)}&{code}");
        (_, string noAccount, _) = await GetAsync(client, $"{SignInPath}?acceptedInvitation={Uri.EscapeDataString(unknown)}&{code}");
        (_, string broken, _) = await GetAsync(client, $"{SignInPath}?acceptedInvitation=not-a-protected-value&{code}");
        (_, string noCode, _) = await GetAsync(client, $"{SignInPath}?acceptedInvitation={Uri.EscapeDataString(known)}");

        Assert.Contains($"value=\"{email}\"", filled, StringComparison.Ordinal);
        Assert.Contains(PanelAccessTexts.AccountCreated, filled, StringComparison.Ordinal);
        foreach (string html in new[] { noAccount, broken, noCode })
        {
            Assert.DoesNotContain("value=\"nobody-", html, StringComparison.Ordinal);
            Assert.DoesNotContain($"value=\"{email}\"", html, StringComparison.Ordinal);
        }

        Assert.Contains(PanelAccessTexts.AccountCreated, noAccount, StringComparison.Ordinal);
    }

    // ---- KK2: the sign-in ----

    [Fact]
    public async Task ASignIn_OpensANewPanelSession_AtEveryTime_AndTouchesNoStorefrontSession_T520_KK2()
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode first, string? firstLocation, List<string> firstCookies) = await PostAsync(client, $"{SignInPath}?handler=SignIn", Credentials(email, Strong));
        (HttpStatusCode second, _, List<string> secondCookies) = await PostAsync(client, $"{SignInPath}?handler=SignIn", Credentials(email, Strong, ("Input.ReturnUrl", "/panel/siparisler?page=2")));

        Assert.Equal(HttpStatusCode.SeeOther, first);
        Assert.Equal("/panel", firstLocation);
        Assert.Equal(HttpStatusCode.SeeOther, second);
        string keyOne = CookieValue(firstCookies, CookieNames.AdminSession);
        string keyTwo = CookieValue(secondCookies, CookieNames.AdminSession);
        Assert.NotEqual(keyOne, keyTwo);
        Assert.DoesNotContain(firstCookies.Concat(secondCookies), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal));
        await using AppDbContext db = NewContext();
        Assert.Equal(2, await db.Sessions.CountAsync(s => s.AdminId == adminId, Ct));
        Assert.True(await db.Sessions.AnyAsync(s => s.AdminId == adminId && s.TokenHash == SecretKey.Hash(keyOne), Ct));
        Assert.Equal(1, await db.Sessions.CountAsync(s => s.CustomerId == customerId, Ct));
        Assert.Equal(2, await db.LoginRecords.CountAsync(r => r.AccountType == AccountType.Admin && r.NormalizedEmail == email && r.Outcome == LoginOutcome.Succeeded, Ct));
    }

    [Theory]
    [InlineData("/panel/siparisler?page=2", "/panel/siparisler?page=2")]
    [InlineData("/panel/urunler", "/panel/urunler")]
    [InlineData("//evil.example", "/panel")]
    [InlineData("https://evil.example", "/panel")]
    [InlineData("/\\evil.example", "/panel")]
    [InlineData("", "/panel")]
    public async Task TheSignInGoesToTheLocalReturnAddress_ElseToThePanelsHomePage_T520_KK2(string returnUrl, string expected)
    {
        (string email, _) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string? location, _) = await PostAsync(client, $"{SignInPath}?handler=SignIn", Credentials(email, Strong, ("Input.ReturnUrl", returnUrl)));

        Assert.Equal(HttpStatusCode.SeeOther, status);
        Assert.Equal(expected, location);
    }

    [Fact]
    public async Task TheSignInGetCarriesALocalReturnAddressInTheForm_AndDropsAForeignOne_T520_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string local, _) = await GetAsync(client, $"{SignInPath}?returnUrl={Uri.EscapeDataString("/panel/siparisler?page=2")}");
        (_, string foreign, _) = await GetAsync(client, $"{SignInPath}?returnUrl={Uri.EscapeDataString("https://evil.example")}");

        Assert.Contains("name=\"Input.ReturnUrl\"", local, StringComparison.Ordinal);
        Assert.Contains("/panel/siparisler?page=2", WebUtility.HtmlDecode(local), StringComparison.Ordinal);
        Assert.DoesNotContain("evil.example", foreign, StringComparison.Ordinal);
    }

    // ---- KK3: the refusals ----

    [Fact]
    public async Task AnEmptyOrMalformedForm_Is422_WithoutASession_AndTheSentPasswordIsNotWrittenBack_T520_KK3()
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode empty, _, List<string> emptyCookies) = await PostAsync(client, $"{SignInPath}?handler=SignIn", Credentials(string.Empty, string.Empty));
        (HttpStatusCode malformed, string body) = await PostBodyAsync(client, $"{SignInPath}?handler=SignIn", Credentials("not-an-address", "typed-password-value"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, malformed);
        Assert.DoesNotContain(emptyCookies, c => c.StartsWith(CookieNames.AdminSession + "=", StringComparison.Ordinal));
        Assert.DoesNotContain("typed-password-value", body, StringComparison.Ordinal);
        Assert.Contains("value=\"not-an-address\"", body, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.AdminId == adminId, Ct));
        Assert.Equal(0, await db.LoginRecords.CountAsync(r => r.NormalizedEmail == email, Ct));
    }

    [Fact]
    public async Task AWrongPassword_AWrongEmail_AndAnAbsentAccount_GetTheOneSentence_AndWriteNoSession_T520_KK3()
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode wrongPassword, string first) = await PostBodyAsync(client, $"{SignInPath}?handler=SignIn", Credentials(email, "not-the-password"));
        (HttpStatusCode absent, string second) = await PostBodyAsync(client, $"{SignInPath}?handler=SignIn", Credentials($"nobody-{Guid.NewGuid():N}@example.com", Strong));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, wrongPassword);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, absent);
        Assert.Contains(PanelAccessTexts.InvalidCredentials, first, StringComparison.Ordinal);
        Assert.Contains(PanelAccessTexts.InvalidCredentials, second, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.AdminId == adminId, Ct));
        Assert.Equal(2, await db.LoginRecords.CountAsync(r => r.AccountType == AccountType.Admin && r.Outcome == LoginOutcome.Failed, Ct));
    }

    [Fact]
    public async Task AfterTheLimit_TheSignInIs429_WithTheNeutralSentence_AndEvenTheRightPasswordOpensNothing_T520_KK3()
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        for (int i = 0; i < ProductConstants.SignInAttemptsPerEmail; i++)
        {
            (HttpStatusCode refused, _) = await PostBodyAsync(client, $"{SignInPath}?handler=SignIn", Credentials(email, "not-the-password"));
            Assert.Equal(HttpStatusCode.UnprocessableEntity, refused);
        }

        (HttpStatusCode limited, string body) = await PostBodyAsync(client, $"{SignInPath}?handler=SignIn", Credentials(email, Strong));

        Assert.Equal(HttpStatusCode.TooManyRequests, limited);
        Assert.DoesNotContain(PanelAccessTexts.InvalidCredentials, body, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.AdminId == adminId, Ct));
        Assert.True(await db.AttemptRecords.AnyAsync(r => r.AttemptLimit == AttemptLimit.L1, Ct));
    }

    // ---- KK4: the reset page by its link ----

    [Fact]
    public async Task TheResetAddress_OpensTheSetting_OnlyForAValidLink_AndEveryOtherStateIsTheInvalidOne_T520_KK4()
    {
        (_, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        string valid = await SeedLinkAsync(keys, adminId, Now.AddMinutes(30), null, null);
        string used = await SeedLinkAsync(keys, adminId, Now.AddMinutes(30), Now, null);
        string expired = await SeedLinkAsync(keys, adminId, Now.AddMinutes(-1), null, null);
        string replaced = await SeedLinkAsync(keys, adminId, Now.AddMinutes(30), null, Now);

        (HttpStatusCode requestStatus, string request, _) = await GetAsync(client, ResetPath);
        (_, string setting, _) = await GetAsync(client, $"{ResetPath}/{valid}");

        Assert.Equal(HttpStatusCode.OK, requestStatus);
        Assert.Contains("name=\"Input.Email\"", request, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.NewPassword\"", setting, StringComparison.Ordinal);
        Assert.Contains("type=\"password\"", setting, StringComparison.Ordinal);
        foreach (string key in new[] { used, expired, replaced, "unknown-key" })
        {
            (HttpStatusCode status, string html, _) = await GetAsync(client, $"{ResetPath}/{key}");
            Assert.Equal(HttpStatusCode.OK, status);
            Assert.DoesNotContain("name=\"Input.NewPassword\"", html, StringComparison.Ordinal);
            Assert.Contains(PanelAccessTexts.ResetLinkInvalid, html, StringComparison.Ordinal);
            Assert.Contains($"href=\"{ResetPath}\"", html, StringComparison.Ordinal);
        }
    }

    // ---- KK5: the request ----

    [Fact]
    public async Task TheRequest_AnswersTheSame_ForAnAdministratorAndForAnyOtherAddress_AndMailsOnlyTheAdministrator_T520_KK5()
    {
        (string email, _) = await SeedAdminAsync();
        string customerEmail = $"customer-{Guid.NewGuid():N}@example.com";
        await using (AppDbContext db = NewContext())
        {
            db.Customers.Add(Modules.Membership.Customer.Create("Musteri", customerEmail, StrongDigest.Value, Now, null, Now));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode admin, string? adminLocation, _) = await PostAsync(client, $"{ResetPath}?handler=RequestPasswordReset", [("Input.Email", email)]);
        (HttpStatusCode customer, string? customerLocation, _) = await PostAsync(client, $"{ResetPath}?handler=RequestPasswordReset", [("Input.Email", customerEmail)]);
        (HttpStatusCode absent, string? absentLocation, _) = await PostAsync(client, $"{ResetPath}?handler=RequestPasswordReset", [("Input.Email", $"nobody-{Guid.NewGuid():N}@example.com")]);

        Assert.All(new[] { admin, customer, absent }, s => Assert.Equal(HttpStatusCode.SeeOther, s));
        Assert.Equal($"{ResetPath}?{NoticeParameter.ParameterName}={NoticeCode.PasswordResetRequested}", adminLocation);
        Assert.Equal(adminLocation, customerLocation);
        Assert.Equal(adminLocation, absentLocation);
        await using AppDbContext check = NewContext();
        DispatchRecord mail = await check.DispatchRecords.AsNoTracking().SingleAsync(d => d.DispatchType == DispatchType.PasswordResetLink, Ct);
        Assert.Equal(email, mail.RecipientEmail);
        Assert.True(await check.AttemptRecords.AnyAsync(r => r.AttemptLimit == AttemptLimit.L2, Ct));

        (_, string sent, _) = await GetAsync(client, adminLocation!);
        Assert.Contains(PanelAccessTexts.ResetRequested, sent, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.Email\"", sent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheRequest_RefusesAnEmptyOrMalformedAddress_With422_AndTheLimitWith429_T520_KK5()
    {
        (string email, _) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode empty, _, _) = await PostAsync(client, $"{ResetPath}?handler=RequestPasswordReset", [("Input.Email", string.Empty)]);
        (HttpStatusCode malformed, _, _) = await PostAsync(client, $"{ResetPath}?handler=RequestPasswordReset", [("Input.Email", "not-an-address")]);
        for (int i = 0; i < ProductConstants.PasswordResetAttemptsPerEmail; i++)
        {
            (HttpStatusCode answered, _, _) = await PostAsync(client, $"{ResetPath}?handler=RequestPasswordReset", [("Input.Email", email)]);
            Assert.Equal(HttpStatusCode.SeeOther, answered);
        }

        (HttpStatusCode limited, _, _) = await PostAsync(client, $"{ResetPath}?handler=RequestPasswordReset", [("Input.Email", email)]);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, malformed);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited);
        await using AppDbContext db = NewContext();
        Assert.Equal(ProductConstants.PasswordResetAttemptsPerEmail, await db.DispatchRecords.CountAsync(d => d.DispatchType == DispatchType.PasswordResetLink, Ct));
    }

    // ---- KK6: the setting ----

    [Fact]
    public async Task TheSetting_ConsumesTheLinkOnce_ClosesEverySession_OpensNone_AndTheNewPasswordSignsIn_T520_KK6()
    {
        (string email, Guid adminId) = await SeedAdminAsync();
        (string sessionKey, _) = await SeedSessionAsync(adminId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string key = await SeedLinkAsync(factory.Services.GetRequiredService<LinkKeys>(), adminId, Now.AddMinutes(30), null, null);

        (HttpStatusCode done, string? location, List<string> cookies) = await PostAsync(client, $"{ResetPath}/{key}?handler=ResetPassword", [("Input.NewPassword", Replacement)]);
        (HttpStatusCode again, string html, _) = await PostWithBodyAsync(client, $"{ResetPath}/{key}?handler=ResetPassword", [("Input.NewPassword", Replacement)]);
        (HttpStatusCode oldPassword, _) = await PostBodyAsync(client, $"{SignInPath}?handler=SignIn", Credentials(email, Strong));
        (HttpStatusCode newPassword, _, _) = await PostAsync(client, $"{SignInPath}?handler=SignIn", Credentials(email, Replacement));

        Assert.Equal(HttpStatusCode.SeeOther, done);
        Assert.Equal($"{ResetPath}/{key}?{NoticeParameter.ParameterName}={NoticeCode.PasswordResetCompleted}", location);
        Assert.DoesNotContain(cookies, c => c.StartsWith(CookieNames.AdminSession + "=", StringComparison.Ordinal));
        Assert.Equal(HttpStatusCode.OK, again);
        Assert.Contains(PanelAccessTexts.ResetLinkInvalid, html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, oldPassword);
        Assert.Equal(HttpStatusCode.SeeOther, newPassword);
        await using AppDbContext db = NewContext();
        Assert.False(await db.Sessions.AnyAsync(s => s.TokenHash == SecretKey.Hash(sessionKey), Ct));

        (_, string completed, _) = await GetAsync(client, location!);
        Assert.Contains(PanelAccessTexts.ResetCompleted, completed, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.NewPassword\"", completed, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AWeakPassword_Is422_AndTheLinkIsNotConsumed_T520_KK6()
    {
        (_, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string key = await SeedLinkAsync(factory.Services.GetRequiredService<LinkKeys>(), adminId, Now.AddMinutes(30), null, null);

        (HttpStatusCode empty, _) = await PostBodyAsync(client, $"{ResetPath}/{key}?handler=ResetPassword", [("Input.NewPassword", string.Empty)]);
        (HttpStatusCode weak, string body) = await PostBodyAsync(client, $"{ResetPath}/{key}?handler=ResetPassword", [("Input.NewPassword", Weak)]);
        (_, string page, _) = await GetAsync(client, $"{ResetPath}/{key}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, weak);
        Assert.Contains("name=\"Input.NewPassword\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain($"value=\"{Weak}\"", body, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.NewPassword\"", page, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Null(await db.LinkRecords.Where(l => l.AdminId == adminId).Select(l => l.UsedAt).SingleAsync(Ct));
    }

    [Fact]
    public async Task ASettingWithAnUnknownLink_IsTheInvalidPage_AndWritesNothing_T520_KK6()
    {
        (_, Guid adminId) = await SeedAdminAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html, _) = await PostWithBodyAsync(client, $"{ResetPath}/unknown-key?handler=ResetPassword", [("Input.NewPassword", Replacement)]);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(PanelAccessTexts.ResetLinkInvalid, html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(StrongDigest.Value, await db.Admins.Where(a => a.Id == adminId).Select(a => a.PasswordHash).SingleAsync(Ct));
    }

    // ---- KK7, KK8: the invitation ----

    [Fact]
    public async Task AValidInvitation_ShowsTheAddressReadOnly_AndEveryOtherStateIsTheOneSentenceWithTheSignInLink_T520_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        (string valid, string invited) = await SeedInvitationAsync(keys, Now.AddHours(1), null);
        (string used, _) = await SeedInvitationAsync(keys, Now.AddHours(1), Now);
        (string expired, _) = await SeedInvitationAsync(keys, Now.AddMinutes(-1), null);

        (HttpStatusCode status, string html, _) = await GetAsync(client, $"{InvitePath}/{valid}");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains($"value=\"{invited}\"", html, StringComparison.Ordinal);
        Assert.Contains("readonly", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Name\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Password\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("panel-top", html, StringComparison.Ordinal);
        foreach (string key in new[] { used, expired, "unknown-key" })
        {
            (HttpStatusCode invalidStatus, string invalid, _) = await GetAsync(client, $"{InvitePath}/{key}");
            Assert.Equal(HttpStatusCode.OK, invalidStatus);
            Assert.DoesNotContain("<form", invalid[invalid.IndexOf("<main", StringComparison.Ordinal)..], StringComparison.Ordinal);
            Assert.Contains(PanelAccessTexts.InvitationInvalid, invalid, StringComparison.Ordinal);
            Assert.Contains($"href=\"{SignInPath}\"", invalid, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheAcceptance_OpensTheAccountWithTheInvitationsAddress_ConsumesTheLink_LeavesATrail_AndOpensNoSession_T520_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string key, string invited) = await SeedInvitationAsync(factory.Services.GetRequiredService<LinkKeys>(), Now.AddHours(1), null);

        (HttpStatusCode status, string? location, List<string> cookies) = await PostAsync(client, $"{InvitePath}/{key}?handler=AcceptAdminInvitation", [("Input.Name", "Yeni Yönetici"), ("Input.Password", Strong), ("Input.Email", "other@example.com")]);
        (HttpStatusCode again, string html, _) = await PostWithBodyAsync(client, $"{InvitePath}/{key}?handler=AcceptAdminInvitation", [("Input.Name", "Baska"), ("Input.Password", Strong)]);

        Assert.Equal(HttpStatusCode.SeeOther, status);
        Assert.StartsWith($"{SignInPath}?acceptedInvitation=", location, StringComparison.Ordinal);
        Assert.EndsWith($"&{NoticeParameter.ParameterName}={NoticeCode.AdminAccountCreated}", location, StringComparison.Ordinal);
        Assert.DoesNotContain(cookies, c => c.StartsWith(CookieNames.AdminSession + "=", StringComparison.Ordinal));
        Assert.Equal(HttpStatusCode.OK, again);
        Assert.Contains(PanelAccessTexts.InvitationInvalid, html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Admin created = await db.Admins.AsNoTracking().SingleAsync(a => a.Email == invited, Ct);
        Assert.Equal("Yeni Yönetici", created.Name);
        Assert.NotNull(created.PasswordHash);
        Assert.Equal(1, await db.Admins.CountAsync(a => a.Email == invited || a.Name == "Baska", Ct));
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.AdminId == created.Id, Ct));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.Operation == "CreateAdminAccount" && a.AdminId == created.Id, Ct));
        Assert.NotNull(await db.AdminInvitations.Where(i => i.Email == invited).Select(i => i.AcceptedAt).SingleAsync(Ct));

        // The sign-in page the acceptance leads to fills the address in; the password set there signs in.
        (_, string signIn, _) = await GetAsync(client, location!);
        Assert.Contains($"value=\"{invited}\"", signIn, StringComparison.Ordinal);
        (HttpStatusCode signedIn, _, _) = await PostAsync(client, $"{SignInPath}?handler=SignIn", Credentials(invited, Strong));
        Assert.Equal(HttpStatusCode.SeeOther, signedIn);
    }

    [Fact]
    public async Task TheAcceptance_Is422_ForAnEmptyNameOrAWeakPassword_AndTheLinkIsNotConsumed_T520_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string key, string invited) = await SeedInvitationAsync(factory.Services.GetRequiredService<LinkKeys>(), Now.AddHours(1), null);

        (HttpStatusCode noName, string noNameBody) = await PostBodyAsync(client, $"{InvitePath}/{key}?handler=AcceptAdminInvitation", [("Input.Name", "  "), ("Input.Password", Strong)]);
        (HttpStatusCode weak, string weakBody) = await PostBodyAsync(client, $"{InvitePath}/{key}?handler=AcceptAdminInvitation", [("Input.Name", "Yeni"), ("Input.Password", Weak)]);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, noName);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, weak);
        Assert.Contains($"value=\"{invited}\"", noNameBody, StringComparison.Ordinal);
        Assert.Contains("value=\"Yeni\"", weakBody, StringComparison.Ordinal);
        Assert.DoesNotContain($"value=\"{Weak}\"", weakBody, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.False(await db.Admins.AnyAsync(a => a.Email == invited, Ct));
        Assert.Null(await db.AdminInvitations.Where(i => i.Email == invited).Select(i => i.AcceptedAt).SingleAsync(Ct));
    }

    // ---- helpers ----

    private static string RememberBoxOf(string html)
    {
        int at = html.IndexOf("name=\"Input.RememberMe\"", StringComparison.Ordinal);
        Assert.True(at > 0, "The sign-in page has no remember-me box.");
        int start = html.LastIndexOf('<', at);
        return html[start..html.IndexOf('>', at)];
    }

    private static (string Name, string Value)[] Credentials(string email, string password, params (string Name, string Value)[] more) =>
        [("Input.Email", email), ("Input.Password", password), .. more];

    private static string CookieValue(IEnumerable<string> setCookies, string name)
    {
        string line = setCookies.First(c => c.StartsWith(name + "=", StringComparison.Ordinal));
        return line.Split(';', 2)[0][(name.Length + 1)..];
    }

    private static async Task<(HttpStatusCode Status, string Html, List<string> SetCookies)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)), SetCookiesOf(response));
    }

    private static async Task<(HttpStatusCode Status, string? Location, List<string> SetCookies)> PostAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields, params string[] cookies)
    {
        using HttpResponseMessage response = await SendPostAsync(client, path, fields, cookies);
        return (response.StatusCode, response.Headers.Location?.OriginalString, SetCookiesOf(response));
    }

    private static async Task<(HttpStatusCode Status, string Html)> PostBodyAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields)
    {
        using HttpResponseMessage response = await SendPostAsync(client, path, fields, []);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<(HttpStatusCode Status, string Html, List<string> SetCookies)> PostWithBodyAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields)
    {
        using HttpResponseMessage response = await SendPostAsync(client, path, fields, []);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)), SetCookiesOf(response));
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

    // The test host carries the sample pages of the test assembly, among them the forgery token page the writing requests need.
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

    private async Task<(string Key, Guid SessionId)> SeedSessionAsync(Guid adminId)
    {
        string key = SecretKey.Create();
        Session session = Session.ForAdmin(adminId, SecretKey.Hash(key), false, Now);
        await using AppDbContext db = NewContext();
        db.Sessions.Add(session);
        await db.SaveChangesAsync(Ct);
        return (key, session.Id);
    }

    private async Task<string> SeedLinkAsync(LinkKeys keys, Guid adminId, DateTimeOffset expiresAt, DateTimeOffset? usedAt, DateTimeOffset? supersededAt)
    {
        LinkRecord link = LinkRecord.ForAdmin(keys, LinkType.PasswordReset, adminId, expiresAt, Now.AddMinutes(-5));
        link.UsedAt = usedAt;
        link.SupersededAt = supersededAt;
        await using AppDbContext db = NewContext();
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return keys.Derive(link.Id);
    }

    private async Task<(string Key, string Email)> SeedInvitationAsync(LinkKeys keys, DateTimeOffset expiresAt, DateTimeOffset? usedAt)
    {
        string email = $"invited-{Guid.NewGuid():N}@example.com";
        AdminInvitation invitation = AdminInvitation.Create(email, null, "Gonderen", Now.AddMinutes(-5));
        LinkRecord link = LinkRecord.ForInvitation(keys, invitation.Id, expiresAt, Now.AddMinutes(-5));
        link.UsedAt = usedAt;
        await using AppDbContext db = NewContext();
        db.AdminInvitations.Add(invitation);
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return (keys.Derive(link.Id), email);
    }
}
