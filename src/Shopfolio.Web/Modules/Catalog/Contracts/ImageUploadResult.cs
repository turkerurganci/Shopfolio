using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The result of an image upload (09 15.6 item 7): the new unlinked image's id, or a refusal. The refusals are named by <see cref="Reason"/>, for
/// the log; the user sees one code, <c>InvalidImage</c> (07 6.2). The refusals are fixed instances, like the other write results (no enum: the
/// reason is not a stored value, 06 5).
/// </summary>
public sealed class ImageUploadResult : IOperationResult
{
    private ImageUploadResult(Guid? imageId, string? reason)
    {
        ImageId = imageId;
        Reason = reason;
    }

    public static ImageUploadResult TooLarge { get; } = new(null, "TooLarge");

    public static ImageUploadResult UnsupportedType { get; } = new(null, "UnsupportedType");

    public static ImageUploadResult OverPixelLimit { get; } = new(null, "OverPixelLimit");

    public static ImageUploadResult Undecodable { get; } = new(null, "Undecodable");

    public Guid? ImageId { get; }

    /// <summary>Why the upload was refused; null when it was accepted.</summary>
    public string? Reason { get; }

    public bool IsRejected => Reason is not null;

    public static ImageUploadResult Uploaded(Guid imageId) => new(imageId, null);
}
