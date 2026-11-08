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
    /// The retention arm of each of the given orders (06 9.1 Z-28, Z-41, Z-43; K-1030 (3), (4)), read from the payment record at this moment: whether the payment
    /// was taken, the moment an unpaid order failed, and whether the order's personal data is disposed. A plain read for the other modules' disposal parts; the
    /// order's own part reads the same way. An order that does not exist is not in the answer.
    /// </summary>
    Task<IReadOnlyList<OrderPaymentArm>> FindPaymentArmsAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct);

    /// <summary>The order each of the given items belongs to (06 4.18); an item that does not exist is not in the answer.</summary>
    Task<IReadOnlyList<OrderOfItem>> FindOrdersOfItemsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>
    /// The unpaid orders that failed before <paramref name="failedBefore"/> (06 9.1 Z-41), except the ones in <paramref name="exclude"/>, at most
    /// <paramref name="max"/>: the disposal part of a record that is kept ten years from the moment the order failed asks for them page by page.
    /// </summary>
    Task<IReadOnlyList<Guid>> FindUnpaidFailedOrdersAsync(DateTimeOffset failedBefore, IReadOnlyCollection<Guid> exclude, int max, CancellationToken ct);

    /// <summary>
    /// The guest orders of a verified address fall to the account that verified it (T341; 02 3.13.2): in the caller's open transaction, one conditional update of the orders that
    /// have no account, are not marked as an account's deleted one and carry the address as their contact e-mail (matched in its normalized form). An order that already belongs
    /// to an account, and the orders of a deleted account, are not taken. The link is a non-raising field: the order's version does not move (06 2.6.5). Returns the number of
    /// orders taken.
    /// </summary>
    Task<int> LinkGuestOrdersAsync(Guid customerId, string email, CancellationToken ct);

    /// <summary>
    /// Takes the cart items of the given variants off the cart (03 1.8.3; 02 3.16.7; 06 4.16): each unmarked item is marked with the product's name in the caller's
    /// operation - the operation that took the product or the variant off the showcase - so it no longer enters the order or the amounts and is told to the customer once.
    /// An item already marked keeps its first mark. Returns the number of items marked. Joins the caller's transaction (09 9.1 item 3).
    /// </summary>
    Task<int> RemoveFromCartsAsync(IReadOnlyCollection<Guid> variantIds, string productName, CancellationToken ct);

    /// <summary>
    /// The once-told message of the items taken off a cart (02 3.16.7; 06 4.16; K-1044 (3)): the product names of the marked items, which are deleted in the same call - the
    /// request that shows them is the request that removes them. Empty when the cart has none. Joins the caller's transaction and takes the cart's advisory lock first (09 10.2 item 5): a request that arrived together with this one waits, then finds none and tells nothing.
    /// </summary>
    Task<IReadOnlyList<string>> TakeRemovedItemNoticesAsync(Guid cartId, CancellationToken ct);

    /// <summary>
    /// The orders that hold a variant's reservation now (02 3.6.7; 06 4.14): the panel's stock field shows them beside the reserved quantity and a refused lowering names
    /// them. One row per order with its number and the reserved units, the most recent reservation first. A reservation lives only until payment or release, so these
    /// are exactly the orders whose payment is awaited. A plain read; it writes nothing.
    /// </summary>
    Task<IReadOnlyList<ReservationHolder>> ListReservationHoldersAsync(Guid variantId, CancellationToken ct);

    /// <summary>
    /// The open orders that hold a product (02 3.7.7; 06 4.17): the orders not in a terminal shipment state - payment awaited, or paid and not completed - with an item of this
    /// product, each counted once. The panel says it before a deletion is confirmed; it stops nothing. An order whose item link was emptied by an earlier deletion is not counted.
    /// A plain read; it writes nothing.
    /// </summary>
    Task<int> CountOpenOrdersOfProductAsync(Guid productId, CancellationToken ct);

    /// <summary>The open orders (as <see cref="CountOpenOrdersOfProductAsync"/>) with an item of this variant, each counted once. A plain read; it writes nothing.</summary>
    Task<int> CountOpenOrdersOfVariantAsync(Guid variantId, CancellationToken ct);

    /// <summary>
    /// What the card payment's final query needs (03 4.1.7; 08 2.1.6): the order's number and whether its window (Z-7) has ended at <paramref name="now"/>;
    /// empty when the order is not a card order that still waits for its payment - paid, cancelled, or a bank transfer. A plain read; it writes nothing.
    /// </summary>
    Task<CardPaymentQueryTarget?> GetCardPaymentQueryTargetAsync(Guid orderId, DateTimeOffset now, CancellationToken ct);
}

/// <summary>A card order that waits for its payment: its number, which the provider's query names, and whether its payment window has ended.</summary>
public sealed record CardPaymentQueryTarget(string OrderNumber, bool WindowEnded);
/// <summary>
/// An order's retention arm (06 9.1): <see cref="IsPaid"/> is true when the order has a payment record that is not corrected; <see cref="FailedAt"/> is the
/// moment of the Ö2 transition of an unpaid Failed order and null otherwise; <see cref="PersonalDataDisposed"/> says the order's disposal part has run.
/// </summary>
public sealed record OrderPaymentArm(Guid OrderId, bool IsPaid, DateTimeOffset? FailedAt, bool PersonalDataDisposed);

/// <summary>The order of an order item.</summary>
public sealed record OrderOfItem(Guid OrderItemId, Guid OrderId);

/// <summary>An order that holds units of a variant's stock or capacity: its id, its number and the reserved units.</summary>
public sealed record ReservationHolder(Guid OrderId, string OrderNumber, int Quantity);

/// <summary>The links a digital order item holds to its file (06 4.18): the variant, emptied when the variant is deleted, and the last known file.</summary>
public sealed record DownloadableItem(Guid? VariantId, Guid? DigitalFileId);
