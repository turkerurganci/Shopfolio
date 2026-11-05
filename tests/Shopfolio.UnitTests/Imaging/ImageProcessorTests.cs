using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Modules.Catalog.Contracts;

using SkiaSharp;

namespace Shopfolio.UnitTests.Imaging;

/// <summary>
/// The image gates (T223 criteria 3, 4, 6, 7; 05 6.6.1, 6.6.2, 7.7.2; 09 15.6 test): one image per gate, made in memory. The library is the
/// table's SkiaSharp (K-1575); the contract of <see cref="ImageProcessor"/> and these tests do not name it except to read an image back.
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
        Assert.Equal(ImageKind.Png, result.Kind);
        Assert.Equal(ImageKind.Png, ImageKinds.Sniff(result.SourceCopy));
    }

    [Fact]
    public async Task AHugeSmallFile_IsRefusedFromItsHeader_WithoutDecoding()
    {
        using ImageProcessor processor = new();
        byte[] tooLongEdge = ImageFixtures.HugePng(ImagingLimits.MaxEdge + 1, 10);
        byte[] tooManyPixels = ImageFixtures.HugePng(ImagingLimits.MaxEdge, (int)(ImagingLimits.MaxPixels / ImagingLimits.MaxEdge) + 1);

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

        byte[] sideways = ImageFixtures.SidewaysJpegWithMetadata();

        ImageProcessing result = await processor.ProcessUploadAsync(sideways, Ct);

        // The fixture carries what the gate must drop: EXIF with a place and a camera, XMP, IPTC.
        Assert.All(["Exif", "http://ns.adobe.com/xap", "Photoshop 3.0", "camera-app-secret", "secret-person", "secret-caption"], marker => Assert.True(ImageInspection.Contains(sideways, marker), marker));
        Assert.False(result.IsRejected);
        Assert.Equal((40, 80), (result.Width, result.Height));
        Assert.Equal((40, 80, 1), ImageInspection.Read(result.SourceCopy));
        Assert.Empty(ImageInspection.MarkersIn(result.SourceCopy));
        Assert.False(ImageInspection.Contains(result.SourceCopy, "secret"));
        // Orientation 6 turns the stored left edge (red) into the top edge.
        SKColor top = ImageInspection.PixelAt(result.SourceCopy, 20, 5);
        SKColor bottom = ImageInspection.PixelAt(result.SourceCopy, 20, 75);
        Assert.True(top.Red > 200 && top.Blue < 80, $"top was {top}");
        Assert.True(bottom.Blue > 100 && bottom.Red < 100, $"bottom was {bottom}");
    }

    [Fact]
    public async Task AMovingWebP_IsEncodedFromItsFirstFrame_AsOneFrame()
    {
        using ImageProcessor processor = new();

        byte[] moving = ImageFixtures.AnimatedWebP();

        ImageProcessing result = await processor.ProcessUploadAsync(moving, Ct);

        Assert.Equal(2, ImageInspection.Read(moving).FrameCount);
        Assert.False(result.IsRejected);
        Assert.Equal(ImageKind.WebP, result.Kind);
        Assert.Equal((48, 32, 1), ImageInspection.Read(result.SourceCopy));
        SKColor pixel = ImageInspection.PixelAt(result.SourceCopy, 10, 10);
        Assert.True(pixel.Red > 150 && pixel.Blue < 100, $"the first frame is red, the pixel was {pixel}");
    }

    [Fact]
    public async Task ACmykJpeg_IsAccepted_AndComesOutAsRgb()
    {
        using ImageProcessor processor = new();

        ImageProcessing result = await processor.ProcessUploadAsync(ImageFixtures.CmykJpeg(), Ct);

        Assert.False(result.IsRejected);
        Assert.Equal((64, 64, 1), ImageInspection.Read(result.SourceCopy));
        Assert.Empty(ImageInspection.MarkersIn(result.SourceCopy));
        // The fixture is orange: red high, blue low, in plain RGB.
        SKColor pixel = ImageInspection.PixelAt(result.SourceCopy, 32, 32);
        Assert.True(pixel.Red > 200 && pixel.Blue < 90, $"the pixel was {pixel}");
    }

    [Fact]
    public async Task APlainImage_KeepsItsColour_AndCarriesNoColourProfile()
    {
        using ImageProcessor processor = new();

        ImageProcessing jpeg = await processor.ProcessUploadAsync(ImageFixtures.Jpeg(64, 64), Ct);
        ImageProcessing png = await processor.ProcessUploadAsync(ImageFixtures.Png(64, 64), Ct);

        Assert.Empty(ImageInspection.MarkersIn(jpeg.SourceCopy));
        Assert.Empty(ImageInspection.MarkersIn(png.SourceCopy));
        SKColor steel = ImageInspection.PixelAt(jpeg.SourceCopy, 32, 32);
        Assert.True(Math.Abs(steel.Red - SKColors.SteelBlue.Red) < 6 && Math.Abs(steel.Blue - SKColors.SteelBlue.Blue) < 6, $"the pixel was {steel}");
        Assert.Equal(SKColors.SeaGreen, ImageInspection.PixelAt(png.SourceCopy, 32, 32));
    }

    [Fact]
    public async Task ADerivative_IsAStillWebPOfTheWantedWidth_WithoutMetadata()
    {
        using ImageProcessor processor = new();
        ImageProcessing source = await processor.ProcessUploadAsync(ImageFixtures.Jpeg(1200, 600), Ct);

        byte[] derivative = await processor.RenderDerivativeAsync(source.SourceCopy, 480, Ct);

        Assert.Equal(ImageKind.WebP, ImageKinds.Sniff(derivative));
        Assert.Equal((480, 240, 1), ImageInspection.Read(derivative));
        Assert.Empty(ImageInspection.MarkersIn(derivative));
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
