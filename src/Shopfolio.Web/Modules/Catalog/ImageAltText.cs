using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The alternative text of a product's image (02 3.11.4; K-93, K-564): the firm's text when it wrote one; otherwise a generated text that is never empty and tells the product's images
/// apart. A variant's own image: the product's name and the variant's option values ("Pamuk Tişört — Kırmızı / M"). An image of the gallery: the product's name and the image's place in
/// the gallery ("Pamuk Tişört — görsel 2 / 5"). The card and the list show one image, so only the product's name is used. A product name that is empty or white space gives the fixed
/// word below instead, so the text still never comes out empty.
/// </summary>
internal static class ImageAltText
{
    /// <summary>The text when even the product's name is empty; the name is required, so this is a defence, not a path.</summary>
    public const string NamelessProduct = "Ürün";

    private const string Dash = " — ";
    private const string ValueSeparator = " / ";

    /// <summary>The text of a variant's own image: the name and the option values in the order of the options; no values (a product without options) gives the name alone.</summary>
    public static string ForVariant(string? productName, IEnumerable<string?> optionValues)
    {
        ArgumentNullException.ThrowIfNull(optionValues);
        string name = NameOf(productName);
        List<string> values = [.. optionValues.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!.Trim())]; // non-null: the Where on this line keeps only filled values
        return values.Count == 0 ? name : name + Dash + string.Join(ValueSeparator, values);
    }

    /// <summary>The text of a gallery image: the name and the place of the image, counted from 1, out of the gallery's count.</summary>
    public static string ForGallery(string? productName, int position, int total)
    {
        return GalleryText.Generated(NameOf(productName), position, total);
    }

    /// <summary>The text of the card and the list image: the product's name alone.</summary>
    public static string ForCard(string? productName) => NameOf(productName);

    /// <summary>The firm's text when it wrote one, else the generated one.</summary>
    public static string Choose(string? firmText, Func<string> generated)
    {
        return GalleryText.Choose(firmText, generated);
    }

    private static string NameOf(string? productName) => string.IsNullOrWhiteSpace(productName) ? NamelessProduct : productName.Trim();
}
