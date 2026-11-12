namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The confirmation's outcome (T509; 07 4.13.6): the answer the page gives for each. A result code, never stored.</summary>
public enum PlaceOrderOutcome
{
    /// <summary>The order is born: 303 to its confirmation screen.</summary>
    Placed,

    /// <summary>An order was already born with the same confirmation key: 303 to that order's confirmation screen; nothing is written (05 4.2.2 (0); K-1199).</summary>
    AlreadyPlaced,

    /// <summary>The address part refused fields - all of them together, the undelivered shipping province among them: 422.</summary>
    AddressRefused,

    /// <summary>The member's chosen book entry is not theirs: 404 (07 2.3.3).</summary>
    AddressBookEntryNotFound,

    /// <summary>The signed-in account cannot place an order (not verified, 02 3.13.1): 422 with the summary's message.</summary>
    AccountNotAccepted,

    /// <summary>No line enters an order any more: 303 to the cart.</summary>
    NoOrderableItem,

    /// <summary>The physical cart fell under the minimum order amount: 303 to the cart (K-1229).</summary>
    MinimumOrderAmountNotMet,

    /// <summary>The summary changed: 422 with the change lines, the current summary and texts (02 3.17.2).</summary>
    SummaryChanged,

    /// <summary>A legal text's version rose or a needed box is unticked: 422 (02 3.24.1).</summary>
    ConfirmationStepNotReady,

    /// <summary>The chosen payment method is not open: 422 (07 4.13.6).</summary>
    PaymentMethodUnavailable,

    /// <summary>The sale closed while the step was open: 422 with the message beside the button (04 5.13.27).</summary>
    SalesClosed,

    /// <summary>L-7, or L-8 on the bank transfer: 429 with the neutral message (02 6.1.5).</summary>
    AttemptLimitExceeded,
}
