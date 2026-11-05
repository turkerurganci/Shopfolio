using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Jobs;

/// <summary>
/// The clean-up of the queue's rows (T220 criterion 6, KK6; 05 4.3.4, 5.2.9; 06 9.4.4) and the card refund's rule on the real runner (criterion 7,
/// KK7; 05 4.3.3, 4.3.4; 09 13.1 item 5): the clean-up deletes the completed rows and only them; a refund request that went is never sent again,
/// the result is asked for, also after a process that died right after the request. The refund handler of the PayTR task (T405) is not here yet;
/// a test-local stand-in follows its rule (09 13.1 item 4) and counts what it would send and ask.
/// </summary>
[Collection(JobQueueDefinition.Name)]
public sealed class JobCleanupAndRefundTests : JobQueueTestBase
{
    public JobCleanupAndRefundTests(CollectionDatabaseFixture collection)
        : base(collection)
    {
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await ExecuteAsync("TRUNCATE refund_records, refunds, payment_records, orders CASCADE");
    }

    public override async ValueTask DisposeAsync()
    {
        await ExecuteAsync("TRUNCATE refund_records, refunds, payment_records, orders CASCADE");
        await base.DisposeAsync();
    }

    // A real refund row (a late payment refund of a real order and its payment record), so the refund records have the rows they point at.
    private async Task<Guid> SeedRefundAsync()
    {
        DateTimeOffset now = JobQueueHarness.Start;
        await using AppDbContext db = NewContext();
        Province? province = await db.Provinces.SingleOrDefaultAsync(p => p.Code == "06", Ct);
        if (province is null)
        {
            province = Province.Create("06", "Ankara", now);
            db.Provinces.Add(province);
        }

        Guid VersionOf(LegalTextType type, bool hosting)
        {
            LegalTextVersion? existing = db.LegalTextVersions.SingleOrDefault(v => v.LegalTextType == type);
            if (existing is not null)
            {
                return existing.Id;
            }

            LegalTextVersion created = LegalTextVersion.CreatePublished(type, 1, now, "metin", now);
            db.LegalTextVersions.Add(created);
            return (hosting ? created.WithHosting() : created).Id;
        }

        Guid form = VersionOf(LegalTextType.PreInformationForm, false);
        Guid contract = VersionOf(LegalTextType.DistanceSalesContract, false);
        Guid notice = VersionOf(LegalTextType.PrivacyNotice, true);
        await db.SaveChangesAsync(Ct);

        Order order = Order.Create("2026-000001", "access-placeholder-1", "confirm-placeholder-1", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, now.AddHours(1), now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = province.Id;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = province.Id;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form;
        order.DistanceSalesContractVersionId = contract;
        order.PrivacyNoticeVersionId = notice;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        db.Orders.Add(order);
        PaymentRecord payment = PaymentRecord.ForCardPayment(order.Id, "tx-t220", null, now);
        db.PaymentRecords.Add(payment);
        Refund refund = Refund.ForLatePayment(order.Id, payment.Id, 120m, now);
        db.Refunds.Add(refund);
        await db.SaveChangesAsync(Ct);
        return refund.Id;
    }

    // ---- KK6: the clean-up deletes the completed rows and nothing else ----

    [Fact]
    public async Task Cleanup_DeletesTheCompletedRows_AndKeepsTheFailedAndThePendingOnes()
    {
        DateTimeOffset now = JobQueueHarness.Start;
        Job completed = Row(JobType.ImageDerivative, JobState.Completed, now);
        Job closed = Row(JobType.CardRefund, JobState.Completed, now);
        Job failedOne = Row(JobType.CardRefund, JobState.Failed, now);
        Job failedTwo = Row(JobType.FinalPaymentQuery, JobState.Failed, now);
        Job pending = Row(JobType.ImageDerivative, JobState.Pending, now);
        Job later = Row(JobType.RefundOutcomeQuery, JobState.Pending, now);
        later.NextAttemptAt = now.AddHours(4);
        await using (AppDbContext db = NewContext())
        {
            db.Jobs.AddRange(completed, closed, failedOne, failedTwo, pending, later);
            await db.SaveChangesAsync(Ct);
        }

        int deleted;
        await using (AppDbContext db = NewContext())
        {
            deleted = await JobCleanup.DeleteCompletedAsync(db, Ct);
        }

        Assert.Equal(2, deleted);
        await using AppDbContext read = NewContext();
        Assert.Equal(
            new[] { failedOne.Id, failedTwo.Id, pending.Id, later.Id }.Order(),
            read.Jobs.Select(j => j.Id).AsEnumerable().Order());
        Assert.Equal(2, read.Jobs.Count(j => j.JobState == JobState.Failed));
    }

    [Fact]
    public async Task Cleanup_WithOnlyFailedRows_DeletesNothing()
    {
        await using (AppDbContext db = NewContext())
        {
            db.Jobs.AddRange(Row(JobType.CardRefund, JobState.Failed, JobQueueHarness.Start), Row(JobType.ImageDerivative, JobState.Failed, JobQueueHarness.Start));
            await db.SaveChangesAsync(Ct);
        }

        await using AppDbContext cleanup = NewContext();
        Assert.Equal(0, await JobCleanup.DeleteCompletedAsync(cleanup, Ct));
        Assert.Equal(2, await ScalarAsync<long>("SELECT count(*) FROM jobs WHERE job_state = 'Failed'"));
    }

    // ---- KK7: the card refund rule over real refund records ----

    [Fact]
    public async Task ReadLatestState_IsTheLatestRecordOfTheRefundWrittenAtOrAfterTheRow_AndNullWhenThereIsNone()
    {
        Guid refund = await SeedRefundAsync();
        DateTimeOffset jobCreated = JobQueueHarness.Start;
        await using AppDbContext db = NewContext();
        Assert.Null(await CardRefundJobRule.ReadLatestStateAsync(db, refund, jobCreated, Ct));

        // A record of an earlier row of the same refund is not this row's (06 4.34).
        await AddRecordAsync(refund, RefundRecordState.Failed, jobCreated.AddMinutes(-30));
        Assert.Null(await CardRefundJobRule.ReadLatestStateAsync(db, refund, jobCreated, Ct));

        await AddRecordAsync(refund, RefundRecordState.Failed, jobCreated);
        Assert.Equal(RefundRecordState.Failed, await CardRefundJobRule.ReadLatestStateAsync(db, refund, jobCreated, Ct));

        await AddRecordAsync(refund, RefundRecordState.Sending, jobCreated.AddMinutes(5));
        Assert.Equal(RefundRecordState.Sending, await CardRefundJobRule.ReadLatestStateAsync(db, refund, jobCreated, Ct));

        // Another refund's records are not read.
        Assert.Null(await CardRefundJobRule.ReadLatestStateAsync(db, Guid.NewGuid(), jobCreated, Ct));
    }

    [Fact]
    public async Task CardRefund_NoRecordYet_SendsOnce_AndTheRecordIsWrittenBeforeTheRequestGoes()
    {
        await using JobQueueHarness harness = NewHarness([JobType.CardRefund]);
        RefundStandIn standIn = new(harness);
        Guid refund = await SeedRefundAsync();
        await harness.EnqueueAsync(JobType.CardRefund, refund);

        Assert.True(await harness.RunOneAsync(Ct));

        Assert.Equal(1, standIn.Sent);
        Assert.Equal(0, standIn.Asked);
        Assert.Equal([RefundRecordState.Sending], standIn.StatesAtSend);
        Assert.Equal(JobState.Completed, Assert.Single(await harness.RowsAsync()).JobState);
    }

    [Fact]
    public async Task CardRefund_ASentRequest_IsNeverSentAgain_AndTheProcessThatDiedRightAfterItAsksTheResult()
    {
        await using JobQueueHarness harness = NewHarness([JobType.CardRefund]);
        RefundStandIn standIn = new(harness) { DieAfterSend = true };
        Guid refund = await SeedRefundAsync();
        await harness.EnqueueAsync(JobType.CardRefund, refund);

        // The first round writes the record, sends, and the process fails before it could write the result: the default rule retries the row.
        Assert.True(await harness.RunOneAsync(Ct));
        Job afterCrash = Assert.Single(await harness.RowsAsync());
        Assert.Equal(JobState.Pending, afterCrash.JobState);
        Assert.Equal(1, afterCrash.AttemptCount);
        Assert.Equal(harness.Time.GetUtcNow() + JobTimings.DefaultRetrySchedule[0], afterCrash.NextAttemptAt);
        Assert.Equal(1, standIn.Sent);

        harness.Time.Advance(JobTimings.DefaultRetrySchedule[0]);
        Assert.True(await harness.RunOneAsync(Ct));

        Assert.Equal(1, standIn.Sent);
        Assert.Equal(1, standIn.Asked);
        Assert.Equal(JobState.Completed, Assert.Single(await harness.RowsAsync()).JobState);
        await using AppDbContext db = NewContext();
        Assert.Equal(RefundRecordState.Succeeded, await CardRefundJobRule.ReadLatestStateAsync(db, refund, JobQueueHarness.Start, Ct));
    }

    [Fact]
    public async Task CardRefund_AResultAlreadyWritten_SendsNothingAndAsksNothing()
    {
        await using JobQueueHarness harness = NewHarness([JobType.CardRefund]);
        RefundStandIn standIn = new(harness);
        Guid refund = await SeedRefundAsync();
        await harness.EnqueueAsync(JobType.CardRefund, refund);
        await AddRecordAsync(refund, RefundRecordState.Succeeded, JobQueueHarness.Start);

        Assert.True(await harness.RunOneAsync(Ct));

        Assert.Equal(0, standIn.Sent);
        Assert.Equal(0, standIn.Asked);
        Assert.Equal(JobState.Completed, Assert.Single(await harness.RowsAsync()).JobState);
    }

    [Fact]
    public async Task CardRefund_AFailureBeforeTheRecordWasWritten_SentNothing_AndTheDefaultRuleSendsOnTheRetry()
    {
        await using JobQueueHarness harness = NewHarness([JobType.CardRefund]);
        RefundStandIn standIn = new(harness) { FailBeforeRecord = true };
        Guid refund = await SeedRefundAsync();
        await harness.EnqueueAsync(JobType.CardRefund, refund);

        Assert.True(await harness.RunOneAsync(Ct));
        Job afterFailure = Assert.Single(await harness.RowsAsync());
        Assert.Equal(JobState.Pending, afterFailure.JobState);
        Assert.Equal(1, afterFailure.AttemptCount);
        Assert.Equal(harness.Time.GetUtcNow() + JobTimings.DefaultRetrySchedule[0], afterFailure.NextAttemptAt);
        Assert.Equal(0, standIn.Sent);

        standIn.FailBeforeRecord = false;
        harness.Time.Advance(JobTimings.DefaultRetrySchedule[0]);
        Assert.True(await harness.RunOneAsync(Ct));

        Assert.Equal(1, standIn.Sent);
        Assert.Equal(0, standIn.Asked);
        Assert.Equal(JobState.Completed, Assert.Single(await harness.RowsAsync()).JobState);
    }

    // ---- Helpers ----

    private static Job Row(JobType type, JobState state, DateTimeOffset now)
    {
        Job row = Job.Enqueue(type, type == JobType.BounceQuery ? null : Guid.NewGuid(), null, now);
        row.JobState = state;
        if (state == JobState.Completed)
        {
            row.CompletedAt = now;
        }

        if (state == JobState.Failed)
        {
            row.AttemptCount = JobTimings.DefaultRetrySchedule.Count + 1;
            row.LastErrorSummary = nameof(InvalidOperationException);
        }

        return row;
    }

    private async Task AddRecordAsync(Guid refundId, RefundRecordState state, DateTimeOffset createdAt)
    {
        RefundRecord record = RefundRecord.Sending(refundId, createdAt);
        record.State = state;
        if (state is RefundRecordState.Succeeded or RefundRecordState.Failed)
        {
            record.OutcomeAt = createdAt;
        }

        await using AppDbContext db = NewContext();
        db.RefundRecords.Add(record);
        await db.SaveChangesAsync(Ct);
    }

    // A test-local stand-in of the PayTR task's refund handler (T405): it decides with the rule, writes the record before it "sends", and
    // counts what it sent and asked. Registered as the CardRefund script of the harness.
    private sealed class RefundStandIn
    {
        private readonly List<RefundRecordState> _statesAtSend = [];
        private int _sent;
        private int _asked;

        public RefundStandIn(JobQueueHarness harness)
        {
            harness.Script.ByType[JobType.CardRefund] = HandleAsync;
        }

        public int Sent => Volatile.Read(ref _sent);

        public int Asked => Volatile.Read(ref _asked);

        public IReadOnlyList<RefundRecordState> StatesAtSend => _statesAtSend;

        public bool DieAfterSend { get; init; }

        public bool FailBeforeRecord { get; set; }

        private async Task<JobOutcome> HandleAsync(HandlerCall call)
        {
            AppDbContext db = call.Services.GetRequiredService<AppDbContext>();
            ITransactionRunner runner = call.Services.GetRequiredService<ITransactionRunner>();
            TimeProvider time = call.Services.GetRequiredService<TimeProvider>();
            Guid refundId = call.Job.RelatedRecordId!.Value;
            RefundRecordState? latest = await CardRefundJobRule.ReadLatestStateAsync(db, refundId, call.Job.CreatedAt, call.Ct);
            switch (CardRefundJobRule.Decide(latest))
            {
                case CardRefundStep.SendRequest:
                    if (FailBeforeRecord)
                    {
                        throw new InvalidOperationException("The attempt failed before the record was written.");
                    }

                    await runner.RunAsync(
                        ct =>
                        {
                            db.RefundRecords.Add(RefundRecord.Sending(refundId, time.GetUtcNow()));
                            return Task.FromResult(new StepResult());
                        },
                        call.Ct);
                    _statesAtSend.Add((await CardRefundJobRule.ReadLatestStateAsync(db, refundId, call.Job.CreatedAt, call.Ct))!.Value);
                    Interlocked.Increment(ref _sent);
                    if (DieAfterSend)
                    {
                        throw new InvalidOperationException("The process died after the request went.");
                    }

                    await WriteResultAsync(db, runner, refundId, time, call.Ct);
                    return JobOutcome.Completed;
                case CardRefundStep.AskOutcome:
                    Interlocked.Increment(ref _asked);
                    await WriteResultAsync(db, runner, refundId, time, call.Ct);
                    return JobOutcome.Completed;
                default:
                    return JobOutcome.Completed;
            }
        }

        private static Task<StepResult> WriteResultAsync(AppDbContext db, ITransactionRunner runner, Guid refundId, TimeProvider time, CancellationToken ct)
        {
            return runner.RunAsync(
                async cancel =>
                {
                    await db.RefundRecords
                        .Where(r => r.RefundId == refundId && r.State == RefundRecordState.Sending)
                        .ExecuteUpdateAsync(
                            s => s.SetProperty(r => r.State, RefundRecordState.Succeeded).SetProperty(r => r.OutcomeAt, time.GetUtcNow()),
                            cancel);
                    return new StepResult();
                },
                ct);
        }
    }
}
