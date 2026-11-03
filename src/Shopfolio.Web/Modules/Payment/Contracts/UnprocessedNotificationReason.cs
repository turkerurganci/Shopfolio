namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>The closed values of UnprocessedNotificationReason (06 5.6.6). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum UnprocessedNotificationReason
{
    AmountMismatch,
    OrderNotFound,
    RefundMarkedFailed,
    UnrecognizedNotification,
}
