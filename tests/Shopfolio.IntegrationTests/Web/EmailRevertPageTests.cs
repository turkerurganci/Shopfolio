using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the revert link's landing page tests (T519): one database copied from the template.</summary>
[CollectionDefinition(EmailRevertPageTestsDefinition.Name)]
public sealed class EmailRevertPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "EmailRevertPageTests";
}

/// <summary>
/// The revert link's landing page on the opened application (T519 KK1-KK9; 07 UC-75, UC-76; 04 E-28): opening the link reverts nothing, the three states of the screen, a link of another type or of an
/// administrator is not recognized and is not consumed, the button's POST goes back to the earlier address, ends every session and mark, drops the Google link and the password, sends the reset link,
/// deletes the pending registration of the address, and the 422 and 200 answers of a changed address and a spent link. Accounts, links, sessions and rows are real rows of the collection database.
/// </summary>
[Collection(EmailRevertPageTestsDefinition.Name)]
public sealed class EmailRevertPageTests : IAsyncLifetime
{
    private const string Earlier = "eski@example.com";
    private const string Current = "yeni@example.com";

    private readonly CollectionDatabaseFixture _database;

    private int _counter;

    public EmailRevertPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE customers, link_records, dispatch_records, attempt_records, disposal_records, orders, sessions, recognized_browser_marks CASCADE", Ct);
        // The storefront writes no cookie until the cookie policy has been published (05 6.7.10): the cookie is what the sign-out and the revert delete.
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO legal_text_versions (id, created_at, legal_text_type, version_number, published_at, text) "
            + "VALUES ('01000000-0000-7000-8000-000000519001', now(), 'CookiePolicy', 1, now(), 'policy') ON CONFLICT DO NOTHING",
            Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: opening reverts nothing; the step shows the sentence and the one button ----

    [Fact]
    public async Task OpeningAValidLink_ShowsTheStepAndTheOneButton_AndWritesNothing_T519_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        Seeded seeded = await SeedAsync(factory);
        await SignInAsync(factory, seeded.CustomerId);

        (HttpStatusCode first, string html, _) = await GetAsync(factory, Path(seeded.Key));
        (_, string again, _) = await GetAsync(factory, Path(seeded.Key));

        Assert.Equal(HttpStatusCode.OK, first);
        Assert.Contains(EmailRevertLandings.RevertStep, html, StringComparison.Ordinal);
        Assert.Contains(EmailRevertLandings.RevertStep, again, StringComparison.Ordinal);
        Assert.Equal(1, CountOf(html, "<button type=\"submit\" class=\"button button--primary\""));
        Assert.Contains(EmailRevertLandings.ButtonLabel, html, StringComparison.Ordinal);
        Assert.Contains($"{FramePaths.EmailRevert}/{seeded.Key}?handler=RevertPendingEmailChange", html, StringComparison.Ordinal);
        Assert.DoesNotContain("type=\"password\"", html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(Current, await db.Customers.Where(c => c.Id == seeded.CustomerId).Select(c => c.Email).SingleAsync(Ct));
        Assert.Null(await db.LinkRecords.Where(l => l.Id == seeded.LinkId).Select(l => l.UsedAt).SingleAsync(Ct));
        Assert.Equal(1, await db.Sessions.CountAsync(Ct));
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
    }

    // ---- KK2: three states - the used link tells by its result code, the others are invalid ----

    [Fact]
    public async Task TheInvalidStates_AreToldWithoutAButton_AndAUsedLinkWithoutTheCodeSaysTheRevertWasDone_T519_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        Seeded expired = await SeedAsync(factory, "k1@example.com", linkEnds: DateTimeOffset.UtcNow.AddHours(-1));
        Seeded used = await SeedAsync(factory, "k2@example.com", used: true);
        Seeded superseded = await SeedAsync(factory, "k3@example.com", superseded: true);
        Seeded valid = await SeedAsync(factory, "k4@example.com");

        foreach (string key in new[] { expired.Key, superseded.Key, "no-such-key" })
        {
            (HttpStatusCode status, string html, _) = await GetAsync(factory, Path(key));
            Assert.Equal(HttpStatusCode.OK, status);
            Assert.Contains(EmailRevertLandings.Invalid, html, StringComparison.Ordinal);
            Assert.DoesNotContain("button--primary", html, StringComparison.Ordinal);
            Assert.DoesNotContain("handler=RevertPendingEmailChange", html, StringComparison.Ordinal);
        }

        (_, string usedHtml, _) = await GetAsync(factory, Path(used.Key));
        (_, string usedWithCode, _) = await GetAsync(factory, Path(used.Key) + $"?notice={NoticeCode.EmailChangeReverted}");
        (_, string validWithCode, _) = await GetAsync(factory, Path(valid.Key) + $"?notice={NoticeCode.EmailChangeReverted}");
        (_, string unknownWithCode, _) = await GetAsync(factory, Path("no-such-key") + $"?notice={NoticeCode.EmailChangeReverted}");

        Assert.Contains(EmailRevertLandings.UsedEarlier, usedHtml, StringComparison.Ordinal);
        Assert.Contains(EmailRevertLandings.Reverted, usedWithCode, StringComparison.Ordinal);
        Assert.DoesNotContain("button--primary", usedWithCode, StringComparison.Ordinal);
        Assert.Contains(EmailRevertLandings.RevertStep, validWithCode, StringComparison.Ordinal);
        Assert.DoesNotContain(EmailRevertLandings.Reverted, validWithCode, StringComparison.Ordinal);
        Assert.Contains(EmailRevertLandings.Invalid, unknownWithCode, StringComparison.Ordinal);
    }

    // ---- KK3: another type's link and an administrator's link are not recognized and are not consumed ----

    [Fact]
    public async Task ALinkOfAnotherTypeOrAnAdministrator_IsNotRecognized_AndIsNotConsumed_T519_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Seeded reset = await SeedAsync(factory, "k3a@example.com", type: LinkType.PasswordReset);
        Seeded verification = await SeedAsync(factory, "k3b@example.com", type: LinkType.NewEmailVerification);
        string adminKey = await SeedAdminRevertLinkAsync(factory);

        foreach (string key in new[] { reset.Key, verification.Key, adminKey })
        {
            (HttpStatusCode get, string html, _) = await GetAsync(factory, Path(key));
            using HttpResponseMessage post = await PostAsync(client, Path(key), "RevertPendingEmailChange", null);

            Assert.Equal(HttpStatusCode.OK, get);
            Assert.Contains(EmailRevertLandings.Invalid, html, StringComparison.Ordinal);
            Assert.Equal(HttpStatusCode.OK, post.StatusCode);
            Assert.Contains(EmailRevertLandings.Invalid, await BodyOf(post), StringComparison.Ordinal);
        }

        await using AppDbContext db = NewContext();
        Assert.Equal(3, await db.LinkRecords.CountAsync(l => l.UsedAt == null, Ct));
        Assert.Equal("k3a@example.com", await db.Customers.Where(c => c.Id == reset.CustomerId).Select(c => c.Email).SingleAsync(Ct));
    }

    // ---- KK4, KK5, KK9: the button's POST ----

    [Fact]
    public async Task TheButton_GoesBackToTheEarlierAddress_ConsumesTheLinkOnce_EndsEverySessionAndMark_AndSendsTheResetLink_T519_KK4_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Seeded seeded = await SeedAsync(factory, google: true);
        SessionOpening mine = await SignInAsync(factory, seeded.CustomerId);
        await SignInAsync(factory, seeded.CustomerId);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, seeded.CustomerId);
        Guid earlierResetId = await SeedPasswordResetLinkAsync(factory, seeded.CustomerId);

        using HttpResponseMessage post = await PostAsync(client, Path(seeded.Key), "RevertPendingEmailChange", $"{CookieNames.Session}={mine.SessionKey}");
        (HttpStatusCode doneStatus, string done, _) = await GetAsync(factory, post.Headers.Location!.OriginalString);

        Assert.Equal(HttpStatusCode.SeeOther, post.StatusCode);
        Assert.Equal($"{Path(seeded.Key)}?notice={NoticeCode.EmailChangeReverted}", post.Headers.Location.OriginalString);
        Assert.Equal(HttpStatusCode.OK, doneStatus);
        Assert.Contains(EmailRevertLandings.Reverted, done, StringComparison.Ordinal);
        Assert.DoesNotContain("button--primary", done, StringComparison.Ordinal);
        Assert.Contains(SetCookies(post), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal) && c.Contains("expires=", StringComparison.OrdinalIgnoreCase));

        await using AppDbContext db = NewContext();
        Customer account = await db.Customers.AsNoTracking().SingleAsync(c => c.Id == seeded.CustomerId, Ct);
        Assert.Equal(Earlier, account.Email);
        Assert.Equal(Earlier, account.NormalizedEmail);
        Assert.Null(account.GoogleLinkedAt);
        Assert.Null(account.PasswordHash);
        Assert.NotNull(account.EmailVerifiedAt);
        Assert.NotNull(await db.LinkRecords.Where(l => l.Id == seeded.LinkId).Select(l => l.UsedAt).SingleAsync(Ct));
        Assert.Equal(0, await db.Sessions.CountAsync(s => s.CustomerId == seeded.CustomerId, Ct));
        Assert.Equal(0, await db.RecognizedBrowserMarks.CountAsync(m => m.CustomerId == seeded.CustomerId && m.ExpiresAt > DateTimeOffset.UtcNow, Ct));
        Assert.Equal(seeded.CustomerId, await db.Orders.Where(o => o.Id == orderId).Select(o => o.CustomerId).SingleAsync(Ct));

        DispatchRecord mail = await db.DispatchRecords.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(DispatchType.PasswordResetLink, mail.DispatchType);
        Assert.Equal(Earlier, mail.RecipientEmail);
        Assert.Equal(seeded.CustomerId, mail.RelatedRecordId);
        LinkRecord reset = await db.LinkRecords.AsNoTracking().SingleAsync(l => l.LinkType == LinkType.PasswordReset && l.Id != earlierResetId, Ct);
        Assert.Equal(LinkType.PasswordReset, reset.LinkType);
        Assert.Null(reset.UsedAt);
        Assert.Null(reset.SupersededAt);
        Assert.NotNull(await db.LinkRecords.Where(l => l.Id == earlierResetId).Select(l => l.SupersededAt).SingleAsync(Ct));
    }

    [Fact]
    public async Task TheButton_ForAnAccountWithoutAPassword_SendsNoResetLink_AndLeavesAnotherAccountsCookie_T519_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Seeded seeded = await SeedAsync(factory, withPassword: false, google: true);
        (string otherKey, _) = await TestSessions.CustomerAsync(_database.Dsn);
        Guid earlierResetId = await SeedPasswordResetLinkAsync(factory, seeded.CustomerId);

        using HttpResponseMessage post = await PostAsync(client, Path(seeded.Key), "RevertPendingEmailChange", $"{CookieNames.Session}={otherKey}");

        Assert.Equal(HttpStatusCode.SeeOther, post.StatusCode);
        Assert.DoesNotContain(SetCookies(post), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal));
        await using AppDbContext db = NewContext();
        Customer account = await db.Customers.AsNoTracking().SingleAsync(c => c.Id == seeded.CustomerId, Ct);
        Assert.Equal(Earlier, account.Email);
        Assert.Null(account.GoogleLinkedAt);
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
        Assert.Equal(1, await db.Sessions.CountAsync(Ct));

        // A reset link asked for while the address was changed does not survive the revert, with or without a password: it would let whoever asked for it set the password (02 3.13.14).
        Assert.NotNull(await db.LinkRecords.Where(l => l.Id == earlierResetId).Select(l => l.SupersededAt).SingleAsync(Ct));
    }

    // ---- KK6: the pending registration of the address goes, counted ----

    [Fact]
    public async Task TheButton_DeletesTheUnverifiedPendingRegistrationOfTheAddress_AndCountsItInTheDisposalRecord_T519_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Seeded seeded = await SeedAsync(factory);
        Guid pendingId = await SeedCustomerAsync(Earlier, verified: false);

        using HttpResponseMessage post = await PostAsync(client, Path(seeded.Key), "RevertPendingEmailChange", null);

        Assert.Equal(HttpStatusCode.SeeOther, post.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.False(await db.Customers.AnyAsync(c => c.Id == pendingId, Ct));
        Assert.Equal(Earlier, await db.Customers.Where(c => c.Id == seeded.CustomerId).Select(c => c.Email).SingleAsync(Ct));
        DisposalRecord record = await db.DisposalRecords.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(DisposalRecordType.PendingRegistration, record.DisposalRecordType);
        Assert.Equal(1, record.Count);
    }

    // ---- KK7: another verified account holds the address - 422, nothing is written ----

    [Fact]
    public async Task TheButton_WhenTheEarlierAddressBecameAnotherVerifiedAccounts_Answers422_ReturnsTheConsumption_AndWritesNothing_T519_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Seeded seeded = await SeedAsync(factory, google: true);
        SessionOpening mine = await SignInAsync(factory, seeded.CustomerId);
        await SeedCustomerAsync(Earlier, verified: true);

        using HttpResponseMessage post = await PostAsync(client, Path(seeded.Key), "RevertPendingEmailChange", $"{CookieNames.Session}={mine.SessionKey}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, post.StatusCode);
        string body = await BodyOf(post);
        Assert.Contains(EmailRevertLandings.NotApplied, body, StringComparison.Ordinal);
        Assert.DoesNotContain("button--primary", body, StringComparison.Ordinal);
        Assert.DoesNotContain(SetCookies(post), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal));
        await using AppDbContext db = NewContext();
        Customer account = await db.Customers.AsNoTracking().SingleAsync(c => c.Id == seeded.CustomerId, Ct);
        Assert.Equal(Current, account.Email);
        Assert.NotNull(account.GoogleLinkedAt);
        Assert.NotNull(account.PasswordHash);
        Assert.Null(await db.LinkRecords.Where(l => l.Id == seeded.LinkId).Select(l => l.UsedAt).SingleAsync(Ct));
        Assert.Equal(1, await db.Sessions.CountAsync(s => s.CustomerId == seeded.CustomerId, Ct));
        Assert.Equal(0, await db.DispatchRecords.CountAsync(Ct));
        Assert.Equal(0, await db.DisposalRecords.CountAsync(Ct));
    }

    // ---- KK8: a link spent in between - 200 with the state it is in now; the second post is the same ----

    [Fact]
    public async Task ASecondPost_FindsTheLinkUsed_Answers200WithTheStateItIsInNow_AndWritesNothingMore_T519_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Seeded seeded = await SeedAsync(factory);

        using HttpResponseMessage first = await PostAsync(client, Path(seeded.Key), "RevertPendingEmailChange", null);
        long afterFirst = await RowCountsAsync();
        using HttpResponseMessage second = await PostAsync(client, Path(seeded.Key), "RevertPendingEmailChange", null);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Contains(EmailRevertLandings.UsedEarlier, await BodyOf(second), StringComparison.Ordinal);
        Assert.Equal(afterFirst, await RowCountsAsync());
    }

    // ---- helpers ----

    private static string Path(string key) => $"{FramePaths.EmailRevert}/{key}";

    private static int CountOf(string text, string part)
    {
        int count = 0;
        int at = 0;
        while ((at = text.IndexOf(part, at, StringComparison.Ordinal)) >= 0)
        {
            count++;
            at += part.Length;
        }

        return count;
    }

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(FrameSampleModel).Assembly)));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<long> RowCountsAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.LinkRecords.CountAsync(Ct) + await db.DispatchRecords.CountAsync(Ct) + await db.Sessions.CountAsync(Ct) + await db.DisposalRecords.CountAsync(Ct);
    }

    private async Task<Guid> SeedCustomerAsync(string email, bool verified)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Customer customer = Customer.Create("Ali", email, "digest-placeholder", verified ? now : null, null, now);
        await using AppDbContext db = NewContext();
        db.Customers.Add(customer);
        await db.SaveChangesAsync(Ct);
        return customer.Id;
    }

    // An account whose address was changed from the earlier one to the current one, with the revert link of the earlier address (06 4.42); returns the raw key as the e-mail carries it.
    private async Task<Seeded> SeedAsync(
        WebApplicationFactory<Program> factory,
        string? currentEmail = null,
        LinkType type = LinkType.EmailChangeRevert,
        DateTimeOffset? linkEnds = null,
        bool used = false,
        bool superseded = false,
        bool withPassword = true,
        bool google = false)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        LinkKeys keys = scope.ServiceProvider.GetRequiredService<LinkKeys>();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Customer customer = Customer.Create("Ali", currentEmail ?? Current, withPassword ? "digest-placeholder" : null, now, google ? now : null, now);
        LinkRecord link = LinkRecord.ForCustomer(keys, type, customer.Id, linkEnds ?? now.AddDays(ProductConstants.EmailChangeRevertLinkDays), now.AddSeconds(_counter++));
        if (type == LinkType.EmailChangeRevert)
        {
            link.PreviousEmail = Earlier;
        }

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
        return new Seeded(customer.Id, link.Id, keys.Derive(link.Id));
    }

    private async Task<Guid> SeedPasswordResetLinkAsync(WebApplicationFactory<Program> factory, Guid customerId)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        LinkKeys keys = scope.ServiceProvider.GetRequiredService<LinkKeys>();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        LinkRecord link = LinkRecord.ForCustomer(keys, LinkType.PasswordReset, customerId, now.AddMinutes(ProductConstants.PasswordResetLinkMinutes), now.AddSeconds(_counter++));
        await using AppDbContext db = NewContext();
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return link.Id;
    }

    private async Task<string> SeedAdminRevertLinkAsync(WebApplicationFactory<Program> factory)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        LinkKeys keys = scope.ServiceProvider.GetRequiredService<LinkKeys>();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        (_, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        LinkRecord link = LinkRecord.ForAdmin(keys, LinkType.EmailChangeRevert, adminId, now.AddDays(1), now);
        link.PreviousEmail = Earlier;
        await using AppDbContext db = NewContext();
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return keys.Derive(link.Id);
    }

    private static async Task<SessionOpening> SignInAsync(WebApplicationFactory<Program> factory, Guid customerId)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        ITransactionRunner transactions = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        ISessionService sessions = scope.ServiceProvider.GetRequiredService<ISessionService>();
        return (await transactions.RunAsync(async cancel => new Opened(await sessions.SignInAsync(customerId, true, null, cancel)), Ct)).Opening;
    }

    private static async Task<(HttpStatusCode Status, string Html, IReadOnlyList<string> SetCookies)> GetAsync(WebApplicationFactory<Program> factory, string path)
    {
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false });
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, await BodyOf(response), SetCookies(response));
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string handler, string? cookie)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{path}?handler={handler}");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        request.Content = new FormUrlEncodedContent([]);
        return await client.SendAsync(request, Ct);
    }

    // The default encoder writes Turkish letters as character references; the texts are compared as the browser reads them.
    private static async Task<string> BodyOf(HttpResponseMessage response) => WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

    private static List<string> SetCookies(HttpResponseMessage response) => response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values) ? [.. values] : [];

    private sealed record Seeded(Guid CustomerId, Guid LinkId, string Key);

    private sealed record Opened(SessionOpening Opening) : IOperationResult
    {
        public bool IsRejected => false;
    }
}
