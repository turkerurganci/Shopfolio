using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure.Validation;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Validation;

/// <summary>
/// The message catalog and the code list (T211 criterion 8, KK8; 09 3.4 item 4): the codes are the codes of 07 6.2.2 and no others, every code has one
/// pattern, every pattern belongs to a code, and a pattern names the field and the limit - never the code.
/// </summary>
public sealed partial class FieldMessageCatalogTests
{
    /// <summary>The codes of the table in 07 6.2.2, read from the document.</summary>
    private static string[] CodesOf07()
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "07_API_DESIGN.md"));
        int start = text.IndexOf("**6.2.2 Alan doğrulaması**", StringComparison.Ordinal);
        int end = text.IndexOf("**6.2.3 ", start, StringComparison.Ordinal);
        Assert.True(start > 0 && end > start, "The 6.2.2 table was not found in 07.");
        return [.. TableCode().Matches(text[start..end]).Select(m => m.Groups[1].Value)];
    }

    [GeneratedRegex(@"^\| `([A-Za-z]+)` \|", RegexOptions.Multiline)]
    private static partial Regex TableCode();

    [Fact]
    public void CodeList_IsTheListOf07_624_ExactlyAndInTheSameSpelling()
    {
        string[] fromDocument = CodesOf07();

        Assert.True(fromDocument.Length > 15);
        Assert.Equal(fromDocument.Order(StringComparer.Ordinal), FieldCodes.All.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void EveryCode_HasAPattern_AndEveryPatternHasACodeOf07()
    {
        string[] fromDocument = CodesOf07();

        Assert.Equal(fromDocument.Order(StringComparer.Ordinal), FieldMessageCatalog.Patterns.Keys.Order(StringComparer.Ordinal));
        foreach (string code in FieldCodes.All)
        {
            Assert.True(FieldMessageCatalog.Patterns.ContainsKey(code), code);
        }
    }

    [Fact]
    public void EveryPattern_NamesTheFieldFirst_IsTurkishSentence_AndNeverShowsACode()
    {
        foreach ((string code, string pattern) in FieldMessageCatalog.Patterns)
        {
            Assert.StartsWith("{0} ", pattern, StringComparison.Ordinal);
            Assert.EndsWith(".", pattern, StringComparison.Ordinal);
            foreach (string other in FieldCodes.All)
            {
                Assert.DoesNotContain(other, pattern, StringComparison.Ordinal);
            }

            Assert.Equal(pattern.Contains("{1}", StringComparison.Ordinal), FieldMessageCatalog.NeedsParameter(code));
        }
    }

    [Fact]
    public void Build_PutsTheLabelAndTheLimitIntoThePattern()
    {
        Assert.Equal("Ad boş olamaz.", FieldMessageCatalog.Build(FieldCodes.FieldRequired, "Ad", null));
        Assert.Equal("Ad en çok 150 karakter olabilir.", FieldMessageCatalog.Build(FieldCodes.TextTooLong, "Ad", "150"));
        Assert.Equal("Adet 1 ile 1000 arasında bir tam sayı olmalıdır.", FieldMessageCatalog.Build(FieldCodes.InvalidQuantity, "Adet", "1000"));
    }

    [Fact]
    public void Build_AmountAndPercentageMessages_NameTheLimit_InTheDisplayFormatTheRuleGives()
    {
        // D-124; 04 7.3.1: the value of the limit is the parameter and appears in the message (07 6.2.2: the money type's limit, K-1214).
        string amountLimit = new InvalidAmountAttribute().ParameterText!;
        string percentageLimit = new InvalidPercentageAttribute().ParameterText!;

        Assert.Equal(
            $"Fiyat geçerli bir tutar olmalıdır; en çok iki ondalık yazılabilir ve en çok {amountLimit} olabilir.",
            FieldMessageCatalog.Build(FieldCodes.InvalidAmount, "Fiyat", amountLimit));
        Assert.Equal(
            $"İndirim oranı geçerli bir yüzde olmalıdır; en çok iki ondalık yazılabilir ve en çok {percentageLimit} olabilir.",
            FieldMessageCatalog.Build(FieldCodes.InvalidPercentage, "İndirim oranı", percentageLimit));
        Assert.True(FieldMessageCatalog.NeedsParameter(FieldCodes.InvalidAmount));
        Assert.True(FieldMessageCatalog.NeedsParameter(FieldCodes.InvalidPercentage));
    }

    [Fact]
    public void Encode_ALimitOfAThousandOrMore_IsWrittenWithTheThousandsSeparator()
    {
        // 04 8.2.3: the message is a display text - "1.000", not "1000".
        Assert.Equal("InvalidQuantity:1.000", FieldError.Encode(FieldCodes.InvalidQuantity, 1000));
        Assert.True(FieldError.TryParse("InvalidAmount:9.999.999.999,99 TL", out string code, out string? parameter));
        Assert.Equal(FieldCodes.InvalidAmount, code);
        Assert.Equal("9.999.999.999,99 TL", parameter);
    }

    [Fact]
    public void Build_WithoutTheLimitTheMessageNames_OrForAnUnknownCode_Throws()
    {
        Assert.Throws<ArgumentException>(() => FieldMessageCatalog.Build(FieldCodes.TextTooLong, "Ad", null));
        Assert.Throws<ArgumentException>(() => FieldMessageCatalog.Build("NotACode", "Ad", null));
    }

    [Fact]
    public void Encode_RejectsAnUnknownCode_AndACodeThatNeedsALimitWithoutOne()
    {
        Assert.Throws<ArgumentException>(() => FieldError.Encode("Required"));
        Assert.Throws<ArgumentException>(() => FieldError.Encode(FieldCodes.TextTooLong));
        Assert.Equal("TextTooLong:20", FieldError.Encode(FieldCodes.TextTooLong, 20));
        Assert.Equal("FieldRequired", FieldError.Encode(FieldCodes.FieldRequired));
    }

    [Theory]
    [InlineData("FieldRequired", "FieldRequired", null)]
    [InlineData("TextTooLong:150", "TextTooLong", "150")]
    public void TryParse_ReadsACodeAndItsLimit(string message, string code, string? parameter)
    {
        Assert.True(FieldError.TryParse(message, out string parsedCode, out string? parsedParameter));
        Assert.Equal(code, parsedCode);
        Assert.Equal(parameter, parsedParameter);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Ad boş olamaz.")]
    [InlineData("The Name field is required.")]
    [InlineData("Unknown:5")]
    [InlineData("fieldrequired")]
    public void TryParse_AnythingThatIsNotARulesMessage_IsLeftAlone(string message)
    {
        Assert.False(FieldError.TryParse(message, out _, out _));
    }

    [Fact]
    public void AddFieldError_PutsTheEncodedCodeIntoTheModelState_ForAHandlerOwnedRule()
    {
        ModelStateDictionary state = new();

        state.AddFieldError("StockCode", FieldCodes.InvalidListValue);

        Assert.Equal("InvalidListValue", Assert.Single(state["StockCode"]!.Errors).ErrorMessage);
        Assert.Throws<ArgumentException>(() => state.AddFieldError("StockCode", "Duplicate"));
    }

    [Fact]
    public void Result_CarriesTheCodeAndTheMember_ForAModelsOwnRule()
    {
        System.ComponentModel.DataAnnotations.ValidationResult result = FieldError.Result(FieldCodes.InvalidDateRange, "EndDate");

        Assert.Equal("InvalidDateRange", result.ErrorMessage);
        Assert.Equal(["EndDate"], result.MemberNames);
    }
}
