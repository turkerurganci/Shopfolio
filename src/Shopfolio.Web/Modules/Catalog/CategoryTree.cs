using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Categories;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The writes and the read of the category tree (02 3.4.1-3.4.6; 06 4.9; 07 9.6; 09 9.4 item 4): the only place that gives a category its level. Create, move and delete take the
/// tree's advisory lock as the first statement of the caller's transaction (05 7.3.5; K-1089), read the whole tree in one query and judge the limits on that reading
/// (<see cref="CategoryLevels"/>); a move writes the level of every category of the moved branch in the same save as the new parent. A refusal is a returned value - nothing is
/// written before it - and the methods join the caller's transaction, never open one (09 9.1 item 3). Sibling order is never stored: reads sort the name column, whose order
/// is the database's Turkish one (06 2.8.3; 02 3.4.1). The address is made from the name when the category is created and stays (02 3.30.2); a rename keeps it.
/// </summary>
internal sealed class CategoryTree
{
    private readonly AppDbContext _db;
    private readonly AdvisoryLocks _locks;
    private readonly CacheInvalidations _invalidations;
    private readonly TimeProvider _time;

    public CategoryTree(AppDbContext db, AdvisoryLocks locks, CacheInvalidations invalidations, TimeProvider time)
    {
        _db = db;
        _locks = locks;
        _invalidations = invalidations;
        _time = time;
    }

    /// <summary>Creates a category at the root (<paramref name="parentCategoryId"/> empty) or under a parent (07 9.6.2): blank name, missing parent or a fourth level are refused.</summary>
    public async Task<CategoryWriteResult> CreateAsync(string? name, Guid? parentCategoryId, CancellationToken ct)
    {
        await _locks.LockAsync(AdvisoryLockScope.CategoryTree, null, ct);

        string trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return CategoryWriteResult.Refused(CategoryWriteOutcome.NameMissing);
        }

        int? parentLevel = null;
        if (parentCategoryId is Guid parentId)
        {
            parentLevel = await _db.Categories.AsNoTracking().Where(c => c.Id == parentId).Select(c => (int?)c.Level).SingleOrDefaultAsync(ct);
            if (parentLevel is null)
            {
                return CategoryWriteResult.Refused(CategoryWriteOutcome.ParentNotFound);
            }
        }

        int level = CategoryLevels.LevelUnder(parentLevel);
        if (!CategoryLevels.FitsDepth(level))
        {
            return CategoryWriteResult.Refused(CategoryWriteOutcome.DepthExceeded);
        }

        string baseSlug = Slug.From(trimmed, "kategori");
        List<string> held = await _db.Categories.AsNoTracking().Where(c => c.Slug.StartsWith(baseSlug)).Select(c => c.Slug).ToListAsync(ct);
        string slug = Slug.FirstFree(baseSlug, held.Where(s => Slug.IsBaseOrNumbered(baseSlug, s)));

        Category category = Category.Create(trimmed, slug, parentCategoryId, level, _time.GetUtcNow());
        _db.Categories.Add(category);
        Invalidate(addressChanges: true);
        return CategoryWriteResult.Written(category.Id);
    }

    /// <summary>Renames a category (07 9.6.3): the address and the level stay, the place among its siblings follows the new name. A plain field edit - no tree lock.</summary>
    public async Task<CategoryWriteResult> RenameAsync(Guid categoryId, string? name, CancellationToken ct)
    {
        string trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return CategoryWriteResult.Refused(CategoryWriteOutcome.NameMissing, categoryId);
        }

        Category? category = await _db.Categories.SingleOrDefaultAsync(c => c.Id == categoryId, ct);
        if (category is null)
        {
            return CategoryWriteResult.Refused(CategoryWriteOutcome.NotFound);
        }

        category.Name = trimmed;
        Invalidate(addressChanges: false);
        return CategoryWriteResult.Written(categoryId);
    }

    /// <summary>
    /// Moves a category with its whole branch under another parent or to the root (07 9.6.4; 02 3.4.6). Refused, with nothing written, when the category or the parent is missing,
    /// when the parent is the category itself or inside its branch, or when any category of the branch would pass the depth. Moving under the parent it already has is allowed
    /// and writes the same values (the new parent is absolute - 07 9.6.4).
    /// </summary>
    public async Task<CategoryWriteResult> MoveAsync(Guid categoryId, Guid? newParentCategoryId, CancellationToken ct)
    {
        await _locks.LockAsync(AdvisoryLockScope.CategoryTree, null, ct);

        List<Category> tree = await _db.Categories.ToListAsync(ct);
        CategoryMoveVerdict verdict = CategoryLevels.CheckMove(categoryId, newParentCategoryId, [.. tree.Select(c => new CategoryPlace(c.Id, c.ParentCategoryId, c.Level))]);
        switch (verdict.Kind)
        {
            case CategoryMoveKind.CategoryNotFound:
                return CategoryWriteResult.Refused(CategoryWriteOutcome.NotFound);
            case CategoryMoveKind.ParentNotFound:
                return CategoryWriteResult.Refused(CategoryWriteOutcome.ParentNotFound, categoryId);
            case CategoryMoveKind.IntoOwnSubtree:
                return CategoryWriteResult.Refused(CategoryWriteOutcome.IntoOwnSubtree, categoryId);
            case CategoryMoveKind.DepthExceeded:
                return CategoryWriteResult.Refused(CategoryWriteOutcome.DepthExceeded, categoryId);
        }

        foreach (Category category in tree.Where(c => verdict.NewLevels.ContainsKey(c.Id)))
        {
            Guid? parent = category.Id == categoryId ? newParentCategoryId : category.ParentCategoryId;
            category.Relocate(parent, verdict.NewLevels[category.Id]);
        }

        Invalidate(addressChanges: false);
        return CategoryWriteResult.Written(categoryId);
    }

    /// <summary>
    /// Deletes a category that holds nothing (07 9.6.5; 02 3.4.5): a category with a sub-category or a product is refused with the two numbers, and the foreign keys stay the last
    /// defense. The address is free again once the row is gone (02 3.30.2).
    /// </summary>
    public async Task<CategoryWriteResult> DeleteAsync(Guid categoryId, CancellationToken ct)
    {
        await _locks.LockAsync(AdvisoryLockScope.CategoryTree, null, ct);

        Category? category = await _db.Categories.SingleOrDefaultAsync(c => c.Id == categoryId, ct);
        if (category is null)
        {
            return CategoryWriteResult.Refused(CategoryWriteOutcome.NotFound);
        }

        int children = await _db.Categories.CountAsync(c => c.ParentCategoryId == categoryId, ct);
        int products = await _db.ProductCategories.CountAsync(p => p.CategoryId == categoryId, ct);
        if (children > 0 || products > 0)
        {
            return CategoryWriteResult.NotEmpty(categoryId, children, products);
        }

        _db.Categories.Remove(category);
        Invalidate(addressChanges: true);
        return CategoryWriteResult.Written(categoryId);
    }

    /// <summary>
    /// The whole tree depth first: a node, then its sub-categories in the Turkish alphabetical order of their names, at every level (02 3.4.1). Each node carries its direct
    /// sub-category and product counts. A plain read - the order comes from the database, not from a number.
    /// </summary>
    public async Task<IReadOnlyList<CategoryNode>> ReadAsync(CancellationToken ct)
    {
        var rows = await _db.Categories.AsNoTracking()
            .OrderBy(c => c.Name).ThenBy(c => c.Id)
            .Select(c => new
            {
                c.Id,
                c.ParentCategoryId,
                c.Name,
                c.Slug,
                c.Level,
                ChildCount = _db.Categories.Count(k => k.ParentCategoryId == c.Id),
                ProductCount = _db.ProductCategories.Count(p => p.CategoryId == c.Id),
            })
            .ToListAsync(ct);

        ILookup<Guid?, CategoryNode> byParent = rows
            .Select(r => new CategoryNode(r.Id, r.ParentCategoryId, r.Name, r.Slug, r.Level, r.ChildCount, r.ProductCount))
            .ToLookup(n => n.ParentCategoryId);

        List<CategoryNode> ordered = [];
        void Walk(Guid? parent)
        {
            foreach (CategoryNode node in byParent[parent])
            {
                ordered.Add(node);
                Walk(node.Id);
            }
        }

        Walk(null);
        return ordered;
    }

    private void Invalidate(bool addressChanges)
    {
        _invalidations.Add(CacheArea.Menu);
        _invalidations.Add(CacheArea.CategoryTree);
        if (addressChanges)
        {
            _invalidations.Add(CacheArea.Sitemap);
        }
    }
}
