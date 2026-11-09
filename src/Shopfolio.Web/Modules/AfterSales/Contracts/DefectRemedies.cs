namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// The firm's rescission of a defect claim's units (02 7.5.3; 03 2.9.4, 8.4.10; K-788, K-793, K-1077): the units taken back - at most the claim's defective units not
/// rescinded yet and at most the units open to an operation - and the item's frozen values the refund's amounts are split from (02 7.1.7; <see cref="ItemFrozenAmounts"/>).
/// </summary>
public sealed record DefectClaimRescissionEntry(ItemRecordSubject Subject, int Quantity, ItemFrozenAmounts Frozen);

/// <summary>
/// The answer of a defect remedy's check (02 7.5.3): a refusal, or the item's open claim and - on a rescission - what the refund pays back for the units
/// (<see cref="RefundAmount"/>; the coupon share and the VAT the rescission sets aside). The price reduction's amount is the firm's, so it carries none.
/// </summary>
public sealed record DefectRemedyCheck(ItemRecordRefusal? Refusal, Guid? ClaimId, RefundAmount? Amount)
{
    public bool IsRejected => Refusal is not null;

    public static DefectRemedyCheck Rejected(ItemRecordRefusal refusal) => new(refusal, null, null);
}

/// <summary>
/// The IBAN a defect remedy's refund needs on the bank transfer line (02 7.5.3, 7.4.5; 03 8.3.3.6; K-686): the item and the administrator who asks. The request is the
/// administrator's (<see cref="RefundIbanRequestReason.AdminRequest"/>), so it names no refund (06 4.29: the refund link is a failed card refund's or an itemless
/// request's); the IBAN entered lands on the order's file and pays the order's waiting refunds. Nothing is asked when the order has an IBAN on file or the item's
/// request is still open.
/// </summary>
public sealed record DefectRemedyIbanRequest(ItemRecordSubject Subject, ItemRecordAdmin Admin, DateTimeOffset At);
