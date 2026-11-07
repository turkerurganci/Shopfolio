using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// One image of the gallery as the product form posts it (02 3.11.2; 06 4.71): the uploaded image, the firm's alternative text - empty or white space is none - and whether the form marks
/// it as the main image. The position in the posted list is the gallery order (02 3.11.2: the firm sets it by hand). Never a row of the entity itself (09 6 item 2).
/// </summary>
public sealed record GalleryEntry(Guid ImageId, string? AltText, bool IsMain);

/// <summary>
/// An image of a product's gallery as it is shown (02 3.11.2, 3.11.4): its place (1 up to the count, in the gallery order), the main mark, the alternative text the firm wrote - null when
/// it wrote none - and the text the page uses, which is never empty: the firm's when written, else the generated "product name - image 2 / 5".
/// </summary>
public sealed record GalleryImage(Guid ImageId, int Position, bool IsMain, string? AltText, string EffectiveAltText);

/// <summary>
/// The image a variant shows (02 3.11.1, 3.11.4): its own when it carries one - the text then names the product and the variant's option values - else the product's main image, whose text is
/// the gallery's. <paramref name="IsOwn"/> tells which. The text is never empty.
/// </summary>
public sealed record VariantImage(Guid ImageId, bool IsOwn, string AltText);

/// <summary>
/// The result of saving a product's gallery (02 3.11.2, 3.11.3; 07 6.2): saved, or one of the named refusals. A refusal writes nothing. The refusals are fixed instances; the reason is for the
/// log, and the user-facing code of the limit is <c>ImageLimitExceeded</c> (07 6.2).
/// </summary>
public sealed class GallerySaveResult : IOperationResult
{
    private GallerySaveResult(string? reason)
    {
        Reason = reason;
    }

    public static GallerySaveResult Saved { get; } = new(null);

    /// <summary>The product's images - the gallery's and its variants' together, each image once - would be more than P-24.</summary>
    public static GallerySaveResult LimitExceeded { get; } = new("LimitExceeded");

    /// <summary>An image of the list is not an uploaded image.</summary>
    public static GallerySaveResult ImageNotFound { get; } = new("ImageNotFound");

    /// <summary>The list names one image twice.</summary>
    public static GallerySaveResult DuplicateImage { get; } = new("DuplicateImage");

    public static GallerySaveResult ProductNotFound { get; } = new("ProductNotFound");

    /// <summary>Why the gallery was refused; null when it was saved.</summary>
    public string? Reason { get; }

    public bool IsRejected => Reason is not null;
}

/// <summary>The result of giving a variant its own image or taking it away (02 3.11.1, 3.11.3): set, or a named refusal that writes nothing.</summary>
public sealed class VariantImageResult : IOperationResult
{
    private VariantImageResult(string? reason)
    {
        Reason = reason;
    }

    public static VariantImageResult Set { get; } = new(null);

    /// <summary>With this image the product's images - the gallery's and its variants' together, each image once - would be more than P-24.</summary>
    public static VariantImageResult LimitExceeded { get; } = new("LimitExceeded");

    public static VariantImageResult ImageNotFound { get; } = new("ImageNotFound");

    public static VariantImageResult VariantNotFound { get; } = new("VariantNotFound");

    /// <summary>Why the write was refused; null when it was done.</summary>
    public string? Reason { get; }

    public bool IsRejected => Reason is not null;
}
