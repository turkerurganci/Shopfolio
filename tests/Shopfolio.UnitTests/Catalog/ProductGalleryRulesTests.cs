using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The pure rules of a product's gallery and the alternative text of its images (T339 criteria 2, 4, 6; KK2, KK4, KK6; 02 3.11.2-3.11.4; K-1069, K-564): the main image is a mark of its own,
/// a gallery that has an image has exactly one main image, the count of a product's images is the gallery's and its variants' together with each image once, the limit is the product
/// constant, and the generated text is never empty and tells the product's images apart.
/// </summary>
public sealed class ProductGalleryRulesTests
{
    private static readonly Guid A = Guid.Parse("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid B = Guid.Parse("00000000-0000-0000-0000-00000000000b");
    private static readonly Guid C = Guid.Parse("00000000-0000-0000-0000-00000000000c");

    private static GalleryEntry Entry(Guid id, bool isMain = false, string? alt = null) => new(id, alt, isMain);

    // ---- main image (criterion 2; KK2) ----

    [Fact]
    public void TheFirstImageAdded_ComesAsTheMainImage_WhateverTheFormMarked()
    {
        Assert.Equal(A, GalleryRules.MainOf([Entry(A)], currentMainImageId: null));
        Assert.Equal(A, GalleryRules.MainOf([Entry(A), Entry(B)], currentMainImageId: null));
    }

    [Fact]
    public void AnEmptyGallery_HasNoMainImage()
    {
        Assert.Null(GalleryRules.MainOf([], currentMainImageId: A));
    }

    [Fact]
    public void TheMainMark_IsIndependentOfTheOrder_AndTheMarkedImageWinsOverTheCurrentOne()
    {
        // The marked image is last in the order; the order does not decide.
        Assert.Equal(C, GalleryRules.MainOf([Entry(A), Entry(B), Entry(C, isMain: true)], currentMainImageId: A));

        // The current main image stays main while it is in the list and nothing else is marked, wherever it moved to.
        Assert.Equal(B, GalleryRules.MainOf([Entry(A), Entry(C), Entry(B)], currentMainImageId: B));
    }

    [Fact]
    public void WhenTheMainImageLeaves_TheFirstImageInTheOrderBecomesMain()
    {
        Assert.Equal(B, GalleryRules.MainOf([Entry(B), Entry(C)], currentMainImageId: A));
        Assert.Equal(C, GalleryRules.MainOf([Entry(C), Entry(B)], currentMainImageId: A));
    }

    [Fact]
    public void WhenTheFormMarksSeveralImages_TheFirstMarkedOneIsTheOnlyMain()
    {
        Assert.Equal(B, GalleryRules.MainOf([Entry(A), Entry(B, isMain: true), Entry(C, isMain: true)], currentMainImageId: null));
    }

    // ---- the limit (criterion 4; KK4) ----

    [Fact]
    public void TheCountOfAProductsImages_IsTheGalleryAndTheVariantsTogether_EachImageOnce()
    {
        Assert.Equal(3, GalleryRules.ImageCount([A, B], [C]));
        Assert.Equal(2, GalleryRules.ImageCount([A, B], [B, A, A]));
        Assert.Equal(0, GalleryRules.ImageCount([], []));
    }

    [Fact]
    public void TheLimit_IsTheProductConstant_AndItIsExceededOnlyAboveIt()
    {
        Assert.False(GalleryRules.IsOverTheLimit(ProductConstants.ImagesPerProductMax));
        Assert.True(GalleryRules.IsOverTheLimit(ProductConstants.ImagesPerProductMax + 1));
        Assert.False(GalleryRules.IsOverTheLimit(0));
    }

    // ---- the alternative text (criterion 6; KK6) ----

    [Fact]
    public void AVariantsOwnImage_NamesTheProductAndTheOptionValuesInOrder()
    {
        Assert.Equal("Pamuk Tişört — Kırmızı / M", ImageAltText.ForVariant("Pamuk Tişört", ["Kırmızı", "M"]));
        Assert.Equal("Pamuk Tişört — Kırmızı", ImageAltText.ForVariant("Pamuk Tişört", ["Kırmızı"]));
        Assert.Equal("Pamuk Tişört", ImageAltText.ForVariant("Pamuk Tişört", []));
        Assert.Equal("Pamuk Tişört — M", ImageAltText.ForVariant("Pamuk Tişört", [null, " ", " M "]));
    }

    [Fact]
    public void AGalleryImage_NamesTheProductAndItsPlaceOutOfTheCount()
    {
        Assert.Equal("Pamuk Tişört — görsel 2 / 5", ImageAltText.ForGallery("Pamuk Tişört", 2, 5));
        Assert.Equal("Pamuk Tişört — görsel 1 / 1", ImageAltText.ForGallery("Pamuk Tişört", 1, 1));
    }

    [Fact]
    public void TheCardAndTheList_UseTheProductNameAlone()
    {
        Assert.Equal("Pamuk Tişört", ImageAltText.ForCard("  Pamuk Tişört "));
    }

    [Fact]
    public void ThePlacesOfTheImagesOfOneProduct_GiveDifferentTexts_AndSoDoTheVariantsOfOneProduct()
    {
        string[] gallery = [.. Enumerable.Range(1, 5).Select(p => ImageAltText.ForGallery("Pamuk Tişört", p, 5))];
        string[] variants = [ImageAltText.ForVariant("Pamuk Tişört", ["Kırmızı", "M"]), ImageAltText.ForVariant("Pamuk Tişört", ["Kırmızı", "L"]), ImageAltText.ForVariant("Pamuk Tişört", ["Mavi", "M"])];

        Assert.Equal(gallery.Length, gallery.Distinct().Count());
        Assert.Equal(variants.Length, variants.Distinct().Count());
        Assert.Empty(gallery.Intersect(variants));
        Assert.DoesNotContain(ImageAltText.ForCard("Pamuk Tişört"), gallery.Concat(variants));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TheGeneratedText_IsNeverEmpty_EvenForANameThatIsEmpty(string? name)
    {
        Assert.False(string.IsNullOrWhiteSpace(ImageAltText.ForCard(name)));
        Assert.False(string.IsNullOrWhiteSpace(ImageAltText.ForGallery(name, 1, 3)));
        Assert.False(string.IsNullOrWhiteSpace(ImageAltText.ForVariant(name, ["M"])));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  \t ")]
    public void AnEmptyFirmText_IsNoText_AndTheGeneratedOneIsUsed(string? firmText)
    {
        Assert.Null(GalleryRules.CleanAltText(firmText));
        Assert.Equal("üretilen", ImageAltText.Choose(firmText, () => "üretilen"));
    }

    [Fact]
    public void TheFirmText_IsKeptTrimmed_AndWinsOverTheGeneratedOne()
    {
        Assert.Equal("Mavi tişört, önden", GalleryRules.CleanAltText("  Mavi tişört, önden "));
        Assert.Equal("Mavi tişört, önden", ImageAltText.Choose(" Mavi tişört, önden ", () => throw new InvalidOperationException("the generated text is not asked for")));
    }

    [Fact]
    public void ThePlaceOfAnImage_CannotBeBelowOneOrAboveTheCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageAltText.ForGallery("Ad", 0, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageAltText.ForGallery("Ad", 4, 3));
    }
}
