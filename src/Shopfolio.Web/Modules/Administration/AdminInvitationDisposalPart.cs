using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The administrator invitations that were accepted, withdrawn, invalidated or whose link ran out (06 9.4.3; Z-3): the invitation's link record
/// goes with it by the foreign key. The invitation's life is its link record's expiry, so it is read from there and not repeated on the invitation.
/// The part lives in the module that owns the invitation (09 6 item 2) and joins the disposal through its registration.
/// </summary>
internal sealed class AdminInvitationDisposalPart : IDisposalPart
{
    public string Name => "AdminInvitations";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset now = run.Now;
        return run.ChunksAsync(
            DisposalRecordType.AdminInvitation,
            async (db, max, cancel) =>
            {
                IQueryable<AdminInvitation> expired = db.AdminInvitations.Where(
                    i => i.AcceptedAt != null
                        || i.WithdrawnAt != null
                        || i.InvalidatedAt != null
                        || db.LinkRecords.Any(l => l.AdminInvitationId == i.Id && l.ExpiresAt < now));
                List<Guid> ids = await expired.OrderBy(i => i.CreatedAt).Select(i => i.Id).Take(max).ToListAsync(cancel);
                return await expired.Where(i => ids.Contains(i.Id)).ExecuteDeleteAsync(cancel);
            },
            ct);
    }
}
