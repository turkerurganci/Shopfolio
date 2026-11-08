namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The forty-one operations of the order page and the panel (03 1.11): the customer's fourteen (<c>1.11.1</c>-<c>1.11.14</c>) and the administrator's twenty-seven
/// (<c>1.11.15</c>-<c>1.11.41</c>). The numeric value of a member is the row's last number, so the row identity is <c>1.11.</c> and the value; a value that is not
/// one of these is no operation, and an operation that is not in the decision table is closed (03 1.11; T323 criterion 8). The interval each one is open in is the
/// decision's (<see cref="IOrderOperations"/>), not this list's: the names are identities only (06 5.1.3).
/// </summary>
public enum OrderOperation
{
    CancelOrder = 1,
    CancelPhysicalItem = 2,
    CancelServiceItem = 3,
    CancelDigitalItem = 4,
    WithdrawPhysicalItem = 5,
    WithdrawServiceItem = 6,
    WithdrawDigitalItem = 7,
    TerminateForLateDelivery = 8,
    OpenDefectClaim = 9,
    ReopenDefectClaimByCustomer = 10,
    EnterRefundIban = 11,
    CorrectRefundIban = 12,
    ViewTransferDetails = 13,
    DownloadDigitalFile = 14,
    MarkTransferPaid = 15,
    ShipOrder = 16,
    MarkDelivered = 17,
    MarkDeliveryFailed = 18,
    ResendOrder = 19,
    MarkServiceCompleted = 20,
    CancelByFirm = 21,
    RemoveOrReduceItem = 22,
    CorrectAddress = 23,
    CorrectCarrier = 24,
    CorrectDeliveryOrArrivalDate = 25,
    CorrectTransition = 26,
    WriteInternalNote = 27,
    ReceiveReturnedGoods = 28,
    RejectReturnedGoods = 29,
    CloseAsNotReturned = 30,
    ProcessRefund = 31,
    RetryOrRedirectCardRefund = 32,
    ReportTransferRefundFailed = 33,
    RecordWithdrawalFromOtherChannel = 34,
    CorrectGuestEmail = 35,
    ResendEmail = 36,
    ResolveOrReopenDefectClaim = 37,
    RenewDownloadAllowance = 38,
    RequestRefundIban = 39,
    CloseOrder = 40,
    RecordLateTerminationFromOtherChannel = 41,
}
