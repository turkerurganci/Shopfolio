namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The shipping period a variant carries into an order (02 3.20.5; 11 T350 criterion 6): the variant's product, its type and the product's own period in business days - null when the
/// product sets none and the firm default applies. A variant has no period of its own; it takes its product's.
/// </summary>
public sealed record VariantShippingPeriod(Guid VariantId, Guid ProductId, ProductType ProductType, int? ProductPeriod);
