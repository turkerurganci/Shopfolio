using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The price label rules as pure functions (T336 criterion 8; KK8; 02 3.8.5; K-1018-K-1020, K-1023): the origin line is shown on a physical product and on a digital
/// product whose field is filled, never on a service; a variant's net quantity is required and above zero when its product has a unit and absent when it has none;
/// the unit price is the price of one base unit, rounded to the kuruş with an exact half up, and is not shown when it equals the sale price. The expected unit prices
/// are worked out in the comments.
/// </summary>
public sealed class PriceLabelRulesTests
{
    private static readonly Guid Origin = Guid.Parse("019a4e3a-0000-7002-8000-000000000001");

    [Fact]
    public void TheOriginLine_IsShownOnAPhysicalAndAFilledDigitalProduct_NeverOnAServiceOrAnEmptyDigitalProduct()
    {
        Assert.True(PriceLabelRules.ShowsOrigin(ProductType.Physical, Origin));
        Assert.True(PriceLabelRules.ShowsOrigin(ProductType.Digital, Origin));
        Assert.False(PriceLabelRules.ShowsOrigin(ProductType.Digital, null));
        Assert.False(PriceLabelRules.ShowsOrigin(ProductType.Service, Origin));
        Assert.False(PriceLabelRules.ShowsOrigin(ProductType.Service, null));
    }

    [Theory]
    [InlineData(null, null, "None")]
    [InlineData(null, "1", "WithoutUnit")]
    [InlineData(MeasurementUnit.Kilogram, null, "Missing")]
    [InlineData(MeasurementUnit.Kilogram, "0", "NotAboveZero")]
    [InlineData(MeasurementUnit.Gram, "-1", "NotAboveZero")]
    [InlineData(MeasurementUnit.Kilogram, "0.0005", "TooManyDecimals")]
    [InlineData(MeasurementUnit.Kilogram, "0.250", "None")]
    [InlineData(MeasurementUnit.Piece, "6", "None")]
    public void TheNetQuantity_IsRequiredAndAboveZeroWithAUnit_AndAbsentWithout(MeasurementUnit? unit, string? netQuantity, string expected)
    {
        decimal? quantity = netQuantity is null ? null : decimal.Parse(netQuantity, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(Enum.Parse<NetQuantityProblem>(expected), PriceLabelRules.CheckNetQuantity(unit, quantity));
    }

    [Theory]
    [InlineData("45.00", MeasurementUnit.Gram, "250", MeasurementUnit.Kilogram, "180.00")] // 45 x 1000 / 250
    [InlineData("30.00", MeasurementUnit.Millilitre, "500", MeasurementUnit.Litre, "60.00")] // 30 x 1000 / 500
    [InlineData("15.00", MeasurementUnit.Centimetre, "75", MeasurementUnit.Metre, "20.00")] // 15 x 100 / 75
    [InlineData("100.00", MeasurementUnit.SquareMetre, "2.5", MeasurementUnit.SquareMetre, "40.00")] // 100 / 2,5
    [InlineData("90.00", MeasurementUnit.CubicMetre, "0.75", MeasurementUnit.CubicMetre, "120.00")] // 90 / 0,75
    [InlineData("30.00", MeasurementUnit.Piece, "6", MeasurementUnit.Piece, "5.00")] // 30 / 6
    [InlineData("10.00", MeasurementUnit.Kilogram, "0.333", MeasurementUnit.Kilogram, "30.03")] // 10 / 0,333 = 30,030..
    [InlineData("0.01", MeasurementUnit.Gram, "16", MeasurementUnit.Kilogram, "0.63")] // 0,01 x 1000 / 16 = 0,625: an exact half, up
    [InlineData("12.00", MeasurementUnit.Litre, "1.5", MeasurementUnit.Litre, "8.00")] // 12 / 1,5
    [InlineData("50.00", MeasurementUnit.Metre, "2", MeasurementUnit.Metre, "25.00")] // 50 / 2
    public void TheUnitPrice_IsThePriceOfOneBaseUnit_RoundedToTheKurus(
        string price, MeasurementUnit unit, string netQuantity, MeasurementUnit baseUnit, string expected)
    {
        UnitPriceLine? line = PriceLabelRules.UnitPrice(Dec(price), unit, Dec(netQuantity));

        Assert.Equal(new UnitPriceLine(baseUnit, Dec(expected)), line);
    }

    [Fact]
    public void TheUnitPrice_IsNotShownWhenItEqualsTheSalePrice_OrWithoutAUnitOrAValidNetQuantity()
    {
        // 1 kilogram at 150,00: the unit price is the sale price itself.
        Assert.Null(PriceLabelRules.UnitPrice(150.00m, MeasurementUnit.Kilogram, 1m));
        Assert.Null(PriceLabelRules.UnitPrice(150.00m, MeasurementUnit.Gram, 1000m));
        Assert.Null(PriceLabelRules.UnitPrice(150.00m, null, null));
        Assert.Null(PriceLabelRules.UnitPrice(150.00m, MeasurementUnit.Kilogram, null));
        Assert.Null(PriceLabelRules.UnitPrice(150.00m, MeasurementUnit.Kilogram, 0m));
    }

    [Fact]
    public void EveryUnitOfTheClosedList_HasItsBaseUnit()
    {
        // 02 3.8.5: kilogram and gram 1 kilogram, litre and millilitre 1 litre, metre and centimetre 1 metre, the others one of themselves.
        Dictionary<MeasurementUnit, (MeasurementUnit, decimal)> expected = new()
        {
            [MeasurementUnit.Kilogram] = (MeasurementUnit.Kilogram, 1m),
            [MeasurementUnit.Gram] = (MeasurementUnit.Kilogram, 1000m),
            [MeasurementUnit.Litre] = (MeasurementUnit.Litre, 1m),
            [MeasurementUnit.Millilitre] = (MeasurementUnit.Litre, 1000m),
            [MeasurementUnit.Metre] = (MeasurementUnit.Metre, 1m),
            [MeasurementUnit.Centimetre] = (MeasurementUnit.Metre, 100m),
            [MeasurementUnit.SquareMetre] = (MeasurementUnit.SquareMetre, 1m),
            [MeasurementUnit.CubicMetre] = (MeasurementUnit.CubicMetre, 1m),
            [MeasurementUnit.Piece] = (MeasurementUnit.Piece, 1m),
        };

        Assert.Equal(Enum.GetValues<MeasurementUnit>().Order(), expected.Keys.Order());
        foreach (MeasurementUnit unit in Enum.GetValues<MeasurementUnit>())
        {
            Assert.Equal(expected[unit], PriceLabelRules.BaseOf(unit));
        }
    }

    private static decimal Dec(string value) => decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
}
