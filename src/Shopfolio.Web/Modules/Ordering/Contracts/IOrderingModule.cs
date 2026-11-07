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

    /// <summary>
    /// What the download endpoint needs of an order item (07 4.31.1; 05 7.7.4): its variant link and last known file link, or null when there is no such item or it
    /// is not a digital item - the caller answers both with the same 404. A plain read; it writes nothing and does not look at the download counter (the right
    /// dropped at the order page's button).
    /// </summary>
    Task<DownloadableItem?> FindDownloadableItemAsync(Guid orderItemId, CancellationToken ct);

    /// <summary>
    /// Takes the cart items of the given variants off the cart (03 1.8.3; 02 3.16.7; 06 4.16): each unmarked item is marked with the product's name in the caller's
    /// operation - the operation that took the product or the variant off the showcase - so it no longer enters the order or the amounts and is told to the customer once.
    /// An item already marked keeps its first mark. Returns the number of items marked. Joins the caller's transaction (09 9.1 item 3).
    /// </summary>
    Task<int> RemoveFromCartsAsync(IReadOnlyCollection<Guid> variantIds, string productName, CancellationToken ct);

    /// <summary>
    /// The once-told message of the items taken off a cart (02 3.16.7; 06 4.16; K-1044 (3)): the product names of the marked items, which are deleted in the same call - the
    /// request that shows them is the request that removes them. Empty when the cart has none.
    /// </summary>
    Task<IReadOnlyList<string>> TakeRemovedItemNoticesAsync(Guid cartId, CancellationToken ct);
}

/// <summary>The links a digital order item holds to its file (06 4.18): the variant, emptied when the variant is deleted, and the last known file.</summary>
public sealed record DownloadableItem(Guid? VariantId, Guid? DigitalFileId);
