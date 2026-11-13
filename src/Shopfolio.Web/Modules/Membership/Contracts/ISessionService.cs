using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Membership.Contracts;

/// <summary>
/// The customer's session, written and ended only here (09 15.1 item 3; 02 3.13.12; D-133): the sign-in's page opens a session after the credential check and ends it at the
/// sign-out; the account's pages ask for the one-operation reauthentication permission and redeem it. Every method joins the caller's transaction and does not save. The cookie
/// is written by the page from the returned <see cref="SessionOpening"/>; no cookie is read here.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Opens a session for the customer: "remember me" gives the long session (P-16 days, Z-5), no choice the short one (P-15 hours from the last operation, Z-4). The presented mark
    /// of the browser is kept when it is valid for this customer, else a new one is given. It runs in the same transaction as the credential check that granted it: the check locks the
    /// account row (<c>AccountLocks</c>) and the session is written under that lock, so a password reset or change cannot slip between the two (09 15.1 item 3; 05 6.1.2).
    /// </summary>
    Task<SessionOpening> SignInAsync(Guid customerId, bool rememberMe, string? presentedMark, CancellationToken ct);

    /// <summary>Ends one session (the sign-out).</summary>
    Task<bool> EndAsync(Guid sessionId, CancellationToken ct);

    /// <summary>Ends every session of the customer except the named one; null ends them all.</summary>
    Task<int> EndAllAsync(Guid customerId, Guid? exceptSessionId, CancellationToken ct);

    /// <summary>Gives the session the permission for one operation and returns the form's value; null when the session is not the customer's.</summary>
    Task<string?> GrantReauthenticationAsync(Guid customerId, Guid sessionId, ReauthenticationOperation operation, CancellationToken ct);

    /// <summary>Drops the permission the session holds, if any (T516; 07 UC-60: opening the re-authentication screen drops the earlier permission, 05 6.1.5); nothing happens when the session is not the customer's.</summary>
    Task DropReauthenticationAsync(Guid customerId, Guid sessionId, CancellationToken ct);

    /// <summary>Takes the permission with one conditional update; false means the operation must not run.</summary>
    Task<bool> TryRedeemReauthenticationAsync(Guid customerId, Guid sessionId, ReauthenticationOperation operation, string? value, CancellationToken ct);

    /// <summary>The same take for a caller that already holds the customer's row by a write of its own and goes on into the order locks (the account lock is a family taken alone, 05 7.3.5).</summary>
    Task<bool> TryRedeemReauthenticationHeldAsync(Guid customerId, Guid sessionId, ReauthenticationOperation operation, string? value, CancellationToken ct);
}
