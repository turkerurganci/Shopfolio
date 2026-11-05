using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Infrastructure.Imaging;

/// <summary>
/// The type of an image read from its first bytes (05 6.6.1; MK-54; 09 15.6 item 2a): not from the extension and not from what the client said.
/// Only P-22's formats are known; everything else - SVG, PDF, scripts - is not an image of the product.
/// </summary>
internal static class ImageKinds
{
    private static ReadOnlySpan<byte> JpegMagic => [0xFF, 0xD8, 0xFF];

    private static ReadOnlySpan<byte> PngMagic => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static ImageFormat? Sniff(ReadOnlySpan<byte> content)
    {
        if (content.StartsWith(JpegMagic))
        {
            return ImageFormat.Jpeg;
        }

        if (content.StartsWith(PngMagic))
        {
            return ImageFormat.Png;
        }

        // RIFF, four size bytes, WEBP.
        if (content.Length >= 12 && content[..4].SequenceEqual("RIFF"u8) && content.Slice(8, 4).SequenceEqual("WEBP"u8))
        {
            return ImageFormat.WebP;
        }

        return null;
    }
}
