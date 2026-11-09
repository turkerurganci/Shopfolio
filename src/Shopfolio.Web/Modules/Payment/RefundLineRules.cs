using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The refund line's one rule home (T370; 02 7.2.8, 7.4.5; 06 4.33, 4.34; K-214, K-569, K-940): the way a refund goes, who starts it, and what the firm may
/// do with a card refund whose request did not go through. The Payment module's writers, the order module's operations and the panel read these rules;
/// none of them keeps a second copy.
/// </summary>
internal static class RefundLineRules
{
    /// <summary>
    /// The way of a refund (02 7.4.5; K-214): the money goes back the way the payment came - a card payment to the card through the provider, a bank transfer
    /// to the customer's IBAN. The refund starts on this way; only the customer's IBAN after a failed card refund turns it to the transfer (<see cref="CardRefundActions.CanTurnToTransfer"/>).
    /// </summary>
    public static PaymentMethod WayOf(PaymentMethod orderPaymentMethod)
    {
        return orderPaymentMethod switch
        {
            PaymentMethod.Card => PaymentMethod.Card,
            PaymentMethod.BankTransfer => PaymentMethod.BankTransfer,
            _ => throw new ArgumentOutOfRangeException(nameof(orderPaymentMethod), orderPaymentMethod, "Unknown payment method."),
        };
    }

    /// <summary>
    /// Whether the system starts the refund by itself, with the record that causes it and without an approval window (02 7.2.8; 06 4.33; K-691): the card refund
    /// of a cancellation - the customer's or the firm's -, a removal, a late delivery termination and a late payment to a cancelled order. Every other refund is
    /// the firm's step on the panel: the whole transfer line, and the withdrawal, the defect claim and the amount-based refund on the card line (03 2.8.4.1).
    /// </summary>
    public static bool IsStartedBySystem(RefundType refundType, PaymentMethod way)
    {
        return way == PaymentMethod.Card
            && refundType is RefundType.Cancellation or RefundType.ItemRemoval or RefundType.LateDeliveryTermination or RefundType.LatePayment;
    }

    /// <summary>
    /// Whether the record's refund asks the customer for an IBAN field (02 7.4.5; K-214): never on the card line; on the transfer line when no IBAN came with the
    /// record - the firm's cancellation and the removal always (K-690).
    /// </summary>
    public static bool NeedsIbanField(PaymentMethod way, bool hasIban)
    {
        return way == PaymentMethod.BankTransfer && !hasIban;
    }

    /// <summary>
    /// What the firm and the customer may do with a refund on the card line (02 7.2.8; K-569, K-940):
    /// <list type="bullet">
    /// <item>a failed latest request ("geri ödeme gerçekleşmedi"): the firm retries or opens the transfer path (B-14) - the retry closes once the customer
    /// entered an IBAN, the same money never goes two ways;</item>
    /// <item>a pending latest request ("geri ödeme sonucu bekleniyor"): the result is asked of the provider; the retry and the transfer path are closed;</item>
    /// <item>a sent or voided refund, or one on the transfer way: nothing is open.</item>
    /// </list>
    /// </summary>
    public static CardRefundActions ActionsOf(CardRefundLine line, bool isIbanEntered)
    {
        ArgumentNullException.ThrowIfNull(line);
        bool failed = line.Way == PaymentMethod.Card && !line.IsSent && !line.IsVoided && line.LatestState == RefundRecordState.Failed;
        return new CardRefundActions(
            CanRetry: failed && !isIbanEntered,
            CanOpenTransferPath: failed && !isIbanEntered,
            CanTurnToTransfer: failed && isIbanEntered);
    }
}

/// <summary>What <see cref="RefundLineRules.ActionsOf"/> opens: the firm's retry, the firm's transfer path, and turning the way to the transfer with the customer's IBAN.</summary>
internal readonly record struct CardRefundActions(bool CanRetry, bool CanOpenTransferPath, bool CanTurnToTransfer);
