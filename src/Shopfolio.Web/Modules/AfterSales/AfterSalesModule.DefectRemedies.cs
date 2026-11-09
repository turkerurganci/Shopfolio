using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The money side of a defect claim's remedy (T374; 02 7.5.3; 03 2.9.4, 8.4.10): the system records and forwards the claim and never runs the remedy - repair, exchange,
/// price reduction and rescission are the firm's, outside the product. When the remedy needs money, the rescission pays back the units taken back with the amounts of
/// 02 7.1.7 and the price reduction pays an amount back without the item coming back. These reads check the claim and split the amounts; the refund the payment module
/// starts is the record (06 4.33: the rescission's units, coupon share and VAT), so a rescinded unit leaves the open quantity through it (K-1077).
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<IReadOnlyDictionary<Guid, int>> UnavailableQuantitiesAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Dictionary<Guid, int> units = new(await ClosedQuantitiesAsync(orderItemIds, ct));
        foreach ((Guid id, RescindedShares rescinded) in await _payment.RescindedSharesAsync(orderItemIds, ct))
        {
            if (rescinded.Quantity > 0)
            {
                units[id] = units.GetValueOrDefault(id) + rescinded.Quantity;
            }
        }

        return units;
    }

    public async Task<DefectRemedyCheck> CheckDefectClaimRescissionAsync(DefectClaimRescissionEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ItemRecordSubject subject = entry.Subject;
        RequireOrderLock(subject);
        DefectClaim? claim = await OpenClaimAsync(subject, ct);
        if (claim is null)
        {
            return DefectRemedyCheck.Rejected(await _db.DefectClaims.AnyAsync(c => c.OrderItemId == subject.OrderItemId, ct)
                ? ItemRecordRefusal.StatusNotAllowed
                : ItemRecordRefusal.ClaimNotFound);
        }

        // The units taken back are the claim's defective ones (02 7.5.3) - the ones rescinded before are not taken twice - and units open to an operation (02 7.1.6).
        IReadOnlyDictionary<Guid, RescindedShares> rescinded = await _payment.RescindedSharesAsync([subject.OrderItemId], ct);
        int rescindedUnits = rescinded.TryGetValue(subject.OrderItemId, out RescindedShares? shares) ? shares.Quantity : 0;
        int open = await OpenQuantityAsync(subject, ct);
        int claimable = Math.Max(0, claim.Quantity - rescindedUnits);
        if (!ItemQuantities.IsSelectable(entry.Quantity, Math.Min(open, claimable)))
        {
            return DefectRemedyCheck.Rejected(ItemRecordRefusal.QuantityExceedsOpen);
        }

        (decimal couponSetAside, decimal vatSetAside) = await SetAsideAsync(subject.OrderItemId, ct);
        ItemAmounts amounts = ItemShares.SetAside(entry.Frozen, subject.ItemQuantity, entry.Quantity, entry.Quantity == open, couponSetAside, vatSetAside);
        return new DefectRemedyCheck(null, claim.Id, new RefundAmount(entry.Quantity, amounts.ProductAmount, amounts.CouponShare, amounts.VatAmount));
    }

    public async Task<DefectRemedyCheck> CheckDefectClaimPriceReductionAsync(ItemRecordSubject subject, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(subject);
        RequireOrderLock(subject);
        DefectClaim? claim = await OpenClaimAsync(subject, ct);
        if (claim is not null)
        {
            return new DefectRemedyCheck(null, claim.Id, null);
        }

        return DefectRemedyCheck.Rejected(await _db.DefectClaims.AnyAsync(c => c.OrderItemId == subject.OrderItemId, ct)
            ? ItemRecordRefusal.StatusNotAllowed
            : ItemRecordRefusal.ClaimNotFound);
    }

    public async Task<bool> RequestDefectRemedyIbanAsync(DefectRemedyIbanRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ItemRecordSubject subject = request.Subject;
        RequireOrderLock(subject);

        // The IBAN on the order's file pays the refund (02 7.4.5); otherwise the order page opens the item's one IBAN field, and a second remedy uses the open request (06 4.29).
        if (await _db.RefundIbans.AnyAsync(i => i.OrderId == subject.OrderId, ct)
            || await _db.RefundIbanRequests.AnyAsync(r => r.OrderItemId == subject.OrderItemId && r.FulfilledAt == null && r.ClosedAt == null, ct))
        {
            return false;
        }

        _db.RefundIbanRequests.Add(RefundIbanRequest.ForItem(
            subject.OrderItemId, RefundIbanRequestReason.AdminRequest, null, request.Admin.AdminId, request.Admin.AdminName, request.At));
        return true;
    }

    /// <summary>The item's claim while it is open: the remedy is the answer to an open claim; a resolved one is reopened first (03 1.9.1).</summary>
    private async Task<DefectClaim?> OpenClaimAsync(ItemRecordSubject subject, CancellationToken ct)
    {
        return await _db.DefectClaims.AsNoTracking()
            .SingleOrDefaultAsync(c => c.OrderItemId == subject.OrderItemId && c.DefectClaimStatus == DefectClaimStatus.Open, ct);
    }
}
