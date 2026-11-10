using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Ordering;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Payment;

/// <summary>
/// The card refund's two job handlers on a real PostgreSQL with the fake provider and a fake clock (T405 KK4-KK9; 08 2.1.9; 05 4.3.3, 4.6; MK-40; D-518, D-908, D-878):
/// the record committed in the "sending" state before the request goes, the three answer classes through the one outcome function, the row that comes back and never
/// sends twice, the refund that is voided or turned to the transfer, the query's "completed" and its schedule past Z-17, the firm's mark that stops the query and the
/// success in flight at the mark that stays on the record, and the defect remedy's card refund with its job row.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class CardRefundJobTests : IAsyncLifetime
{
    private const string ContactEmail = "Ali@Example.com";

    private static readonly DateTimeOffset Start = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

    private static readonly ItemAmounts Amounts = new(100m, 0m, 16.67m);

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _clock = new(Start);
    private readonly FakePaymentGateway _gateway = new();
    private readonly CollectingLoggerProvider _logs = new();
    private ServiceProvider _provider = null!;
    private int _counter;

    public CardRefundJobTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private DateTimeOffset Now => _clock.GetUtcNow();

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, refund_records, refunds, payment_records, cancellations, internal_notes, transition_records, "
            + "order_items, orders, admins, legal_text_versions, provinces CASCADE");
        _provider = OrderingTestServices.Build(_collection.Dsn, _clock, true, _logs, services => services.AddSingleton<IPaymentGateway>(_gateway));
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK4: the record before the request; at most one open and one successful request ----

    [Fact]
    public async Task Request_GoesAfterTheSendingRecordIsCommitted_AndAcceptedIsSucceeded_O3_B8_F4_KK4_KK5()
    {
        (Order order, Guid refundId, Job job) = await SeedAsync(40m);
        List<(RefundRecordState State, Guid Id)> seenAtTheRequest = [];
        _gateway.BeforeRefund = async refund =>
        {
            await using AppDbContext reader = NewContext();
            seenAtTheRequest.AddRange(await reader.RefundRecords.Where(r => r.RefundId == refundId).Select(r => new ValueTuple<RefundRecordState, Guid>(r.State, r.Id)).ToListAsync(Ct));
        };

        Assert.Equal(JobOutcome.Completed, await RefundAsync(job));

        (RefundRecordState state, Guid recordId) = Assert.Single(seenAtTheRequest);
        Assert.Equal(RefundRecordState.Sending, state);
        FakeCall call = Assert.Single(_gateway.Calls);
        Assert.Equal(new CardRefund(order.OrderNumber, 40m, recordId), call.Argument);
        await using AppDbContext db = NewContext();
        RefundRecord record = await db.RefundRecords.SingleAsync(r => r.RefundId == refundId, Ct);
        Assert.Equal((recordId, RefundRecordState.Succeeded, (DateTimeOffset?)Now), (record.Id, record.State, record.OutcomeAt));
        Assert.Equal(Now, (await db.Refunds.SingleAsync(r => r.Id == refundId, Ct)).SentAt);
        Assert.Equal(PaymentStatus.PartiallyRefunded, (await db.Orders.SingleAsync(o => o.Id == order.Id, Ct)).PaymentStatus);
        Assert.Equal([DispatchType.B8, DispatchType.F4], (await db.DispatchRecords.Select(d => d.DispatchType).ToListAsync(Ct)).Order());
        Assert.False(await db.Jobs.AnyAsync(j => j.JobType == JobType.RefundOutcomeQuery, Ct));
    }

    [Fact]
    public async Task RowSeenAgain_AfterItsRequest_SendsNoSecondRequest_KK4()
    {
        (_, Guid refundId, Job job) = await SeedAsync(40m);
        _gateway.EnqueueRefund(new RefundRequestResult.Unknown(ExternalCallFailure.TimeoutReason));
        await RefundAsync(job);

        // The runner delivers the row again - after the unknown outcome and after a success alike: no second request, no second record.
        Assert.Equal(JobOutcome.Completed, await RefundAsync(job));
        (_, Guid otherRefundId, Job other) = await SeedAsync(30m);
        await RefundAsync(other);
        Assert.Equal(JobOutcome.Completed, await RefundAsync(other));

        Assert.Equal(2, _gateway.Calls.Count(c => c.Job == nameof(IPaymentGateway.RequestRefundAsync)));
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.RefundRecords.CountAsync(r => r.RefundId == refundId, Ct));
        Assert.Equal(1, await db.RefundRecords.CountAsync(r => r.RefundId == otherRefundId, Ct));
    }

    [Fact]
    public async Task RowSeenAgain_WithItsSendingRecord_DoesNotSend_TheOutcomeIsPending_AndTheQueryRowIsWritten_KK4()
    {
        (_, Guid refundId, Job job) = await SeedAsync(40m);

        // The host stopped after the record was committed and before the answer was written: the request went or may have gone.
        RefundRecord sending = RefundRecord.Sending(refundId, Now);
        await SaveAsync(c => c.RefundRecords.Add(sending));

        Assert.Equal(JobOutcome.Completed, await RefundAsync(job));

        Assert.Empty(_gateway.Calls);
        await using AppDbContext db = NewContext();
        Assert.Equal(RefundRecordState.OutcomePending, (await db.RefundRecords.SingleAsync(r => r.Id == sending.Id, Ct)).State);
        Job query = await db.Jobs.SingleAsync(j => j.JobType == JobType.RefundOutcomeQuery, Ct);
        Assert.Equal(((Guid?)sending.Id, (DateTimeOffset?)(Now + JobTimings.RefundOutcomeQueryFirstDelay)), (query.RelatedRecordId, query.NextAttemptAt));
    }

    [Fact]
    public async Task FirmRetry_IsANewRowAndANewRecord_AfterTheFailedOne_KK4()
    {
        (_, Guid refundId, Job job) = await SeedAsync(40m);
        _gateway.EnqueueRefund(new RefundRequestResult.NotCompleted("err:005"));
        await RefundAsync(job);

        _clock.Advance(TimeSpan.FromMinutes(10));
        Job retry = await EnqueueAsync(JobType.CardRefund, refundId);
        Assert.Equal(JobOutcome.Completed, await RefundAsync(retry));

        await using AppDbContext db = NewContext();
        List<RefundRecordState> states = await db.RefundRecords.Where(r => r.RefundId == refundId).OrderBy(r => r.CreatedAt).Select(r => r.State).ToListAsync(Ct);
        Assert.Equal([RefundRecordState.Failed, RefundRecordState.Succeeded], states);
    }

    // ---- KK5: the three answer classes, through the one outcome function ----

    [Theory]
    [InlineData("not-completed")]
    [InlineData("keys-missing")]
    [InlineData("unknown")]
    public async Task Answer_RefusedOrUnknown_LeavesTheAxis_FailedHasNoQuery_UnknownWritesTheQueryRow_KK5_KK9(string answer)
    {
        (Order order, Guid refundId, Job job) = await SeedAsync(40m);
        RefundRequestResult result = answer switch
        {
            "not-completed" => new RefundRequestResult.NotCompleted("err:005"),
            "keys-missing" => new RefundRequestResult.NotCompleted(PayTrPaymentGateway.KeysMissingReason),
            _ => new RefundRequestResult.Unknown(ExternalCallFailure.TimeoutReason),
        };
        _gateway.EnqueueRefund(result);

        await RefundAsync(job);

        RefundRecordState expected = CardRefundJobRule.OutcomeOf(result);
        await using AppDbContext db = NewContext();
        RefundRecord record = await db.RefundRecords.SingleAsync(r => r.RefundId == refundId, Ct);
        Assert.Equal(expected, record.State);
        Assert.Null((await db.Refunds.SingleAsync(r => r.Id == refundId, Ct)).SentAt);
        Assert.Equal(PaymentStatus.Paid, (await db.Orders.SingleAsync(o => o.Id == order.Id, Ct)).PaymentStatus);
        Assert.False(await db.DispatchRecords.AnyAsync(Ct));
        Assert.Equal(
            expected == RefundRecordState.OutcomePending ? [record.Id] : Array.Empty<Guid>(),
            await db.Jobs.Where(j => j.JobType == JobType.RefundOutcomeQuery).Select(j => j.RelatedRecordId!.Value).ToListAsync(Ct));
    }

    [Fact]
    public async Task WithoutTheGatewayInTheHost_TheRowFailsByTheCodeErrorRule_BeforeAnyRecord()
    {
        (_, Guid refundId, Job job) = await SeedAsync(40m);
        await using ServiceProvider bare = OrderingTestServices.Build(_collection.Dsn, _clock, true, null);
        await using AsyncServiceScope scope = bare.CreateAsyncScope();

        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<CardRefundJobHandler>().HandleAsync(Work(job), Ct));

        await using AppDbContext db = NewContext();
        Assert.False(await db.RefundRecords.AnyAsync(r => r.RefundId == refundId, Ct));
    }

    // ---- D-518, D-908: no request to a voided refund or to one turned to the transfer ----

    [Fact]
    public async Task VoidedRefund_GetsNoRequest_AndTheRowCompletes_D518()
    {
        (_, Guid refundId, Job job) = await SeedAsync(40m);
        await ExecuteAsync($"UPDATE refunds SET voided_at = now() WHERE id = '{refundId}'");

        Assert.Equal(JobOutcome.Completed, await RefundAsync(job));

        Assert.Empty(_gateway.Calls);
        await using AppDbContext db = NewContext();
        Assert.False(await db.RefundRecords.AnyAsync(Ct));
    }

    [Fact]
    public async Task RetryInTheQueue_ThenTheIbanTurnsTheRefundToTheTransfer_NoCardRequestGoes_D908()
    {
        (_, Guid refundId, Job job) = await SeedAsync(40m);
        _gateway.EnqueueRefund(new RefundRequestResult.NotCompleted("err:005"));
        await RefundAsync(job);
        _clock.Advance(TimeSpan.FromMinutes(10));
        Job retry = await EnqueueAsync(JobType.CardRefund, refundId);

        // The customer's IBAN turns the refund to the transfer while the retry waits in the queue.
        await ExecuteAsync($"UPDATE refunds SET payment_method = 'BankTransfer' WHERE id = '{refundId}'");
        Assert.Equal(JobOutcome.Completed, await RefundAsync(retry));

        Assert.Single(_gateway.Calls);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.RefundRecords.CountAsync(r => r.RefundId == refundId, Ct));
    }

    // ---- KK6, KK7: the query and its schedule ----

    [Fact]
    public async Task Query_Completed_IsAppliedByTheSameFunction_AndASecondApplicationChangesNothing_KK6_KK8()
    {
        (Order order, Guid refundId, Job query, Guid recordId) = await SeedPendingAsync(120m);
        _gateway.EnqueueRefundQuery(new RefundQueryResult.Completed());

        Assert.Equal(JobOutcome.Completed, await QueryAsync(query));

        FakeCall call = Assert.Single(QueryCalls());
        Assert.Equal(new CardRefund(order.OrderNumber, 120m, recordId), call.Argument);
        DateTimeOffset firstSent = Now;
        _clock.Advance(TimeSpan.FromMinutes(5));
        PaymentTransitionResult again = await ApplyAsync(new CardRefundOutcome(order.Id, recordId, RefundRecordState.Succeeded));
        Assert.Equal(JobOutcome.Completed, await QueryAsync(query));

        Assert.True(again.IsNotOpen);
        Assert.Single(QueryCalls());
        await using AppDbContext db = NewContext();
        Assert.Equal(firstSent, (await db.Refunds.SingleAsync(r => r.Id == refundId, Ct)).SentAt);
        Assert.Equal(PaymentStatus.Refunded, (await db.Orders.SingleAsync(o => o.Id == order.Id, Ct)).PaymentStatus);
        Assert.Equal(RefundRecordState.Succeeded, (await db.RefundRecords.SingleAsync(r => r.Id == recordId, Ct)).State);
        Assert.Equal(2, await db.DispatchRecords.CountAsync(Ct));
    }

    [Fact]
    public async Task Query_NoResult_AsksAgainByTheSchedule_AndPastZ17StillHourly_KK7()
    {
        (_, _, Job query, _) = await SeedPendingAsync(40m);
        JobWork work = Work(query);

        _clock.Advance(JobTimings.RefundOutcomeQueryFirstDelay);
        Assert.Equal(JobOutcome.RescheduleAfter(JobTimings.RefundOutcomeQueryIntervals[0]), await QueryAsync(work));

        // Z-17's fourteen days run out; the query has no upper limit and stays hourly.
        _clock.Advance(TimeSpan.FromDays(15));
        Assert.Equal(JobOutcome.RescheduleAfter(JobTimings.RefundOutcomeQueryHourly), await QueryAsync(work));
        Assert.Equal(2, QueryCalls().Count);
    }

    // ---- KK8: the firm's mark stops the query; a success in flight at the mark stays on the record ----

    [Fact]
    public async Task AfterTheFirmsMark_NoQueryIsAsked_AndNoQueryRowIsWritten_KK8()
    {
        (_, _, Job query, Guid recordId) = await SeedPendingAsync(40m);
        await MarkFailedByFirmAsync(recordId);

        Assert.Equal(JobOutcome.Completed, await QueryAsync(query));

        Assert.Empty(QueryCalls());
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Jobs.CountAsync(j => j.JobType == JobType.RefundOutcomeQuery, Ct));
    }

    [Fact]
    public async Task MarkWhileTheQueryWaits_ItsCompletedIsNotApplied_AndStaysOnTheMarkedRecord_OnlyOnce_KK8()
    {
        (Order order, Guid refundId, Job query, Guid recordId) = await SeedPendingAsync(40m);
        int versionBefore = await VersionAsync(order.Id);
        _gateway.BeforeRefundQuery = _ => MarkFailedByFirmAsync(recordId);
        _gateway.EnqueueRefundQuery(new RefundQueryResult.Completed());

        Assert.Equal(JobOutcome.Completed, await QueryAsync(query));
        DateTimeOffset lateAt = Now;
        _clock.Advance(TimeSpan.FromMinutes(1));
        PaymentTransitionResult again = await ApplyAsync(new CardRefundOutcome(order.Id, recordId, RefundRecordState.Succeeded));

        Assert.True(again.IsNotOpen);
        await using AppDbContext db = NewContext();
        RefundRecord record = await db.RefundRecords.SingleAsync(r => r.Id == recordId, Ct);
        Assert.Equal((RefundRecordState.Failed, true, (DateTimeOffset?)lateAt), (record.State, record.IsMarkedFailedByAdmin, record.LateSuccessAt));
        Assert.Null((await db.Refunds.SingleAsync(r => r.Id == refundId, Ct)).SentAt);
        Order after = await db.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((PaymentStatus.Paid, versionBefore + 1), (after.PaymentStatus, after.Version));
        Assert.False(await db.DispatchRecords.AnyAsync(Ct));
        Assert.False(await db.TransitionRecords.AnyAsync(Ct));
    }

    // ---- D-878: the defect remedy's card refund is sent by the system ----

    [Theory]
    [InlineData(PaymentMethod.Card, true)]
    [InlineData(PaymentMethod.BankTransfer, false)]
    public async Task DefectRemedyRefund_OnTheCardWay_WritesItsCardRefundRow_D878(PaymentMethod way, bool expectsRow)
    {
        (Order order, OrderItem item) = await SeedOrderAsync();
        Shopfolio.Modules.Administration.Admin admin = Shopfolio.Modules.Administration.Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(admin));
        Guid refundId;
        await using (AsyncServiceScope scope = _provider.CreateAsyncScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(Ct);
            refundId = await scope.ServiceProvider.GetRequiredService<IPaymentModule>().StartDefectRemedyRefundAsync(
                new DefectRemedyRefundStart(order.Id, way, item.Id, 20m, admin.Id, admin.Name, null), Ct);
            await db.SaveChangesAsync(Ct);
            await transaction.CommitAsync(Ct);
        }

        await using AppDbContext reader = NewContext();
        Assert.Equal(expectsRow ? [refundId] : Array.Empty<Guid>(), await reader.Jobs.Where(j => j.JobType == JobType.CardRefund).Select(j => j.RelatedRecordId!.Value).ToListAsync(Ct));
    }

    // ---- Helpers ----

    private List<FakeCall> QueryCalls() => [.. _gateway.Calls.Where(c => c.Job == nameof(IPaymentGateway.QueryRefundAsync))];

    private static JobWork Work(Job job) => new(job.Id, job.JobType, job.RelatedRecordId, job.CreatedAt, job.AttemptCount);

    private Task<JobOutcome> RefundAsync(Job job) => HandleAsync<CardRefundJobHandler>(Work(job));

    private Task<JobOutcome> QueryAsync(Job job) => QueryAsync(Work(job));

    private Task<JobOutcome> QueryAsync(JobWork work) => HandleAsync<RefundOutcomeQueryJobHandler>(work);

    private async Task<JobOutcome> HandleAsync<THandler>(JobWork work)
        where THandler : IJobHandler
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<THandler>().HandleAsync(work, Ct);
    }

    private async Task<PaymentTransitionResult> ApplyAsync(CardRefundOutcome outcome)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => unit.ApplyCardRefundOutcomeAsync(outcome, token), Ct);
    }

    /// <summary>The firm's "did not happen" mark as the panel writes it (T529): the pending record turns failed with its moment.</summary>
    private Task MarkFailedByFirmAsync(Guid recordId) =>
        ExecuteAsync($"UPDATE refund_records SET state = 'Failed', is_marked_failed_by_admin = true, outcome_at = now() WHERE id = '{recordId}'");

    private async Task<int> VersionAsync(Guid orderId)
    {
        await using AppDbContext db = NewContext();
        return (await db.Orders.SingleAsync(o => o.Id == orderId, Ct)).Version;
    }

    /// <summary>A paid order's system card refund of an item cancellation, with its job row.</summary>
    private async Task<(Order Order, Guid RefundId, Job Job)> SeedAsync(decimal amount)
    {
        (Order order, OrderItem item) = await SeedOrderAsync();
        Cancellation cancellation = Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now);
        Refund refund = Refund.BySystem(order.Id, RefundType.Cancellation, PaymentMethod.Card, amount, Now);
        refund.CancellationId = cancellation.Id;
        refund.OrderItemId = item.Id;
        refund.Quantity = 1;
        await SaveAsync(c =>
        {
            c.Cancellations.Add(cancellation);
            c.Refunds.Add(refund);
        });
        Job job = await EnqueueAsync(JobType.CardRefund, refund.Id);
        return (order, refund.Id, job);
    }

    /// <summary>A refund whose request timed out: its record pending and the query's row, written by the job handler itself.</summary>
    private async Task<(Order Order, Guid RefundId, Job Query, Guid RecordId)> SeedPendingAsync(decimal amount)
    {
        (Order order, Guid refundId, Job job) = await SeedAsync(amount);
        _gateway.EnqueueRefund(new RefundRequestResult.Unknown(ExternalCallFailure.TimeoutReason));
        await RefundAsync(job);
        await using AppDbContext db = NewContext();
        Job query = await db.Jobs.SingleAsync(j => j.JobType == JobType.RefundOutcomeQuery, Ct);
        Guid recordId = await db.RefundRecords.Where(r => r.RefundId == refundId).Select(r => r.Id).SingleAsync(Ct);
        Assert.Equal(recordId, query.RelatedRecordId);
        return (order, refundId, query, recordId);
    }

    private async Task<Job> EnqueueAsync(JobType type, Guid relatedRecordId)
    {
        // The runner completes the row its handler finished; the firm's retry is a new pending row of the same refund.
        await ExecuteAsync($"UPDATE jobs SET job_state = 'Completed', completed_at = now() WHERE related_record_id = '{relatedRecordId}' AND job_state = 'Pending'");
        Job job = Job.Enqueue(type, relatedRecordId, null, Now);
        await SaveAsync(c => c.Jobs.Add(job));
        return job;
    }

    private async Task<(Order Order, OrderItem Item)> SeedOrderAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Now, "metin", Now).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, ContactEmail, 120m, 0m, Now.AddHours(1), Now);
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
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Ürün", 3, 100m, 20m, 16.67m, Now);
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });
        await ExecuteAsync($"UPDATE orders SET order_status = 'Preparing', payment_status = 'Paid' WHERE id = '{order.Id}'");
        return (order, item);
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        await context.SaveChangesAsync(Ct);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
