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
    /// of the browser is kept when it is valid for this customer, else a new one is given.
    /// </summary>
    Task<SessionOpening> SignInAsync(Guid customerId, bool rememberMe, string? presentedMark, CancellationToken ct);

    /// <summary>Ends one session (the sign-out).</summary>
    Task<bool> EndAsync(Guid sessionId, CancellationToken ct);

    /// <summary>Ends every session of the customer except the named one; null ends them all.</summary>
    Task<int> EndAllAsync(Guid customerId, Guid? exceptSessionId, CancellationToken ct);

    /// <summary>Gives the session the permission for one operation and returns the form's value; null when the session is not the customer's.</summary>
    Task<string?> GrantReauthenticationAsync(Guid customerId, Guid sessionId, ReauthenticationOperation operation, CancellationToken ct);

    /// <summary>Takes the permission with one conditional update; false means the operation must not run.</summary>
    Task<bool> TryRedeemReauthenticationAsync(Guid customerId, Guid sessionId, ReauthenticationOperation operation, string? value, CancellationToken ct);
}
