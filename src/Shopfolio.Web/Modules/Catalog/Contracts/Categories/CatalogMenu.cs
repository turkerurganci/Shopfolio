namespace Shopfolio.Modules.Catalog.Contracts.Categories;

/// <summary>One category of the storefront menu (02 3.28.6): the level is 1 at the root, and the node stands in the menu because it or a category below it holds a published product.</summary>
public sealed record MenuCategory(Guid Id, Guid? ParentCategoryId, string Name, string Slug, int Level);

/// <summary>
/// What the catalog gives the storefront menu (02 3.28.4, 3.28.6): whether any product is published at all - without one the menu's products part does not show - and the categories
/// that show, depth first, each followed by its sub-categories in the Turkish alphabetical order of their names at every level. A category with no published product in itself or
/// in any branch below it is not in the list; its address still answers with an empty category page (02 3.28.6). Immutable, so the menu's cache can hold it (09 10.4 item 1).
/// </summary>
public sealed record CatalogMenu(bool HasListedProducts, IReadOnlyList<MenuCategory> Categories);
