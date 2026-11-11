using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.UnitTests.CorporateContent;

/// <summary>
/// The corporate content types and their entry rules (T358 criteria 1-10; KK1-KK10; 02 3.27.1-3.27.9, 3.27.12): the closed type list, the fixed forms, the field rules, the
/// branch's map link and the social links' closed platform list.
/// </summary>
public sealed class CorporateContentTypesTests
{
    private static readonly string[] BuilderWords = ["PageBuilder", "Builder", "Layout", "Block", "Section", "Widget", "Template"];

    private static readonly string[] SalesWords = ["Price", "Cart", "Stock", "Basket", "Checkout", "Fiyat", "Sepet"];

    private static List<Type> ModuleTypes() =>
    [
        .. typeof(ContentTypes).Assembly.GetTypes().Where(t => t.Namespace is not null && t.Namespace.StartsWith("Shopfolio.Modules.CorporateContent", StringComparison.Ordinal)),
    ];

    // ---- KK1: no page builder ----

    [Fact]
    public void NoTypeOrMemberOfTheModule_IsAPageBuilder_KK1()
    {
        // The entry side of the module: the entry class, the forms, the type list and every content record. (HomepageLayout and the cache area HomepageBlocks belong
        // to the fixed home page of T361, a closed two-value order, not to the entry of a content page.)
        List<Type> entry =
        [
            typeof(ContentAuthoring), typeof(ContentInputRules), typeof(ContentTypes), typeof(ContentPresentation), typeof(ContentRecordKind),
            typeof(AboutPageInput), typeof(OfferingInput), typeof(PortfolioItemInput), typeof(FaqItemInput), typeof(BranchInput), typeof(CustomPageInput),
            .. ModuleTypes().Where(t => typeof(IContentRecord).IsAssignableFrom(t) && t is { IsClass: true }),
        ];
        Assert.Equal(Enum.GetValues<ContentRecordKind>().Length, entry.Count(t => typeof(IContentRecord).IsAssignableFrom(t)));
        foreach (Type type in entry)
        {
            Assert.DoesNotContain(BuilderWords, w => type.Name.Contains(w, StringComparison.OrdinalIgnoreCase));
            foreach (MemberInfo member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (member.Name.Contains('<', StringComparison.Ordinal))
                {
                    continue;
                }

                Assert.DoesNotContain(BuilderWords, w => member.Name.Contains(w, StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    [Fact]
    public void TheEntryForms_HaveNoBlockSectionOrLayoutField_KK1()
    {
        Type[] forms = [typeof(AboutPageInput), typeof(OfferingInput), typeof(PortfolioItemInput), typeof(FaqItemInput), typeof(BranchInput), typeof(CustomPageInput)];
        foreach (Type form in forms)
        {
            Assert.DoesNotContain(form.GetProperties(), p => BuilderWords.Any(w => p.Name.Contains(w, StringComparison.OrdinalIgnoreCase)));
        }
    }

    // ---- KK2: closed type list ----

    [Fact]
    public void TheTypeList_IsTheContentRecordKindEnum_InBothDirections_KK2()
    {
        ContentRecordKind[] listed = [.. ContentTypes.ReadyMade, .. ContentTypes.Added];

        Assert.Equal(Enum.GetValues<ContentRecordKind>().Order(), listed.Order());
        Assert.Equal(listed.Length, listed.Distinct().Count());
        Assert.Equal(5, ContentTypes.ReadyMade.Count);
        foreach (ContentRecordKind kind in Enum.GetValues<ContentRecordKind>())
        {
            Assert.True(ContentTypes.TryParse(kind.ToString(), out ContentRecordKind parsed));
            Assert.Equal(kind, parsed);
        }
    }

    [Theory]
    [InlineData("Gallery")]
    [InlineData("Contact")]
    [InlineData("ContactPage")]
    [InlineData("Testimonial")]
    [InlineData("Blog")]
    [InlineData("LegalText")]
    [InlineData("aboutpage")]
    [InlineData("0")]
    [InlineData("")]
    [InlineData(null)]
    public void ANameOutsideTheClosedList_IsNotAType_KK2(string? name)
    {
        Assert.False(ContentTypes.TryParse(name, out _));
    }

    [Fact]
    public void TheRecordKinds_HaveNoGalleryOrContactPageKind_KK2()
    {
        Assert.DoesNotContain(Enum.GetNames<ContentRecordKind>(), n => n.Contains("Gallery", StringComparison.OrdinalIgnoreCase) || n.Contains("Contact", StringComparison.OrdinalIgnoreCase));
    }

    // ---- KK3: the about page ----

    [Fact]
    public void TheAboutPage_IsOneRecord_AndTheShortIntroductionHasTheProductCeiling_KK3()
    {
        Assert.Contains(ContentRecordKind.AboutPage, ContentTypes.SingleRecord);

        List<FieldRejection> within = ContentInputRules.Check(new AboutPageInput(new string('a', ContentRules.ShortIntroductionMaxLength), null));
        List<FieldRejection> over = ContentInputRules.Check(new AboutPageInput(new string('a', ContentRules.ShortIntroductionMaxLength + 1), null));

        Assert.Empty(within);
        Assert.Equal([new FieldRejection(nameof(AboutPageInput.ShortIntroduction), FieldCodes.TextTooLong)], over);
    }

    [Fact]
    public void TheAboutPage_ShortIntroduction_IsKeptLiterally_NoMarkupIsReadOrRemoved_KK3()
    {
        Assert.Equal("<b>x</b>", ContentInputRules.Text("  <b>x</b>  "));
        Assert.Null(ContentInputRules.Text("   "));
        Assert.Null(ContentInputRules.Text(null));
    }

    // ---- KK4: the service introduction ----

    [Fact]
    public void TheServiceIntroduction_HasNoPriceCartOrStockMember_KK4()
    {
        foreach (Type type in new[] { typeof(OfferingInput), typeof(Offering) })
        {
            Assert.DoesNotContain(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), p => SalesWords.Any(w => p.Name.Contains(w, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [Fact]
    public void TheServiceButton_SaysBizeUlasinAndLeadsToTheContactPage_KK4()
    {
        Assert.Equal("Bize ulaşın", ContentPresentation.OfferingContactButtonText);
        Assert.Equal("/" + AddressRules.ContactPage, ContentPresentation.OfferingContactButtonPath);
        Assert.Equal("/iletisim", ContentPresentation.OfferingContactButtonPath);
    }

    [Fact]
    public void TheServiceName_IsRequiredAndWithinTheTitleCeiling_KK4()
    {
        Assert.Equal([new FieldRejection(nameof(OfferingInput.Name), FieldCodes.FieldRequired)], ContentInputRules.Check(new OfferingInput("  ", null, null, false)));
        Assert.Equal(
            [new FieldRejection(nameof(OfferingInput.Name), FieldCodes.TextTooLong)],
            ContentInputRules.Check(new OfferingInput(new string('a', ContentRules.TitleMaxLength + 1), null, null, false)));
        Assert.Equal(
            [new FieldRejection(nameof(OfferingInput.ShortDescription), FieldCodes.TextTooLong)],
            ContentInputRules.Check(new OfferingInput("Bakım", new string('a', ContentRules.ShortDescriptionMaxLength + 1), null, false)));
    }

    // ---- KK5: the reference work ----

    [Fact]
    public void TheReferenceWork_HasExactlyTheFourFormFields_AndNoCustomerYearOrCategory_KK5()
    {
        Assert.Equal(
            ["IsShownOnHomepage", "ShortDescription", "Text", "Title"],
            typeof(PortfolioItemInput).GetProperties().Select(p => p.Name).Order().ToArray());
        string[] forbidden = ["Customer", "Client", "Musteri", "Year", "Yil", "Category", "Kategori", "Tag"];
        foreach (Type type in new[] { typeof(PortfolioItemInput), typeof(PortfolioItem) })
        {
            Assert.DoesNotContain(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), p => forbidden.Any(w => p.Name.Contains(w, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [Fact]
    public void TheReferenceWork_TitleIsRequired_KK5()
    {
        Assert.Equal([new FieldRejection(nameof(PortfolioItemInput.Title), FieldCodes.FieldRequired)], ContentInputRules.Check(new PortfolioItemInput(null, null, null, false)));
        Assert.Empty(ContentInputRules.Check(new PortfolioItemInput("Proje", null, null, true)));
    }

    // ---- KK6: the FAQ ----

    [Fact]
    public void TheQuestionCeiling_IsTheShortTextConstant_AndTheQuestionIsKeptLiterally_KK6()
    {
        Assert.Empty(ContentInputRules.Check(new FaqItemInput(new string('a', ContentRules.ShortDescriptionMaxLength), null)));
        Assert.Equal(
            [new FieldRejection(nameof(FaqItemInput.Question), FieldCodes.TextTooLong)],
            ContentInputRules.Check(new FaqItemInput(new string('a', ContentRules.ShortDescriptionMaxLength + 1), null)));
        Assert.Equal([new FieldRejection(nameof(FaqItemInput.Question), FieldCodes.FieldRequired)], ContentInputRules.Check(new FaqItemInput(" ", "<p>Cevap</p>")));
        Assert.Equal("<i>İade?</i>", ContentInputRules.Text("<i>İade?</i>"));
    }

    [Fact]
    public void TheQuestion_HasNoHeadingOrCategoryField_AndTheListIsOne_KK6()
    {
        Assert.Equal(["Answer", "Question"], typeof(FaqItemInput).GetProperties().Select(p => p.Name).Order().ToArray());
        Assert.DoesNotContain(
            typeof(FaqItem).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
            p => p.Name.Contains("Category", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Heading", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Group", StringComparison.OrdinalIgnoreCase));
        MethodInfo list = typeof(ContentReaders).GetMethod(nameof(ContentReaders.ListFaqAsync))!;
        Assert.Equal(typeof(Task<IReadOnlyList<FaqView>>), list.ReturnType);
        Assert.All(list.GetParameters(), p => Assert.Equal(typeof(CancellationToken), p.ParameterType));
    }

    // ---- KK7: the branch ----

    [Fact]
    public void TheBranchName_IsRequired_AndTheAddressPartsAreNotRequiredToSaveADraft_KK7()
    {
        Assert.Equal([new FieldRejection(nameof(BranchInput.Name), FieldCodes.FieldRequired)], ContentInputRules.Check(new BranchInput(null, null, null, null, null, null, null)));
        Assert.Empty(ContentInputRules.Check(new BranchInput("Merkez", null, null, null, null, null, null)));
    }

    [Theory]
    [InlineData("https://maps.example.com/?q=1", true)]
    [InlineData("http://maps.example.com/x", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("ftp://maps.example.com/x", false)]
    [InlineData("maps.example.com", false)]
    [InlineData("https://", false)]
    [InlineData("https://a b.example.com", false)]
    [InlineData("<iframe src=\"https://maps.example.com\"></iframe>", false)]
    public void TheMapLink_IsKeptOnlyWhenItIsAnHttpOrHttpsAddress_KK7(string url, bool accepted)
    {
        List<FieldRejection> rejections = ContentInputRules.Check(new BranchInput("Merkez", null, null, null, null, null, url));

        Assert.Equal(accepted, rejections.Count == 0);
        MapLinkView? link = ContentReaders.MapLinkOf(url);
        Assert.Equal(accepted, link is not null);
        if (link is not null)
        {
            Assert.Equal(url, link.Url);
            Assert.Equal("Haritada aç", link.Text);
        }
    }

    [Fact]
    public void ABranchWithoutAMapLink_ShowsNoMapLink_KK7()
    {
        Assert.Null(ContentReaders.MapLinkOf(null));
        Assert.Null(ContentReaders.MapLinkOf("   "));
        Assert.Equal("Haritada aç", ContentPresentation.BranchMapLinkText);
    }

    [Fact]
    public void TheBranch_HasNoEmbeddedMapOrCoordinateMember_KK7()
    {
        string[] forbidden = ["Iframe", "Embed", "Latitude", "Longitude", "Coordinate"];
        foreach (Type type in new[] { typeof(BranchInput), typeof(Branch), typeof(BranchView), typeof(MapLinkView) })
        {
            Assert.DoesNotContain(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), p => forbidden.Any(w => p.Name.Contains(w, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [Theory]
    [InlineData("0 (532) 111 22 33", "5321112233")]
    [InlineData("532 111 22 33", "5321112233")]
    [InlineData("+90 532 111 22 33", "5321112233")]
    public void ABranchPhone_OfTheAcceptedForms_IsStoredAsTenDigits_KK7(string typed, string stored)
    {
        Assert.Equal(stored, ContentInputRules.Phone(typed));
        Assert.Empty(ContentInputRules.Check(new BranchInput("Merkez", null, null, null, typed, null, null)));
    }

    [Fact]
    public void ABranchPhone_OfAnotherForm_IsRefused_KK7()
    {
        Assert.Equal(
            [new FieldRejection(nameof(BranchInput.Phone), FieldCodes.InvalidPhone)],
            ContentInputRules.Check(new BranchInput("Merkez", null, null, null, "ara beni", null, null)));
    }

    // ---- KK8: the contact page has no record ----

    [Fact]
    public void TheContactPage_IsBuiltFromThreeParts_AndHasNoRecordKindOfItsOwn_KK8()
    {
        Assert.Equal(["CompanyIdentity", "Branches", "ContactForm"], ContentPresentation.ContactPageParts);
        Assert.DoesNotContain(Enum.GetNames<ContentRecordKind>(), n => n.Contains("Contact", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(AddressRules.ContactPage, AddressRules.FixedPages);
        Assert.Equal(typeof(ContactPageContent).GetProperties().Select(p => p.Name).Order(), ["Branches", "Identity"]);
    }

    // ---- KK9: the general page ----

    [Fact]
    public void TheGeneralPage_HasNoCountLimit_AndNoLegalTextType_KK9()
    {
        Assert.DoesNotContain(ModuleTypes().SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)), f => f.Name.Contains("MaxPages", StringComparison.OrdinalIgnoreCase) || f.Name.Contains("MaxCustom", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(Enum.GetNames<ContentRecordKind>(), n => n.Contains("Legal", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(ContentInputRules.Check(new CustomPageInput("Sayfa", null, true)));
        Assert.Equal([new FieldRejection(nameof(CustomPageInput.Title), FieldCodes.FieldRequired)], ContentInputRules.Check(new CustomPageInput(" ", "metin", false)));
    }

    // ---- KK10: the social links ----

    [Fact]
    public void ThePlatformList_IsTheSixClosedValues_InTheOrderOfTheEnum_KK10()
    {
        Assert.Equal(Enum.GetValues<SocialPlatform>(), SocialLinks.Platforms);
        Assert.Equal(["Instagram", "Facebook", "X", "LinkedIn", "YouTube", "TikTok"], SocialLinks.Platforms.Select(p => p.ToString()).ToArray());
    }

    [Theory]
    [InlineData("Instagram", true)]
    [InlineData("TikTok", true)]
    [InlineData("instagram", false)]
    [InlineData("Twitter", false)]
    [InlineData("WhatsApp", false)]
    [InlineData("1", false)]
    [InlineData("0", false)]
    [InlineData(" X", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void APlatformName_IsOnTheListOnlyWhenItIsExactlyAValue_KK10(string? name, bool expected)
    {
        Assert.Equal(expected, SocialLinks.TryParsePlatform(name, out _));
    }

    [Theory]
    [InlineData("0 (532) 111 22 33", "5321112233", true)]
    [InlineData("5321112233", "5321112233", true)]
    [InlineData("", null, true)]
    [InlineData(null, null, true)]
    [InlineData("12345", "12345", false)]
    [InlineData("whatsapp numaram", "whatsapp numaram", false)]
    public void TheWhatsAppNumber_IsOptional_AndTenDigitsWhenGiven_KK10(string? typed, string? stored, bool accepted)
    {
        (string? actualStored, FieldRejection? rejection) = SocialLinks.CheckWhatsAppNumber("WhatsAppNumber", typed);

        Assert.Equal(stored, actualStored);
        Assert.Equal(accepted, rejection is null);
        if (rejection is not null)
        {
            Assert.Equal(new FieldRejection("WhatsAppNumber", FieldCodes.InvalidPhone), rejection);
        }
    }

    [Fact]
    public void NoOrderTable_HasASocialLinkOrWhatsAppColumn_KK10()
    {
        Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, "Host=127.0.0.1;Database=never_opened");
        using AppDbContext context = new(builder.Options);

        List<IEntityType> orderTables = [.. context.Model.GetEntityTypes().Where(e => e.GetTableName() is { } table && table.StartsWith("order", StringComparison.Ordinal))];

        Assert.NotEmpty(orderTables);
        foreach (IEntityType entity in orderTables)
        {
            Assert.DoesNotContain(entity.GetProperties(), p => p.Name.Contains("Social", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("WhatsApp", StringComparison.OrdinalIgnoreCase));
        }

        IEntityType links = context.Model.FindEntityType(typeof(SocialLink))!;
        Assert.DoesNotContain(links.GetForeignKeys(), _ => true);
    }
}
