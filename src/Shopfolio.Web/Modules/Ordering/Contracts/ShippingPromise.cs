using System.Globalization;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The delivery promise as pure rules (02 3.20.4-3.20.6; 11 T350 criteria 5-7): the promise is the shipping period, counted in business days, and covers physical items only. A product's
/// own period overrides the firm default and a variant takes its product's; the order's promise is the longest of its physical items'; the period starts at the moment the payment is
/// confirmed. Nothing here reads a clock, a table or a setting: the caller passes what it read.
/// </summary>
public static class ShippingPromise
{
    /// <summary>
    /// The shipping period of one item (02 3.20.5): only a physical item has one - a digital or a service item makes no promise and gives null. The product's own period wins over
    /// <paramref name="companyDefault"/> when it is set; zero is a valid period (same day) and does not fall back to the default.
    /// </summary>
    public static int? Effective(ProductType type, int? productPeriod, int companyDefault)
    {
        return type == ProductType.Physical ? productPeriod ?? companyDefault : null;
    }

    /// <summary>
    /// The promise of an order (02 3.20.3, 3.20.5): the longest of the physical items' periods, so the whole parcel waits for its slowest item; null when the order has no physical
    /// item (a digital or service order promises no shipping). Items that are null are not physical and are skipped.
    /// </summary>
    public static int? OfOrder(IEnumerable<int?> itemPeriods)
    {
        ArgumentNullException.ThrowIfNull(itemPeriods);
        int? longest = null;
        foreach (int? period in itemPeriods)
        {
            if (period is { } days && (longest is null || days > longest))
            {
                longest = days;
            }
        }

        return longest;
    }

    /// <summary>The storefront's promise text (04 5.4.5; 02 3.20.4): the period in business days, as "N iş günü içinde kargoya verilir".</summary>
    public static string Text(int businessDays)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(businessDays);
        return string.Create(CultureInfo.InvariantCulture, $"{businessDays} iş günü içinde kargoya verilir");
    }

    /// <summary>
    /// The moment the promise runs out (02 3.20.6): the period counted in business days from the day the payment was confirmed - that day is not counted - up to and including its last
    /// day, so the promise ends at 00:00 Turkey time of the day after (<see cref="PeriodCounting"/>). The product's holiday calendar is the counting's.
    /// </summary>
    public static DateTimeOffset PromiseEnds(DateTimeOffset paymentConfirmedAt, int businessDays)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(businessDays);
        return PeriodCounting.EndOf(CountedPeriod.ShippingPeriod, paymentConfirmedAt, businessDays, BusinessCalendar.Product);
    }
}
