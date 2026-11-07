using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the account e-mail tests (T309): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(AccountEmailQueueDefinition.Name)]
public sealed class AccountEmailQueueDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "AccountEmailQueue";
}

/// <summary>
/// The account and administration e-mails on a real PostgreSQL, written as the application user (T309 criteria 1-3, 5, 7, 9, 10; KK1-KK3, KK5, KK7, KK9, KK10; 08 2.2.8;
/// 06 4.58): each of the six kinds is one "queued" row in the caller's transaction that satisfies the table's type and related record rule and names its link record by id;
/// the body is made at the time of sending after the link record is read - a link used, superseded, expired or gone gives no body, a link of another type or owner is
/// unreadable, a retried e-mail says the whole life and goes out as long as the link lives; only the invitation's row can carry the "did not arrive" mark; the Google
/// confirmation makes no new link and no new e-mail while a valid one exists and makes the next one when it ended.
/// </summary>
[Collection(AccountEmailQueueDefinition.Name)]
public sealed class AccountEmailQueueTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);
    private static readonly LinkKeys _keys = new(Options.Create(new LinkKeyOptions { DerivationSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(LinkKeyOptions.DerivationSecretMinBytes)) }));

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Now);

    public AccountEmailQueueTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string AppDsn => _template.DsnFor(_collection.DatabaseName, _template.Users.App, _template.Users.AppPassword, pooling: false);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE dispatch_records, link_records, admin_invitations, customers, admins CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1, KK2: one queued row per kind, named link record, no key, table rule ----

    [Fact]
    public async Task EachKind_IsOneQueuedRowToItsRecipient_WithItsRelatedRecord_NoOrderLink_AndTheLinkRecordsIdButNeverTheKey()
    {
        Customer customer = await SeedCustomerAsync("ali@example.com");
        Admin admin = await SeedAdminAsync("Ali Yılmaz", "ali.admin@example.com");
        AdminInvitation invitation = await SeedInvitationAsync("davetli@example.com", admin);
        Guid link = Guid.CreateVersion7();

        AccountEmail[] emails =
        [
            new VerificationLinkEmail(customer.Id, link, "ali@example.com"),
            new PasswordResetLinkEmail(DispatchRelatedRecordType.Customer, customer.Id, link, "ali@example.com", false),
            new PasswordResetLinkEmail(DispatchRelatedRecordType.Admin, admin.Id, link, "ali.admin@example.com", true),
            new NewAddressVerificationEmail(DispatchRelatedRecordType.Customer, customer.Id, link, "yeni@example.com"),
            new NewAddressVerificationEmail(DispatchRelatedRecordType.Admin, admin.Id, link, "yeni.admin@example.com"),
            new AddressChangeNoticeEmail(DispatchRelatedRecordType.Customer, customer.Id, link, "eski@example.com", Now),
            new AddressChangeNoticeEmail(DispatchRelatedRecordType.Admin, admin.Id, link, "eski.admin@example.com", Now),
            new AdminInvitationEmail(invitation.Id, link, "davetli@example.com", "Ali Yılmaz"),
            new AccountDeletedEmail(customer.Id, "ali@example.com", Now),
        ];
        foreach (AccountEmail email in emails)
        {
            Assert.Equal(1, await RunAsync(queue => Task.FromResult(queue.Queue(email))));
        }

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(emails.Length, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Null(row.OrderId);
            Assert.Equal(DispatchState.Queued, row.DispatchState);
            Assert.Equal(Now, row.CreatedAt);
            Assert.NotNull(row.RelatedRecordId);
            Assert.DoesNotContain(_keys.Derive(link), row.EventValues ?? string.Empty, StringComparison.Ordinal);
        });
        Assert.All(rows.Where(r => r.DispatchType != DispatchType.AccountDeletedNotice), row => Assert.Contains(link.ToString("D"), row.EventValues, StringComparison.Ordinal));
        Assert.Equal("yeni@example.com", Assert.Single(rows, r => r.DispatchType == DispatchType.NewEmailVerificationLink && r.RelatedRecordType == DispatchRelatedRecordType.Customer).RecipientEmail);
        Assert.Equal("eski@example.com", Assert.Single(rows, r => r.DispatchType == DispatchType.EmailChangeNotice && r.RelatedRecordType == DispatchRelatedRecordType.Customer).RecipientEmail);
        DispatchRecord invited = Assert.Single(rows, r => r.DispatchType == DispatchType.AdminInvitation);
        Assert.Equal("davetli@example.com", invited.RecipientEmail);
        Assert.Equal(DispatchRelatedRecordType.AdminInvitation, invited.RelatedRecordType);
        Assert.Equal(invitation.Id, invited.RelatedRecordId);
    }

    [Fact]
    public async Task TheDeletionNoticeRow_KeepsTheAddressAndTheDate_AfterTheCustomerIsDeleted()
    {
        Customer customer = await SeedCustomerAsync("ali@example.com");
        await RunAsync(queue => Task.FromResult(queue.Queue(new AccountDeletedEmail(customer.Id, "ali@example.com", Now))));
        await ExecuteAsync("DELETE FROM customers");

        DispatchRecord row = await ReadSingleAsync();
        AccountEmailBody body = await MakeBodyAsync(row);

        Assert.Equal("ali@example.com", row.RecipientEmail);
        Assert.Equal(AccountEmailBodyStatus.Ready, body.Status);
        Assert.Contains(body.Content!.Blocks.OfType<EmailParagraph>(), p => p.Text == "Hesabınız 5 Kasım 2026 tarihinde silindi.");
        Assert.Empty(body.Content.Blocks.OfType<EmailButton>());
    }

    [Fact]
    public async Task TheRow_GoesWithTheTransaction_AndWithoutOneNothingIsWritten()
    {
        Customer customer = await SeedCustomerAsync("ali@example.com");
        await RunAsync(queue => Task.FromResult(queue.Queue(new VerificationLinkEmail(customer.Id, Guid.CreateVersion7(), "ali@example.com"))), rejected: true);

        Assert.Equal(0, await CountAsync());

        await using AppDbContext db = NewContext();
        AccountEmailQueue queue = NewQueue(db);
        Assert.Throws<InvalidOperationException>(() => queue.Queue(new VerificationLinkEmail(customer.Id, Guid.CreateVersion7(), "ali@example.com")));
        await Assert.ThrowsAsync<InvalidOperationException>(() => queue.QueueGoogleConfirmationAsync(customer.Id, "ali@example.com", Ct));
        Assert.Equal(0, await CountAsync());
    }

    // ---- KK3, KK7: the body is made at the time of sending, after the link record is read ----

    [Fact]
    public async Task AValidLink_GivesTheBody_AndARetriedEmailSaysTheWholeLifeUntilTheLinkEnds()
    {
        Customer customer = await SeedCustomerAsync("ali@example.com");
        LinkRecord link = await SeedLinkAsync(l => LinkRecord.ForCustomer(_keys, LinkType.EmailVerification, customer.Id, Now.AddHours(ProductConstants.EmailVerificationLinkHours), Now));
        await RunAsync(queue => Task.FromResult(queue.Queue(new VerificationLinkEmail(customer.Id, link.Id, "ali@example.com"))));
        DispatchRecord row = await ReadSingleAsync();

        string whole = $"{ProductConstants.EmailVerificationLinkHours} saat geçerlidir";
        foreach (TimeSpan late in new[] { TimeSpan.Zero, TimeSpan.FromMinutes(5), TimeSpan.FromHours(2), TimeSpan.FromHours(ProductConstants.EmailVerificationLinkHours) - TimeSpan.FromMinutes(1) })
        {
            _time.SetUtcNow(Now + late);
            AccountEmailBody body = await MakeBodyAsync(row);

            Assert.Equal(AccountEmailBodyStatus.Ready, body.Status);
            Assert.Contains(body.Content!.Blocks.OfType<EmailParagraph>(), p => p.Text.Contains(whole, StringComparison.Ordinal));
            Assert.Contains(Uri.EscapeDataString(_keys.Derive(link.Id)), Assert.Single(body.Content.Blocks.OfType<EmailButton>()).Link.AbsoluteUri, StringComparison.Ordinal);
        }

        _time.SetUtcNow(Now.AddHours(ProductConstants.EmailVerificationLinkHours));
        Assert.Equal(AccountEmailBodyStatus.LinkNotValid, (await MakeBodyAsync(row)).Status);
    }

    [Theory]
    [InlineData("used")]
    [InlineData("superseded")]
    [InlineData("expired")]
    public async Task ALinkUsedSupersededOrExpiredByTheTimeOfSending_GivesNoBody(string reason)
    {
        Customer customer = await SeedCustomerAsync("ali@example.com");
        LinkRecord link = await SeedLinkAsync(l => LinkRecord.ForCustomer(_keys, LinkType.PasswordReset, customer.Id, Now.AddMinutes(ProductConstants.PasswordResetLinkMinutes), Now));
        await RunAsync(queue => Task.FromResult(queue.Queue(new PasswordResetLinkEmail(DispatchRelatedRecordType.Customer, customer.Id, link.Id, "ali@example.com", false))));
        DispatchRecord row = await ReadSingleAsync();
        Assert.Equal(AccountEmailBodyStatus.Ready, (await MakeBodyAsync(row)).Status);

        switch (reason)
        {
            case "used":
                await ExecuteAsync($"UPDATE link_records SET used_at = now() WHERE id = '{link.Id}'");
                break;
            case "superseded":
                await ExecuteAsync($"UPDATE link_records SET superseded_at = now() WHERE id = '{link.Id}'");
                break;
            default:
                _time.SetUtcNow(Now.AddMinutes(ProductConstants.PasswordResetLinkMinutes + 1));
                break;
        }

        AccountEmailBody body = await MakeBodyAsync(row);

        Assert.Equal(AccountEmailBodyStatus.LinkNotValid, body.Status);
        Assert.Null(body.Content);
    }

    [Fact]
    public async Task ALinkWhoseOwnerIsGone_GivesNoBody()
    {
        Customer customer = await SeedCustomerAsync("ali@example.com");
        LinkRecord link = await SeedLinkAsync(l => LinkRecord.ForCustomer(_keys, LinkType.EmailVerification, customer.Id, Now.AddHours(1), Now));
        await RunAsync(queue => Task.FromResult(queue.Queue(new VerificationLinkEmail(customer.Id, link.Id, "ali@example.com"))));
        await ExecuteAsync("DELETE FROM customers");

        Assert.Equal(0, await ExecuteScalarAsync("SELECT count(*) FROM link_records"));
        Assert.Equal(AccountEmailBodyStatus.LinkNotValid, (await MakeBodyAsync(await ReadSingleAsync())).Status);
    }

    [Fact]
    public async Task ALinkOfAnotherTypeOrAnotherOwner_IsUnreadable_AndAMissingLinkNameIsToo()
    {
        Customer ali = await SeedCustomerAsync("ali@example.com");
        Customer veli = await SeedCustomerAsync("veli@example.com");
        LinkRecord reset = await SeedLinkAsync(l => LinkRecord.ForCustomer(_keys, LinkType.PasswordReset, ali.Id, Now.AddHours(1), Now));
        await RunAsync(queue => Task.FromResult(queue.Queue(new VerificationLinkEmail(ali.Id, reset.Id, "ali@example.com"))));
        await RunAsync(queue => Task.FromResult(queue.Queue(new PasswordResetLinkEmail(DispatchRelatedRecordType.Customer, veli.Id, reset.Id, "veli@example.com", false))));

        foreach (DispatchRecord row in await ReadAllAsync())
        {
            Assert.Equal(AccountEmailBodyStatus.Unreadable, (await MakeBodyAsync(row)).Status);
        }

        await using AppDbContext readDb = NewContext();
        AccountEmailBody missingName = await NewBodies(readDb).MakeAsync(DispatchType.EmailVerificationLink, DispatchRelatedRecordType.Customer, ali.Id, "[]", "Örnek Mağaza", Ct);
        Assert.Equal(AccountEmailBodyStatus.Unreadable, missingName.Status);
    }

    [Fact]
    public async Task TheInvitationLink_BelongsToTheInvitation_AndAnAdministratorsRevertLinkToTheAdministrator()
    {
        Admin admin = await SeedAdminAsync("Ali Yılmaz", "ali.admin@example.com");
        AdminInvitation invitation = await SeedInvitationAsync("davetli@example.com", admin);
        LinkRecord invitationLink = await SeedLinkAsync(l => LinkRecord.ForInvitation(_keys, invitation.Id, Now.AddHours(ProductConstants.AdminInvitationLinkHours), Now));
        LinkRecord revert = await SeedLinkAsync(l => LinkRecord.ForAdmin(_keys, LinkType.EmailChangeRevert, admin.Id, Now.AddDays(ProductConstants.EmailChangeRevertLinkDays), Now));
        await RunAsync(queue => Task.FromResult(queue.Queue(new AdminInvitationEmail(invitation.Id, invitationLink.Id, "davetli@example.com", "Ali Yılmaz"))));
        await RunAsync(queue => Task.FromResult(queue.Queue(new AddressChangeNoticeEmail(DispatchRelatedRecordType.Admin, admin.Id, revert.Id, "eski.admin@example.com", Now))));

        AccountEmailBody invited = await MakeBodyAsync((await ReadAllAsync()).Single(r => r.DispatchType == DispatchType.AdminInvitation));
        AccountEmailBody notice = await MakeBodyAsync((await ReadAllAsync()).Single(r => r.DispatchType == DispatchType.EmailChangeNotice));

        Assert.Equal(AccountEmailBodyStatus.Ready, invited.Status);
        Assert.Equal(AccountEmailBodyStatus.Ready, notice.Status);
        Assert.StartsWith("https://magaza.example.com/panel/davet/", Assert.Single(invited.Content!.Blocks.OfType<EmailButton>()).Link.AbsoluteUri, StringComparison.Ordinal);
        Assert.StartsWith("https://magaza.example.com/panel/geri-al/", Assert.Single(notice.Content!.Blocks.OfType<EmailButton>()).Link.AbsoluteUri, StringComparison.Ordinal);
    }

    // ---- KK5, KK9: only the invitation's row carries the mark ----

    [Fact]
    public async Task OnlyTheInvitationsUndeliveredRow_PutsTheMark_NoOtherKindAndNotTheFirmNoticeOfTheSameInvitation()
    {
        Customer customer = await SeedCustomerAsync("ali@example.com");
        Admin admin = await SeedAdminAsync("Ali Yılmaz", "ali.admin@example.com");
        AdminInvitation invitation = await SeedInvitationAsync("davetli@example.com", admin);
        Guid link = Guid.CreateVersion7();
        await RunAsync(queue => Task.FromResult(queue.Queue(new VerificationLinkEmail(customer.Id, link, "ali@example.com"))));
        await RunAsync(queue => Task.FromResult(queue.Queue(new PasswordResetLinkEmail(DispatchRelatedRecordType.Customer, customer.Id, link, "ali@example.com", false))));
        await RunAsync(queue => Task.FromResult(queue.Queue(new AccountDeletedEmail(customer.Id, "ali@example.com", Now))));
        await RunAsync(queue => Task.FromResult(queue.Queue(new AdminInvitationEmail(invitation.Id, link, "davetli@example.com", "Ali Yılmaz"))));
        await using (AppDbContext write = NewContext())
        {
            write.DispatchRecords.Add(DispatchRecord.Queue(DispatchType.F6, "ali.admin@example.com", null, DispatchRelatedRecordType.AdminInvitation, Guid.CreateVersion7(), Now));
            await write.SaveChangesAsync(Ct);
        }

        await using AppDbContext db = NewContext();
        Assert.False(await DispatchMarks.AccountEmailHasMarkAsync(db, DispatchType.AdminInvitation, invitation.Id, Ct));

        await ExecuteAsync("UPDATE dispatch_records SET dispatch_state = 'Undelivered', next_attempt_at = NULL, attempt_count = 4, last_outcome = 'RecipientError'");

        Assert.True(await DispatchMarks.AccountEmailHasMarkAsync(db, DispatchType.AdminInvitation, invitation.Id, Ct));
        Assert.False(await DispatchMarks.AccountEmailHasMarkAsync(db, DispatchType.AdminInvitation, Guid.CreateVersion7(), Ct));
        Assert.False(await DispatchMarks.AccountEmailHasMarkAsync(db, DispatchType.EmailVerificationLink, customer.Id, Ct));
        Assert.False(await DispatchMarks.AccountEmailHasMarkAsync(db, DispatchType.PasswordResetLink, customer.Id, Ct));
        Assert.False(await DispatchMarks.AccountEmailHasMarkAsync(db, DispatchType.AccountDeletedNotice, customer.Id, Ct));
        Assert.False(await DispatchMarks.AccountEmailHasMarkAsync(db, DispatchType.GoogleLinkConfirmationLink, customer.Id, Ct));
    }

    // ---- KK10: the Google confirmation ----

    [Fact]
    public async Task TheGoogleConfirmation_MakesTheLinkAndTheRow_AndNothingWhileAValidOneExists()
    {
        Customer customer = await SeedCustomerAsync("ali@example.com");

        Assert.True(await RunGoogleAsync(customer.Id, "ali@example.com"));
        LinkRecord link = await ReadSingleLinkAsync();
        DispatchRecord row = await ReadSingleAsync();
        Assert.Equal(LinkType.GoogleLinkConfirmation, link.LinkType);
        Assert.Equal(customer.Id, link.CustomerId);
        Assert.Equal(Now, link.CreatedAt);
        Assert.Equal(Now.AddMinutes(ProductConstants.PasswordResetLinkMinutes), link.ExpiresAt);
        Assert.Equal(_keys.DigestFor(link.Id), link.TokenHash);
        Assert.Equal(DispatchType.GoogleLinkConfirmationLink, row.DispatchType);
        Assert.Equal(DispatchRelatedRecordType.Customer, row.RelatedRecordType);
        Assert.Equal(customer.Id, row.RelatedRecordId);
        Assert.Equal("ali@example.com", row.RecipientEmail);
        Assert.DoesNotContain(_keys.Derive(link.Id), row.EventValues, StringComparison.Ordinal);

        // A second Google sign-in while the link lives: no new link, no new e-mail.
        _time.Advance(TimeSpan.FromMinutes(ProductConstants.PasswordResetLinkMinutes - 1));
        Assert.False(await RunGoogleAsync(customer.Id, "ali@example.com"));
        Assert.Equal(1, await ExecuteScalarAsync("SELECT count(*) FROM link_records"));
        Assert.Equal(1, await CountAsync());
    }

    [Fact]
    public async Task TheGoogleConfirmation_MakesTheNextLink_WhenTheEarlierOneIsUsedSupersededOrExpired_AndEachCustomerHasItsOwn()
    {
        Customer customer = await SeedCustomerAsync("ali@example.com");
        Customer other = await SeedCustomerAsync("veli@example.com");
        Assert.True(await RunGoogleAsync(customer.Id, "ali@example.com"));
        Assert.True(await RunGoogleAsync(other.Id, "veli@example.com"));

        await ExecuteAsync($"UPDATE link_records SET used_at = now() WHERE customer_id = '{customer.Id}'");
        Assert.True(await RunGoogleAsync(customer.Id, "ali@example.com"));
        await ExecuteAsync($"UPDATE link_records SET superseded_at = now() WHERE customer_id = '{customer.Id}' AND used_at IS NULL");
        Assert.True(await RunGoogleAsync(customer.Id, "ali@example.com"));
        _time.Advance(TimeSpan.FromMinutes(ProductConstants.PasswordResetLinkMinutes));
        Assert.True(await RunGoogleAsync(customer.Id, "ali@example.com"));
        Assert.False(await RunGoogleAsync(customer.Id, "ali@example.com"));
        Assert.Equal(5, await ExecuteScalarAsync("SELECT count(*) FROM link_records WHERE link_type = 'GoogleLinkConfirmation'"));
        Assert.Equal(5, await CountAsync());
    }

    [Fact]
    public async Task TheGoogleConfirmation_RolledBackWithItsTransaction_LeavesNeitherTheLinkNorTheRow()
    {
        Customer customer = await SeedCustomerAsync("ali@example.com");

        Assert.True(await RunGoogleAsync(customer.Id, "ali@example.com", rejected: true));

        Assert.Equal(0, await ExecuteScalarAsync("SELECT count(*) FROM link_records"));
        Assert.Equal(0, await CountAsync());
    }

    // ---- helpers ----

    private static EmailLinks Links() => new(new SiteAddress(Options.Create(new SiteOptions { Domain = "magaza.example.com" })));

    private static TransactionRunner NewRunner(AppDbContext db) => new(db, new JobEnqueueTracker(), new JobSignal(), new CacheInvalidations(db), new AppCache());

    private AccountEmailQueue NewQueue(AppDbContext db) => new(db, _keys, _time);

    private AccountEmailBodies NewBodies(AppDbContext db) => new(db, _time, Links(), _keys);

    private async Task<int> RunAsync(Func<AccountEmailQueue, Task<int>> work, bool rejected = false)
    {
        await using AppDbContext db = NewContext();
        AccountEmailQueue queue = NewQueue(db);
        int written = 0;
        await NewRunner(db).RunAsync(
            async cancel =>
            {
                written = await work(queue);
                return new Outcome(rejected);
            },
            Ct);
        return written;
    }

    private async Task<bool> RunGoogleAsync(Guid customerId, string email, bool rejected = false)
    {
        await using AppDbContext db = NewContext();
        AccountEmailQueue queue = NewQueue(db);
        bool queued = false;
        await NewRunner(db).RunAsync(
            async cancel =>
            {
                queued = await queue.QueueGoogleConfirmationAsync(customerId, email, cancel);
                return new Outcome(rejected);
            },
            Ct);
        return queued;
    }

    private async Task<AccountEmailBody> MakeBodyAsync(DispatchRecord row)
    {
        await using AppDbContext db = NewContext();
        return await NewBodies(db).MakeAsync(row.DispatchType, row.RelatedRecordType, row.RelatedRecordId, row.EventValues, "Örnek Mağaza", Ct);
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, AppDsn);
        return new AppDbContext(builder.Options);
    }

    private AppDbContext NewOwnerContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<DispatchRecord> ReadSingleAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.AsNoTracking().SingleAsync(Ct);
    }

    private async Task<List<DispatchRecord>> ReadAllAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.AsNoTracking().OrderBy(d => d.CreatedAt).ThenBy(d => d.Id).ToListAsync(Ct);
    }

    private async Task<LinkRecord> ReadSingleLinkAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.LinkRecords.AsNoTracking().SingleAsync(Ct);
    }

    private async Task<int> CountAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.CountAsync(Ct);
    }

    private async Task<Customer> SeedCustomerAsync(string email)
    {
        await using AppDbContext db = NewOwnerContext();
        Customer customer = Customer.Create("Ali", email, "hash", Now, null, Now);
        db.Customers.Add(customer);
        await db.SaveChangesAsync(Ct);
        return customer;
    }

    private async Task<Admin> SeedAdminAsync(string name, string email)
    {
        await using AppDbContext db = NewOwnerContext();
        Admin admin = Admin.Create(name, email, "hash", Now);
        db.Admins.Add(admin);
        await db.SaveChangesAsync(Ct);
        return admin;
    }

    private async Task<AdminInvitation> SeedInvitationAsync(string email, Admin sender)
    {
        await using AppDbContext db = NewOwnerContext();
        AdminInvitation invitation = AdminInvitation.Create(email, sender.Id, sender.Name, Now);
        db.AdminInvitations.Add(invitation);
        await db.SaveChangesAsync(Ct);
        return invitation;
    }

    private async Task<LinkRecord> SeedLinkAsync(Func<int, LinkRecord> make)
    {
        await using AppDbContext db = NewOwnerContext();
        LinkRecord link = make(0);
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(Ct);
        return link;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<long> ExecuteScalarAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return Convert.ToInt64(await command.ExecuteScalarAsync(Ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    private sealed record Outcome(bool Rejected) : IOperationResult
    {
        public bool IsRejected => Rejected;
    }
}
