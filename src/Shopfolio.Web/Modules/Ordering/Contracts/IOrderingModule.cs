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
    /// The guest orders of a verified address fall to the account that verified it (T341; 02 3.13.2): in the caller's open transaction, one conditional update of the orders that
    /// have no account, are not marked as an account's deleted one and carry the address as their contact e-mail (matched in its normalized form). An order that already belongs
    /// to an account, and the orders of a deleted account, are not taken. The link is a non-raising field: the order's version does not move (06 2.6.5). Returns the number of
    /// orders taken.
    /// </summary>
    Task<int> LinkGuestOrdersAsync(Guid customerId, string email, CancellationToken ct);
}

/// <summary>The links a digital order item holds to its file (06 4.18): the variant, emptied when the variant is deleted, and the last known file.</summary>
public sealed record DownloadableItem(Guid? VariantId, Guid? DigitalFileId);
