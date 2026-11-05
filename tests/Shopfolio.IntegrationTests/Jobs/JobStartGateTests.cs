using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.IntegrationTests.Jobs;

/// <summary>
/// D-96 K1 (T220; 09 15.1 item 4, 13.1 item 3; 05 6.x.20): the queue takes no row before the start-up rewrite of the pending revert links' digests
/// has run. A start at which the database cannot be reached leaves the step not run; the runner waits without stopping the process; when the
/// database comes back the gate runs the step once, and only then the first row is handed to a handler. The database that "comes back" is a copy
/// of the template that is out of reach under another name until the test gives it back its own.
/// </summary>
public sealed class JobStartGateTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = JobQueueHarness.Start;

    private readonly TemplateDatabaseFixture _template;
    private string _name = string.Empty;
    private string _hiddenName = string.Empty;
    private string _dsn = string.Empty;

    public JobStartGateTests(TemplateDatabaseFixture template)
    {
        _template = template;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        _name = await _template.CreateDatabaseFromTemplateAsync(Ct);
        _hiddenName = _name + "_away";
        _dsn = _template.DsnFor(_name);
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await _template.DropDatabaseAsync(_name, CancellationToken.None);
        await _template.DropDatabaseAsync(_hiddenName, CancellationToken.None);
    }

    [Fact]
    public async Task Gate_WhenTheDatabaseComesBackAfterTheStart_RunsTheRewriteOnce_AndOnlyThenTheQueueTakesARow()
    {
        string rotated = Convert.ToBase64String(RandomNumberGenerator.GetBytes(LinkKeyOptions.DerivationSecretMinBytes));
        LinkKeys oldKeys = Keys(Convert.ToBase64String(RandomNumberGenerator.GetBytes(LinkKeyOptions.DerivationSecretMinBytes)));
        LinkKeys newKeys = Keys(rotated);
        Guid linkId = await SeedQueuedRevertNoticeAsync(oldKeys);
        Assert.Equal(oldKeys.DigestFor(linkId), await DigestOfAsync(linkId));

        // The database is out of reach at the start.
        NpgsqlConnection.ClearAllPools();
        await ExecuteAdminAsync($"ALTER DATABASE {_name} RENAME TO {_hiddenName}");
        string? digestSeenByTheHandler = null;
        await using JobQueueHarness harness = new(
            _dsn,
            [JobType.ImageDerivative],
            configure: services =>
            {
                services.AddSingleton(newKeys);
                services.AddScoped<RevertLinkDigestRewrite>();
                services.AddSingleton<RevertLinkDigestRewriteStartup>();
                services.AddJobStartGate<RevertLinkDigestRewriteGate>();
            });
        RevertLinkDigestRewriteStartup startup = harness.Services.GetRequiredService<RevertLinkDigestRewriteStartup>();
        harness.Script.ByType[JobType.ImageDerivative] = async call =>
        {
            digestSeenByTheHandler = await DigestOfAsync(linkId);
            return JobOutcome.Completed;
        };
        Assert.False(await startup.EnsureRunAsync(Ct));
        JobRunnerService runner = harness.NewRunner();
        await runner.StartAsync(Ct);
        try
        {
            // The runner waits, round after round, and does not stop the process.
            for (int round = 0; round < 3; round++)
            {
                await harness.WaitForTimerAsync();
                harness.Time.Advance(JobTimings.PollInterval);
            }

            await harness.WaitForTimerAsync();
            Assert.False(startup.HasRun);
            Assert.False(runner.ExecuteTask!.IsCompleted);

            // The database comes back, with a row waiting for the queue.
            NpgsqlConnection.ClearAllPools();
            await ExecuteAdminAsync($"ALTER DATABASE {_hiddenName} RENAME TO {_name}");
            Job row = Job.Enqueue(JobType.ImageDerivative, Guid.NewGuid(), null, Now);
            await using (AppDbContext db = NewContext())
            {
                db.Jobs.Add(row);
                await db.SaveChangesAsync(Ct);
            }

            await JobQueueHarness.EventuallyAsync(
                async () =>
                {
                    harness.Time.Advance(JobTimings.PollInterval);
                    return (await harness.RowAsync(row.Id)).JobState == JobState.Completed;
                },
                "the row is handled after the database came back");
        }
        finally
        {
            await runner.StopAsync(CancellationToken.None);
        }

        // The step ran before the first row was given to a handler: the handler saw the digest of the present secret.
        Assert.True(startup.HasRun);
        Assert.Equal(newKeys.DigestFor(linkId), digestSeenByTheHandler);
        Assert.Equal(newKeys.DigestFor(linkId), await DigestOfAsync(linkId));

        // And it ran once: asking again changes nothing.
        string afterFirstRun = await LinkSnapshotAsync();
        Assert.True(await startup.EnsureRunAsync(Ct));
        Assert.True(await startup.EnsureRunAsync(Ct));
        Assert.Equal(afterFirstRun, await LinkSnapshotAsync());
    }

    [Fact]
    public async Task Gate_WhenTheDatabaseIsThereAtTheStart_IsOpenAtOnce_AndTheRewriteHasRun()
    {
        LinkKeys newKeys = Keys(Convert.ToBase64String(RandomNumberGenerator.GetBytes(LinkKeyOptions.DerivationSecretMinBytes)));
        Guid linkId = await SeedQueuedRevertNoticeAsync(Keys(Convert.ToBase64String(RandomNumberGenerator.GetBytes(LinkKeyOptions.DerivationSecretMinBytes))));
        await using JobQueueHarness harness = new(
            _dsn,
            [],
            configure: services =>
            {
                services.AddSingleton(newKeys);
                services.AddScoped<RevertLinkDigestRewrite>();
                services.AddSingleton<RevertLinkDigestRewriteStartup>();
                services.AddJobStartGate<RevertLinkDigestRewriteGate>();
            });

        IJobStartGate gate = Assert.Single(harness.Services.GetServices<IJobStartGate>());

        Assert.True(await gate.IsOpenAsync(Ct));
        Assert.Equal(newKeys.DigestFor(linkId), await DigestOfAsync(linkId));
        Assert.True(harness.Services.GetRequiredService<RevertLinkDigestRewriteStartup>().HasRun);
    }

    // ---- Helpers ----

    private static LinkKeys Keys(string secret) => new(Options.Create(new LinkKeyOptions { DerivationSecret = secret }));

    // A queued revert notice and its link under the given keys, as the e-mail change writes them (05 6.x.15).
    private async Task<Guid> SeedQueuedRevertNoticeAsync(LinkKeys keys)
    {
        Customer customer = Customer.Create("Ali", "ali@example.com", "digest-placeholder", Now, null, Now);
        LinkRecord link = LinkRecord.ForCustomer(keys, LinkType.EmailChangeRevert, customer.Id, Now.AddDays(7), Now);
        link.PreviousEmail = "old@example.com";
        DispatchRecord notice = DispatchRecord.Queue(DispatchType.EmailChangeNotice, "old@example.com", null, DispatchRelatedRecordType.Customer, customer.Id, Now);
        notice.EventValues = LinkEventValues.ForLink(link.Id);
        await using AppDbContext db = NewContext();
        db.Customers.Add(customer);
        await db.SaveChangesAsync(Ct);
        db.LinkRecords.Add(link);
        db.DispatchRecords.Add(notice);
        await db.SaveChangesAsync(Ct);
        return link.Id;
    }

    private async Task<string> DigestOfAsync(Guid linkId)
    {
        await using NpgsqlConnection connection = new(_dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT token_hash FROM link_records WHERE id = '{linkId}'", connection);
        return (string)(await command.ExecuteScalarAsync(Ct))!;
    }

    // The values and the row version of every link row.
    private async Task<string> LinkSnapshotAsync()
    {
        await using NpgsqlConnection connection = new(_dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("SELECT coalesce(string_agg(t.xmin::text || to_jsonb(t)::text, '|' ORDER BY t.id), '') FROM link_records t", connection);
        return (string)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task ExecuteAdminAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_template.DsnFor("postgres"));
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _dsn);
        return new AppDbContext(builder.Options);
    }
}
