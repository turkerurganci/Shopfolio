namespace Shopfolio.Modules.Catalog.Contracts.Categories;

/// <summary>
/// One node of the category tree as the panel and the menu read it (02 3.4.1; 06 4.9; 07 9.6.1): the level is 1 at the root, <see cref="ChildCount"/> the direct
/// sub-categories and <see cref="ProductCount"/> the products hung directly on the node - the numbers the delete block (02 3.4.5) and the panel's row show.
/// </summary>
public sealed record CategoryNode(Guid Id, Guid? ParentCategoryId, string Name, string Slug, int Level, int ChildCount, int ProductCount);
