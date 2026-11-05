using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>Why an image upload was refused; for the log. The user sees one code, <c>InvalidImage</c> (09 15.6 items 1, 7; 07 6.2).</summary>
public enum ImageUploadRefusal
{
    TooLarge,
    UnsupportedType,
    OverPixelLimit,
    Undecodable,
}

/// <summary>The result of an image upload (09 15.6 item 7): the new unlinked image's id, or the refusal.</summary>
public sealed class ImageUploadResult : IOperationResult
{
    private ImageUploadResult(Guid? imageId, ImageUploadRefusal? refusal)
    {
        ImageId = imageId;
        Refusal = refusal;
    }

    public Guid? ImageId { get; }

    public ImageUploadRefusal? Refusal { get; }

    public bool IsRejected => Refusal is not null;

    public static ImageUploadResult Uploaded(Guid imageId) => new(imageId, null);

    public static ImageUploadResult Refused(ImageUploadRefusal refusal) => new(null, refusal);
}
