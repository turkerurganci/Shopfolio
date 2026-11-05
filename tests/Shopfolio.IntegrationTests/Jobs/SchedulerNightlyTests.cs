using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;

namespace Shopfolio.IntegrationTests.Jobs;

/// <summary>
/// The nightly job (T221 criteria 3 and 5, KK1, KK4, KK5; 05 5.1.1, 5.1.3, 5.3.2; 09 13.1 items 7, 9): it starts from the heartbeat record's two
/// instants, runs its steps once, writes the attempt first and the success last, is tried again an hour after a failure and not sooner, and a
/// second instance does not run it beside the first. All times are derived from <see cref="SchedulerTimings"/> and <see cref="TurkeyTime"/>.
/// </summary>
[Collection(SchedulerDefinition.Name)]
public sealed class SchedulerNightlyTests : SchedulerTestBase
{
    public SchedulerNightlyTests(CollectionDatabaseFixture collection)
        : base(collection)
    {
    }

    private static DateTimeOffset LatestNightly => NightlyJobSchedule.LatestDueMoment(Now);

    [Fact]
    public async Task RunIfDue_WhenTheLastSuccessIsAfterTheLatestHour_DoesNothing()
    {
        ScriptedStep step = new();
        await using SchedulerHarness harness = NewHarness(steps: [step]);
        await harness.SetHeartbeatAsync(Now, LatestNightly + TimeSpan.FromMinutes(5));

        bool ran = await harness.Nightly.RunIfDueAsync(Ct);

        Assert.False(ran);
        Assert.Equal(0, step.Runs);
    }

    [Fact]
    public async Task RunIfDue_WhenTheNightIsDue_RunsTheStepsOnce_WritesTheAttemptAndTheSuccess_AndNotAgainTheSameNight()
    {
        ScriptedStep first = new("First");
        ScriptedStep second = new("Second");
        await using SchedulerHarness harness = NewHarness(steps: [first, second]);
        await harness.SetHeartbeatAsync(Now, LatestNightly - TimeSpan.FromDays(1));

        bool ran = await harness.Nightly.RunIfDueAsync(Ct);
        bool again = await harness.Nightly.RunIfDueAsync(Ct);

        HeartbeatRecord record = await harness.HeartbeatAsync();
        Assert.True(ran);
        Assert.False(again);
        Assert.Equal(1, first.Runs);
        Assert.Equal(1, second.Runs);
        Assert.Equal(Now, record.NightlyJobLastAttemptedAt);
        Assert.Equal(Now, record.NightlyJobLastSucceededAt);
        Assert.Contains(harness.Logs.Lines, l => l.Contains("JobStarted", StringComparison.Ordinal));
        Assert.Contains(harness.Logs.Lines, l => l.Contains("JobFinished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RunIfDue_AFailedRun_WritesNoSuccess_IsNotTriedBeforeTheRetryWait_AndIsTriedAtIt()
    {
        ScriptedStep step = new() { Script = () => Task.FromException(new InvalidOperationException("a step failed")) };
        await using SchedulerHarness harness = NewHarness(steps: [step]);
        DateTimeOffset yesterdays = LatestNightly - TimeSpan.FromDays(1);
        await harness.SetHeartbeatAsync(Now, yesterdays);

        bool ran = await harness.Nightly.RunIfDueAsync(Ct);

        HeartbeatRecord afterFailure = await harness.HeartbeatAsync();
        Assert.False(ran);
        Assert.Equal(1, step.Runs);
        Assert.Equal(Now, afterFailure.NightlyJobLastAttemptedAt);
        Assert.Equal(yesterdays, afterFailure.NightlyJobLastSucceededAt);
        Assert.Contains(harness.Logs.Lines, l => l.Contains("UnexpectedError", StringComparison.Ordinal));

        harness.Time.Advance(SchedulerTimings.NightlyRetryWait - TimeSpan.FromMinutes(1));
        await harness.Nightly.RunIfDueAsync(Ct);
        Assert.Equal(1, step.Runs);

        // The step is mended; at the retry wait the job runs again and now writes the success.
        step.Script = null;
        harness.Time.Advance(TimeSpan.FromMinutes(1));
        bool retried = await harness.Nightly.RunIfDueAsync(Ct);

        Assert.True(retried);
        Assert.Equal(2, step.Runs);
        Assert.Equal(harness.Time.GetUtcNow(), (await harness.HeartbeatAsync()).NightlyJobLastSucceededAt);
    }

    [Fact]
    public async Task RunIfDue_WhileOneInstanceRuns_TheNightlyLockIsHeld_AndASecondInstanceDoesNotRunTheJob()
    {
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        ScriptedStep step = new()
        {
            Script = async () =>
            {
                started.SetResult();
                await release.Task;
            },
        };
        await using SchedulerHarness harness = NewHarness(steps: [step]);
        await using SchedulerHarness other = NewHarness(steps: [new ScriptedStep()]);
        await harness.SetHeartbeatAsync(Now, LatestNightly - TimeSpan.FromDays(1));

        Task<bool> running = harness.Nightly.RunIfDueAsync(Ct);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(60), Ct);
        SessionAdvisoryLock? stolen = await AdvisoryLocks.TryHoldAsync(other.DataSource, SchedulerTimings.NightlyLockName, Ct);
        bool second = await other.Nightly.RunIfDueAsync(Ct);
        release.SetResult();
        bool first = await running;

        Assert.Null(stolen);
        Assert.False(second);
        Assert.True(first);
        Assert.Equal(1, step.Runs);

        // The lock fell with the run: now an instance can hold it.
        SessionAdvisoryLock? free = await AdvisoryLocks.TryHoldAsync(other.DataSource, SchedulerTimings.NightlyLockName, Ct);
        Assert.NotNull(free);
        await free.DisposeAsync();
    }

    [Fact]
    public async Task Scan_AMissedNight_IsMadeUpAtTheFirstWakeUpAfterIt_OnceOnly_AndTheTimeIsInTheHeartbeatRecordNotACalendar()
    {
        ScriptedStep step = new();
        await using SchedulerHarness harness = NewHarness(steps: [step]);
        await harness.SetHeartbeatAsync(Now, LatestNightly - TimeSpan.FromDays(3));

        await harness.Scan.RunAsync(Ct);
        await harness.Scan.RunAsync(Ct);

        Assert.Equal(1, step.Runs);
        Assert.Equal(Now, (await harness.HeartbeatAsync()).NightlyJobLastSucceededAt);
    }

    [Fact]
    public async Task Scan_JustBeforeTheHour_DoesNotRunTheNight_AndAtTheHourRunsIt()
    {
        ScriptedStep step = new();
        await using SchedulerHarness harness = NewHarness(steps: [step]);
        DateTimeOffset nextNightly = LatestNightly + TimeSpan.FromDays(1);
        await harness.SetHeartbeatAsync(Now, LatestNightly + TimeSpan.FromMinutes(5));

        harness.Time.SetUtcNow(nextNightly - TimeSpan.FromMinutes(1));
        await harness.Scan.RunAsync(Ct);
        Assert.Equal(0, step.Runs);

        harness.Time.SetUtcNow(nextNightly);
        await harness.Scan.RunAsync(Ct);
        Assert.Equal(1, step.Runs);
    }

    [Fact]
    public async Task JobQueueCleanupStep_DeletesTheCompletedRows_AndKeepsTheFailedAndThePendingOnes()
    {
        Job completed = Row(JobType.ImageDerivative, JobState.Completed);
        Job failed = Row(JobType.CardRefund, JobState.Failed);
        Job pending = Row(JobType.ImageDerivative, JobState.Pending);
        await using (AppDbContext db = NewContext())
        {
            db.Jobs.AddRange(completed, failed, pending);
            await db.SaveChangesAsync(Ct);
        }

        await using SchedulerHarness harness = NewHarness();
        await harness.SetHeartbeatAsync(Now, LatestNightly - TimeSpan.FromDays(1));

        bool ran = await harness.Nightly.RunIfDueAsync(Ct);

        await using AppDbContext read = NewContext();
        Assert.True(ran);
        Assert.Equal(
            new[] { failed.Id, pending.Id }.Order(),
            (await read.Jobs.Select(j => j.Id).ToListAsync(Ct)).Order());
    }

    private static Job Row(JobType type, JobState state)
    {
        Job row = Job.Enqueue(type, Guid.NewGuid(), null, Now);
        row.JobState = state;
        if (state == JobState.Completed)
        {
            row.CompletedAt = Now;
        }

        if (state == JobState.Failed)
        {
            row.AttemptCount = JobTimings.DefaultRetrySchedule.Count + 1;
            row.LastErrorSummary = nameof(InvalidOperationException);
        }

        return row;
    }
}
