using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.IntegrationTests.Jobs;

/// <summary>
/// One wake-up of the scheduler (T221 criteria 1 and 5, KK1, KK3, KK5, KK7, KK8; 05 5.1.1, 5.4.3; 09 13.1 items 6, 7): the due records of all
/// sources, in order of their moments, each in a scope and a transaction of its own; the time comes from the record's field; a source that
/// waits for a fresh heartbeat is held back while the last heartbeat is old; one record's failure does not stop the others.
/// </summary>
[Collection(SchedulerDefinition.Name)]
public sealed class SchedulerScanTests : SchedulerTestBase
{
    public SchedulerScanTests(CollectionDatabaseFixture collection)
        : base(collection)
    {
    }

    [Fact]
    public async Task Scan_HandsEveryDueRecordOnce_AndNoRecordWhoseMomentHasNotCome()
    {
        ScriptedSource source = new("Records");
        Guid first = Guid.NewGuid();
        Guid second = Guid.NewGuid();
        Guid later = Guid.NewGuid();
        source.Add(first, Now - TimeSpan.FromMinutes(5));
        source.Add(second, Now);
        source.Add(later, Now + TimeSpan.FromSeconds(1));
        await using SchedulerHarness harness = NewHarness([source]);
        await harness.SetHeartbeatAsync(Now, Now);

        await harness.Scan.RunAsync(Ct);
        await harness.Scan.RunAsync(Ct);

        Assert.Equal([first, second], source.Processed);
        Assert.Equal(2, source.FindCalls);
    }

    [Fact]
    public async Task Scan_PutsTheRecordsOfAllSourcesInOrderOfTheirMoments_AndAFailedRecordDoesNotStopTheOthers()
    {
        List<Guid> order = [];
        Guid a1 = Guid.NewGuid();
        Guid a2 = Guid.NewGuid();
        Guid b1 = Guid.NewGuid();
        Guid b2 = Guid.NewGuid();
        Guid b3 = Guid.NewGuid();
        ScriptedSource sourceA = new("A");
        ScriptedSource sourceB = new("B");
        sourceA.OnProcess = id =>
        {
            lock (order)
            {
                order.Add(id);
            }

            return Task.CompletedTask;
        };
        sourceA.Add(a1, Now - TimeSpan.FromHours(3));
        sourceA.Add(a2, Now - TimeSpan.FromHours(1));
        sourceB.Add(b1, Now - TimeSpan.FromHours(2));
        sourceB.Add(b2, Now - TimeSpan.FromMinutes(30));
        sourceB.Add(b3, Now - TimeSpan.FromMinutes(10));
        sourceB.OnProcess = id =>
        {
            lock (order)
            {
                order.Add(id);
            }

            return id == b2 ? Task.FromException(new InvalidOperationException("the record failed")) : Task.CompletedTask;
        };
        await using SchedulerHarness harness = NewHarness([sourceA, sourceB]);
        await harness.SetHeartbeatAsync(Now, Now);

        await harness.Scan.RunAsync(Ct);

        // The moments decide, not the source: a1 (3 h), b1 (2 h), a2 (1 h), b2 (30 min, fails), b3 (10 min).
        Assert.Equal([a1, b1, a2, b2, b3], order);
        Assert.Equal([b1, b3], sourceB.Processed);
        Assert.Contains(harness.Logs.Lines, l => l.Contains("UnexpectedError", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Scan_ASourceThatWaitsForAFreshHeartbeat_IsHeldBackWhileTheLastBeatIsOld_AndRunsWhenItIsFresh()
    {
        ScriptedSource waiting = new("Cancellation", waitsForFreshHeartbeat: true);
        ScriptedSource other = new("Reminders");
        Guid cancel = Guid.NewGuid();
        Guid remind = Guid.NewGuid();
        waiting.Add(cancel, Now - TimeSpan.FromHours(1));
        other.Add(remind, Now - TimeSpan.FromHours(1));
        await using SchedulerHarness harness = NewHarness([waiting, other]);
        await harness.SetHeartbeatAsync(Now - SchedulerTimings.OutageThreshold - TimeSpan.FromSeconds(1), Now);

        await harness.Scan.RunAsync(Ct);

        Assert.Equal(0, waiting.FindCalls);
        Assert.Empty(waiting.Processed);
        Assert.Equal([remind], other.Processed);

        await harness.SetHeartbeatAsync(Now - SchedulerTimings.OutageThreshold, Now);
        await harness.Scan.RunAsync(Ct);

        Assert.Equal([cancel], waiting.Processed);
    }

    [Fact]
    public async Task Scan_WithNoHeartbeatAtAll_HoldsBackTheSourceThatWaitsForOne()
    {
        ScriptedSource waiting = new("Cancellation", waitsForFreshHeartbeat: true);
        waiting.Add(Guid.NewGuid(), Now - TimeSpan.FromHours(1));
        await using SchedulerHarness harness = NewHarness([waiting]);
        await harness.SetHeartbeatAsync(null, Now);

        await harness.Scan.RunAsync(Ct);

        Assert.Equal(0, waiting.FindCalls);
    }

    [Fact]
    public async Task Scan_TheHeartbeatGoingOldBetweenTheLookAndTheRecord_StopsThatSourcesRecords()
    {
        ScriptedSource waiting = new("Cancellation", waitsForFreshHeartbeat: true);
        Guid first = Guid.NewGuid();
        Guid second = Guid.NewGuid();
        waiting.Add(first, Now - TimeSpan.FromHours(2));
        waiting.Add(second, Now - TimeSpan.FromHours(1));
        await using SchedulerHarness harness = NewHarness([waiting]);
        await harness.SetHeartbeatAsync(Now, Now);

        // The first record takes a long while: the clock moves past the threshold while it is processed, and the second must not run.
        waiting.OnProcess = _ =>
        {
            harness.Time.Advance(SchedulerTimings.OutageThreshold + TimeSpan.FromSeconds(1));
            return Task.CompletedTask;
        };

        await harness.Scan.RunAsync(Ct);

        Assert.Equal([first], waiting.Processed);
    }

    [Fact]
    public async Task OrderSource_TakesTheOrderRowLock_BeforeItChangesTheOrder_AndRaisesTheVersion()
    {
        Order order = await SeedTransferOrderAsync(Now - TimeSpan.FromHours(1));
        TestOrderSource source = new();
        await using SchedulerHarness harness = NewHarness([source]);
        await harness.SetHeartbeatAsync(Now, Now);

        await harness.Scan.RunAsync(Ct);

        Order after = await harness.OrderAsync(order.Id);
        Assert.Equal(1, source.Applied);
        Assert.True(source.SecondLockRefused, "a second connection took the order's row lock while the first held it");
        Assert.Equal(Now, after.PaymentReminderAt);
        Assert.Equal(order.Version + 1, after.Version);
    }

    [Fact]
    public async Task OrderSource_TheTimeIsTheOrdersOwnField_AnOrderNotYetDueIsLeftAlone_AndADoneOrderIsNotDoneTwice()
    {
        Order due = await SeedTransferOrderAsync(Now - TimeSpan.FromMinutes(1));
        Order notYet = await SeedTransferOrderAsync(Now + TimeSpan.FromMinutes(1));
        TestOrderSource source = new();
        await using SchedulerHarness harness = NewHarness([source]);
        await harness.SetHeartbeatAsync(Now, Now);

        await harness.Scan.RunAsync(Ct);
        await harness.Scan.RunAsync(Ct);

        Assert.Equal(1, source.Applied);
        Assert.NotNull((await harness.OrderAsync(due.Id)).PaymentReminderAt);
        Assert.Null((await harness.OrderAsync(notYet.Id)).PaymentReminderAt);

        // The clock moves; the other order's own field says its moment has come.
        harness.Time.Advance(TimeSpan.FromMinutes(2));
        await harness.Scan.RunAsync(Ct);

        Assert.Equal(2, source.Applied);
        Assert.NotNull((await harness.OrderAsync(notYet.Id)).PaymentReminderAt);
    }

    [Fact]
    public async Task OrderSource_AnOrderThatStoppedBeingDueBeforeItsLock_IsNotChanged()
    {
        Order order = await SeedTransferOrderAsync(Now - TimeSpan.FromHours(1));
        TestOrderSource source = new();
        await using SchedulerHarness harness = NewHarness([source]);
        await using AsyncServiceScope scope = harness.Services.CreateAsyncScope();
        IReadOnlyList<DueRecord> found = await source.FindDueAsync(scope.ServiceProvider, Now, SchedulerTimings.DueBatchLimit, Ct);

        // Between the look and the processing another actor handles the order (the field is filled).
        await ExecuteAsync($"UPDATE orders SET payment_reminder_at = now() WHERE id = '{order.Id}'");
        await source.ProcessAsync(scope.ServiceProvider, Assert.Single(found).Id, Ct);

        Assert.Equal(0, source.Applied);
        Assert.Equal(order.Version, (await harness.OrderAsync(order.Id)).Version);
    }
}
