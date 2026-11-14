using System.Globalization;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>
/// A variant's net quantity (04 7.2.4.13; K-1018): above zero, at most <see cref="Decimals"/> decimals, at most what the column holds (06 4.6 <c>numeric(12,3)</c>). The price label's
/// rule (<c>PriceLabelRules.CheckNetQuantityValue</c>) reads the same scale from here: a contract or an infrastructure type does not reach a module's internals, so the field's rule is the
/// home of the scale. 07 6.2.2 names no code of its own for the net quantity: the decimal field's code - the amount's - is used (T525 ⚠; D-1397, the F5 gate).
/// </summary>
public sealed class InvalidNetQuantityAttribute : FieldRuleAttribute
{
    /// <summary>The decimals of a net quantity (06 4.6 <c>numeric(12,3)</c>; 04 7.2.4.13).</summary>
    public const int Decimals = 3;

    public override string Code => FieldCodes.InvalidAmount;

    public override string? ParameterText => DisplayFormat.Number(Money.MaxNetQuantity);

    public override string? MessagePattern => FieldMessageCatalog.NetQuantityPattern;

    public override bool OwnsConversionFailure => true;

    protected override bool Accepts(object? value)
    {
        if (value is null)
        {
            return true;
        }

        decimal quantity = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        return quantity > 0 && HasAtMostDecimals(quantity, Decimals) && quantity <= Money.MaxNetQuantity;
    }
}
