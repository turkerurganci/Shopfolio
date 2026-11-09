using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The firm's cancellation reason as the panel sends it (02 7.2.4; 06 4.22; T368): one home for what a reason needs besides itself. "Other" carries a free note and no
/// other reason does (K-373, K-451); "müşteriyle anlaşıldı (müşteri talebi)" carries the firm's statement of how the owner's request was confirmed - from the order's
/// e-mail, or confirmed by turning back to the order's e-mail or delivery telephone - and no other reason does (02 8.3.8; K-607). The whole-order cancellation and the
/// item cancellation read this one rule; the product does not check the proof of the confirmation (the risk is the firm's, 02 12.5).
/// </summary>
internal static class FirmReasonRules
{
    public static bool IsComplete(CancellationReason reason, string? note, AgreementConfirmation? agreement)
    {
        return (reason == CancellationReason.Other) == !string.IsNullOrWhiteSpace(note)
            && (reason == CancellationReason.AgreedWithBuyer) == (agreement is not null);
    }
}
