namespace Shopfolio.Modules.Catalog.Contracts.Lists;

/// <summary>
/// A product card of a content page's "İlgili ürünler" (T505; 04 5.8.5, 2.11.1): the list item the card is drawn from, the product's main image - null when it has none - and the alternative
/// text the card's image uses (the firm's text for the main image when written, else the product's name alone; 02 3.11.4).
/// </summary>
public sealed record ProductListCard(ProductListItem Item, Guid? MainImageId, string? ImageAltText);
