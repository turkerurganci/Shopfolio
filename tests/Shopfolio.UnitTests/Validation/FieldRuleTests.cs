using System.ComponentModel.DataAnnotations;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.UnitTests.Validation;

/// <summary>
/// The rule attributes, one by one (T211 KK7; 09 3.4 item 7): a valid and an invalid value for each, and the code in the error message. The code is the
/// one of 07 6.2.2; a limit's value follows it after a colon.
/// </summary>
public sealed class FieldRuleTests
{
    private static string? MessageOf(FieldRuleAttribute rule, object? value)
    {
        ValidationContext context = new(new object()) { MemberName = "Field" };
        return rule.GetValidationResult(value, context)?.ErrorMessage;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void FieldRequired_NullEmptyAndWhiteSpaceText_AreRejectedWithTheCode(string? value)
    {
        Assert.Equal("FieldRequired", MessageOf(new FieldRequiredAttribute(), value));
    }

    [Fact]
    public void FieldRequired_EmptyListAndEmptyKey_AreRejected_AndFilledValuesPass()
    {
        FieldRequiredAttribute rule = new();

        Assert.Equal("FieldRequired", MessageOf(rule, new List<int>()));
        Assert.Equal("FieldRequired", MessageOf(rule, Guid.Empty));
        Assert.Null(MessageOf(rule, new List<int> { 1 }));
        Assert.Null(MessageOf(rule, Guid.NewGuid()));
        Assert.Null(MessageOf(rule, "ad"));
        Assert.Null(MessageOf(rule, 0));
    }

    [Theory]
    [InlineData("ad@example.com", true)]
    [InlineData("ad.soyad+etiket@alt.example.com.tr", true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("ad@example", false)]
    [InlineData("@example.com", false)]
    [InlineData("ad@", false)]
    [InlineData("ad@@example.com", false)]
    [InlineData("a@b@example.com", false)]
    [InlineData("ad example@example.com", false)]
    [InlineData("ad@example..com", false)]
    [InlineData("ad@.example.com", false)]
    [InlineData("ad@-example.com", false)]
    [InlineData(".ad@example.com", false)]
    [InlineData("ad.@example.com", false)]
    [InlineData("a..b@example.com", false)]
    [InlineData("a.b.c@example.com", true)]
    [InlineData("ad@example.com, baska@example.com", false)]
    public void InvalidEmail_AcceptsOneAddressAndAnEmptyValue(string value, bool valid)
    {
        InvalidEmailAttribute rule = new();

        if (valid)
        {
            Assert.Null(MessageOf(rule, value));
        }
        else
        {
            Assert.Equal("InvalidEmail", MessageOf(rule, value));
        }
    }

    [Fact]
    public void InvalidEmail_OverTheLengthOfAnAddress_IsRejected()
    {
        Assert.Equal("InvalidEmail", MessageOf(new InvalidEmailAttribute(), new string('a', 250) + "@example.com"));
    }

    [Theory]
    [InlineData("5321234567", true)]
    [InlineData("05321234567", true)]
    [InlineData("+905321234567", true)]
    [InlineData("0 (532) 123-45 67", true)]
    [InlineData("+90 532 123 45 67", true)]
    [InlineData("", true)]
    [InlineData("532123456", false)]
    [InlineData("53212345678", false)]
    [InlineData("0532123456a", false)]
    [InlineData("0053212345678", false)]
    [InlineData("+9005321234567", false)]
    [InlineData("0212345678", false)]
    public void InvalidPhone_AcceptsTenDigitsWithTheOptionalPrefix(string value, bool valid)
    {
        InvalidPhoneAttribute rule = new();

        if (valid)
        {
            Assert.Null(MessageOf(rule, value));
        }
        else
        {
            Assert.Equal("InvalidPhone", MessageOf(rule, value));
        }
    }

    [Theory]
    [InlineData("TR330006100519786457841326", true)]
    [InlineData("TR33 0006 1005 1978 6457 8413 26", true)]
    [InlineData("", true)]
    [InlineData("TR330006100519786457841327", false)]
    [InlineData("TR33000610051978645784132", false)]
    [InlineData("GB82WEST12345698765432", false)]
    [InlineData("TR33000610051978645784132A", false)]
    public void InvalidIban_ChecksTheFormAndTheCheckDigits(string value, bool valid)
    {
        InvalidIbanAttribute rule = new();

        if (valid)
        {
            Assert.Null(MessageOf(rule, value));
        }
        else
        {
            Assert.Equal("InvalidIban", MessageOf(rule, value));
        }
    }

    [Theory]
    [InlineData("10.50", true)]
    [InlineData("0.01", true)]
    [InlineData("99999.99", true)]
    [InlineData("0", false)]
    [InlineData("-1", false)]
    [InlineData("10.505", false)]
    [InlineData("100000", false)]
    public void InvalidAmount_PriceRule_RejectsZeroNegativeThreeDecimalsAndOverTheLimit(string text, bool valid)
    {
        InvalidAmountAttribute rule = new() { Max = 99999.99 };
        decimal amount = decimal.Parse(text, System.Globalization.CultureInfo.InvariantCulture);

        if (valid)
        {
            Assert.Null(MessageOf(rule, amount));
        }
        else
        {
            Assert.Equal("InvalidAmount:" + DisplayFormat.Amount(99999.99m), MessageOf(rule, amount));
        }
    }

    [Fact]
    public void InvalidAmount_SettingRule_AllowsZero_AndOwnsTheConversionFailure()
    {
        InvalidAmountAttribute rule = new() { AllowZero = true };

        Assert.Null(MessageOf(rule, 0m));
        Assert.Equal("InvalidAmount:" + DisplayFormat.Amount(Money.MaxAmount), MessageOf(rule, -0.01m));
        Assert.True(rule.OwnsConversionFailure);
    }

    [Fact]
    public void InvalidAmount_Limit_IsTheMoneyTypesLimit_UnlessALowerMaxIsGiven_AndTheMessageNamesItAsAnAmount()
    {
        InvalidAmountAttribute open = new();
        InvalidAmountAttribute lower = new() { Max = 1000 };

        // 04 7.3.2.8, K-1214: the upper limit of an amount field is the limit of numeric(12,2); a value above it is the field's message, not a rounding.
        Assert.Equal(Money.MaxAmount, open.Limit);
        Assert.Null(MessageOf(open, Money.MaxAmount));
        Assert.Equal("InvalidAmount:" + DisplayFormat.Amount(Money.MaxAmount), MessageOf(open, Money.MaxAmount + 0.01m));
        Assert.Equal("9.999.999.999,99 TL", open.ParameterText);
        Assert.Equal(1000m, lower.Limit);
        Assert.Equal("1.000 TL", lower.ParameterText);
        Assert.Equal(Money.MaxAmount, new InvalidAmountAttribute { Max = double.MaxValue }.Limit);
    }

    [Fact]
    public void InvalidPercentage_MessageNamesTheLargestRate_InTheDisplayFormatOfAPercentage()
    {
        Assert.Equal("%99,99", new InvalidPercentageAttribute().ParameterText);
        Assert.Null(MessageOf(new InvalidPercentageAttribute(), Money.MaxPercentage));
    }

    [Fact]
    public void InvalidDate_PassesADayAndAnEmptyValue_AndOwnsTheConversionFailure()
    {
        InvalidDateAttribute rule = new();

        Assert.Null(MessageOf(rule, new DateOnly(2026, 10, 4)));
        Assert.Null(MessageOf(rule, null));
        Assert.Equal("InvalidDate", MessageOf(rule, "2026-10-04"));
        Assert.True(rule.OwnsConversionFailure);
    }

    [Theory]
    [InlineData("0.01", false, true)]
    [InlineData("99.99", false, true)]
    [InlineData("0", false, false)]
    [InlineData("0", true, true)]
    [InlineData("100", true, false)]
    [InlineData("100", false, false)]
    [InlineData("12.345", false, false)]
    [InlineData("-5", true, false)]
    public void InvalidPercentage_IsAboveZeroOrZeroWhenAllowed_AndUnderOneHundred_WithTwoDecimals(string text, bool allowZero, bool valid)
    {
        InvalidPercentageAttribute rule = new() { AllowZero = allowZero };
        decimal rate = decimal.Parse(text, System.Globalization.CultureInfo.InvariantCulture);

        if (valid)
        {
            Assert.Null(MessageOf(rule, rate));
        }
        else
        {
            Assert.Equal("InvalidPercentage:" + DisplayFormat.Percent(Money.MaxPercentage), MessageOf(rule, rate));
        }
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(1000, true)]
    [InlineData(0, false)]
    [InlineData(-3, false)]
    [InlineData(1001, false)]
    public void InvalidQuantity_IsAWholeNumberFromOneToTheLimit_AndNamesTheLimit(int value, bool valid)
    {
        InvalidQuantityAttribute rule = new(1000);

        if (valid)
        {
            Assert.Null(MessageOf(rule, value));
        }
        else
        {
            Assert.Equal("InvalidQuantity:" + DisplayFormat.Number(1000), MessageOf(rule, value));
        }
    }

    [Fact]
    public void InvalidQuantity_AFractionIsRejected_AndAZeroLimitIsNotAllowed()
    {
        Assert.Equal("InvalidQuantity:10", MessageOf(new InvalidQuantityAttribute(10), 1.5m));
        Assert.Null(MessageOf(new InvalidQuantityAttribute(10), 2.0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InvalidQuantityAttribute(0));
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com/a?b=c", true)]
    [InlineData("HTTPS://Example.com", true)]
    [InlineData("", true)]
    [InlineData("example.com", false)]
    [InlineData("ftp://example.com", false)]
    [InlineData("https://", false)]
    [InlineData("https://exa mple.com", false)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("//example.com", false)]
    public void InvalidUrl_NeedsAFullHttpOrHttpsAddress(string value, bool valid)
    {
        InvalidUrlAttribute rule = new();

        if (valid)
        {
            Assert.Null(MessageOf(rule, value));
        }
        else
        {
            Assert.Equal("InvalidUrl", MessageOf(rule, value));
        }
    }

    [Fact]
    public void TextTooLong_CountsCharacters_NamesTheLimit_AndPassesAnEmptyText()
    {
        TextTooLongAttribute rule = new(5);

        Assert.Null(MessageOf(rule, string.Empty));
        Assert.Null(MessageOf(rule, "çğışö"));
        Assert.Null(MessageOf(rule, "👍👍👍👍👍"));
        Assert.Equal("TextTooLong:5", MessageOf(rule, "çğışöü"));
        Assert.Equal("TextTooLong:5", MessageOf(rule, "👍👍👍👍👍👍"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextTooLongAttribute(0));
    }

    [Fact]
    public void InvalidListValue_AcceptsTheListedTexts_OrTheNamesOfAnEnum_AndAnEmptyValue()
    {
        InvalidListValueAttribute listed = new("Damaged", "Wrong");
        InvalidListValueAttribute named = new(typeof(DayOfWeek));

        Assert.Null(MessageOf(listed, "Damaged"));
        Assert.Null(MessageOf(listed, string.Empty));
        Assert.Equal("InvalidListValue", MessageOf(listed, "damaged"));
        Assert.Equal("InvalidListValue", MessageOf(listed, "Hacked"));
        Assert.Null(MessageOf(named, "Monday"));
        Assert.Null(MessageOf(named, DayOfWeek.Friday));
        Assert.Equal("InvalidListValue", MessageOf(named, "Someday"));
    }

    [Theory]
    [InlineData("#a1B2c3", true)]
    [InlineData("", true)]
    [InlineData("a1b2c3", false)]
    [InlineData("#a1b2c", false)]
    [InlineData("#a1b2c3d", false)]
    [InlineData("#g1b2c3", false)]
    public void InvalidColor_IsAHashAndSixHexadecimalDigits(string value, bool valid)
    {
        InvalidColorAttribute rule = new();

        if (valid)
        {
            Assert.Null(MessageOf(rule, value));
        }
        else
        {
            Assert.Equal("InvalidColor", MessageOf(rule, value));
        }
    }

    [Fact]
    public void CheckboxNotAccepted_NeedsATrueValue()
    {
        CheckboxNotAcceptedAttribute rule = new();

        Assert.Null(MessageOf(rule, true));
        Assert.Equal("CheckboxNotAccepted", MessageOf(rule, false));
        Assert.Equal("CheckboxNotAccepted", MessageOf(rule, null));
    }

    [Fact]
    public void EveryRule_ReportsACodeOfTheList_AndTheRuleAttributesAreTheProjectsOnly()
    {
        FieldRuleAttribute[] rules =
        [
            new FieldRequiredAttribute(), new InvalidEmailAttribute(), new InvalidPhoneAttribute(), new InvalidIbanAttribute(), new InvalidAmountAttribute(),
            new InvalidPercentageAttribute(), new InvalidQuantityAttribute(5), new InvalidUrlAttribute(), new TextTooLongAttribute(5),
            new InvalidListValueAttribute("A"), new InvalidColorAttribute(), new CheckboxNotAcceptedAttribute(), new InvalidDateAttribute(),
            new PasswordTooWeakAttribute(),
        ];

        foreach (FieldRuleAttribute rule in rules)
        {
            Assert.True(FieldCodes.IsKnown(rule.Code), rule.GetType().Name);
            Assert.Equal(rule.GetType().Name, rule.Code + "Attribute");
        }

        Assert.Equal(rules.Length, rules.Select(r => r.Code).Distinct().Count());
    }
}
