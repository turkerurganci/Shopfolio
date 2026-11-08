using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The classification of a verified payment notification at its record (08 2.1.5 "classification of the notification", rows 3-7; 05 4.5.5; 06 4.35
/// <c>UnprocessedReason</c>, 5.6.6). Rows 1 and 2 - the signature and the repeat - are the signed endpoint pipeline's and the record's unique index's;
/// this rule takes the rest in the table's order, and the first row that holds decides:
/// (3) the reference maps to no order - <see cref="UnprocessedNotificationReason.OrderNotFound"/>;
/// (4) the event type or the outcome is unknown, the payment is not by card, not in lira, a test notification on the live installation, a field of the
/// body could not be read, or the order is not a card order - <see cref="UnprocessedNotificationReason.UnrecognizedNotification"/>;
/// (5) a failed payment - processed, no amount is checked;
/// (6) a successful payment whose amount does not hold - <see cref="UnprocessedNotificationReason.AmountMismatch"/>;
/// (7) a successful payment whose amount holds - processed.
/// The signature and the amount are checked in the link layer (05 3.6.1; <see cref="IPaymentGateway.CheckNotificationAmount"/>); this rule only orders the
/// classes. A marked notification is recorded with its reason and is not processed; the others get the job row that processes them.
/// </summary>
internal static class PaymentNotificationRule
{
    /// <summary>The reason the notification is not processed, or null when the processing job applies it.</summary>
    public static UnprocessedNotificationReason? Classify(PaymentNotice notice, OrderForPayment? order, IPaymentGateway gateway, bool isLiveInstallation)
    {
        ArgumentNullException.ThrowIfNull(notice);
        ArgumentNullException.ThrowIfNull(gateway);
        if (order is null)
        {
            return UnprocessedNotificationReason.OrderNotFound;
        }

        NotificationResult result = gateway.ReadNotificationResult(notice.EventType, notice.Outcome);
        if (IsUnrecognized(notice, order, result, isLiveInstallation))
        {
            return UnprocessedNotificationReason.UnrecognizedNotification;
        }

        if (result == NotificationResult.PaymentFailed)
        {
            return null;
        }

        return gateway.CheckNotificationAmount(notice, order.Total) == NotificationAmountCheck.Holds ? null : UnprocessedNotificationReason.AmountMismatch;
    }

    /// <summary>The amount the record keeps: the collected total when it was read and is not negative (06 4.35 <c>amount_not_negative</c>), otherwise none.</summary>
    public static decimal? RecordedAmount(PaymentNotice notice)
    {
        ArgumentNullException.ThrowIfNull(notice);
        return notice.TotalAmount is >= 0m ? notice.TotalAmount : null;
    }

    private static bool IsUnrecognized(PaymentNotice notice, OrderForPayment order, NotificationResult result, bool isLiveInstallation)
    {
        if (result == NotificationResult.Unrecognized || !notice.PaidByCard || !notice.InLira || (notice.TestMode && isLiveInstallation))
        {
            return true;
        }

        // A body whose amount could not be read is not parsed (05 4.5.5); the started amount is read only for a success, whose amount is compared.
        if (RecordedAmount(notice) is null || (result == NotificationResult.PaymentSucceeded && notice.PaymentAmount is not >= 0m))
        {
            return true;
        }

        // The provider's notification is about a card payment it started; an order on another payment method never sent it a reference.
        return order.PaymentMethod != PaymentMethod.Card;
    }
}
