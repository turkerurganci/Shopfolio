using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog;

/// <summary>The definition of a product's discount as the rules read it (06 4.12): the rate in percent and the two dates of its range.</summary>
internal sealed record DiscountTerms(decimal Percentage, DateOnly StartDate, DateOnly EndDate);

/// <summary>A price that applied to a variant from a moment on: a row of its price history, written or about to be written (06 4.11).</summary>
internal readonly record struct PricePoint(DateTimeOffset StartedAt, decimal Price);

/// <summary>What keeps a discount from being saved (02 3.9.1, 3.9.5; 04 7.2.4.18, 7.3.2.9, 7.3.2.12, 9.5.17; 02 6.6.1, 6.6.13).</summary>
internal enum DiscountProblem
{
    None,

    /// <summary>The rate is not a discount's percentage: zero or below, one hundred or above, or more than two decimals (04 7.3.2.9; K-600).</summary>
    InvalidPercentage,

    /// <summary>The end date is before the start date (04 7.3.2.12).</summary>
    EndBeforeStart,

    /// <summary>A variant's discounted price, rounded to the kuruş, would be 0,00 TL (02 3.9.1; K-600).</summary>
    DiscountedPriceZero,

    /// <summary>A variant's discounted price is not below its reference price (02 3.9.5; K-68).</summary>
    NotBelowReference,
}

/// <summary>
/// The rules of a product's dated percentage discount as pure functions (02 3.9.1, 3.9.2, 3.9.5, 4.1.4, 5.11.1; 06 2.5.2, 4.11, 4.12; 11 T337 criteria 1-3, 8). The
/// discount is set on the product and comes down to every variant on its own price: the discounted price is the price less the rate, rounded to the kuruş once
/// (09 14.1 item 2). The range starts at 00.00 Turkey time of the start day and holds the end day as a whole - it ends at 00.00 of the day after (the one helper
/// <see cref="TurkeyTime"/>); its state is read from the dates. A discount is refused when a variant's rounded discounted price would be zero or would not be
/// below the variant's reference price. The rate's rule is the panel's percentage field rule (<see cref="InvalidPercentageAttribute"/>, without zero): one home.
/// </summary>
internal static class DiscountRules
{
    /// <summary>The panel's discount rate field (04 7.3.2.9; zero is not a discount).</summary>
    private static readonly InvalidPercentageAttribute PercentageField = new();

    /// <summary>Whether <paramref name="percentage"/> is a discount's rate: above zero, below one hundred, at most two decimals (04 7.3.2.9; 06 4.12; K-600).</summary>
    public static bool IsValidPercentage(decimal percentage) => PercentageField.Allows(percentage);

    /// <summary>
    /// The discounted price of a variant (02 3.9.1): <paramref name="price"/> less <paramref name="percentage"/> percent, one amount rounded to the kuruş (an exact half
    /// kuruş up). A rate outside the rule is a programming error.
    /// </summary>
    public static decimal DiscountedPrice(decimal price, decimal percentage)
    {
        if (!IsValidPercentage(percentage))
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), percentage, "A discount's rate is above zero and below one hundred, with at most two decimals.");
        }

        return Money.Round(price * (100m - percentage) / 100m);
    }

    /// <summary>The moment the discount starts (02 4.1.4; 06 2.5.2; K-1023): 00.00 Turkey time of the start day; it is also the moment the reference looks back from (Z-21).</summary>
    public static DateTimeOffset StartsAt(DiscountTerms terms) => TurkeyTime.StartOf(terms.StartDate);

    /// <summary>The moment the discount ends (02 4.1.4; K-1559): 00.00 Turkey time of the day after the end date, so the end day is included as a whole.</summary>
    public static DateTimeOffset EndsAt(DiscountTerms terms) => TurkeyTime.EndOfRange(terms.EndDate);

    /// <summary>The discount's state at <paramref name="at"/> (04 9.5.8; 06 4.12): not started before its start, running inside its range, ended from its end on.</summary>
    public static DiscountState StateAt(DiscountTerms terms, DateTimeOffset at)
    {
        if (at < StartsAt(terms))
        {
            return DiscountState.NotStarted;
        }

        return TurkeyTime.IsWithinRange(at, terms.StartDate, terms.EndDate) ? DiscountState.Running : DiscountState.Ended;
    }

    /// <summary>The price that applies to a variant at <paramref name="at"/> (02 3.9.1, 5.11.1): the discounted price while its product's discount runs, else its own price.</summary>
    public static decimal AppliedPrice(decimal price, DiscountTerms? terms, DateTimeOffset at)
    {
        return terms is not null && StateAt(terms, at) == DiscountState.Running ? DiscountedPrice(price, terms.Percentage) : price;
    }

    /// <summary>The check of the discount's own fields: its rate (04 7.3.2.9) and its range (04 7.3.2.12). A date in the future is allowed (04 7.2.4.18).</summary>
    public static DiscountProblem CheckTerms(DiscountTerms terms)
    {
        ArgumentNullException.ThrowIfNull(terms);
        if (!IsValidPercentage(terms.Percentage))
        {
            return DiscountProblem.InvalidPercentage;
        }

        return terms.EndDate < terms.StartDate ? DiscountProblem.EndBeforeStart : DiscountProblem.None;
    }

    /// <summary>
    /// The check of one variant against a valid rate (02 3.9.1, 3.9.5): a rounded discounted price of zero is refused first (K-600), then a discounted price that is
    /// not below <paramref name="referencePrice"/> (K-68).
    /// </summary>
    public static DiscountProblem CheckVariant(decimal price, decimal referencePrice, decimal percentage)
    {
        decimal discounted = DiscountedPrice(price, percentage);
        if (discounted == 0m)
        {
            return DiscountProblem.DiscountedPriceZero;
        }

        return discounted < referencePrice ? DiscountProblem.None : DiscountProblem.NotBelowReference;
    }

    /// <summary>
    /// The declaration of a running discount on a variant (02 3.9.2, 3.9.3, 12.1.6): its rounded discounted price, its reference price, the rate and the range's two
    /// dates, which are shown wherever the declaration is (K-545).
    /// </summary>
    public static DiscountLine LineOf(DiscountTerms terms, decimal price, decimal referencePrice)
    {
        ArgumentNullException.ThrowIfNull(terms);
        return new DiscountLine(DiscountedPrice(price, terms.Percentage), referencePrice, terms.Percentage, terms.StartDate, terms.EndDate);
    }

    /// <summary>Whether a price, entered while a discount has not ended, would be discounted to 0,00 TL (02 3.9.1: "İndirim yürürken ... bir fiyat da girilemez"; K-600).</summary>
    public static bool ReachesZero(decimal price, decimal percentage) => DiscountedPrice(price, percentage) == 0m;

    /// <summary>
    /// The price history rows a discount's range owes a variant by <paramref name="now"/> (02 5.11.1; 06 2.5.2, 4.11 "Satırın yazıldığı anlar"; K-1023 (4), K-1041 (2)):
    /// its start - the discounted price - and its end - the variant's own price -, each at the range's own bound even when the system writes it late. A bound is owed
    /// when it has come, when the discount was defined before it (a range entered in the past did not apply then, and is not written back) and when the variant's last
    /// row is older than it - the last row tells whether the bound is already written (K-1041 (2)). <paramref name="lastRowAt"/> is null for a variant without rows.
    /// </summary>
    public static IReadOnlyList<PricePoint> OwedBounds(DiscountTerms terms, DateTimeOffset definedAt, decimal price, DateTimeOffset? lastRowAt, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(terms);
        List<PricePoint> owed = [];
        DateTimeOffset? last = lastRowAt;
        foreach (PricePoint bound in (ReadOnlySpan<PricePoint>)[new(StartsAt(terms), DiscountedPrice(price, terms.Percentage)), new(EndsAt(terms), price)])
        {
            if (definedAt < bound.StartedAt && bound.StartedAt <= now && (last is null || last < bound.StartedAt))
            {
                owed.Add(bound);
                last = bound.StartedAt;
            }
        }

        return owed;
    }
}
