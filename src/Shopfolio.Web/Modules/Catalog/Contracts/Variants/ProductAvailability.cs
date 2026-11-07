namespace Shopfolio.Modules.Catalog.Contracts.Variants;

/// <summary>
/// The sold-out state of a product as the visitor sees it (02 3.6.3, 3.6.4): the published variants, each sold out or not, and the product sold out when every one of them is.
/// A sold-out product stays on the shop window; it is only marked and cannot be added to the cart. The reserved units are already taken off, so a last unit held by a pending
/// payment shows as sold out and returns when the payment does not come. Carries no number.
/// </summary>
public sealed record ProductAvailability(IReadOnlyList<VariantAvailability> Variants, bool IsSoldOut);
