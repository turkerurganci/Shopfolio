namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>How the firm knows an address correction was asked by the order's owner (02 10.4.2, 8.3.7). The product does not check it; the proof is the firm's (02 12.5). Never stored.</summary>
public enum AddressRequestConfirmation
{
    /// <summary>Nothing says the request is the owner's: a request from another channel that the firm has not confirmed through the order's own channels.</summary>
    None,

    /// <summary>The request came from the order's contact e-mail.</summary>
    FromOrderEmail,

    /// <summary>The request came from another channel; the firm wrote to the order's contact e-mail and the owner confirmed.</summary>
    ConfirmedByEmailToOrder,

    /// <summary>The request came from another channel; the firm called the order's shipping telephone and the owner confirmed.</summary>
    ConfirmedByCallToOrderPhone,
}
