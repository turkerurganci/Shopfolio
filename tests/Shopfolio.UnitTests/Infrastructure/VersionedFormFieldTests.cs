using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The hidden fields, the shown-value comparison and the conflict controls of a form (T208 criteria 1-3, 5, 6; KK1-KK3, KK5, KK6; 07 2.4). The end-to-end
/// behaviour on pages and the database is in <c>VersionedFormPageTests</c> of the integration tests.
/// </summary>
public sealed class VersionedFormFieldTests
{
    // --- KK1: the version field, named by the column (07 2.4.1) ---

    [Fact]
    public void Version_IsAHiddenFieldNamedVersion_WithTheValue()
    {
        string html = VersionFields.Render(VersionFields.Version(7));

        Assert.Equal("<input name=\"Version\" type=\"hidden\" value=\"7\" />", html);
    }

    [Fact]
    public void Version_WithAModelPath_IsNamedUnderIt()
    {
        Assert.Contains("name=\"Input.Version\"", VersionFields.Render(VersionFields.Version(7, "Input")), StringComparison.Ordinal);
    }

    [Fact]
    public void SectionVersion_IsNamedByTheSectionsColumn_AndOnlyAVersionColumnIsAccepted()
    {
        string html = VersionFields.Render(VersionFields.SectionVersion("PaymentMethodsVersion", 3));

        Assert.Contains("name=\"PaymentMethodsVersion\"", html, StringComparison.Ordinal);
        Assert.Contains("value=\"3\"", html, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => VersionFields.SectionVersion("PaymentMethods", 3));
        Assert.Throws<ArgumentException>(() => VersionFields.SectionVersion(string.Empty, 3));
    }

    [Fact]
    public void Version_IsWrittenWithTheInvariantCulture_NotTheCurrentOne()
    {
        System.Globalization.CultureInfo before = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("ar-SA");

            Assert.Contains("value=\"1234\"", VersionFields.Render(VersionFields.Version(1234)), StringComparison.Ordinal);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = before;
        }
    }

    // --- KK3: the shown value in a field behind the Original prefix (07 2.4.3) ---

    [Theory]
    [InlineData("StockQuantity", "OriginalStockQuantity")]
    [InlineData("Text", "OriginalText")]
    [InlineData("OpenQuantity", "OriginalOpenQuantity")]
    public void OriginalName_IsThePrefixAndTheFieldsName(string field, string expected)
    {
        Assert.Equal(expected, VersionFields.OriginalName(field));
        Assert.True(VersionFields.IsOriginalName(expected));
    }

    [Fact]
    public void OpenQuantityField_IsTheCustomerItemFieldOfK1202()
    {
        Assert.Equal("OriginalOpenQuantity", VersionFields.OpenQuantityField);
    }

    [Theory]
    [InlineData("Original")]
    [InlineData("Stock")]
    [InlineData("originalStock")]
    [InlineData("")]
    public void IsOriginalName_OnAnythingButThePrefixAndAFieldsName_IsFalse(string name)
    {
        Assert.False(VersionFields.IsOriginalName(name));
    }

    [Fact]
    public void Original_IsAHiddenFieldWithTheShownValue_AndAnEmptyValueIsAnEmptyField()
    {
        Assert.Equal(
            "<input name=\"OriginalStockQuantity\" type=\"hidden\" value=\"12\" />",
            VersionFields.Render(VersionFields.Original("StockQuantity", 12)));
        Assert.Equal(
            "<input name=\"Items[0].OriginalOpenQuantity\" type=\"hidden\" value=\"\" />",
            VersionFields.Render(VersionFields.Original("OpenQuantity", (int?)null, "Items[0]")));
    }

    [Fact]
    public void Original_EncodesTheShownText()
    {
        string html = VersionFields.Render(VersionFields.Original("Text", "a \"quoted\" <b>note</b> & more"));

        Assert.DoesNotContain("<b>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("\"quoted\"", html, StringComparison.Ordinal);
        Assert.Contains("&amp; more", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Version", true)]
    [InlineData("PaymentMethodsVersion", true)]
    [InlineData("HomepageVersion", true)]
    [InlineData("Stock", false)]
    [InlineData("Versioning", false)]
    public void IsVersionColumn_IsTheNameThatEndsWithVersion(string name, bool expected)
    {
        Assert.Equal(expected, VersionFields.IsVersionColumn(name));
    }

    // --- The shown number (07 2.4.3, 2.4.5) ---

    [Theory]
    [InlineData("12", 12, true)]
    [InlineData("0", 0, true)]
    [InlineData("12", 13, false)]
    [InlineData("12", null, false)]
    [InlineData("", null, true)]
    [InlineData(null, null, true)]
    [InlineData("", 0, false)]
    [InlineData("0", null, false)]
    [InlineData("abc", 0, false)]
    [InlineData("1.5", 1, false)]
    [InlineData("-1", -1, false)]
    [InlineData("99999999999", 0, false)]
    [InlineData(" 12", 12, false)]
    public void Matches_OfAShownNumber_IsTrueOnlyForTheSameNumberOrTheSameEmptyValue(string? posted, int? current, bool expected)
    {
        Assert.Equal(expected, ShownValue.Matches(posted, current));
    }

    [Fact]
    public void TryReadNumber_OfAWord_IsNotAShownValue()
    {
        Assert.False(ShownValue.TryReadNumber("ten", out int? value));
        Assert.Null(value);
        Assert.True(ShownValue.TryReadNumber(string.Empty, out int? empty));
        Assert.Null(empty);
    }

    [Theory]
    [InlineData("kontrol edildi", "kontrol edildi", true)]
    [InlineData("a\r\nb", "a\nb", true)]
    [InlineData("a\nb", "a\r\nb", true)]
    [InlineData(null, "", true)]
    [InlineData("", null, true)]
    [InlineData("Not", "not", false)]
    [InlineData("a b", "a  b", false)]
    [InlineData("a", "b", false)]
    [InlineData("a", null, false)]
    public void Matches_OfAShownText_IgnoresOnlyTheBrowsersLineBreak(string? posted, string? current, bool expected)
    {
        Assert.Equal(expected, ShownValue.Matches(posted, current));
    }

    [Theory]
    [InlineData("true", true, true)]
    [InlineData("false", false, true)]
    [InlineData("True", true, true)]
    [InlineData("true", false, false)]
    [InlineData("", false, false)]
    [InlineData(null, true, false)]
    [InlineData("yes", true, false)]
    public void Matches_OfAShownSwitch_NeedsABooleanThatEqualsTheSwitchNow(string? posted, bool current, bool expected)
    {
        Assert.Equal(expected, ShownValue.Matches(posted, current));
    }

    // --- KK6: the order's version after the lock (05 7.3.2) ---

    [Fact]
    public void ShownVersion_MatchesOnlyTheVersionTheFormShowed()
    {
        FaqItem item = FaqItem.Create("Soru", 1, DateTimeOffset.UnixEpoch);

        Assert.True(ShownVersion.Matches(item, 1));
        item.BumpVersion();
        Assert.False(ShownVersion.Matches(item, 1));
        Assert.True(ShownVersion.Matches(item, 2));
    }

    // --- KK5, KK6: the controls of a conflict (07 2.4.5, 2.4.6; 04 2.10; K-800, K-799) ---

    [Fact]
    public void RecordSaveControl_WithoutAConflict_IsTheOrdinarySaveButtonOnly()
    {
        string html = VersionFields.Render(ConflictControls.RecordSaveControl(false, "/panel/urun/1"));

        Assert.Equal("<button type=\"submit\">Kaydet</button>", html);
    }

    [Fact]
    public void RecordSaveControl_WithAConflict_IsTheMessageTheLinkToTheCurrentStateInANewTabAndOneSubmitButton()
    {
        string html = System.Net.WebUtility.HtmlDecode(VersionFields.Render(ConflictControls.RecordSaveControl(true, "/panel/urun/ornek-urun")));

        Assert.Contains(ConflictControls.RecordMessage, html, StringComparison.Ordinal);
        Assert.Contains("role=\"alert\"", html, StringComparison.Ordinal);
        Assert.Contains("<a href=\"/panel/urun/ornek-urun\" rel=\"noopener noreferrer\" target=\"_blank\">Güncel hâli aç</a>", html, StringComparison.Ordinal);
        Assert.Equal(1, CountOf(html, "<button"));
        Assert.Contains("<button type=\"submit\">Yine de kaydet</button>", html, StringComparison.Ordinal);
        Assert.DoesNotContain(">Kaydet<", html, StringComparison.Ordinal);
    }

    [Fact]
    public void RecordMessage_IsTheTextOfK800()
    {
        Assert.Equal(
            "Bu kayıt siz düzenlerken başka bir yöneticice kaydedildi. Yine de kaydederseniz onun değişiklikleri sizinkilerle değişir.",
            ConflictControls.RecordMessage);
    }

    [Theory]
    [InlineData("//example.com")]
    [InlineData("https://example.com/panel")]
    [InlineData("panel/urun")]
    [InlineData("/\\example.com")]
    public void RecordSaveControl_ToAnythingButASitePath_Throws(string path)
    {
        Assert.Throws<ArgumentException>(() => ConflictControls.RecordSaveControl(true, path));
    }

    [Fact]
    public void OrderNotice_IsAMessageOnly_ThereIsNoWayToOverwrite()
    {
        string html = VersionFields.Render(ConflictControls.OrderNotice(ConflictControls.OrderMessage));

        Assert.Contains("Sipariş siz bakarken değişti; işlem uygulanmadı. Siparişin güncel hâli aşağıda.", System.Net.WebUtility.HtmlDecode(html), StringComparison.Ordinal);
        Assert.DoesNotContain("<button", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<a ", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Yine de", html, StringComparison.Ordinal);
    }

    [Fact]
    public void OrderIbanMessage_IsTheTextOfK799()
    {
        Assert.Equal("Müşteri IBAN'ını değiştirdi; geri ödeme işlenmedi. Güncel IBAN aşağıda.", ConflictControls.OrderIbanMessage);
    }

    // --- 2.4.2: the customer's item operations carry the open quantity, not a version ---

    [Fact]
    public void TheCustomerItemFieldIsTheOpenQuantity_AndNoVersionColumnIsInIt()
    {
        Assert.False(VersionFields.IsVersionColumn(VersionFields.OpenQuantityField));
    }

    // --- Product, order: the version is the interface's ---

    [Fact]
    public void TheRecordsOfTheEditFormsAndTheOrder_AreVersionedRecords()
    {
        Assert.True(typeof(Shopfolio.Infrastructure.DataModel.IVersionedRecord).IsAssignableFrom(typeof(Product)));
        Assert.True(typeof(Shopfolio.Infrastructure.DataModel.IVersionedRecord).IsAssignableFrom(typeof(Coupon)));
        Assert.True(typeof(Shopfolio.Infrastructure.DataModel.IVersionedRecord).IsAssignableFrom(typeof(Order)));
    }

    private static int CountOf(string text, string part)
    {
        int count = 0;
        for (int index = text.IndexOf(part, StringComparison.Ordinal); index >= 0; index = text.IndexOf(part, index + part.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }
}
