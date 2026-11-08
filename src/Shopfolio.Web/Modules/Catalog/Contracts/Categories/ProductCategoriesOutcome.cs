namespace Shopfolio.Modules.Catalog.Contracts.Categories;

/// <summary>
/// What the write of a product's categories came to (02 3.4.2, 3.4.4; 06 4.10): written; the product or a category does not exist; or the set breaks the main category
/// rule - a product that hangs on categories has exactly one main category and it is one of them, and a product on none has none. Not a stored value.
/// </summary>
public enum ProductCategoriesOutcome
{
    Written,
    ProductNotFound,
    CategoryNotFound,
    PrimaryMissing,
    PrimaryNotAmongCategories,
}
