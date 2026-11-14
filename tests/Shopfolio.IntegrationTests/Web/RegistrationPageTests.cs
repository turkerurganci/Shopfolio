using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the registration and verification pages' tests (T514): one database copied from the template.</summary>
[CollectionDefinition(RegistrationPageTestsDefinition.Name)]
public sealed class RegistrationPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "RegistrationPageTests";
}

/// <summary>
/// The registration screen and the verification landing page on the opened application (T514 KK1-KK10; 07 UC-47 to UC-49, UC-52, UC-53; 04 E-20, E-21): the form and its closed state, the pending
/// registration with its link and e-mail row, the refusals and the limits, the resend, the six states of the landing, and the three buttons. Accounts, links and rows are real rows of the
/// collection database; the sales gate is the real one except where a test opens it (the template's firm does not meet the gate's conditions).
/// </summary>
[Collection(RegistrationPageTestsDefinition.Name)]
public sealed class RegistrationPageTests : IAsyncLifetime
{
    private const string Entered = "orchid-lantern-4471-violet";

    private readonly CollectionDatabaseFixture _database;

    private int _counter;

    public RegistrationPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE customers, link_records, dispatch_records, attempt_records, disposal_records, orders CASCADE", Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the form; the closed gate has no button ----

    [Fact]
    public async Task TheForm_HasTheThreeFields_NoValueInTheSecretField_AndNoGoogleButton_T514_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);

        (HttpStatusCode status, string html, _) = await GetAsync(factory, FramePaths.Register);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("id=\"registration-form\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Name\"", html, StringComparison.Ordinal);
        Assert.Contains("type=\"email\"", html, StringComparison.Ordinal);
        Assert.Contains("type=\"password\"", html, StringComparison.Ordinal);
        Assert.Contains(RegistrationTexts.SubmitLabel, html, StringComparison.Ordinal);
        Assert.Contains($"href=\"{FramePaths.PrivacyNotice}\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Google", html, StringComparison.Ordinal);
        Assert.DoesNotContain("pazarlama", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WhileTheGateIsClosed_TheFormIsReplacedByTheSentence_HasNoButton_AndTheSendWritesNothing_T514_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html, _) = await GetAsync(factory, FramePaths.Register);
        using HttpResponseMessage response = await PostAsync(client, FramePaths.Register, "Register", Form("kapali@example.com"));

        Assert.Contains(SalesClosedMessages.DataEntryText, html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"registration-form\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("type=\"password\"", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(SalesClosedMessages.DataEntryText, await BodyOf(response), StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Customers.CountAsync(Ct));
    }

    // ---- KK2, KK3: the pending registration, its link, its e-mail row, no session ----

    [Fact]
    public async Task Register_OpensAnUnverifiedAccount_ALinkOfZ1_AndTheEmailRowInTheSameTransaction_AndNoSession_T514_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAsync(client, FramePaths.Register, "Register", Form("ayse@example.com"));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.StartsWith($"{FramePaths.Register}?{RegisterModel.PendingParameter}=", response.Headers.Location!.OriginalString, StringComparison.Ordinal);
        Assert.EndsWith($"&notice={NoticeCode.RegistrationSubmitted}", response.Headers.Location.OriginalString, StringComparison.Ordinal);
        Assert.DoesNotContain(CookieNames.Session, string.Join(";", SetCookies(response)), StringComparison.Ordinal);

        await using AppDbContext db = NewContext();
        Customer customer = await db.Customers.AsNoTracking().SingleAsync(Ct);
        Assert.Null(customer.EmailVerifiedAt);
        Assert.Equal("ayse@example.com", customer.Email);
        LinkRecord link = await db.LinkRecords.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(LinkType.EmailVerification, link.LinkType);
        Assert.Equal(customer.Id, link.CustomerId);
        Assert.Equal(TimeSpan.FromHours(ProductConstants.EmailVerificationLinkHours), link.ExpiresAt - link.CreatedAt);
        DispatchRecord row = await db.DispatchRecords.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(DispatchType.EmailVerificationLink, row.DispatchType);
        Assert.Equal("ayse@example.com", row.RecipientEmail);
        Assert.Equal(customer.Id, row.RelatedRecordId);
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
    }

    [Fact]
    public async Task TheAfterSendState_IsReadFromTheValue_AndTheValueIsNotStoredAnywhere_T514_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage response = await PostAsync(client, FramePaths.Register, "Register", Form("ayse@example.com"));
        string location = response.Headers.Location!.OriginalString;
        string value = Uri.UnescapeDataString(location.Split($"{RegisterModel.PendingParameter}=")[1].Split('&')[0]);

        (HttpStatusCode status, string html, _) = await GetAsync(factory, location);
        (_, string tampered, _) = await GetAsync(factory, $"{FramePaths.Register}?{RegisterModel.PendingParameter}={Uri.EscapeDataString(value[..^3])}&notice={NoticeCode.RegistrationSubmitted}");
        (_, string noCode, _) = await GetAsync(factory, $"{FramePaths.Register}?{RegisterModel.PendingParameter}={Uri.EscapeDataString(value)}");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("id=\"registration-sent\"", html, StringComparison.Ordinal);
        Assert.Contains("ayse@example.com", html, StringComparison.Ordinal);
        Assert.Contains(RegistrationTexts.ResendLabel, html, StringComparison.Ordinal);
        Assert.DoesNotContain("type=\"password\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"registration-sent\"", tampered, StringComparison.Ordinal);
        Assert.Contains("id=\"registration-form\"", tampered, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"registration-sent\"", noCode, StringComparison.Ordinal);

        // The value is a function of the key ring and the id: no column of the account or of the links holds it (06 4.36).
        await using AppDbContext db = NewContext();
        Customer customer = await db.Customers.AsNoTracking().SingleAsync(Ct);
        Assert.NotEqual(customer.Id.ToString("N"), value);
        string[] columns = await db.Database.SqlQueryRaw<string>("SELECT column_name AS \"Value\" FROM information_schema.columns WHERE table_name IN ('customers', 'link_records')").ToArrayAsync(Ct);
        Assert.DoesNotContain(columns, c => c.Contains("pending", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0, await db.Customers.CountAsync(c => c.Name == value || c.Email == value || c.PasswordHash == value, Ct));
    }

    // ---- KK4: the five 422 reasons and the 429 ----

    [Theory]
    [InlineData("", "ayse@example.com", Entered, "Ad boş olamaz.")]
    [InlineData("Ayşe", "not-an-email", Entered, "geçerli bir e-posta")]
    [InlineData("Ayşe", "ayse@example.com", "kisa", "en az")]
    [InlineData("Ayşe", "ayse@example.com", "1234567890", "çok yaygın")]
    public async Task ARefusedField_Answers422_WithItsMessage_KeepsTheTypedValues_NotTheSecret_AndWritesNothing_T514_KK4(string name, string email, string typedSecret, string message)
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAsync(client, FramePaths.Register, "Register", [("Name", name), ("Email", email), ("Password", typedSecret)]);
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(message, html, StringComparison.Ordinal);
        Assert.Contains("id=\"form-error-summary\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain($"value=\"{typedSecret}\"", html, StringComparison.Ordinal);
        if (email.Length > 0)
        {
            Assert.Contains($"value=\"{email}\"", html, StringComparison.Ordinal);
        }

        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Customers.CountAsync(Ct));
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AnAddressOfAnAccount_PendingOrVerified_Answers422_WithTheSignInLink_AndOpensNoSecondRegistration_T514_KK4_KK5(bool verified)
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        await SeedCustomerAsync("ayse@example.com", verified);

        using HttpResponseMessage response = await PostAsync(client, FramePaths.Register, "Register", Form("AYSE@example.com"));
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(RegistrationTexts.EmailAlreadyRegistered, html, StringComparison.Ordinal);
        Assert.Contains($"href=\"{FramePaths.SignIn}\"", html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Customers.CountAsync(Ct));
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
    }

    [Fact]
    public async Task ASecondSend_OfTheSameForm_IsRefused_AndOpensNoSecondRegistration_T514_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage first = await PostAsync(client, FramePaths.Register, "Register", Form("ayse@example.com"));
        using HttpResponseMessage second = await PostAsync(client, FramePaths.Register, "Register", Form("ayse@example.com"));

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, second.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Customers.CountAsync(Ct));
        Assert.Equal(1, await db.LinkRecords.CountAsync(Ct));
        Assert.Equal(1, await db.DispatchRecords.CountAsync(Ct));
    }

    [Fact]
    public async Task PastTheLimitL3_TheAnswerIs429_WithTheNeutralSentence_AndNoRetryAfter_T514_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        HttpStatusCode last = default;
        string html = string.Empty;
        HttpResponseMessage? limited = null;
        for (int i = 0; i <= ProductConstants.RegistrationAttemptsPerEmail; i++)
        {
            limited?.Dispose();
            limited = await PostAsync(client, FramePaths.Register, "Register", Form("ayse@example.com"));
            last = limited.StatusCode;
        }

        html = await BodyOf(limited!);
        Assert.Equal(HttpStatusCode.TooManyRequests, last);
        Assert.Contains(NeutralMessages.LimitExceeded, html, StringComparison.Ordinal);
        Assert.False(limited!.Headers.Contains("Retry-After"));
        limited.Dispose();
    }

    // ---- criterion 6, KK6: the resend ----

    [Fact]
    public async Task TheResend_SendsANewLink_SupersedesTheEarlierOnes_AndDoesNotLengthenTheRegistration_T514_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage registered = await PostAsync(client, FramePaths.Register, "Register", Form("ayse@example.com"));
        string value = ValueOf(registered);
        DateTimeOffset firstEnd = await NewContext().LinkRecords.Select(l => l.ExpiresAt).SingleAsync(Ct);

        using HttpResponseMessage resent = await PostAsync(client, FramePaths.Register, "ResendVerificationLink", [("PendingRegistration", value)]);
        (_, string html, _) = await GetAsync(factory, resent.Headers.Location!.OriginalString);

        Assert.Equal(HttpStatusCode.SeeOther, resent.StatusCode);
        Assert.EndsWith($"&notice={NoticeCode.LinkResent}", resent.Headers.Location.OriginalString, StringComparison.Ordinal);
        Assert.Contains(RegistrationTexts.LinkResent(firstEnd), html, StringComparison.Ordinal);
        Assert.Contains("Yeni bir bağlantı gönderdik; önceki bağlantılar artık geçersiz. Bağlantı " + DisplayFormat.Date(firstEnd) + " tarihine kadar geçerli.", html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        List<LinkRecord> links = await db.LinkRecords.AsNoTracking().OrderBy(l => l.CreatedAt).ToListAsync(Ct);
        Assert.Equal(2, links.Count);
        Assert.NotNull(links[0].SupersededAt);
        Assert.Null(links[1].SupersededAt);
        Assert.Equal(firstEnd, links[1].ExpiresAt);
        Assert.Equal(2, await db.DispatchRecords.CountAsync(d => d.DispatchType == DispatchType.EmailVerificationLink && d.RecipientEmail == "ayse@example.com", Ct));
    }

    [Fact]
    public async Task TheResend_OfAValueOrARegistrationThatIsNotPending_SendsNothing_T514_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage registered = await PostAsync(client, FramePaths.Register, "Register", Form("ayse@example.com"));
        string value = ValueOf(registered);

        using HttpResponseMessage garbage = await PostAsync(client, FramePaths.Register, "ResendVerificationLink", [("PendingRegistration", "garbage")]);
        using HttpResponseMessage empty = await PostAsync(client, FramePaths.Register, "ResendVerificationLink", [("PendingRegistration", string.Empty)]);
        await using (AppDbContext ended = NewContext())
        {
            await ended.LinkRecords.ExecuteUpdateAsync(s => s.SetProperty(l => l.ExpiresAt, DateTimeOffset.UtcNow.AddMinutes(-1)), Ct);
        }

        using HttpResponseMessage late = await PostAsync(client, FramePaths.Register, "ResendVerificationLink", [("PendingRegistration", value)]);
        (_, string html, _) = await GetAsync(factory, late.Headers.Location!.OriginalString);

        foreach (HttpResponseMessage response in new[] { garbage, empty, late })
        {
            Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
            Assert.Equal(FramePaths.Register, response.Headers.Location!.OriginalString);
        }

        Assert.Contains("id=\"registration-form\"", html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.LinkRecords.CountAsync(Ct));
        Assert.Equal(1, await db.DispatchRecords.CountAsync(Ct));
    }

    [Fact]
    public async Task PastTheLimitL9_TheResendAnswers429_WithTheNeutralSentence_AndTheStateStaysDrawn_T514_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage registered = await PostAsync(client, FramePaths.Register, "Register", Form("ayse@example.com"));
        string value = ValueOf(registered);

        HttpResponseMessage? last = null;
        for (int i = 0; i <= ProductConstants.VerificationResendAttemptsPerEmail; i++)
        {
            last?.Dispose();
            last = await PostAsync(client, FramePaths.Register, "ResendVerificationLink", [("PendingRegistration", value)]);
        }

        string html = await BodyOf(last!);
        Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);
        Assert.Contains(NeutralMessages.LimitExceeded, html, StringComparison.Ordinal);
        Assert.Contains("id=\"registration-sent\"", html, StringComparison.Ordinal);
        Assert.False(last.Headers.Contains("Retry-After"));
        last.Dispose();
        await using AppDbContext db = NewContext();
        Assert.Equal(1 + ProductConstants.VerificationResendAttemptsPerEmail, await db.DispatchRecords.CountAsync(Ct));
    }

    // ---- KK7, KK8: the landing's states; opening writes nothing; a foreign link is not recognized ----

    [Fact]
    public async Task OpeningARegistrationLink_ShowsTheStepAndTheButton_AndWritesNothing_T514_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        (Guid customerId, string key) = await SeedLinkedCustomerAsync(factory, "ayse@example.com", LinkType.EmailVerification, verified: false);

        (HttpStatusCode first, string html, _) = await GetAsync(factory, $"{FramePaths.Verification}/{key}");
        (_, string again, _) = await GetAsync(factory, $"{FramePaths.Verification}/{key}");

        Assert.Equal(HttpStatusCode.OK, first);
        Assert.Contains("E-postamı doğrula", html, StringComparison.Ordinal);
        Assert.Contains("E-postamı doğrula", again, StringComparison.Ordinal);
        Assert.Contains($"?handler=VerifyEmail", html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Null(await db.Customers.Where(c => c.Id == customerId).Select(c => c.EmailVerifiedAt).SingleAsync(Ct));
        Assert.Null(await db.LinkRecords.Select(l => l.UsedAt).SingleAsync(Ct));
    }

    [Fact]
    public async Task TheOtherStates_OfARegistrationLink_AreToldWithOneNextStep_T514_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        (_, string expired) = await SeedLinkedCustomerAsync(factory, "a1@example.com", LinkType.EmailVerification, verified: false, linkEnds: DateTimeOffset.UtcNow.AddHours(-1));
        (_, string used) = await SeedLinkedCustomerAsync(factory, "a2@example.com", LinkType.EmailVerification, verified: true, used: true);
        (_, string superseded) = await SeedLinkedCustomerAsync(factory, "a3@example.com", LinkType.EmailVerification, verified: false, superseded: true);

        (_, string expiredHtml, _) = await GetAsync(factory, $"{FramePaths.Verification}/{expired}");
        (_, string usedHtml, _) = await GetAsync(factory, $"{FramePaths.Verification}/{used}");
        (_, string supersededHtml, _) = await GetAsync(factory, $"{FramePaths.Verification}/{superseded}");
        (HttpStatusCode unknown, string unknownHtml, _) = await GetAsync(factory, $"{FramePaths.Verification}/no-such-key");

        Assert.Contains(VerificationLandings.Unrecognized.Sentence, expiredHtml, StringComparison.Ordinal);
        Assert.Contains($"href=\"{FramePaths.Register}\"", expiredHtml, StringComparison.Ordinal);
        Assert.Contains("zaten doğrulandı", usedHtml, StringComparison.Ordinal);
        Assert.Contains("en son gönderilen bağlantıyı kullan", supersededHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, unknown);
        Assert.Contains(VerificationLandings.Unrecognized.Sentence, unknownHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("?handler=VerifyEmail", expiredHtml + usedHtml + supersededHtml + unknownHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePress_OnALinkThatDoesNotHold_ShowsItsOwnState_AndWritesNothing_T514_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (Guid expiredId, string expired) = await SeedLinkedCustomerAsync(factory, "a1@example.com", LinkType.EmailVerification, verified: false, linkEnds: DateTimeOffset.UtcNow.AddHours(-1));
        (Guid usedId, string used) = await SeedLinkedCustomerAsync(factory, "a2@example.com", LinkType.EmailVerification, verified: false, used: true);
        (Guid supersededId, string superseded) = await SeedLinkedCustomerAsync(factory, "a3@example.com", LinkType.EmailVerification, verified: false, superseded: true);
        await using AppDbContext before = NewContext();
        DateTimeOffset?[] usedBefore = await before.LinkRecords.OrderBy(l => l.CreatedAt).Select(l => l.UsedAt).ToArrayAsync(Ct);

        using HttpResponseMessage expiredPost = await PostAsync(client, $"{FramePaths.Verification}/{expired}", "VerifyEmail", []);
        using HttpResponseMessage usedPost = await PostAsync(client, $"{FramePaths.Verification}/{used}", "VerifyEmail", []);
        using HttpResponseMessage supersededPost = await PostAsync(client, $"{FramePaths.Verification}/{superseded}", "VerifyEmail", []);

        foreach (HttpResponseMessage post in new[] { expiredPost, usedPost, supersededPost })
        {
            Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        }

        Assert.Contains(VerificationLandings.Unrecognized.Sentence, await BodyOf(expiredPost), StringComparison.Ordinal);
        Assert.Contains("zaten doğrulandı", await BodyOf(usedPost), StringComparison.Ordinal);
        Assert.Contains("en son gönderilen bağlantıyı kullan", await BodyOf(supersededPost), StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Customers.CountAsync(c => c.EmailVerifiedAt != null, Ct));
        Assert.Equal(usedBefore, await db.LinkRecords.OrderBy(l => l.CreatedAt).Select(l => l.UsedAt).ToArrayAsync(Ct));
        Assert.Equal(3, await db.Customers.CountAsync(c => c.Id == expiredId || c.Id == usedId || c.Id == supersededId, Ct));
    }

    [Fact]
    public async Task OpeningTheLinkManyTimes_DoesNotVerify_NorBindsTheGuestOrdersOfTheAddress_T514_KK7_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        (Guid customerId, string key) = await SeedLinkedCustomerAsync(factory, "alici@example.com", LinkType.EmailVerification, verified: false);

        for (int i = 0; i < 3; i++)
        {
            (HttpStatusCode status, _, _) = await GetAsync(factory, $"{FramePaths.Verification}/{key}");
            Assert.Equal(HttpStatusCode.OK, status);
        }

        await using AppDbContext db = NewContext();
        Assert.Null(await db.Customers.Where(c => c.Id == customerId).Select(c => c.EmailVerifiedAt).SingleAsync(Ct));
        Assert.Null(await db.LinkRecords.Select(l => l.UsedAt).SingleAsync(Ct));
        Assert.Null(await db.Orders.Where(o => o.Id == orderId).Select(o => o.CustomerId).SingleAsync(Ct));
    }

    [Fact]
    public async Task AResultCode_OnAnAddressThatWasNotUsed_TellsNothing_T514_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        (_, string key) = await SeedLinkedCustomerAsync(factory, "ayse@example.com", LinkType.EmailVerification, verified: false);

        (_, string html, _) = await GetAsync(factory, $"{FramePaths.Verification}/{key}?notice={NoticeCode.EmailVerified}");

        Assert.DoesNotContain("Hesabınız açıldı", html, StringComparison.Ordinal);
        Assert.Contains("E-postamı doğrula", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnotherTypesLink_AndAnAdministratorsLink_AreNotRecognized_AndNothingIsConsumed_T514_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (_, string reset) = await SeedLinkedCustomerAsync(factory, "ayse@example.com", LinkType.PasswordReset, verified: true);
        string admin = await SeedAdminLinkAsync(factory);

        foreach (string key in new[] { reset, admin })
        {
            (HttpStatusCode status, string html, _) = await GetAsync(factory, $"{FramePaths.Verification}/{key}");
            using HttpResponseMessage post = await PostAsync(client, $"{FramePaths.Verification}/{key}", "VerifyEmail", []);

            Assert.Equal(HttpStatusCode.OK, status);
            Assert.Contains(VerificationLandings.Unrecognized.Sentence, html, StringComparison.Ordinal);
            Assert.Equal(HttpStatusCode.OK, post.StatusCode);
            Assert.Contains(VerificationLandings.Unrecognized.Sentence, await BodyOf(post), StringComparison.Ordinal);
        }

        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.LinkRecords.CountAsync(l => l.UsedAt != null, Ct));
    }

    // ---- KK9, KK10: the buttons ----

    [Fact]
    public async Task VerifyEmail_ConsumesTheLinkOnce_Answers303WithTheCode_OpensNoSession_AndBindsTheGuestOrders_T514_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        (Guid customerId, string key) = await SeedLinkedCustomerAsync(factory, "alici@example.com", LinkType.EmailVerification, verified: false);

        using HttpResponseMessage first = await PostAsync(client, $"{FramePaths.Verification}/{key}", "VerifyEmail", []);
        using HttpResponseMessage second = await PostAsync(client, $"{FramePaths.Verification}/{key}", "VerifyEmail", []);
        (_, string done, _) = await GetAsync(factory, first.Headers.Location!.OriginalString);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal($"{FramePaths.Verification}/{key}?notice={NoticeCode.EmailVerified}", first.Headers.Location.OriginalString);
        Assert.DoesNotContain(CookieNames.Session, string.Join(";", SetCookies(first)), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Contains("zaten doğrulandı", await BodyOf(second), StringComparison.Ordinal);
        Assert.Contains("Hesabınız açıldı", done, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.NotNull(await db.Customers.Where(c => c.Id == customerId).Select(c => c.EmailVerifiedAt).SingleAsync(Ct));
        Assert.Equal(customerId, await db.Orders.Where(o => o.Id == orderId).Select(o => o.CustomerId).SingleAsync(Ct));
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
    }

    [Fact]
    public async Task TwoPressesAtOnce_ConsumeTheLinkOnce_T514_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (_, string key) = await SeedLinkedCustomerAsync(factory, "ayse@example.com", LinkType.EmailVerification, verified: false);

        Task<HttpResponseMessage>[] presses = [.. Enumerable.Range(0, 4).Select(_ => PostAsync(client, $"{FramePaths.Verification}/{key}", "VerifyEmail", []))];
        HttpResponseMessage[] answers = await Task.WhenAll(presses);

        Assert.Equal(1, answers.Count(a => a.StatusCode == HttpStatusCode.SeeOther));
        Assert.Equal(3, answers.Count(a => a.StatusCode == HttpStatusCode.OK));
        foreach (HttpResponseMessage answer in answers)
        {
            answer.Dispose();
        }
    }

    [Fact]
    public async Task TheNewAddressButton_ChangesTheAddress_DeletesThePendingRegistrationOfTheAddress_AndAnswers303_T514_KK9_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Guid customerId = await SeedCustomerAsync("eski@example.com", verified: true);
        Guid pendingId = await SeedCustomerAsync("yeni@example.com", verified: false);
        string key = await RequestEmailChangeAsync(factory, customerId, "yeni@example.com");

        (_, string html, _) = await GetAsync(factory, $"{FramePaths.Verification}/{key}");
        using HttpResponseMessage post = await PostAsync(client, $"{FramePaths.Verification}/{key}", "VerifyEmail", []);
        (_, string done, _) = await GetAsync(factory, post.Headers.Location!.OriginalString);

        Assert.Contains("Yeni adresi doğrula", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, post.StatusCode);
        Assert.Contains("Yeni e-posta adresiniz doğrulandı", done, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal("yeni@example.com", await db.Customers.Where(c => c.Id == customerId).Select(c => c.Email).SingleAsync(Ct));
        Assert.False(await db.Customers.AnyAsync(c => c.Id == pendingId, Ct));
    }

    [Fact]
    public async Task TheNewAddressButton_OnAnAddressThatBecameAnotherAccounts_Answers422_ReturnsTheConsumption_AndChangesNothing_T514_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Guid customerId = await SeedCustomerAsync("eski@example.com", verified: true);
        string key = await RequestEmailChangeAsync(factory, customerId, "yeni@example.com");
        await SeedCustomerAsync("yeni@example.com", verified: true);

        using HttpResponseMessage post = await PostAsync(client, $"{FramePaths.Verification}/{key}", "VerifyEmail", []);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, post.StatusCode);
        Assert.Contains("başka bir hesaba ait", await BodyOf(post), StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal("eski@example.com", await db.Customers.Where(c => c.Id == customerId).Select(c => c.Email).SingleAsync(Ct));
        Assert.Null(await db.LinkRecords.Where(l => l.LinkType == LinkType.NewEmailVerification).Select(l => l.UsedAt).SingleAsync(Ct));
    }

    [Fact]
    public async Task TheGoogleConfirmationButton_LinksTheAccount_Answers303_AndOpensNoSession_T514_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (Guid customerId, string key) = await SeedLinkedCustomerAsync(factory, "ayse@example.com", LinkType.GoogleLinkConfirmation, verified: true);

        (_, string html, _) = await GetAsync(factory, $"{FramePaths.Verification}/{key}");
        using HttpResponseMessage post = await PostAsync(client, $"{FramePaths.Verification}/{key}", "VerifyEmail", []);
        (_, string done, _) = await GetAsync(factory, post.Headers.Location!.OriginalString);

        Assert.Contains("Google girişini onayla", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, post.StatusCode);
        Assert.EndsWith($"?notice={NoticeCode.GoogleLinkConfirmed}", post.Headers.Location!.OriginalString, StringComparison.Ordinal);
        Assert.Contains("Google girişi hesabınıza bağlandı", done, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.NotNull(await db.Customers.Where(c => c.Id == customerId).Select(c => c.GoogleLinkedAt).SingleAsync(Ct));
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
    }

    [Fact]
    public async Task TheLandingPage_SendsNoReferrer_AndIsNotStored_T514_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open(gateOpen: true);
        (_, string key) = await SeedLinkedCustomerAsync(factory, "ayse@example.com", LinkType.EmailVerification, verified: false);
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false });
        using HttpRequestMessage request = new(HttpMethod.Get, $"{FramePaths.Verification}/{key}");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");

        using HttpResponseMessage response = await client.SendAsync(request, Ct);

        Assert.Equal("no-referrer", string.Join(",", response.Headers.GetValues("Referrer-Policy")));
        Assert.Contains("no-store", string.Join(",", response.Headers.GetValues("Cache-Control")), StringComparison.Ordinal);
    }

    // ---- helpers ----

    private static List<(string Name, string Value)> Form(string email) => [("Name", "Ayşe"), ("Email", email), ("Password", Entered)];

    private WebApplicationFactory<Program> Open(bool gateOpen) =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddRazorPages().AddApplicationPart(typeof(FrameSampleModel).Assembly);
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

    private async Task<Guid> SeedCustomerAsync(string email, bool verified)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Customer customer = Customer.Create("Ali", email, "digest-placeholder", verified ? now : null, null, now);
        await using AppDbContext db = NewContext();
        db.Customers.Add(customer);
        if (!verified)
        {
            db.LinkRecords.Add(LinkRecord.ForCustomer(LinkType.EmailVerification, customer.Id, $"hash-{_counter++}-{Guid.NewGuid():N}", now.AddHours(ProductConstants.EmailVerificationLinkHours), now));
        }

        await db.SaveChangesAsync(Ct);
        return customer.Id;
    }

    // A customer with one link of the given type; returns the account and the raw key as the e-mail carries it.
    private async Task<(Guid CustomerId, string Key)> SeedLinkedCustomerAsync(
        WebApplicationFactory<Program> factory, string email, LinkType type, bool verified, DateTimeOffset? linkEnds = null, bool used = false, bool superseded = false)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        LinkKeys keys = scope.ServiceProvider.GetRequiredService<LinkKeys>();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Customer customer = Customer.Create("Ali", email, "digest-placeholder", verified ? now : null, null, now);
        LinkRecord link = LinkRecord.ForCustomer(keys, type, customer.Id, linkEnds ?? now.AddHours(ProductConstants.EmailVerificationLinkHours), now.AddSeconds(_counter++));
        if (used)
        {
            link.UsedAt = now;
        }

        if (superseded)
        {
            link.SupersededAt = now;
        }

        await using AppDbContext db = NewContext();
        db.Customers.Add(customer);
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return (customer.Id, keys.Derive(link.Id));
    }

    private async Task<string> SeedAdminLinkAsync(WebApplicationFactory<Program> factory)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        LinkKeys keys = scope.ServiceProvider.GetRequiredService<LinkKeys>();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        (_, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        LinkRecord link = LinkRecord.ForAdmin(keys, LinkType.NewEmailVerification, adminId, now.AddHours(1), now);
        await using AppDbContext db = NewContext();
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return keys.Derive(link.Id);
    }

    private static async Task<string> RequestEmailChangeAsync(WebApplicationFactory<Program> factory, Guid customerId, string newEmail)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        ITransactionRunner transactions = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        IMembershipModule membership = scope.ServiceProvider.GetRequiredService<IMembershipModule>();
        LinkKeys keys = scope.ServiceProvider.GetRequiredService<LinkKeys>();
        ISessionService sessions = scope.ServiceProvider.GetRequiredService<ISessionService>();
        // The session is saved before the permission is granted: the grant reads the session row from the database.
        GrantedPermission opened = await transactions.RunAsync(async cancel => new GrantedPermission((await sessions.SignInAsync(customerId, true, null, cancel)).SessionId, null), Ct);
        GrantedPermission granted = await transactions.RunAsync(
            async cancel => opened with { Held = await sessions.GrantReauthenticationAsync(customerId, opened.SessionId, ReauthenticationOperation.EmailChange, cancel) },
            Ct);
        EmailChangeRequestResult result = await transactions.RunAsync(async cancel => await membership.RequestEmailChangeAsync(customerId, granted.SessionId, newEmail, granted.Held, null, cancel), Ct);
        Assert.True(result.IsRequested);
        return keys.Derive(result.LinkId!.Value);
    }

    private sealed record GrantedPermission(Guid SessionId, string? Held) : IOperationResult
    {
        public bool IsRejected => false;
    }

    private static string ValueOf(HttpResponseMessage registered)
    {
        Assert.Equal(HttpStatusCode.SeeOther, registered.StatusCode);
        return Uri.UnescapeDataString(registered.Headers.Location!.OriginalString.Split($"{RegisterModel.PendingParameter}=")[1].Split('&')[0]);
    }

    private static async Task<(HttpStatusCode Status, string Html, IReadOnlyList<string> SetCookies)> GetAsync(WebApplicationFactory<Program> factory, string path)
    {
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false });
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, await BodyOf(response), SetCookies(response));
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string handler, IEnumerable<(string Name, string Value)> fields)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{path}?handler={handler}");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        return await client.SendAsync(request, Ct);
    }

    // The default encoder writes Turkish letters as character references; the texts are compared as the browser reads them.
    private static async Task<string> BodyOf(HttpResponseMessage response) => WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

    private static List<string> SetCookies(HttpResponseMessage response) => response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values) ? [.. values] : [];
}
