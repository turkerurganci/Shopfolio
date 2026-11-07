using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.LegalTextTests;

/// <summary>
/// The two shipped templates and the two drafts (T366 KK1, KK2, KK6, KK7; 11 T366 criteria 1, 2, 6, 7; 02 3.33.1, 3.33.2, 3.33.7): the token set is closed, a template's version id
/// follows its content, both texts carry the dispute information without a money limit, the form and the contract name no file, and the privacy notice draft carries its three contents.
/// </summary>
public sealed partial class LegalTextTemplateTests
{
    public static TheoryData<LegalTextType> GeneratedTypes => [.. LegalTextTemplates.GeneratedTypes];

    /// <summary>The closed token set of 09 9.3 item 2: the field names of the order and of its items, the two province tokens (the field holds the id), and the company values the version fills.</summary>
    private static HashSet<string> ClosedSet()
    {
        HashSet<string> names = [.. typeof(Order).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name)];
        names.UnionWith(typeof(OrderItem).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name));
        names.UnionWith(["ShippingProvince", "BillingProvince"]);
        names.UnionWith(LegalTextTemplates.CompanyTokens);
        return names;
    }

    [Fact]
    public void TheGeneratedTypes_AreExactlyTheFormAndTheContract_AndOnlyTheyHaveATemplate()
    {
        Assert.Equal([LegalTextType.PreInformationForm, LegalTextType.DistanceSalesContract], LegalTextTemplates.GeneratedTypes);
        Assert.Throws<ArgumentException>(() => LegalTextTemplates.Get(LegalTextType.PrivacyNotice));
        Assert.Throws<ArgumentException>(() => LegalTextTemplates.Get(LegalTextType.CookiePolicy));
    }

    [Theory]
    [MemberData(nameof(GeneratedTypes))]
    public void EveryTokenOfATemplate_IsInTheClosedSet_AndNoDoubleBraceHasAnotherPurpose(LegalTextType type)
    {
        string text = LegalTextTemplates.Get(type).Text;
        HashSet<string> closed = ClosedSet();

        string[] tokens = [.. PlaceholderFiller.TokenNames(text)];

        Assert.NotEmpty(tokens);
        Assert.All(tokens, t => Assert.Contains(t, closed));
        Assert.Equal(tokens.Length, Regex.Count(text, @"\{\{"));
        Assert.Equal(tokens.Length, Regex.Count(text, @"\}\}"));
    }

    [Theory]
    [MemberData(nameof(GeneratedTypes))]
    public void TheVersionId_IsTwelveHexDigits_StableAcrossLineEndings_AndChangesWithTheContent(LegalTextType type)
    {
        LegalTextTemplate template = LegalTextTemplates.Get(type);

        Assert.Matches("^[0-9a-f]{12}$", template.Version);
        Assert.Equal(template.Version, LegalTextTemplates.VersionOf(template.Text.ReplaceLineEndings("\r\n")));
        Assert.NotEqual(template.Version, LegalTextTemplates.VersionOf(template.Text + " "));
        Assert.NotEqual(LegalTextTemplates.Get(LegalTextType.PreInformationForm).Version, LegalTextTemplates.Get(LegalTextType.DistanceSalesContract).Version);
    }

    [Theory]
    [MemberData(nameof(GeneratedTypes))]
    public void TheTemplate_WritesTheDisputeBodies_AndNoMoneyLimit(LegalTextType type)
    {
        string text = LegalTextTemplates.Get(type).Text;

        Assert.Contains("tüketici hakem heyetine", text, StringComparison.Ordinal);
        Assert.Contains("tüketici mahkemesine", text, StringComparison.Ordinal);
        Assert.Contains("arabulucuya başvurma şartıyla", text, StringComparison.Ordinal);

        // The limit between the bodies changes every year and is never written as a figure (02 3.33.7): no number is next to a money word or a currency sign.
        Assert.DoesNotMatch(MoneyFigure(), text);
    }

    [Theory]
    [MemberData(nameof(GeneratedTypes))]
    public void TheTemplate_CarriesTheReturnTermsOfTheForm_AndNamesNoFile(LegalTextType type)
    {
        string text = LegalTextTemplates.Get(type).Text;

        Assert.Contains("{{ReturnAddress}}", text, StringComparison.Ordinal);
        Assert.Contains("on dört gün", text, StringComparison.Ordinal);
        Assert.Contains("taşıyıcı belirlenmemiştir", text, StringComparison.Ordinal);
        Assert.Contains("İade kargo bedeli satıcıya aittir", text, StringComparison.Ordinal);
        Assert.Contains("tam metin", text, StringComparison.Ordinal);

        // The two texts travel in the body of the confirmation e-mail; there is no attachment (02 3.33.6).
        Assert.DoesNotMatch(FileWord(), text);
    }

    [Fact]
    public void TheForm_AddsTheFourItemsOfTheSummary_AndTheContractTheAgeRule()
    {
        string form = LegalTextTemplates.Get(LegalTextType.PreInformationForm).Text;
        string contract = LegalTextTemplates.Get(LegalTextType.DistanceSalesContract).Text;

        Assert.Contains("yalnızca satıcının teslimat yaptığı illere", form, StringComparison.Ordinal);
        Assert.Contains("{{ShippingPeriod}}", form, StringComparison.Ordinal);
        Assert.Contains("{{CompanyLegalName}}", form, StringComparison.Ordinal);
        Assert.Contains("on sekiz yaşını doldurduğunu", contract, StringComparison.Ordinal);
    }

    // ---- the privacy notice draft (criterion 2) ----

    [Fact]
    public void ThePrivacyNoticeDraft_WritesTheFiveRecipientGroups_TheLegalBases_AndTheApplicationRoutes()
    {
        string draft = InsertStartupData.PrivacyNoticeDraft;

        string[] groups =
        [
            "Kargo ve teslimat şirketleri", "Fatura ve muhasebe hizmeti sağlayıcıları", "Ödeme kuruluşu", "Barındırma ve e-posta altyapısı (veri işleyen)",
            "Bakım ve destek sağlayıcısı (veri işleyen)",
        ];
        Assert.All(groups, g => Assert.Contains(g, draft, StringComparison.Ordinal));

        // The legal base of every purpose (02 12.2.3) and the ten-year keeping of the electronic commerce records.
        Assert.Contains("KVKK m.5/2-c", draft, StringComparison.Ordinal);
        Assert.Contains("KVKK m.5/2-ç", draft, StringComparison.Ordinal);
        Assert.Contains("6563 sayılı Kanun m.11/3", draft, StringComparison.Ordinal);

        // The application routes of the Communiqué.
        Assert.Contains("Veri Sorumlusuna Başvuru Usul ve Esasları Hakkında Tebliğ", draft, StringComparison.Ordinal);
        Assert.Contains("KEP", draft, StringComparison.Ordinal);
        Assert.Contains("Barındırma ve e-posta altyapısının konumu", draft, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDraftTokens_AreIdentityFields_ThePublicationFills()
    {
        foreach (string draft in new[] { InsertStartupData.PrivacyNoticeDraft, InsertStartupData.CookiePolicyDraft })
        {
            Assert.All(PlaceholderFiller.TokenNames(draft), t => Assert.Contains(t, Shopfolio.Modules.Settings.LegalTexts.IdentityTokens));
        }
    }

    [GeneratedRegex(@"\d[\d.,]*\s*(?:TL|₺|lira|bin\s*lira|milyon)|(?:TL|₺)\s*\d", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex MoneyFigure();

    [GeneratedRegex(@"\bek\s*dosya|\bekte\b|\.pdf\b|\bPDF\b|dosya eki", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FileWord();
}
