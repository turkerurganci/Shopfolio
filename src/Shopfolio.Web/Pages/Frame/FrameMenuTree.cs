using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts.Categories;

namespace Shopfolio.Pages.Frame;

/// <summary>One category of the menu with the categories under it (04 2.2.3, 2.2.4): at most three levels, the address of the category page ready.</summary>
public sealed record MenuCategoryNode(string Name, string Path, IReadOnlyList<MenuCategoryNode> Children);

/// <summary>
/// Turns the menu's flat category list - depth first, each branch in the alphabetic order of its names, <see cref="MenuCategory.Level"/> 1 at the root (02 3.28.6) - into the tree the menu draws
/// (T540; 04 2.2.3 "birinci seviye kategoriler ve altlarında ikinci ve üçüncü seviye"). The order of the list is the order of the tree; nothing is sorted again.
/// </summary>
public static class FrameMenuTree
{
    /// <summary>The tree of <paramref name="categories"/>; a category whose parent is not in the list (the list is the menu's own and complete) stands at the root.</summary>
    public static IReadOnlyList<MenuCategoryNode> Build(IReadOnlyList<MenuCategory> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);
        HashSet<Guid> known = [.. categories.Select(c => c.Id)];
        return Children(categories, null, known);
    }

    private static List<MenuCategoryNode> Children(IReadOnlyList<MenuCategory> all, Guid? parentId, HashSet<Guid> known) =>
        [.. all
            .Where(c => parentId is null ? c.ParentCategoryId is null || !known.Contains(c.ParentCategoryId.Value) : c.ParentCategoryId == parentId)
            .Select(c => new MenuCategoryNode(c.Name, AddressRules.RecordAddress(AddressRules.CategoryPrefix, c.Slug), Children(all, c.Id, known)))];
}
