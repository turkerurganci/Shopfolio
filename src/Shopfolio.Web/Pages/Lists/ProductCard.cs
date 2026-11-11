using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Lists;

/// <summary>The image of a card: its address under <c>/gorsel</c> (07 2.1.2) and the alternative text, which is never empty (02 3.11.4).</summary>
public sealed record CardImage(string Path, string AltText);

/// <summary>
/// The declaration of a running discount on a product card (04 2.11.2; 02 3.9.2): the discounted price beside the reference price, the mark that carries text ("İndirim %20"; 04 2.5.6), and the dates of
/// the range as one line - the page writes the line right under the price row.
/// </summary>
public sealed record ProductCardDiscount(string DiscountedPriceText, string ReferencePriceText, string MarkText, string DateRangeText);

/// <summary>
/// A product card (04 2.11.1-2.11.3; T545): the main image, the name and the VAT-included price, and nothing that sells or counts - no add-to-cart, no stock count, no rating (K-774). The card is one
/// link: its only action is opening the product page. <see cref="PriceText"/> is null when the product has no published variant. <see cref="IsSoldOut"/> is the "Tükendi" mark; a sold-out product
/// stays in the list (04 2.11.3).
/// </summary>
public sealed record ProductCard(string Name, string Path, CardImage? Image, string? PriceText, bool IsSoldOut, ProductCardDiscount? Discount);

/// <summary>The one place a product card is made from a list item (T545): the page gives the item and, when the product has a main image, its address.</summary>
public static class ProductCards
{
    /// <summary>The path prefix of a product's page (07 2.1.2: <c>/urun/{slug}</c>).</summary>
    internal const string ProductPathPrefix = "/urun/";

    private const string DiscountWord = "İndirim";

    /// <summary>The address of a product's page.</summary>
    public static string ProductPath(string slug)
    {
        ArgumentException.ThrowIfNullOrEmpty(slug);
        return ProductPathPrefix + Uri.EscapeDataString(slug);
    }

    /// <summary>
    /// The card of one list item: one card for the product, however many variants it has (04 2.11). The price is the lowest of the published variants (the reader's <c>LowestPrice</c> is the list price, without the discount); while a discount runs the main price is the discounted price (<c>DiscountLine.DiscountedPrice</c>) and the card declares the list price struck through as the reference
    /// beside the reference price and writes the range's dates (04 2.11.2).
    /// </summary>
    public static ProductCard Of(ProductListItem item, CardImage? image)
    {
        ArgumentNullException.ThrowIfNull(item);

        ProductCardDiscount? discount = item.Discount is { } line
            ? new ProductCardDiscount(
                DisplayFormat.Amount(line.DiscountedPrice),
                DisplayFormat.Amount(line.ReferencePrice),
                DiscountWord + " " + DisplayFormat.Percent(line.Percentage),
                DisplayFormat.DateRange(line.StartDate, line.EndDate))
            : null;

        return new ProductCard(
            item.Name,
            ProductPath(item.Slug),
            image,
            item.Discount is { } running ? DisplayFormat.Amount(running.DiscountedPrice) : item.LowestPrice is { } price ? DisplayFormat.Amount(price) : null,
            !item.IsPurchasable,
            discount);
    }

    /// <summary>The card of one list item with the main image the item itself carries (D-332): the image is absent for a product without a gallery.</summary>
    public static ProductCard Of(ProductListItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return Of(item, item.MainImageId is { } imageId ? ImageOf(imageId, string.IsNullOrWhiteSpace(item.MainImageAltText) ? item.Name : item.MainImageAltText) : null);
    }

    /// <summary>The cards of a page of the list with the images the items carry, in the list's order, one per item (the category page and the home page's showcase).</summary>
    public static IReadOnlyList<ProductCard> Of(IReadOnlyList<ProductListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return [.. items.Select(item => Of(item))];
    }

    /// <summary>The cards of a page of the list, in the list's order, one per item.</summary>
    public static IReadOnlyList<ProductCard> Of(IReadOnlyList<ProductListItem> items, Func<ProductListItem, CardImage?> imageOf)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(imageOf);
        return [.. items.Select(item => Of(item, imageOf(item)))];
    }

    /// <summary>The image of a card from the stored file: the address of the file and the text (the product name stands in when the page has no other text).</summary>
    public static CardImage ImageOf(Guid imageId, string altText) => new(FrameStyle.ImagePath(imageId), altText);
}

/// <summary>
/// A content card (04 2.11.4; T545): a service introduction or a reference work - the main image, the title and the short description; no price and no add-to-cart (K-755). A record without a main
/// image is shown without one, as text only; <see cref="ShortDescription"/> is null when the firm wrote none.
/// </summary>
public sealed record ContentCard(string Title, string Path, CardImage? Image, string? ShortDescription);

/// <summary>The one place a content card is made from the home page's and the list page's card record (T545).</summary>
public static class ContentCards
{
    public static ContentCard Of(HomepageCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        CardImage? image = card.MainImage is { } main
            ? new CardImage(FrameStyle.ImagePath(main.ImageId), string.IsNullOrWhiteSpace(main.AltText) ? card.Title : main.AltText.Trim())
            : null;
        string? description = string.IsNullOrWhiteSpace(card.ShortDescription) ? null : card.ShortDescription;
        return new ContentCard(card.Title, card.Path, image, description);
    }

    public static IReadOnlyList<ContentCard> Of(IReadOnlyList<HomepageCard> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);
        return [.. cards.Select(Of)];
    }
}
