using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The writers of the item records (03 1.7.1, 1.7.4, 1.9.1; T320): each call adds its own row - two declarations are two rows, two partial arrivals two receipts
/// (K-790, K-794) - after the unit rule (the quantity is at least one and at most the quantity open to the operation). The writers join the caller's transaction
/// and require the order's row lock in it (05 7.3.2; <see cref="LockSequence"/>): the caller is the transition unit, which raises the order's version and saves.
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<DefectClaimInfo?> FindDefectClaimAsync(Guid orderItemId, CancellationToken ct)
    {
        DefectClaim? claim = await _db.DefectClaims.AsNoTracking().SingleOrDefaultAsync(c => c.OrderItemId == orderItemId, ct);
        return claim is null ? null : Info(claim);
    }

    public async Task<ItemRecordWriteResult> OpenDefectClaimAsync(DefectClaimOpening opening, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(opening);
        ItemRecordSubject subject = opening.Subject;
        RequireOrderLock(subject);

        // One claim per item (K-676): an open claim takes no new one, a resolved one is reopened - never a second row.
        if (await _db.DefectClaims.AnyAsync(c => c.OrderItemId == subject.OrderItemId, ct))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.ClaimExists);
        }

        if (!DefectClaimWindow.IsOpen(subject, opening.At))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.ChannelClosed);
        }

        if (!ItemQuantities.IsSelectable(opening.Quantity, await OpenQuantityAsync(subject, ct)))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.QuantityExceedsOpen);
        }

        ReturnAddress? address = subject.ProductType == ProductType.Physical ? await FirmReturnAddressAsync(ct) : null;
        DefectClaim claim = DefectClaim.Create(subject.OrderItemId, opening.Quantity, opening.Description, address, opening.At);
        _db.DefectClaims.Add(claim);
        return new ItemRecordWriteResult(null, claim.Id, Info(claim));
    }

    public async Task<ItemRecordWriteResult> ChangeDefectClaimStatusAsync(DefectClaimStatusChange change, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(change);
        ItemRecordSubject subject = change.Subject;
        RequireOrderLock(subject);

        DefectClaim? claim = await _db.DefectClaims.SingleOrDefaultAsync(c => c.OrderItemId == subject.OrderItemId, ct);
        if (claim is null)
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.ClaimNotFound);
        }

        bool reopening = change.Status == DefectClaimStatus.Open && claim.DefectClaimStatus == DefectClaimStatus.Resolved;
        if (reopening && !DefectClaimWindow.IsOpen(subject, change.At))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.ChannelClosed);
        }

        // The customer may raise the defective units up to the units open to a claim (04 5.19.3): the quantity minus the closed units.
        if (reopening && change.Actor == ActorKind.Buyer && change.Quantity is { } raised && raised > claim.Quantity
            && raised > await OpenQuantityAsync(subject, ct))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.QuantityExceedsOpen);
        }

        ItemRecordRefusal? refusal = DefectClaimStatusWriter.TryChange(claim, change.Status, change.Actor, change.Quantity, change.At);
        return refusal is { } refused ? ItemRecordWriteResult.Rejected(refused) : new ItemRecordWriteResult(null, claim.Id, Info(claim));
    }

    public async Task<ItemRecordWriteResult> AddWithdrawalAsync(WithdrawalDeclaration declaration, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(declaration);
        ItemRecordSubject subject = declaration.Subject;
        RequireOrderLock(subject);
        if (!ItemQuantities.IsSelectable(declaration.Quantity, await OpenQuantityAsync(subject, ct)))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.QuantityExceedsOpen);
        }

        ReturnAddress? address = subject.ProductType == ProductType.Physical ? await FirmReturnAddressAsync(ct) : null;
        ItemAmounts amounts = new(declaration.ProductAmount, declaration.CouponShare, declaration.VatAmount);
        Withdrawal withdrawal = declaration.Admin is { } admin
            ? Withdrawal.ByAdmin(subject.OrderItemId, declaration.Quantity, admin.AdminId, admin.AdminName, declaration.DeclarationDate, address, amounts, declaration.RefundIbanId, declaration.At)
            : Withdrawal.ByBuyer(subject.OrderItemId, declaration.Quantity, declaration.DeclarationDate, address, amounts, declaration.RefundIbanId, declaration.At);
        withdrawal.IsDeliveredBeforeDeclaration = declaration.IsDeliveredBeforeDeclaration;
        _db.Withdrawals.Add(withdrawal);
        return new ItemRecordWriteResult(null, withdrawal.Id, null);
    }

    public async Task<ItemRecordWriteResult> AddReturnReceiptAsync(ReturnReceiptEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        RequireOrderLock(entry.Subject);
        ItemRecordRefusal? refusal = await ReceiptRefusalAsync(entry.Subject, entry.WithdrawalId, entry.Quantity, entry.ArrivalDate, ct);
        if (refusal is { } refused)
        {
            return ItemRecordWriteResult.Rejected(refused);
        }

        Return receipt = Return.ForWithdrawal(entry.Subject.OrderItemId, entry.WithdrawalId, entry.Quantity, entry.ArrivalDate, entry.Admin.AdminId, entry.Admin.AdminName, entry.At);
        _db.Returns.Add(receipt);
        return new ItemRecordWriteResult(null, receipt.Id, null);
    }

    public async Task<ItemRecordWriteResult> AddReturnRejectionAsync(ReturnRejectionEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        RequireOrderLock(entry.Subject);

        // Only the conditional exemption's goods are rejected - the protective packaging was opened (02 7.4.9; K-656).
        if (!WithdrawalExemptions.IsConditional(entry.Subject.WithdrawalExemptionReason))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.NotConditionalExemption);
        }

        ItemRecordRefusal? refusal = await ReceiptRefusalAsync(entry.Subject, entry.WithdrawalId, entry.Quantity, entry.ArrivalDate, ct);
        if (refusal is { } refused)
        {
            return ItemRecordWriteResult.Rejected(refused);
        }

        ReturnRejection rejection = ReturnRejection.Create(
            entry.Subject.OrderItemId, entry.WithdrawalId, entry.Quantity, entry.ArrivalDate, entry.Admin.AdminId, entry.Admin.AdminName, entry.At);
        _db.ReturnRejections.Add(rejection);
        return new ItemRecordWriteResult(null, rejection.Id, null);
    }

    public async Task<IReadOnlyDictionary<Guid, ItemMarks>> ItemMarksAsync(IReadOnlyCollection<ItemDelivery> items, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(items);
        Dictionary<Guid, DateOnly?> deliveries = items.GroupBy(i => i.OrderItemId).ToDictionary(g => g.Key, g => g.First().DeliveryDate);
        Guid[] ids = [.. deliveries.Keys];
        Dictionary<Guid, ItemMarks> marks = [];
        if (ids.Length == 0)
        {
            return marks;
        }

        var withdrawals = await _db.Withdrawals.AsNoTracking().Where(w => ids.Contains(w.OrderItemId))
            .Select(w => new { w.Id, w.OrderItemId, w.Quantity, w.DeclarationDate, w.IsDeliveredBeforeDeclaration }).ToListAsync(ct);
        Guid[] withdrawalIds = [.. withdrawals.Select(w => w.Id)];
        Dictionary<Guid, int> received = await _db.Returns.AsNoTracking().Where(r => r.WithdrawalId != null && withdrawalIds.Contains(r.WithdrawalId.Value))
            .GroupBy(r => r.WithdrawalId!.Value).Select(g => new { g.Key, Sum = g.Sum(r => r.Quantity) }).ToDictionaryAsync(g => g.Key, g => g.Sum, ct);
        Dictionary<Guid, int> rejected = await _db.ReturnRejections.AsNoTracking().Where(r => withdrawalIds.Contains(r.WithdrawalId))
            .GroupBy(r => r.WithdrawalId).Select(g => new { g.Key, Sum = g.Sum(r => r.Quantity) }).ToDictionaryAsync(g => g.Key, g => g.Sum, ct);
        Dictionary<Guid, int> notReturned = await _db.NotReturnedClosures.AsNoTracking().Where(n => withdrawalIds.Contains(n.WithdrawalId) && n.ReopenedAt == null)
            .GroupBy(n => n.WithdrawalId).Select(g => new { g.Key, Sum = g.Sum(n => n.Quantity) }).ToDictionaryAsync(g => g.Key, g => g.Sum, ct);
        HashSet<Guid> awaitingIban = [.. await _db.RefundIbanRequests.AsNoTracking()
            .Where(r => r.OrderItemId != null && ids.Contains(r.OrderItemId.Value) && r.FulfilledAt == null && r.ClosedAt == null)
            .Select(r => r.OrderItemId!.Value).Distinct().ToListAsync(ct)];

        foreach (Guid id in ids)
        {
            DateOnly? delivered = deliveries[id];
            bool awaitingGoods = withdrawals.Any(w => w.OrderItemId == id
                && IsAfterDelivery(w.DeclarationDate, w.IsDeliveredBeforeDeclaration, delivered)
                && ItemQuantities.AwaitingReceipt(w.Quantity, received.GetValueOrDefault(w.Id), rejected.GetValueOrDefault(w.Id), notReturned.GetValueOrDefault(w.Id)) > 0);
            marks[id] = new ItemMarks(awaitingGoods, awaitingIban.Contains(id));
        }

        return marks;
    }

    /// <summary>A withdrawal declared after the delivery starts the return (03 1.7.1.5, 1.7.3.5): a later day, or the same day when the firm answered that the delivery came first.</summary>
    private static bool IsAfterDelivery(DateOnly declared, bool? deliveredFirst, DateOnly? delivered) =>
        delivered is { } day && (declared > day || (declared == day && deliveredFirst == true));

    private static DefectClaimInfo Info(DefectClaim claim)
    {
        ItemReturnAddress? address = claim is { ReturnRecipientName: { } name, ReturnProvinceId: { } province, ReturnDistrict: { } district, ReturnAddressLine: { } line }
            ? new ItemReturnAddress(name, province, district, line, claim.ReturnPhone)
            : null;
        return new DefectClaimInfo(claim.Id, claim.OrderItemId, claim.DefectClaimStatus, claim.Quantity, claim.CreatedAt, address);
    }

    /// <summary>The writers run inside the caller's transaction and after the order's row lock (05 7.3.2): anything else is a programming error.</summary>
    private void RequireOrderLock(ItemRecordSubject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        if (_db.Database.CurrentTransaction is null || !LockSequence.LockedOrders(_db).Contains(subject.OrderId))
        {
            throw new InvalidOperationException("An item record is written inside the caller's transaction, under the order's row lock (05 7.3.2; 09 6 item 3).");
        }
    }

    /// <summary>The item's open quantity (03 1.7.4): its quantity minus the units its records closed, the caller's unsaved ones included.</summary>
    private async Task<int> OpenQuantityAsync(ItemRecordSubject subject, CancellationToken ct)
    {
        IReadOnlyDictionary<Guid, int> closed = await ClosedQuantitiesAsync([subject.OrderItemId], ct);
        return ItemQuantities.Open(subject.ItemQuantity, closed.GetValueOrDefault(subject.OrderItemId));
    }

    /// <summary>
    /// The receipt step's rule (03 1.7.1.6, 1.7.1.7; K-794): the withdrawal is the item's, the units are at most those still on the way back, and the arrival date is
    /// not before the declaration (06 4.26; the caller checks it is not in the future).
    /// </summary>
    private async Task<ItemRecordRefusal?> ReceiptRefusalAsync(ItemRecordSubject subject, Guid withdrawalId, int quantity, DateOnly arrivalDate, CancellationToken ct)
    {
        Withdrawal? withdrawal = await _db.Withdrawals.AsNoTracking().SingleOrDefaultAsync(w => w.Id == withdrawalId && w.OrderItemId == subject.OrderItemId, ct);
        if (withdrawal is null)
        {
            return ItemRecordRefusal.WithdrawalNotAwaiting;
        }

        int received = await _db.Returns.Where(r => r.WithdrawalId == withdrawalId).SumAsync(r => r.Quantity, ct)
            + AddedSum<Return>(r => r.WithdrawalId == withdrawalId, r => r.Quantity);
        int rejected = await _db.ReturnRejections.Where(r => r.WithdrawalId == withdrawalId).SumAsync(r => r.Quantity, ct)
            + AddedSum<ReturnRejection>(r => r.WithdrawalId == withdrawalId, r => r.Quantity);
        int notReturned = await _db.NotReturnedClosures.Where(n => n.WithdrawalId == withdrawalId && n.ReopenedAt == null).SumAsync(n => n.Quantity, ct);
        int awaiting = ItemQuantities.AwaitingReceipt(withdrawal.Quantity, received, rejected, notReturned);
        if (awaiting == 0)
        {
            return ItemRecordRefusal.WithdrawalNotAwaiting;
        }

        if (!ItemQuantities.IsSelectable(quantity, awaiting))
        {
            return ItemRecordRefusal.QuantityExceedsOpen;
        }

        return arrivalDate < withdrawal.DeclarationDate ? ItemRecordRefusal.ArrivalDateOutOfRange : null;
    }

    private int AddedSum<TRecord>(Func<TRecord, bool> filter, Func<TRecord, int> quantityOf)
        where TRecord : class
    {
        return _db.ChangeTracker.Entries<TRecord>().Where(e => e.State == EntityState.Added && filter(e.Entity)).Sum(e => quantityOf(e.Entity));
    }

    /// <summary>The firm's return address as a value (06 4.25, 4.30; K-817): copied at the moment of the record, so a later change never reaches it.</summary>
    private async Task<ReturnAddress?> FirmReturnAddressAsync(CancellationToken ct)
    {
        ReturnAddressSection section = await _settings.GetReturnAddressAsync(ct);
        return section.Address is { } a ? new ReturnAddress(a.RecipientName, a.ProvinceId, a.District, a.AddressLine, a.Phone) : null;
    }
}
