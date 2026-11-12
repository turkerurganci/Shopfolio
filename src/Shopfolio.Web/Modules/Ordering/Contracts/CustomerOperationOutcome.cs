namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>Why a customer's operation was not applied, or that it was (07 4.16.2, 4.17.2–4.17.4).</summary>
public enum CustomerOperationOutcome
{
    Applied,

    /// <summary>The operation or the field is not open now (303 with <c>OperationClosed</c> on the screen; 422 on the IBAN field).</summary>
    Closed,

    /// <summary>A quantity above the open units, or an open quantity other than the one shown (K-1202): 303 with <c>QuantityExceedsOpen</c>.</summary>
    QuantityExceedsOpen,

    /// <summary>The IBAN is empty where it is required, or fails format or check digits: 422 with the field's message.</summary>
    InvalidIban,

    /// <summary>An item that is not the order's: 404 (07 2.3.3).</summary>
    NotFound,
}
