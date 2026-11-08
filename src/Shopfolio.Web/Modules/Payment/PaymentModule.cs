using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The Payment module's implementation of its open surface; internal, reached only through <see cref="IPaymentModule"/>. The payment axis's members
/// (T317) write into the caller's open transaction under the order's row lock the caller holds, and save nothing.
/// </summary>
internal sealed class PaymentModule : IPaymentModule
{
    private readonly AppDbContext _db;
    private readonly IJobQueue _jobs;
    private readonly TimeProvider _time;

    public PaymentModule(AppDbContext db, IJobQueue jobs, TimeProvider time)
    {
        _db = db;
        _jobs = jobs;
        _time = time;
    }

    public async Task<int> CountOpenCardRefundsAsync(CancellationToken ct)
    {
        return await _db.Refunds.AsNoTracking()
            .Where(r => r.PaymentMethod == PaymentMethod.Card && r.SentAt == null && r.VoidedAt == null)
            .Where(r => _db.RefundRecords
                .Where(record => record.RefundId == r.Id)
                .OrderByDescending(record => record.CreatedAt)
                .ThenByDescending(record => record.Id)
                .Select(record => (RefundRecordState?)record.State)
                .FirstOrDefault() != RefundRecordState.Failed)
            .CountAsync(ct);
    }

    public async Task<bool> IsPaymentRecordedAsync(string providerTransactionId, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerTransactionId);
        return _db.PaymentRecords.Local.Any(p => p.ProviderTransactionId == providerTransactionId)
            || await _db.PaymentRecords.AnyAsync(p => p.ProviderTransactionId == providerTransactionId, ct);
    }

    public Task<Guid> RecordCardPaymentAsync(Guid orderId, string providerTransactionId, Guid? transitionRecordId, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerTransactionId);
        PaymentRecord record = PaymentRecord.ForCardPayment(orderId, providerTransactionId, transitionRecordId, _time.GetUtcNow());
        _db.PaymentRecords.Add(record);
        return Task.FromResult(record.Id);
    }

    public Task<Guid> RecordTransferMarkAsync(Guid orderId, Guid transitionRecordId, CancellationToken ct)
    {
        PaymentRecord record = PaymentRecord.ForBankTransferMark(orderId, transitionRecordId, _time.GetUtcNow());
        _db.PaymentRecords.Add(record);
        return Task.FromResult(record.Id);
    }

    public async Task<Guid> StartLatePaymentRefundAsync(Guid orderId, Guid paymentRecordId, decimal paidTotal, CancellationToken ct)
    {
        Refund refund = Refund.ForLatePayment(orderId, paymentRecordId, paidTotal, _time.GetUtcNow());
        _db.Refunds.Add(refund);

        // The card refund is a side effect that goes outside: its job row is written in the same transaction (05 4.3.1; 4.5.3, K-970).
        await _jobs.EnqueueAsync(JobType.CardRefund, refund.Id, null, ct);
        return refund.Id;
    }

    public async Task<RefundFacts?> WriteCardRefundOutcomeAsync(Guid orderId, Guid refundRecordId, RefundRecordState outcome, CancellationToken ct)
    {
        if (outcome is not (RefundRecordState.Succeeded or RefundRecordState.Failed or RefundRecordState.OutcomePending))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "An outcome is succeeded, failed or pending; sending is the state before the request.");
        }

        RefundRecord? record = await _db.RefundRecords.SingleOrDefaultAsync(r => r.Id == refundRecordId, ct);
        if (record is null || record.State is not (RefundRecordState.Sending or RefundRecordState.OutcomePending))
        {
            return null;
        }

        Refund? refund = await _db.Refunds.SingleOrDefaultAsync(r => r.Id == record.RefundId, ct);
        if (refund is null || refund.OrderId != orderId || refund.PaymentMethod != PaymentMethod.Card || refund.SentAt is not null || refund.VoidedAt is not null)
        {
            return null;
        }

        DateTimeOffset now = _time.GetUtcNow();
        record.State = outcome;
        if (outcome is RefundRecordState.Succeeded or RefundRecordState.Failed)
        {
            record.OutcomeAt = now;
        }

        if (outcome == RefundRecordState.Succeeded)
        {
            refund.SentAt = now;
        }

        // A success after a failed request of the same refund is the firm's retry (02 7.2.8; 03 7.1.18).
        bool isFirmRetry = await _db.RefundRecords.AnyAsync(r => r.RefundId == refund.Id && r.Id != record.Id && r.State == RefundRecordState.Failed, ct);
        return Facts(refund, isFirmRetry);
    }

    public async Task<RefundFacts?> MarkTransferRefundSentAsync(Guid orderId, Guid refundId, CancellationToken ct)
    {
        Refund? refund = await _db.Refunds.SingleOrDefaultAsync(r => r.Id == refundId, ct);
        if (refund is null || refund.OrderId != orderId || refund.PaymentMethod != PaymentMethod.BankTransfer || refund.SentAt is not null || refund.VoidedAt is not null)
        {
            return null;
        }

        refund.SentAt = _time.GetUtcNow();
        return Facts(refund, isFirmRetry: false);
    }

    public async Task<decimal> SentRefundTotalAsync(Guid orderId, CancellationToken ct)
    {
        // Loading puts the saved rows into the context without overwriting the tracked ones; the open transaction's changes are read from there.
        await _db.Refunds.Where(r => r.OrderId == orderId).LoadAsync(ct);
        return _db.Refunds.Local
            .Where(r => r.OrderId == orderId && r.SentAt is not null && r.VoidedAt is null && r.RefundType != RefundType.LatePayment)
            .Sum(r => r.Amount);
    }

    public async Task<RefundMarks> ReadRefundMarksAsync(Guid orderId, CancellationToken ct)
    {
        await _db.Refunds.Where(r => r.OrderId == orderId).LoadAsync(ct);
        List<Refund> open = [.. _db.Refunds.Local.Where(r => r.OrderId == orderId && r.SentAt is null && r.VoidedAt is null)];
        List<Guid> openIds = [.. open.Select(r => r.Id)];
        await _db.RefundRecords.Where(r => openIds.Contains(r.RefundId)).LoadAsync(ct);

        List<RefundRecordState> latest = [.. open
            .Select(refund => _db.RefundRecords.Local
                .Where(r => r.RefundId == refund.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .FirstOrDefault())
            .OfType<RefundRecord>()
            .Select(r => r.State)];

        return new RefundMarks(latest.Contains(RefundRecordState.Failed), latest.Contains(RefundRecordState.OutcomePending));
    }

    public async Task<IReadOnlyList<PaymentSource>> FindPaymentSourcesAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        if (orderIds.Count == 0)
        {
            return [];
        }

        Guid[] ids = [.. orderIds];
        return await _db.PaymentRecords.AsNoTracking()
            .Where(p => ids.Contains(p.OrderId))
            .Select(p => new PaymentSource(p.OrderId, p.TransitionRecordId))
            .ToListAsync(ct);
    }

    public async Task<bool> HasRefundAsync(Guid orderId, CancellationToken ct)
    {
        await _db.Refunds.Where(r => r.OrderId == orderId).LoadAsync(ct);
        return _db.Refunds.Local.Any(r => r.OrderId == orderId && r.VoidedAt is null);
    }

    public async Task<bool> IsCancellationRefundUnderwayAsync(IReadOnlyCollection<Guid> cancellationIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cancellationIds);
        if (cancellationIds.Count == 0)
        {
            return false;
        }

        // Sent, or a card request that is being sent, whose outcome is awaited or that succeeded: the money may have left (06 4.22; K-940). A failed
        // request alone does not stop the correction - the refund is voided with it.
        Guid[] ids = [.. cancellationIds];
        List<Refund> refunds = await LoadCancellationRefundsAsync(ids, ct);
        if (refunds.Any(r => r.SentAt is not null))
        {
            return true;
        }

        Guid[] refundIds = [.. refunds.Select(r => r.Id)];
        await _db.RefundRecords.Where(r => refundIds.Contains(r.RefundId)).LoadAsync(ct);
        return _db.RefundRecords.Local.Any(r => refundIds.Contains(r.RefundId)
            && r.State is RefundRecordState.Sending or RefundRecordState.OutcomePending or RefundRecordState.Succeeded);
    }

    public async Task<IReadOnlyList<Guid>> VoidCancellationRefundsAsync(IReadOnlyCollection<Guid> cancellationIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cancellationIds);
        if (cancellationIds.Count == 0)
        {
            return [];
        }

        DateTimeOffset now = _time.GetUtcNow();
        List<Refund> voided = [.. (await LoadCancellationRefundsAsync([.. cancellationIds], ct)).Where(r => r.SentAt is null)];
        foreach (Refund refund in voided)
        {
            refund.VoidedAt = now;
        }

        return [.. voided.Select(r => r.Id)];
    }

    public async Task<IReadOnlyList<Guid>> ErasePersonalDataAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The refunds' personal data is emptied inside the caller's transaction (09 6 item 3).");
        }

        if (orderIds.Count == 0)
        {
            return [];
        }

        // Every refund of the orders is loaded (the caller needs all their ids) and tracked: the order side's version guard reads the tracked rows
        // (09 10.2). Only the administrator link and name are emptied (06 9.2.1); the amount, the type, the links and the moments stay.
        Guid[] ids = [.. orderIds];
        List<Refund> refunds = await _db.Refunds.Where(r => ids.Contains(r.OrderId)).ToListAsync(ct);
        foreach (Refund refund in refunds)
        {
            refund.AdminId = null;
            refund.AdminName = null;
        }

        return [.. refunds.Select(r => r.Id)];
    }

    /// <summary>The cancellations' refunds that are not voided, the open transaction's changes included.</summary>
    private async Task<List<Refund>> LoadCancellationRefundsAsync(Guid[] cancellationIds, CancellationToken ct)
    {
        await _db.Refunds.Where(r => r.CancellationId != null && cancellationIds.Contains(r.CancellationId.Value)).LoadAsync(ct);
        return [.. _db.Refunds.Local.Where(r => r.CancellationId is { } id && cancellationIds.Contains(id) && r.VoidedAt is null)];
    }

    private static RefundFacts Facts(Refund refund, bool isFirmRetry)
    {
        return new RefundFacts(
            refund.Id,
            refund.OrderId,
            refund.RefundType,
            refund.Amount,
            refund.PaymentMethod,
            refund.ActorKind,
            refund.AdminId,
            refund.AdminName,
            refund.SentAt is not null,
            refund.VoidedAt is not null,
            isFirmRetry);
    }
}

/// <summary>
/// The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddPaymentModule</c> once, and maps the payment provider's notification
/// endpoint with <c>MapPaymentNotificationEndpoint</c> (UC-80).
/// </summary>
internal static class PaymentModuleRegistration
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services)
    {
        services.AddScoped<IPaymentModule, PaymentModule>();
        services.AddDisposalPart<InboundNotificationDisposalPart>();
        services.AddScoped<PaymentNotificationEndpoint>();
        services.AddTransactionalJobHandler<InboundNotificationJobHandler>(JobType.InboundNotificationProcessing);

        // The final query's handler (FinalPaymentQueryJobHandler, T324) takes the payment gateway, which has no implementation before the provider's task:
        // T403 registers it with AddJobHandler<FinalPaymentQueryJobHandler>(JobType.FinalPaymentQuery) beside the gateway (D-449). Until then the rows the
        // card window's end writes wait in the queue - a type without a handler is not read (09 13.1 item 3).
        return services;
    }

    /// <summary>
    /// Maps UC-80 (07 4.30.1): <c>POST /sistem/odeme-bildirimi</c> as a signed machine endpoint, outside the protection against cross-site forgery
    /// (07 2.5.5), with the body ceiling of the notification class (07 2.8.1).
    /// </summary>
    public static IEndpointConventionBuilder MapPaymentNotificationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapSignedEndpoint<PaymentNotificationEndpoint, Infrastructure.Integrations.PaymentNotice>(
            Infrastructure.Security.AntiforgeryExemptions.PaymentNotificationPath);
    }
}
