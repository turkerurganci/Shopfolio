using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The order page's channel of the defect claim (02 Z-18, 7.5.2; 03 1.7.2, 1.9.1): a legal period (02 Z-18), not a product constant, so it lives with
/// the rule that applies it (09 3.5 item 1). On a physical item the channel opens when the order is shipped (or the item carries the delivery mark); on a digital or
/// service item when the item carries the delivery mark (the payment approval, the "completed" mark). The period runs from the delivery date: while a physical item
/// has no delivery date the period does not run (the risk is the firm's, 02 7.5.2). Its last day is the delivery date two years on, read in Turkey time. When the
/// channel closes, no new claim is opened and a resolved claim is not reopened - by the customer or by an administrator (03 1.9.1).
/// </summary>
internal static class DefectClaimWindow
{
    /// <summary>Z-18: the claim's period, in years from the delivery date.</summary>
    public const int Years = 2;

    private static readonly OrderStatus[] ShippedOrLater = [OrderStatus.Shipped, OrderStatus.DeliveryFailed, OrderStatus.Delivered];

    /// <summary>The last day of the period; null while the item has no delivery date (the period does not run).</summary>
    public static DateOnly? LastDay(ItemRecordSubject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        DateOnly? delivered = subject.DeliveryDate ?? (subject.DeliveryMarkedAt is { } marked ? TurkeyTime.DateOf(marked) : null);
        return delivered?.AddYears(Years);
    }

    /// <summary>Whether the channel is open at <paramref name="now"/>: open by the item's type and not past the period's last day.</summary>
    public static bool IsOpen(ItemRecordSubject subject, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(subject);
        bool opened = subject.DeliveryMarkedAt is not null
            || (subject.ProductType == ProductType.Physical && ShippedOrLater.Contains(subject.OrderStatus));
        if (!opened)
        {
            return false;
        }

        return LastDay(subject) is not { } last || TurkeyTime.DateOf(now) <= last;
    }
}

/// <summary>
/// The withdrawal exemptions (06 5.3.7; 02 7.3.4): the conditional ones - the hygiene-sensitive good and the sealed media - hold only while the protective packaging is
/// unopened, so only their returned goods may be rejected at the receipt step (02 7.4.9; K-656). The others exclude the withdrawal outright.
/// </summary>
internal static class WithdrawalExemptions
{
    public static bool IsConditional(WithdrawalExemptionReason? reason) =>
        reason is WithdrawalExemptionReason.HygieneSensitive or WithdrawalExemptionReason.SealedMedia;
}
