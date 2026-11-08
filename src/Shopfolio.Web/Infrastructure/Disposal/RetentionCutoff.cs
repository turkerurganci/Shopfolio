namespace Shopfolio.Infrastructure.Disposal;

/// <summary>
/// The one way a disposal part turns a period set in calendar years into the moment before which a record is past it (05 7.4.1; 05 7.2.1): the years
/// are counted in Turkey time (<see cref="TurkeyTime"/>), the product's one time zone, and the result is a UTC instant. The period's number is the
/// caller's constant; no number is written here.
/// </summary>
internal static class RetentionCutoff
{
    /// <summary>The moment <paramref name="years"/> calendar years before <paramref name="now"/>: a record made before it is past a period of that many years.</summary>
    public static DateTimeOffset YearsBefore(DateTimeOffset now, int years)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(years);
        return now.ToOffset(TurkeyTime.Offset).AddYears(-years).ToUniversalTime();
    }
}
