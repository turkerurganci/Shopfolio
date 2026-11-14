using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The IBAN a transfer refund is paid to (02 7.4.5; 04 9.9.6, 9.9.21; D-907; K-356, K-1131; 09 25 one function): one decision per refund, read by the panel's
/// refund line and steps and by the IBAN's life (<see cref="RefundIbanUses.IsInUse"/>), so the screen never pays a refund to an IBAN the life rule does not
/// count, nor keeps an IBAN for a refund the screen does not pay to it.
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<IReadOnlyList<RefundPayee>> ResolveRefundPayeesAsync(Guid orderId, IReadOnlyCollection<RefundPayeeSubject> subjects, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(subjects);
        PayeeBook book = await LoadPayeeBookAsync(orderId, subjects, ct);
        return [.. subjects.Select(book.PayeeOf)];
    }

    /// <summary>
    /// Reads what the subjects' decisions need in five queries whatever their number (09 20 items 2-4): the order's IBANs on file in their order, the requests of
    /// the subjects' refunds and items, and the cancellations, late delivery terminations and withdrawals of the subjects' records and items that carry an IBAN.
    /// </summary>
    private async Task<PayeeBook> LoadPayeeBookAsync(Guid orderId, IReadOnlyCollection<RefundPayeeSubject> subjects, CancellationToken ct)
    {
        List<IbanOnFile> file = await _db.RefundIbans.AsNoTracking().Where(i => i.OrderId == orderId)
            .OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)
            .Select(i => new IbanOnFile(i.Id, i.Iban))
            .ToListAsync(ct);
        if (file.Count == 0 || subjects.Count == 0)
        {
            // Without an IBAN on file every refund waits for one.
            return new PayeeBook(file, [], []);
        }

        Guid[] refundIds = [.. subjects.Select(s => s.RefundId).OfType<Guid>().Distinct()];
        Guid[] itemIds = [.. subjects.Select(s => s.OrderItemId).OfType<Guid>().Distinct()];
        Guid[] recordIds = [.. subjects.Select(s => s.RecordId).OfType<Guid>().Distinct()];
        Guid[] fileIds = [.. file.Select(f => f.Id)];

        // The subjects' refunds' and items' requests, and every request on a refund that brought an IBAN on the file: that IBAN is its refund's own (the file step leaves it).
        List<PayeeRequest> requests = await _db.RefundIbanRequests.AsNoTracking()
            .Where(r => (r.RefundId != null && refundIds.Contains(r.RefundId.Value)) || (r.OrderItemId != null && itemIds.Contains(r.OrderItemId.Value))
                || (r.RefundId != null && r.RefundIbanId != null && fileIds.Contains(r.RefundIbanId.Value)))
            .Select(r => new PayeeRequest(r.OrderItemId, r.RefundId, r.RefundIbanId, r.FulfilledAt == null && r.ClosedAt == null))
            .ToListAsync(ct);
        List<PayeeRecord> records =
        [
            .. await _db.Cancellations.AsNoTracking().Where(c => c.RefundIbanId != null && (recordIds.Contains(c.Id) || itemIds.Contains(c.OrderItemId)))
                .Select(c => new PayeeRecord(c.Id, c.OrderItemId, c.RefundIbanId!.Value)).ToListAsync(ct), // non-null: the Where keeps only cancellations with an IBAN
            .. await _db.LateDeliveryTerminations.AsNoTracking().Where(t => t.RefundIbanId != null && (recordIds.Contains(t.Id) || itemIds.Contains(t.OrderItemId)))
                .Select(t => new PayeeRecord(t.Id, t.OrderItemId, t.RefundIbanId!.Value)).ToListAsync(ct), // non-null: the Where keeps only terminations with an IBAN
            .. await _db.Withdrawals.AsNoTracking().Where(w => w.RefundIbanId != null && (recordIds.Contains(w.Id) || itemIds.Contains(w.OrderItemId)))
                .Select(w => new PayeeRecord(w.Id, w.OrderItemId, w.RefundIbanId!.Value)).ToListAsync(ct), // non-null: the Where keeps only withdrawals with an IBAN
        ];
        return new PayeeBook(file, requests, records);
    }

    /// <summary>An IBAN on the order's file and its value.</summary>
    private sealed record IbanOnFile(Guid Id, string Iban);

    /// <summary>An IBAN request of a subject's refund or item: the IBAN it brought (gone: null), and whether it is still open.</summary>
    private sealed record PayeeRequest(Guid? OrderItemId, Guid? RefundId, Guid? RefundIbanId, bool IsOpen);

    /// <summary>A record carrying an IBAN of its own: a cancellation, a late delivery termination or a withdrawal.</summary>
    private sealed record PayeeRecord(Guid Id, Guid OrderItemId, Guid RefundIbanId);

    /// <summary>The order's IBANs and their links as one read loaded them; each subject is decided here without a further query.</summary>
    private sealed class PayeeBook(List<IbanOnFile> file, List<PayeeRequest> requests, List<PayeeRecord> records)
    {
        public RefundPayee PayeeOf(RefundPayeeSubject subject)
        {
            Guid? ibanId = Decide(subject);
            return new RefundPayee(subject, ibanId, ibanId is { } id ? file.First(f => f.Id == id).Iban : null);
        }

        /// <summary>
        /// The one rule, in its order (02 7.4.5; D-907): the record's own IBAN; else the refund's own requests - one open waits (VAL-A912), else the IBAN one
        /// brought -; else the item's requests (those naming no refund) - one open waits ("havale gerçekleşmedi" on the item: the item is back to "IBAN
        /// bekleniyor"), else the IBAN one of them brought, else the IBAN of one of the item's records -; else the first IBAN on the order's file that is not another refund's own (one
        /// its own request brought: the late payment's, a failed card refund's transfer path). A waiting refund is no IBAN's payee. Several candidates of a step:
        /// the latest entered on file (02 7.4.5: "alanda duran IBAN" - an item's earlier request IBAN gives way to the one the field asked again for after a later
        /// firm cancellation, removal or "havale gerçekleşmedi" (K-690); the IBAN's life lets the earlier one go, as it does while the item waits: RefundIbanUses.IsInUse).
        /// </summary>
        private Guid? Decide(RefundPayeeSubject subject)
        {
            if (subject.RecordId is { } recordId && NewestOnFile(records.Where(r => r.Id == recordId).Select(r => (Guid?)r.RefundIbanId)) is { } recordIban)
            {
                return recordIban;
            }

            if (subject.RefundId is { } refundId)
            {
                List<PayeeRequest> own = [.. requests.Where(r => r.RefundId == refundId)];
                if (own.Any(r => r.IsOpen))
                {
                    return null;
                }

                if (NewestOnFile(own.Select(r => r.RefundIbanId)) is { } ownIban)
                {
                    return ownIban;
                }
            }

            if (subject.OrderItemId is { } itemId)
            {
                List<PayeeRequest> item = [.. requests.Where(r => r.RefundId == null && r.OrderItemId == itemId)];
                if (item.Any(r => r.IsOpen))
                {
                    return null;
                }

                // The IBAN the customer entered on the item's request comes before the item's records' (02 7.4.5: the IBAN asked again after "havale
                // gerçekleşmedi" is the one paid to), the same order the waiting above follows.
                if (NewestOnFile(item.Select(r => r.RefundIbanId)) is { } requestIban)
                {
                    return requestIban;
                }

                if (NewestOnFile(records.Where(r => r.OrderItemId == itemId).Select(r => (Guid?)r.RefundIbanId)) is { } recordsIban)
                {
                    return recordsIban;
                }
            }

            return file.FirstOrDefault(f => !requests.Any(r => r.RefundIbanId == f.Id && r.RefundId is { } owner && owner != subject.RefundId))?.Id;
        }

        /// <summary>The latest entered of the candidates still on the order's file (its order: entry, then id) - the one standing in the field (02 7.4.5).</summary>
        private Guid? NewestOnFile(IEnumerable<Guid?> ibanIds)
        {
            HashSet<Guid> ids = [.. ibanIds.OfType<Guid>()];
            return file.LastOrDefault(f => ids.Contains(f.Id))?.Id;
        }
    }
}
