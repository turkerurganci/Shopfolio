using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;

namespace Shopfolio.UnitTests.Imaging;

/// <summary>
/// The test images, made in memory (09 15.6 test: each gate has its own image; 09 19.2 item 2: a unit test writes no file). The huge image is a
/// real PNG header that declares a size and carries no pixels, so the size gate can be shown to refuse it before any decoding.
/// </summary>
internal static class ImageFixtures
{
    public static byte[] Jpeg(int width, int height, Action<Image<Rgba32>>? prepare = null)
    {
        using Image<Rgba32> image = new(width, height, Color.SteelBlue.ToPixel<Rgba32>());
        prepare?.Invoke(image);
        using MemoryStream output = new();
        image.Save(output, new JpegEncoder());
        return output.ToArray();
    }

    public static byte[] Png(int width, int height)
    {
        using Image<Rgba32> image = new(width, height, Color.SeaGreen.ToPixel<Rgba32>());
        using MemoryStream output = new();
        image.Save(output, new PngEncoder());
        return output.ToArray();
    }

    public static byte[] CmykJpeg(int width, int height)
    {
        using Image<Rgba32> image = new(width, height, Color.Orange.ToPixel<Rgba32>());
        using MemoryStream output = new();
        image.Save(output, new JpegEncoder { ColorType = JpegColorType.Cmyk });
        return output.ToArray();
    }

    /// <summary>A JPEG stored sideways with the EXIF orientation 6 (a phone photograph), a camera, a place and a name in its metadata.</summary>
    public static byte[] SidewaysJpegWithLocation(int storedWidth, int storedHeight)
    {
        return Jpeg(
            storedWidth,
            storedHeight,
            image =>
            {
                ExifProfile profile = new();
                profile.SetValue(ExifTag.Orientation, (ushort)6);
                profile.SetValue(ExifTag.Software, "camera-app-secret");
                profile.SetValue(ExifTag.GPSLatitudeRef, "N");
                profile.SetValue(ExifTag.GPSLatitude, [new Rational(41, 1), new Rational(0, 1), new Rational(0, 1)]);
                image.Metadata.ExifProfile = profile;
            });
    }

    /// <summary>A moving WebP of two frames.</summary>
    public static byte[] AnimatedWebP(int width, int height)
    {
        using Image<Rgba32> image = new(width, height, Color.Red.ToPixel<Rgba32>());
        using Image<Rgba32> second = new(width, height, Color.Blue.ToPixel<Rgba32>());
        image.Frames.AddFrame(second.Frames.RootFrame);
        using MemoryStream output = new();
        image.Save(output, new WebpEncoder { FileFormat = WebpFileFormatType.Lossy });
        return output.ToArray();
    }

    /// <summary>A PNG signature and header that declare the size and nothing more: a few dozen bytes that would be gigabytes decoded.</summary>
    public static byte[] HugePngHeader(int width, int height)
    {
        byte[] signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        byte[] header = new byte[13];
        WriteBigEndian(header, 0, width);
        WriteBigEndian(header, 4, height);
        header[8] = 8;
        header[9] = 2;
        byte[] typeAndData = [.. "IHDR"u8.ToArray(), .. header];
        byte[] chunk = new byte[4];
        WriteBigEndian(chunk, 0, header.Length);
        byte[] crc = new byte[4];
        WriteBigEndian(crc, 0, unchecked((int)Crc32(typeAndData)));
        return [.. signature, .. chunk, .. typeAndData, .. crc];
    }

    public static byte[] Svg() => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"10\" height=\"10\"><script>alert(1)</script></svg>"u8.ToArray();

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
