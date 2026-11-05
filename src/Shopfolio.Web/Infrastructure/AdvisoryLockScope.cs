namespace Shopfolio.Infrastructure;

/// <summary>
/// The three scopes of the transaction-scoped advisory lock (09 10.2 item 5): the cart, by the cart's id (the order confirmation; 05 4.2.2),
/// the category tree, without a key (creating and moving a category; 06 4.9), and the administrator set, without a key (removing an
/// administrator; 05 6.1.2). Not stored.
/// </summary>
internal enum AdvisoryLockScope
{
    Cart,
    CategoryTree,
    AdminSet,
}
