using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The provider amount rule over the compiled code (T403; 09 14.2 item 4): in the link layer (<c>Shopfolio.Infrastructure.Integrations</c>) a text is turned into an
/// amount only by the conversion class <c>PaymentAmounts</c> - <c>decimal.Parse</c>, <c>decimal.TryParse</c>, <c>Convert.ToDecimal</c> and the JSON readers'
/// decimal getters are not called anywhere else in the layer. The calls are read from the compiled code, so a method group, a lambda, a span overload and a
/// <c>using static</c> name the same member; the exemption is the one class by its full name. Each form is shown to fail on the violation sample.
/// </summary>
public sealed partial class TimeAndMoneyRulesTests
{
    private const string PaymentAmountsClass = "Shopfolio.Infrastructure.Integrations.PaymentAmounts";

    [Fact]
    public void ProviderAmountParsing_InTheLinkLayer_IsOnlyInPaymentAmounts()
    {
        Assert.Empty(ProviderAmountParsingOutsidePaymentAmounts(_application.Value));
        Assert.Contains(Calls(_application.Value), c => IsClassOrNested(c.Caller, PaymentAmountsClass) && ProviderAmountParsingMember().IsMatch(c.Target));
    }

    [Theory]
    [InlineData("Parse", "System.Decimal::Parse(System.String,System.IFormatProvider)")]
    [InlineData("ParseSpan", "System.Decimal::Parse(System.ReadOnlySpan`1<System.Char>,System.Globalization.NumberStyles,System.IFormatProvider)")]
    [InlineData("TryParse", "System.Decimal::TryParse(")]
    [InlineData("Convert", "System.Convert::ToDecimal(System.String,System.IFormatProvider)")]
    [InlineData("FromJson", "System.Text.Json.JsonElement::GetDecimal()")]
    [InlineData("TryFromJson", "System.Text.Json.JsonElement::TryGetDecimal(")]
    [InlineData("ParseAll", "System.Decimal::Parse(System.String)")]
    [InlineData("ParseAllInLambda", "System.Decimal::Parse(System.String,System.IFormatProvider)")]
    public void ProviderAmountParsing_OnViolationSample_IsCaughtInEachForm(string member, string target)
    {
        Assert.Contains(
            ProviderAmountParsingOutsidePaymentAmounts(_sample.Value),
            f => f.Caller == "Shopfolio.Infrastructure.Integrations.ProviderAmountReader"
                && f.Member.StartsWith(member + "(", StringComparison.Ordinal)
                && f.Target.Contains(" " + target, StringComparison.Ordinal));
    }

    [Fact]
    public void ProviderAmountParsing_OnViolationSample_ExemptsPaymentAmountsByItsNameOnly_AndLooksOnlyAtTheLayer()
    {
        IReadOnlyList<(string Caller, string Member, string Target)> findings = ProviderAmountParsingOutsidePaymentAmounts(_sample.Value);

        Assert.DoesNotContain(findings, f => f.Caller == PaymentAmountsClass);
        Assert.DoesNotContain(findings, f => f.Caller.StartsWith("Shopfolio.Infrastructure.Validation.", StringComparison.Ordinal));
        Assert.Contains(Calls(_sample.Value), c => c.Caller == PaymentAmountsClass && ProviderAmountParsingMember().IsMatch(c.Target));
    }

    private static List<(string Caller, string Member, string Target)> ProviderAmountParsingOutsidePaymentAmounts(ArchUnitNET.Domain.Architecture architecture)
    {
        return Calls(architecture)
            .Where(c => LinkLayerType().IsMatch(c.Caller) && ProviderAmountParsingMember().IsMatch(c.Target) && !IsClassOrNested(c.Caller, PaymentAmountsClass))
            .ToList();
    }

    [GeneratedRegex(@"^Shopfolio\.Infrastructure\.Integrations\.")]
    private static partial Regex LinkLayerType();

    [GeneratedRegex(
        @" System\.Decimal::(Try)?Parse\(| System\.Convert::ToDecimal\(| System\.Text\.Json\.(JsonElement|Utf8JsonReader)::(Try)?GetDecimal\(| System\.Xml\.XmlConvert::ToDecimal\(")]
    private static partial Regex ProviderAmountParsingMember();
}
