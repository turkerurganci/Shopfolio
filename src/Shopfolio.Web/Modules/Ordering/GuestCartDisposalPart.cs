using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The guest carts nobody opened for 400 days (06 9.4.3; Z-6; 05 6.7.8); the cart's items go with it. A member's cart lives with the account and is
/// never judged here. The part lives in the module that owns the cart (09 6 item 2) and joins the disposal through its registration.
/// </summary>
internal sealed class GuestCartDisposalPart : IDisposalPart
{
    public string Name => "GuestCarts";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset cutoff = run.Now - TimeSpan.FromDays(DisposalTimings.GuestCartIdleDays);
        return run.ChunksAsync(
            DisposalRecordType.Cart,
            async (db, max, cancel) =>
            {
                IQueryable<Cart> expired = db.Carts.Where(c => c.GuestCartKeyHash != null && c.LastAccessedAt < cutoff);
                List<Guid> ids = await expired.OrderBy(c => c.LastAccessedAt).Select(c => c.Id).Take(max).ToListAsync(cancel);
                return await expired.Where(c => ids.Contains(c.Id)).ExecuteDeleteAsync(cancel);
            },
            ct);
    }
}
