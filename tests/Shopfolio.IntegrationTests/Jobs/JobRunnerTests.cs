using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;

namespace Shopfolio.IntegrationTests.Jobs;

/// <summary>
/// The runner on a real PostgreSQL (T220 criteria 1-6 and 9, KK1-KK3, KK5, KK9; 09 13.1 items 3, 4, 5, 7 and 11; 05 4.3): one round takes the
/// oldest due row of a known type and handles it; a row's failure is written on the row by the type's retry rule and the next row goes on;
/// a cancelled round leaves the row pending for the next runner; a handler is called with no transaction open. Time is the fake clock.
/// </summary>
[Collection(JobQueueDefinition.Name)]
public sealed class JobRunnerTests : JobQueueTestBase
{
    public JobRunnerTests(CollectionDatabaseFixture collection)
        : base(collection)
    {
    }

    // ---- One round ----

    [Fact]
    public async Task RunOne_WithNoRow_ReturnsFalse()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);

        Assert.False(await harness.RunOneAsync(Ct));
    }

    [Fact]
    public async Task RunOne_TakesTheOldestDueRowFirst_AndOneRowPerRound()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        Guid firstRecord = Guid.NewGuid();
        Guid secondRecord = Guid.NewGuid();
        await harness.EnqueueAsync(JobType.ImageDerivative, firstRecord);
        harness.Time.Advance(TimeSpan.FromSeconds(1));
        await harness.EnqueueAsync(JobType.ImageDerivative, secondRecord);

        Assert.True(await harness.RunOneAsync(Ct));

        Assert.Equal([firstRecord], harness.Script.Calls.Select(c => c.RelatedRecordId));
        IReadOnlyList<Job> rows = await harness.RowsAsync();
        Assert.Equal(JobState.Completed, rows.Single(r => r.RelatedRecordId == firstRecord).JobState);
        Assert.Equal(JobState.Pending, rows.Single(r => r.RelatedRecordId == secondRecord).JobState);
        Assert.True(await harness.RunOneAsync(Ct));
        Assert.Equal([firstRecord, secondRecord], harness.Script.Calls.Select(c => c.RelatedRecordId));
        Assert.False(await harness.RunOneAsync(Ct));
    }

    [Fact]
    public async Task RunOne_ACompletedRow_CarriesTheCompletionMoment_AndNoCountOrError()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());
        harness.Time.Advance(TimeSpan.FromMinutes(2));

        Assert.True(await harness.RunOneAsync(Ct));

        Job row = Assert.Single(await harness.RowsAsync());
        Assert.Equal(JobState.Completed, row.JobState);
        Assert.Equal(harness.Time.GetUtcNow(), row.CompletedAt);
        Assert.Equal(0, row.AttemptCount);
        Assert.Null(row.LastErrorSummary);
    }

    [Fact]
    public async Task RunOne_ARowWhoseMomentHasNotComeYet_IsNotTaken_UntilTheClockReachesIt()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        TimeSpan wait = TimeSpan.FromMinutes(10);
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid(), JobQueueHarness.Start + wait);

        Assert.False(await harness.RunOneAsync(Ct));
        harness.Time.Advance(wait - TimeSpan.FromMilliseconds(1));
        Assert.False(await harness.RunOneAsync(Ct));
        harness.Time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.True(await harness.RunOneAsync(Ct));
    }

    [Fact]
    public async Task RunOne_ARescheduledRow_StaysPendingForTheGivenTime_WithTheCountAndErrorUntouched()
    {
        await using JobQueueHarness harness = NewHarness([JobType.FinalPaymentQuery]);
        harness.Script.ByType[JobType.FinalPaymentQuery] = _ => Task.FromResult(JobOutcome.RescheduleAfter(JobTimings.FinalPaymentQueryInterval));
        await harness.EnqueueAsync(JobType.FinalPaymentQuery, Guid.NewGuid());

        Assert.True(await harness.RunOneAsync(Ct));

        Job row = Assert.Single(await harness.RowsAsync());
        Assert.Equal(JobState.Pending, row.JobState);
        Assert.Equal(harness.Time.GetUtcNow() + JobTimings.FinalPaymentQueryInterval, row.NextAttemptAt);
        Assert.Equal(0, row.AttemptCount);
        Assert.Null(row.LastErrorSummary);
        Assert.False(await harness.RunOneAsync(Ct));
        harness.Time.Advance(JobTimings.FinalPaymentQueryInterval);
        Assert.True(await harness.RunOneAsync(Ct));
        Assert.Equal(2, harness.Script.Calls.Count);
    }

    [Fact]
    public async Task RunOne_ARowOfATypeWithoutAHandler_IsNeitherReadNorTouched_AndTheOthersGoOn()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        await harness.EnqueueAsync(JobType.FinalPaymentQuery, Guid.NewGuid());
        harness.Time.Advance(TimeSpan.FromSeconds(1));
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());

        Assert.True(await harness.RunOneAsync(Ct));
        Assert.False(await harness.RunOneAsync(Ct));

        IReadOnlyList<Job> rows = await harness.RowsAsync();
        Job unknown = rows.Single(r => r.JobType == JobType.FinalPaymentQuery);
        Assert.Equal(JobState.Pending, unknown.JobState);
        Assert.Equal(0, unknown.AttemptCount);
        Assert.Null(unknown.NextAttemptAt);
        Assert.Null(unknown.LastErrorSummary);
        Assert.Equal(JobState.Completed, rows.Single(r => r.JobType == JobType.ImageDerivative).JobState);
    }

    // ---- Criterion 4: the inner effect and the completion are one commit ----

    [Fact]
    public async Task RunOne_ATransactionalHandler_WritesItsEffectAndTheCompletionInOneCommit()
    {
        await using JobQueueHarness harness = NewHarness([], JobType.InboundNotificationProcessing);
        harness.Script.Transactional = async call =>
        {
            AppDbContext db = call.Services.GetRequiredService<AppDbContext>();
            Assert.NotNull(db.Database.CurrentTransaction);
            await WriteEffectAsync(db, call.Job.Id, call.Ct);
        };
        await harness.EnqueueAsync(JobType.InboundNotificationProcessing, Guid.NewGuid());

        Assert.True(await harness.RunOneAsync(Ct));

        Assert.Equal(JobState.Completed, Assert.Single(await harness.RowsAsync()).JobState);
        Assert.Equal(1, await EffectCountAsync());
    }

    [Fact]
    public async Task RunOne_ATransactionalHandlerThatFailsAfterItWrote_RollsTheEffectBack_AndTheRetryRuleApplies()
    {
        await using JobQueueHarness harness = NewHarness([], JobType.InboundNotificationProcessing);
        harness.Script.Transactional = async call =>
        {
            await WriteEffectAsync(call.Services.GetRequiredService<AppDbContext>(), call.Job.Id, call.Ct);
            throw new InvalidOperationException("The handler failed after it wrote.");
        };
        await harness.EnqueueAsync(JobType.InboundNotificationProcessing, Guid.NewGuid());

        Assert.True(await harness.RunOneAsync(Ct));

        Assert.Equal(0, await EffectCountAsync());
        Job row = Assert.Single(await harness.RowsAsync());
        Assert.Equal(JobState.Pending, row.JobState);
        Assert.Equal(1, row.AttemptCount);
        Assert.Equal(harness.Time.GetUtcNow() + JobTimings.DefaultRetrySchedule[0], row.NextAttemptAt);
    }

    // ---- KK3: at least once, and the handler does not change the result on the second sight ----

    [Fact]
    public async Task RunOne_ARowSeenTwice_BecauseTheOutcomeWasNeverWritten_GivesTheSameResult()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        using var firstRound = CancellationTokenSource.CreateLinkedTokenSource(Ct);

        // The handler is idempotent: its effect is one row per job. The first round is cancelled after the effect was written and before the
        // outcome was; the process died there. The second round sees the same row again.
        harness.Script.ByType[JobType.ImageDerivative] = async call =>
        {
            AppDbContext db = call.Services.GetRequiredService<AppDbContext>();
            await call.Services.GetRequiredService<ITransactionRunner>().RunAsync(
                async ct =>
                {
                    await WriteEffectAsync(db, call.Job.Id, ct);
                    return new StepResult();
                },
                Ct);
            if (!firstRound.IsCancellationRequested)
            {
                await firstRound.CancelAsync();
            }

            return JobOutcome.Completed;
        };
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => harness.NewRunner().RunOneAsync(firstRound.Token));
        Job afterCrash = Assert.Single(await harness.RowsAsync());
        Assert.Equal(JobState.Pending, afterCrash.JobState);
        Assert.Equal(0, afterCrash.AttemptCount);
        Assert.Equal(1, await EffectCountAsync());

        Assert.True(await harness.RunOneAsync(Ct));

        Assert.Equal(2, harness.Script.Calls.Count);
        Assert.Single(harness.Script.Calls.Select(c => c.Id).Distinct());
        Assert.Equal(1, await EffectCountAsync());
        Assert.Equal(JobState.Completed, Assert.Single(await harness.RowsAsync()).JobState);
    }

    // ---- KK2: a row is not taken twice, and a killed process's row is taken again ----

    [Fact]
    public async Task Claim_ARowAnotherTransactionHolds_IsSkipped_NotWaitedFor()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());
        harness.Time.Advance(TimeSpan.FromSeconds(1));
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());
        string[] known = [nameof(JobType.ImageDerivative)];
        DateTimeOffset now = harness.Time.GetUtcNow();

        await using AppDbContext first = NewContext();
        await using AppDbContext second = NewContext();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction firstTransaction = await first.Database.BeginTransactionAsync(Ct);
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction secondTransaction = await second.Database.BeginTransactionAsync(Ct);

        Job? takenByFirst = await JobClaim.TakeNextAsync(first, known, now, Ct);
        Job? takenBySecond = await JobClaim.TakeNextAsync(second, known, now, Ct);
        Job? takenByThird = await JobClaim.TakeNextAsync(second, known, now, Ct);

        Assert.NotNull(takenByFirst);
        Assert.NotNull(takenBySecond);
        Assert.NotEqual(takenByFirst.Id, takenBySecond.Id);
        Assert.Equal(takenBySecond.Id, takenByThird?.Id);
        await using AppDbContext third = NewContext();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction thirdTransaction = await third.Database.BeginTransactionAsync(Ct);
        Assert.Null(await JobClaim.TakeNextAsync(third, known, now, Ct));
    }

    [Fact]
    public async Task RunOne_CancelledWhileTheHandlerWorks_LeavesTheRowPendingWithTheCountUnchanged_AndANewRunnerFinishesIt()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        harness.Script.ByType[JobType.ImageDerivative] = async call =>
        {
            started.TrySetResult();
            await new TaskCompletionSource().Task.WaitAsync(call.Ct);
            return JobOutcome.Completed;
        };
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());
        using var dying = CancellationTokenSource.CreateLinkedTokenSource(Ct);

        Task<bool> round = harness.NewRunner().RunOneAsync(dying.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(60), Ct);
        await dying.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => round);

        Job pending = Assert.Single(await harness.RowsAsync());
        Assert.Equal(JobState.Pending, pending.JobState);
        Assert.Equal(0, pending.AttemptCount);
        Assert.Null(pending.NextAttemptAt);
        Assert.Null(pending.LastErrorSummary);
        harness.Script.ByType[JobType.ImageDerivative] = _ => Task.FromResult(JobOutcome.Completed);
        Assert.True(await harness.RunOneAsync(Ct));
        Assert.Equal(JobState.Completed, Assert.Single(await harness.RowsAsync()).JobState);
        Assert.Equal(2, harness.Script.Calls.Count);
    }

    [Fact]
    public async Task SingleRunLock_TheSecondHolderGetsNull_UntilTheFirstIsDisposed_AndAnotherWorkNameIsSeparate()
    {
        await using JobQueueHarness harness = NewHarness([]);

        SessionAdvisoryLock? first = await AdvisoryLocks.TryHoldAsync(harness.DataSource, JobTimings.RunnerLockName, Ct);
        Assert.NotNull(first);
        Assert.Null(await AdvisoryLocks.TryHoldAsync(harness.DataSource, JobTimings.RunnerLockName, Ct));
        await using (SessionAdvisoryLock? other = await AdvisoryLocks.TryHoldAsync(harness.DataSource, "Scheduler", Ct))
        {
            Assert.NotNull(other);
        }

        await first.DisposeAsync();

        await using SessionAdvisoryLock? again = await AdvisoryLocks.TryHoldAsync(harness.DataSource, JobTimings.RunnerLockName, Ct);
        Assert.NotNull(again);
    }

    [Fact]
    public async Task SingleRunLock_WhenItsConnectionDies_IsGone_TheHolderSeesIt_AndAnotherCanTakeIt()
    {
        await using JobQueueHarness harness = NewHarness([]);
        SessionAdvisoryLock? first = await AdvisoryLocks.TryHoldAsync(harness.DataSource, JobTimings.RunnerLockName, Ct);
        Assert.NotNull(first);
        await first.EnsureAliveAsync(Ct);

        await ExecuteAsync("SELECT pg_terminate_backend(pid) FROM pg_locks WHERE locktype = 'advisory' AND granted");

        await Assert.ThrowsAnyAsync<Exception>(() => first.EnsureAliveAsync(Ct));
        await first.DisposeAsync();
        await using SessionAdvisoryLock? second = await AdvisoryLocks.TryHoldAsync(harness.DataSource, JobTimings.RunnerLockName, Ct);
        Assert.NotNull(second);
    }

    [Fact]
    public async Task TwoRunnerServices_HandleEveryRowExactlyOnce()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        JobRunnerService one = harness.NewRunner(new JobSignal());
        JobRunnerService two = harness.NewRunner(new JobSignal());
        List<Guid> records = [.. Enumerable.Range(0, 4).Select(_ => Guid.NewGuid())];
        foreach (Guid record in records)
        {
            await harness.EnqueueAsync(JobType.ImageDerivative, record);
        }

        await one.StartAsync(Ct);
        await two.StartAsync(Ct);
        try
        {
            await JobQueueHarness.EventuallyAsync(
                async () =>
                {
                    harness.Time.Advance(JobTimings.PollInterval);
                    return (await harness.RowsAsync()).All(r => r.JobState == JobState.Completed);
                },
                "every row is completed");
        }
        finally
        {
            await one.StopAsync(CancellationToken.None);
            await two.StopAsync(CancellationToken.None);
        }

        Assert.Equal(records.Order(), harness.Script.Calls.Select(c => c.RelatedRecordId!.Value).Order());
    }

    [Fact]
    public async Task RunnerService_WhileAnotherHoldsTheSingleRunLock_TakesNoRow_AndTakesItOnceTheLockIsFree()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        SessionAdvisoryLock? other = await AdvisoryLocks.TryHoldAsync(harness.DataSource, JobTimings.RunnerLockName, Ct);
        Assert.NotNull(other);
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());
        JobRunnerService runner = harness.NewRunner();
        await runner.StartAsync(Ct);
        try
        {
            for (int round = 0; round < 3; round++)
            {
                await harness.WaitForTimerAsync();
                harness.Time.Advance(JobTimings.PollInterval);
            }

            await harness.WaitForTimerAsync();
            Assert.Empty(harness.Script.Calls);
            Assert.Equal(JobState.Pending, Assert.Single(await harness.RowsAsync()).JobState);

            await other.DisposeAsync();
            await JobQueueHarness.EventuallyAsync(
                async () =>
                {
                    harness.Time.Advance(JobTimings.PollInterval);
                    return (await harness.RowsAsync()).All(r => r.JobState == JobState.Completed);
                },
                "the row is handled once the lock is free");
        }
        finally
        {
            await runner.StopAsync(CancellationToken.None);
        }

        Assert.Single(harness.Script.Calls);
    }

    // ---- KK1: the runner wakes at once after a write, and looks every five seconds ----

    [Fact]
    public async Task RunnerService_AfterACommitThatWroteARow_WakesAndHandlesItWithoutTheClockMoving()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        TaskCompletionSource handled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        harness.Script.ByType[JobType.ImageDerivative] = _ =>
        {
            handled.TrySetResult();
            return Task.FromResult(JobOutcome.Completed);
        };
        JobRunnerService runner = harness.NewRunner();
        await runner.StartAsync(Ct);
        try
        {
            await harness.WaitForTimerAsync();
            DateTimeOffset before = harness.Time.GetUtcNow();

            await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());
            await handled.Task.WaitAsync(TimeSpan.FromSeconds(60), Ct);

            Assert.Equal(before, harness.Time.GetUtcNow());
        }
        finally
        {
            await runner.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task RunnerService_ARowWrittenWithoutAWakeUp_IsFoundWhenTheFiveSecondsPass()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        TaskCompletionSource handled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        harness.Script.ByType[JobType.ImageDerivative] = _ =>
        {
            handled.TrySetResult();
            return Task.FromResult(JobOutcome.Completed);
        };
        JobRunnerService runner = harness.NewRunner();
        await runner.StartAsync(Ct);
        try
        {
            await harness.WaitForTimerAsync();
            Job row = Job.Enqueue(JobType.ImageDerivative, Guid.NewGuid(), null, harness.Time.GetUtcNow());
            await using (AppDbContext db = NewContext())
            {
                db.Jobs.Add(row);
                await db.SaveChangesAsync(Ct);
            }

            Assert.Equal(0, harness.Signal.Notifications);
            Assert.False(handled.Task.IsCompleted);
            harness.Time.Advance(JobTimings.PollInterval);
            await handled.Task.WaitAsync(TimeSpan.FromSeconds(60), Ct);
        }
        finally
        {
            await runner.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task RunnerService_AtStart_WritesTheUnknownTypeWarningOnce_AndLeavesTheRowsAlone()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        await harness.EnqueueAsync(JobType.FinalPaymentQuery, Guid.NewGuid());
        await harness.EnqueueAsync(JobType.BounceQuery, null);
        JobRunnerService runner = harness.NewRunner();
        await runner.StartAsync(Ct);
        try
        {
            await JobQueueHarness.EventuallyAsync(() => Task.FromResult(harness.Logs.Lines.Any(l => l.Contains("JobTypeUnknown", StringComparison.Ordinal))), "the warning");
            for (int round = 0; round < 3; round++)
            {
                await harness.WaitForTimerAsync();
                harness.Time.Advance(JobTimings.PollInterval);
            }

            await harness.WaitForTimerAsync();
        }
        finally
        {
            await runner.StopAsync(CancellationToken.None);
        }

        string warning = Assert.Single(harness.Logs.Lines, l => l.Contains("JobTypeUnknown", StringComparison.Ordinal));
        Assert.Contains("2 pending rows", warning, StringComparison.Ordinal);
        Assert.All(await harness.RowsAsync(), r => Assert.Equal((JobState.Pending, 0), (r.JobState, r.AttemptCount)));
    }

    // ---- D-96 K1: the queue takes no row before its start gates are open ----

    [Fact]
    public async Task RunnerService_WhileAStartGateIsClosed_TakesNoRow_AndTakesItAfterTheGateOpens()
    {
        SwitchGate gate = new();
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative], gates: [gate]);
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());
        JobRunnerService runner = harness.NewRunner();
        await runner.StartAsync(Ct);
        try
        {
            for (int round = 0; round < 3; round++)
            {
                await harness.WaitForTimerAsync();
                harness.Time.Advance(JobTimings.PollInterval);
            }

            await harness.WaitForTimerAsync();
            Assert.Empty(harness.Script.Calls);
            Assert.Equal(JobState.Pending, Assert.Single(await harness.RowsAsync()).JobState);

            gate.IsOpen = true;
            await JobQueueHarness.EventuallyAsync(
                async () =>
                {
                    harness.Time.Advance(JobTimings.PollInterval);
                    return (await harness.RowsAsync()).All(r => r.JobState == JobState.Completed);
                },
                "the row is handled once the gate is open");
        }
        finally
        {
            await runner.StopAsync(CancellationToken.None);
        }
    }

    // ---- KK5: the default retry rule on the real runner ----

    [Fact]
    public async Task RunOne_AHandlerThatAlwaysFails_IsRetriedAtTheDocumentedMoments_AndTheSixthFailureFailsTheRow()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        harness.Script.ByType[JobType.ImageDerivative] = _ => throw new InvalidOperationException("ali@example.com could not be processed");
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());

        for (int failure = 0; failure < JobTimings.DefaultRetrySchedule.Count; failure++)
        {
            Assert.True(await harness.RunOneAsync(Ct));

            Job row = Assert.Single(await harness.RowsAsync());
            Assert.Equal(JobState.Pending, row.JobState);
            Assert.Equal(failure + 1, row.AttemptCount);
            Assert.Equal(harness.Time.GetUtcNow() + JobTimings.DefaultRetrySchedule[failure], row.NextAttemptAt);
            Assert.Equal(nameof(InvalidOperationException), row.LastErrorSummary);

            // Not before its moment, and then at it.
            harness.Time.Advance(JobTimings.DefaultRetrySchedule[failure] - TimeSpan.FromMilliseconds(1));
            Assert.False(await harness.RunOneAsync(Ct));
            harness.Time.Advance(TimeSpan.FromMilliseconds(1));
        }

        Assert.True(await harness.RunOneAsync(Ct));

        Job failed = Assert.Single(await harness.RowsAsync());
        Assert.Equal(JobState.Failed, failed.JobState);
        Assert.Equal(JobTimings.DefaultRetrySchedule.Count + 1, failed.AttemptCount);
        Assert.Null(failed.NextAttemptAt);
        Assert.Null(failed.CompletedAt);
        Assert.Equal(nameof(InvalidOperationException), failed.LastErrorSummary);
        Assert.Equal(JobTimings.DefaultRetrySchedule.Count + 1, harness.Script.Calls.Count);

        // A failed row is not taken again.
        harness.Time.Advance(TimeSpan.FromDays(30));
        Assert.False(await harness.RunOneAsync(Ct));
    }

    [Fact]
    public async Task RunOne_AFailingRowDoesNotHoldTheRowsBehindIt()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative, JobType.FinalPaymentQuery]);
        harness.Script.ByType[JobType.ImageDerivative] = _ => throw new InvalidOperationException();
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());
        harness.Time.Advance(TimeSpan.FromSeconds(1));
        await harness.EnqueueAsync(JobType.FinalPaymentQuery, Guid.NewGuid());

        Assert.True(await harness.RunOneAsync(Ct));
        Assert.True(await harness.RunOneAsync(Ct));
        Assert.False(await harness.RunOneAsync(Ct));

        IReadOnlyList<Job> rows = await harness.RowsAsync();
        Assert.Equal(JobState.Pending, rows.Single(r => r.JobType == JobType.ImageDerivative).JobState);
        Assert.Equal(JobState.Completed, rows.Single(r => r.JobType == JobType.FinalPaymentQuery).JobState);
    }

    [Fact]
    public async Task RunOne_ADatabaseErrorOfTheHandler_IsSummarizedAsTheTypeNameAndTheSqlState_NeverTheMessage()
    {
        await using JobQueueHarness harness = NewHarness([JobType.ImageDerivative]);
        harness.Script.ByType[JobType.ImageDerivative] = async call =>
        {
            await call.Services.GetRequiredService<AppDbContext>().Database.ExecuteSqlRawAsync("SELECT 1 / 0", call.Ct);
            return JobOutcome.Completed;
        };
        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid());

        Assert.True(await harness.RunOneAsync(Ct));

        string? summary = Assert.Single(await harness.RowsAsync()).LastErrorSummary;
        Assert.Equal($"{nameof(PostgresException)} {PostgresErrorCodes.DivisionByZero}", summary);
        Assert.DoesNotContain("division", summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunOne_TheBounceQueryThatFails_RepeatsEveryQueryInterval_AndNeverFailsTheRow()
    {
        await using JobQueueHarness harness = NewHarness([JobType.BounceQuery]);
        harness.Script.ByType[JobType.BounceQuery] = _ => throw new InvalidOperationException();
        await harness.EnqueueAsync(JobType.BounceQuery, null);

        for (int round = 0; round < JobTimings.DefaultRetrySchedule.Count + 3; round++)
        {
            Assert.True(await harness.RunOneAsync(Ct));

            Job row = Assert.Single(await harness.RowsAsync());
            Assert.Equal(JobState.Pending, row.JobState);
            Assert.Equal(0, row.AttemptCount);
            Assert.Equal(harness.Time.GetUtcNow() + JobTimings.BounceQueryInterval, row.NextAttemptAt);
            harness.Time.Advance(JobTimings.BounceQueryInterval);
        }
    }

    // ---- KK9: no outside call under a database lock ----

    [Fact]
    public async Task RunOne_CallsAnOutsideHandlerWithNoTransactionOpen_NoRowLockHeld_AndNoOrderLockPossible()
    {
        await using JobQueueHarness harness = NewHarness([JobType.CardRefund]);
        Observation? seen = null;
        harness.Script.ByType[JobType.CardRefund] = async call =>
        {
            AppDbContext db = call.Services.GetRequiredService<AppDbContext>();
            bool orderLockRefused = false;
            try
            {
                LockSequence.Enter(db, LockKind.Order, Guid.NewGuid());
            }
            catch (InvalidOperationException)
            {
                orderLockRefused = true;
            }

            seen = new Observation(
                db.Database.CurrentTransaction is null,
                orderLockRefused,
                await ScalarAsync<long>("SELECT count(*) FROM pg_stat_activity WHERE datname = current_database() AND state = 'idle in transaction'"),
                await RowIsFreeAsync(call.Job.Id));
            return JobOutcome.Completed;
        };
        await harness.EnqueueAsync(JobType.CardRefund, Guid.NewGuid());

        Assert.True(await harness.RunOneAsync(Ct));

        Assert.NotNull(seen);
        Assert.True(seen.NoTransaction);
        Assert.True(seen.OrderLockRefused);
        Assert.Equal(0, seen.IdleInTransaction);
        Assert.True(seen.RowIsFree);
    }

    private sealed record Observation(bool NoTransaction, bool OrderLockRefused, long IdleInTransaction, bool RowIsFree);

    // Another connection asks for the row's lock without waiting: a lock the runner held would refuse it.
    private async Task<bool> RowIsFreeAsync(Guid id)
    {
        await using NpgsqlConnection connection = new(Collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT id FROM jobs WHERE id = '{id}' FOR UPDATE NOWAIT", connection, transaction);
        try
        {
            return await command.ExecuteScalarAsync(Ct) is not null;
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.LockNotAvailable)
        {
            return false;
        }
    }
}
