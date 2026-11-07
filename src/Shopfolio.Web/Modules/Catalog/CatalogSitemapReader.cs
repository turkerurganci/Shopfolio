using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The Catalog module's part of the site map (T313; 05 3.7.3; 02 3.30.5, 3.30.6): the address of every published product and of every category. An archived product
/// and a draft are not read - the archived page answers by its direct address but is closed to the search engines and stays out of the map. The read is
/// untracked and gives plain paths; the cache of the whole map is not here (it holds both modules' paths, 09 10.4 item 1).
/// </summary>
internal sealed class CatalogSitemapReader
{
    private readonly AppDbContext _db;

    public CatalogSitemapReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<string>> ListAsync(CancellationToken ct)
    {
        List<string> products = await _db.Products.AsNoTracking()
            .Listed()
            .OrderBy(p => p.Slug).ThenBy(p => p.Id)
            .Select(p => p.Slug)
            .ToListAsync(ct);
        List<string> categories = await _db.Categories.AsNoTracking()
            .OrderBy(c => c.Slug).ThenBy(c => c.Id)
            .Select(c => c.Slug)
            .ToListAsync(ct);

        return
        [
            .. products.Select(slug => AddressRules.RecordAddress(AddressRules.ProductPrefix, slug)),
            .. categories.Select(slug => AddressRules.RecordAddress(AddressRules.CategoryPrefix, slug)),
        ];
    }
}
