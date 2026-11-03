namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>The closed values of RefundIbanRequestReason (06 5.6.4). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum RefundIbanRequestReason
{
    ReturnAfterIbanDeleted,
    TransferFailed,
    CardRefundFailed,
    NoticeWithoutIban,
    CancellationOrRemoval,
    AdminRequest,
}
