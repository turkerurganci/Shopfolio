namespace Shopfolio.Infrastructure;

/// <summary>
/// The pure rules every image gallery shares (02 3.11.2, 3.11.4, 3.27.15): the product's gallery (Catalog) and the corporate content's galleries (CorporateContent) keep them in this one place, so the
/// two cannot drift apart (09 25 "tek işlev"). The main image is a mark of its own, independent of the order, and a gallery that has an image has exactly one main image; the firm's alternative text is
/// optional, and when it is empty a generated text - the record's name and the image's place in the gallery - stands in, never empty.
/// </summary>
internal static class GalleryText
{
    private const string Dash = " — ";
    private const string ImageWord = "görsel";

    /// <summary>The firm's alternative text as it is kept: trimmed; empty or white space is none.</summary>
    public static string? CleanAltText(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    /// <summary>The generated text of a gallery image: the name and the image's place counted from 1 out of the gallery's count ("Ad — görsel 2 / 5").</summary>
    public static string Generated(string name, int position, int total)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(total, position);
        return $"{name}{Dash}{ImageWord} {position} / {total}";
    }

    /// <summary>The firm's text when it wrote one, else the generated one.</summary>
    public static string Choose(string? firmText, Func<string> generated)
    {
        ArgumentNullException.ThrowIfNull(generated);
        return CleanAltText(firmText) ?? generated();
    }

    /// <summary>
    /// The image that is main after a save: the one the list marks (the first marked one if the list marks several), else the current main image when it is still in the list, else the first image in
    /// the order; null for an empty gallery. <paramref name="currentMainImageId"/> is the main image before the save, null when there is none.
    /// </summary>
    public static Guid? MainOf(IReadOnlyList<(Guid ImageId, bool IsMain)> entries, Guid? currentMainImageId)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (entries.Count == 0)
        {
            return null;
        }

        foreach ((Guid imageId, bool isMain) in entries)
        {
            if (isMain)
            {
                return imageId;
            }
        }

        if (currentMainImageId is { } current && entries.Any(e => e.ImageId == current))
        {
            return current;
        }

        return entries[0].ImageId;
    }
}
