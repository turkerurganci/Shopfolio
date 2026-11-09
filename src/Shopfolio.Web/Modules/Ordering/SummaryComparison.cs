using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>What changed between the confirmed summary and the current one (02 3.17.2). A result code, never stored.</summary>
internal enum SummaryChangeKind
{
    /// <summary>An item of the confirmed summary no longer enters the order.</summary>
    ItemLeft,

    /// <summary>An item enters the order that the confirmed summary did not hold.</summary>
    ItemAdded,

    /// <summary>The item's quantity differs.</summary>
    Quantity,

    /// <summary>The item's unit price that applies differs - a price change or a discount that began or ended.</summary>
    UnitPrice,

    /// <summary>The item's breakdown differs: the list price, the discount rate, the coupon's share, the VAT rate or the VAT.</summary>
    Breakdown,

    /// <summary>The shipping fee differs - the fee, the free shipping threshold or the delivery provinces changed.</summary>
    ShippingFee,

    /// <summary>The total differs, or the coupon or the minimum order amount no longer holds.</summary>
    Total,

    /// <summary>An item could not be reserved; its quantity is never told (02 3.17.2; K-150).</summary>
    ItemNotReserved,
}

/// <summary>One change line (02 3.17.2): the item's name and the confirmed and the current value, so the page can say "Lambanın fiyatı 1.000 TL'den 1.200 TL'ye değişti".</summary>
internal sealed record SummaryChange(SummaryChangeKind Kind, string? ProductName, decimal? Before, decimal? After);

/// <summary>
/// The comparison of 02 3.17.2: every difference of the amount to pay, its breakdown (VAT, discount, coupon share, shipping) or the items stops the order; the direction of a
/// change does not matter. Both directions are compared - an item that left and an item that came both count. Pure: no database, no clock.
/// </summary>
internal static class SummaryComparison
{
    public static IReadOnlyList<SummaryChange> Compare(ConfirmationSummary confirmed, ConfirmationSummary current)
    {
        ArgumentNullException.ThrowIfNull(confirmed);
        ArgumentNullException.ThrowIfNull(current);
        List<SummaryChange> changes = [];
        int shared = Math.Min(confirmed.Lines.Count, current.Lines.Count);
        for (int i = 0; i < shared; i++)
        {
            ConfirmationSummaryLine before = confirmed.Lines[i];
            ConfirmationSummaryLine after = current.Lines[i];
            if (!IsSameItem(before, after))
            {
                changes.Add(new SummaryChange(SummaryChangeKind.ItemLeft, before.ProductName, before.Quantity, null));
                changes.Add(new SummaryChange(SummaryChangeKind.ItemAdded, after.ProductName, null, after.Quantity));
                continue;
            }

            if (before.Quantity != after.Quantity)
            {
                changes.Add(new SummaryChange(SummaryChangeKind.Quantity, after.ProductName, before.Quantity, after.Quantity));
            }

            if (before.UnitPrice != after.UnitPrice)
            {
                changes.Add(new SummaryChange(SummaryChangeKind.UnitPrice, after.ProductName, before.UnitPrice, after.UnitPrice));
            }

            if (before.ListPrice != after.ListPrice || before.DiscountPercentage != after.DiscountPercentage || before.CouponShare != after.CouponShare
                || before.VatRate != after.VatRate || before.Vat != after.Vat || before.Payable != after.Payable)
            {
                changes.Add(new SummaryChange(SummaryChangeKind.Breakdown, after.ProductName, before.Payable, after.Payable));
            }
        }

        changes.AddRange(confirmed.Lines.Skip(shared).Select(l => new SummaryChange(SummaryChangeKind.ItemLeft, l.ProductName, l.Quantity, null)));
        changes.AddRange(current.Lines.Skip(shared).Select(l => new SummaryChange(SummaryChangeKind.ItemAdded, l.ProductName, null, l.Quantity)));
        if (confirmed.ShippingFee != current.ShippingFee)
        {
            changes.Add(new SummaryChange(SummaryChangeKind.ShippingFee, null, confirmed.ShippingFee, current.ShippingFee));
        }

        if (confirmed.Total != current.Total)
        {
            changes.Add(new SummaryChange(SummaryChangeKind.Total, null, confirmed.Total, current.Total));
        }

        return changes;
    }

    private static bool IsSameItem(ConfirmationSummaryLine before, ConfirmationSummaryLine after) =>
        before.ProductName == after.ProductName
        && before.FirstOptionValue == after.FirstOptionValue
        && before.SecondOptionValue == after.SecondOptionValue
        && before.ProductType == after.ProductType;
}
