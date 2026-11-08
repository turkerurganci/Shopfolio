using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>Which of the two rules of 02 3.9.3 sets a variant's reference price.</summary>
internal enum ReferenceRule
{
    /// <summary>Physical and digital goods: the lowest price applied in the window before the discount starts (02 3.9.3, 3.9.4).</summary>
    LowestInWindow,

    /// <summary>A service, and a product whose withdrawal exemption reason is "perishable": the price applied right before the discount (02 3.9.3, 7.3.4).</summary>
    ImmediatelyBefore,
}

/// <summary>
/// The reference price as a pure function (02 3.9.3, 3.9.4, 4.2 Z-21, 12.1.6; 06 4.6 <c>FirstOnSaleAt</c>, 4.11; Fiyat Etiketi Yönetmeliği m.11/1 as changed by
/// RG 11/10/2025-33044; 11 T337 criteria 5-7). The system computes it from the variant's own price history; the administrator never enters it and nothing stores it
/// (06 2.7.2). On physical and digital goods it is the lowest price applied to the variant in the <see cref="LookbackDays"/> days before the discount's start - every
/// price applied in the window counts, the stretches the variant was off sale included (K-1023 (2)), and a discounted price is an applied price too (consecutive
/// campaigns lower the reference step by step). The window never reaches back before the variant first went on sale (the first moment it and its product were
/// Published together): a variant with a shorter history takes the lowest price since then, and the other variants of the product never count (02 3.9.4; K-563). On
/// a service and on a product exempt from withdrawal as perishable it is the price applied right before the discount; a product exempt for an expiry date follows
/// the goods' rule (02 3.9.3; K-1483, K-1493).
/// </summary>
internal static class ReferencePriceRules
{
    /// <summary>
    /// Z-21 - the window of the reference price on goods, in days back from the discount's start (02 4.2 Z-21; Fiyat Etiketi Yönetmeliği m.11/1). A legal period, not
    /// a product constant: it is not in the parameter inventory (02 11 "Envantere girmeyenler"; 09 3.5 item 1) and lives with its rule.
    /// </summary>
    public const int LookbackDays = 10;

    /// <summary>The rule of a product (02 3.9.3): the immediately preceding price on a service and on a perishable product, the window's lowest on every other product.</summary>
    public static ReferenceRule RuleOf(ProductType type, WithdrawalExemptionReason? exemptionReason)
    {
        return type == ProductType.Service || exemptionReason == WithdrawalExemptionReason.Perishable
            ? ReferenceRule.ImmediatelyBefore
            : ReferenceRule.LowestInWindow;
    }

    /// <summary>
    /// The reference price of one variant for a discount starting at <paramref name="discountStart"/>. <paramref name="history"/> is the variant's own price history
    /// (rows from the start on are ignored); <paramref name="price"/> is the variant's own price, which stands in for a variant with no row before the start.
    /// A variant that never went on sale before the start has no window: the price that applied right before the start is its reference.
    /// </summary>
    public static decimal Reference(
        ReferenceRule rule,
        DateTimeOffset discountStart,
        DateTimeOffset? firstOnSaleAt,
        IEnumerable<PricePoint> history,
        decimal price)
    {
        ArgumentNullException.ThrowIfNull(history);
        List<PricePoint> before = [.. history.Where(p => p.StartedAt < discountStart).OrderBy(p => p.StartedAt)];
        decimal immediatelyBefore = before.Count == 0 ? price : before[^1].Price;
        if (rule == ReferenceRule.ImmediatelyBefore || firstOnSaleAt is not { } onSale || onSale >= discountStart)
        {
            return immediatelyBefore;
        }

        DateTimeOffset windowStart = discountStart.AddDays(-LookbackDays);
        DateTimeOffset from = onSale > windowStart ? onSale : windowStart;

        // The price in force when the window opens applied in it too; so did every price that started inside it.
        List<decimal> applied = [.. before.Where(p => p.StartedAt > from).Select(p => p.Price)];
        int inForce = before.FindLastIndex(p => p.StartedAt <= from);
        if (inForce >= 0)
        {
            applied.Add(before[inForce].Price);
        }

        return applied.Count == 0 ? immediatelyBefore : applied.Min();
    }
}
