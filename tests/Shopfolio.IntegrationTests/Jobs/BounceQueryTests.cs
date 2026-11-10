using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Jobs;

/// <summary>
/// The returned-address query on a real PostgreSQL, the real job runner and a fake clock and e-mail service (T408 criteria 1, 3, 4, 5, 6, KK1, KK3, KK4, KK5, KK6; 08 2.2.3;
/// 05 4.3.4, 5.2.16; K-1241): the row repeats every fifteen minutes and asks from 24 hours before each round; a "sent" row of the window whose recipient is on the list - however
/// the case or the Turkish dotted letters are written - becomes "undelivered" with a "recipient error" and keeps its sending moment, an older row or a queued one does not; the channel
/// state is untouched and a second application changes nothing; a failed round writes nothing, keeps no cursor and does not fail the row; the type has one pending row, kept in place
/// by the scheduler source while a gateway is registered.
/// </summary>
[Collection(JobQueueDefinition.Name)]
public sealed class BounceQueryTests : JobQueueTestBase
{
    private static readonly DateTimeOffset Now = JobQueueHarness.Start;

    public BounceQueryTests(CollectionDatabaseFixture collection)
        : base(collection)
    {
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await ExecuteAsync("TRUNCATE channel_state_records, dispatch_records, jobs CASCADE");
    }

    private JobQueueHarness NewBounceHarness(FakeEmailGateway? gateway) =>
        NewHarness(
            [],
            configure: services =>
            {
                if (gateway is not null)
                {
                    services.AddSingleton<IEmailGateway>(gateway);
                }

                services.AddJobHandler<BounceQueryJobHandler>(JobType.BounceQuery);
                services.AddDueRecordSource<BounceQueryRowSource>();
            });

    // ---- KK1: every fifteen minutes, from 24 hours before the round ----

    [Fact]
    public async Task TheRow_IsTakenEveryFifteenMinutes_AndAsksFromTwentyFourHoursBeforeEachRound_KK1_KK2()
    {
        FakeEmailGateway gateway = new();
        await using JobQueueHarness harness = NewBounceHarness(gateway);
        await harness.EnqueueAsync(JobType.BounceQuery, null);

        Assert.True(await harness.RunOneAsync(Ct));
        Job row = Assert.Single(await harness.RowsAsync());
        Assert.Equal((JobState.Pending, 0, Now + TimeSpan.FromMinutes(15)), (row.JobState, row.AttemptCount, row.NextAttemptAt));
        Assert.Equal(Now - TimeSpan.FromHours(24), Assert.Single(gateway.Calls).Argument);

        harness.Time.Advance(TimeSpan.FromMinutes(14) + TimeSpan.FromSeconds(59));
        Assert.False(await harness.RunOneAsync(Ct));
        Assert.Single(gateway.Calls);

        harness.Time.Advance(TimeSpan.FromSeconds(1));
        Assert.True(await harness.RunOneAsync(Ct));
        Assert.Equal(2, gateway.Calls.Count);
        Assert.Equal(harness.Time.GetUtcNow() - TimeSpan.FromHours(24), gateway.Calls[1].Argument);
        Assert.Equal(harness.Time.GetUtcNow() + TimeSpan.FromMinutes(15), Assert.Single(await harness.RowsAsync()).NextAttemptAt);
    }

    // ---- KK3: the match is case-free, the window is the last 24 hours, only "sent" rows ----

    [Fact]
    public async Task ASentRowOfTheWindow_WhoseRecipientIsOnTheList_BecomesUndelivered_WithARecipientError_AndKeepsItsSendingMoment_KK3()
    {
        FakeEmailGateway gateway = new();
        gateway.EnqueueBounceQuery(new BounceQueryResult.List(["musteri@example.com"]));
        Guid inWindow = await SeedAsync("MUSTERI@Example.COM", DispatchState.Sent, Now.AddHours(-3));
        Guid exactlyAtTheEdge = await SeedAsync("Musteri@example.com", DispatchState.Sent, Now.AddHours(-24));
        Guid tooOld = await SeedAsync("musteri@example.com", DispatchState.Sent, Now.AddHours(-24).AddSeconds(-1));
        Guid otherAddress = await SeedAsync("baska@example.com", DispatchState.Sent, Now.AddHours(-1));
        Guid queued = await SeedAsync("musteri@example.com", DispatchState.Queued, null);
        Dictionary<Guid, DispatchSnapshot> before = await SnapshotAsync();
        await using JobQueueHarness harness = NewBounceHarness(gateway);
        await harness.EnqueueAsync(JobType.BounceQuery, null);

        await harness.RunOneAsync(Ct);

        Dictionary<Guid, DispatchSnapshot> after = await SnapshotAsync();
        foreach (Guid marked in new[] { inWindow, exactlyAtTheEdge })
        {
            Assert.Equal(DispatchState.Undelivered, after[marked].State);
            Assert.Equal(DispatchOutcome.RecipientError, after[marked].LastOutcome);
            Assert.Equal(before[marked].SentAt, after[marked].SentAt);
            Assert.Equal(before[marked] with { State = DispatchState.Undelivered, LastOutcome = DispatchOutcome.RecipientError }, after[marked]);
        }

        foreach (Guid untouched in new[] { tooOld, otherAddress, queued })
        {
            Assert.Equal(before[untouched], after[untouched]);
        }
    }

    [Fact]
    public async Task TheMatch_FoldsTheTurkishDottedLetters_LikeTheOneIdentityNormalization_KK3()
    {
        FakeEmailGateway gateway = new();
        gateway.EnqueueBounceQuery(new BounceQueryResult.List(["ali.işik@example.com", "  BİLGİ@Example.com "]));
        Guid dottedUpper = await SeedAsync("ALİ.IŞIK@example.com", DispatchState.Sent, Now.AddHours(-2));
        Guid dottedLower = await SeedAsync("bilgi@example.com", DispatchState.Sent, Now.AddHours(-2));
        await using JobQueueHarness harness = NewBounceHarness(gateway);
        await harness.EnqueueAsync(JobType.BounceQuery, null);

        await harness.RunOneAsync(Ct);

        Dictionary<Guid, DispatchSnapshot> after = await SnapshotAsync();
        Assert.Equal(DispatchState.Undelivered, after[dottedUpper].State);
        Assert.Equal(DispatchState.Undelivered, after[dottedLower].State);
    }

    // ---- KK4: the channel state is untouched; the second application changes nothing ----

    [Fact]
    public async Task TheChannelState_IsNotTouched_AndASecondApplication_ChangesNothing_KK4()
    {
        DateTimeOffset firstError = Now.AddHours(-1);
        await SeedChannelStateAsync(errorCount: 2, firstError: firstError, lastSucceeded: Now.AddHours(-2));
        FakeEmailGateway gateway = new();
        gateway.EnqueueBounceQuery(new BounceQueryResult.List(["musteri@example.com"]), new BounceQueryResult.List(["musteri@example.com"]));
        Guid row = await SeedAsync("Musteri@example.com", DispatchState.Sent, Now.AddHours(-3));
        await using JobQueueHarness harness = NewBounceHarness(gateway);
        await harness.EnqueueAsync(JobType.BounceQuery, null);

        await harness.RunOneAsync(Ct);
        Dictionary<Guid, DispatchSnapshot> first = await SnapshotAsync();
        harness.Time.Advance(TimeSpan.FromMinutes(15));
        await harness.RunOneAsync(Ct);
        Dictionary<Guid, DispatchSnapshot> second = await SnapshotAsync();

        Assert.Equal(DispatchState.Undelivered, first[row].State);
        Assert.Equal(first, second);
        Assert.Equal((2, firstError, Now.AddHours(-2)), await ReadChannelStateAsync());
    }

    // ---- KK5: a failed round passes empty ----

    [Fact]
    public async Task AThrottledRound_PassesEmpty_WritesNothing_KeepsNoCursor_AndNeverFailsTheRow_KK5()
    {
        await SeedChannelStateAsync(errorCount: 0, firstError: null, lastSucceeded: Now.AddHours(-2));
        FakeEmailGateway gateway = new();
        gateway.EnqueueBounceQuery(new BounceQueryResult.QueryFailed("TooManyRequestsException"), new BounceQueryResult.List(["musteri@example.com"]));
        Guid row = await SeedAsync("musteri@example.com", DispatchState.Sent, Now.AddHours(-3));
        Dictionary<Guid, DispatchSnapshot> before = await SnapshotAsync();
        await using JobQueueHarness harness = NewBounceHarness(gateway);
        await harness.EnqueueAsync(JobType.BounceQuery, null);

        Assert.True(await harness.RunOneAsync(Ct));

        Assert.Equal(before, await SnapshotAsync());
        Assert.Equal((0, (DateTimeOffset?)null, (DateTimeOffset?)Now.AddHours(-2)), await ReadChannelStateAsync());
        Job failed = Assert.Single(await harness.RowsAsync());
        Assert.Equal((JobState.Pending, 0, Now + TimeSpan.FromMinutes(15), null), (failed.JobState, failed.AttemptCount, failed.NextAttemptAt, failed.LastErrorSummary));

        // The next round asks again from 24 hours before its own start - no cursor was kept - and catches the address the missed round could not.
        harness.Time.Advance(TimeSpan.FromMinutes(15));
        Assert.True(await harness.RunOneAsync(Ct));
        Assert.Equal(harness.Time.GetUtcNow() - TimeSpan.FromHours(24), gateway.Calls[1].Argument);
        Assert.Equal(DispatchState.Undelivered, (await SnapshotAsync())[row].State);
    }

    // ---- KK6: one pending row of the type ----

    [Fact]
    public async Task TheRowSource_WithAGateway_WritesTheOnePendingRow_NoSecondOne_AndWritesItAgainWhenItIsClosed_KK6()
    {
        await using JobQueueHarness harness = NewBounceHarness(new FakeEmailGateway());
        IDueRecordSource source = harness.Services.GetServices<IDueRecordSource>().OfType<BounceQueryRowSource>().Single();

        IReadOnlyList<DueRecord> due = await FindDueAsync(harness, source);
        Assert.Single(due);
        await ProcessAsync(harness, source, due[0].Id);
        await ProcessAsync(harness, source, due[0].Id);

        Job row = Assert.Single(await harness.RowsAsync());
        Assert.Equal((JobType.BounceQuery, (Guid?)null, JobState.Pending), (row.JobType, row.RelatedRecordId, row.JobState));

        // The source does not read the job table: it is due at every wake-up and the queue's insert leaves the pending row as it is.
        Assert.Single(await FindDueAsync(harness, source));
        await ProcessAsync(harness, source, Guid.Empty);
        Assert.Equal(1, await ScalarAsync<long>("SELECT count(*) FROM jobs"));

        // The installer closed it (05 4.3.4): the scheduler brings the row back.
        await ExecuteAsync("UPDATE jobs SET job_state = 'Completed', completed_at = now()");
        await ProcessAsync(harness, source, Guid.Empty);
        Assert.Equal(1, await ScalarAsync<long>("SELECT count(*) FROM jobs WHERE job_state = 'Pending' AND job_type = 'BounceQuery'"));
    }

    [Fact]
    public async Task TheRowSource_WithNoGateway_FindsNothingToWrite_KK6()
    {
        await using JobQueueHarness harness = NewBounceHarness(gateway: null);
        IDueRecordSource source = harness.Services.GetServices<IDueRecordSource>().OfType<BounceQueryRowSource>().Single();

        Assert.Empty(await FindDueAsync(harness, source));
        Assert.Empty(await harness.RowsAsync());
    }

    [Fact]
    public async Task ASecondPendingRowOfTheType_IsRefusedByTheDatabase_KK6()
    {
        await using JobQueueHarness harness = NewBounceHarness(new FakeEmailGateway());
        await harness.EnqueueAsync(JobType.BounceQuery, null);

        PostgresException refused = await Assert.ThrowsAsync<PostgresException>(
            () => ExecuteAsync($"INSERT INTO jobs (id, created_at, job_type) VALUES ('{Guid.NewGuid()}', now(), 'BounceQuery')"));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, refused.SqlState);
    }

    private static async Task<IReadOnlyList<DueRecord>> FindDueAsync(JobQueueHarness harness, IDueRecordSource source)
    {
        await using AsyncServiceScope scope = harness.Services.CreateAsyncScope();
        return await source.FindDueAsync(scope.ServiceProvider, harness.Time.GetUtcNow(), Ct);
    }

    private static async Task ProcessAsync(JobQueueHarness harness, IDueRecordSource source, Guid recordId)
    {
        await using AsyncServiceScope scope = harness.Services.CreateAsyncScope();
        await source.ProcessAsync(scope.ServiceProvider, recordId, Ct);
    }

    private async Task<Guid> SeedAsync(string recipient, DispatchState state, DateTimeOffset? sentAt)
    {
        DispatchRecord row = DispatchRecord.Queue(DispatchType.F5, recipient, null, null, null, Now.AddDays(-2));
        row.DispatchState = state;
        if (state == DispatchState.Sent)
        {
            row.SentAt = sentAt;
            row.AttemptCount = 1;
            row.LastOutcome = DispatchOutcome.Sent;
        }
        else
        {
            row.NextAttemptAt = Now.AddMinutes(5);
        }

        await using AppDbContext db = NewContext();
        db.DispatchRecords.Add(row);
        await db.SaveChangesAsync(Ct);
        return row.Id;
    }

    private async Task<Dictionary<Guid, DispatchSnapshot>> SnapshotAsync()
    {
        await using AppDbContext db = NewContext();
        List<DispatchRecord> rows = await db.DispatchRecords.AsNoTracking().ToListAsync(Ct);
        return rows.ToDictionary(r => r.Id, r => new DispatchSnapshot(r.RecipientEmail, r.DispatchState, r.LastOutcome, r.AttemptCount, r.NextAttemptAt, r.SentAt));
    }

    private async Task SeedChannelStateAsync(int errorCount, DateTimeOffset? firstError, DateTimeOffset? lastSucceeded)
    {
        ChannelStateRecord state = ChannelStateRecord.Create(Now.AddDays(-30));
        state.ConsecutiveChannelErrorCount = errorCount;
        state.FirstChannelErrorAt = firstError;
        state.LastSucceededAt = lastSucceeded;
        await using AppDbContext db = NewContext();
        db.ChannelStateRecords.Add(state);
        await db.SaveChangesAsync(Ct);
    }

    private async Task<(int Count, DateTimeOffset? FirstError, DateTimeOffset? LastSucceeded)> ReadChannelStateAsync()
    {
        await using AppDbContext db = NewContext();
        ChannelStateRecord state = await db.ChannelStateRecords.AsNoTracking().SingleAsync(Ct);
        return (state.ConsecutiveChannelErrorCount, state.FirstChannelErrorAt, state.LastSucceededAt);
    }

    private sealed record DispatchSnapshot(string? Recipient, DispatchState State, DispatchOutcome? LastOutcome, int AttemptCount, DateTimeOffset? NextAttemptAt, DateTimeOffset? SentAt);
}
