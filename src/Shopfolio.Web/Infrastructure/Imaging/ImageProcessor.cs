using Shopfolio.Modules.Catalog.Contracts;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Shopfolio.Infrastructure.Imaging;

/// <summary>
/// The one place that touches the image library (09 15.6 item 1; the library is the table's fallback, ImageSharp - the first candidate's
/// native libraries carry copyleft licenses, 09 3.3 item 4). The order is fixed and each step needs the one before it: the type from the first
/// bytes, the size from the header (before any decoding), the decode of the first frame, the orientation applied to the pixels, the metadata
/// dropped, the source copy encoded. The request's source copy and the queue's derivatives share one limit on concurrent operations.
/// </summary>
internal sealed class ImageProcessor : IDisposable
{
    private readonly SemaphoreSlim _gate = new(ImagingLimits.MaxConcurrent, ImagingLimits.MaxConcurrent);
    private int _decodeCount;

    /// <summary>How many times the library decoded pixels; the header read does not count (the test of 05 6.6.2 reads it).</summary>
    public int DecodeCount => Volatile.Read(ref _decodeCount);

    public void Dispose() => _gate.Dispose();

    public async Task<ImageProcessing> ProcessUploadAsync(byte[] content, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(content);
        await _gate.WaitAsync(ct);
        try
        {
            return ProcessUpload(content);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<byte[]> RenderDerivativeAsync(byte[] sourceCopy, int width, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(sourceCopy);
        await _gate.WaitAsync(ct);
        try
        {
            return RenderDerivative(sourceCopy, width);
        }
        finally
        {
            _gate.Release();
        }
    }

    private ImageProcessing ProcessUpload(byte[] content)
    {
        ImageFormat? kind = ImageKinds.Sniff(content);
        if (kind is null)
        {
            return ImageProcessing.Rejected(ImageRejection.UnsupportedType);
        }

        IImageDecoder decoder = DecoderOf(kind.Value);
        DecoderOptions options = Options();
        try
        {
            ImageInfo? info;
            using (MemoryStream header = new(content, writable: false))
            {
                info = decoder.Identify(options, header);
            }

            if (info is null)
            {
                return ImageProcessing.Rejected(ImageRejection.Undecodable);
            }

            if (info.Width > ImagingLimits.MaxEdge || info.Height > ImagingLimits.MaxEdge || (long)info.Width * info.Height > ImagingLimits.MaxPixels)
            {
                return ImageProcessing.Rejected(ImageRejection.OverPixelLimit);
            }

            using Image<Rgba32> image = Decode(decoder, options, content);
            image.Mutate(context => context.AutoOrient());
            if (Math.Max(image.Width, image.Height) > ImagingLimits.SourceMaxEdge)
            {
                // Only smaller: the longest side becomes the limit and the other side keeps the ratio.
                Size target = image.Width >= image.Height ? new Size(ImagingLimits.SourceMaxEdge, 0) : new Size(0, ImagingLimits.SourceMaxEdge);
                image.Mutate(context => context.Resize(target.Width, target.Height));
            }

            Strip(image);
            return ImageProcessing.Accepted(Encode(image, kind.Value), kind.Value, image.Width, image.Height);
        }
        // The library throws a NullReferenceException from its header reader on some corrupt headers (found by the broken-file test, ImageSharp 4.1.3):
        // for an upload that is a file that cannot be decoded, not an unexpected failure of the application.
        catch (Exception exception) when (exception is ImageFormatException or InvalidImageContentException or UnknownImageFormatException
            or InvalidMemoryOperationException or NotSupportedException or EndOfStreamException or IOException
            or NullReferenceException)
        {
            return ImageProcessing.Rejected(ImageRejection.Undecodable);
        }
    }

    private byte[] RenderDerivative(byte[] sourceCopy, int width)
    {
        ImageFormat kind = ImageKinds.Sniff(sourceCopy) ?? throw new ArgumentException("The source copy is not a known image.", nameof(sourceCopy));
        using Image<Rgba32> image = Decode(DecoderOf(kind), Options(), sourceCopy);
        image.Mutate(context => context.Resize(width, 0));
        Strip(image);
        using MemoryStream output = new();
        image.Save(output, new WebpEncoder { Quality = ImagingLimits.DerivativeQuality, FileFormat = WebpFileFormatType.Lossy, SkipMetadata = true });
        return output.ToArray();
    }

    private Image<Rgba32> Decode(IImageDecoder decoder, DecoderOptions options, byte[] content)
    {
        Interlocked.Increment(ref _decodeCount);
        using MemoryStream stream = new(content, writable: false);
        return decoder.Decode<Rgba32>(options, stream);
    }

    // The first frame only: a moving WebP or PNG is read as its first frame, so the frame count does not multiply the memory (05 6.6.2).
    // The embedded colour profile is applied while decoding; a profile-less CMYK JPEG comes out as RGB (the library's own conversion).
    private static DecoderOptions Options() => new()
    {
        Configuration = ImagingLimits.Configuration,
        MaxFrames = 1,
        SkipMetadata = false,
        ColorProfileHandling = ColorProfileHandling.Convert,
    };

    private static IImageDecoder DecoderOf(ImageFormat kind) => kind switch
    {
        ImageFormat.Jpeg => JpegDecoder.Instance,
        ImageFormat.Png => PngDecoder.Instance,
        ImageFormat.WebP => WebpDecoder.Instance,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown image format."),
    };

    // The orientation was applied before this runs; metadata does not fall off a decoded image by itself (05 7.7.2).
    private static void Strip(Image image)
    {
        image.Metadata.ExifProfile = null;
        image.Metadata.XmpProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.IccProfile = null;
        image.Metadata.CicpProfile = null;
        image.Metadata.GetPngMetadata().TextData.Clear();
        foreach (ImageFrame frame in image.Frames)
        {
            frame.Metadata.ExifProfile = null;
            frame.Metadata.XmpProfile = null;
            frame.Metadata.IccProfile = null;
            frame.Metadata.IptcProfile = null;
            frame.Metadata.CicpProfile = null;
        }
    }

    private static byte[] Encode(Image<Rgba32> image, ImageFormat kind)
    {
        using MemoryStream output = new();
        switch (kind)
        {
            case ImageFormat.Jpeg:
                image.Save(output, new JpegEncoder { Quality = ImagingLimits.SourceQuality, SkipMetadata = true });
                break;
            case ImageFormat.Png:
                image.Save(output, new PngEncoder { SkipMetadata = true });
                break;
            default:
                image.Save(output, new WebpEncoder { Quality = ImagingLimits.SourceQuality, FileFormat = WebpFileFormatType.Lossy, SkipMetadata = true });
                break;
        }

        return output.ToArray();
    }
}
