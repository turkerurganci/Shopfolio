using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Outages;

/// <summary>The collection of the outage tests (T204): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(OutagesDefinition.Name)]
public sealed class OutagesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "Outages";
}

/// <summary>
/// The e-mail service's outage on a real PostgreSQL (T204 criteria 4, 5, 7, 8; KK4, KK5, KK7, KK8; 08 2.2.2, 3.3; 05 4.4.3, 10.2.2, 10.2.3;
/// 06 4.58, 4.60): a dispatch row whose sending fails - the fake service down, or the recipient refused - is tried again 5 minutes, 30 minutes
/// and 2 hours after its first attempt, also when an attempt starts late, and is "undelivered" after the last one; the channel state counts the
/// channel errors, a sending resets it, a recipient error leaves it as it is. The outage writes no other row: no notice to the customer, no
/// change to another dispatch row. The clock is the fake clock; nothing waits for the e-mail.
/// </summary>
[Collection(OutagesDefinition.Name)]
public sealed class EmailOutageTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);

    // The moments of the three retries from the first attempt (P-38); the numbers live in ProductConstants only.
    private static readonly DateTimeOffset FirstRetry = Start + DispatchRetrySchedule.RetriesFromFirstAttempt[0];

    private static readonly DateTimeOffset SecondRetry = Start + DispatchRetrySchedule.RetriesFromFirstAttempt[1];

    private static readonly DateTimeOffset ThirdRetry = Start + DispatchRetrySchedule.RetriesFromFirstAttempt[2];

    private static readonly EmailMessage Message = new("ali@example.com", "Örnek Mağaza", null, "Konu", "<p>metin</p>", "metin");

    private readonly CollectionDatabaseFixture _collection;

    public EmailOutageTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE dispatch_records CASCADE");
        await ExecuteAsync("UPDATE channel_state_records SET consecutive_channel_error_count = 0, first_channel_error_at = NULL, last_succeeded_at = NULL");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK4, KK5, KK7, KK8: a lasting channel outage ----

    [Theory]
    [InlineData(ExternalCallFailureKind.TimedOut)]
    [InlineData(ExternalCallFailureKind.Unreachable)]
    [InlineData(ExternalCallFailureKind.Unrecognized)]
    public async Task ChannelOutage_RetriesFiveThirtyAndHundredTwentyMinutesAfterTheFirstAttempt_ThenUndelivered_CountingTheChannelRun(ExternalCallFailureKind kind)
    {
        ManualTimeProvider time = new(Start);
        FakeEmailGateway gateway = new() { Outage = kind };
        Guid row = await QueueAsync(DispatchType.F5, Start);
        (Guid other, string otherBefore) = await OtherSentRowAsync();

        DispatchAttemptResult first = await AttemptAsync(time, gateway, row);

        Assert.Equal(new DispatchAttemptResult(true, DispatchOutcome.ChannelError), first);
        await AssertRowAsync(row, DispatchState.Queued, 1, FirstRetry);
        await AssertChannelAsync(1, Start, null);

        // The second attempt starts two minutes late: the later moments still count from the first attempt.
        time.SetUtcNow(FirstRetry.AddMinutes(2));
        await AttemptAsync(time, gateway, row);
        await AssertRowAsync(row, DispatchState.Queued, 2, SecondRetry);
        await AssertChannelAsync(2, Start, null);

        time.SetUtcNow(SecondRetry);
        await AttemptAsync(time, gateway, row);
        await AssertRowAsync(row, DispatchState.Queued, 3, ThirdRetry);
        await AssertChannelAsync(3, Start, null);

        time.SetUtcNow(ThirdRetry);
        DispatchAttemptResult last = await AttemptAsync(time, gateway, row);

        Assert.Equal(new DispatchAttemptResult(true, DispatchOutcome.ChannelError), last);
        await AssertRowAsync(row, DispatchState.Undelivered, 4, null);
        await AssertChannelAsync(4, Start, null);

        // An undelivered row takes no further result: no bulk resending, the channel state is not counted again.
        DispatchAttemptResult again = await RecordAsync(time, row, new EmailSendResult.Sent(), time.GetUtcNow());

        Assert.False(again.Applied);
        await AssertRowAsync(row, DispatchState.Undelivered, 4, null);
        await AssertChannelAsync(4, Start, null);
        Assert.Equal(4, gateway.Messages.Count);
        Assert.All(gateway.Messages, m => Assert.Equal(Message, m));

        // KK8: the outage wrote no dispatch row of its own and left the other row as it was.
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
        Assert.Equal(otherBefore, await RowTextAsync(other));
    }

    // ---- KK4, KK5: a recipient error has the same moments and leaves the channel state alone ----

    [Fact]
    public async Task RecipientErrors_HaveTheSameMoments_AndNeitherCountNorResetTheChannelRun()
    {
        DateTimeOffset firstError = Start.AddHours(-1);
        await ExecuteAsync(
            $"UPDATE channel_state_records SET consecutive_channel_error_count = 2, first_channel_error_at = '{firstError:O}', last_succeeded_at = NULL");
        ManualTimeProvider time = new(Start);
        FakeEmailGateway gateway = new() { DefaultSend = new EmailSendResult.RecipientError("recipient-refused") };
        Guid row = await QueueAsync(DispatchType.F5, Start);

        DispatchAttemptResult first = await AttemptAsync(time, gateway, row);

        Assert.Equal(new DispatchAttemptResult(true, DispatchOutcome.RecipientError), first);
        await AssertRowAsync(row, DispatchState.Queued, 1, FirstRetry);
        foreach ((DateTimeOffset at, int attempts, DateTimeOffset? next) in new (DateTimeOffset, int, DateTimeOffset?)[]
        {
            (FirstRetry, 2, SecondRetry),
            (SecondRetry, 3, ThirdRetry),
            (ThirdRetry, 4, null),
        })
        {
            time.SetUtcNow(at);
            await AttemptAsync(time, gateway, row);
            await AssertRowAsync(row, next is null ? DispatchState.Undelivered : DispatchState.Queued, attempts, next);
            await AssertChannelAsync(2, firstError, null);
        }

        Assert.Equal(DispatchOutcome.RecipientError, await LastOutcomeAsync(row));
    }

    // ---- KK4: a sending after the outage resets the run and writes the last success ----

    [Fact]
    public async Task SendingAfterAnOutage_SendsTheRow_ResetsTheChannelRun_AndWritesTheLastSuccess()
    {
        ManualTimeProvider time = new(Start);
        FakeEmailGateway gateway = new() { Outage = ExternalCallFailureKind.Unreachable };
        Guid row = await QueueAsync(DispatchType.F5, Start);

        await AttemptAsync(time, gateway, row);
        await AssertChannelAsync(1, Start, null);

        gateway.Outage = null;
        time.SetUtcNow(FirstRetry);
        DispatchAttemptResult sent = await AttemptAsync(time, gateway, row);

        Assert.Equal(new DispatchAttemptResult(true, DispatchOutcome.Sent), sent);
        await AssertRowAsync(row, DispatchState.Sent, 2, null);
        Assert.Equal(FirstRetry, await SentAtAsync(row));
        await AssertChannelAsync(0, null, FirstRetry);
    }

    private async Task<DispatchAttemptResult> AttemptAsync(ManualTimeProvider time, FakeEmailGateway gateway, Guid row)
    {
        await using AppDbContext db = NewContext();
        DispatchAttempts attempts = new(db, new TransactionRunner(db, new JobEnqueueTracker(), new JobSignal(), new CacheInvalidations(db), new AppCache()), gateway, time);
        return await attempts.SendAsync(row, Message, Ct);
    }

    private async Task<DispatchAttemptResult> RecordAsync(ManualTimeProvider time, Guid row, EmailSendResult result, DateTimeOffset startedAt)
    {
        await using AppDbContext db = NewContext();
        DispatchAttempts attempts = new(db, new TransactionRunner(db, new JobEnqueueTracker(), new JobSignal(), new CacheInvalidations(db), new AppCache()), new FakeEmailGateway(), time);
        return await attempts.RecordAsync(row, result, startedAt, Ct);
    }

    private async Task<Guid> QueueAsync(DispatchType type, DateTimeOffset now)
    {
        await using AppDbContext db = NewContext();
        DispatchRecord row = DispatchRecord.Queue(type, Message.RecipientEmail, null, null, null, now);
        db.DispatchRecords.Add(row);
        await db.SaveChangesAsync(Ct);
        return row.Id;
    }

    // A row of an ordinary transition that was sent before the outage: the outage must leave it as it is.
    private async Task<(Guid Id, string Text)> OtherSentRowAsync()
    {
        await using AppDbContext db = NewContext();
        DispatchRecord row = DispatchRecord.Queue(DispatchType.EmailVerificationLink, "veli@example.com", null, DispatchRelatedRecordType.Customer, Guid.NewGuid(), Start.AddMinutes(-10));
        DispatchRetrySchedule.Apply(row, DispatchOutcome.Sent, Start.AddMinutes(-10), Start.AddMinutes(-10));
        db.DispatchRecords.Add(row);
        await db.SaveChangesAsync(Ct);
        return (row.Id, await RowTextAsync(row.Id));
    }

    private async Task AssertRowAsync(Guid id, DispatchState state, int attempts, DateTimeOffset? nextAttemptAt)
    {
        await using AppDbContext db = NewContext();
        DispatchRecord row = await db.DispatchRecords.AsNoTracking().SingleAsync(d => d.Id == id, Ct);
        Assert.Equal(state, row.DispatchState);
        Assert.Equal(attempts, row.AttemptCount);
        Assert.Equal(nextAttemptAt, row.NextAttemptAt);
    }

    private async Task AssertChannelAsync(int count, DateTimeOffset? firstErrorAt, DateTimeOffset? lastSucceededAt)
    {
        await using AppDbContext db = NewContext();
        ChannelStateRecord state = await db.ChannelStateRecords.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(count, state.ConsecutiveChannelErrorCount);
        Assert.Equal(firstErrorAt, state.FirstChannelErrorAt);
        Assert.Equal(lastSucceededAt, state.LastSucceededAt);
    }

    private async Task<DispatchOutcome?> LastOutcomeAsync(Guid id)
    {
        await using AppDbContext db = NewContext();
        return (await db.DispatchRecords.AsNoTracking().SingleAsync(d => d.Id == id, Ct)).LastOutcome;
    }

    private async Task<DateTimeOffset?> SentAtAsync(Guid id)
    {
        await using AppDbContext db = NewContext();
        return (await db.DispatchRecords.AsNoTracking().SingleAsync(d => d.Id == id, Ct)).SentAt;
    }

    private async Task<string> RowTextAsync(Guid id)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("SELECT row_to_json(d)::text FROM dispatch_records d WHERE d.id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        return (string)(await command.ExecuteScalarAsync(Ct))!;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }
}
