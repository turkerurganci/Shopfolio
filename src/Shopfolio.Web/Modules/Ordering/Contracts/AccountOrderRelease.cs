namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// What the deletion of a customer account did on the order side (T345; 02 3.15.1-3.15.3): the number of the account's orders that were marked as an account's deleted one and cut from
/// the account (they go on with their own record and are never linked again), and the number of carts the account owned, which go with it.
/// </summary>
/// <param name="Orders">The orders marked.</param>
/// <param name="Carts">The account's carts (the cart is deleted with the account by the database).</param>
public sealed record AccountOrderRelease(int Orders, int Carts);
