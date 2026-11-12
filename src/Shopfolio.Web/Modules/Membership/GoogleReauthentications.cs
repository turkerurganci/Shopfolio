using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// The Google re-authentication's return rule (T516; 02 3.13.13; 05 6.1.5; K-968): the account's side of the check that follows the connector's verification. A re-authentication through Google
/// is accepted only for an account that is already linked to Google (it binds nothing and makes no confirmation link), only when Google gives the e-mail as verified and it is the account's own
/// address, and only when the Google sign-in's moment (<c>auth_time</c>) is after the start of the flow - a Google that returned from an open session without asking again has an older moment or none, and does
/// not count. The permission goes to the session of this account only and is the one-operation permission of the password route. Joins the caller's transaction; it is not counted to L-1 (02 3.13.17).
/// </summary>
internal sealed class GoogleReauthentications
{
    private readonly AppDbContext _db;
    private readonly SessionStore _sessions;

    public GoogleReauthentications(AppDbContext db, SessionStore sessions)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(sessions);
        _db = db;
        _sessions = sessions;
    }

    public async Task<GoogleReauthenticationResult> GrantAsync(
        Guid customerId, Guid sessionId, ReauthenticationOperation operation, GoogleReauthenticationIdentity identity, DateTimeOffset flowStartedAt, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (!identity.EmailVerified || identity.AuthTime is not { } signedInAt || signedInAt <= flowStartedAt || string.IsNullOrWhiteSpace(identity.Email))
        {
            return GoogleReauthenticationResult.NotGranted();
        }

        var account = await _db.Customers.AsNoTracking()
            .Where(c => c.Id == customerId && c.EmailVerifiedAt != null)
            .Select(c => new { c.NormalizedEmail, c.GoogleLinkedAt })
            .SingleOrDefaultAsync(ct);
        if (account is null || account.GoogleLinkedAt is null || account.NormalizedEmail != TextNormalizer.NormalizeIdentity(identity.Email))
        {
            return GoogleReauthenticationResult.NotGranted();
        }

        string? value = await _sessions.GrantReauthenticationAsync(AccountType.Customer, customerId, sessionId, operation, ct);
        return value is null ? GoogleReauthenticationResult.NotGranted() : GoogleReauthenticationResult.Granted(value);
    }
}
