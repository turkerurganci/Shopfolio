using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The panel home's two warnings (T382; 05 10.2.1): read from the channel state record and from the firm notices' dispatch rows at every call, never stored - so the warning lifts by
/// itself when the next firm notice is sent or the contact e-mail changes. The channel rule is <see cref="ChannelWarningCondition.IsOpen"/>, the one home of its two numbers.
/// </summary>
internal sealed class PanelWarningReader : IPanelWarnings
{
    // F-1 to F-4: the notices that go to the firm (05 10.2.4).
    private static readonly DispatchType[] FirmNotices = [DispatchType.F1, DispatchType.F2, DispatchType.F3, DispatchType.F4];

    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public PanelWarningReader(AppDbContext db, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _time = time;
    }

    public async Task<PanelWarnings> ReadAsync(string? currentCompanyEmail, CancellationToken ct)
    {
        var channel = await _db.ChannelStateRecords.AsNoTracking()
            .Select(c => new { c.ConsecutiveChannelErrorCount, c.FirstChannelErrorAt })
            .SingleOrDefaultAsync(ct);
        bool channelOpen = channel is not null && ChannelWarningCondition.IsOpen(channel.ConsecutiveChannelErrorCount, channel.FirstChannelErrorAt, _time.GetUtcNow());

        bool firmOpen = false;
        if (!string.IsNullOrWhiteSpace(currentCompanyEmail))
        {
            // The last row whose result is settled (Sent or Undelivered; a queued row is still being tried); the id breaks a tie of the same moment (06 UuidOrder).
            var last = await _db.DispatchRecords.AsNoTracking()
                .Where(d => FirmNotices.Contains(d.DispatchType) && d.DispatchState != DispatchState.Queued)
                .OrderByDescending(d => d.CreatedAt).ThenByDescending(d => d.Id)
                .Select(d => new { d.DispatchState, d.RecipientEmail })
                .FirstOrDefaultAsync(ct);
            firmOpen = last is { DispatchState: DispatchState.Undelivered, RecipientEmail: { } recipient }
                && string.Equals(recipient, currentCompanyEmail, StringComparison.OrdinalIgnoreCase);
        }

        return new PanelWarnings(channelOpen, firmOpen);
    }
}
