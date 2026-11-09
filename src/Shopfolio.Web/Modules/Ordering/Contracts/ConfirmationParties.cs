namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The customer's name, addresses and contact e-mail as they were at the moment of confirmation (02 3.23.3; K-1027 (4), (5)): the values the frozen pre-information form and distance sales
/// contract fill their placeholders with. The firm's later correction of an address or of a guest order's e-mail does not change them: a corrected value is read from the first row of its
/// history, an uncorrected one from the order. Empty fields on an order whose personal data is disposed.
/// </summary>
/// <param name="ContactEmail">The contact e-mail at confirmation.</param>
/// <param name="Shipping">The shipping address with its telephone at confirmation; empty on an order with no physical item.</param>
/// <param name="Billing">The billing address at confirmation.</param>
public sealed record ConfirmationParties(string? ContactEmail, PartyAddress? Shipping, PartyAddress? Billing);

/// <summary>An address of an order as a value: the recipient, the telephone - only on the shipping address -, the province, the district and the address line.</summary>
public sealed record PartyAddress(string RecipientName, string? Phone, Guid ProvinceId, string District, string AddressLine);
