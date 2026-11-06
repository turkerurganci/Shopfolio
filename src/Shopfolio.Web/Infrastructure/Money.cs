namespace Shopfolio.Infrastructure;

/// <summary>
/// The only place where an amount is rounded to the kuruş (09 14.1 item 1; 05 7.2.6; 02 3.8.3). The direction is written out:
/// .NET's default <c>Math.Round(x, 2)</c> takes an exact half to the even digit (12,345 to 12,34), the rule wants 12,35. Money is
/// <see cref="decimal"/>; no floating point type takes part (05 7.2.5). The other two functions of 09 14.1 item 1 (the share of units and
/// the VAT of a gross amount) come with the tasks that compute them; they are written here, next to these two.
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
    /// negative where the product rounds them (02 3.8.3), so a negative input is a programming error and throws (09 14.1 item 1).
    /// </summary>
    public static decimal Round(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
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
        ArgumentOutOfRangeException.ThrowIfNegative(total);
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
