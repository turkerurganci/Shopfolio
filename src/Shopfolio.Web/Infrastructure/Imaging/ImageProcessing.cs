using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Infrastructure.Imaging;

/// <summary>Why an upload was refused; for the log. The user sees one code, <c>InvalidImage</c> (09 15.6 item 1; 07 6.2).</summary>
internal enum ImageRejection
{
    UnsupportedType,
    OverPixelLimit,
    Undecodable,
}

/// <summary>
/// The result of processing an upload (09 15.6 item 1): the re-encoded source copy and its facts, or a named rejection. A refused image is a
/// result, not an exception (09 8.1 item 1).
/// </summary>
internal sealed record ImageProcessing : IOperationResult
{
    private ImageProcessing()
    {
    }

    public bool IsRejected => Rejection is not null;

    public ImageRejection? Rejection { get; private init; }

    public byte[] SourceCopy { get; private init; } = [];

    public ImageFormat ImageFormat { get; private init; }

    public int Width { get; private init; }

    public int Height { get; private init; }

    public static ImageProcessing Accepted(byte[] sourceCopy, ImageFormat imageFormat, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(sourceCopy);
        return new ImageProcessing { SourceCopy = sourceCopy, ImageFormat = imageFormat, Width = width, Height = height };
    }

    public static ImageProcessing Rejected(ImageRejection rejection) => new() { Rejection = rejection };
}
