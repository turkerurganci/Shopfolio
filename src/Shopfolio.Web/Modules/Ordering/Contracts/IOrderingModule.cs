namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The open surface of the Ordering module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns the cart, the order and its transitions (05 3.3.3).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3).
/// </summary>
public interface IOrderingModule
{
    /// <summary>
    /// The order a member opens from the member root <c>/siparis/{orderNumber}</c> (07 2.3.3, 2.3.4): the order with that number that belongs to that customer,
    /// or null. A number that does not exist, an order of another customer and a guest order (it belongs to no account; its way is the access-key root) all
    /// give null - the caller answers all of them with the same 404 (07 2.3.3). The number is matched without regard to letter case (07 2.1.7).
    /// </summary>
    Task<Guid?> FindMemberOrderAsync(string orderNumber, Guid customerId, CancellationToken ct);

    /// <summary>
    /// The order the access-key root <c>/siparis/erisim/{accessKey}</c> opens (07 2.3.4): the order that holds exactly that key, or null for an unknown, renewed,
    /// emptied or empty key. The key is case sensitive (07 2.1.7). The order is a guest's or a member's: the address opens for a member too (02 3.22.3).
    /// </summary>
    Task<Guid?> FindOrderByAccessKeyAsync(string accessKey, CancellationToken ct);
}
