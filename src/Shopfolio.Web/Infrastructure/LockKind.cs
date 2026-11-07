namespace Shopfolio.Infrastructure;

/// <summary>
/// The kinds of database lock an application transaction takes, in the order of 05 7.3.5: the cart's advisory lock, the order rows, the variant
/// rows, the coupon row. The category tree and the administrator set are their own families: a transaction that takes one of them takes no
/// order, variant or coupon lock; the company settings row, which the identity save, the settings sections and the generated legal text take (05 7.3.5;
/// K-1088), is a family of its own too. Not stored; the order of the members is the lock order.
/// </summary>
internal enum LockKind
{
    Cart,
    Order,
    Variant,
    Coupon,
    CategoryTree,
    AdminSet,
    CompanySettings,
}
