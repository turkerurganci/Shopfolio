using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The one writer of the download counter (09 10.2 item 4; 05 7.7.4; 06 2.6.4, 4.18; K-980): an order item's <c>DownloadCount</c> is written here and nowhere else.
/// A download is taken by one conditional update - the condition (a digital item, a confirmed payment, a count below the frozen allowance) and the arithmetic are in
/// the statement; nothing is read, counted and written back, so two requests at the same moment never both take the last download. The renewal is the firm's
/// operation: under the order's row lock, against the version the screen showed, it sets the counter back to zero and raises the order's version by one (06 2.6.4:
/// the firm's write of an item raises it, the customer's counter does not). Neither touches the allowance, which is frozen in the item at the order (02 3.12.5, 3.23.1),
/// and neither is changed by a file update (02 3.12.8). Both join the caller's transaction and need one (09 9.1 item 3).
/// </summary>
internal sealed class DownloadCounters
{
    private readonly AppDbContext _db;
    private readonly OrderLocks _locks;

    public DownloadCounters(AppDbContext db, OrderLocks locks)
    {
        _db = db;
        _locks = locks;
    }

    /// <summary>Takes one download from the item's right; see <see cref="DownloadTakeResult"/>.</summary>
    public async Task<DownloadTakeResult> TakeAsync(Guid orderItemId, CancellationToken ct)
    {
        RequireTransaction();
        int written = await _db.OrderItems
            .Where(i => i.Id == orderItemId
                && i.ProductType == ProductType.Digital
                && i.DownloadAllowance != null
                && i.DownloadCount < i.DownloadAllowance
                && _db.Orders.Where(DownloadRules.PaymentConfirmed).Any(o => o.Id == i.OrderId))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.DownloadCount, i => i.DownloadCount + 1), ct);
        if (written == 1)
        {
            return DownloadTakeResult.Taken;
        }

        // Nothing was written: the reason is read in the same transaction. A used-up right is told apart from an item that cannot be downloaded.
        bool isUsedUp = await _db.OrderItems.AsNoTracking()
            .Where(i => i.Id == orderItemId
                && i.ProductType == ProductType.Digital
                && i.DownloadAllowance != null
                && i.DownloadCount >= i.DownloadAllowance
                && _db.Orders.Where(DownloadRules.PaymentConfirmed).Any(o => o.Id == i.OrderId))
            .AnyAsync(ct);
        return isUsedUp ? DownloadTakeResult.AllowanceUsed : DownloadTakeResult.NotAvailable;
    }

    /// <summary>The firm's renewal of the item's right (02 3.12.6): the counter back at zero, no notice (02 9.2), the order's version up by one.</summary>
    public async Task<DownloadRenewalResult> RenewAsync(Guid orderItemId, int expectedVersion, CancellationToken ct)
    {
        RequireTransaction();
        Guid? orderId = await _db.OrderItems.AsNoTracking()
            .Where(i => i.Id == orderItemId)
            .Select(i => (Guid?)i.OrderId)
            .SingleOrDefaultAsync(ct);
        if (orderId is not Guid id)
        {
            return DownloadRenewalResult.NotFound;
        }

        // The order is locked first and read after the lock (05 7.3.2; 09 9.1 item 5); the item is read again under it.
        await _locks.LockAsync([id], ct);
        Order order = await _db.Orders.SingleAsync(o => o.Id == id, ct);
        if (!ShownVersion.Matches(order, expectedVersion))
        {
            return DownloadRenewalResult.VersionConflict;
        }

        int written = await _db.OrderItems
            .Where(i => i.Id == orderItemId && i.ProductType == ProductType.Digital)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.DownloadCount, 0), ct);
        if (written == 0)
        {
            return DownloadRenewalResult.NotAvailable;
        }

        order.BumpVersion();
        return DownloadRenewalResult.Renewed;
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The download counter is written only inside the caller's transaction (09 9.1 item 3).");
        }
    }
}
