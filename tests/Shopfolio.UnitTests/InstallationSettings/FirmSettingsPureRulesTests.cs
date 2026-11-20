using System.Reflection;

using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Panel;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.UnitTests.InstallationSettings;

/// <summary>
/// The pure parts of the firm settings' home page save and of the sales gate rows of the identity screen (T604; D-1363; T535 doubts): the required menu name
/// (<c>FirmSettings.RequiredName</c>), the trail's text difference (<c>FirmSettings.TrackText</c>) and the rows of the gate conditions
/// (<c>FirmIdentityModel.ConditionsOf</c>). The three are private static helpers of a class that needs a database to be built, so the tests reach them by name; a renamed
/// or removed helper fails the lookup here, not silently. The behaviour that needs the real rows stays with <c>HomepageAndIdentityTrailTests</c> and <c>HomeAndFirmPageTests</c>.
/// </summary>
public sealed class FirmSettingsPureRulesTests
{
    private const string Field = "MenuNameProducts";

    private static readonly SalesGateCondition[] AllConditions = [.. Enum.GetValues<SalesGateCondition>()];

    private static MethodInfo Helper(Type owner, string name)
    {
        return owner.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"{owner.Name}.{name} is gone: the pure rule moved, so this test must follow it.");
    }

    private static string? RequiredName(List<FieldRejection> rejections, string? typed) =>
        (string?)Helper(typeof(FirmSettings), "RequiredName").Invoke(null, [rejections, Field, typed]);

    private static List<AuditValueChange> Track(string? oldValue, string? newValue)
    {
        List<AuditValueChange> changes = [];
        Helper(typeof(FirmSettings), "TrackText").Invoke(null, [changes, Field, oldValue, newValue]);
        return changes;
    }

    private static List<GateConditionRowView> Rows(IReadOnlyList<SalesGateCondition> unmet, IReadOnlyList<string> missingIdentity) =>
        (List<GateConditionRowView>)Helper(typeof(FirmIdentityModel), "ConditionsOf").Invoke(null, [new SalesGate(unmet, missingIdentity)])!;

    private static GateConditionRowView Row(List<GateConditionRowView> rows, string name) => Assert.Single(rows, r => r.Name == name);

    [Theory]
    [InlineData("Ürünler", "Ürünler")]
    [InlineData("  Ürünler  ", "Ürünler")]
    [InlineData("\tÜrünler\n", "Ürünler")]
    public void RequiredName_OnATypedName_ReturnsItTrimmedAndRefusesNothing(string typed, string expected)
    {
        List<FieldRejection> rejections = [];

        string? name = RequiredName(rejections, typed);

        Assert.Equal(expected, name);
        Assert.Empty(rejections);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void RequiredName_OnAnEmptyName_ReturnsNothingAndRefusesTheFieldAsRequired(string? typed)
    {
        List<FieldRejection> rejections = [];

        string? name = RequiredName(rejections, typed);

        Assert.Null(name);
        FieldRejection refusal = Assert.Single(rejections);
        Assert.Equal(Field, refusal.Field);
        Assert.Equal(FieldCodes.FieldRequired, refusal.Code);
    }

    [Fact]
    public void RequiredName_AddsOneRefusalPerEmptyField_AndKeepsTheEarlierOnes()
    {
        List<FieldRejection> rejections = [new FieldRejection("Earlier", FieldCodes.FieldRequired)];

        RequiredName(rejections, " ");
        RequiredName(rejections, "Kept");

        Assert.Equal(["Earlier", Field], rejections.Select(r => r.Field));
    }

    [Fact]
    public void TrackText_OnAChangedValue_WritesOneRowWithTheOldAndTheNewValue()
    {
        AuditValueChange change = Assert.Single(Track("Ürünler", "Hizmetler"));

        Assert.Equal(Field, change.Field);
        Assert.Equal("Ürünler", change.OldValue);
        Assert.Equal("Hizmetler", change.NewValue);
    }

    [Theory]
    [InlineData("Ürünler", "Ürünler")]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void TrackText_OnAnUnchangedValue_WritesNoRow(string? oldValue, string? newValue)
    {
        Assert.Empty(Track(oldValue, newValue));
    }

    [Theory]
    [InlineData("Ürünler", "ürünler")]
    [InlineData("Ürünler", "Ürünler ")]
    [InlineData(null, "")]
    [InlineData("", null)]
    public void TrackText_ComparesOrdinally_SoACaseSpaceOrNullToEmptyDifferenceIsARow(string? oldValue, string? newValue)
    {
        AuditValueChange change = Assert.Single(Track(oldValue, newValue));

        Assert.Equal(oldValue, change.OldValue);
        Assert.Equal(newValue, change.NewValue);
    }

    [Fact]
    public void ConditionsOf_OnAnOpenGate_ListsTheFiveRowsInOrderAllMet()
    {
        List<GateConditionRowView> rows = Rows([], []);

        Assert.Equal(
            [
                FirmIdentityModel.IdentityConditionName, FirmIdentityModel.PaymentConditionName, FirmIdentityModel.LegalTextsConditionName,
                FirmIdentityModel.ReturnAddressConditionName, FirmIdentityModel.ClosureConditionName,
            ],
            rows.Select(r => r.Name));
        Assert.All(rows, r => Assert.True(r.IsMet));
    }

    [Fact]
    public void ConditionsOf_OnAGateWithEveryConditionUnmet_MarksEveryRowUnmetWithItsMissingText()
    {
        List<GateConditionRowView> rows = Rows(AllConditions, []);

        Assert.All(rows, r => Assert.False(r.IsMet));
        Assert.Equal("Eksik", Row(rows, FirmIdentityModel.IdentityConditionName).MissingText);
        Assert.Equal("Eksik: açık ödeme yöntemi yok", Row(rows, FirmIdentityModel.PaymentConditionName).MissingText);
        Assert.Equal("Eksik: aydınlatma metni tamamlanıp yayına alınmadı; çerez politikası yayına alınmadı", Row(rows, FirmIdentityModel.LegalTextsConditionName).MissingText);
        Assert.Equal("Eksik: iade adresi girilmedi", Row(rows, FirmIdentityModel.ReturnAddressConditionName).MissingText);
        Assert.Equal("Satış geçici olarak kapatıldı", Row(rows, FirmIdentityModel.ClosureConditionName).MissingText);
    }

    [Theory]
    [InlineData(SalesGateCondition.CompanyIdentity, FirmIdentityModel.IdentityConditionName)]
    [InlineData(SalesGateCondition.PaymentMethod, FirmIdentityModel.PaymentConditionName)]
    [InlineData(SalesGateCondition.PrivacyNotice, FirmIdentityModel.LegalTextsConditionName)]
    [InlineData(SalesGateCondition.CookiePolicy, FirmIdentityModel.LegalTextsConditionName)]
    [InlineData(SalesGateCondition.ReturnAddress, FirmIdentityModel.ReturnAddressConditionName)]
    [InlineData(SalesGateCondition.TemporaryClosure, FirmIdentityModel.ClosureConditionName)]
    public void ConditionsOf_OnOneUnmetCondition_BreaksExactlyItsRow(SalesGateCondition unmet, string rowName)
    {
        List<GateConditionRowView> rows = Rows([unmet], []);

        Assert.Equal([rowName], rows.Where(r => !r.IsMet).Select(r => r.Name));
    }

    [Fact]
    public void ConditionsOf_OnTheLegalRow_NamesOnlyThePartThatWaits()
    {
        Assert.Equal("Eksik: aydınlatma metni tamamlanıp yayına alınmadı", Row(Rows([SalesGateCondition.PrivacyNotice], []), FirmIdentityModel.LegalTextsConditionName).MissingText);
        Assert.Equal("Eksik: çerez politikası yayına alınmadı", Row(Rows([SalesGateCondition.CookiePolicy], []), FirmIdentityModel.LegalTextsConditionName).MissingText);
    }

    [Fact]
    public void ConditionsOf_OnTheIdentityRow_NamesTheMissingFieldsByTheirLabels()
    {
        string[] missing = ["LegalName", "KepAddress"];

        GateConditionRowView row = Row(Rows([SalesGateCondition.CompanyIdentity], missing), FirmIdentityModel.IdentityConditionName);

        Assert.Equal("Eksik: " + LegalTextsModel.LabelOf("LegalName") + ", " + LegalTextsModel.LabelOf("KepAddress"), row.MissingText);
        Assert.NotEqual("LegalName", LegalTextsModel.LabelOf("LegalName"));
    }

    [Fact]
    public void ConditionsOf_LinksEachRowToItsScreen_AndTheClosureRowToNone()
    {
        List<GateConditionRowView> rows = Rows([], []);

        Assert.Equal("#" + FirmIdentityModel.IdentitySectionId, Row(rows, FirmIdentityModel.IdentityConditionName).Href);
        Assert.Equal(PanelFramePaths.PaymentMethods, Row(rows, FirmIdentityModel.PaymentConditionName).Href);
        Assert.Equal(PanelFramePaths.LegalTexts, Row(rows, FirmIdentityModel.LegalTextsConditionName).Href);
        Assert.Equal(PanelFramePaths.ShippingAndDefaults, Row(rows, FirmIdentityModel.ReturnAddressConditionName).Href);
        Assert.Null(Row(rows, FirmIdentityModel.ClosureConditionName).Href);
    }
}
