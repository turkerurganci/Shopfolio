using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The firm's refund steps on the order screen (T529; 07 UC-131, UC-133, UC-137; 05 4.6; 06 4.33, 4.34): the refund row born with the administrator's step, the administrator's "gerçekleşmedi"
/// mark on a card refund whose result is awaited, and the refund lines the screen reads. Writes join the caller's transaction under the order's row lock and save nothing.
/// </summary>
internal sealed partial class PaymentModule
{
    public async Task<RefundFacts> StartFirmRefundAsync(FirmRefundStart start, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(start);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The firm's refund is started inside the caller's transaction, under the order's row lock (09 6 item 3).");
        }

        if (start.RefundType is RefundType.DefectClaimRescission or RefundType.LatePayment)
        {
            throw new ArgumentException("A defect claim's rescission and a late payment have their refund rows already (06 4.33).", nameof(start));
        }

        PaymentMethod way = RefundLineRules.WayOf(start.OrderPaymentMethod);
        Refund refund = Refund.ByAdmin(start.OrderId, start.RefundType, way, start.Amount, start.AdminId, start.AdminName, _time.GetUtcNow());
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
            case RefundType.LateDeliveryTermination:
                refund.LateDeliveryTerminationId = start.RecordId;
                break;
            case RefundType.Withdrawal:
                refund.WithdrawalId = start.RecordId;
                refund.ReturnId = start.ReturnId;
                break;
            default:
                refund.NotReturnedClosureId = start.NotReturnedClosureId;
                break;
        }

        _db.Refunds.Add(refund);
        if (way == PaymentMethod.Card)
        {
            // The card refund goes outside: its job row is written in the same transaction and the provider's result moves the axis (05 4.3.1, 4.6.1; K-970).
            await _jobs.EnqueueAsync(JobType.CardRefund, refund.Id, null, ct);
        }
        else
        {
            // On the transfer the firm's step is the sending itself (07 UC-131: "işlemle gönderilmiş sayılır").
            refund.SentAt = _time.GetUtcNow();
        }

        return Facts(refund, isFirmRetry: false);
    }

    public async Task<bool> MarkCardRefundFailedByAdminAsync(Guid orderId, Guid refundId, CancellationToken ct)
    {
        CardRefundLine? line = await ReadRefundLineAsync(orderId, refundId, ct);
        if (line is not { Way: PaymentMethod.Card, IsSent: false, IsVoided: false, LatestState: RefundRecordState.OutcomePending })
        {
            return false;
        }

        // The latest request is the one the outcome query waits on; the mark turns it failed and the query handler finds no pending record to ask about (06 4.34; RefundOutcomeQueryJobHandler).
        RefundRecord latest = _db.RefundRecords.Local
            .Where(r => r.RefundId == refundId)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .First();
        latest.State = RefundRecordState.Failed;
        latest.OutcomeAt = _time.GetUtcNow();
        latest.IsMarkedFailedByAdmin = true;
        return true;
    }

    public async Task<IReadOnlyList<PanelRefundFacts>> ListPanelRefundsAsync(Guid orderId, CancellationToken ct)
    {
        await _db.Refunds.Where(r => r.OrderId == orderId).LoadAsync(ct);
        List<Refund> refunds = [.. _db.Refunds.Local.Where(r => r.OrderId == orderId).OrderBy(r => r.CreatedAt).ThenBy(r => r.Id)];
        Guid[] ids = [.. refunds.Select(r => r.Id)];
        await _db.RefundRecords.Where(r => ids.Contains(r.RefundId)).LoadAsync(ct);
        return
        [
            .. refunds.Select(r =>
            {
                RefundRecord? latest = _db.RefundRecords.Local.Where(x => x.RefundId == r.Id).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).FirstOrDefault();
                return new PanelRefundFacts(
                    r.Id,
                    r.RefundType,
                    r.PaymentMethod,
                    r.Amount,
                    r.IsShippingFeeIncluded,
                    r.OrderItemId,
                    r.Quantity,
                    r.CreatedAt,
                    r.SentAt is not null,
                    r.VoidedAt is not null,
                    latest?.State,
                    latest?.IsMarkedFailedByAdmin ?? false,
                    // The warning stays on the refund's line (04 9.9.8; K-978): a later retry's record does not hide the success that came after the mark.
                    _db.RefundRecords.Local.Where(x => x.RefundId == r.Id).Max(x => x.LateSuccessAt))
                {
                    CancellationId = r.CancellationId,
                    ItemRemovalId = r.ItemRemovalId,
                    LateDeliveryTerminationId = r.LateDeliveryTerminationId,
                    WithdrawalId = r.WithdrawalId,
                    ReturnId = r.ReturnId,
                    DefectClaimId = r.DefectClaimId,
                    NotReturnedClosureId = r.NotReturnedClosureId,
                    PaymentRecordId = r.PaymentRecordId,
                };
            }),
        ];
    }
}
