using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The models of the FAQ page and the contact page (T506 KK1, KK3, KK7; 04 5.9, 5.10): the page's name is the menu's name of the FAQ item or the default, the list keeps the module's order and shows the
/// empty-state line only while it is empty, a draft makes the page a preview, the five topics of the form are the enum's names in the enum's order with the texts the firm's notice uses, and the
/// page's input holds the fields of the module's input and nothing else.
/// </summary>
public sealed class FaqAndContactPageModelTests
{
    private static readonly LegalIdentity _identity = new(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);

    private static StorefrontFrame FrameWith(FrameMenu? menu) => new(
        new FrameBrand("Marka", null, null, "M", "#102030", "#ffffff"),
        menu,
        null,
        null,
        new FrameAccount(null, "/giris", "/siparis-takibi", "/hesabim", "/siparislerim", null),
        new FrameCart("/sepet", 0),
        new FrameContact([], null),
        new FrameFooter("/iletisim", _identity, null, [], [], true),
        new FrameSales(false, false, true),
        null,
        [],
        false);

    private static FrameMenu MenuWith(string? faqName) => new(
        HomepageLayout.ContentFirst,
        faqName is null ? [] : [new MenuItem(MenuItemKind.FaqItems, faqName, "/sss", true, null)],
        []);

    private static FaqQuestionView Question(string text, bool isDraft = false) => new(Guid.NewGuid(), text, "<p>Cevap</p>", isDraft);

    // ---- KK1: the page's name, the list, the empty-state line ----

    [Fact]
    public void ThePagesName_IsTheMenusNameOfTheFaqItem_TrimmedOrTheDefault_KK1()
    {
        Assert.Equal("Merak edilenler", FaqPage.TitleOf(FrameWith(MenuWith("  Merak edilenler "))));
        Assert.Equal(FaqPage.DefaultTitle, FaqPage.TitleOf(FrameWith(MenuWith(null))));
        Assert.Equal(FaqPage.DefaultTitle, FaqPage.TitleOf(FrameWith(MenuWith("   "))));
        Assert.Equal(FaqPage.DefaultTitle, FaqPage.TitleOf(FrameWith(null)));
        Assert.Equal("Sık sorulan sorular", FaqPage.DefaultTitle);
    }

    [Fact]
    public void TheList_KeepsTheModulesOrder_AndHasNoEmptyStateLine_KK1()
    {
        FaqQuestionView[] questions = [Question("C"), Question("A"), Question("B")];

        FaqPage page = FaqPage.Of("Sorular", questions);

        Assert.Equal(["C", "A", "B"], page.Questions.Select(q => q.Question));
        Assert.Null(page.Empty);
        Assert.False(page.HasDraft);
    }

    [Fact]
    public void WithoutAQuestion_TheEmptyStateLineNamesTheWayHome_KK1()
    {
        FaqPage page = FaqPage.Of("Sorular", []);

        Assert.NotNull(page.Empty);
        Assert.Equal("Henüz yayında soru yok.", page.Empty.Message);
        Assert.Equal(FaqPage.EmptyText, page.Empty.Message);
        Assert.Equal(FramePaths.Home, page.Empty.ActionPath);
        Assert.Equal("Ana sayfaya dön", page.Empty.ActionText);
    }

    [Fact]
    public void AListWithADraftQuestion_IsAPreview_KK2()
    {
        FaqPage page = FaqPage.Of("Sorular", [Question("A"), Question("B", isDraft: true)]);

        Assert.True(page.HasDraft);
    }

    [Fact]
    public void ThePageName_NeedsAName_AndTheQuestionsAList()
    {
        Assert.Throws<ArgumentException>(() => FaqPage.Of(" ", []));
        Assert.Throws<ArgumentNullException>(() => FaqPage.Of("Sorular", null!));
        Assert.Throws<ArgumentNullException>(() => FaqPage.TitleOf(null!));
    }

    // ---- KK7: the five topics ----

    [Fact]
    public void TheTopicOptions_AreTheEnumsNames_InTheEnumsOrder_FiveOfThem_KK7()
    {
        Assert.Equal(5, ContactTopicLabels.Options.Count);
        Assert.Equal(Enum.GetNames<ContactTopic>(), ContactTopicLabels.Options.Select(o => o.Value));
        Assert.Equal(Enum.GetValues<ContactTopic>().Select(ContactTopicLabels.TextOf), ContactTopicLabels.Options.Select(o => o.Text));
        Assert.Equal(5, ContactTopicLabels.Options.Select(o => o.Text).Distinct(StringComparer.Ordinal).Count());
        Assert.DoesNotContain(ContactTopicLabels.Options, o => o.Disabled);
    }

    [Theory]
    [InlineData(ContactTopic.GeneralQuestion, "Genel soru")]
    [InlineData(ContactTopic.AboutOrder, "Sipariş hakkında")]
    [InlineData(ContactTopic.AboutProduct, "Ürün hakkında")]
    [InlineData(ContactTopic.PersonalDataRequest, "KVKK talebi")]
    [InlineData(ContactTopic.Other, "Diğer")]
    public void ATopicsText_IsTheOneTheFirmsNoticeNames_KK7(ContactTopic topic, string text)
    {
        EmailLinks links = new(new SiteAddress(Options.Create(new SiteOptions { Domain = "magaza.example.com" })));
        CompanyNoticeRow row = CompanyNoticeValues.ForCompany(new NewContactRequestNotice(Guid.NewGuid(), topic, new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero)));

        EmailContent notice = CompanyNoticeTemplates.Compose(row.Type, row.EventValues, links)!;

        Assert.Equal(text, ContactTopicLabels.TextOf(topic));
        Assert.Contains($"Konu: {text}.", notice.Blocks.OfType<EmailParagraph>().Select(p => p.Text).Single(t => t.Contains("Konu:", StringComparison.Ordinal)), StringComparison.Ordinal);
    }

    [Fact]
    public void ATopicOutsideTheEnum_HasNoText_KK7()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ContactTopicLabels.TextOf((ContactTopic)99));
    }

    // ---- the form's fields ----

    [Fact]
    public void ThePagesInput_HoldsTheFieldsOfTheModulesInput_AndNothingElse_KK7()
    {
        string[] page = [.. typeof(SendContactRequestInput).GetProperties().Select(p => p.Name).Order(StringComparer.Ordinal)];
        string[] module = [.. typeof(ContactFormInput).GetProperties().Select(p => p.Name).Order(StringComparer.Ordinal)];

        Assert.Equal(module, page);
    }

    [Fact]
    public void TheSummarysOrder_IsTheFormsOrder_WithoutTheTrapField_KK7()
    {
        Assert.Equal(
            ["Input.Name", "Input.Email", "Input.Phone", "Input.ContactTopic", "Input.Message"],
            ContactModel.FormOrder);
        Assert.DoesNotContain(ContactModel.FormOrder, f => f.EndsWith(ContactFormFields.TrapFieldName, StringComparison.Ordinal));
        Assert.Equal("Input.Website", ContactModel.FieldName(ContactFormFields.TrapFieldName));
    }
}
