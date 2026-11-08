namespace Shopfolio.Modules.Catalog.Contracts.Lists;

/// <summary>
/// One product of a storefront list (04 2.11.1, 2.11.3): the card's name and address, the lowest price of its published variants (VAT included; empty when it has none) and
/// whether at least one published variant can be bought now (the sold-out card otherwise). The stock count is never here (02 3.6.5).
/// </summary>
public sealed record ProductListItem(Guid Id, string Name, string Slug, decimal? LowestPrice, bool IsPurchasable);
