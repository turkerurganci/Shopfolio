using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog;

/// <summary>The coupon as the rules read it: the stored values of <see cref="Coupon"/>, taken in one plain read.</summary>
internal sealed record CouponTerms(
    Guid Id,
    string Code,
    decimal? Percentage,
    decimal? Amount,
    decimal? MinimumBasketAmount,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalUsageLimit,
    int UsedCount,
    int ReservedCount);

/// <summary>
/// The coupon's rules as pure functions (02 3.10.1-3.10.7, 5.11.2-5.11.4; 11 T338 criteria 1-9): what a new coupon may hold, the verdict on a code for a basket with the
/// share of every item, and when a closed order gives its right back. The shares are set item by item (02 3.10.3): a percentage coupon makes every item that percentage
/// smaller and rounds its share in the item (<see cref="Money.Round"/>); a fixed amount is spread over the items in proportion to their amounts, the parts rounded down and
/// the kuruş left over on the largest item (<see cref="Money.Allocate"/>). The VAT is separated after the share (<see cref="ItemPricing.Price"/>; 02 3.8.3). Rounding is
/// <see cref="Money"/>'s alone (09 14.1); the field rules are the panel's own attributes (09 3.4 item 1). The counters are not here: they are written only by
/// <see cref="StockCounters"/> (09 10.2 item 4).
/// </summary>
internal static class CouponRules
{
    private const decimal Hundred = 100m;

    /// <summary>The value of a percentage coupon (04 7.2.6.3, 7.3.2.9): above zero, under one hundred, at most two decimals.</summary>
    private static readonly InvalidPercentageAttribute PercentageField = new();

    /// <summary>An amount the coupon form takes (04 7.3.2.8): at most two decimals, at most the money type's limit; the coupon's own value above zero.</summary>
    private static readonly InvalidAmountAttribute AmountField = new() { AllowZero = true };

    /// <summary>
    /// Every field of a new coupon that cannot be saved (02 3.10.2, 3.10.4, 3.10.5; 04 7.2.6, 9.7.8; 07 UC-122): a blank code; a percentage that is not above zero and under
    /// one hundred (<c>InvalidPercentage</c> - %100 included, 07 6.2.6); a fixed amount of zero or less, or a minimum basket amount missing or not larger than it
    /// (<c>CouponValueInvalid</c>); an amount that is no kuruş amount or passes the money type (<c>InvalidAmount</c>); an end before the start (<c>InvalidDateRange</c>);
    /// a usage count under the used and reserved rights - a new coupon has none, so under zero (<c>CouponUsageBelowUsed</c>). The code's uniqueness needs the table and is
    /// <see cref="Coupons"/>'.
    /// </summary>
    public static IReadOnlyList<CouponProblem> CheckDefinition(CouponDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        List<CouponProblem> problems = [];
        if (string.IsNullOrWhiteSpace(definition.Code))
        {
            problems.Add(new CouponProblem(nameof(CouponDefinition.Code), ErrorCode.FieldRequired));
        }

        if (definition.Kind == CouponKind.Percentage)
        {
            if (definition.Value is not { } percentage || !PercentageField.Allows(percentage))
            {
                problems.Add(new CouponProblem(nameof(CouponDefinition.Value), ErrorCode.InvalidPercentage));
            }

            if (definition.MinimumBasketAmount is { } minimum && !AmountField.Allows(minimum))
            {
                problems.Add(new CouponProblem(nameof(CouponDefinition.MinimumBasketAmount), ErrorCode.InvalidAmount));
            }
        }
        else if (definition.Kind == CouponKind.Amount)
        {
            CheckFixedAmount(definition, problems);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(definition), definition.Kind, "A coupon is a percentage or a fixed amount (02 3.10.2).");
        }

        if (definition.EndDate < definition.StartDate)
        {
            problems.Add(new CouponProblem(nameof(CouponDefinition.EndDate), ErrorCode.InvalidDateRange));
        }

        if (definition.TotalUsageLimit < 0)
        {
            problems.Add(new CouponProblem(nameof(CouponDefinition.TotalUsageLimit), ErrorCode.CouponUsageBelowUsed));
        }

        return problems;
    }

    /// <summary>
    /// The verdict on a coupon for a basket at <paramref name="now"/> (02 3.10.4, 3.10.5, 5.11.2; 6.2.12). Not accepted: a coupon outside its date range - the start day from
    /// 00:00, the end day included as a whole, in Turkey time (02 4.1.4; Z-22) -, a coupon whose used and reserved rights fill its total, a fixed amount equal to or above
    /// the basket total, and a coupon whose shares, once rounded, take the total to zero. A valid coupon on a basket under its minimum amount is its own outcome. The basket
    /// total is the items' amounts before the coupon - only the items that go into the order (K-130). An empty basket or a line that is no item is a programming error.
    /// </summary>
    public static CouponApplication Evaluate(CouponTerms terms, IReadOnlyList<CouponLine> lines, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(terms);
        IReadOnlyList<decimal> amounts = AmountsOf(lines);
        decimal basketTotal = amounts.Sum();

        if (!TurkeyTime.IsWithinRange(now, terms.StartDate, terms.EndDate) || terms.UsedCount + terms.ReservedCount >= terms.TotalUsageLimit)
        {
            return CouponApplication.Rejected(basketTotal);
        }

        if (terms.MinimumBasketAmount is { } minimum && basketTotal < minimum)
        {
            return CouponApplication.BelowMinimum(basketTotal, minimum);
        }

        IReadOnlyList<decimal> shares;
        if (terms.Percentage is { } percentage)
        {
            shares = PercentageShares(percentage, amounts);
        }
        else if (terms.Amount is { } amount)
        {
            if (amount >= basketTotal)
            {
                return CouponApplication.Rejected(basketTotal);
            }

            shares = AmountShares(amount, amounts);
        }
        else
        {
            throw new InvalidOperationException($"Coupon {terms.Id} has neither a percentage nor an amount (06 4.13).");
        }

        if (basketTotal - shares.Sum() <= 0)
        {
            return CouponApplication.Rejected(basketTotal);
        }

        List<ItemAmounts> items = [.. lines.Select((line, i) => ItemPricing.Price(new ItemPriceInput(line.UnitPrice, line.Quantity, shares[i], line.VatRate)))];
        return CouponApplication.Accepted(terms.Id, terms.Code, basketTotal, items);
    }

    /// <summary>
    /// The verdict on a code no coupon holds - or a blank one: not accepted, with the one message every other refusal gets (K-845), so a guessed code learns nothing. The
    /// lines are checked as for a known code and the basket total is still given.
    /// </summary>
    public static CouponApplication RejectUnknown(IReadOnlyList<CouponLine> lines) => CouponApplication.Rejected(AmountsOf(lines).Sum());

    /// <summary>
    /// The shares of a percentage coupon (02 3.10.3): every item becomes that percentage smaller, its share rounded to the kuruş in the item - the nearest kuruş, a half
    /// up (02 3.8.3). The percentage is under one hundred, so a share never passes its item's amount.
    /// </summary>
    public static IReadOnlyList<decimal> PercentageShares(decimal percentage, IReadOnlyList<decimal> amounts)
    {
        ArgumentNullException.ThrowIfNull(amounts);
        if (!PercentageField.Allows(percentage))
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), percentage, "A coupon's percentage is above zero and under one hundred.");
        }

        return [.. amounts.Select(a => Money.Round(a * percentage / Hundred))];
    }

    /// <summary>
    /// The shares of a fixed-amount coupon (02 3.10.3): the amount spread over the items in proportion to their amounts, each part rounded down to the kuruş and the kuruş
    /// left over written to the largest item - the first of them when two are equal (<see cref="Money.Allocate"/>). The amount is under the basket total (02 3.10.4). An item's
    /// share cannot pass its own amount - its payable amount and its VAT are never below zero (02 3.8.3) -, so when the left-over kuruş would take the largest item past its
    /// amount, the kuruş that do not fit go to the next largest items in the same order; the shares still add up to the amount (D-482).
    /// </summary>
    public static IReadOnlyList<decimal> AmountShares(decimal amount, IReadOnlyList<decimal> amounts)
    {
        ArgumentNullException.ThrowIfNull(amounts);
        if (amount <= 0 || amount >= amounts.Sum())
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "A fixed coupon is above zero and under the basket total.");
        }

        decimal[] shares = [.. Money.Allocate(amount, amounts)];
        decimal overflow = 0m;
        for (int i = 0; i < shares.Length; i++)
        {
            if (shares[i] > amounts[i])
            {
                overflow += shares[i] - amounts[i];
                shares[i] = amounts[i];
            }
        }

        foreach (int i in Enumerable.Range(0, shares.Length).OrderByDescending(i => amounts[i]).ThenBy(i => i))
        {
            if (overflow == 0)
            {
                break;
            }

            decimal taken = Math.Min(amounts[i] - shares[i], overflow);
            shares[i] += taken;
            overflow -= taken;
        }

        return shares;
    }

    /// <summary>
    /// Whether the closing of an order's units gives the coupon right back (02 3.10.6, 5.11.3, 7.2.6, 7.4.8; K-201, K-202, K-216, K-791): only when the whole order is
    /// closed - every unit of every item cancelled, removed, terminated for late delivery, withdrawn or returned. A partial cancellation or return keeps the right used.
    /// The caller is the cancellation and the return (T367, T373); the right itself moves through <c>ICatalogModule.ReturnAsync</c>.
    /// </summary>
    public static bool ReturnsRight(IReadOnlyList<ItemClosure> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
        {
            throw new ArgumentException("An order has at least one item.", nameof(items));
        }

        foreach (ItemClosure item in items)
        {
            if (item.Quantity < 1 || item.ClosedQuantity < 0 || item.ClosedQuantity > item.Quantity)
            {
                throw new ArgumentOutOfRangeException(nameof(items), item, "An item has one unit or more and closes from none up to all of them.");
            }
        }

        return items.All(i => i.ClosedQuantity == i.Quantity);
    }

    // A fixed coupon (04 7.2.6.3, 7.2.6.4; 07 CouponValueInvalid): the amount above zero, the minimum basket amount required and larger than it; either value that is no
    // kuruş amount, or passes the money type, is the amount field's own message.
    private static void CheckFixedAmount(CouponDefinition definition, List<CouponProblem> problems)
    {
        decimal? amount = definition.Value;
        decimal? minimum = definition.MinimumBasketAmount;
        if (amount is not { } value || value <= 0)
        {
            problems.Add(new CouponProblem(nameof(CouponDefinition.Value), ErrorCode.CouponValueInvalid));
        }
        else if (!AmountField.Allows(value))
        {
            problems.Add(new CouponProblem(nameof(CouponDefinition.Value), ErrorCode.InvalidAmount));
        }

        if (minimum is { } floor && !AmountField.Allows(floor))
        {
            problems.Add(new CouponProblem(nameof(CouponDefinition.MinimumBasketAmount), ErrorCode.InvalidAmount));
        }
        else if (minimum is null || (amount is { } given && minimum <= given))
        {
            problems.Add(new CouponProblem(nameof(CouponDefinition.MinimumBasketAmount), ErrorCode.CouponValueInvalid));
        }
    }

    // The amounts of the items (02 3.8.3 step 1): the unit price times the quantity, not rounded; the unit price and the quantity are checked by ItemPricing's own rules.
    private static List<decimal> AmountsOf(IReadOnlyList<CouponLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count == 0)
        {
            throw new ArgumentException("The payment step has at least one item.", nameof(lines));
        }

        return [.. lines.Select(l => ItemPricing.Price(new ItemPriceInput(l.UnitPrice, l.Quantity, 0m, l.VatRate)).Amount)];
    }
}
