using System.Net;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The payment step's values that make the summary (T509; 07 UC-22…UC-25; 02 3.24.3): the cart's owner, the payment method, the coupon code, the guest's e-mail and the two addresses as far as
/// they were written. A member's e-mail is the account's and is not sent. Nothing here is validated: the read only evaluates what was written.
/// </summary>
public sealed record CheckoutInput(
    CartOwner Owner,
    PaymentMethod PaymentMethod,
    string? CouponCode,
    string? ContactEmail,
    PartyAddress? Shipping,
    PartyAddress? Billing);

/// <summary>
/// The payment step as the server judges it now (T509; 07 UC-22): <see cref="Block"/> is the cart's own way to the step (no line enters an order, or a physical cart under the minimum order
/// amount) - the sales gate is the page's to ask -; <see cref="Summary"/> is empty when no line enters an order; <see cref="Coupon"/> is the code's verdict, empty without a code;
/// <see cref="EarlierUnpaidOrderNumber"/> is the cart's unpaid order the confirmation cancels (K-837); <see cref="HasPhysicalItem"/> opens the shipping section (02 3.14.4).
/// </summary>
public sealed record CheckoutStep(
    CartPaymentBlock Block,
    ConfirmationSummary? Summary,
    CouponApplication? Coupon,
    string? EarlierUnpaidOrderNumber,
    bool HasPhysicalItem) : IOperationResult
{
    /// <summary>The read writes only the cart's marks and, on UC-23, the coupon's attempt (L-6): both are kept.</summary>
    public bool IsRejected => false;
}

/// <summary>
/// What the confirmation sends (T509; 07 UC-27): the confirmation key of the form shown, the summary the customer saw (the fingerprint the form carried), the legal text versions shown and the
/// boxes ticked, the coupon code, the guest's e-mail (a member's is the account's), the address part as the membership module reads it and the client address.
/// </summary>
public sealed record PlaceOrderRequest(
    CartOwner Owner,
    string ConfirmationKey,
    ConfirmationSummary ConfirmedSummary,
    Guid ShownPreInformationFormId,
    Guid ShownDistanceSalesContractId,
    TickedBoxes Ticked,
    string? CouponCode,
    string? GuestEmail,
    OrderAddressRequest Addresses,
    IPAddress IpAddress)
{
    /// <summary>
    /// The unique indexes of the order table that a clash of a made value breaks - the number's and the access key's - in the form the transaction runner takes (09 9.4 item 3;
    /// D-563): the payment step runs the confirmation with them, so a clash runs the whole work again.
    /// </summary>
    public static IReadOnlyCollection<string> RetryOnUniqueIndexes { get; } = ["ix_orders_order_number", "ix_orders_order_access_key"];
}

/// <summary>The confirmation's outcome (T509; 07 4.13.6): the answer the page gives for each. A result code, never stored.</summary>
public enum PlaceOrderOutcome
{
    /// <summary>The order is born: 303 to its confirmation screen.</summary>
    Placed,

    /// <summary>An order was already born with the same confirmation key: 303 to that order's confirmation screen; nothing is written (05 4.2.2 (0); K-1199).</summary>
    AlreadyPlaced,

    /// <summary>The address part refused fields - all of them together, the undelivered shipping province among them: 422.</summary>
    AddressRefused,

    /// <summary>The member's chosen book entry is not theirs: 404 (07 2.3.3).</summary>
    AddressBookEntryNotFound,

    /// <summary>The signed-in account cannot place an order (not verified, 02 3.13.1): 422 with the summary's message.</summary>
    AccountNotAccepted,

    /// <summary>No line enters an order any more: 303 to the cart.</summary>
    NoOrderableItem,

    /// <summary>The physical cart fell under the minimum order amount: 303 to the cart (K-1229).</summary>
    MinimumOrderAmountNotMet,

    /// <summary>The summary changed: 422 with the change lines, the current summary and texts (02 3.17.2).</summary>
    SummaryChanged,

    /// <summary>A legal text's version rose or a needed box is unticked: 422 (02 3.24.1).</summary>
    ConfirmationStepNotReady,

    /// <summary>The chosen payment method is not open: 422 (07 4.13.6).</summary>
    PaymentMethodUnavailable,

    /// <summary>The sale closed while the step was open: 422 with the message beside the button (04 5.13.27).</summary>
    SalesClosed,

    /// <summary>L-7, or L-8 on the bank transfer: 429 with the neutral message (02 6.1.5).</summary>
    AttemptLimitExceeded,
}

/// <summary>
/// The confirmation's answer (T509): the order's number and access key with its guest mark - they pick the confirmation screen's root (07 2.1.3) - for <see cref="PlaceOrderOutcome.Placed"/>
/// and <see cref="PlaceOrderOutcome.AlreadyPlaced"/>; the current summary, the change lines and the step's verdict for a refusal; the address fields refused; the limit that stopped the order,
/// for the log line only. Every outcome other than <see cref="PlaceOrderOutcome.Placed"/> rolls the transaction back (09 9.1 item 3).
/// </summary>
public sealed record PlaceOrderResult(
    PlaceOrderOutcome Outcome,
    string? OrderNumber = null,
    string? OrderAccessKey = null,
    bool IsGuestOrder = false,
    ConfirmationSummary? CurrentSummary = null,
    IReadOnlyList<SummaryChange>? Changes = null,
    ConfirmationStepCheck? StepCheck = null,
    IReadOnlyList<MembershipFieldError>? AddressFields = null,
    AttemptLimit? ExceededLimit = null) : IOperationResult
{
    public bool IsRejected => Outcome != PlaceOrderOutcome.Placed;
}

/// <summary>
/// The payment step's two operations (T509; 07 UC-22…UC-27). Both join the caller's transaction (09 6 item 3); the read takes the cart's lock like the confirmation does, so the summary shown and
/// the summary compared are made the same way.
/// </summary>
public interface ICheckout
{
    /// <summary>
    /// The step's read (07 UC-22, UC-24, UC-25; UC-23 with <paramref name="appliesCoupon"/>): the way to the step, the summary, the coupon's verdict and the earlier unpaid order. On UC-23 the code is
    /// judged behind L-6 and a code that is not accepted keeps its attempt; no coupon right is reserved (07 4.13.2).
    /// </summary>
    Task<CheckoutStep> ReadAsync(CheckoutInput input, bool appliesCoupon, CancellationToken ct);

    /// <summary>
    /// The confirmation (07 UC-27; 05 4.2.2): the account and the address part are resolved first (a refusal is 422 or 404 before the order's work), then the order's one transaction - the cart's
    /// lock, the confirmation key, the gates and limits, the evaluation, the earlier unpaid order's cancellation, the reservation and the order. A member who chose to keep the typed address has it
    /// written to the book in the same transaction (K-680); a guest order whose e-mail is a verified account's is bound to it (02 3.13.3).
    /// </summary>
    Task<PlaceOrderResult> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct);

    /// <summary>
    /// The return to the cart while no payment method can be used (07 UC-26 <c>cart</c>; K-1183): writes the cart's once-told mark, which carries no personal data; the cart tells it once. Joins
    /// the caller's transaction.
    /// </summary>
    Task MarkPaymentUnavailableAsync(CartOwner owner, CancellationToken ct);
}
