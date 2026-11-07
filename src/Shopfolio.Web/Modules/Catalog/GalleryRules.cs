using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The pure rules of a product's gallery (02 3.11.2, 3.11.3; 06 4.71), apart from any storage so they are tested without a database. The main image is a mark of its own, independent of
/// the order; a gallery that has an image always has exactly one main image: the first image added comes with the mark, and when the main image leaves the gallery the first image
/// in the order becomes main (K-1069). The count of a product's images is the gallery's and its variants' together, each image once (02 3.11.3; P-24).
/// </summary>
internal static class GalleryRules
{
    /// <summary>
    /// The image that is main after a save: the one the list marks (the first marked one if the list marks several), else the current main image when it is still in the list, else the
    /// first image in the order; null for an empty gallery. <paramref name="currentMainImageId"/> is the main image before the save, null when there is none.
    /// </summary>
    public static Guid? MainOf(IReadOnlyList<GalleryEntry> entries, Guid? currentMainImageId)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (entries.Count == 0)
        {
            return null;
        }

        GalleryEntry? marked = entries.FirstOrDefault(e => e.IsMain);
        if (marked is not null)
        {
            return marked.ImageId;
        }

        if (currentMainImageId is { } current && entries.Any(e => e.ImageId == current))
        {
            return current;
        }

        return entries[0].ImageId;
    }

    /// <summary>The number of images a product holds: the gallery's and its variants' images together, an image used twice counted once.</summary>
    public static int ImageCount(IEnumerable<Guid> galleryImageIds, IEnumerable<Guid> variantImageIds)
    {
        ArgumentNullException.ThrowIfNull(galleryImageIds);
        ArgumentNullException.ThrowIfNull(variantImageIds);
        return galleryImageIds.Concat(variantImageIds).Distinct().Count();
    }

    /// <summary>Whether a product's image count is above the product constant P-24; the value is read from <see cref="ProductConstants.ImagesPerProductMax"/>.</summary>
    public static bool IsOverTheLimit(int imageCount) => imageCount > ProductConstants.ImagesPerProductMax;

    /// <summary>The firm's alternative text as it is kept: trimmed; empty or white space is none (the text is then generated, <see cref="ImageAltText"/>).</summary>
    public static string? CleanAltText(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
