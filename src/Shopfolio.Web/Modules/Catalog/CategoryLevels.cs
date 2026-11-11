using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>A category as the tree rules see it: its key, its parent's key (empty at the root) and its level.</summary>
internal readonly record struct CategoryPlace(Guid Id, Guid? ParentId, int Level);

/// <summary>What a move comes to before anything is written (02 3.4.6).</summary>
internal enum CategoryMoveKind
{
    Allowed,
    CategoryNotFound,
    ParentNotFound,
    IntoOwnSubtree,
    DepthExceeded,
}

/// <summary>The verdict of a move; <see cref="NewLevels"/> holds the level every category of the moved sub-tree has afterwards (empty unless the move is allowed).</summary>
internal sealed record CategoryMoveVerdict(CategoryMoveKind Kind, IReadOnlyDictionary<Guid, int> NewLevels)
{
    public static CategoryMoveVerdict Refused(CategoryMoveKind kind) => new(kind, new Dictionary<Guid, int>());
}

/// <summary>
/// The rules of the three-level category tree as pure functions over the whole tree (02 3.4.1, 3.4.3, 3.4.6; 06 4.9; P-20). The depth comes from
/// <see cref="ProductConstants.CategoryLevelMax"/> only. The tree is a few dozen rows, read whole under the tree lock; a move's two limits - no branch past the depth, no category under
/// its own sub-tree - are judged on that reading, so two moves cannot each look right alone and break the limits together (06 4.9; K-1089). Sibling order is not here: it is the
/// database's Turkish order of the name column (06 2.8.3), never a stored number (02 3.4.1).
/// </summary>
internal static class CategoryLevels
{
    /// <summary>The level of a category created under a parent of level <paramref name="parentLevel"/> (empty parent: the root, level 1).</summary>
    public static int LevelUnder(int? parentLevel) => (parentLevel ?? 0) + 1;

    /// <summary>Whether a level is inside the tree's depth.</summary>
    public static bool FitsDepth(int level) => level >= 1 && level <= ProductConstants.CategoryLevelMax;

    /// <summary>The category and every category below it, the category first; the order is breadth first. A category the tree does not hold gives an empty list.</summary>
    public static IReadOnlyList<Guid> SubtreeOf(Guid root, IReadOnlyCollection<CategoryPlace> tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
        if (!tree.Any(c => c.Id == root))
        {
            return [];
        }

        ILookup<Guid, Guid> children = tree.Where(c => c.ParentId is not null).ToLookup(c => c.ParentId!.Value, c => c.Id); // non-null: the Where on this line keeps only categories with a parent
        List<Guid> found = [root];
        HashSet<Guid> seen = [root];
        for (int i = 0; i < found.Count; i++)
        {
            foreach (Guid child in children[found[i]])
            {
                if (seen.Add(child))
                {
                    found.Add(child);
                }
            }
        }

        return found;
    }

    /// <summary>Judges moving <paramref name="categoryId"/> under <paramref name="newParentId"/> (empty: to the root) in <paramref name="tree"/>.</summary>
    public static CategoryMoveVerdict CheckMove(Guid categoryId, Guid? newParentId, IReadOnlyCollection<CategoryPlace> tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
        Dictionary<Guid, CategoryPlace> byId = tree.ToDictionary(c => c.Id);
        if (!byId.TryGetValue(categoryId, out CategoryPlace node))
        {
            return CategoryMoveVerdict.Refused(CategoryMoveKind.CategoryNotFound);
        }

        int? parentLevel = null;
        if (newParentId is Guid parentId)
        {
            if (!byId.TryGetValue(parentId, out CategoryPlace parent))
            {
                return CategoryMoveVerdict.Refused(CategoryMoveKind.ParentNotFound);
            }

            IReadOnlyList<Guid> subtree = SubtreeOf(categoryId, tree);
            if (subtree.Contains(parentId))
            {
                return CategoryMoveVerdict.Refused(CategoryMoveKind.IntoOwnSubtree);
            }

            parentLevel = parent.Level;
        }

        int newLevel = LevelUnder(parentLevel);
        int shift = newLevel - node.Level;
        Dictionary<Guid, int> levels = SubtreeOf(categoryId, tree).ToDictionary(id => id, id => byId[id].Level + shift);
        if (levels.Values.Any(level => !FitsDepth(level)))
        {
            return CategoryMoveVerdict.Refused(CategoryMoveKind.DepthExceeded);
        }

        return new CategoryMoveVerdict(CategoryMoveKind.Allowed, levels);
    }
}
