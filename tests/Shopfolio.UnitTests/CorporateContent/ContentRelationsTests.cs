using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.UnitTests.CorporateContent;

/// <summary>
/// The relations, the order, the images and the limits of the corporate content (T359; KK1, KK5-KK8; 02 3.27.10, 3.27.11, 3.27.15-3.27.20): the rules that need no database - the shared
/// gallery text, the shape of the product link, what the catalog contract does not carry, the kinds that have a manual order, the ceilings taken from the parameters, and the capabilities
/// the content does not have.
/// </summary>
public sealed class ContentRelationsTests
{
    private static readonly string[] OutOfScopeWords = ["Video", "Embed", "Attachment", "Pdf", "Testimonial", "Blog", "Gorus", "Quote", "Carousel", "Slider"];

    private static readonly string[] SuggestionWords = ["Recommend", "AlsoBought", "Similar", "RelatedContent", "Offering", "PortfolioItem", "ReferenceWork", "Suggestion"];

    private static readonly Guid A = Guid.Parse("018f0000-0000-7000-8000-00000000000a");
    private static readonly Guid B = Guid.Parse("018f0000-0000-7000-8000-00000000000b");
    private static readonly Guid C = Guid.Parse("018f0000-0000-7000-8000-00000000000c");

    // ---- KK5: the gallery text is one function shared with the product ----

    [Fact]
    public void TheMainImage_IsTheMarkedOne_IndependentOfTheOrder_KK5()
    {
        Assert.Equal(B, GalleryText.MainOf([(A, false), (B, true), (C, false)], null));
        Assert.Equal(B, GalleryText.MainOf([(A, false), (B, true), (C, true)], A));
    }

    [Fact]
    public void WithNoMark_TheCurrentMainStays_IfItIsStillInTheList_ElseTheFirstInTheOrder_KK5()
    {
        Assert.Equal(C, GalleryText.MainOf([(A, false), (B, false), (C, false)], C));
        Assert.Equal(A, GalleryText.MainOf([(A, false), (B, false)], C));
        Assert.Equal(A, GalleryText.MainOf([(A, false), (B, false)], null));
        Assert.Null(GalleryText.MainOf([], A));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void AnEmptyAltText_IsNone_AndTheGeneratedTextStandsIn_KK5(string? firmText)
    {
        Assert.Null(GalleryText.CleanAltText(firmText));
        Assert.Equal("Montaj — görsel 2 / 5", GalleryText.Choose(firmText, () => GalleryText.Generated("Montaj", 2, 5)));
    }

    [Fact]
    public void TheFirmsAltText_IsKeptTrimmed_AndWinsOverTheGeneratedOne_KK5()
    {
        Assert.Equal("Atölye", GalleryText.CleanAltText("  Atölye "));
        Assert.Equal("Atölye", GalleryText.Choose(" Atölye ", () => throw new InvalidOperationException("The generated text is not needed.")));
    }

    [Fact]
    public void TheGeneratedText_IsTheNameAndThePlaceCountedFromOne_AndNeverEmpty_KK5()
    {
        Assert.Equal("Ad — görsel 1 / 1", GalleryText.Generated("Ad", 1, 1));
        Assert.Throws<ArgumentException>(() => GalleryText.Generated(" ", 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => GalleryText.Generated("Ad", 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => GalleryText.Generated("Ad", 3, 2));
    }

    [Fact]
    public void TheNumberOfImagesOfOneRecord_IsTheParameter_AndTheFiveGalleriesShareOneLinkShape_KK5()
    {
        Assert.Equal(ProductConstants.ImagesPerContentRecordMax, ProductConstants.ImagesPerProductMax);
        Type[] links = [typeof(AboutPageImage), typeof(OfferingImage), typeof(PortfolioItemImage), typeof(FaqItemImage), typeof(CustomPageImage)];
        Assert.All(links, t => Assert.True(typeof(IContentGalleryLink).IsAssignableFrom(t), t.Name));
        Assert.Equal(links.Length, ContentGallery.WithGallery.Count);
        Assert.DoesNotContain(ContentRecordKind.Branch, ContentGallery.WithGallery);
        Assert.DoesNotContain(ContentRecordKind.Announcement, ContentGallery.WithGallery);
    }

    [Fact]
    public void TheCeilings_OfTheContentFields_AreTheParameters_NotACopy_KK5()
    {
        Assert.Equal(ProductConstants.ProductNameMaxLength, ContentRules.TitleMaxLength);
        Assert.Equal(ProductConstants.ShortTextMaxLength, ContentRules.ShortDescriptionMaxLength);
        Assert.Equal(ProductConstants.ShortIntroductionMaxLength, ContentRules.ShortIntroductionMaxLength);
        Assert.Equal(ProductConstants.AnnouncementTextMaxLength, ContentRules.AnnouncementTextMaxLength);

        IModel model = ReadModel();
        Assert.Equal(ProductConstants.ProductNameMaxLength, MaxLength(model, typeof(Offering), nameof(Offering.Name)));
        Assert.Equal(ProductConstants.ProductNameMaxLength, MaxLength(model, typeof(PortfolioItem), nameof(PortfolioItem.Title)));
        Assert.Equal(ProductConstants.ProductNameMaxLength, MaxLength(model, typeof(CustomPage), nameof(CustomPage.Title)));
        Assert.Equal(ProductConstants.ProductNameMaxLength, MaxLength(model, typeof(Branch), nameof(Branch.Name)));
        Assert.Equal(ProductConstants.ShortTextMaxLength, MaxLength(model, typeof(Offering), nameof(Offering.ShortDescription)));
        Assert.Equal(ProductConstants.ShortTextMaxLength, MaxLength(model, typeof(PortfolioItem), nameof(PortfolioItem.ShortDescription)));
        Assert.Equal(ProductConstants.ShortTextMaxLength, MaxLength(model, typeof(FaqItem), nameof(FaqItem.Question)));
        Assert.Equal(ProductConstants.ShortTextMaxLength, MaxLength(model, typeof(Branch), nameof(Branch.WorkingHours)));
        Assert.Equal(ProductConstants.ShortIntroductionMaxLength, MaxLength(model, typeof(AboutPage), nameof(AboutPage.ShortIntroduction)));
        Assert.Equal(ProductConstants.AnnouncementTextMaxLength, MaxLength(model, typeof(Announcement), nameof(Announcement.Text)));
    }

    // ---- KK1: the link is at the product level and one way ----

    [Fact]
    public void TheProductLinks_CarryNoVariant_OnlyTheOwnerTheProductAndThePlace_KK1()
    {
        Assert.Equal(["CreatedAt", "Id", "OfferingId", "ProductId", "SortOrder"], PropertyNames(typeof(OfferingProduct)));
        Assert.Equal(["CreatedAt", "Id", "PortfolioItemId", "ProductId", "SortOrder"], PropertyNames(typeof(PortfolioItemProduct)));
        Assert.DoesNotContain(ReadModel().GetEntityTypes().SelectMany(e => e.GetProperties()).Where(p => p.DeclaringType.ClrType.Name.EndsWith("Product", StringComparison.Ordinal) && p.DeclaringType.ClrType.Namespace == typeof(Offering).Namespace), p => p.Name.Contains("Variant", StringComparison.Ordinal));
    }

    [Fact]
    public void TheCatalogContract_HasNoReverseLinkAndNoSuggestionBlock_KK1()
    {
        Type[] contract = [.. typeof(ICatalogModule).Assembly.GetTypes().Where(t => t.Namespace is not null && t.Namespace.StartsWith(typeof(ICatalogModule).Namespace!, StringComparison.Ordinal))];
        Assert.Contains(typeof(ProductPage), contract);

        foreach (Type type in contract)
        {
            Assert.DoesNotContain(SuggestionWords, w => type.Name.Contains(w, StringComparison.Ordinal));
            foreach (MemberInfo member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                Assert.DoesNotContain(SuggestionWords, w => member.Name.Contains(w, StringComparison.Ordinal));
            }
        }
    }

    [Fact]
    public void TheProductAndItsVariants_HoldNoPropertyThatPointsAtTheContent_KK1()
    {
        foreach (Type type in new[] { typeof(ShowcaseProduct), typeof(ProductPage), typeof(ProductPageVariant) })
        {
            Assert.DoesNotContain(type.GetProperties(), p => p.Name.Contains("Linked", StringComparison.Ordinal) || p.Name.Contains("Content", StringComparison.Ordinal));
        }
    }

    // ---- KK2: the manual order ----

    [Fact]
    public void TheManualOrder_IsOfTheFiveLists_NotOfTheAboutPageNorTheAnnouncement_KK2()
    {
        Assert.Equal(
            [ContentRecordKind.Offering, ContentRecordKind.PortfolioItem, ContentRecordKind.FaqItem, ContentRecordKind.Branch, ContentRecordKind.CustomPage],
            ContentOrdering.Orderable);
        Assert.Equal(5, ContentOrdering.Orderable.Count);
        Assert.DoesNotContain(ContentRecordKind.AboutPage, ContentOrdering.Orderable);
        Assert.DoesNotContain(ContentRecordKind.Announcement, ContentOrdering.Orderable);
    }

    // ---- KK6: the long text ----

    [Fact]
    public void TheLongText_DropsEmbeddedFrames_Players_AndScripts_ButKeepsAVideoLink_KK6()
    {
        string text = "<p>Giriş</p><iframe src=\"https://video.example/e\"></iframe><video src=\"a.mp4\"></video><object data=\"a.swf\"></object><embed src=\"a.swf\">"
            + "<script>alert(1)</script><p><a href=\"https://www.youtube.com/watch?v=abc\">Videoyu izle</a></p>";

        string? cleaned = ContentRules.CleanLongText(text);

        Assert.NotNull(cleaned);
        Assert.DoesNotContain("iframe", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<video", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<object", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<embed", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("script", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("href=\"https://www.youtube.com/watch?v=abc\"", cleaned, StringComparison.Ordinal);
        Assert.Contains("Videoyu izle</a>", cleaned, StringComparison.Ordinal);
        Assert.Contains("<p>Giriş</p>", cleaned, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLongText_UsesTheProductsFormatSet_AndATagOutsideItIsStripped_KK6()
    {
        string? cleaned = ContentRules.CleanLongText("<h1>Büyük</h1><p style=\"color:red\" onclick=\"x()\">Metin</p><table><tr><td>h</td></tr></table><img src=\"x.png\">");

        Assert.NotNull(cleaned);
        foreach (string tag in new[] { "<h1", "<table", "<td", "<img", "style=", "onclick" })
        {
            Assert.DoesNotContain(tag, cleaned, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("Metin", cleaned, StringComparison.Ordinal);
        Assert.Null(ContentRules.CleanLongText("<script>x</script>"));
    }

    // ---- KK7, KK8: what the content does not have ----

    [Fact]
    public void ThePolicy_AllowsNoFrame_SoNoVideoPlayerCanBeEmbedded_KK7()
    {
        string policy = ContentSecurityPolicy.Build("nonce");

        Assert.Contains("frame-src 'none'", policy, StringComparison.Ordinal);
        Assert.Contains("object-src 'none'", policy, StringComparison.Ordinal);
    }

    [Fact]
    public void TheContentModule_HasNoTypeMemberOrColumn_ForAVideoAnAttachmentATestimonialOrABlog_KK7_KK8()
    {
        Type[] types = [.. typeof(Offering).Assembly.GetTypes().Where(t => t.Namespace is not null && t.Namespace.StartsWith(typeof(Offering).Namespace!, StringComparison.Ordinal))];
        Assert.NotEmpty(types);

        foreach (Type type in types)
        {
            Assert.DoesNotContain(OutOfScopeWords, w => type.Name.Contains(w, StringComparison.OrdinalIgnoreCase));
            foreach (MemberInfo member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                Assert.DoesNotContain(OutOfScopeWords, w => member.Name.Contains(w, StringComparison.OrdinalIgnoreCase));
            }
        }

        foreach (IEntityType entity in ReadModel().GetEntityTypes().Where(e => e.ClrType.Namespace?.StartsWith(typeof(Offering).Namespace!, StringComparison.Ordinal) == true))
        {
            Assert.DoesNotContain(OutOfScopeWords, w => (entity.GetTableName() ?? string.Empty).Contains(w, StringComparison.OrdinalIgnoreCase));
            foreach (IProperty property in entity.GetProperties())
            {
                Assert.DoesNotContain(OutOfScopeWords, w => (property.GetColumnName() ?? string.Empty).Contains(w, StringComparison.OrdinalIgnoreCase));
            }
        }

        Assert.DoesNotContain(OutOfScopeWords, w => string.Join(' ', Enum.GetNames<ContentRecordKind>()).Contains(w, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TheClosedAddressList_HasNoPrefixForABlogAVideoOrACustomerOpinion_KK8()
    {
        foreach (string word in new[] { "blog", "yazi", "gorus", "video", "yorum", "musteri-gorusleri", "ek", "dosya" })
        {
            Assert.DoesNotContain(word, AddressRules.ClosedFirstSegments);
            Assert.DoesNotContain(word, AddressRules.RecordPrefixes);
        }
    }

    private static string[] PropertyNames(Type type) => [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).Order(StringComparer.Ordinal)];

    private static int? MaxLength(IModel model, Type entity, string property) => model.FindEntityType(entity)!.FindProperty(property)!.GetMaxLength();

    private static IModel ReadModel()
    {
        DbContextOptionsBuilder<AppDbContext> options = new();
        AppDbContext.Configure(options, "Host=localhost;Database=shopfolio_content_relations_model");
        using AppDbContext context = new(options.Options);
        return context.GetService<IDesignTimeModel>().Model;
    }
}
