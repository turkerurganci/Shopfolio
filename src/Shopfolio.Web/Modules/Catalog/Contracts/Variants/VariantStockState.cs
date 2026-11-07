namespace Shopfolio.Modules.Catalog.Contracts.Variants;

/// <summary>
/// The stock field and the reserved quantity beside it, for the panel only (02 3.6.7): the field is empty for a digital variant and for a service without capacity. Never given
/// to a visitor-facing page; the visitor reads <see cref="ProductAvailability"/>.
/// </summary>
public sealed record VariantStockState(int? StockQuantity, int ReservedQuantity);
