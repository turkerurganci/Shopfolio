using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The CorporateContent module's part of the site map (T313; 05 3.7.3; 02 3.30.5, 3.30.6): the address of every published service introduction, reference work
/// and general page, and the about page's fixed address when it is published. A draft is not read. The read is untracked and gives plain paths (09 10.4 item 1).
/// </summary>
internal sealed class CorporateContentSitemapReader
{
    /// <summary>The about page's fixed address (07 2.1.2).</summary>
    public static readonly string AboutPagePath = "/" + AddressRules.AboutPage;

    private readonly AppDbContext _db;

    public CorporateContentSitemapReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<string>> ListAsync(CancellationToken ct)
    {
        List<string> offerings = await _db.Offerings.AsNoTracking()
            .Where(o => o.PublishStatus == PublishStatus.Published)
            .OrderBy(o => o.Slug).ThenBy(o => o.Id)
            .Select(o => o.Slug)
            .ToListAsync(ct);
        List<string> portfolio = await _db.PortfolioItems.AsNoTracking()
            .Where(p => p.PublishStatus == PublishStatus.Published)
            .OrderBy(p => p.Slug).ThenBy(p => p.Id)
            .Select(p => p.Slug)
            .ToListAsync(ct);
        List<string> pages = await _db.CustomPages.AsNoTracking()
            .Where(c => c.PublishStatus == PublishStatus.Published)
            .OrderBy(c => c.Slug).ThenBy(c => c.Id)
            .Select(c => c.Slug)
            .ToListAsync(ct);
        bool aboutPublished = await _db.AboutPages.AsNoTracking().AnyAsync(a => a.PublishStatus == PublishStatus.Published, ct);

        List<string> paths =
        [
            .. offerings.Select(slug => AddressRules.RecordAddress(AddressRules.ServicePrefix, slug)),
            .. portfolio.Select(slug => AddressRules.RecordAddress(AddressRules.ReferencePrefix, slug)),
            .. pages.Select(slug => AddressRules.RecordAddress(AddressRules.GeneralPagePrefix, slug)),
        ];
        if (aboutPublished)
        {
            paths.Add(AboutPagePath);
        }

        return paths;
    }
}
