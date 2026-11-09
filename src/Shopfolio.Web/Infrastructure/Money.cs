namespace Shopfolio.Infrastructure;

/// <summary>
/// The only place where an amount is rounded to the kuruş (09 14.1 item 1; 05 7.2.6; 02 3.8.3). The direction is written out:
/// .NET's default <c>Math.Round(x, 2)</c> takes an exact half to the even digit (12,345 to 12,34), the rule wants 12,35. Money is
/// <see cref="decimal"/>; no floating point type takes part (05 7.2.5). The fourth function of 09 14.1 item 1, the share of units
/// (02 7.1.7), came with the first operation that computes it (T372, the withdrawal declaration).
/// </summary>
public static class Money
{
    /// <summary>
    /// The largest amount the money type holds (06 2.5.1: <c>numeric(12,2)</c>, at most 9.999.999.999,99 TL), derived from the type's precision and scale.
    /// It is the upper limit of every amount field (04 7.3.2.8; 07 2.9.4; K-1214): a larger value is a field message, never a value the database rounds.
    /// </summary>
    public static readonly decimal MaxAmount = Limit(DataModel.ModelBuilderExtensions.MoneyType);

    /// <summary>
    /// The largest percentage a rate field takes (04 7.3.2.9; 06 2.5.1): under one hundred, at most two decimals (the scale of the percent type) -
    /// one hundred less one step, 99,99.
    /// </summary>
    public static readonly decimal MaxPercentage = 100m - Step(DataModel.ModelBuilderExtensions.PercentType.Scale);

    /// <summary>
    /// One amount to the nearest kuruş, an exact half kuruş up (12,345 to 12,35; 0,005 to 0,01; 0,004 to 0,00). Amounts are never
    /// negative where the product rounds them (02 3.8.3), so a negative input is a programming error and throws (09 14.1 item 1). The
    /// check compares the value: a zero with the minus sign bit (<c>-0.00m</c>) is not negative and rounds to a plain zero.
    /// </summary>
    public static decimal Round(decimal amount)
    {
        ThrowIfBelowZero(amount, nameof(amount));

        // The value is zero or above; Abs only drops the sign bit a negative zero carries (-0.00 to 0.00).
        return Math.Abs(Math.Round(amount, 2, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// The VAT inside a VAT-included amount (09 14.1 item 3; 02 3.8.3): <c>Round(gross x rate / (100 + rate))</c>, one amount rounded to the
    /// kuruş. The rate is a percentage with at most two decimals - 20 is 20% (K-1014) -, from zero up to <see cref="MaxPercentage"/>. The net
    /// amount is the gross less this VAT and is not rounded again. 120 at 20: 20,00; 100 at 20: 16,67; 110 at 10: 10,00; any amount at 0: 0,00.
    /// </summary>
    public static decimal VatOf(decimal grossAmount, decimal ratePercent)
    {
        ThrowIfBelowZero(grossAmount, nameof(grossAmount));
        if (ratePercent < 0 || ratePercent > MaxPercentage)
        {
            throw new ArgumentOutOfRangeException(nameof(ratePercent), ratePercent, "A VAT rate is a percentage from zero up to the largest rate.");
        }

        return Round(grossAmount * ratePercent / (100m + ratePercent));
    }

    /// <summary>
    /// Splits <paramref name="total"/> in proportion to <paramref name="weights"/> (02 3.8.3): each part is the proportional amount rounded
    /// DOWN to the kuruş, and the kuruş left over by the split goes to the part of the largest weight - the first of them when two weights
    /// are equal (09 14.1 item 1). The parts add up to the total exactly. The total is a kuruş amount (at most two decimals, not negative);
    /// every weight is greater than zero.
    /// </summary>
    public static IReadOnlyList<decimal> Allocate(decimal total, IReadOnlyList<decimal> weights)
    {
        ArgumentNullException.ThrowIfNull(weights);
        ThrowIfBelowZero(total, nameof(total));
        if (total * 100 % 1 != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(total), total, "The total has more than two decimals.");
        }

        if (weights.Count == 0)
        {
            throw new ArgumentException("There is no part to allocate to.", nameof(weights));
        }

        decimal weightSum = 0m;
        int largest = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            if (weights[i] <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weights), weights[i], "A weight is zero or negative.");
            }

            weightSum += weights[i];
            if (weights[i] > weights[largest])
            {
                largest = i;
            }
        }

        decimal[] parts = new decimal[weights.Count];
        decimal allocated = 0m;
        for (int i = 0; i < weights.Count; i++)
        {
            parts[i] = Math.Floor(total * weights[i] / weightSum * 100) / 100;
            allocated += parts[i];
        }

        parts[largest] += total - allocated;
        return parts;
    }

    /// <summary>
    /// The share of <paramref name="units"/> of <paramref name="totalUnits"/> in a frozen amount (02 7.1.7 (2), (3); 09 14.1 item 1): the amount times
    /// units / totalUnits, rounded DOWN to the kuruş. 100 for one unit of three: 33,33. The amount is not negative; the units are at least one and at most
    /// the total. The operation that closes the last open units takes what is left instead (02 7.1.7 (4)); that is a subtraction, not a rounding.
    /// </summary>
    public static decimal ShareOfUnits(decimal frozenAmount, int units, int totalUnits)
    {
        ThrowIfBelowZero(frozenAmount, nameof(frozenAmount));
        if (totalUnits <= 0 || units <= 0 || units > totalUnits)
        {
            throw new ArgumentOutOfRangeException(nameof(units), units, "The units are at least one and at most the total units.");
        }

        return Math.Floor(frozenAmount * units / totalUnits * 100) / 100;
    }

    // A value below zero is a programming error. The comparison is on the value, so a negative zero (-0.00m) passes; the framework's
    // ThrowIfNegative looks at the sign bit and would refuse it (D-76).
    private static void ThrowIfBelowZero(decimal value, string name)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(name, value, "The amount is below zero.");
        }
    }

    // One step of a scale: 10^-scale (two decimals: 0,01).
    private static decimal Step(int scale)
    {
        decimal step = 1m;
        for (int i = 0; i < scale; i++)
        {
            step /= 10m;
        }

        return step;
    }

    // The largest value of numeric(precision, scale): precision - scale nines before the separator and scale nines after it.
    private static decimal Limit((int Precision, int Scale) type)
    {
        decimal whole = 1m;
        for (int i = 0; i < type.Precision - type.Scale; i++)
        {
            whole *= 10m;
        }

        return whole - Step(type.Scale);
    }
}
