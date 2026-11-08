using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Security;

/// <summary>
/// The password policy (T342 KK1; 02 3.13.15; 09 15.1 item 5; P-17): the length limit counted in text elements, the shipped list compared without letter case,
/// no letter, digit or symbol asked for, and an empty value reported as a missing field.
/// </summary>
public sealed class PasswordPolicyTests
{
    [Fact]
    public void Limit_IsTheProductConstant_AndTheTestsBelowUseIt()
    {
        Assert.Equal(10, ProductConstants.PasswordMinLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_OnAnEmptyValue_ReportsAMissingField(string? entered)
    {
        Assert.Equal(ErrorCode.FieldRequired, PasswordPolicy.Validate(entered));
    }

    [Fact]
    public void Validate_OnNineCharacters_IsTooWeak_AndOnTenItPasses()
    {
        Assert.Equal(ErrorCode.PasswordTooWeak, PasswordPolicy.Validate(new string('x', ProductConstants.PasswordMinLength - 1)));
        Assert.Null(PasswordPolicy.Validate("kqzvbnmwrt"));
    }

    [Fact]
    public void Validate_AsksForNoLetterDigitOrSymbol()
    {
        Assert.Null(PasswordPolicy.Validate("kqzvbnmwrtyp"));
        Assert.Null(PasswordPolicy.Validate("KQZVBNMWRTYP"));
        Assert.Null(PasswordPolicy.Validate("4829175036"));
        Assert.Null(PasswordPolicy.Validate("!?#%&/()=*-"));
    }

    [Fact]
    public void Validate_KeepsWhiteSpace_AsPartOfTheValue()
    {
        Assert.Null(PasswordPolicy.Validate("          "));
        Assert.Equal(ErrorCode.PasswordTooWeak, PasswordPolicy.Validate("         "));
    }

    [Fact]
    public void IsLongEnough_CountsTextElements_NotCodeUnits()
    {
        string combined = string.Concat(Enumerable.Repeat("é", 9));
        string family = string.Concat(Enumerable.Repeat("\U0001F468‍\U0001F469‍\U0001F467", 9));

        Assert.False(PasswordPolicy.IsLongEnough(combined));
        Assert.False(PasswordPolicy.IsLongEnough(family));
        Assert.True(PasswordPolicy.IsLongEnough(combined + "é"));
        Assert.True(PasswordPolicy.IsLongEnough(family + "\U0001F468‍\U0001F469‍\U0001F467"));
    }

    [Theory]
    [InlineData("qwertyuiop")]
    [InlineData("QWERTYUIOP")]
    [InlineData("QwertyUiop")]
    [InlineData("1234567890")]
    [InlineData("Password12")]
    [InlineData("PASSWORD123")]
    public void IsCommon_OnAListedValue_IgnoresLetterCase(string entered)
    {
        Assert.True(PasswordPolicy.IsCommon(entered));
    }

    [Theory]
    [InlineData("qwertyuiop")]
    [InlineData("QWERTYUIOP")]
    [InlineData("password123")]
    [InlineData("PassWord123")]
    public void Validate_OnAListedValueOfEnoughLength_IsTooWeak(string entered)
    {
        Assert.Equal(ErrorCode.PasswordTooWeak, PasswordPolicy.Validate(entered));
    }

    [Fact]
    public void IsCommon_OnAnUnlistedValue_IsFalse()
    {
        Assert.False(PasswordPolicy.IsCommon("kqzvbnmwrt-zxcv-8421"));
    }

    [Fact]
    public void IsCommon_OnNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PasswordPolicy.IsCommon(null!));
    }

    [Fact]
    public void ShippedList_IsNotEmpty_AndHoldsNoEntryBelowTheLimit()
    {
        Assert.True(PasswordPolicy.CommonCount > 0);

        string root = Support.RepositoryRoot.Find();
        string[] entries = File.ReadAllLines(Path.Combine(root, "src", "Shopfolio.Web", "Infrastructure", "Security", "CommonPasswords.txt"))
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && l[0] != '#')
            .ToArray();

        Assert.NotEmpty(entries);
        Assert.All(entries, e => Assert.True(PasswordPolicy.IsLongEnough(e), e));
        Assert.All(entries, e => Assert.True(PasswordPolicy.IsCommon(e), e));
    }
}
