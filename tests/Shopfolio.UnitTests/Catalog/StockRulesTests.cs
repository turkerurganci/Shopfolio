using System.Reflection;

using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Variants;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The stock rules as pure functions (T334 criteria 1-3, 5; KK1-KK3, KK5; 02 3.6.1-3.6.5): the stock is a whole number that never goes below zero; a physical variant needs it, a
/// digital variant has none, a service has it only if the firm enters a capacity; a variant is sold out when stock minus the reserved units is not above zero, and an empty stock
/// is never sold out; a product is sold out when every visible variant is; and what the visitor is given carries no number.
/// </summary>
public sealed class StockRulesTests
{
    public static TheoryData<ProductType, int?, string> Cases => new()
    {
        { ProductType.Physical, 5, "" },
        { ProductType.Physical, 0, "" },
        { ProductType.Physical, null, "PhysicalWithoutStock" },
        { ProductType.Physical, -1, "NegativeStock" },
        { ProductType.Digital, null, "" },
        { ProductType.Digital, 0, "DigitalWithStock" },
        { ProductType.Digital, 7, "DigitalWithStock" },
        { ProductType.Service, null, "" },
        { ProductType.Service, 12, "" },
        { ProductType.Service, 0, "" },
        { ProductType.Service, -3, "NegativeStock" },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void TheStock_IsRequiredForAPhysicalVariant_AbsentForADigitalOne_AndOptionalForAService_NeverBelowZero_KK1_KK2(
        ProductType type,
        int? stock,
        string expected)
    {
        Assert.Equal(expected, string.Join(",", StockRules.Check(type, stock)));
    }

    [Fact]
    public void EveryProductType_IsJudgedByTheRule_NoTypeIsLeftOut()
    {
        foreach (ProductType type in Enum.GetValues<ProductType>())
        {
            // The one stock value that fits each type: a number for physical, none for digital, either for a service.
            int? fitting = type == ProductType.Digital ? null : 3;
            Assert.Empty(StockRules.Check(type, fitting));
        }
    }

    [Theory]
    [InlineData(5, 0, false)]
    [InlineData(5, 4, false)]
    [InlineData(5, 5, true)]
    [InlineData(0, 0, true)]
    [InlineData(1, 1, true)]
    [InlineData(2, 1, false)]
    public void ASoldOutVariant_IsOneWithNoUnitLeftAfterTheReservedOnes_KK3(int stock, int reserved, bool soldOut)
    {
        Assert.Equal(soldOut, StockRules.IsSoldOut(stock, reserved));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public void AnEmptyStock_IsNeverSoldOut_ADigitalVariantAndAServiceWithoutCapacity_KK2(int reserved)
    {
        Assert.False(StockRules.IsSoldOut(null, reserved));
    }

    [Fact]
    public void AProduct_IsSoldOutOnlyWhenEveryVisibleVariantIs_AndNeverWhenItHasNone_KK4()
    {
        Assert.True(StockRules.IsProductSoldOut([true]));
        Assert.True(StockRules.IsProductSoldOut([true, true, true]));
        Assert.False(StockRules.IsProductSoldOut([true, false]));
        Assert.False(StockRules.IsProductSoldOut([false]));
        Assert.False(StockRules.IsProductSoldOut([]));
    }

    [Fact]
    public void WhatTheVisitorIsGiven_CarriesNoNumberAndNoThreshold_KK5()
    {
        // The visitor-facing types of the stock state: only identifiers and the sold-out flag; no member of any numeric, text or enum kind that could hold a count, a threshold
        // warning ("last 2 units") or a "notify me" state.
        Type[] visitorTypes = [typeof(VariantAvailability), typeof(ProductAvailability)];
        foreach (Type type in visitorTypes)
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Type kind = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                Assert.True(
                    kind == typeof(bool) || kind == typeof(Guid) || kind == typeof(IReadOnlyList<VariantAvailability>),
                    $"{type.Name}.{property.Name} is a {kind.Name}; the visitor's stock state carries only identifiers and the sold-out flag (02 3.6.5).");
            }
        }

        Assert.Equal(["IsSoldOut", "VariantId"], typeof(VariantAvailability).GetProperties().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.Equal(["IsSoldOut", "Variants"], typeof(ProductAvailability).GetProperties().Select(p => p.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheWriteResult_HasOneAnswerForEachWayTheWriteEnds_AndTheBelowReservedOneTellsTheReservedQuantity_KK7()
    {
        Assert.True(StockQuantityWriteResult.Written.IsWritten);
        Assert.False(StockQuantityWriteResult.Written.IsRejected);

        StockQuantityWriteResult below = StockQuantityWriteResult.BelowReservedBy(4);
        Assert.True(below.IsRejected);
        Assert.True(below.IsBelowReserved);
        Assert.Equal(4, below.ReservedQuantity);

        StockQuantityWriteResult[] refusals =
        [
            StockQuantityWriteResult.ShownValueChanged,
            StockQuantityWriteResult.BelowReserved,
            StockQuantityWriteResult.StockRequired,
            StockQuantityWriteResult.NotTracked,
        ];
        Assert.All(refusals, r => Assert.True(r.IsRejected));
        Assert.Equal(1, refusals.Count(r => r.IsShownValueChanged));
        Assert.Equal(1, refusals.Count(r => r.IsBelowReserved));
        Assert.Equal(1, refusals.Count(r => r.IsStockRequired));
        Assert.Equal(1, refusals.Count(r => r.IsNotTracked));
        Assert.Throws<ArgumentOutOfRangeException>(() => StockQuantityWriteResult.BelowReservedBy(-1));
    }
}
