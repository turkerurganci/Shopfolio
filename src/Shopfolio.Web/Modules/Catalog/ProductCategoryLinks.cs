using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Categories;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The categories a product hangs on (02 3.4.2, 3.4.4; 06 4.10): any node of the tree, any number of them, and exactly one of them is the main category - or none, when the product
/// hangs on none. The link belongs to the product form, so the product's version rises when the set changes (06 2.6.2). The main category moves in two steps: the old mark is cleared
/// by one statement before the new one is written, because the filtered unique index lets a product hold only one mark at any moment. Whether a product being published has a main
/// category is the publication gate's (T335), not read here. The method joins the caller's transaction and never opens one (09 9.1 item 3).
/// </summary>
internal sealed class ProductCategoryLinks
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public ProductCategoryLinks(AppDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public async Task<ProductCategoriesResult> SetAsync(Guid productId, IReadOnlyCollection<Guid> categoryIds, Guid? primaryCategoryId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(categoryIds);
        HashSet<Guid> wanted = [.. categoryIds];

        Product? product = await _db.Products.SingleOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null)
        {
            return ProductCategoriesResult.Refused(ProductCategoriesOutcome.ProductNotFound);
        }

        int known = await _db.Categories.CountAsync(c => wanted.Contains(c.Id), ct);
        if (known != wanted.Count)
        {
            return ProductCategoriesResult.Refused(ProductCategoriesOutcome.CategoryNotFound);
        }

        if (wanted.Count > 0 && primaryCategoryId is null)
        {
            return ProductCategoriesResult.Refused(ProductCategoriesOutcome.PrimaryMissing);
        }

        if (primaryCategoryId is Guid primary && !wanted.Contains(primary))
        {
            return ProductCategoriesResult.Refused(ProductCategoriesOutcome.PrimaryNotAmongCategories);
        }

        List<ProductCategory> held = await _db.ProductCategories.AsNoTracking().Where(l => l.ProductId == productId).ToListAsync(ct);
        List<Guid> toRemove = [.. held.Where(l => !wanted.Contains(l.CategoryId)).Select(l => l.Id)];
        List<Guid> toAdd = [.. wanted.Where(id => held.All(l => l.CategoryId != id))];
        bool markChanges = held.Any(l => l.IsPrimaryCategory != (l.CategoryId == primaryCategoryId) && wanted.Contains(l.CategoryId));
        if (toRemove.Count == 0 && toAdd.Count == 0 && !markChanges)
        {
            return ProductCategoriesResult.Written;
        }

        // The mark is cleared first, the links that go are deleted, the mark is set on a link that stays; a new link is added at the save with its mark already right.
        await _db.ProductCategories.Where(l => l.ProductId == productId && l.IsPrimaryCategory && l.CategoryId != primaryCategoryId)
            .ExecuteUpdateAsync(set => set.SetProperty(l => l.IsPrimaryCategory, false), ct);
        await _db.ProductCategories.Where(l => toRemove.Contains(l.Id)).ExecuteDeleteAsync(ct);
        if (primaryCategoryId is Guid kept && held.Any(l => l.CategoryId == kept))
        {
            await _db.ProductCategories.Where(l => l.ProductId == productId && l.CategoryId == kept && !l.IsPrimaryCategory)
                .ExecuteUpdateAsync(set => set.SetProperty(l => l.IsPrimaryCategory, true), ct);
        }

        DateTimeOffset now = _time.GetUtcNow();
        foreach (Guid categoryId in toAdd)
        {
            _db.ProductCategories.Add(ProductCategory.Create(productId, categoryId, categoryId == primaryCategoryId, now));
        }

        product.BumpVersion();
        return ProductCategoriesResult.Written;
    }
}
