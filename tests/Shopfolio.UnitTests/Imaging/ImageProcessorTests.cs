using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Modules.Catalog.Contracts;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Shopfolio.UnitTests.Imaging;

/// <summary>
/// The image gates (T223 criteria 3, 4, 6, 7; 05 6.6.1, 6.6.2, 7.7.2; 09 15.6 test): one image per gate, made in memory. The library is the
/// table's fallback (ImageSharp); the contract of <see cref="ImageProcessor"/> and these tests are the same for either library (09 15.6 item 8).
/// </summary>
public sealed class ImageProcessorTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task AnSvgWithAnImageName_IsUnsupported_AndNeverReachesTheLibrary()
    {
        using ImageProcessor processor = new();

        ImageProcessing result = await processor.ProcessUploadAsync(ImageFixtures.Svg(), Ct);

        Assert.Equal(ImageRejection.UnsupportedType, result.Rejection);
        Assert.Equal(0, processor.DecodeCount);
    }

    [Fact]
    public async Task APngContent_IsReadAsPng_WhateverTheFileIsCalled()
    {
        using ImageProcessor processor = new();

        ImageProcessing result = await processor.ProcessUploadAsync(ImageFixtures.Png(40, 20), Ct);

        Assert.False(result.IsRejected);
        Assert.Equal(ImageFormat.Png, result.ImageFormat);
        Assert.Equal(ImageFormat.Png, ImageKinds.Sniff(result.SourceCopy));
    }

    [Fact]
    public async Task AHugeSmallFile_IsRefusedFromItsHeader_WithoutDecoding()
    {
        using ImageProcessor processor = new();
        byte[] tooLongEdge = ImageFixtures.HugePngHeader(ImagingLimits.MaxEdge + 1, 10);
        byte[] tooManyPixels = ImageFixtures.HugePngHeader(ImagingLimits.MaxEdge, (int)(ImagingLimits.MaxPixels / ImagingLimits.MaxEdge) + 1);

        ImageProcessing edge = await processor.ProcessUploadAsync(tooLongEdge, Ct);
        ImageProcessing area = await processor.ProcessUploadAsync(tooManyPixels, Ct);

        Assert.Equal(ImageRejection.OverPixelLimit, edge.Rejection);
        Assert.Equal(ImageRejection.OverPixelLimit, area.Rejection);
        Assert.Equal(0, processor.DecodeCount);
    }

    [Fact]
    public async Task ABrokenFile_IsUndecodable_AndATruncatedOneToo()
    {
        using ImageProcessor processor = new();
        byte[] truncated = ImageFixtures.Jpeg(64, 64)[..40];
        byte[] justTheSignature = [0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02];

        Assert.Equal(ImageRejection.Undecodable, (await processor.ProcessUploadAsync(truncated, Ct)).Rejection);
        Assert.Equal(ImageRejection.Undecodable, (await processor.ProcessUploadAsync(justTheSignature, Ct)).Rejection);
    }

    [Fact]
    public async Task ABigSource_IsReducedToTheLongestSideLimit_AndASmallOneIsNotEnlarged()
    {
        using ImageProcessor processor = new();

        ImageProcessing big = await processor.ProcessUploadAsync(ImageFixtures.Jpeg(ImagingLimits.SourceMaxEdge + 440, 1000), Ct);
        ImageProcessing small = await processor.ProcessUploadAsync(ImageFixtures.Jpeg(300, 200), Ct);

        Assert.Equal(ImagingLimits.SourceMaxEdge, big.Width);
        Assert.Equal(Math.Round(1000.0 * ImagingLimits.SourceMaxEdge / (ImagingLimits.SourceMaxEdge + 440)), big.Height, 1);
        Assert.Equal((300, 200), (small.Width, small.Height));
    }

    [Fact]
    public async Task TheOrientationIsAppliedToThePixels_AndNoMetadataIsLeft()
    {
        using ImageProcessor processor = new();

        ImageProcessing result = await processor.ProcessUploadAsync(ImageFixtures.SidewaysJpegWithLocation(80, 40), Ct);

        Assert.False(result.IsRejected);
        Assert.Equal((40, 80), (result.Width, result.Height));
        ImageInfo info = await Image.IdentifyAsync(new MemoryStream(result.SourceCopy), Ct);
        Assert.Equal((40, 80), (info.Width, info.Height));
        Assert.Null(info.Metadata.ExifProfile);
        Assert.Null(info.Metadata.XmpProfile);
        Assert.Null(info.Metadata.IptcProfile);
        Assert.Null(info.Metadata.IccProfile);
    }

    [Fact]
    public async Task AMovingWebP_IsEncodedFromItsFirstFrame_AsOneFrame()
    {
        using ImageProcessor processor = new();

        ImageProcessing result = await processor.ProcessUploadAsync(ImageFixtures.AnimatedWebP(48, 32), Ct);

        Assert.False(result.IsRejected);
        Assert.Equal(ImageFormat.WebP, result.ImageFormat);
        ImageInfo info = await Image.IdentifyAsync(new MemoryStream(result.SourceCopy), Ct);
        using Image decoded = Image.Load(result.SourceCopy);
        Assert.Single(decoded.Frames);
    }

    [Fact]
    public async Task ACmykJpeg_IsAccepted_AndComesOutAsRgb()
    {
        using ImageProcessor processor = new();

        ImageProcessing result = await processor.ProcessUploadAsync(ImageFixtures.CmykJpeg(64, 64), Ct);

        Assert.False(result.IsRejected);
        using Image<Rgba32> decoded = Image.Load<Rgba32>(result.SourceCopy);
        Assert.Equal((64, 64), (decoded.Width, decoded.Height));
    }

    [Fact]
    public async Task ADerivative_IsAStillWebPOfTheWantedWidth_WithoutMetadata()
    {
        using ImageProcessor processor = new();
        ImageProcessing source = await processor.ProcessUploadAsync(ImageFixtures.Jpeg(1200, 600), Ct);

        byte[] derivative = await processor.RenderDerivativeAsync(source.SourceCopy, 480, Ct);

        Assert.Equal(ImageFormat.WebP, ImageKinds.Sniff(derivative));
        ImageInfo info = await Image.IdentifyAsync(new MemoryStream(derivative), Ct);
        Assert.Equal((480, 240), (info.Width, info.Height));
        using Image decodedDerivative = Image.Load(derivative);
        Assert.Single(decodedDerivative.Frames);
        Assert.Null(info.Metadata.ExifProfile);
    }

    [Fact]
    public void DerivativeWidths_AreOnlyThoseSmallerThanTheSource()
    {
        Assert.Equal(ImagingLimits.DerivativeWidths, Shopfolio.Modules.Catalog.ImageServing.DerivativeWidthsFor(ImagingLimits.SourceMaxEdge));
        Assert.Equal([ImagingLimits.DerivativeWidths[0]], Shopfolio.Modules.Catalog.ImageServing.DerivativeWidthsFor(ImagingLimits.DerivativeWidths[1]));
        Assert.Empty(Shopfolio.Modules.Catalog.ImageServing.DerivativeWidthsFor(ImagingLimits.DerivativeWidths[0]));
    }

    [Fact]
    public void AnImageWithoutDerivatives_IsServedFromTheSourceCopy_AndOnceReadyFromTheNarrowestWideEnoughDerivative()
    {
        string key = new('a', FileKeys.ImageFingerprintLength);
        int first = ImagingLimits.DerivativeWidths[0];
        int second = ImagingLimits.DerivativeWidths[1];

        Assert.Equal(key, Shopfolio.Modules.Catalog.ImageServing.KeyFor(key, ImagingLimits.SourceMaxEdge, derivativesReadyAt: null, wantedWidth: first));
        DateTimeOffset ready = DateTimeOffset.UnixEpoch;
        Assert.Equal(FileKeys.DerivativeKey(key, first), Shopfolio.Modules.Catalog.ImageServing.KeyFor(key, ImagingLimits.SourceMaxEdge, ready, first));
        Assert.Equal(FileKeys.DerivativeKey(key, second), Shopfolio.Modules.Catalog.ImageServing.KeyFor(key, ImagingLimits.SourceMaxEdge, ready, first + 1));
        Assert.Equal(key, Shopfolio.Modules.Catalog.ImageServing.KeyFor(key, ImagingLimits.SourceMaxEdge, ready, ImagingLimits.SourceMaxEdge));
    }
}
