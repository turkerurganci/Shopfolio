namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The forty-one operations of the order page and the panel (03 1.11): the customer's fourteen (<c>1.11.1</c>-<c>1.11.14</c>) and the administrator's twenty-seven
/// (<c>1.11.15</c>-<c>1.11.41</c>). The members stand in the document's order, so the row identity is <c>1.11.</c> and the member's position counted from one (the enum file holds no explicit value; 06 5.1.3); a value that is not
/// one of these is no operation, and an operation that is not in the decision table is closed (03 1.11; T323 criterion 8). The interval each one is open in is the
/// decision's (<see cref="IOrderOperations"/>), not this list's: the names are identities only (06 5.1.3).
/// </summary>
public enum OrderOperation
{
    CancelOrder,
    CancelPhysicalItem,
    CancelServiceItem,
    CancelDigitalItem,
    WithdrawPhysicalItem,
    WithdrawServiceItem,
    WithdrawDigitalItem,
    TerminateForLateDelivery,
    OpenDefectClaim,
    ReopenDefectClaimByCustomer,
    EnterRefundIban,
    CorrectRefundIban,
    ViewTransferDetails,
    DownloadDigitalFile,
    MarkTransferPaid,
    ShipOrder,
    MarkDelivered,
    MarkDeliveryFailed,
    ResendOrder,
    MarkServiceCompleted,
    CancelByFirm,
    RemoveOrReduceItem,
    CorrectAddress,
    CorrectCarrier,
    CorrectDeliveryOrArrivalDate,
    CorrectTransition,
    WriteInternalNote,
    ReceiveReturnedGoods,
    RejectReturnedGoods,
    CloseAsNotReturned,
    ProcessRefund,
    RetryOrRedirectCardRefund,
    ReportTransferRefundFailed,
    RecordWithdrawalFromOtherChannel,
    CorrectGuestEmail,
    ResendEmail,
    ResolveOrReopenDefectClaim,
    RenewDownloadAllowance,
    RequestRefundIban,
    CloseOrder,
    RecordLateTerminationFromOtherChannel,
}
