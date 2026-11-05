using System.Security.Cryptography;

using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.StartupDatabase;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The rewrite of the pending revert links' digests on the opened application (T222 KK8; 05 6.x.20; K-1441): at the start, before the job
/// queue and the e-mail sending can begin, the digest of a queued revert notice's link is written again from the secret the application started
/// with; a sent notice's link is left as it is; a database that cannot be reached does not stop the start and the step is marked as not run.
/// Every test works on its own copy of the template database.
/// </summary>
public sealed class RevertLinkDigestRewriteStartupTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private readonly TemplateDatabaseFixture _template;
    private string _databaseName = string.Empty;
    private string _dsn = string.Empty;

    public RevertLinkDigestRewriteStartupTests(TemplateDatabaseFixture template)
    {
        _template = template;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        _databaseName = await _template.CreateDatabaseFromTemplateAsync(Ct);
        _dsn = _template.DsnFor(_databaseName);
    }

    public async ValueTask DisposeAsync()
    {
        using NpgsqlConnection pooled = new(_dsn);
        NpgsqlConnection.ClearPool(pooled);
        await _template.DropDatabaseAsync(_databaseName, CancellationToken.None);
    }

    [Fact]
    public async Task Start_WithANewSecret_WritesTheQueuedNoticesDigestBeforeAnyLaterServiceStarts_AndLeavesTheSentOnes()
    {
        string rotated = RandomMaterial();
        LinkKeys oldKeys = NewKeys(RandomMaterial());
        LinkKeys newKeys = NewKeys(rotated);
        (Customer queuedOwner, LinkRecord queuedLink, DispatchRecord queued) = PendingRevert(oldKeys);
        (Customer sentOwner, LinkRecord sentLink, DispatchRecord sent) = PendingRevert(oldKeys, customerEmail: "other@example.com");
        sent.DispatchState = DispatchState.Sent;
        sent.SentAt = Now;
        await SeedAsync(queuedOwner, sentOwner, queuedLink, sentLink, queued, sent);
        Assert.Equal(oldKeys.DigestFor(queuedLink.Id), await DigestOfAsync(queuedLink.Id));

        QueueProbe probe = new(_dsn, queuedLink.Id, sentLink.Id);
        await using ShopfolioWebApplicationFactory factory = new(_dsn, new Dictionary<string, string> { [LinkKeyOptions.DerivationSecretKey] = rotated });
        using HttpClient client = factory.WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddSingleton<IHostedService>(probe))).CreateClient();

        RevertLinkDigestRewriteStartup rewrite = factory.Services.GetServices<IHostedService>().OfType<RevertLinkDigestRewriteStartup>().Single();
        Assert.True(rewrite.HasRun);
        Assert.Equal(newKeys.DigestFor(queuedLink.Id), await DigestOfAsync(queuedLink.Id));
        Assert.Equal(oldKeys.DigestFor(sentLink.Id), await DigestOfAsync(sentLink.Id));

        // The service that stands for the job queue and the e-mail sending started after the rewrite and saw the new digest at its first call.
        Assert.True(probe.Started);
        Assert.Equal(newKeys.DigestFor(queuedLink.Id), probe.QueuedDigestAtStart);
        Assert.Equal(oldKeys.DigestFor(sentLink.Id), probe.SentDigestAtStart);
    }

    [Fact]
    public async Task Start_RegistersTheRewriteAfterTheTwoChecksAndBeforeEveryOtherServiceOfTheApplication()
    {
        await using ShopfolioWebApplicationFactory factory = new(_dsn);
        using HttpClient client = factory.CreateClient();

        List<Type> order = [.. factory.Services.GetServices<IHostedService>().Select(s => s.GetType())];

        int rewrite = order.IndexOf(typeof(RevertLinkDigestRewriteStartup));
        Assert.True(rewrite >= 0);
        Assert.True(order.IndexOf(typeof(StartupSettingsCheck)) < order.IndexOf(typeof(StartupDatabaseCheck)));
        Assert.True(order.IndexOf(typeof(StartupDatabaseCheck)) < rewrite);
        Assert.All(
            order.Where(t => t != typeof(StartupSettingsCheck) && t != typeof(StartupDatabaseCheck) && t != typeof(RevertLinkDigestRewriteStartup)
                && t.Namespace?.StartsWith("Shopfolio", StringComparison.Ordinal) == true),
            t => Assert.True(order.IndexOf(t) > rewrite, t.Name + " starts before the rewrite."));
    }

    [Fact]
    public async Task Start_WhenTheDatabaseCannotBeReached_DoesNotStopTheStart_AndTheRewriteIsMarkedNotRun()
    {
        DsnBuilder unreachable = new(_dsn) { Port = 1 };
        await using ShopfolioWebApplicationFactory factory = new(unreachable.ConnectionString);

        using HttpClient client = factory.CreateClient();

        Assert.False(factory.Services.GetServices<IHostedService>().OfType<RevertLinkDigestRewriteStartup>().Single().HasRun);
    }

    // ---- Helpers ----

    // A service registered last, so the host starts it after the rewrite; it stands for the job queue and the sending, which read the digest at once.
    private sealed class QueueProbe : IHostedService
    {
        private readonly string _dsn;
        private readonly Guid _queuedLinkId;
        private readonly Guid _sentLinkId;

        public QueueProbe(string dsn, Guid queuedLinkId, Guid sentLinkId)
        {
            _dsn = dsn;
            _queuedLinkId = queuedLinkId;
            _sentLinkId = sentLinkId;
        }

        public bool Started { get; private set; }

        public string? QueuedDigestAtStart { get; private set; }

        public string? SentDigestAtStart { get; private set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            QueuedDigestAtStart = await ReadDigestAsync(_dsn, _queuedLinkId, cancellationToken);
            SentDigestAtStart = await ReadDigestAsync(_dsn, _sentLinkId, cancellationToken);
            Started = true;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private static async Task<string?> ReadDigestAsync(string dsn, Guid linkId, CancellationToken ct)
    {
        await using NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(ct);
        await using NpgsqlCommand command = new($"SELECT token_hash FROM link_records WHERE id = '{linkId}'", connection);
        return (string?)await command.ExecuteScalarAsync(ct);
    }

    // Made at run time (32 random bytes, Base64): no value of a secret setting is written into the repository.
    private static string RandomMaterial() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(LinkKeyOptions.DerivationSecretMinBytes));

    private static LinkKeys NewKeys(string secret) => new(Options.Create(new LinkKeyOptions { DerivationSecret = secret }));

    private static (Customer Owner, LinkRecord Link, DispatchRecord Notice) PendingRevert(LinkKeys keys, string customerEmail = "ali@example.com")
    {
        Customer customer = Customer.Create("Ali", customerEmail, "digest-placeholder", Now, null, Now);
        LinkRecord link = LinkRecord.ForCustomer(keys, LinkType.EmailChangeRevert, customer.Id, Now.AddDays(7), Now);
        link.PreviousEmail = "old@example.com";
        DispatchRecord notice = DispatchRecord.Queue(DispatchType.EmailChangeNotice, "old@example.com", null, DispatchRelatedRecordType.Customer, customer.Id, Now);
        notice.EventValues = LinkEventValues.ForLink(link.Id);
        return (customer, link, notice);
    }

    // The owners are saved first, then the links and the notices that point at them.
    private async Task SeedAsync(params object[] rows)
    {
        await using AppDbContext context = NewContext();
        context.Customers.AddRange(rows.OfType<Customer>());
        await context.SaveChangesAsync(Ct);
        context.LinkRecords.AddRange(rows.OfType<LinkRecord>());
        context.DispatchRecords.AddRange(rows.OfType<DispatchRecord>());
        await context.SaveChangesAsync(Ct);
    }

    private Task<string?> DigestOfAsync(Guid linkId) => ReadDigestAsync(_dsn, linkId, Ct);

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _dsn);
        return new AppDbContext(builder.Options);
    }
}
