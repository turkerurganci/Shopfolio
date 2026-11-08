namespace Shopfolio.Modules.Catalog.Contracts.Categories;

/// <summary>
/// What a category write came to (07 9.6.2-9.6.5; 02 3.4.1, 3.4.5, 3.4.6): written; the name is blank (422); the category or the new parent does not exist (404);
/// the branch would pass three levels (422); the category would go under its own sub-tree (422, the same error code as the depth - the reason is told in place);
/// or the category still holds a product or a sub-category (422). Not a stored value.
/// </summary>
public enum CategoryWriteOutcome
{
    Written,
    NameMissing,
    NotFound,
    ParentNotFound,
    DepthExceeded,
    IntoOwnSubtree,
    NotEmpty,
}
