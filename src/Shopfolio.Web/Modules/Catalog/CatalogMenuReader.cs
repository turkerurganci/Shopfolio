using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Categories;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The catalog's part of the storefront menu (T361; 02 3.28.4, 3.28.6; 10 KP-1): the categories that hold a published product in themselves or in a branch below them, and whether
/// the catalog has a published product at all. A category's visibility follows the category page's own list - which takes the products of every branch below - so a category that
/// shows in the menu never opens an empty page and a parent whose only products hang on a sub-category shows. The order is the category tree's: the database's Turkish order of the
/// name at every level (02 3.4.1). A plain, untracked read that gives immutable records; it writes nothing.
/// </summary>
internal sealed class CatalogMenuReader
{
    private readonly AppDbContext _db;

    public CatalogMenuReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CatalogMenu> ReadAsync(CancellationToken ct)
    {
        if (!await _db.Products.AsNoTracking().Listed().AnyAsync(ct))
        {
            return new CatalogMenu(false, []);
        }

        var rows = await _db.Categories.AsNoTracking()
            .OrderBy(c => c.Name).ThenBy(c => c.Id)
            .Select(c => new { c.Id, c.ParentCategoryId, c.Name, c.Slug, c.Level })
            .ToListAsync(ct);
        List<Guid> holders = await _db.ProductCategories.AsNoTracking()
            .Where(l => _db.Products.Listed().Any(p => p.Id == l.ProductId))
            .Select(l => l.CategoryId)
            .Distinct()
            .ToListAsync(ct);

        // A category shows when it holds a published product itself or a branch below it does: walk up from each holder and mark its ancestors.
        Dictionary<Guid, Guid?> parents = rows.ToDictionary(r => r.Id, r => r.ParentCategoryId);
        HashSet<Guid> shown = [];
        foreach (Guid holder in holders)
        {
            Guid? current = holder;
            while (current is Guid id && parents.ContainsKey(id) && shown.Add(id))
            {
                current = parents[id];
            }
        }

        ILookup<Guid?, MenuCategory> byParent = rows
            .Where(r => shown.Contains(r.Id))
            .Select(r => new MenuCategory(r.Id, r.ParentCategoryId, r.Name, r.Slug, r.Level))
            .ToLookup(c => c.ParentCategoryId);

        List<MenuCategory> ordered = [];
        void Walk(Guid? parent)
        {
            foreach (MenuCategory category in byParent[parent])
            {
                ordered.Add(category);
                Walk(category.Id);
            }
        }

        Walk(null);
        return new CatalogMenu(true, ordered);
    }
}
