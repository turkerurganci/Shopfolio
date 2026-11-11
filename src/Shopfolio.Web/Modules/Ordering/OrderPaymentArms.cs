using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The one place that reads which arm of the retention an order is on (06 4.32, 9.1 Z-28, Z-41, Z-43; K-1030 (3), (4)): the arm is read from the payment
/// record at the moment of the disposal and never stored. An order is paid when it has a payment record that is not corrected - a record whose confirming
/// transition (Ö1) a correction row corrects is not, a late card payment (no transition) never is corrected; an unpaid order that is Failed has the moment of
/// its Ö2 transition as the moment it failed. The order's own disposal part (<see cref="OrderDisposalPart"/>) and the other modules' parts, through
/// <see cref="IOrderingModule"/>, judge with this one reading, so a change of the rule is made once.
/// </summary>
internal sealed class OrderPaymentArms
{
    private readonly AppDbContext _db;
    private readonly IPaymentModule _payment;

    public OrderPaymentArms(AppDbContext db, IPaymentModule payment)
    {
        _db = db;
        _payment = payment;
    }

    /// <summary>The arm of each of the given orders that exists; a plain read.</summary>
    public async Task<IReadOnlyList<OrderPaymentArm>> ReadAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        Guid[] ids = [.. orderIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        List<OrderRow> rows = await _db.Orders.AsNoTracking()
            .Where(o => ids.Contains(o.Id))
            .Select(o => new OrderRow(o.Id, o.PaymentStatus, o.ContactEmail == null))
            .ToListAsync(ct);

        IReadOnlyList<PaymentSource> sources = await _payment.FindPaymentSourcesAsync([.. rows.Select(r => r.Id)], ct);
        Guid[] confirming = [.. sources.Where(s => s.TransitionRecordId is not null).Select(s => s.TransitionRecordId!.Value).Distinct()]; // non-null: the Where before it keeps only sources with a transition record
        HashSet<Guid> corrected = confirming.Length == 0
            ? []
            : [.. await _db.TransitionRecords.AsNoTracking()
                .Where(t => t.CorrectedTransitionRecordId != null && confirming.Contains(t.CorrectedTransitionRecordId.Value))
                .Select(t => t.CorrectedTransitionRecordId!.Value) // non-null: the Where above keeps only records that correct another
                .ToListAsync(ct)];
        HashSet<Guid> paid = [.. sources.Where(s => s.TransitionRecordId is null || !corrected.Contains(s.TransitionRecordId.Value)).Select(s => s.OrderId)];

        Guid[] failedUnpaid = [.. rows.Where(r => !paid.Contains(r.Id) && r.PaymentStatus == PaymentStatus.Failed).Select(r => r.Id)];
        Dictionary<Guid, DateTimeOffset> failedAt = failedUnpaid.Length == 0
            ? []
            : await _db.TransitionRecords.AsNoTracking()
                .Where(t => failedUnpaid.Contains(t.OrderId) && t.TransitionCode == TransitionCode.O2)
                .GroupBy(t => t.OrderId)
                .Select(g => new FailedMoment(g.Key, g.Max(t => t.CreatedAt)))
                .ToDictionaryAsync(g => g.OrderId, g => g.At, ct);

        return
        [
            .. rows.Select(r => new OrderPaymentArm(
                r.Id,
                paid.Contains(r.Id),
                !paid.Contains(r.Id) && failedAt.TryGetValue(r.Id, out DateTimeOffset at) ? at : null,
                r.PersonalDataDisposed)),
        ];
    }

    /// <summary>
    /// The unpaid orders that failed before <paramref name="failedBefore"/>, except the ones in <paramref name="exclude"/>, at most <paramref name="max"/>
    /// (06 9.1 Z-41: the notification rows of such an order go ten years after the moment it failed). The database narrows to the Failed orders that have an
    /// Ö2 before the moment; the arm is the exact judgement, so a Failed order a late card payment was refunded for is passed over.
    /// </summary>
    public async Task<IReadOnlyList<Guid>> FindUnpaidFailedBeforeAsync(
        DateTimeOffset failedBefore,
        IReadOnlyCollection<Guid> exclude,
        int max,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(exclude);
        ArgumentOutOfRangeException.ThrowIfLessThan(max, 1);
        List<Guid> found = [];
        List<Guid> seen = [.. exclude];
        while (found.Count < max)
        {
            Guid[] passed = [.. seen];
            List<Guid> page = await _db.Orders.AsNoTracking()
                .Where(o => !passed.Contains(o.Id)
                    && o.PaymentStatus == PaymentStatus.Failed
                    && _db.TransitionRecords.Any(t => t.OrderId == o.Id && t.TransitionCode == TransitionCode.O2 && t.CreatedAt < failedBefore))
                .OrderBy(o => o.Id)
                .Select(o => o.Id)
                .Take(max)
                .ToListAsync(ct);
            if (page.Count == 0)
            {
                break;
            }

            IReadOnlyList<OrderPaymentArm> arms = await ReadAsync(page, ct);
            HashSet<Guid> due = [.. arms.Where(a => !a.IsPaid && a.FailedAt is { } at && at < failedBefore).Select(a => a.OrderId)];
            seen.AddRange(page);
            found.AddRange(page.Where(due.Contains).Take(max - found.Count));
            if (page.Count < max)
            {
                break;
            }
        }

        return found;
    }

    private sealed record OrderRow(Guid Id, PaymentStatus PaymentStatus, bool PersonalDataDisposed);

    private sealed record FailedMoment(Guid OrderId, DateTimeOffset At);
}
