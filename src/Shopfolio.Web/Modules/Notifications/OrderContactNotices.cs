using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The Notifications side of a guest order's e-mail correction (T375; 02 10.4.11; 06 4.58): the root of the B-1 chain and the redirection of the unsent customer notices. It only reads and
/// updates the order's own dispatch rows, in the caller's transaction; the row lock of the order is the caller's.
/// </summary>
internal sealed class OrderContactNotices : IOrderContactNotices
{
    private readonly AppDbContext _db;

    public OrderContactNotices(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public async Task<Guid?> FindFirstOrderReceivedAsync(Guid orderId, CancellationToken ct)
    {
        return await _db.DispatchRecords.AsNoTracking()
            .Where(d => d.OrderId == orderId && d.DispatchType == DispatchType.B1 && d.OriginalDispatchId == null)
            .OrderBy(d => d.CreatedAt).ThenBy(d => d.Id)
            .Select(d => (Guid?)d.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> ReroutePendingCustomerNoticesAsync(Guid orderId, string newEmail, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newEmail);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The unsent notices are pointed anew only inside the correction's transaction (09 9.1 item 3).");
        }

        string address = newEmail.Trim();
        DispatchType[] redirected = [.. Enum.GetValues<DispatchType>().Where(t => t is >= DispatchType.B1 and <= DispatchType.B16 && t != DispatchType.B15)];
        return await _db.DispatchRecords
            .Where(d => d.OrderId == orderId && d.DispatchState == DispatchState.Queued && redirected.Contains(d.DispatchType))
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.RecipientEmail, address), ct);
    }
}
