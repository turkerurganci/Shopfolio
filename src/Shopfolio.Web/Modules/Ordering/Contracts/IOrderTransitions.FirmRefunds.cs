using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>A refund the firm's panel step just sent on the transfer (T529; 07 UC-131, UC-133): the order, the new refund's facts and the administrator.</summary>
public sealed record FirmRefundSent(Guid OrderId, RefundFacts Refund, TransitionAdmin Admin);

public partial interface IOrderTransitions
{
    /// <summary>
    /// The payment axis of a refund the firm's step sent with the step (T529; 07 UC-131, UC-133; 02 5.5): Ö3, Ö4 or Ö5 by the administrator, B-8, and the order's IBANs no pending refund uses any more
    /// go with their disposal records (K-1110). Joins the caller's transaction under the order's row lock; the version was compared by the caller's step.
    /// </summary>
    Task<PaymentTransitionResult> ApplyFirmRefundSentAsync(FirmRefundSent sent, CancellationToken ct);
}
