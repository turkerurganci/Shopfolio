using System.Net;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.IntegrationTests.Jobs;

/// <summary>
/// The heartbeat and the return from an outage (T221 criteria 2 and 4, KK2, KK7; 05 5.4.1, 5.4.3; 09 13.1 items 1, 8): a beat is written only
/// when the liveness request is answered with 200; a gap of two minutes or less is no outage and a longer one is recorded together with the
/// first beat and the put-off of the transfer orders, in one transaction; a slow scan does not delay a beat.
/// </summary>
[Collection(SchedulerDefinition.Name)]
public sealed class SchedulerHeartbeatTests : SchedulerTestBase
{
    public SchedulerHeartbeatTests(CollectionDatabaseFixture collection)
        : base(collection)
    {
    }

    [Fact]
    public async Task Beat_AnsweredWith200_WritesTheClockToTheHeartbeatRecord()
    {
        await using SchedulerHarness harness = NewHarness();
        harness.Liveness.Status = HttpStatusCode.OK;
        harness.Time.Advance(SchedulerTimings.HeartbeatInterval);

        bool beat = await harness.Pulse.BeatAsync(Ct);

        Assert.True(beat);
        Assert.Equal(harness.Time.GetUtcNow(), (await harness.HeartbeatAsync()).LastHeartbeatAt);
        Assert.Empty(await harness.OutagesAsync());
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task Beat_NotAnsweredWith200_WritesNothing(HttpStatusCode status)
    {
        await using SchedulerHarness harness = NewHarness();
        await harness.SetHeartbeatAsync(Now - SchedulerTimings.HeartbeatInterval, null);
        harness.Liveness.Status = status;

        bool beat = await harness.Pulse.BeatAsync(Ct);

        Assert.False(beat);
        Assert.Equal(1, harness.Liveness.Requests);
        Assert.Equal(Now - SchedulerTimings.HeartbeatInterval, (await harness.HeartbeatAsync()).LastHeartbeatAt);
    }

    [Fact]
    public async Task Beat_ARefusedConnection_WritesNothing()
    {
        await using SchedulerHarness harness = NewHarness();
        await harness.SetHeartbeatAsync(Now - SchedulerTimings.HeartbeatInterval, null);
        harness.Liveness.Refuse = true;

        bool beat = await harness.Pulse.BeatAsync(Ct);

        Assert.False(beat);
        Assert.Equal(1, harness.Liveness.Requests);
        Assert.Equal(Now - SchedulerTimings.HeartbeatInterval, (await harness.HeartbeatAsync()).LastHeartbeatAt);
        Assert.Empty(await harness.OutagesAsync());
    }

    [Fact]
    public async Task Beat_NoAnswerWithinTheUserWaitingLimit_WritesNothing()
    {
        await using SchedulerHarness harness = NewHarness();
        await harness.SetHeartbeatAsync(Now - SchedulerTimings.HeartbeatInterval, null);
        harness.Liveness.Hang = true;

        // The limit runs on the fake clock: the beat waits, one tick short of the limit it is still waiting, at the limit it ends by itself.
        Task<bool> beatTask = harness.Pulse.BeatAsync(Ct);
        await harness.WaitForPulseLimitAsync();
        harness.Time.Advance(ExternalCallTimeouts.UserWaiting - TimeSpan.FromTicks(1));
        Assert.False(beatTask.IsCompleted);

        harness.Time.Advance(TimeSpan.FromTicks(1));
        bool beat = await beatTask;

        Assert.False(beat);
        Assert.Equal(1, harness.Liveness.Requests);
        Assert.Equal(Now - SchedulerTimings.HeartbeatInterval, (await harness.HeartbeatAsync()).LastHeartbeatAt);
        Assert.Empty(await harness.OutagesAsync());
    }

    [Fact]
    public async Task Service_AFailedBeat_DoesNotEndTheLoop_TheNextBeatWrites()
    {
        await using SchedulerHarness harness = NewHarness();
        await harness.SetHeartbeatAsync(Now, Now);
        harness.Liveness.Refuse = true;
        SchedulerService service = harness.NewService();

        await service.StartAsync(Ct);
        try
        {
            await JobQueueHarness.EventuallyAsync(() => Task.FromResult(harness.Liveness.Requests >= 1), "the first beat is sent and refused");
            await harness.WaitForTimersAsync(1);
            Assert.Equal(Now, (await harness.HeartbeatAsync()).LastHeartbeatAt);

            harness.Liveness.Refuse = false;
            harness.Liveness.Status = HttpStatusCode.OK;
            harness.Time.Advance(SchedulerTimings.HeartbeatInterval);

            await JobQueueHarness.EventuallyAsync(
                async () => (await harness.HeartbeatAsync()).LastHeartbeatAt == harness.Time.GetUtcNow(),
                "the loop went on and the next beat wrote the heartbeat");
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Beat_AGapOfTheThreshold_IsNoOutage_AndALongerGapIsRecordedWithTheFirstBeat()
    {
        await using SchedulerHarness harness = NewHarness();
        harness.Liveness.Status = HttpStatusCode.OK;
        await harness.SetHeartbeatAsync(Now - SchedulerTimings.OutageThreshold, null);

        await harness.Pulse.BeatAsync(Ct);

        Assert.Empty(await harness.OutagesAsync());

        // The gap from this beat to the next is one tick longer than the threshold.
        harness.Time.Advance(SchedulerTimings.OutageThreshold + TimeSpan.FromSeconds(1));
        await harness.Pulse.BeatAsync(Ct);

        OutageRecord outage = Assert.Single(await harness.OutagesAsync());
        Assert.Equal(Now, outage.StartedAt);
        Assert.Equal(harness.Time.GetUtcNow(), outage.EndedAt);
        Assert.Equal(harness.Time.GetUtcNow(), (await harness.HeartbeatAsync()).LastHeartbeatAt);
    }

    [Fact]
    public async Task Beat_ReturnFromAnOutage_PutsOffTheTransferOrderWhoseWindowRanOut_AndDropsItsReminder()
    {
        DateTimeOffset gapStart = Now - TimeSpan.FromHours(6);
        Order ranOut = await SeedTransferOrderAsync(Now - TimeSpan.FromHours(2), reminderAt: gapStart);
        Order endedBefore = await SeedTransferOrderAsync(gapStart - TimeSpan.FromMinutes(1));
        Order endsLater = await SeedTransferOrderAsync(Now + TimeSpan.FromMinutes(1));
        await using SchedulerHarness harness = NewHarness();
        harness.Liveness.Status = HttpStatusCode.OK;
        await harness.SetHeartbeatAsync(gapStart, null);

        await harness.Pulse.BeatAsync(Ct);

        Order moved = await harness.OrderAsync(ranOut.Id);
        Assert.Equal(TransferSchedule.DeferredWindowEnd(Now, BusinessCalendar.Product), moved.PaymentWindowEndsAt);
        Assert.Null(moved.PaymentReminderAt);
        Assert.Equal(ranOut.Version + 1, moved.Version);
        Order beforeGap = await harness.OrderAsync(endedBefore.Id);
        Assert.Equal(endedBefore.PaymentWindowEndsAt, beforeGap.PaymentWindowEndsAt);
        Assert.Equal(endedBefore.Version, beforeGap.Version);
        Order later = await harness.OrderAsync(endsLater.Id);
        Assert.Equal(endsLater.PaymentWindowEndsAt, later.PaymentWindowEndsAt);
        Assert.Equal(endsLater.Version, later.Version);
        Assert.Single(await harness.OutagesAsync());
    }

    [Fact]
    public async Task Beat_AnOrderThatIsPaidOrCancelled_IsNotPutOff()
    {
        DateTimeOffset gapStart = Now - TimeSpan.FromHours(6);
        Order paid = await SeedTransferOrderAsync(Now - TimeSpan.FromHours(2));
        await ExecuteAsync($"UPDATE orders SET payment_status = 'Paid' WHERE id = '{paid.Id}'");
        await using SchedulerHarness harness = NewHarness();
        harness.Liveness.Status = HttpStatusCode.OK;
        await harness.SetHeartbeatAsync(gapStart, null);

        await harness.Pulse.BeatAsync(Ct);

        Assert.Equal(paid.PaymentWindowEndsAt, (await harness.OrderAsync(paid.Id)).PaymentWindowEndsAt);
    }

    [Fact]
    public async Task Beat_WhenThePutOffFails_NeitherTheBeatNorTheOutageRecordIsWritten()
    {
        DateTimeOffset gapStart = Now - TimeSpan.FromHours(6);
        Order order = await SeedTransferOrderAsync(Now - TimeSpan.FromHours(2));
        await using SchedulerHarness harness = NewHarness();
        harness.Liveness.Status = HttpStatusCode.OK;
        await harness.SetHeartbeatAsync(gapStart, null);
        await ExecuteAsync(
            """
            CREATE OR REPLACE FUNCTION t221_block_orders() RETURNS trigger AS $body$
            BEGIN
                RAISE EXCEPTION 'the order write is refused';
            END;
            $body$ LANGUAGE plpgsql;
            CREATE TRIGGER t221_block_orders BEFORE UPDATE ON orders FOR EACH ROW EXECUTE FUNCTION t221_block_orders();
            """);
        try
        {
            await Assert.ThrowsAnyAsync<Exception>(() => harness.Pulse.BeatAsync(Ct));
        }
        finally
        {
            await ExecuteAsync("DROP TRIGGER t221_block_orders ON orders; DROP FUNCTION t221_block_orders();");
        }

        // The first heartbeat of the return, the outage record and the put-off are one commit or none: none was written.
        Assert.Equal(gapStart, (await harness.HeartbeatAsync()).LastHeartbeatAt);
        Assert.Empty(await harness.OutagesAsync());
        Assert.Equal(order.PaymentWindowEndsAt, (await harness.OrderAsync(order.Id)).PaymentWindowEndsAt);
    }

    [Fact]
    public async Task Scan_AfterAnOutageThatCoveredTheNightlyHour_RunsTheNightlyJobOnce()
    {
        ScriptedStep step = new();
        ScriptedSource cancellation = new("Cancellation", waitsForFreshHeartbeat: true);
        Guid due = Guid.NewGuid();
        cancellation.Add(due, Now - TimeSpan.FromHours(1));
        await using SchedulerHarness harness = NewHarness([cancellation], [step]);
        harness.Liveness.Status = HttpStatusCode.OK;
        DateTimeOffset nightly = NightlyJobSchedule.LatestDueMoment(Now);
        await harness.SetHeartbeatAsync(nightly - TimeSpan.FromHours(1), nightly - TimeSpan.FromDays(1));

        // Before the first beat of the return the heartbeat is old: the cancellation waits, the nightly job does not depend on it.
        await harness.Scan.RunAsync(Ct);
        Assert.Equal(0, cancellation.FindCalls);
        Assert.Equal(1, step.Runs);

        await harness.Pulse.BeatAsync(Ct);
        await harness.Scan.RunAsync(Ct);
        await harness.Scan.RunAsync(Ct);

        Assert.Equal([due], cancellation.Processed);
        Assert.Equal(1, step.Runs);
        Assert.Single(await harness.OutagesAsync());
    }

    [Fact]
    public async Task Service_ASlowScan_DoesNotDelayTheHeartbeat_AndTheHeartbeatDoesNotWaitForTheScan()
    {
        ScriptedSource slow = new("Slow") { BlockFind = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously) };
        await using SchedulerHarness harness = NewHarness([slow]);
        harness.Liveness.Status = HttpStatusCode.OK;
        await harness.SetHeartbeatAsync(Now, Now);
        SchedulerService service = harness.NewService();

        await service.StartAsync(Ct);
        try
        {
            await JobQueueHarness.EventuallyAsync(() => Task.FromResult(slow.FindCalls == 1 && harness.Liveness.Requests >= 1), "the scan is stuck and the first beat is sent");
            await harness.WaitForTimersAsync(1);

            harness.Time.Advance(SchedulerTimings.HeartbeatInterval);
            await JobQueueHarness.EventuallyAsync(() => Task.FromResult(harness.Liveness.Requests >= 2), "the second beat is sent while the scan is stuck");

            await JobQueueHarness.EventuallyAsync(
                async () => (await harness.HeartbeatAsync()).LastHeartbeatAt == harness.Time.GetUtcNow(),
                "the second beat wrote the heartbeat while the scan is stuck");
            Assert.Equal(1, slow.FindCalls);
        }
        finally
        {
            slow.BlockFind.SetResult();
            await service.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Service_ScansOncePerScanInterval()
    {
        ScriptedSource source = new("Records");
        await using SchedulerHarness harness = NewHarness([source]);
        await harness.SetHeartbeatAsync(Now, Now);
        SchedulerService service = harness.NewService();

        await service.StartAsync(Ct);
        try
        {
            await JobQueueHarness.EventuallyAsync(() => Task.FromResult(source.FindCalls == 1), "the first scan");
            await harness.WaitForTimersAsync(2);

            harness.Time.Advance(SchedulerTimings.ScanInterval - TimeSpan.FromSeconds(1));
            await harness.WaitForTimersAsync(2);
            Assert.Equal(1, source.FindCalls);

            harness.Time.Advance(TimeSpan.FromSeconds(1));
            await JobQueueHarness.EventuallyAsync(() => Task.FromResult(source.FindCalls == 2), "the second scan, one interval after the first");
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Service_TwoInstances_ScanOnce_BecauseTheSchedulerLockIsHeldByOne()
    {
        ScriptedSource source = new("Records");
        await using SchedulerHarness harness = NewHarness([source]);
        await harness.SetHeartbeatAsync(Now, Now);
        SchedulerService one = harness.NewService();
        SchedulerService two = harness.NewService();

        await one.StartAsync(Ct);
        await two.StartAsync(Ct);
        try
        {
            await JobQueueHarness.EventuallyAsync(() => Task.FromResult(source.FindCalls >= 1), "the first scan");
            await harness.WaitForTimersAsync(4);
            await Task.Delay(200, Ct);
            Assert.Equal(1, source.FindCalls);

            harness.Time.Advance(SchedulerTimings.ScanInterval);
            await JobQueueHarness.EventuallyAsync(() => Task.FromResult(source.FindCalls >= 2), "the second scan");
            await harness.WaitForTimersAsync(4);
            await Task.Delay(200, Ct);
            Assert.Equal(2, source.FindCalls);
        }
        finally
        {
            await one.StopAsync(CancellationToken.None);
            await two.StopAsync(CancellationToken.None);
        }
    }
}
