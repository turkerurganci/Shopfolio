using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

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
        // Both marks are the card way's (06 4.34; K-675, K-1015 (5)): a refund the customer's IBAN turned to the transfer drops the mark (D-322).
        List<Refund> open = [.. _db.Refunds.Local.Where(r => r.OrderId == orderId && r.SentAt is null && r.VoidedAt is null && r.PaymentMethod == PaymentMethod.Card)];
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

    public async Task<IReadOnlySet<Guid>> FindSettledRefundSubjectsAsync(IReadOnlyCollection<Guid> subjectIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(subjectIds);
        HashSet<Guid> settled = [];
        if (subjectIds.Count == 0)
        {
            return settled;
        }

        // A sent refund is done; a voided one never goes (06 4.33). Each subject reaches its refund through its own link.
        Guid[] ids = [.. subjectIds.Distinct()];
        List<RefundLinks> refunds = await _db.Refunds.AsNoTracking()
            .Where(r => (r.SentAt != null || r.VoidedAt != null)
                && ((r.CancellationId != null && ids.Contains(r.CancellationId.Value))
                    || (r.LateDeliveryTerminationId != null && ids.Contains(r.LateDeliveryTerminationId.Value))
                    || (r.ReturnId != null && ids.Contains(r.ReturnId.Value))))
            .Select(r => new RefundLinks(r.CancellationId, r.LateDeliveryTerminationId, r.ReturnId))
            .ToListAsync(ct);
        foreach (RefundLinks refund in refunds)
        {
            foreach (Guid? link in new[] { refund.CancellationId, refund.LateDeliveryTerminationId, refund.ReturnId })
            {
                if (link is { } id && ids.Contains(id))
                {
                    settled.Add(id);
                }
            }
        }

        return settled;
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

    public async Task<IReadOnlyDictionary<Guid, RescindedShares>> RescindedSharesAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        if (ids.Length == 0)
        {
            return new Dictionary<Guid, RescindedShares>();
        }

        // The saved rows are loaded into the context, so the rows the caller added a moment ago and the ones it voided are read the same way (HasRefundAsync's way).
        await _db.Refunds.Where(r => r.RefundType == RefundType.DefectClaimRescission && r.OrderItemId != null && ids.Contains(r.OrderItemId.Value)).LoadAsync(ct);
        return _db.Refunds.Local
            .Where(r => r.RefundType == RefundType.DefectClaimRescission && r.VoidedAt is null && r.OrderItemId is { } item && ids.Contains(item))
            .GroupBy(r => r.OrderItemId!.Value)
            .ToDictionary(
                g => g.Key,
                g => new RescindedShares(g.Sum(r => r.Quantity ?? 0), g.Sum(r => r.CouponShare ?? 0m), g.Sum(r => r.VatAmount ?? 0m)));
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

    public async Task<RecordRefundStarted> StartRecordRefundAsync(RecordRefundStart start, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(start);
        if (start.RefundType is not (RefundType.Cancellation or RefundType.ItemRemoval or RefundType.LateDeliveryTermination))
        {
            throw new ArgumentException("The record refund of the line is a cancellation's, a removal's or a late delivery termination's (06 4.33).", nameof(start));
        }

        PaymentMethod way = RefundLineRules.WayOf(start.OrderPaymentMethod);
        if (!RefundLineRules.IsStartedBySystem(start.RefundType, way))
        {
            // The transfer line: the firm processes the refund on the panel, and its row is born with that step (06 4.33; 02 7.2.8).
            return new RecordRefundStarted(null, way, RefundLineRules.NeedsIbanField(way, start.HasIban));
        }

        Refund refund = Refund.BySystem(start.OrderId, start.RefundType, way, start.Amount, _time.GetUtcNow());
        refund.IsShippingFeeIncluded = start.IsShippingFeeIncluded;
        refund.OrderItemId = start.OrderItemId;
        refund.Quantity = start.Quantity;
        switch (start.RefundType)
        {
            case RefundType.Cancellation:
                refund.CancellationId = start.RecordId;
                break;
            case RefundType.ItemRemoval:
                refund.ItemRemovalId = start.RecordId;
                break;
            default:
                refund.LateDeliveryTerminationId = start.RecordId;
                break;
        }

        _db.Refunds.Add(refund);

        // The card refund is a side effect that goes outside: its job row is written in the same transaction (05 4.3.1; 4.6.1).
        await _jobs.EnqueueAsync(JobType.CardRefund, refund.Id, null, ct);
        return new RecordRefundStarted(refund.Id, way, false);
    }

    public async Task<CardRefundLine?> ReadRefundLineAsync(Guid orderId, Guid refundId, CancellationToken ct)
    {
        Refund? refund = await _db.Refunds.SingleOrDefaultAsync(r => r.Id == refundId, ct);
        if (refund is null || refund.OrderId != orderId)
        {
            return null;
        }

        RefundRecordState? latest = await LatestStateAsync(refund.Id, ct);
        return new CardRefundLine(refund.Id, refund.OrderId, refund.OrderItemId, refund.PaymentMethod, refund.SentAt is not null, refund.VoidedAt is not null, latest);
    }

    public async Task<bool> RetryCardRefundAsync(Guid orderId, Guid refundId, bool isIbanEntered, CancellationToken ct)
    {
        CardRefundLine? line = await ReadRefundLineAsync(orderId, refundId, ct);
        if (line is null || !RefundLineRules.ActionsOf(line, isIbanEntered).CanRetry)
        {
            return false;
        }

        // The retry is a new request of the same refund (06 4.34): a new job row; its handler writes the new refund record (CardRefundJobRule).
        await _jobs.EnqueueAsync(JobType.CardRefund, refundId, null, ct);
        return true;
    }

    public async Task<bool> TurnRefundToTransferAsync(Guid orderId, Guid refundId, CancellationToken ct)
    {
        CardRefundLine? line = await ReadRefundLineAsync(orderId, refundId, ct);
        if (line is null || !RefundLineRules.ActionsOf(line, isIbanEntered: true).CanTurnToTransfer)
        {
            return false;
        }

        Refund refund = await _db.Refunds.SingleAsync(r => r.Id == refundId, ct);
        refund.PaymentMethod = PaymentMethod.BankTransfer;
        return true;
    }

    public async Task<IReadOnlyList<RefundOfRecord>> FindRefundsOfRecordsAsync(IReadOnlyCollection<Guid> recordIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(recordIds);
        Guid[] ids = [.. recordIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        // Loading puts the saved rows into the context without overwriting the tracked ones; the open transaction's changes are read from there.
        static bool Touches(Refund r, Guid[] ids) =>
            ids.Contains(r.Id)
            || (r.OrderItemId is { } item && ids.Contains(item))
            || (r.CancellationId is { } c && ids.Contains(c))
            || (r.ItemRemovalId is { } i && ids.Contains(i))
            || (r.LateDeliveryTerminationId is { } t && ids.Contains(t))
            || (r.WithdrawalId is { } w && ids.Contains(w))
            || (r.ReturnId is { } ret && ids.Contains(ret));
        await _db.Refunds
            .Where(r => ids.Contains(r.Id)
                || (r.OrderItemId != null && ids.Contains(r.OrderItemId.Value))
                || (r.CancellationId != null && ids.Contains(r.CancellationId.Value))
                || (r.ItemRemovalId != null && ids.Contains(r.ItemRemovalId.Value))
                || (r.LateDeliveryTerminationId != null && ids.Contains(r.LateDeliveryTerminationId.Value))
                || (r.WithdrawalId != null && ids.Contains(r.WithdrawalId.Value))
                || (r.ReturnId != null && ids.Contains(r.ReturnId.Value)))
            .LoadAsync(ct);
        return [.. _db.Refunds.Local
            .Where(r => Touches(r, ids))
            .Select(r => new RefundOfRecord(
                r.Id, r.OrderItemId, r.CancellationId, r.ItemRemovalId, r.LateDeliveryTerminationId, r.WithdrawalId, r.ReturnId, r.Quantity, r.SentAt ?? r.VoidedAt))];
    }

    /// <summary>The state of the refund's latest card request, the open transaction's changes included; null when none was written.</summary>
    private async Task<RefundRecordState?> LatestStateAsync(Guid refundId, CancellationToken ct)
    {
        await _db.RefundRecords.Where(r => r.RefundId == refundId).LoadAsync(ct);
        return _db.RefundRecords.Local
            .Where(r => r.RefundId == refundId)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Select(r => (RefundRecordState?)r.State)
            .FirstOrDefault();
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

    /// <summary>The three subject links of a settled refund, as <see cref="FindSettledRefundSubjectsAsync"/> reads them.</summary>
    private sealed record RefundLinks(Guid? CancellationId, Guid? LateDeliveryTerminationId, Guid? ReturnId);
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

        // The card line of the sales gate (T352; D-81, D-198): it reads the conditional lines worked out at start-up, registered here in place of the Settings module's
        // unbound default (that one is a TryAdd, and a later registration wins either way). The offered methods read the same two conditions as the gate.
        services.AddConditionalLines();
        services.AddScoped<ICardPaymentCondition, CardPaymentCondition>();
        services.AddScoped<IPaymentMethods, PaymentMethods>();
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
