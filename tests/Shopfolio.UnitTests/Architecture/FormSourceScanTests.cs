using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The style and source rules of the form components on the source (T543 KK2, KK3, KK5, KK10; 04 2.12; 09 3.4 item 6, 16.1): the form is one column in every width class and pairs fields only from the
/// first limit, a closed list is never built as a text box, an address form has no postal code and no identity number field, and a limit is never a number typed in a component. Each rule goes red on a
/// deliberate violation and stays quiet on the allowed near miss.
/// </summary>
public sealed class FormSourceScanTests
{
    private const string Web = "src/Shopfolio.Web";

    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    private static readonly string[] ComponentFiles =
    [
        "Pages/Components/FieldModels.cs",
        "Pages/Components/AddressModels.cs",
        "Pages/Components/IbanModels.cs",
        "Pages/Components/ReasonModels.cs",
        "Pages/Shared/_Field.cshtml",
        "Pages/Shared/_FormSection.cshtml",
        "Pages/Shared/_AddressForm.cshtml",
        "Pages/Shared/_IbanField.cshtml",
        "Pages/Shared/_ReasonSelect.cshtml",
    ];

    private static string ReadWeb(string relative) => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), Web, relative));

    private static string StripComments(string text) => Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline | RegexOptions.CultureInvariant, PatternTimeout);

    private static string StripRazorComments(string text) => Regex.Replace(text, @"@\*.*?\*@", string.Empty, RegexOptions.Singleline | RegexOptions.CultureInvariant, PatternTimeout);

    // ---- KK2: one column; pairing only from the first limit ----

    private static List<string> ColumnViolations(string css)
    {
        string text = StripComments(css);
        List<string> found = [];
        Match baseRule = Regex.Match(text, @"(?<![\w-])\.form-fields\s*\{([^}]*)\}", RegexOptions.CultureInvariant, PatternTimeout);
        if (!baseRule.Success || !Regex.IsMatch(baseRule.Groups[1].Value, @"grid-template-columns:\s*minmax\(0,\s*1fr\)\s*;", RegexOptions.CultureInvariant, PatternTimeout))
        {
            found.Add(".form-fields is not one column by default");
        }

        // Cut the min-width blocks out; a several-column form-fields rule left outside them is a violation.
        string outside = Regex.Replace(text, @"@media\s*\(\s*min-width[^)]*\)\s*\{(?:[^{}]*\{[^{}]*\})*[^{}]*\}", string.Empty, RegexOptions.CultureInvariant, PatternTimeout);
        foreach (Match many in Regex.Matches(outside, @"([^{}]*)\{[^{}]*grid-template-columns:\s*(repeat\(\s*[2-9]|[^;{}\s(]+\s+[^;{}\s]|[^;{}]*\)\s+[^;{}\s])[^{}]*\}", RegexOptions.CultureInvariant, PatternTimeout))
        {
            if (many.Groups[1].Value.Contains(".form-fields", StringComparison.Ordinal))
            {
                found.Add("a form-fields rule with several columns is outside a min-width block");
            }
        }

        return found;
    }

    [Fact]
    public void TheForm_IsOneColumnByDefault_AndPairsFieldsOnlyFromTheFirstLimit_KK2()
    {
        Assert.Empty(ColumnViolations(ReadWeb("wwwroot/css/components.css")));
    }

    [Theory]
    [InlineData(".form-fields { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); }")]
    [InlineData(".form-fields { display: grid; }")]
    [InlineData(".form-fields { grid-template-columns: minmax(0, 1fr); } .form-fields--paired { grid-template-columns: repeat(2, minmax(0, 1fr)); }")]
    [InlineData(".form-fields { grid-template-columns: minmax(0, 1fr); } @media (max-width: 767px)" + " { .form-fields--paired { grid-template-columns: 1fr 1fr; } }")]
    public void AFormThatIsNotOneColumnOnTheNarrowClass_IsFound_KK2(string css)
    {
        Assert.NotEmpty(ColumnViolations(css));
    }

    [Fact]
    public void AFormThatPairsFieldsInsideAMinWidthBlock_IsQuiet_KK2()
    {
        Assert.Empty(ColumnViolations(".form-fields { grid-template-columns: minmax(0, 1fr); } @media (min-width: 768px)" + " { .form-fields--paired { grid-template-columns: repeat(2, minmax(0, 1fr)); } }"));
    }

    // ---- KK3: a closed list is never built as a text box ----

    private static List<string> ClosedListAsTextViolations(string source)
    {
        List<string> found = [];
        foreach (Match call in Regex.Matches(source, @"(Fields\.(Text|TextArea|Email|Telephone|Number)|new FieldView)\([^;]*?(ProvinceId|ReasonMember|Province\b)[^;]*;", RegexOptions.CultureInvariant, PatternTimeout))
        {
            found.Add(call.Value);
        }

        return found;
    }

    [Fact]
    public void TheProvinceAndTheReason_AreBuiltAsClosedLists_NeverAsTextBoxes_KK3()
    {
        Assert.Empty(ClosedListAsTextViolations(ReadWeb("Pages/Components/AddressModels.cs")));
        Assert.Empty(ClosedListAsTextViolations(ReadWeb("Pages/Components/ReasonModels.cs")));
        Assert.Contains("Fields.ClosedList(namePrefix + Members.ProvinceId", ReadWeb("Pages/Components/AddressModels.cs"), StringComparison.Ordinal);
        Assert.Contains("Fields.ClosedList(namePrefix + ReasonMember", ReadWeb("Pages/Components/ReasonModels.cs"), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("fields.Add(Fields.Text(namePrefix + Members.ProvinceId, Labels.Province, FieldRequirement.Required, shown.ProvinceId));")]
    [InlineData("fields.Add(Fields.Text(\n    name: namePrefix + Members.ProvinceId,\n    label: \"İl\"));")]
    [InlineData("FieldView reason = Fields.TextArea(namePrefix + ReasonMember, \"Sebep\", FieldRequirement.Required);")]
    public void AClosedListBuiltAsATextBox_IsFound_KK3(string source)
    {
        Assert.NotEmpty(ClosedListAsTextViolations(source));
    }

    [Theory]
    [InlineData("fields.Add(Fields.ClosedList(namePrefix + Members.ProvinceId, Labels.Province, FieldRequirement.Required, options, shown.ProvinceId));")]
    [InlineData("fields.Add(Fields.Text(namePrefix + Members.District, Labels.District, FieldRequirement.Required, shown.District));")]
    public void AClosedListAsASelect_OrAFreeTextDistrict_IsQuiet_KK3(string source)
    {
        Assert.Empty(ClosedListAsTextViolations(source));
    }

    [Fact]
    public void TheFieldView_DrawsAClosedListAsASelectWithNoTextBox_KK3()
    {
        string view = StripRazorComments(ReadWeb("Pages/Shared/_Field.cshtml"));
        int select = view.IndexOf("case FieldKind.ClosedList:", StringComparison.Ordinal);
        int textArea = view.IndexOf("case FieldKind.TextArea:", StringComparison.Ordinal);

        Assert.True(select >= 0 && textArea > select);
        string closedList = view[select..textArea];
        Assert.Contains("<select", closedList, StringComparison.Ordinal);
        Assert.DoesNotContain("<input", closedList, StringComparison.Ordinal);
        Assert.DoesNotContain("<datalist", view, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFieldView_ReadsTheFieldsRequirementInWords_AndNamesTheMessage_KK1()
    {
        string view = StripRazorComments(ReadWeb("Pages/Shared/_Field.cshtml"));

        Assert.Contains("class=\"field__requirement\">@Model.RequirementText", view, StringComparison.Ordinal);
        Assert.Contains("for=\"@Model.Id\"", view, StringComparison.Ordinal);
        Assert.Equal(3, Regex.Count(view, @"aria-describedby=""@Model\.DescribedBy""", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.DoesNotMatch(new Regex(@"\srequired(=|\s|>)", RegexOptions.CultureInvariant, PatternTimeout), view);
    }

    // ---- KK5: no postal code and no identity number field ----

    private static List<string> ForbiddenAddressFields(string source) =>
        [.. Regex.Matches(source, @"(Postal|PostCode|Post_Code|ZipCode|Zip\b|Tckn|NationalId|IdentityNumber|IdentityNo|TcKimlik|Kimlik No)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, PatternTimeout).Select(m => m.Value)];

    [Fact]
    public void TheAddressForm_HasNoPostalCodeAndNoIdentityNumberField_KK5()
    {
        Assert.Empty(ForbiddenAddressFields(StripRazorComments(ReadWeb("Pages/Shared/_AddressForm.cshtml"))));
        string models = Regex.Replace(ReadWeb("Pages/Components/AddressModels.cs"), @"///.*", string.Empty, RegexOptions.CultureInvariant, PatternTimeout);
        Assert.Empty(ForbiddenAddressFields(models));
    }

    [Theory]
    [InlineData("fields.Add(Fields.Text(namePrefix + \"PostalCode\", \"Posta kodu\"));")]
    [InlineData("Labels.Tckn")]
    [InlineData("<input name=\"Input.IdentityNumber\">")]
    [InlineData("<input name=\"ZipCode\">")]
    public void AnAddressFormWithAPostalCodeOrAnIdentityNumber_IsFound_KK5(string source)
    {
        Assert.NotEmpty(ForbiddenAddressFields(source));
    }

    [Theory]
    [InlineData("Fields.Text(namePrefix + Members.District, Labels.District, FieldRequirement.Required)")]
    [InlineData("Fields.Telephone(namePrefix + Members.Phone, Labels.Phone, FieldRequirement.Optional)")]
    public void TheFourAddressFieldsAndThePhone_AreQuiet_KK5(string source)
    {
        Assert.Empty(ForbiddenAddressFields(source));
    }

    // ---- KK10: a limit is never a number typed in a component ----

    private static List<string> TypedLimits(string source) =>
        [.. Regex.Matches(source, @"(maxlength=""\d|maxLength:\s*\d|MaxLength\s*=\s*\d|new TextTooLong\w*\(\s*\d|\[(StringLength|MaxLength)\()", RegexOptions.CultureInvariant, PatternTimeout).Select(m => m.Value)];

    [Fact]
    public void TheFormComponents_TypeNoLimit_KK10()
    {
        foreach (string file in ComponentFiles)
        {
            Assert.Empty(TypedLimits(StripRazorComments(ReadWeb(file))));
        }
    }

    [Theory]
    [InlineData("<input maxlength=\"150\">")]
    [InlineData("Fields.Text(\"A\", \"B\", FieldRequirement.Required, maxLength: 150)")]
    [InlineData("MaxLength = 254")]
    [InlineData("[StringLength(150)]")]
    public void ATypedLimit_IsFound_KK10(string source)
    {
        Assert.NotEmpty(TypedLimits(source));
    }

    [Theory]
    [InlineData("<input maxlength=\"@Model.MaxLength\">")]
    [InlineData("Fields.Text(\"A\", \"B\", FieldRequirement.Required, maxLength: ProductConstants.ProductNameMaxLength)")]
    public void ALimitFromTheCaller_IsQuiet_KK10(string source)
    {
        Assert.Empty(TypedLimits(source));
    }
}
