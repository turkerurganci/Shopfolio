using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The named reason of a refused refund step the screen tells apart (T529; 07 UC-131, UC-133, UC-134): the codes are the page's (07 6.2.7).</summary>
public enum PanelRefundObstacle
{
    /// <summary>The transfer line has no IBAN on file for the refund: the IBAN request comes first (K-686; <c>RefundIbanMissing</c>).</summary>
    IbanMissing,

    /// <summary>The amount based refund's amount is zero or more than the paid money not yet given back (K-1078; <c>AmountBasedRefundExceeded</c>).</summary>
    AmountExceeded,

    /// <summary>The IBAN request names no item of the order (07 UC-134; <c>FieldRequired</c>).</summary>
    ItemNotSelected,
}

/// <summary>
/// The outcome of one of the screen's refund steps (T529; 07 UC-131...UC-137): the refusal (null on success) with its named reason, and - on the unconfirmed first POST that passed the version and the
/// rule (<see cref="TransitionRefusal.ConfirmationRequired"/>) - the amount and, on the transfer, the IBAN the window repeats (K-797). A refusal writes nothing.
/// </summary>
public sealed record PanelRefundStepResult(TransitionRefusal? Refusal) : IOperationResult
{
    public bool IsRejected => Refusal is not null;

    /// <summary>What the page's <c>OperationRunner</c> reads (09 8.3 item 3): applied, the version conflict (409 before the rule), or the operation closed. A rejected one rolls the transaction back.</summary>
    public GatedOperationResult Gate => Refusal switch
    {
        null => GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Allowed, OperationAvailability.Open())),
        TransitionRefusal.VersionConflict => GatedOperationResult.Conflict,
        _ => GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed)),
    };

    public PanelRefundObstacle? Obstacle { get; init; }

    /// <summary>The amount the server worked out, repeated by the confirmation window; null otherwise.</summary>
    public decimal? Amount { get; init; }

    /// <summary>The IBAN the transfer goes to, repeated by the confirmation window; null on the card and otherwise.</summary>
    public string? Iban { get; init; }

    public static PanelRefundStepResult Done { get; } = new((TransitionRefusal?)null);
}

/// <summary>
/// One refund line of the order screen (T529; 04 9.9.6, 9.9.8, 9.9.21-9.9.24): a refund owed by an item record whose row is not born yet, or a refund row that is not sent; the key the forms carry (the
/// record that causes it, 07 UC-131; the refund itself for UC-135...UC-137), the amount and the IBAN the server worked out, and which of the steps is open now - the one decision the POST asks again.
/// </summary>
public sealed record PanelRefundLine(Guid Key, Guid? RefundId, Guid? OrderItemId, RefundType Type, int? Quantity, decimal Amount, PaymentMethod Way, string? Iban, DateTimeOffset StartedAt)
{
    /// <summary>The request field that carries <see cref="Key"/> - one of <see cref="PanelRefundKeys"/>.</summary>
    public string KeyName { get; init; } = PanelRefundKeys.RefundId;

    /// <summary>UC-131: "Geri ödemeyi işle".</summary>
    public bool CanProcess { get; init; }

    /// <summary>UC-132: "Havale gerçekleşmedi".</summary>
    public bool CanReportTransferFailed { get; init; }

    /// <summary>UC-135 and UC-136: the card refund is "geri ödeme gerçekleşmedi" and no IBAN came on its transfer path (03 1.11.32; K-940).</summary>
    public bool CanRetryOrSwitch { get; init; }

    /// <summary>UC-137: the card refund's result is awaited ("geri ödeme sonucu bekleniyor").</summary>
    public bool CanMarkFailed { get; init; }

    /// <summary>The lasting warning of a success that came after the firm's "gerçekleşmedi" mark (04 9.9.8; K-978): the same money may have gone twice.</summary>
    public bool HasLateSuccess { get; init; }

    /// <summary>True when the refund was already sent; only a line with the lasting warning is shown then.</summary>
    public bool IsSent { get; init; }
}

/// <summary>
/// The request fields of UC-131 and UC-132 (07 9.9.10: exactly one of the links of 06 4.33 by the refund's type - the record that causes the refund) and the refund's own id for a refund that has
/// none (an amount based refund) and for UC-135...UC-137.
/// </summary>
public static class PanelRefundKeys
{
    public const string CancellationId = nameof(CancellationId);

    public const string ItemRemovalId = nameof(ItemRemovalId);

    public const string LateDeliveryTerminationId = nameof(LateDeliveryTerminationId);

    public const string WithdrawalId = nameof(WithdrawalId);

    public const string ReturnId = nameof(ReturnId);

    public const string DefectClaimId = nameof(DefectClaimId);

    public const string PaymentRecordId = nameof(PaymentRecordId);

    public const string RefundId = nameof(RefundId);
}

/// <summary>A refund step on one line (T529; 07 UC-131, UC-132, UC-135...UC-137): the order, the line's key, the administrator, the version the screen showed and the confirmation mark.</summary>
public sealed record PanelRefundRequest(Guid OrderId, Guid Key, TransitionAdmin Admin, int ExpectedVersion, bool Confirmed = false);

/// <summary>UC-133: the amount based partial refund - the amount, the optional item, the administrator, the version and the confirmation mark.</summary>
public sealed record AmountBasedRefundRequest(Guid OrderId, decimal Amount, Guid? OrderItemId, TransitionAdmin Admin, int ExpectedVersion, bool Confirmed);

/// <summary>
/// The panel's refund steps on one order (T529; 07 UC-131...UC-137; 04 9.9.21-9.9.24), the write half of operations 03 1.11.31-1.11.33 and 1.11.39 (09 8.3 items 1-3; D-153). Each method joins the
/// transaction the page opened with <c>OperationRunner</c> and, under the order's row lock, compares the version the screen showed (409 before the rule), asks the one operation decision the GET drew the
/// buttons from (<see cref="IOrderOperations"/>; the card refund's two steps by <c>OrderOperationCatalog.CheckCardRefundStep</c>, D-657, D-660) and only then writes, with the audit row in the same
/// transaction (02 10.3.1 (3); D-898). The amounts are the server's (K-1105, K-1078): the page never works one out. Nothing is saved here.
/// </summary>
public interface IPanelRefundSteps
{
    /// <summary>The order's refund lines as the screen draws them, with the steps open now. A plain read.</summary>
    Task<IReadOnlyList<PanelRefundLine>> ReadLinesAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// UC-131: processes the refund the line's record causes. On the transfer the refund is sent with the step and the axis moves (Ö3-Ö5, B-8; the IBAN goes with the last refund that uses it, K-1110);
    /// on the card the refund row and its job row are written and the provider's result moves it (K-970). Unconfirmed, the checks only and the window's amount and IBAN.
    /// </summary>
    Task<PanelRefundStepResult> ProcessRefundAsync(PanelRefundRequest request, CancellationToken ct);

    /// <summary>UC-132: "havale gerçekleşmedi" - the IBAN is deleted, the field opens again and the item waits for an IBAN (K-1131); a processed refund is refused.</summary>
    Task<PanelRefundStepResult> MarkBankTransferRefundFailedAsync(PanelRefundRequest request, CancellationToken ct);

    /// <summary>UC-133: the amount based partial refund, bound to the item's open "mal dönmedi" closure when there is one (K-1105); the amount is checked against the paid money not yet given back.</summary>
    Task<PanelRefundStepResult> ProcessAmountBasedRefundAsync(AmountBasedRefundRequest request, CancellationToken ct);

    /// <summary>UC-134: the IBAN request on an item of a transfer order; the reason is the server's (<c>AdminRequest</c>; K-1201) and B-14 goes. The key is the item.</summary>
    Task<PanelRefundStepResult> RequestRefundIbanAsync(PanelRefundRequest request, CancellationToken ct);

    /// <summary>UC-135: a new card refund request for a "geri ödeme gerçekleşmedi" refund; confirmed (03 1.6.1.4). The key is the refund.</summary>
    Task<PanelRefundStepResult> RetryCardRefundAsync(PanelRefundRequest request, CancellationToken ct);

    /// <summary>UC-136: the transfer path of a failed card refund - an IBAN request with the reason <c>CardRefundFailed</c> and B-14. The key is the refund.</summary>
    Task<PanelRefundStepResult> SwitchRefundToBankTransferAsync(PanelRefundRequest request, CancellationToken ct);

    /// <summary>UC-137: the administrator's "gerçekleşmedi" mark on a card refund whose result is awaited; the query stops and no notice goes (K-940, K-978). The key is the refund.</summary>
    Task<PanelRefundStepResult> MarkCardRefundFailedAsync(PanelRefundRequest request, CancellationToken ct);
}
