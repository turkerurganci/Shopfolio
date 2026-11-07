using Shopfolio.Infrastructure;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.LegalTextTests;

/// <summary>
/// The placeholder filler (09 9.3 item 7: no expansion, a token left in place, null, requireAll) and the links' list (T366 KK5, KK8; 11 T366 criteria 5, 8; 02 3.33.5, 3.33.8).
/// </summary>
public sealed class PlaceholderFillerAndLinksTests
{
    private static Dictionary<string, string?> Values(params (string Key, string? Value)[] pairs) => pairs.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);

    [Fact]
    public void Fill_ReplacesTheKnownTokens_AndDoesNotExpandATokenInsideAValue()
    {
        string result = PlaceholderFiller.Fill("A {{Name}} B {{Total}}", Values(("Name", "{{Total}}"), ("Total", "10")), requireAll: true);

        Assert.Equal("A {{Total}} B 10", result);
    }

    [Fact]
    public void Fill_LeavesATokenWithoutAnEntryInPlace_ForTheProductionStage()
    {
        string result = PlaceholderFiller.Fill("{{CompanyLegalName}} - {{ShippingPhone}}", Values(("CompanyLegalName", "Firma")), requireAll: false);

        Assert.Equal("Firma - {{ShippingPhone}}", result);
    }

    [Fact]
    public void Fill_TurnsANullValueIntoEmptyText_AnErasedField()
    {
        Assert.Equal("[]", PlaceholderFiller.Fill("[{{ContactEmail}}]", Values(("ContactEmail", null)), requireAll: true));
    }

    [Fact]
    public void Fill_WithRequireAll_ThrowsForATokenWithoutAnEntry_EvenWhenAValueHoldsBraces()
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => PlaceholderFiller.Fill("{{Name}} {{Missing}}", Values(("Name", "{{")), requireAll: true));

        Assert.Contains("Missing", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("Name", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Fill_IsOneFunctionForEveryText_SoTheSameInputGivesTheSameOutput()
    {
        Dictionary<string, string?> values = Values(("Total", "5"));

        Assert.Equal(PlaceholderFiller.Fill("{{Total}}", values, false), PlaceholderFiller.Fill("{{Total}}", values, true));
        Assert.Equal("{{total}} {{Total1}}", PlaceholderFiller.Fill("{{total}} {{Total1}}", values, false));
    }

    // ---- the links (criterion 5) ----

    [Fact]
    public void ThePrivacyNoticeLink_IsOnEveryScreenThatOpensAPersonalDataField_AndOnTheFooter()
    {
        LegalLinkScreen[] required =
        [
            LegalLinkScreen.AccountRegistration, LegalLinkScreen.GoogleSignIn, LegalLinkScreen.OrderConfirmationStep, LegalLinkScreen.CancellationIbanEntry,
            LegalLinkScreen.DelayTerminationIbanEntry, LegalLinkScreen.WithdrawalIbanEntry, LegalLinkScreen.OrderPageIbanField, LegalLinkScreen.DefectRequest,
            LegalLinkScreen.ContactForm, LegalLinkScreen.Footer,
        ];

        Assert.Equal(required.Select(s => s.Name).Order(StringComparer.Ordinal), LegalTextLinks.PrivacyNoticeScreens.Select(s => s.Name).Order(StringComparer.Ordinal));
        Assert.Equal(10, LegalLinkScreen.All.Count);
    }

    [Fact]
    public void TheCookiePolicyLink_IsOnTheFooterOnly()
    {
        Assert.Equal([LegalLinkScreen.Footer], LegalTextLinks.CookiePolicyScreens);
    }

    // ---- the reminder (criterion 8) ----

    [Fact]
    public void TheReminderLine_NamesWhoIsResponsible_AndWhichTextBinds_AndTheLinksHoldNoDetection()
    {
        Assert.Contains("sizin sorumluluğunuzdadır", LegalTextLinks.Reminder, StringComparison.Ordinal);
        Assert.Contains("bağlayıcı olan", LegalTextLinks.Reminder, StringComparison.Ordinal);

        // The product detects no contradiction and builds no information page: the module has no member that compares a free text with a setting.
        string[] members = [.. typeof(ISettingsModule).GetMethods().Select(m => m.Name)];
        Assert.DoesNotContain(members, m => m.Contains("Contradict", StringComparison.OrdinalIgnoreCase) || m.Contains("Conflict", StringComparison.OrdinalIgnoreCase) || m.Contains("InfoPage", StringComparison.OrdinalIgnoreCase));
    }
}
