// Deliberate violations of the time rules (09 7.1 items 1, 3, 5). Compiled into its own assembly (TimeAndMoneyViolationSample.dll); the
// architecture test shows each read is caught, also inside a lambda, an async method and a static field initializer.
using System;
using System.Threading.Tasks;

namespace Shopfolio.Modules.Sample;

public static class TimeViolations
{
    private static readonly DateTime _loadedAt = DateTime.UtcNow;

    public static DateTime LocalNow() => DateTime.Now;

    public static DateTime Today() => DateTime.Today;

    public static DateTimeOffset OffsetNow() => DateTimeOffset.Now;

    public static DateTimeOffset OffsetUtcNow() => DateTimeOffset.UtcNow;

    public static TimeZoneInfo LocalZone() => TimeZoneInfo.Local;

    public static TimeZoneInfo ZoneDatabase() => TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    public static DateTime ServerLocal(DateTime value) => value.ToLocalTime();

    public static DateTime ServerUniversal(DateTime value) => value.ToUniversalTime();

    public static DateTimeOffset OffsetServerLocal(DateTimeOffset value) => value.ToLocalTime();

    public static DateTime OffsetLocalDateTime(DateTimeOffset value) => value.LocalDateTime;

    public static DateTimeOffset FromDateTime(DateTime value) => new(value);

    public static DateTimeOffset Converted(DateTime value) => value;

    public static DateTimeOffset ProviderLocal(TimeProvider time) => time.GetLocalNow();

    public static TimeZoneInfo ProviderZone(TimeProvider time) => time.LocalTimeZone;

    public static TimeProvider SystemClock() => TimeProvider.System;

    public static Func<DateTime> InLambda() => () => DateTime.UtcNow;

    public static async Task<DateTimeOffset> InAsyncMethod()
    {
        await Task.Yield();
        return DateTimeOffset.UtcNow;
    }

    public static DateTime LoadedAt() => _loadedAt;
}

// An instant kept as DateTime (09 7.1 item 2): a property, a field, a parameter, a constructor parameter and a return. StoredAt is the
// allowed form and is not reported.
public sealed class LocalTimeRecord
{
    private DateTime? _shippedAt;

    public LocalTimeRecord(DateTime openedAt)
    {
        CreatedAt = openedAt;
    }

    public DateTime CreatedAt { get; set; }

    public DateTimeOffset StoredAt { get; set; }

    public void Expire(DateTime dueAt) => _shippedAt = dueAt;

    public DateTime? DueAt() => _shippedAt;
}
