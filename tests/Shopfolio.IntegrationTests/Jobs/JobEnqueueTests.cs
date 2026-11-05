using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.IntegrationTests.Jobs;

/// <summary>
/// Writing to the queue on a real PostgreSQL (T220 criteria 4 and 8, KK4 and KK8; 09 13.1 items 2 and 11; 05 4.3.1, 4.4.1; MK-37, MK-38): a row is
/// added in the caller's transaction and commits or rolls back with the transition's own changes; the same pending (type, record) is one row;
/// the runner is woken after the commit that wrote a row and by no other end; an e-mail writes no row.
/// </summary>
[Collection(JobQueueDefinition.Name)]
public sealed class JobEnqueueTests : JobQueueTestBase
{
    public JobEnqueueTests(CollectionDatabaseFixture collection)
        : base(collection)
    {
    }

    [Fact]
    public async Task Enqueue_InTheCallersTransaction_AddsAPendingRow_AndWakesTheRunnerOnceAfterTheCommit()
    {
        await using JobQueueHarness harness = NewHarness([]);
        Guid record = Guid.NewGuid();

        await harness.EnqueueAsync(JobType.FinalPaymentQuery, record);

        Job row = Assert.Single(await harness.RowsAsync());
        Assert.Equal(JobType.FinalPaymentQuery, row.JobType);
        Assert.Equal(record, row.RelatedRecordId);
        Assert.Equal(JobState.Pending, row.JobState);
        Assert.Equal(0, row.AttemptCount);
        Assert.Null(row.NextAttemptAt);
        Assert.Null(row.LastErrorSummary);
        Assert.Null(row.CompletedAt);
        Assert.Equal(harness.Time.GetUtcNow(), row.CreatedAt);
        Assert.Equal(1, harness.Signal.Notifications);
    }

    [Fact]
    public async Task Enqueue_WithAFirstAttemptMoment_WritesItOnTheRow()
    {
        await using JobQueueHarness harness = NewHarness([]);
        DateTimeOffset notBefore = JobQueueHarness.Start.AddMinutes(3);

        await harness.EnqueueAsync(JobType.ImageDerivative, Guid.NewGuid(), notBefore);

        Assert.Equal(notBefore, Assert.Single(await harness.RowsAsync()).NextAttemptAt);
    }

    [Fact]
    public async Task Enqueue_TheSamePendingTypeAndRecordTwice_IsOneRow_AndTheFirstRowIsLeftAsItIs()
    {
        await using JobQueueHarness harness = NewHarness([]);
        Guid record = Guid.NewGuid();
        DateTimeOffset firstMoment = JobQueueHarness.Start.AddMinutes(1);

        await harness.EnqueueAsync(JobType.FinalPaymentQuery, record, firstMoment);
        harness.Time.Advance(TimeSpan.FromMinutes(10));
        await harness.EnqueueAsync(JobType.FinalPaymentQuery, record, JobQueueHarness.Start.AddHours(5));

        Job row = Assert.Single(await harness.RowsAsync());
        Assert.Equal(firstMoment, row.NextAttemptAt);
        Assert.Equal(JobQueueHarness.Start, row.CreatedAt);
    }

    [Fact]
    public async Task Enqueue_TheBounceQueryTwice_IsOneRow_BecauseTheEmptyRecordCountsAsEqual()
    {
        await using JobQueueHarness harness = NewHarness([]);

        await harness.EnqueueAsync(JobType.BounceQuery, null);
        await harness.EnqueueAsync(JobType.BounceQuery, null);

        Assert.Equal(JobType.BounceQuery, Assert.Single(await harness.RowsAsync()).JobType);
    }

    [Fact]
    public async Task Enqueue_AnotherRecordOrAnotherType_IsAnotherRow()
    {
        await using JobQueueHarness harness = NewHarness([]);
        Guid record = Guid.NewGuid();

        await harness.EnqueueAsync(JobType.FinalPaymentQuery, record);
        await harness.EnqueueAsync(JobType.FinalPaymentQuery, Guid.NewGuid());
        await harness.EnqueueAsync(JobType.RefundOutcomeQuery, record);

        Assert.Equal(3, (await harness.RowsAsync()).Count);
    }

    [Fact]
    public async Task Enqueue_WhenTheEarlierRowIsNoLongerPending_AddsANewRow()
    {
        await using JobQueueHarness harness = NewHarness([]);
        Guid record = Guid.NewGuid();
        await harness.EnqueueAsync(JobType.FinalPaymentQuery, record);
        await ExecuteAsync("UPDATE jobs SET job_state = 'Completed', completed_at = now()");

        await harness.EnqueueAsync(JobType.FinalPaymentQuery, record);

        IReadOnlyList<Job> rows = await harness.RowsAsync();
        Assert.Equal(2, rows.Count);
        Assert.Single(rows, j => j.JobState == JobState.Pending);
    }

    [Fact]
    public async Task Enqueue_OutsideATransaction_IsRefused()
    {
        await using JobQueueHarness harness = NewHarness([]);
        await using AsyncServiceScope scope = harness.Services.CreateAsyncScope();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => scope.ServiceProvider.GetRequiredService<IJobQueue>().EnqueueAsync(JobType.CardRefund, Guid.NewGuid(), null, Ct));

        Assert.Empty(await harness.RowsAsync());
    }

    [Fact]
    public async Task Enqueue_WithARecordThatTheTypeDoesNotTake_OrAnUnknownType_IsRefused()
    {
        await using JobQueueHarness harness = NewHarness([]);

        await Assert.ThrowsAsync<ArgumentException>(() => harness.EnqueueAsync(JobType.BounceQuery, Guid.NewGuid()));
        await Assert.ThrowsAsync<ArgumentException>(() => harness.EnqueueAsync(JobType.CardRefund, null));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => harness.EnqueueAsync((JobType)999, Guid.NewGuid()));

        Assert.Empty(await harness.RowsAsync());
        Assert.Equal(0, harness.Signal.Notifications);
    }

    // ---- KK4: a row and the transition's own changes commit and roll back together ----

    [Fact]
    public async Task Enqueue_InATransactionThatCommits_WritesTheRowAndTheInnerEffectTogether()
    {
        await using JobQueueHarness harness = NewHarness([]);
        Guid effectId = Guid.NewGuid();

        await RunTransitionAsync(harness, effectId, refuse: false, fail: false);

        Assert.Single(await harness.RowsAsync());
        Assert.Equal(1, await EffectCountAsync());
        Assert.Equal(1, harness.Signal.Notifications);
    }

    [Fact]
    public async Task Enqueue_InATransactionThatIsRefused_LeavesNoRowNoInnerEffectAndNoWakeUp()
    {
        await using JobQueueHarness harness = NewHarness([]);

        await RunTransitionAsync(harness, Guid.NewGuid(), refuse: true, fail: false);

        Assert.Empty(await harness.RowsAsync());
        Assert.Equal(0, await EffectCountAsync());
        Assert.Equal(0, harness.Signal.Notifications);
    }

    [Fact]
    public async Task Enqueue_InATransactionThatThrows_LeavesNoRowNoInnerEffectAndNoWakeUp()
    {
        await using JobQueueHarness harness = NewHarness([]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => RunTransitionAsync(harness, Guid.NewGuid(), refuse: false, fail: true));

        Assert.Empty(await harness.RowsAsync());
        Assert.Equal(0, await EffectCountAsync());
        Assert.Equal(0, harness.Signal.Notifications);
    }

    [Fact]
    public async Task Commit_OfATransactionThatWroteNoRow_WakesNobody()
    {
        await using JobQueueHarness harness = NewHarness([]);
        await using AsyncServiceScope scope = harness.Services.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async ct =>
            {
                await WriteEffectAsync(db, Guid.NewGuid(), ct);
                return new StepResult();
            },
            Ct);

        Assert.Equal(1, await EffectCountAsync());
        Assert.Equal(0, harness.Signal.Notifications);
    }

    // ---- KK8: an e-mail has no row of its own ----

    [Fact]
    public async Task QueueingAnEmail_WritesTheDispatchRecordAndNoJobRow()
    {
        await using JobQueueHarness harness = NewHarness([]);
        Customer customer = Customer.Create("Ali", "ali@example.com", "digest-placeholder", JobQueueHarness.Start, null, JobQueueHarness.Start);
        DispatchRecord notice = DispatchRecord.Queue(DispatchType.EmailChangeNotice, "old@example.com", null, DispatchRelatedRecordType.Customer, customer.Id, JobQueueHarness.Start);
        await using (AppDbContext db = NewContext())
        {
            db.Customers.Add(customer);
            await db.SaveChangesAsync(Ct);
            db.DispatchRecords.Add(notice);
            await db.SaveChangesAsync(Ct);
        }

        Assert.Equal(1, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE dispatch_state = 'Queued'"));
        Assert.Empty(await harness.RowsAsync());
        Assert.Equal(0, harness.Signal.Notifications);
    }

    // ---- Helpers ----

    // A transition stand-in: an inner side effect and a queue row in one transaction, then the end the test asks for.
    private static async Task RunTransitionAsync(JobQueueHarness harness, Guid effectId, bool refuse, bool fail)
    {
        await using AsyncServiceScope scope = harness.Services.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IJobQueue queue = scope.ServiceProvider.GetRequiredService<IJobQueue>();
        await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async ct =>
            {
                await WriteEffectAsync(db, effectId, ct);
                await queue.EnqueueAsync(JobType.CardRefund, Guid.NewGuid(), null, ct);
                return fail ? throw new InvalidOperationException("The transition failed after it wrote.") : new StepResult(refuse);
            },
            Ct);
    }
}
