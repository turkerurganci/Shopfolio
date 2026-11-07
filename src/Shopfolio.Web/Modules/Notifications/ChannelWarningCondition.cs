using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Status;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The e-mail channel warning (05 10.2.3; 9.4.7) as a condition of the status endpoint: at least five consecutive channel errors since the last
/// successful sending and the series' first error at least fifteen minutes old; the first success closes it. The warning is read from the
/// channel state record (06 4.60), never stored. The two numbers are the design values of 05 10.2.3 and belong to the sender's rule, not to the
/// status endpoint's tunable thresholds (05 8.4.5).
/// </summary>
internal sealed class ChannelWarningCondition : IStatusCondition
{
    /// <summary>05 10.2.3 - consecutive channel errors that open the warning.</summary>
    public const int ErrorsInARow = 5;

    /// <summary>05 10.2.3 - minutes the series' first error must be older than.</summary>
    public const int FirstErrorAgeMinutes = 15;

    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public ChannelWarningCondition(AppDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public string Name => "EmailChannelWarning";

    public static bool IsOpen(int consecutiveErrors, DateTimeOffset? firstErrorAt, DateTimeOffset now) =>
        consecutiveErrors >= ErrorsInARow
        && firstErrorAt is { } first
        && now - first >= TimeSpan.FromMinutes(FirstErrorAgeMinutes);

    public async Task<bool> IsOpenAsync(CancellationToken ct)
    {
        var state = await _db.ChannelStateRecords.AsNoTracking()
            .Select(c => new { c.ConsecutiveChannelErrorCount, c.FirstChannelErrorAt })
            .SingleAsync(ct);
        return IsOpen(state.ConsecutiveChannelErrorCount, state.FirstChannelErrorAt, _time.GetUtcNow());
    }
}
