using System.IO.Compression;

using Shopfolio.UnitTests.Support;

using SkiaSharp;

namespace Shopfolio.UnitTests.Imaging;

/// <summary>
/// The test images (09 15.6 test: each gate has its own image; 09 19.2 item 2: a unit test writes no file). The plain ones are made in memory.
/// The ones the library cannot write itself - a JPEG with EXIF orientation, location, XMP and IPTC; a CMYK JPEG; a moving WebP - are small
/// files under <c>Imaging/fixtures/</c> (09 15.6 test), read only. The huge image is a real, tiny PNG that declares a huge size, so the
/// size gate can be shown to refuse it before any decoding.
/// </summary>
internal static class ImageFixtures
{
    public static byte[] Jpeg(int width, int height) => Encode(width, height, SKColors.SteelBlue, SKEncodedImageFormat.Jpeg);

    public static byte[] Png(int width, int height) => Encode(width, height, SKColors.SeaGreen, SKEncodedImageFormat.Png);

    /// <summary>A CMYK JPEG without a colour profile (file).</summary>
    public static byte[] CmykJpeg() => FromFile("cmyk.jpg");

    /// <summary>
    /// A JPEG stored sideways, 80 by 40, with the EXIF orientation 6 (a phone photograph); a camera name, a place, an XMP packet and an IPTC
    /// caption are in its metadata. Its left 20 columns, as stored, are red and the rest steel blue (file).
    /// </summary>
    public static byte[] SidewaysJpegWithMetadata() => FromFile("sideways-with-metadata.jpg");

    /// <summary>A moving WebP of two frames, 48 by 32: red, then blue (file).</summary>
    public static byte[] AnimatedWebP() => FromFile("animated.webp");

    /// <summary>
    /// A valid one-bit PNG of the given size with every pixel black: it compresses to a few kilobytes and would be hundreds of megabytes decoded
    /// to four bytes a pixel. The library's own codec must read its size and the gate must refuse it before any pixel is decoded.
    /// </summary>
    public static byte[] HugePng(int width, int height)
    {
        byte[] header = new byte[13];
        WriteBigEndian(header, 0, width);
        WriteBigEndian(header, 4, height);
        header[8] = 1;
        header[9] = 0;
        byte[] row = new byte[((width + 7) / 8) + 1];
        using MemoryStream compressed = new();
        using (ZLibStream deflate = new(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            for (int y = 0; y < height; y++)
            {
                deflate.Write(row);
            }
        }

        using MemoryStream png = new();
        png.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        WriteChunk(png, "IHDR", header);
        WriteChunk(png, "IDAT", compressed.ToArray());
        WriteChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static void WriteChunk(Stream into, string type, byte[] data)
    {
        byte[] length = new byte[4];
        WriteBigEndian(length, 0, data.Length);
        byte[] typeAndData = [.. System.Text.Encoding.ASCII.GetBytes(type), .. data];
        byte[] crc = new byte[4];
        WriteBigEndian(crc, 0, unchecked((int)Crc32(typeAndData)));
        into.Write(length);
        into.Write(typeAndData);
        into.Write(crc);
    }

    public static byte[] Svg() => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"10\" height=\"10\"><script>alert(1)</script></svg>"u8.ToArray();

    private static byte[] FromFile(string name) =>
        File.ReadAllBytes(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Imaging", "fixtures", name));

    private static byte[] Encode(int width, int height, SKColor color, SKEncodedImageFormat format)
    {
        using SKBitmap bitmap = new(width, height);
        bitmap.Erase(color);
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(format, 90);
        return data.ToArray();
    }

    private static void WriteBigEndian(byte[] into, int at, int value)
    {
        into[at] = (byte)(value >> 24);
        into[at + 1] = (byte)(value >> 16);
        into[at + 2] = (byte)(value >> 8);
        into[at + 3] = (byte)value;
    }

    private static uint Crc32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            }
        }

        return ~crc;
    }
}
