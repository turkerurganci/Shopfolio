using Shopfolio.Modules.Catalog.Contracts;

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
    /// The order a payment notification names (08 2.1.5; 05 4.5.5): the order with exactly that number - the number the link layer mapped the provider's
    /// reference back to -, with its paid total and payment method, or null when there is none. A plain read without the order's lock: the notification
    /// endpoint only classifies and records, the order's state is written by the processing job through the transition unit (05 4.5.2, 4.5.3).
    /// </summary>
    Task<OrderForPayment?> FindOrderForPaymentAsync(string orderNumber, CancellationToken ct);

    /// <summary>
    /// What the download endpoint needs of an order item (07 4.31.1; 05 7.7.4): its variant link and last known file link, or null when there is no such item, it
    /// is not a digital item or its order's payment is not confirmed (02 3.12.1) - the caller answers all of them with the same 404. A plain read; it writes nothing
    /// and does not look at the download counter (the right dropped at the order page's button).
    /// </summary>
    Task<DownloadableItem?> FindDownloadableItemAsync(Guid orderItemId, CancellationToken ct);

    /// <summary>
    /// Takes one download from an order item's right (05 7.7.4; 02 3.12.5; 09 10.2 item 4): one conditional update that counts the download when the item is digital, its
    /// payment is confirmed and its counter is below the allowance frozen at the order. The right drops once, when the order page's button makes the link (T511); the
    /// transfer the link opens takes nothing more. Joins the caller's transaction and must run inside one.
    /// </summary>
    Task<DownloadTakeResult> TakeDownloadAsync(Guid orderItemId, CancellationToken ct);

    /// <summary>
    /// The firm renews an item's download right (07 UC-143; 02 3.12.6): under the order's row lock and against <paramref name="expectedVersion"/> the counter goes back to
    /// zero and the order's version rises by one. No notice goes out (02 9.2) and the allowance is not touched. Joins the caller's transaction and must run inside one.
    /// </summary>
    Task<DownloadRenewalResult> RenewDownloadAllowanceAsync(Guid orderItemId, int expectedVersion, CancellationToken ct);

    /// <summary>
    /// What a digital variant's add to a cart is asked (02 3.12.4, 3.12.9): the quantity is one, the variant is not already in the cart, and a member who bought it before
    /// is warned and not stopped; a guest has no past to check. A plain read; it writes nothing. A variant of another product type gets the neutral answer.
    /// </summary>
    Task<DigitalAddCheck> CheckDigitalAddAsync(DigitalAddRequest request, CancellationToken ct);

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
    /// The cart page's read (T346; 02 3.16): the owner's cart with its lines read live from the current product - the price that applies now, the "fiyatı değişti" mark, the "Tükendi" mark - and the
    /// once-told messages: the items taken off the showcase (deleted by this call) and the products the sign-in merge added (their mark is cleared by this call). A guest cart's last access moment is
    /// renewed. No cart is created by a read. The payment step is blocked when no line can enter an order (02 3.16.7). Takes the cart's advisory lock first and joins the caller's transaction (09 10.2
    /// item 5); it does not save. The cart never empties by itself: neither time, nor an open payment, nor a closed sale touches it (02 3.16.2).
    /// </summary>
    Task<CartView> OpenCartAsync(CartOwner owner, CancellationToken ct);

    /// <summary>
    /// Adds a variant to the owner's cart (02 3.16.1, 3.16.5): a new line remembers the unit price of this moment; the same variant again raises the quantity and leaves that price alone. The cart is
    /// made when it does not exist. A variant that is not on the showcase or is sold out is not added. The stock and per-order limits of the quantity are T347's. Joins the caller's transaction and
    /// does not save.
    /// </summary>
    Task<CartAddResult> AddToCartAsync(CartOwner owner, Guid variantId, int quantity, CancellationToken ct);

    /// <summary>
    /// The sign-in merge (T346; 02 3.16.3, 3.16.4): the guest browser's cart falls into the member's cart and ends. The same variant keeps the larger quantity and the price of the line added first; other
    /// variants stand side by side. Whatever the member's cart holds that the guest cart did not show - another product, or a larger quantity - is marked to be told once. One method for every sign-in path.
    /// It takes the guest cart's advisory lock first, so it runs in a transaction of its own and not in the one that holds the account's lock (05 6.1.2; <c>LockKind.Account</c> is a family of its own).
    /// Joins the caller's transaction and does not save. A key that matches no cart merges nothing.
    /// </summary>
    Task<CartMergeResult> MergeGuestCartAsync(Guid customerId, string guestCartKey, CancellationToken ct);

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

    /// <summary>
    /// What the customer's order page shows of the delivery (02 3.20.4-3.20.7; 04 5.16.8): the order's frozen shipping promise and its text, the moment the payment was confirmed, the moment
    /// the promise runs out, the moment of the shipment and the carrier display (the company and tracking number, the "Takip et" link only for a listed company, or the own-vehicle
    /// declaration alone). Null when there is no such order. A plain read; it writes nothing.
    /// </summary>
    Task<OrderShippingView?> GetShippingViewAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// Which of the given transition records a correction row points at (06 4.19 <c>CorrectedTransitionRecordId</c>), the caller's unsaved rows included: a
    /// cancellation whose closing transition is corrected is undone (06 4.22; T319). A plain read; it writes nothing.
    /// </summary>
    Task<IReadOnlySet<Guid>> FindCorrectedTransitionRecordsAsync(IReadOnlyCollection<Guid> transitionRecordIds, CancellationToken ct);

    /// <summary>
    /// The number of card orders that wait for their payment (T389; 05 8.5.6; 08 2.1.2): received, payment pending, paid by card - the payment form may still be open or the final query is
    /// waiting. These are the "open card job" part that lives in the order. A plain read; it counts and writes nothing.
    /// </summary>
    Task<int> CountCardOrdersAwaitingPaymentAsync(CancellationToken ct);

    /// <summary>
    /// The items of the orders paid by card whose payment is not fully refunded (T389; 05 8.5.6; 08 2.1.2): the facts the AfterSales module needs to tell which of them could still
    /// cause a card refund. A plain read; it writes nothing.
    /// </summary>
    Task<IReadOnlyList<PaidCardItem>> ListPaidCardItemsAsync(CancellationToken ct);
}

/// <summary>
/// One item of an order paid by card, as the status command's refund-exposure count reads it: the order and the item, the type, the ordered quantity and the delivery facts
/// (the physical item's delivery date, the mark of a service or digital item).
/// </summary>
public sealed record PaidCardItem(Guid OrderId, Guid OrderItemId, ProductType ProductType, int Quantity, DateOnly? DeliveryDate, DateTimeOffset? DeliveryMarkedAt);

/// <summary>A card order that waits for its payment: its number, which the provider's query names, and whether its payment window has ended.</summary>
public sealed record CardPaymentQueryTarget(string OrderNumber, bool WindowEnded);
/// <summary>
/// An order's retention arm (06 9.1): <see cref="IsPaid"/> is true when the order has a payment record that is not corrected; <see cref="FailedAt"/> is the
/// moment of the Ö2 transition of an unpaid Failed order and null otherwise; <see cref="PersonalDataDisposed"/> says the order's disposal part has run.
/// </summary>
public sealed record OrderPaymentArm(Guid OrderId, bool IsPaid, DateTimeOffset? FailedAt, bool PersonalDataDisposed);

/// <summary>The order a payment notification names: its id, its paid total (06 4.17 <c>Total</c>) and its payment method.</summary>
public sealed record OrderForPayment(Guid OrderId, decimal Total, Shopfolio.Modules.Payment.Contracts.PaymentMethod PaymentMethod);

/// <summary>The order of an order item.</summary>
public sealed record OrderOfItem(Guid OrderItemId, Guid OrderId);

/// <summary>An order that holds units of a variant's stock or capacity: its id, its number and the reserved units.</summary>
public sealed record ReservationHolder(Guid OrderId, string OrderNumber, int Quantity);

/// <summary>The links a digital order item holds to its file (06 4.18): the variant, emptied when the variant is deleted, and the last known file.</summary>
public sealed record DownloadableItem(Guid? VariantId, Guid? DigitalFileId);
