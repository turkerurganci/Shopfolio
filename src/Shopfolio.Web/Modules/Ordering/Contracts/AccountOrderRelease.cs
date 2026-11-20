namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// What the deletion of a customer account did on the order side (T345; 02 3.15.1-3.15.3): the number of the account's orders that were marked as an account's deleted one and cut from
/// the account (they go on with their own record and are never linked again). The account's carts go with the account by the database and are part of its disposal row (06 5.5.3; D-608).
/// </summary>
/// <param name="Orders">The orders marked.</param>
public sealed record AccountOrderRelease(int Orders);
