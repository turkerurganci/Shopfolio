namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// Whether the cookie policy has been published for the first time (05 6.7.10): once it has, the gate is open for good. A published version is never
/// withdrawn — a change is a new version (02 3.1.6) — so the answer, once true, is kept in memory and the database is asked no more; until then it is
/// asked at every storefront request, which is the price of opening the gate at the first request after the publication. The answer is read through
/// <see cref="ICookiePolicyPublication"/>; the gate touches no table.
/// </summary>
internal sealed class CookiePolicyGate
{
    private volatile bool _open;

    public async ValueTask<bool> IsOpenAsync(ICookiePolicyPublication publication, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(publication);
        if (_open)
        {
            return true;
        }

        if (await publication.IsPublishedAsync(cancellationToken))
        {
            _open = true;
        }

        return _open;
    }
}
