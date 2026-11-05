namespace Shopfolio.Infrastructure.Imaging;

/// <summary>
/// The image processing numbers (09 3.5 table row "Görüntü işleme", in code; 05 6.6.2, 7.7.2): the pixel ceiling, the derivative widths and
/// qualities, the number of concurrent operations and the library's memory. A number is written once, here.
/// </summary>
internal static class ImagingLimits
{
    /// <summary>The longest side of the stored source copy (05 7.7.2).</summary>
    public const int SourceMaxEdge = 2560;

    /// <summary>The longest side the header may declare (05 6.6.2).</summary>
    public const int MaxEdge = 10_000;

    /// <summary>The area the header may declare: 40 megapixels (05 6.6.2).</summary>
    public const long MaxPixels = 40_000_000;

    /// <summary>The widths of the storefront's derivatives; only those smaller than the source's width are made (05 7.7.2).</summary>
    public static IReadOnlyList<int> DerivativeWidths { get; } = [480, 960, 1600];

    public const int SourceQuality = 90;

    public const int DerivativeQuality = 80;

    /// <summary>Operations at the same time; the request's source copy and the queue's derivatives share the limit (09 15.6 item 4).</summary>
    public const int MaxConcurrent = 2;

    /// <summary>The library's memory: the ceiling of the one pixel buffer a decode allocates (the library has no allocator limit of its own).</summary>
    public const int MemoryLimitMegabytes = 256;

    /// <summary>Bytes of one decoded pixel (RGBA, 8 bits per channel).</summary>
    public const int BytesPerPixel = 4;
}
