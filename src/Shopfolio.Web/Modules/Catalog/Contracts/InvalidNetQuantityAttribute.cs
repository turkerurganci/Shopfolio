using System.Globalization;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// A variant's net quantity (04 7.2.4.13; K-1018): above zero, at most three decimals, at most what the column holds (06 4.6 <c>numeric(12,3)</c>). The rule is the price label's
/// (<see cref="PriceLabelRules.CheckNetQuantityValue"/>); this field rule only lets the form read it and owns the value's conversion failure (07 2.9.4). 07 6.2.2 names no code of its own
/// for the net quantity: the decimal field's code - the amount's - is used (T525 ⚠; the code and the message are a backlog item for the F5 gate).
/// </summary>
public sealed class InvalidNetQuantityAttribute : FieldRuleAttribute
{
    public override string Code => FieldCodes.InvalidAmount;

    public override string? ParameterText => DisplayFormat.Number(Money.MaxNetQuantity);

    public override bool OwnsConversionFailure => true;

    protected override bool Accepts(object? value)
    {
        if (value is null)
        {
            return true;
        }

        decimal quantity = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        return PriceLabelRules.CheckNetQuantityValue(quantity) == NetQuantityProblem.None && quantity <= Money.MaxNetQuantity;
    }
}
