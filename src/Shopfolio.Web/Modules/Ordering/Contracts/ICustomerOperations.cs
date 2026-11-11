using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The customer's operations of the order page and of its cancellation and late delivery termination screen (T512; 07 UC-35, UC-38…UC-41; 04 5.16.12, 5.17). The reads
/// give the screen what the server decided - the operation, the items with the three quantities, the refund's way and period, the IBAN field (07 8.1.4; K-1170 (2)) -
/// from the operation decision (<see cref="IOrderOperations"/>), so the screen and the POST cannot differ. The writes join the caller's transaction (<c>OperationRunner</c>)
/// and run under the order's row lock against the state as it is now; a refused result (<see cref="CustomerOperationResult.Gate"/> rejected) rolls the transaction back and
/// names its <see cref="CustomerOperationOutcome"/>; nothing was written (K-1170 (4), K-1202). The caller has already found the order through its root.
/// </summary>
public interface ICustomerOperations
{
    /// <summary>
    /// UC-38 — the screen of <paramref name="screen"/> as it opens now, or null when its operation is not open on the order (303 with the reason; K-1170 (1)). The cancellation
    /// address opens the order's cancellation on Alındı + Bekliyor and the item cancellation on a paid order (K-1162 (3)).
    /// </summary>
    Task<CustomerOperationScreen?> ReadScreenAsync(Guid orderId, CustomerScreen screen, CancellationToken ct);

    /// <summary>The order page's open IBAN fields (04 5.16.4, 5.16.12; 2.12.3): an open request to answer, or an IBAN to correct until the refund is processed.</summary>
    Task<IReadOnlyList<RefundIbanFieldView>> ReadRefundIbanFieldsAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// UC-35 — the IBAN of the field of <paramref name="orderItemId"/> (null: the late payment's itemless field; K-1075): entered for the open request or corrected in place
    /// (02 7.4.5; K-575). No field open there is <see cref="CustomerOperationOutcome.Closed"/>; a value that fails format or check digits is
    /// <see cref="CustomerOperationOutcome.InvalidIban"/>. Writes no e-mail (K-710).
    /// </summary>
    Task<CustomerOperationResult> SaveRefundIbanAsync(Guid orderId, Guid? orderItemId, string iban, CancellationToken ct);

    /// <summary>UC-39 — the order's cancellation on Alındı + Bekliyor with all its items (03 1.11.1; 02 7.2.2): the transition unit's S3 with Ö2, the reservations freed.</summary>
    Task<CustomerOperationResult> CancelOrderAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// UC-40 — the cancellation of the chosen units of one or more items (03 1.11.2, 1.11.3; 02 7.2.9): the whole request is checked first against the state as it is now -
    /// every item open for its cancellation, every quantity within the open units and every shown open quantity equal to the current one (K-1202) - and refused whole
    /// otherwise; then each line is the transition unit's cancellation, with <paramref name="iban"/> on the transfer line.
    /// </summary>
    Task<CustomerOperationResult> CancelOrderItemsAsync(Guid orderId, IReadOnlyList<CancelOrderItemsLine> lines, string? iban, CancellationToken ct);

    /// <summary>UC-41 — the late delivery termination of every open unit of the undelivered physical items (03 1.11.8; K-796), with <paramref name="iban"/> on the transfer line.</summary>
    Task<CustomerOperationResult> TerminateForLateDeliveryAsync(Guid orderId, string? iban, CancellationToken ct);
}

/// <summary>The two addresses of the screen under the order's root (07 2.1.3, 4.17.1).</summary>
public enum CustomerScreen
{
    /// <summary><c>{kök}/iptal</c>: the order's cancellation or the item cancellation by the payment status (K-1162 (3)).</summary>
    Cancellation,

    /// <summary><c>{kök}/gecikme-feshi</c>.</summary>
    LateDeliveryTermination,
}

/// <summary>The three operations the one screen carries (04 5.17 m1; K-744).</summary>
public enum CustomerScreenOperation
{
    CancelOrder,
    CancelOrderItems,
    TerminateForLateDelivery,
}

/// <summary>Why a customer's operation was not applied, or that it was (07 4.16.2, 4.17.2–4.17.4).</summary>
public enum CustomerOperationOutcome
{
    Applied,

    /// <summary>The operation or the field is not open now (303 with <c>OperationClosed</c> on the screen; 422 on the IBAN field).</summary>
    Closed,

    /// <summary>A quantity above the open units, or an open quantity other than the one shown (K-1202): 303 with <c>QuantityExceedsOpen</c>.</summary>
    QuantityExceedsOpen,

    /// <summary>The IBAN is empty where it is required, or fails format or check digits: 422 with the field's message.</summary>
    InvalidIban,

    /// <summary>An item that is not the order's: 404 (07 2.3.3).</summary>
    NotFound,
}

/// <summary>The answer of a customer's write: <see cref="Gate"/> is what the runner reads (a rejected one rolls back) and <see cref="Outcome"/> says why.</summary>
public sealed record CustomerOperationResult(GatedOperationResult Gate, CustomerOperationOutcome Outcome);

/// <summary>One line of UC-40's request (07 5 <c>CancelOrderItemsInput</c>): the item, the units and the open quantity the screen showed (K-1202).</summary>
public sealed record CancelOrderItemsLine(Guid OrderItemId, int Quantity, int OriginalOpenQuantity);

/// <summary>
/// The screen of UC-38 (04 5.17.3–5.17.5): the operation, its items, the refund's way and period, whether the IBAN field is drawn and the open physical units the shipping fee
/// rule reads. The cancellation of the order has no refund line (04 5.17.5); the late delivery termination always pays the fee back (02 7.2.3).
/// </summary>
public sealed record CustomerOperationScreen(
    CustomerScreenOperation Operation,
    IReadOnlyList<CustomerScreenItem> Items,
    PaymentMethod PaymentMethod,
    bool AsksIban,
    int RefundPeriodDays,
    IReadOnlyDictionary<Guid, int> OpenPhysicalUnits);

/// <summary>
/// One item of the screen with the server's three values (07 4.17.1; K-1170 (2)): the open units, the default and the upper limit. <see cref="HasQuantityField"/> is false on a
/// digital item and on an item of one unit (04 2.12.1.8), and on the read-only lists of the order's cancellation and of the termination (04 5.17.4).
/// </summary>
public sealed record CustomerScreenItem(
    Guid OrderItemId,
    string ProductName,
    string? FirstOptionValue,
    string? SecondOptionValue,
    ProductType ProductType,
    int Quantity,
    int OpenQuantity,
    int DefaultQuantity,
    int MaxQuantity,
    bool HasQuantityField);

/// <summary>An open IBAN field of the order page (04 5.16.12): its item (null on the late payment's itemless field), the IBAN on file when it is a correction and the open request's reason otherwise.</summary>
public sealed record RefundIbanFieldView(Guid? OrderItemId, string? Iban, RefundIbanRequestReason? Reason)
{
    /// <summary>A correction of an IBAN on file rather than an answer to an open request.</summary>
    public bool IsCorrection => Reason is null;

    /// <summary>The two card-line exceptions where entering an IBAN is a choice (04 2.12.3.1): the failed card refund's transfer path (K-569) and the late payment's refund (K-1075).</summary>
    public bool IsOptional => Reason == RefundIbanRequestReason.CardRefundFailed || (Reason is not null && OrderItemId is null);
}
