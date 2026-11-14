using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Modules.Membership;

/// <summary>The customer's thin face of the shared <see cref="SessionStore"/> (09 15.1 item 3): the administrator's session goes through the same store.</summary>
internal sealed class SessionService : ISessionService
{
    private readonly SessionStore _store;

    public SessionService(SessionStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    public Task<SessionOpening> SignInAsync(Guid customerId, bool rememberMe, string? presentedMark, CancellationToken ct)
        => _store.OpenAsync(AccountType.Customer, customerId, rememberMe, presentedMark, ct);

    public Task<bool> EndAsync(Guid sessionId, CancellationToken ct) => _store.EndAsync(sessionId, ct);

    public Task<int> EndAllAsync(Guid customerId, Guid? exceptSessionId, CancellationToken ct) => _store.EndAllAsync(AccountType.Customer, customerId, exceptSessionId, ct);

    public Task<string?> GrantReauthenticationAsync(Guid customerId, Guid sessionId, ReauthenticationOperation operation, CancellationToken ct)
        => _store.GrantReauthenticationAsync(AccountType.Customer, customerId, sessionId, operation, ct);

    public Task DropReauthenticationAsync(Guid customerId, Guid sessionId, CancellationToken ct)
        => _store.DropReauthenticationAsync(AccountType.Customer, customerId, sessionId, ct);

    public Task<bool> HoldsReauthenticationAsync(Guid customerId, Guid sessionId, ReauthenticationOperation operation, string? value, CancellationToken ct)
        => _store.HoldsReauthenticationAsync(AccountType.Customer, customerId, sessionId, operation, value, ct);

    public Task<bool> TryRedeemReauthenticationAsync(Guid customerId, Guid sessionId, ReauthenticationOperation operation, string? value, CancellationToken ct)
        => _store.TryRedeemReauthenticationAsync(AccountType.Customer, customerId, sessionId, operation, value, ct);

    public Task<bool> TryRedeemReauthenticationHeldAsync(Guid customerId, Guid sessionId, ReauthenticationOperation operation, string? value, CancellationToken ct)
        => _store.TryRedeemReauthenticationHeldAsync(AccountType.Customer, customerId, sessionId, operation, value, ct);
}
