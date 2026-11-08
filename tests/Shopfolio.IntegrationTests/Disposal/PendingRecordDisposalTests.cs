using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Membership;

namespace Shopfolio.IntegrationTests.Disposal;

/// <summary>
/// Z-1's nightly deletion on a real PostgreSQL (T324 criterion 1, KK1; 03 4.1.1; 02 3.13.5; 06 9.4.1; D-253): an unverified registration whose first
/// link has ended goes - a later link asked again does not lengthen it - and a pending address change drops after its life; the counts are in the
/// disposal record of the same transaction; a verified account, a registration whose link still runs and a young change stay. The lazy deletion of
/// the registration reads the same rule.
/// </summary>
[Collection(DisposalDefinition.Name)]
public sealed class PendingRecordDisposalTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 6, 0, 0, TimeSpan.Zero);

    private static readonly TimeSpan LinkLife = TimeSpan.FromHours(ProductConstants.EmailVerificationLinkHours);

    private readonly CollectionDatabaseFixture _collection;
    private ServiceProvider _provider = null!;

    public PendingRecordDisposalTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using (NpgsqlConnection connection = new(_collection.Dsn))
        {
            await connection.OpenAsync(Ct);
            await using NpgsqlCommand truncate = new("TRUNCATE pending_email_changes, link_records, disposal_records, carts, customers, admins CASCADE", connection);
            await truncate.ExecuteNonQueryAsync(Ct);
        }

        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        _provider = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    [Fact]
    public async Task EndedRegistrations_Go_WithTheirCountInTheDisposalRecord_AndTheOthersStay_KK1()
    {
        // Ended: the first link ended a minute ago, the link asked again ends where the first one ends (02 3.13.5).
        Guid ended = await SeedCustomerAsync("ended@example.com", verified: false, Now - LinkLife - TimeSpan.FromMinutes(1));
        await SeedLinkAsync(ended, Now - TimeSpan.FromMinutes(1), createdAt: Now - LinkLife - TimeSpan.FromMinutes(1));
        await SeedLinkAsync(ended, Now - TimeSpan.FromMinutes(1), createdAt: Now - TimeSpan.FromHours(2));

        // Ended without any link; still running; verified long ago.
        Guid noLink = await SeedCustomerAsync("nolink@example.com", verified: false, Now - LinkLife);
        Guid running = await SeedCustomerAsync("running@example.com", verified: false, Now - TimeSpan.FromHours(1));
        await SeedLinkAsync(running, Now - TimeSpan.FromHours(1) + LinkLife, createdAt: Now - TimeSpan.FromHours(1));
        Guid verified = await SeedCustomerAsync("verified@example.com", verified: true, Now - LinkLife - LinkLife);
        await SeedLinkAsync(verified, Now - LinkLife, createdAt: Now - LinkLife - LinkLife);

        await RunAsync(new PendingRegistrationDisposalPart());

        await using AppDbContext db = NewContext();
        List<Guid> left = await db.Customers.Select(c => c.Id).ToListAsync(Ct);
        Assert.Equal(new[] { running, verified }.Order(), left.Order());
        Assert.DoesNotContain(ended, left);
        Assert.DoesNotContain(noLink, left);
        Assert.False(await db.LinkRecords.AnyAsync(l => l.CustomerId == ended, Ct));
        DisposalRecord record = Assert.Single(await db.DisposalRecords.ToListAsync(Ct));
        Assert.Equal(DisposalRecordType.PendingRegistration, record.DisposalRecordType);
        Assert.Equal(2, record.Count);
    }

    [Fact]
    public async Task EndedEmailChanges_Drop_ForACustomerAndAnAdministrator_AndAYoungOneStays_KK1()
    {
        Guid customer = await SeedCustomerAsync("owner@example.com", verified: true, Now - LinkLife - LinkLife);
        Guid other = await SeedCustomerAsync("other@example.com", verified: true, Now - LinkLife - LinkLife);
        Admin admin = Admin.Create("Yönetici", "admin@example.com", "digest-placeholder", Now - LinkLife - LinkLife);
        await using (AppDbContext seed = NewContext())
        {
            seed.Admins.Add(admin);
            seed.PendingEmailChanges.Add(PendingEmailChange.ForCustomer(customer, "new-owner@example.com", Now - LinkLife));
            seed.PendingEmailChanges.Add(PendingEmailChange.ForAdmin(admin.Id, "new-admin@example.com", Now - LinkLife - TimeSpan.FromSeconds(1)));
            seed.PendingEmailChanges.Add(PendingEmailChange.ForCustomer(other, "new-other@example.com", Now - LinkLife + TimeSpan.FromSeconds(1)));
            await seed.SaveChangesAsync(Ct);
        }

        await RunAsync(new PendingEmailChangeDisposalPart());

        await using AppDbContext db = NewContext();
        PendingEmailChange left = Assert.Single(await db.PendingEmailChanges.ToListAsync(Ct));
        Assert.Equal(other, left.CustomerId);
        DisposalRecord record = Assert.Single(await db.DisposalRecords.ToListAsync(Ct));
        Assert.Equal(DisposalRecordType.PendingEmailChange, record.DisposalRecordType);
        Assert.Equal(2, record.Count);
    }

    [Fact]
    public async Task NothingEnded_WritesNoDisposalRecord()
    {
        Guid running = await SeedCustomerAsync("young@example.com", verified: false, Now);
        await SeedLinkAsync(running, Now + LinkLife, createdAt: Now);

        await RunAsync(new PendingRegistrationDisposalPart());
        await RunAsync(new PendingEmailChangeDisposalPart());

        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Customers.CountAsync(Ct));
        Assert.False(await db.DisposalRecords.AnyAsync(Ct));
    }

    private async Task RunAsync(IDisposalPart part)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        await part.RunAsync(new DisposalRun(scope.ServiceProvider, Now), Ct);
    }

    private async Task<Guid> SeedCustomerAsync(string email, bool verified, DateTimeOffset createdAt)
    {
        Customer customer = Customer.Create("Ali", email, "digest-placeholder", verified ? createdAt : null, null, createdAt);
        await using AppDbContext db = NewContext();
        db.Customers.Add(customer);
        await db.SaveChangesAsync(Ct);
        return customer.Id;
    }

    private async Task SeedLinkAsync(Guid customerId, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        await using AppDbContext db = NewContext();
        db.LinkRecords.Add(LinkRecord.ForCustomer(LinkType.EmailVerification, customerId, "digest-" + Guid.NewGuid().ToString("N"), expiresAt, createdAt));
        await db.SaveChangesAsync(Ct);
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }
}
