using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
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

        if (!AfterSalesPeriodRules.IsDefectClaimChannelOpen(subject.ProductType, subject.OrderStatus, subject.DeliveryDate, opening.At))
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
        if (reopening && !AfterSalesPeriodRules.IsDefectClaimChannelOpen(subject.ProductType, subject.OrderStatus, subject.DeliveryDate, change.At))
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
        int open = await OpenQuantityAsync(subject, ct);
        if (!ItemQuantities.IsSelectable(declaration.Quantity, open))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.QuantityExceedsOpen);
        }

        WithdrawalIban iban = declaration.Iban;
        RefundIban? onFile = iban.UseOnFile
            ? await _db.RefundIbans.Where(i => i.OrderId == subject.OrderId).OrderByDescending(i => i.EnteredAt).ThenBy(i => i.Id).FirstOrDefaultAsync(ct)
            : null;
        if (iban.UseOnFile && onFile is null)
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.IbanMissing);
        }

        // The amounts set aside from the item's frozen values against what its earlier records set aside; the last open units take the rest (02 7.1.7; K-1104).
        (decimal couponSetAside, decimal vatSetAside) = await SetAsideAsync(subject.OrderItemId, ct);
        ItemAmounts amounts = ItemShares.SetAside(
            declaration.Frozen, subject.ItemQuantity, declaration.Quantity, declaration.Quantity == open, couponSetAside, vatSetAside);

        // The firm's current return address is copied as a value on a physical item (02 7.3.5; K-665): a later change of the setting never reaches it.
        ReturnAddress? address = subject.ProductType == ProductType.Physical ? await FirmReturnAddressAsync(ct) : null;
        RefundIban? refundIban = onFile;
        if (iban.Entered is { } entered)
        {
            refundIban = RefundIban.Create(subject.OrderId, entered, declaration.At);
            _db.RefundIbans.Add(refundIban);
        }

        Withdrawal withdrawal = declaration.Admin is { } admin
            ? Withdrawal.ByAdmin(subject.OrderItemId, declaration.Quantity, admin.AdminId, admin.AdminName, declaration.DeclarationDate, address, amounts, refundIban?.Id, declaration.At)
            : Withdrawal.ByBuyer(subject.OrderItemId, declaration.Quantity, declaration.DeclarationDate, address, amounts, refundIban?.Id, declaration.At);
        withdrawal.IsDeliveredBeforeDeclaration = declaration.IsDeliveredBeforeDeclaration;
        _db.Withdrawals.Add(withdrawal);

        // Z-38: after the delivery the IBAN waits for the goods; it is deleted when the sending period and P-43 have run out unless the goods came (D-667). A later
        // deletion day already on the IBAN (another declaration of it) is kept (K-1110).
        if (refundIban is not null
            && AfterSalesPeriodRules.IsAfterDelivery(subject.ProductType, subject.DeliveryDate, declaration.DeclarationDate, declaration.IsDeliveredBeforeDeclaration))
        {
            DateTimeOffset deletionAt = AfterSalesPeriodRules.ReturnIbanDeletionAt(declaration.DeclarationDate);
            if (refundIban.ScheduledDeletionAt is not { } scheduled || scheduled < deletionAt)
            {
                refundIban.ScheduledDeletionAt = deletionAt;
            }
        }

        // The item has at most one open IBAN request - the order page opens one IBAN field for it (06 4.29; 02 7.4.5).
        RefundIbanRequest? openRequest = await _db.RefundIbanRequests
            .Where(r => r.OrderItemId == subject.OrderItemId && r.FulfilledAt == null && r.ClosedAt == null)
            .FirstOrDefaultAsync(ct);

        // A record without an IBAN on the transfer line opens the item's IBAN request (02 10.4.10; K-581): the customer enters it on the order page. While the
        // item's request is still open, a further record uses it - the field is already open (06 4.29: a second event uses the open request).
        if (iban.Request)
        {
            ItemRecordAdmin requester = declaration.Admin ?? throw new InvalidOperationException("Only the administrator's record of a notice opens an IBAN request (02 10.4.10).");
            if (openRequest is null)
            {
                _db.RefundIbanRequests.Add(RefundIbanRequest.ForItem(
                    subject.OrderItemId, RefundIbanRequestReason.NoticeWithoutIban, null, requester.AdminId, requester.AdminName, declaration.At));
            }
        }
        else if (refundIban is not null && openRequest is not null)
        {
            // A declaration that brings the item an IBAN - entered, carried from the order's e-mail or on file - fulfils its open request (06 4.29; 02 7.4.5).
            openRequest.FulfilledAt = declaration.At;
            openRequest.RefundIbanId = refundIban.Id;
        }

        ItemReturnAddress? copied = address is { } a ? new ItemReturnAddress(a.RecipientName, a.ProvinceId, a.District, a.AddressLine, a.Phone) : null;
        return new ItemRecordWriteResult(null, withdrawal.Id, null, copied);
    }

    public async Task<WithdrawalNotes> WithdrawalNotesAsync(ProductType productType, bool shippedNotDelivered, WithdrawalExemptionReason? exemption, DateOnly? deliveryDate, CancellationToken ct)
    {
        // Z-16's start for a declaration sent now (02 7.4.1): the customer's own declaration never carries the firm's same-day answer, so the delivery day counts as before it.
        DateOnly today = TurkeyTime.DateOf(_time.GetUtcNow());
        bool fromArrival = AfterSalesPeriodRules.IsAfterDelivery(productType, deliveryDate, today, null);
        // A physical item's screen shows the current return address and the sending period; in transit it first says the carrier brings the goods back (K-838). A service
        // item's shows neither (K-530). The packaging condition is the conditional exemption's (K-509), the one rule of WithdrawalExemptions.
        bool physical = productType == ProductType.Physical;
        ReturnAddress? address = physical ? await FirmReturnAddressAsync(ct) : null;
        return new WithdrawalNotes(
            address is { } a ? new ItemReturnAddress(a.RecipientName, a.ProvinceId, a.District, a.AddressLine, a.Phone) : null,
            physical ? AfterSalesPeriodRules.ReturnShipmentPeriodDays : null,
            physical ? PeriodCounting.LastDay(CountedPeriod.ReturnShipmentPeriod, today, AfterSalesPeriodRules.ReturnShipmentPeriodDays, BusinessCalendar.Product) : null,
            physical && shippedNotDelivered,
            WithdrawalExemptions.IsConditional(exemption) ? exemption : null,
            AfterSalesPeriodRules.RefundPeriodDays,
            fromArrival);
    }

    public async Task<ItemRecordWriteResult> AddReturnReceiptAsync(ReturnReceiptEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ItemRecordSubject subject = entry.Subject;
        RequireOrderLock(subject);

        // The units of an open "mal dönmedi" closure may arrive too: the receipt reopens the closure (02 10.4.9; 06 4.26, 4.28; K-496).
        ItemRecordRefusal? refusal = await ReceiptRefusalAsync(subject, entry.WithdrawalId, entry.Quantity, entry.ArrivalDate, includeClosed: true, ct);
        if (refusal is { } refused)
        {
            return ItemRecordWriteResult.Rejected(refused);
        }

        Withdrawal withdrawal = await _db.Withdrawals.AsNoTracking().SingleAsync(w => w.Id == entry.WithdrawalId, ct);
        WithdrawalProgress progress = await WithdrawalProgressAsync(withdrawal, ct);
        Return receipt = Return.ForWithdrawal(subject.OrderItemId, entry.WithdrawalId, entry.Quantity, entry.ArrivalDate, entry.Admin.AdminId, entry.Admin.AdminName, entry.At);
        _db.Returns.Add(receipt);

        await ReopenClosuresBeyondAwaitingAsync(entry.WithdrawalId, entry.Quantity, progress.Awaiting, entry.At, ct);

        bool ibanRequested = entry.IsTransferLine && await RequestIbanAgainAsync(entry, ct);
        return new ItemRecordWriteResult(null, receipt.Id, null, null, ibanRequested);
    }

    public async Task<ItemRecordWriteResult> AddNotReturnedClosureAsync(NotReturnedClosureEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ItemRecordSubject subject = entry.Subject;
        RequireOrderLock(subject);
        Withdrawal? withdrawal = subject.ProductType == ProductType.Physical
            ? await _db.Withdrawals.AsNoTracking().SingleOrDefaultAsync(w => w.Id == entry.WithdrawalId && w.OrderItemId == subject.OrderItemId, ct)
            : null;
        int awaiting = withdrawal is null ? 0 : (await WithdrawalProgressAsync(withdrawal, ct)).Awaiting;
        if (withdrawal is null || awaiting == 0)
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.WithdrawalNotAwaiting);
        }

        // Only a withdrawal after the delivery whose sending period (Z-42) ran out; running out closes nothing by itself (02 7.4.4, 10.4.9; K-494).
        bool afterDelivery = AfterSalesPeriodRules.IsAfterDelivery(subject.ProductType, subject.DeliveryDate, withdrawal.DeclarationDate, withdrawal.IsDeliveredBeforeDeclaration);
        if (!AfterSalesPeriodRules.IsNotReturnedClosureOpen(afterDelivery, awaiting, withdrawal.DeclarationDate, entry.At))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.SendingPeriodNotOver);
        }

        if (!ItemQuantities.IsSelectable(entry.Quantity, awaiting))
        {
            return ItemRecordWriteResult.Rejected(ItemRecordRefusal.QuantityExceedsOpen);
        }

        NotReturnedClosure closure = NotReturnedClosure.Create(subject.OrderItemId, withdrawal.Id, entry.Quantity, entry.Admin.AdminId, entry.Admin.AdminName, entry.At);
        _db.NotReturnedClosures.Add(closure);
        return new ItemRecordWriteResult(null, closure.Id, null);
    }

    public async Task<ReturnStockResult> MarkReturnStockAddedAsync(ReturnStockEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        RequireOrderLock(entry.Subject);
        if (entry.Subject.ProductType != ProductType.Physical)
        {
            return new ReturnStockResult(ItemRecordRefusal.ReceiptNotFound, 0);
        }

        Return? receipt = await _db.Returns.SingleOrDefaultAsync(r => r.Id == entry.ReturnId && r.OrderItemId == entry.Subject.OrderItemId, ct);
        if (receipt is null)
        {
            return new ReturnStockResult(ItemRecordRefusal.ReceiptNotFound, 0);
        }

        // Once per receipt and as many units as were received (02 7.4.7; 06 4.26; K-215): the step never adds twice.
        if (receipt.StockAddedAt is not null)
        {
            return new ReturnStockResult(ItemRecordRefusal.StockNotAddable, 0);
        }

        receipt.StockAddedAt = entry.At;
        return new ReturnStockResult(null, receipt.Quantity);
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

        // The rejection is a part of the receipt step (02 7.4.7): the units of an open "mal dönmedi" closure may arrive and be rejected too, and the rejection
        // reopens the closure as the receipt does (06 4.26 "ret de"; D-915).
        ItemRecordRefusal? refusal = await ReceiptRefusalAsync(entry.Subject, entry.WithdrawalId, entry.Quantity, entry.ArrivalDate, includeClosed: true, ct);
        if (refusal is { } refused)
        {
            return ItemRecordWriteResult.Rejected(refused);
        }

        Withdrawal withdrawal = await _db.Withdrawals.AsNoTracking().SingleAsync(w => w.Id == entry.WithdrawalId, ct);
        WithdrawalProgress progress = await WithdrawalProgressAsync(withdrawal, ct);
        ReturnRejection rejection = ReturnRejection.Create(
            entry.Subject.OrderItemId, entry.WithdrawalId, entry.Quantity, entry.ArrivalDate, entry.Admin.AdminId, entry.Admin.AdminName, entry.At);
        _db.ReturnRejections.Add(rejection);
        await ReopenClosuresBeyondAwaitingAsync(entry.WithdrawalId, entry.Quantity, progress.Awaiting, entry.At, ct);
        return new ItemRecordWriteResult(null, rejection.Id, null);
    }

    public async Task<IReadOnlyDictionary<Guid, ItemMarks>> ItemMarksAsync(IReadOnlyCollection<ItemDelivery> items, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(items);
        Dictionary<Guid, ItemDelivery> deliveries = items.GroupBy(i => i.OrderItemId).ToDictionary(g => g.Key, g => g.First());
        Guid[] ids = [.. deliveries.Keys];
        Dictionary<Guid, ItemMarks> marks = [];
        if (ids.Length == 0)
        {
            return marks;
        }

        IReadOnlyList<WithdrawalStanding> withdrawals = await WithdrawalStandingsAsync(ids, ct);
        HashSet<Guid> awaitingIban = [.. await _db.RefundIbanRequests.AsNoTracking()
            .Where(r => r.OrderItemId != null && ids.Contains(r.OrderItemId.Value) && r.FulfilledAt == null && r.ClosedAt == null)
            .Select(r => r.OrderItemId!.Value).Distinct().ToListAsync(ct)]; // non-null: the Where above keeps only requests with an item

        foreach (Guid id in ids)
        {
            ItemDelivery item = deliveries[id];
            bool awaitingGoods = withdrawals.Any(w => w.OrderItemId == id
                && AfterSalesPeriodRules.IsAfterDelivery(item.ProductType, item.DeliveryDate, w.DeclarationDate, w.IsDeliveredBeforeDeclaration)
                && ItemQuantities.AwaitingReceipt(w.Quantity, w.Received, w.Rejected, w.NotReturned) > 0);
            marks[id] = new ItemMarks(awaitingGoods, awaitingIban.Contains(id));
        }

        return marks;
    }

    private static DefectClaimInfo Info(DefectClaim claim)
    {
        ItemReturnAddress? address = claim is { ReturnRecipientName: { } name, ReturnProvinceId: { } province, ReturnDistrict: { } district, ReturnAddressLine: { } line }
            ? new ItemReturnAddress(name, province, district, line, claim.ReturnPhone)
            : null;
        return new DefectClaimInfo(claim.Id, claim.OrderItemId, claim.DefectClaimStatus, claim.Quantity, claim.CreatedAt, address, claim.Description);
    }

    /// <summary>
    /// Units of a receipt or a rejection beyond those on the way back are closed ones: the closure opens as a whole (06 4.26, 4.28 ReopenedAt; K-1055; D-915) - the
    /// units still not come back await their receipt again and may be closed by a new row.
    /// </summary>
    private async Task ReopenClosuresBeyondAwaitingAsync(Guid withdrawalId, int quantity, int awaiting, DateTimeOffset at, CancellationToken ct)
    {
        if (quantity <= awaiting)
        {
            return;
        }

        foreach (NotReturnedClosure closure in await _db.NotReturnedClosures.Where(n => n.WithdrawalId == withdrawalId && n.ReopenedAt == null).ToListAsync(ct))
        {
            closure.ReopenedAt = at;
        }
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

    /// <summary>
    /// The item's quantity open to an operation (03 1.7.4; 02 7.1.6; 06 2.7.2): its quantity minus the units its records closed, the caller's unsaved ones included, and
    /// minus the units rescinded on a defect claim - they do not close the item but are no other operation's subject (K-1077).
    /// </summary>
    private async Task<int> OpenQuantityAsync(ItemRecordSubject subject, CancellationToken ct)
    {
        IReadOnlyDictionary<Guid, int> closed = await ClosedQuantitiesAsync([subject.OrderItemId], ct);
        IReadOnlyDictionary<Guid, RescindedShares> rescinded = await _payment.RescindedSharesAsync([subject.OrderItemId], ct);
        int rescindedUnits = rescinded.TryGetValue(subject.OrderItemId, out RescindedShares? shares) ? shares.Quantity : 0;
        return ItemQuantities.Open(subject.ItemQuantity, closed.GetValueOrDefault(subject.OrderItemId) + rescindedUnits);
    }

    /// <summary>
    /// The receipt step's rule (03 1.7.1.6, 1.7.1.7; 02 7.4.7; K-794): a physical item's withdrawal, the units at most those still on the way back - with
    /// <paramref name="includeClosed"/> also those of an open "mal dönmedi" closure, which the receipt reopens -, and the arrival date not before the item's entered
    /// delivery date; a parcel withdrawn before its delivery has no delivery date and no lower bound. The date may be before the declaration (K-554: the refund
    /// period then runs from the declaration); the caller refuses a date in the future.
    /// </summary>
    private async Task<ItemRecordRefusal?> ReceiptRefusalAsync(
        ItemRecordSubject subject, Guid withdrawalId, int quantity, DateOnly arrivalDate, bool includeClosed, CancellationToken ct)
    {
        Withdrawal? withdrawal = subject.ProductType == ProductType.Physical
            ? await _db.Withdrawals.AsNoTracking().SingleOrDefaultAsync(w => w.Id == withdrawalId && w.OrderItemId == subject.OrderItemId, ct)
            : null;
        if (withdrawal is null)
        {
            return ItemRecordRefusal.WithdrawalNotAwaiting;
        }

        WithdrawalProgress progress = await WithdrawalProgressAsync(withdrawal, ct);
        int takeable = progress.Awaiting + (includeClosed ? progress.NotReturned : 0);
        if (takeable == 0)
        {
            return ItemRecordRefusal.WithdrawalNotAwaiting;
        }

        if (!ItemQuantities.IsSelectable(quantity, takeable))
        {
            return ItemRecordRefusal.QuantityExceedsOpen;
        }

        return subject.DeliveryDate is { } delivered && arrivalDate < delivered ? ItemRecordRefusal.ArrivalDateOutOfRange : null;
    }

    /// <summary>A withdrawal's units still on the way back and those of its open "mal dönmedi" closures, the caller's unsaved records included (K-794).</summary>
    private async Task<WithdrawalProgress> WithdrawalProgressAsync(Withdrawal withdrawal, CancellationToken ct)
    {
        Guid id = withdrawal.Id;
        int received = await _db.Returns.Where(r => r.WithdrawalId == id).SumAsync(r => r.Quantity, ct)
            + AddedSum<Return>(r => r.WithdrawalId == id, r => r.Quantity);
        int rejected = await _db.ReturnRejections.Where(r => r.WithdrawalId == id).SumAsync(r => r.Quantity, ct)
            + AddedSum<ReturnRejection>(r => r.WithdrawalId == id, r => r.Quantity);
        int notReturned = await _db.NotReturnedClosures.Where(n => n.WithdrawalId == id && n.ReopenedAt == null).SumAsync(n => n.Quantity, ct)
            + AddedSum<NotReturnedClosure>(n => n.WithdrawalId == id && n.ReopenedAt == null, n => n.Quantity);
        return new WithdrawalProgress(ItemQuantities.AwaitingReceipt(withdrawal.Quantity, received, rejected, notReturned), notReturned);
    }

    /// <summary>
    /// The IBAN asked for again at a receipt (02 7.4.5; 03 1.7.1.6; K-497, K-498): on the transfer line, after the delivery - the receipt starts the refund's period -,
    /// when the declaration's IBAN is gone (deleted by Z-38, the closure or the refund line) and no IBAN fulfilled on the item still stands. The customer enters it on
    /// the order page; the firm never enters it (02 8.4.2). True when a new request was opened - the caller sends B-14; an open request is used as it is.
    /// </summary>
    private async Task<bool> RequestIbanAgainAsync(ReturnReceiptEntry entry, CancellationToken ct)
    {
        ItemRecordSubject subject = entry.Subject;
        Withdrawal withdrawal = await _db.Withdrawals.AsNoTracking().SingleAsync(w => w.Id == entry.WithdrawalId, ct);
        bool afterDelivery = AfterSalesPeriodRules.IsAfterDelivery(subject.ProductType, subject.DeliveryDate, withdrawal.DeclarationDate, withdrawal.IsDeliveredBeforeDeclaration);
        if (!afterDelivery || withdrawal.RefundIbanId is not null
            || await _db.RefundIbanRequests.AnyAsync(r => r.OrderItemId == subject.OrderItemId && r.RefundIbanId != null, ct))
        {
            return false;
        }

        RefundIbanRequestOpened opened = await OpenRefundIbanRequestAsync(
            new RefundIbanRequestEntry(subject.OrderItemId, null, RefundIbanRequestReason.ReturnAfterIbanDeleted, entry.Admin.AdminId, entry.Admin.AdminName, entry.At), ct);
        return opened.IsNew;
    }

    /// <summary>A withdrawal's units on the way back and those of its open closures.</summary>
    private sealed record WithdrawalProgress(int Awaiting, int NotReturned);

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
