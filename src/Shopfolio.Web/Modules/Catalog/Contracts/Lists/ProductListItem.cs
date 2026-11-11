using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog.Contracts.Lists;

/// <summary>
/// One product of a storefront list (04 2.11.1, 2.11.3): the card's name and address, the lowest price of its published variants (VAT included; empty when it has none),
/// whether at least one published variant can be bought now (the sold-out card otherwise), and - while the product's discount runs - the discount declaration of that
/// lowest-priced variant with the range's start and end dates (02 3.9.2; 04 2.11.2; K-1578). The stock count is never here (02 3.6.5).
/// </summary>
/// <remarks>
/// <see cref="MainImageId"/> is the product's main gallery image and <see cref="MainImageAltText"/> its text, never empty when the image is there (the firm's text, else the product's name; 02 3.11.4);
/// both are null for a product without a gallery (T502; D-332) - the card is then drawn without an image.
/// </remarks>
public sealed record ProductListItem(
    Guid Id,
    string Name,
    string Slug,
    decimal? LowestPrice,
    bool IsPurchasable,
    DiscountLine? Discount,
    Guid? MainImageId = null,
    string? MainImageAltText = null);
