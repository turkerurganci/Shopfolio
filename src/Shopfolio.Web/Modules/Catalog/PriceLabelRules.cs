using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog;

/// <summary>What a variant's net quantity breaks against its product's measurement unit (02 3.8.5; 06 4.6 "Uygulamada"; K-1019, K-1020).</summary>
internal enum NetQuantityProblem
{
    None,

    /// <summary>The product has a unit and the variant's net quantity is empty.</summary>
    Missing,

    /// <summary>The product has no unit and the variant carries a net quantity.</summary>
    WithoutUnit,

    /// <summary>The net quantity is zero or below (the database's check keeps it too).</summary>
    NotAboveZero,

    /// <summary>The net quantity has more than three decimals, which the column would round (06 4.6 <c>numeric(12,3)</c>; 04 7.2.4.13).</summary>
    TooManyDecimals,
}

/// <summary>
/// The rules of the price label on the product page as pure functions (02 3.8.5, 12.1.5; 11 T336 criterion 8; K-1018-K-1020, K-1023): which products show the
/// place of origin, the net quantity a variant carries against the product's unit, and the unit price derived from the variant's price and net quantity. The
/// unit is the product's, from the closed list; the net quantity is the variant's, required and above zero when the product has a unit and absent when it has
/// none. The unit price is the VAT-included price of one base unit, rounded to the kuruş (02 3.8.3), and is not shown when it equals the sale price.
/// </summary>
internal static class PriceLabelRules
{
    /// <summary>The decimals of a net quantity (06 4.6 <c>numeric(12,3)</c>; 04 7.2.4.13).</summary>
    public const int NetQuantityDecimals = Shopfolio.Infrastructure.Validation.InvalidNetQuantityAttribute.Decimals;

    /// <summary>Whether the product form asks for the place of origin as a required field: on a physical product (02 3.8.5; 04 7.2.4.12).</summary>
    public static bool IsOriginRequired(ProductType type) => type == ProductType.Physical;

    /// <summary>Whether the product carries the place of origin at all: a physical or a digital product; a service has no such field (02 3.8.5; 04 9.5.22).</summary>
    public static bool IsOriginAllowed(ProductType type) => type != ProductType.Service;

    /// <summary>Whether the product page shows the origin line: when the product carries the field and it is filled (on a physical product it always is once published).</summary>
    public static bool ShowsOrigin(ProductType type, Guid? originCountryId) => IsOriginAllowed(type) && originCountryId is not null;

    /// <summary>The verdict on a variant's net quantity against its product's unit.</summary>
    public static NetQuantityProblem CheckNetQuantity(MeasurementUnit? unit, decimal? netQuantity)
    {
        if (unit is null)
        {
            return netQuantity is null ? NetQuantityProblem.None : NetQuantityProblem.WithoutUnit;
        }

        return netQuantity is { } quantity ? CheckNetQuantityValue(quantity) : NetQuantityProblem.Missing;
    }

    /// <summary>
    /// The verdict on a net quantity's value alone - above zero, at most <see cref="NetQuantityDecimals"/> decimals (04 7.2.4.13; K-1018): the one rule that the form field's rule
    /// (<see cref="Shopfolio.Infrastructure.Validation.InvalidNetQuantityAttribute"/>, which owns the scale) and the verdict against the unit above share.
    /// </summary>
    public static NetQuantityProblem CheckNetQuantityValue(decimal quantity)
    {
        if (quantity <= 0)
        {
            return NetQuantityProblem.NotAboveZero;
        }

        return ItemPricing.HasAtMostDecimals(quantity, NetQuantityDecimals) ? NetQuantityProblem.None : NetQuantityProblem.TooManyDecimals;
    }

    /// <summary>
    /// The unit price of a variant (02 3.8.5; K-1018): <paramref name="price"/> - the price that applies, the discounted one in a discount - for one base unit,
    /// rounded to the kuruş. Null when the product has no unit, when the net quantity is not a valid one, or when the unit price equals the sale price.
    /// </summary>
    public static UnitPriceLine? UnitPrice(decimal price, MeasurementUnit? unit, decimal? netQuantity)
    {
        if (unit is not { } measurementUnit || netQuantity is not { } quantity || CheckNetQuantity(unit, netQuantity) != NetQuantityProblem.None)
        {
            return null;
        }

        (MeasurementUnit baseUnit, decimal unitsPerBase) = BaseOf(measurementUnit);
        decimal amount = Money.Round(price * unitsPerBase / quantity);
        return amount == price ? null : new UnitPriceLine(baseUnit, amount);
    }

    /// <summary>The base unit of a measurement unit and how many of the unit make one base unit (02 3.8.5): gram to 1 kilogram, millilitre to 1 litre, centimetre to 1 metre.</summary>
    public static (MeasurementUnit BaseUnit, decimal UnitsPerBase) BaseOf(MeasurementUnit unit) => unit switch
    {
        MeasurementUnit.Kilogram => (MeasurementUnit.Kilogram, 1m),
        MeasurementUnit.Gram => (MeasurementUnit.Kilogram, 1000m),
        MeasurementUnit.Litre => (MeasurementUnit.Litre, 1m),
        MeasurementUnit.Millilitre => (MeasurementUnit.Litre, 1000m),
        MeasurementUnit.Metre => (MeasurementUnit.Metre, 1m),
        MeasurementUnit.Centimetre => (MeasurementUnit.Metre, 100m),
        MeasurementUnit.SquareMetre => (MeasurementUnit.SquareMetre, 1m),
        MeasurementUnit.CubicMetre => (MeasurementUnit.CubicMetre, 1m),
        MeasurementUnit.Piece => (MeasurementUnit.Piece, 1m),
        _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "A measurement unit outside the closed list."),
    };
}
