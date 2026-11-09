namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>Why an item record was not written; the transition unit maps it to its own refusal (07 6.2.7).</summary>
public enum ItemRecordRefusal
{
    /// <summary>The quantity is below one or above the quantity open to the operation (03 1.7.4).</summary>
    QuantityExceedsOpen,

    /// <summary>The item already has a claim: an open one takes no new claim, a resolved one is reopened (03 1.9.1; K-676).</summary>
    ClaimExists,

    /// <summary>The item has no claim to change.</summary>
    ClaimNotFound,

    /// <summary>The claim is not in the status the change starts from (Open to Resolved; Resolved to Open).</summary>
    StatusNotAllowed,

    /// <summary>The kind of actor may not make the change (only an administrator resolves; only the customer raises the quantity).</summary>
    ActorNotAllowed,

    /// <summary>The order page's channel of the claim is closed: not shipped or delivered yet, or the two-year period ran out (02 Z-18).</summary>
    ChannelClosed,

    /// <summary>The item carries no conditional exemption: its return cannot be rejected (02 7.4.9; K-656).</summary>
    NotConditionalExemption,

    /// <summary>The withdrawal is not the item's, or none of its units is on the way back.</summary>
    WithdrawalNotAwaiting,

    /// <summary>The arrival date is before the item's entered delivery date (02 7.4.7; 06 4.26); the caller refuses a date in the future.</summary>
    ArrivalDateOutOfRange,

    /// <summary>The "mal dönmedi" closure is not open: not a withdrawal after the delivery, or its sending period (Z-42) has not run out (02 10.4.9).</summary>
    SendingPeriodNotOver,

    /// <summary>The receipt is not the item's, or not a withdrawal's or termination's receipt of a physical item.</summary>
    ReceiptNotFound,

    /// <summary>The receipt's units were already added to the stock, or the item's variant is gone (06 4.26 <c>StockAddedAt</c>): the step is taken once.</summary>
    StockNotAddable,

    /// <summary>The declaration on the bank transfer line asked for the IBAN on file and the order has none (02 7.4.5): the IBAN is entered with the declaration.</summary>
    IbanMissing,
}
