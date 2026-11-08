using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// The deletion of a customer account (02 3.15.1-3.15.3, 3.15.5, 3.15.6; 03 9.4.2, 9.4.6; 07 UC-68, UC-157; T345): one rule for the member's own deletion and for the administrator's deletion on
/// request. In the caller's transaction, under the account's row (05 6.1.2; K-1084): the open revert link of an e-mail change closes the deletion; the account's orders are marked as an
/// account's deleted one and cut from it (they go on with their own record and are never linked again); the "your account was deleted" e-mail is queued once for the account's address; the
/// account is removed and the database deletes what hangs on it - the sessions, the recognized browser marks, the address book, the cart, the pending e-mail change and the link records;
/// the disposal record gets the account and what went with it, by type and number. The method never saves: the caller's unit of work writes the whole in one save.
/// </summary>
internal sealed class AccountDeletions
{
    private readonly AppDbContext _db;
    private readonly IOrderingModule _ordering;
    private readonly INotificationsModule _notifications;
    private readonly IAuditTrail _trail;
    private readonly ISessionService _sessions;
    private readonly TimeProvider _time;

    public AccountDeletions(AppDbContext db, IOrderingModule ordering, INotificationsModule notifications, IAuditTrail trail, ISessionService sessions, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(trail);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _ordering = ordering;
        _notifications = notifications;
        _trail = trail;
        _sessions = sessions;
        _time = time;
    }

    /// <summary>
    /// The member's own deletion (07 UC-68): the account row first, then the revert link's gate, then the one-operation permission of the reauthentication (02 3.13.13) is redeemed - a refusal
    /// before it leaves the value unspent - and then the deletion. The session the permission belongs to goes with the account.
    /// </summary>
    public async Task<AccountDeletionResult> DeleteOwnAsync(Guid customerId, Guid sessionId, string? reauthenticationValue, CancellationToken ct)
    {
        RequireTransaction();
        DateTimeOffset now = _time.GetUtcNow();
        AccountDeletionResult? refusal = await HoldAndCheckAsync(customerId, now, ct);
        if (refusal is not null)
        {
            return refusal;
        }

        if (!await _sessions.TryRedeemReauthenticationAsync(customerId, sessionId, ReauthenticationOperation.AccountDeletion, reauthenticationValue, ct))
        {
            return AccountDeletionResult.Refused(ErrorCode.ReauthenticationRequired);
        }

        return await RemoveAsync(customerId, now, ct);
    }

    /// <summary>
    /// The administrator's deletion on the data subject's request (02 3.15.5; 07 UC-157): the same gate and the same removal as the member's own, with no reauthentication (that is the account's own
    /// operation, 02 8.1.5), and the operation's row in the trail - it names the operation and its date, not the account (02 10.3.1 (8)). The irreversible-action confirmation is the page's (03 1.6.1.8).
    /// An unverified pending registration is no account: it is not found.
    /// </summary>
    public async Task<AccountDeletionResult> DeleteByAdministratorAsync(Guid customerId, CancellationToken ct)
    {
        RequireTransaction();
        DateTimeOffset now = _time.GetUtcNow();
        AccountDeletionResult? refusal = await HoldAndCheckAsync(customerId, now, ct);
        if (refusal is not null)
        {
            return refusal;
        }

        // The row is written in the same transaction as the removal (09 9.2 item 5); the details are the operation and its Turkey day, nothing of the customer.
        string day = TurkeyTime.DateOf(now).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await _trail.WriteAsync(
            new AuditEntry(
                AuditOperation.DeleteCustomerAccount,
                "Customer",
                customerId,
                new AuditDetails(values: [new AuditValue("Operation", nameof(AuditOperation.DeleteCustomerAccount)), new AuditValue("Date", day)])),
            ct);
        return await RemoveAsync(customerId, now, ct);
    }

    // The account's row is held by a no-op update of the verified account (the same row lock a sign-in or a password change takes): the lock family of the account is taken alone (05 7.3.5)
    // and this transaction goes on into the order rows, so the row is held by the write, not by the lock sequence - the way the new address's verification holds it. Everything is read after it.
    private async Task<AccountDeletionResult?> HoldAndCheckAsync(Guid customerId, DateTimeOffset now, CancellationToken ct)
    {
        int held = await _db.Customers
            .Where(c => c.Id == customerId && c.EmailVerifiedAt != null)
            .ExecuteUpdateAsync(set => set.SetProperty(c => c.Name, c => c.Name), ct);
        if (held != 1)
        {
            return AccountDeletionResult.Refused(ErrorCode.NotFound);
        }

        // An open revert link of an e-mail change closes the deletion - the member's and the firm's (02 3.15.6; K-698): the sign of the way back must not be spent by deleting the account.
        DateTimeOffset? revertEnds = await EmailChanges.OpenRevertEndAsync(_db, customerId, now, ct);
        return revertEnds is { } ends ? AccountDeletionResult.RevertPending(ends) : null;
    }

    private async Task<AccountDeletionResult> RemoveAsync(Guid customerId, DateTimeOffset now, CancellationToken ct)
    {
        Customer account = await _db.Customers.SingleAsync(c => c.Id == customerId, ct);
        string address = account.Email;

        // What hangs on the account is counted before it goes: the database deletes it with the account (06 2.2.5), the disposal record says how much.
        int sessions = await _db.Sessions.AsNoTracking().CountAsync(s => s.CustomerId == customerId, ct);
        int marks = await _db.RecognizedBrowserMarks.AsNoTracking().CountAsync(m => m.CustomerId == customerId, ct);
        int pendingChanges = await _db.PendingEmailChanges.AsNoTracking().CountAsync(p => p.CustomerId == customerId, ct);
        int links = await _db.LinkRecords.AsNoTracking().CountAsync(l => l.CustomerId == customerId, ct);
        AccountOrderRelease orders = await _ordering.MarkAccountDeletedAsync(customerId, ct);

        // One e-mail to the address of the account, whoever deletes it (02 3.15.1, 9.4); it carries no link and no way back. The address is kept as a value of the row.
        await _notifications.QueueAccountEmailAsync(new AccountDeletedEmail(customerId, address, now), ct);

        _db.Customers.Remove(account);
        _db.DisposalRecords.Add(DisposalRecord.Counted(DisposalRecordType.Customer, 1, now));
        AddCounted(DisposalRecordType.Session, sessions, now);
        AddCounted(DisposalRecordType.RecognizedBrowserMark, marks, now);
        AddCounted(DisposalRecordType.Cart, orders.Carts, now);
        AddCounted(DisposalRecordType.PendingEmailChange, pendingChanges, now);
        AddCounted(DisposalRecordType.LinkRecord, links, now);
        return AccountDeletionResult.Deleted(orders.Orders);
    }

    // A disposal row has a number above zero (06 4.64): what was not there writes no row.
    private void AddCounted(DisposalRecordType type, int count, DateTimeOffset now)
    {
        if (count > 0)
        {
            _db.DisposalRecords.Add(DisposalRecord.Counted(type, count, now));
        }
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The account is deleted only inside a transaction (09 6 item 3).");
        }
    }
}
