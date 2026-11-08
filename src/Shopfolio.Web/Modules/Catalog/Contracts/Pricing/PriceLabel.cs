namespace Shopfolio.Modules.Catalog.Contracts.Pricing;

/// <summary>
/// The price label of a variant on the product page (02 3.8.5, 12.1.5; Fiyat Etiketi Yönetmeliği m.5): the price exactly as the administrator typed it, VAT
/// included (02 3.8.1); the product's VAT rate, which the variant inherits (02 3.8.2); the place of origin - filled exactly when the page shows the line: on a
/// physical product, and on a digital product whose field is filled; never on a service -; the date the price applies since, read from the variant's price history
/// (empty when the history has no row yet); and, when the product is sold by measure, the measurement unit, the variant's net quantity and the unit price.
/// </summary>
public sealed record PriceLabel(
    decimal Price,
    decimal VatRate,
    Guid? OriginCountryId,
    DateTimeOffset? PriceAppliesSince,
    MeasurementUnit? MeasurementUnit,
    decimal? NetQuantity,
    UnitPriceLine? UnitPrice);

/// <summary>
/// The unit price shown next to the sale price (02 3.8.5; K-1018): the VAT-included price of one <paramref name="BaseUnit"/> - 1 kilogram for kilogram and gram,
/// 1 litre for litre and millilitre, 1 metre for metre and centimetre, otherwise one of the unit itself -, rounded to the kuruş.
/// </summary>
public sealed record UnitPriceLine(MeasurementUnit BaseUnit, decimal Amount);
