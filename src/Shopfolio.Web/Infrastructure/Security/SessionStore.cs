using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// What opening a session gave back (02 3.13.12; 05 6.1.1, 6.7.2): the random key of the session cookie, the cookie's lifetime (the long session's P-16 days; null for the short
/// one, which is a browser-session cookie and ends P-15 hours after the last operation on the server) and the recognized browser mark to write (05 6.7.6; P-48 days). The pages
/// that write the cookies are the sign-in pages' tasks; this record is the whole of what they need.
/// </summary>
public sealed record SessionOpening(Guid SessionId, string SessionKey, bool IsLongLived, TimeSpan? CookieLifetime, string MarkValue, TimeSpan MarkLifetime);

/// <summary>
/// The one code that writes and deletes sessions, and the one that writes recognized browser marks (09 15.1 item 3; D-133): the customer's and the administrator's sign-in take the
/// same path, so the administrator gets the same lifetimes and the same invalidation (02 3.13.16). Every write joins the caller's transaction and never saves: a new row is added and
/// the transaction's owner saves, a delete or an update is a conditional statement of its own. The reauthentication permission is the one written by
/// <see cref="ReauthenticationValues"/>, reached only through here. A source scan keeps every other writer out.
/// </summary>
internal sealed class SessionStore
{
    private readonly AppDbContext _db;
    private readonly ReauthenticationValues _reauthentication;
    private readonly TimeProvider _time;

    public SessionStore(AppDbContext db, ReauthenticationValues reauthentication, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(reauthentication);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _reauthentication = reauthentication;
        _time = time;
    }

    /// <summary>The cookie lifetime of a long session and the idle lifetime the handler counts from the last operation (P-16).</summary>
    public static TimeSpan LongLifetime { get; } = TimeSpan.FromDays(ProductConstants.LongSessionDays);

    /// <summary>The recognized browser mark's lifetime (P-48).</summary>
    public static TimeSpan MarkLifetime { get; } = TimeSpan.FromDays(ProductConstants.RecognizedBrowserMarkDays);

    /// <summary>
    /// Opens a session for the account and gives the mark of its browser: a presented mark that is valid for this account is kept and its life renewed, anything else gets a new
    /// value and a new row (Z-46). The session is a new row; the cookie is not read here.
    /// </summary>
    public async Task<SessionOpening> OpenAsync(AccountType type, Guid accountId, bool isLongLived, string? presentedMark, CancellationToken ct)
    {
        // The session row joins the sign-in's transaction under the account's row lock (taken by the credential check, asked again here): a reset or change that wrote the digest
        // first has already ended the sessions, one that comes later waits and then deletes this one (09 15.1 item 3; 05 6.1.2).
        await AccountLocks.LockAsync(_db, type, accountId, ct);
        DateTimeOffset now = _time.GetUtcNow();
        string key = SecretKey.Create();
        string digest = SecretKey.Hash(key);
        Session session = type == AccountType.Admin ? Session.ForAdmin(accountId, digest, isLongLived, now) : Session.ForCustomer(accountId, digest, isLongLived, now);
        _db.Sessions.Add(session);
        string mark = await IssueMarkAsync(type, accountId, presentedMark, ct);
        return new SessionOpening(session.Id, key, isLongLived, isLongLived ? LongLifetime : null, mark, MarkLifetime);
    }

    /// <summary>Ends one session: the row is deleted.</summary>
    public async Task<bool> EndAsync(Guid sessionId, CancellationToken ct)
    {
        return await _db.Sessions.Where(s => s.Id == sessionId).ExecuteDeleteAsync(ct) == 1;
    }

    /// <summary>Ends every session of the account, except the one named (the session that made the change stays open); null ends them all.</summary>
    public async Task<int> EndAllAsync(AccountType type, Guid accountId, Guid? exceptSessionId, CancellationToken ct)
    {
        // Session rows of an account are written under the account's row lock, asked first (a no-op when the flow took it already); 09 15.1 item 3.
        await AccountLocks.LockAsync(_db, type, accountId, ct);
        Guid keep = exceptSessionId ?? Guid.Empty;
        return type == AccountType.Admin
            ? await _db.Sessions.Where(s => s.AdminId == accountId && s.Id != keep).ExecuteDeleteAsync(ct)
            : await _db.Sessions.Where(s => s.CustomerId == accountId && s.Id != keep).ExecuteDeleteAsync(ct);
    }

    /// <summary>True when the session exists and belongs to the account of the type.</summary>
    public async Task<bool> OwnsAsync(AccountType type, Guid accountId, Guid sessionId, CancellationToken ct)
    {
        return type == AccountType.Admin
            ? await _db.Sessions.AnyAsync(s => s.Id == sessionId && s.AdminId == accountId, ct)
            : await _db.Sessions.AnyAsync(s => s.Id == sessionId && s.CustomerId == accountId, ct);
    }

    /// <summary>The identifier of the mark's record when the presented value is a valid mark of this account (known, this account's, not ended); null otherwise.</summary>
    public async Task<Guid?> FindMarkAsync(AccountType type, Guid accountId, string? presentedMark, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(presentedMark))
        {
            return null;
        }

        string digest = SecretKey.Hash(presentedMark);
        DateTimeOffset now = _time.GetUtcNow();
        return type == AccountType.Admin
            ? await _db.RecognizedBrowserMarks.AsNoTracking().Where(m => m.MarkHash == digest && m.AdminId == accountId && m.ExpiresAt > now).Select(m => (Guid?)m.Id).SingleOrDefaultAsync(ct)
            : await _db.RecognizedBrowserMarks.AsNoTracking().Where(m => m.MarkHash == digest && m.CustomerId == accountId && m.ExpiresAt > now).Select(m => (Guid?)m.Id).SingleOrDefaultAsync(ct);
    }

    /// <summary>
    /// Ends the account's marks, except the presented one (the browser that made the change stays recognized): the expiry is pulled to now (Z-46), so an ended mark is not a state.
    /// </summary>
    public async Task<int> InvalidateMarksAsync(AccountType type, Guid accountId, string? exceptMark, CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        string keep = string.IsNullOrEmpty(exceptMark) ? string.Empty : SecretKey.Hash(exceptMark);
        return type == AccountType.Admin
            ? await _db.RecognizedBrowserMarks.Where(m => m.AdminId == accountId && m.MarkHash != keep && m.ExpiresAt > now).ExecuteUpdateAsync(s => s.SetProperty(m => m.ExpiresAt, now), ct)
            : await _db.RecognizedBrowserMarks.Where(m => m.CustomerId == accountId && m.MarkHash != keep && m.ExpiresAt > now).ExecuteUpdateAsync(s => s.SetProperty(m => m.ExpiresAt, now), ct);
    }

    /// <summary>
    /// The mark to write after a successful sign-in, reset or change: the presented value when it is valid for the account (its life renewed from now, P-48), else a new value and a
    /// new row.
    /// </summary>
    public async Task<string> IssueMarkAsync(AccountType type, Guid accountId, string? presentedMark, CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        DateTimeOffset expiresAt = now + MarkLifetime;
        if (!string.IsNullOrEmpty(presentedMark))
        {
            string known = SecretKey.Hash(presentedMark);
            int renewed = type == AccountType.Admin
                ? await _db.RecognizedBrowserMarks.Where(m => m.MarkHash == known && m.AdminId == accountId && m.ExpiresAt > now).ExecuteUpdateAsync(s => s.SetProperty(m => m.ExpiresAt, expiresAt), ct)
                : await _db.RecognizedBrowserMarks.Where(m => m.MarkHash == known && m.CustomerId == accountId && m.ExpiresAt > now).ExecuteUpdateAsync(s => s.SetProperty(m => m.ExpiresAt, expiresAt), ct);
            if (renewed == 1)
            {
                return presentedMark;
            }
        }

        string value = SecretKey.Create();
        string digest = SecretKey.Hash(value);
        _db.RecognizedBrowserMarks.Add(type == AccountType.Admin
            ? RecognizedBrowserMark.ForAdmin(accountId, digest, expiresAt, now)
            : RecognizedBrowserMark.ForCustomer(accountId, digest, expiresAt, now));
        return value;
    }

    /// <summary>
    /// Gives the one-operation permission to a session of the account and returns the value of the form's hidden field; null when the session is not the account's, or when the
    /// operation is not the account type's (an administrator has no account deletion, 02 3.13.16).
    /// </summary>
    public async Task<string?> GrantReauthenticationAsync(AccountType type, Guid accountId, Guid sessionId, ReauthenticationOperation operation, CancellationToken ct)
    {
        if (type == AccountType.Admin && operation == ReauthenticationOperation.AccountDeletion)
        {
            return null;
        }

        await AccountLocks.LockAsync(_db, type, accountId, ct);
        return await OwnsAsync(type, accountId, sessionId, ct) ? await _reauthentication.GrantAsync(sessionId, operation, ct) : null;
    }

    /// <summary>Takes the permission with one conditional update (the session must be the account's); false means the operation must not run.</summary>
    public async Task<bool> TryRedeemReauthenticationAsync(AccountType type, Guid accountId, Guid sessionId, ReauthenticationOperation operation, string? value, CancellationToken ct)
    {
        await AccountLocks.LockAsync(_db, type, accountId, ct);
        return await OwnsAsync(type, accountId, sessionId, ct) && await _reauthentication.TryRedeemAsync(sessionId, operation, value, ct);
    }
}
