using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog.Contracts.Lists;

/// <summary>
/// One product of a storefront list (04 2.11.1, 2.11.3): the card's name and address, the lowest price of its published variants (VAT included; empty when it has none),
/// whether at least one published variant can be bought now (the sold-out card otherwise), and - while the product's discount runs - the discount declaration of that
/// lowest-priced variant with the range's start and end dates (02 3.9.2; 04 2.11.2; K-1578). The stock count is never here (02 3.6.5).
/// </summary>
public sealed record ProductListItem(Guid Id, string Name, string Slug, decimal? LowestPrice, bool IsPurchasable, DiscountLine? Discount);
