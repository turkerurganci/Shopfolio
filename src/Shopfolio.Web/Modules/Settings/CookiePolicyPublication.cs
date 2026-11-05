using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// Answers the shared infrastructure's question whether the cookie policy has been published (05 6.7.10; <see cref="ICookiePolicyPublication"/>):
/// a version row of the type <see cref="LegalTextType.CookiePolicy"/> that has a publish time. The draft row has none and does not count.
/// </summary>
internal sealed class CookiePolicyPublication : ICookiePolicyPublication
{
    private readonly AppDbContext _context;

    public CookiePolicyPublication(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public Task<bool> IsPublishedAsync(CancellationToken cancellationToken)
    {
        return _context.LegalTextVersions
            .AsNoTracking()
            .AnyAsync(l => l.LegalTextType == LegalTextType.CookiePolicy && l.PublishedAt != null, cancellationToken);
    }
}
