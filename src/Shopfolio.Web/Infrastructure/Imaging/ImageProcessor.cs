using SkiaSharp;

namespace Shopfolio.Infrastructure.Imaging;

/// <summary>
/// The one place that touches the image library (09 15.6 item 1; the library is SkiaSharp, the table's decision K-1575 - the first candidate's
/// native libraries carry copyleft licenses, 09 3.3 item 4). The order is fixed and each step needs the one before it: the type from the first
/// bytes, the size from the header (before any decoding), the decode of the first frame into sRGB, the shrink, the orientation applied to the (shrunk) pixels, the
/// source copy encoded without any metadata. The request's source copy and the queue's derivatives share one limit on concurrent operations.
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
        ImageKind? kind = ImageKinds.Sniff(content);
        if (kind is null)
        {
            return ImageProcessing.Rejected(ImageRejection.UnsupportedType);
        }

        // The header: the codec reads the size and the type without decoding a pixel.
        using SKData data = SKData.CreateCopy(content);
        using SKCodec? codec = SKCodec.Create(data);
        if (codec is null || codec.EncodedFormat != EncodedFormatOf(kind.Value))
        {
            return ImageProcessing.Rejected(ImageRejection.Undecodable);
        }

        SKImageInfo header = codec.Info;
        long pixels = (long)header.Width * header.Height;
        if (header.Width > ImagingLimits.MaxEdge || header.Height > ImagingLimits.MaxEdge || pixels > ImagingLimits.MaxPixels
            || pixels * ImagingLimits.BytesPerPixel > ImagingLimits.MemoryLimitMegabytes * 1024L * 1024L)
        {
            return ImageProcessing.Rejected(ImageRejection.OverPixelLimit);
        }

        // The first frame only, converted into sRGB while decoding: the embedded colour profile is applied by the codec, a profile-less
        // image is taken as sRGB. A moving WebP or PNG is read as its first frame, so the frame count does not multiply the memory (05 6.6.2).
        using SKBitmap? decoded = Decode(codec, header);
        if (decoded is null)
        {
            return ImageProcessing.Rejected(ImageRejection.Undecodable);
        }

        // Reduced before it is turned: the turn makes a second copy, and the copy of a reduced image is small (the longest side does not change
        // with the turn, so the order does not change the result).
        SKBitmap current = decoded;
        using SKBitmap? reduced = ReduceToSourceLimit(current);
        if (reduced is not null)
        {
            current = reduced;
        }

        using SKBitmap? turned = Orient(current, codec.EncodedOrigin);
        if (turned is not null)
        {
            current = turned;
        }

        byte[] encoded = Encode(current, kind.Value);
        return ImageProcessing.Accepted(encoded, kind.Value, current.Width, current.Height);
    }

    // Only smaller: the longest side becomes the limit and the other side keeps the ratio; null when the image is within the limit.
    private static SKBitmap? ReduceToSourceLimit(SKBitmap source)
    {
        int longest = Math.Max(source.Width, source.Height);
        if (longest <= ImagingLimits.SourceMaxEdge)
        {
            return null;
        }

        return Reduce(source, Scaled(source.Width, ImagingLimits.SourceMaxEdge, longest), Scaled(source.Height, ImagingLimits.SourceMaxEdge, longest))
            ?? throw new InvalidOperationException("The reduced image could not be allocated.");
    }

    // A side times a ratio, rounded to the nearest pixel in integers, and never below one pixel (the one place pixel sizes are scaled).
    private static int Scaled(int side, int numerator, int denominator) =>
        Math.Max(1, (int)((((long)side * numerator) + (denominator / 2)) / denominator));

    private byte[] RenderDerivative(byte[] sourceCopy, int width)
    {
        _ = ImageKinds.Sniff(sourceCopy) ?? throw new ArgumentException("The source copy is not a known image.", nameof(sourceCopy));
        using SKData data = SKData.CreateCopy(sourceCopy);
        using SKCodec codec = SKCodec.Create(data) ?? throw new ArgumentException("The source copy cannot be decoded.", nameof(sourceCopy));
        using SKBitmap decoded = Decode(codec, codec.Info) ?? throw new ArgumentException("The source copy cannot be decoded.", nameof(sourceCopy));
        using SKBitmap reduced = Reduce(decoded, width, Scaled(decoded.Height, width, decoded.Width)) ?? throw new InvalidOperationException("The derivative could not be allocated.");
        return EncodeAs(reduced, SKEncodedImageFormat.Webp, ImagingLimits.DerivativeQuality);
    }

    private SKBitmap? Decode(SKCodec codec, SKImageInfo header)
    {
        SKImageInfo target = new(header.Width, header.Height, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        SKBitmap bitmap = new();
        if (!bitmap.TryAllocPixels(target))
        {
            bitmap.Dispose();
            return null;
        }

        Interlocked.Increment(ref _decodeCount);
        SKCodecResult result = codec.GetPixels(target, bitmap.GetPixels());
        if (result != SKCodecResult.Success)
        {
            bitmap.Dispose();
            return null;
        }

        return bitmap;
    }

    // The orientation is applied to the pixels; the stored copy has none (05 7.7.2).
    // Null when the image is stored upright: no second copy of the pixels is made.
    private static SKBitmap? Orient(SKBitmap source, SKEncodedOrigin origin)
    {
        if (origin == SKEncodedOrigin.TopLeft)
        {
            return null;
        }

        int w = source.Width;
        int h = source.Height;
        bool swaps = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        SKMatrix matrix = origin switch
        {
            SKEncodedOrigin.TopRight => Matrix(-1, 0, w, 0, 1, 0),
            SKEncodedOrigin.BottomRight => Matrix(-1, 0, w, 0, -1, h),
            SKEncodedOrigin.BottomLeft => Matrix(1, 0, 0, 0, -1, h),
            SKEncodedOrigin.LeftTop => Matrix(0, 1, 0, 1, 0, 0),
            SKEncodedOrigin.RightTop => Matrix(0, -1, h, 1, 0, 0),
            SKEncodedOrigin.RightBottom => Matrix(0, -1, h, -1, 0, w),
            SKEncodedOrigin.LeftBottom => Matrix(0, 1, 0, -1, 0, w),
            _ => SKMatrix.Identity,
        };
        SKBitmap target = new(new SKImageInfo(swaps ? h : w, swaps ? w : h, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
        using SKCanvas canvas = new(target);
        canvas.Clear(SKColors.Transparent);
        canvas.SetMatrix(matrix);
        using SKImage image = SKImage.FromBitmap(source);
        canvas.DrawImage(image, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
        return target;
    }

    // x' = a*x + b*y + c ; y' = d*x + e*y + f
    private static SKMatrix Matrix(float a, float b, float c, float d, float e, float f) => new(a, b, c, d, e, f, 0, 0, 1);

    private static SKBitmap? Reduce(SKBitmap source, int width, int height)
    {
        SKImageInfo info = new(width, height, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        return source.Resize(info, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
    }

    private static byte[] Encode(SKBitmap bitmap, ImageKind kind) => kind switch
    {
        ImageKind.Jpeg => EncodeAs(bitmap, SKEncodedImageFormat.Jpeg, ImagingLimits.SourceQuality),
        ImageKind.Png => EncodeAs(bitmap, SKEncodedImageFormat.Png, 100),
        ImageKind.WebP => EncodeAs(bitmap, SKEncodedImageFormat.Webp, ImagingLimits.SourceQuality),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown image format."),
    };

    // The encoder writes the pixels and nothing else; the pixel view carries no colour space, so no profile is embedded either (05 7.7.2).
    private static byte[] EncodeAs(SKBitmap bitmap, SKEncodedImageFormat format, int quality)
    {
        SKImageInfo bare = bitmap.Info.WithColorSpace(null);
        using SKPixmap pixels = new(bare, bitmap.GetPixels(), bitmap.RowBytes);
        using MemoryStream output = new();
        using (SKManagedWStream stream = new(output))
        {
            bool written = format switch
            {
                SKEncodedImageFormat.Jpeg => pixels.Encode(stream, new SKJpegEncoderOptions(quality)),
                SKEncodedImageFormat.Png => pixels.Encode(stream, SKPngEncoderOptions.Default),
                _ => pixels.Encode(stream, new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, quality)),
            };
            if (!written)
            {
                throw new InvalidOperationException("The image could not be encoded.");
            }
        }

        return output.ToArray();
    }

    private static SKEncodedImageFormat EncodedFormatOf(ImageKind kind) => kind switch
    {
        ImageKind.Jpeg => SKEncodedImageFormat.Jpeg,
        ImageKind.Png => SKEncodedImageFormat.Png,
        ImageKind.WebP => SKEncodedImageFormat.Webp,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown image format."),
    };
}
