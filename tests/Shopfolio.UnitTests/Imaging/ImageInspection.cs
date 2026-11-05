using System.Text;

using SkiaSharp;

namespace Shopfolio.UnitTests.Imaging;

/// <summary>What a test reads back from an image the processor produced: its size, its frame count, one pixel and the markers of metadata in its bytes.</summary>
internal static class ImageInspection
{
    /// <summary>The names of the metadata blocks of the formats (JPEG segments, PNG and WebP chunks): none may be left in a stored copy.</summary>
    public static readonly string[] MetadataMarkers =
    [
        "Exif", "http://ns.adobe.com/xap", "Photoshop 3.0", "ICC_PROFILE", "iCCP", "eXIf", "iTXt", "tEXt", "zTXt", "EXIF", "XMP ", "ICCP",
    ];

    public static (int Width, int Height, int FrameCount) Read(byte[] image)
    {
        using SKData data = SKData.CreateCopy(image);
        using SKCodec codec = SKCodec.Create(data) ?? throw new InvalidOperationException("The image has no readable header.");
        return (codec.Info.Width, codec.Info.Height, Math.Max(1, codec.FrameCount));
    }

    public static SKColor PixelAt(byte[] image, int x, int y)
    {
        using SKBitmap bitmap = SKBitmap.Decode(image) ?? throw new InvalidOperationException("The image cannot be decoded.");
        return bitmap.GetPixel(x, y);
    }

    public static bool Contains(byte[] image, string text) => image.AsSpan().IndexOf(Encoding.Latin1.GetBytes(text)) >= 0;

    public static IReadOnlyList<string> MarkersIn(byte[] image) => [.. MetadataMarkers.Where(marker => Contains(image, marker))];
}
