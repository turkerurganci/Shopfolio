using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Settings.Contracts;

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
    /// The guest's lookup by number and e-mail (07 UC-32; 02 3.22.3, 3.22.5): the order whose number matches without regard to letter case and whose contact e-mail, as frozen
    /// on the order, matches in its normalized form - with the access key the caller turns into the 303 to the access-key root. The number alone opens nothing. An order of a
    /// deleted account, or of an account whose address changed, matches by its frozen address; an order whose personal data is disposed never matches. Any wrong or empty
    /// part gives the same null: the answer does not say which part failed or whether the order exists (K-772). A plain read; the failed-lookup limits (L-5) are the caller's
    /// attempt layer.
    /// </summary>
    Task<OrderAccessLookup?> FindOrderByNumberAndEmailAsync(string orderNumber, string email, CancellationToken ct);

    /// <summary>
    /// The status, the delivery area with its tracking link, the download buttons and the frozen bank transfer IBAN of an order the caller has already opened by one of the
    /// three ways (02 3.22.4); null when the order does not exist. Read from the order alone: no e-mail stands between the buyer and any of them (02 6.1.1). A plain read.
    /// </summary>
    Task<OrderPageSummary?> GetOrderPageSummaryAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// The customer's name, addresses and contact e-mail at the moment of confirmation (T355; 02 3.23.3; K-1027 (4), (5)): what the order's frozen pre-information form and distance sales
    /// contract show in their placeholders. A value the firm corrected later is read from the first row of its history, so the documents keep showing what the customer confirmed; null
    /// when the order does not exist. A plain read.
    /// </summary>
    Task<ConfirmationParties?> GetConfirmationPartiesAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// The invoice data of the given orders (T357; 02 3.25.1, 3.25.3): the values the firm issues the invoice from outside the product - the items' frozen amounts, the shipping fee and
    /// its frozen VAT breakdown, the billing address, the firm's identity frozen at confirmation (not the panel's current one), the payment method and approval, the hand-over moments,
    /// the carrier and the domain. The order export (T384) writes them to its file; the product issues and keeps no invoice. An id with no order is skipped. A plain read.
    /// </summary>
    Task<IReadOnlyList<InvoiceData>> ListInvoiceDataAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct);

    /// <summary>
    /// Writes the order export file's content to <paramref name="destination"/> (T384; 02 10.7.1; 09 10.10): a CSV with a header line and one line per order item of the orders created
    /// in the Turkey day range (both days included) whose payment was completed - the set the sales summary counts - with the invoice data, the contact e-mail and recipient name, the
    /// hand-over day and the late card payment and its refund. An order whose personal data is disposed keeps its lines with the name, e-mail and address cells empty. Returns the number of
    /// item lines (0: the range has no order of that set - the caller makes no file). The stream stays open; the writing stops with an exception when the file passes
    /// <paramref name="maxBytes"/>. A plain read; nothing is written to the records and no lock is taken.
    /// </summary>
    Task<int> WriteOrderExportAsync(DateOnly startDay, DateOnly endDay, Stream destination, long maxBytes, CancellationToken ct);

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
    /// What names each of the given items and its order in the panel's requests list (T532; 04 9.10.4): the order's id and number, the product name as the order recorded it and the item's option values.
    /// One read for all the items, whatever their number; an item that does not exist is not in the answer. A plain read.
    /// </summary>
    Task<IReadOnlyList<OrderItemLabel>> FindItemLabelsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

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
    /// The account's deletion on the order side (T345; 02 3.15.1-3.15.3; 05 6.1.2), in the caller's open transaction, under the account's row lock the caller holds: the account's orders
    /// are locked (05 7.3.2), then each is marked as an account's deleted one and its account link is emptied together (a check keeps the two together), so the order is never linked
    /// to any account again and goes on with its own record - status, contact e-mail and personal data stay until their legal period ends. The marks are a non-raising write: the order's
    /// version does not move (06 2.6.4). The account's carts are only counted: the database deletes them with the account. The caller saves; the method does not.
    /// </summary>
    Task<AccountOrderRelease> MarkAccountDeletedAsync(Guid customerId, CancellationToken ct);

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
    /// renewed. No cart is created by a read. The payment step is blocked when no line can enter an order (02 3.16.7) or when the basis is under the minimum order amount (02 3.18.1).
    /// <paramref name="shippingTerms"/> is the firm's shipping section as the caller read it (<c>ISettingsModule.GetShippingFeeAsync</c>; P-1, P-3, P-4): the view carries the fee, the free shipping
    /// message and the missing amount on the orderable basis (T349; 02 3.18, 3.19). Takes the cart's advisory lock first and joins the caller's transaction (09 10.2
    /// item 5); it does not save. The cart never empties by itself: neither time, nor an open payment, nor a closed sale touches it (02 3.16.2).
    /// </summary>
    Task<CartView> OpenCartAsync(CartOwner owner, ShippingFeeSection shippingTerms, CancellationToken ct);

    /// <summary>
    /// The cart page's read (T508; 07 UC-19): <see cref="OpenCartAsync"/>, and the cart's once-told "no payment method" mark (06 4.15; K-1183, K-1200) is taken - the view carries
    /// <see cref="CartView.ShowsPaymentUnavailableNotice"/> and the mark is cleared in the same call, so a reload does not tell it again. The confirmation's reads do not take the mark.
    /// Takes the cart's advisory lock first and joins the caller's transaction; it does not save.
    /// </summary>
    Task<CartView> OpenCartPageAsync(CartOwner owner, ShippingFeeSection shippingTerms, CancellationToken ct);

    /// <summary>
    /// Writes a cart line's quantity (T508; 07 UC-20; 03 2.3.3): the quantity is absolute - a repeat writes the same value - and the price of the first add does not move (02 3.16.5). A raise goes
    /// through the add's check under the cart's lock: a quantity the stock less the reserved units does not hold is NoStock, one past the product's limit over all its lines is OverOrderLimit (the
    /// limit wins); nothing is written then (02 3.16.8, 3.16.9). A lower quantity is always written. A line taken off the showcase writes nothing in either direction (K-1179 (3)); a line that is not
    /// this owner's is NotInCart. A digital line's quantity is one (02 3.12.4). Takes the cart's advisory lock first, joins the caller's transaction and does not save.
    /// </summary>
    Task<CartItemUpdateResult> UpdateCartItemAsync(CartOwner owner, Guid cartItemId, int quantity, CancellationToken ct);

    /// <summary>
    /// Removes a line from the owner's cart (T508; 07 UC-21): a line taken off the showcase too - the removal does not depend on the showcase (K-1209 (2)). A line that is not this owner's removes
    /// nothing. The result says whether the cart holds no line afterwards. Takes the cart's advisory lock first, joins the caller's transaction and does not save.
    /// </summary>
    Task<CartItemRemovalResult> RemoveCartItemAsync(CartOwner owner, Guid cartItemId, CancellationToken ct);

    /// <summary>
    /// Adds a variant to the owner's cart (02 3.16.1, 3.16.5): a new line remembers the unit price of this moment; the same variant again raises the quantity and leaves that price alone. The cart is
    /// made when it does not exist. A variant that is not on the showcase or is sold out is not added. A quantity the stock less the reserved units does not hold is refused as NoStock without a number; one that passes the product's per-order limit over all its variants is refused as OverOrderLimit with the limit, and the limit wins when both are broken (T347; 02 3.16.8, 3.16.9). Joins the caller's transaction and
    /// does not save.
    /// </summary>
    Task<CartAddResult> AddToCartAsync(CartOwner owner, Guid variantId, int quantity, CancellationToken ct);

    /// <summary>The id of the owner's cart, or null when the owner has none yet (T503): the product page asks it to tell a member that a digital variant is already in the cart. A plain read; it opens no cart.</summary>
    Task<Guid?> FindCartIdAsync(CartOwner owner, CancellationToken ct);

    /// <summary>
    /// The storefront frame's cart read (T537; 07 5.5.1): how many live lines the owner's cart holds and whether the sign-in merge left a message to tell once. A plain read - no lock, no cart
    /// made, no guest access moment renewed - and it never opens the cart's lines.
    /// </summary>
    Task<CartBadge> ReadCartBadgeAsync(CartOwner owner, CancellationToken ct);

    /// <summary>
    /// The once-told merge message of the frame (T537; 07 4.34.3; 02 3.16.4): the product names of the member's lines the sign-in merge marked, the marks cleared in the same call - the request
    /// that tells them is the request that clears them. Takes the cart's advisory lock first, so a request that arrived together waits, finds no mark and tells nothing. Joins the caller's
    /// transaction (09 10.2 item 5) and does not save.
    /// </summary>
    Task<IReadOnlyList<string>> TakeMergeNoticeAsync(Guid customerId, CancellationToken ct);

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
    /// What the card payment's final query needs (03 4.1.7; 08 2.1.6): the order's number, whether its window (Z-7) has ended at <paramref name="now"/> and its total;
    /// empty when the order is not a card order that still waits for its payment - paid, cancelled, or a bank transfer. A plain read; it writes nothing.
    /// </summary>
    Task<CardPaymentQueryTarget?> GetCardPaymentQueryTargetAsync(Guid orderId, DateTimeOffset now, CancellationToken ct);

    /// <summary>The order's number, the card refund's query asks by it (T405; 08 2.1.9); null when there is no such order. A plain read.</summary>
    Task<string?> GetOrderNumberAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// What the installer's <c>apply-payment-result</c> command needs (T387; 05 8.5.12): the order with exactly that number, its paid total (06 4.17 <c>Total</c>), whether it is a card
    /// order and whether it carries the "payment outcome unknown" mark at <paramref name="now"/> (the rule of the final query's handler and the panel - <c>PaymentWindowRules</c>); null
    /// when there is no such order. A plain read; the command writes through the transition unit, which locks the order.
    /// </summary>
    Task<InstallerPaymentTarget?> FindInstallerPaymentTargetAsync(string orderNumber, DateTimeOffset now, CancellationToken ct);

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
    /// cause a card refund. Read one page at a time: the items of the next <paramref name="maxOrders"/> such orders after <paramref name="afterOrderId"/> (by order id; null starts
    /// from the first), an empty list at the end - the set grows with every paid order, so it is never taken whole (09 20 item 2). A plain read; it writes nothing.
    /// </summary>
    Task<IReadOnlyList<PaidCardItem>> ListPaidCardItemsAsync(Guid? afterOrderId, int maxOrders, CancellationToken ct);

    /// <summary>
    /// The order facts of the sales summary's period <c>[periodStart, periodEnd)</c> (T383; 02 10.6.2, 10.6.3): the approved orders born in it, their totals, methods, products and items, the
    /// orders of the carts that had an order born in it and the shipping promises that ran out in it by <paramref name="now"/>. A plain read at the moment of the call; it keeps
    /// and writes nothing.
    /// </summary>
    Task<SalesPeriodFacts> ReadSalesPeriodAsync(DateTimeOffset periodStart, DateTimeOffset periodEnd, DateTimeOffset now, CancellationToken ct);

    /// <summary>
    /// The orders bound to the member account <paramref name="customerId"/>, newest first, one page of <c>PageSizes.Panel</c> (T533; 07 UC-95; 04 2.13.3); a page past the last shows the last.
    /// A plain read; it writes nothing.
    /// </summary>
    Task<CustomerOrderPage> ListCustomerOrdersAsync(Guid customerId, int page, CancellationToken ct);

    /// <summary>
    /// The member's own order history (T518; 07 UC-74; 04 5.27; 02 3.17.8): the orders bound to the account that the customer's list shows - a never-paid order the system cancelled by itself is
    /// not among them (<c>OrderVisibility</c>) -, newest first, one page of <c>PageSizes.OrderHistory</c>; each row carries the first item's name and the count of the others. A page past the last
    /// shows the last. A plain read; it writes nothing. The panel's member record reads <see cref="ListCustomerOrdersAsync"/>, which shows every order.
    /// </summary>
    Task<MemberOrderPage> ListMemberOrdersAsync(Guid customerId, int page, CancellationToken ct);

    /// <summary>The ids of the orders of one of the two sets <see cref="CountPendingOrdersAsync"/> counts (T527; 04 9.8.4): the list's filter and the home's counter share the rule. A plain read.</summary>
    Task<IReadOnlyList<Guid>> ListPendingOrderIdsAsync(PendingOrderKind kind, CancellationToken ct);

    /// <summary>
    /// One page of the panel's order list (T527; 07 UC-91; 04 9.8): the orders the filter keeps - the auto-cancelled unpaid ones included (04 9.8.12) -, newest first, <paramref name="pageSize"/> a page; a page past
    /// the last shows the last. A plain read; the items of the page are one read (09 20 item 4).
    /// </summary>
    Task<PanelOrderPage> ListPanelOrdersAsync(PanelOrderFilter filter, int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// The orders the filter keeps inside its pending work set (<see cref="PanelOrderFilter.Within"/> is required), newest first: a timed pending work filter sorts them by the remaining period
    /// outside the database (04 9.8.7) and cuts its page from them. A plain read.
    /// </summary>
    Task<IReadOnlyList<PanelOrderKey>> FindPanelOrderKeysAsync(PanelOrderFilter filter, CancellationToken ct);

    /// <summary>The panel list rows of the given orders, in the given order; an id with no order is not in the answer. A plain read of two queries whatever the number of ids.</summary>
    Task<IReadOnlyList<PanelOrderRow>> ReadPanelOrderRowsAsync(IReadOnlyList<Guid> orderIds, CancellationToken ct);

    /// <summary>The orders of the given items whose payment was approved (T383; 02 10.6.3); the items of a Failed or pending order are left out. A plain read.</summary>
    Task<IReadOnlyList<OrderOfItem>> FindPaidOrdersOfItemsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);

    /// <summary>
    /// The panel home's two counts the orders alone decide (T382; 02 10.6.1): the bank transfer orders that wait for their payment approval and the Teslim edilemedi orders. A plain read
    /// that counts; nothing is kept or written.
    /// </summary>
    Task<PendingOrderCounts> CountPendingOrdersAsync(CancellationToken ct);

    /// <summary>
    /// One page of a pending work set (T382; 02 10.6.1): the items of the next <paramref name="maxOrders"/> orders of the set after <paramref name="afterOrderId"/> (by order id; null starts
    /// from the first), an empty list at the end - the set grows with the shop, so it is never taken whole (09 20 item 2). The open quantity is the caller's to decide. A plain read.
    /// </summary>
    Task<IReadOnlyList<PendingWorkItem>> ListPendingWorkItemsAsync(PendingWorkSet workSet, Guid? afterOrderId, int maxOrders, CancellationToken ct);

    /// <summary>The refund work's facts of the given items and their orders (T382; 02 10.6.1): type, delivery date, payment way and state, and whether the order was ever shipped. A plain read.</summary>
    Task<IReadOnlyList<RefundWorkItemFacts>> FindRefundWorkItemFactsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct);
}

/// <summary>
/// One item of an order paid by card, as the status command's refund-exposure count reads it: the order and the item, the type, the ordered quantity and the delivery facts
/// (the physical item's delivery date, the mark of a service or digital item).
/// </summary>
public sealed record PaidCardItem(Guid OrderId, Guid OrderItemId, ProductType ProductType, int Quantity, DateOnly? DeliveryDate, DateTimeOffset? DeliveryMarkedAt);

/// <summary>
/// A card order that waits for its payment: its number, which the provider's query names, whether its payment window has ended, and its total, which a successful
/// answer's amounts are compared with (08 2.1.6; T403).
/// </summary>
public sealed record CardPaymentQueryTarget(string OrderNumber, bool WindowEnded, decimal Total);

/// <summary>The order the installer's payment result names (05 8.5.12): its id and number, its paid total, whether it is a card order and whether it carries the "payment outcome unknown" mark.</summary>
public sealed record InstallerPaymentTarget(Guid OrderId, string OrderNumber, decimal Total, bool IsCard, bool PaymentOutcomeUnknown);
/// <summary>
/// An order's retention arm (06 9.1): <see cref="IsPaid"/> is true when the order has a payment record that is not corrected; <see cref="FailedAt"/> is the
/// moment of the Ö2 transition of an unpaid Failed order and null otherwise; <see cref="PersonalDataDisposed"/> says the order's disposal part has run.
/// </summary>
public sealed record OrderPaymentArm(Guid OrderId, bool IsPaid, DateTimeOffset? FailedAt, bool PersonalDataDisposed);

/// <summary>The order a payment notification names: its id, its paid total (06 4.17 <c>Total</c>) and its payment method.</summary>
public sealed record OrderForPayment(Guid OrderId, decimal Total, Shopfolio.Modules.Payment.Contracts.PaymentMethod PaymentMethod);

/// <summary>The order of an order item.</summary>
public sealed record OrderOfItem(Guid OrderItemId, Guid OrderId);

/// <summary>The labels of an order item for a list that names it (T532): the item's order and its number, the product name the order recorded and the option values the item was bought with (06 4.18).</summary>
public sealed record OrderItemLabel(Guid OrderItemId, Guid OrderId, string OrderNumber, string ProductName, string? FirstOptionValue, string? SecondOptionValue);

/// <summary>An order that holds units of a variant's stock or capacity: its id, its number and the reserved units.</summary>
public sealed record ReservationHolder(Guid OrderId, string OrderNumber, int Quantity);

/// <summary>The links a digital order item holds to its file (06 4.18): the variant, emptied when the variant is deleted, and the last known file.</summary>
public sealed record DownloadableItem(Guid? VariantId, Guid? DigitalFileId);
