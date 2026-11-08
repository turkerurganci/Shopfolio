using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The minimum order amount, the shipping fee and the free shipping threshold as pure rules (T349 KK1-KK9; 02 3.18.1-3.18.3, 3.19.1-3.19.6): one basis before the coupon, the fee once with a
/// physical item, the threshold and its message, the minimum only with a physical item, the VAT split frozen on the order.
/// </summary>
public sealed class ShippingChargeTests
{
    private const decimal Fee = 50m;
    private const decimal Threshold = 500m;
    private const decimal Minimum = 300m;

    private static readonly DateTimeOffset Now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private static readonly ShippingFeeSection AllSet = new(Fee, Threshold, Minimum, 1);

    // ---- KK1: under the minimum the order stops and the missing amount is told ----

    [Theory]
    [InlineData(170, 130, "Sipariş verebilmek için 130 TL'lik daha ürün ekleyin")]
    [InlineData(299.50, 0.50, "Sipariş verebilmek için 0,50 TL'lik daha ürün ekleyin")]
    public void UnderTheMinimum_TheOrderIsRefused_WithTheMissingAmount_KK1(double basis, double missing, string message)
    {
        ShippingAssessment assessment = ShippingCharge.Assess(AllSet, (decimal)basis, hasPhysicalItem: true);

        Assert.Equal((decimal)missing, assessment.MinimumOrderShortfall);
        Assert.Equal(message, assessment.MinimumOrderMessage);
        Assert.Equal(ErrorCode.MinimumOrderAmountNotMet, assessment.Refusal);
    }

    [Fact]
    public void AtTheMinimum_OrWithoutOne_TheOrderMayBePlaced_KK1()
    {
        Assert.Null(ShippingCharge.Assess(AllSet, Minimum, hasPhysicalItem: true).Refusal);
        Assert.Null(ShippingCharge.Assess(AllSet with { MinimumOrderAmount = null }, 1m, hasPhysicalItem: true).Refusal);
    }

    [Fact]
    public void AtConfirmation_APriceDropBelowTheMinimum_StopsTheOrder_OnTheSameRule_KK1()
    {
        // The cart saw 320; at confirmation the price fell and the basis is 280 (02 3.18.1; K-155): the confirmation's assessment refuses.
        Assert.Null(ShippingCharge.Assess(AllSet, 320m, hasPhysicalItem: true).Refusal);
        Assert.Equal(ErrorCode.MinimumOrderAmountNotMet, ShippingCharge.Assess(AllSet, 280m, hasPhysicalItem: true).Refusal);
    }

    // ---- KK2, KK7: the basis is before the coupon; a discount lowers it, the coupon does not ----

    [Fact]
    public void TheCouponShare_DoesNotLowerTheBasis_ButTheDiscountedPriceDoes_KK2_KK7()
    {
        // Discounted unit price 260 (was 300), two of them: 520 before the coupon; a coupon share of 100 makes the payable 420.
        ItemAmounts item = ItemPricing.Price(new ItemPriceInput(260m, 2, 100m, 20m));
        Assert.Equal(420m, item.Payable);

        // The basis is the amount before the coupon (02 3.10.7): the threshold is met and the minimum is too.
        ShippingAssessment assessment = ShippingCharge.Assess(AllSet, item.Amount, hasPhysicalItem: true);
        Assert.Equal(520m, assessment.Basis);
        Assert.Equal(0m, assessment.Fee);
        Assert.Null(assessment.Refusal);

        // The same items without the discount (300) would be 600; the discount lowered the basis to 520, so a threshold of 550 is no longer met.
        ShippingAssessment discounted = ShippingCharge.Assess(AllSet with { FreeShippingThreshold = 550m }, item.Amount, hasPhysicalItem: true);
        Assert.Equal(Fee, discounted.Fee);
        Assert.Equal(30m, discounted.RemainingToFreeShipping);
    }

    // ---- KK3: the minimum applies only with a physical item ----

    [Fact]
    public void ADigitalOrServiceOrder_HasNoMinimum_NoFee_AndNoMessage_KK3_KK4()
    {
        ShippingAssessment assessment = ShippingCharge.Assess(AllSet, 10m, hasPhysicalItem: false);

        Assert.Null(assessment.MinimumOrderShortfall);
        Assert.Null(assessment.Refusal);
        Assert.Equal(0m, assessment.Fee);
        Assert.Null(assessment.RemainingToFreeShipping);
        Assert.Null(assessment.FreeShippingMessage);
    }

    // ---- KK4: the fee once per order; zero makes every order free ----

    [Fact]
    public void TheFee_IsTheFirmsOneAmount_WhateverTheBasis_AndZeroIsFreeForEveryOrder_KK4()
    {
        ShippingFeeSection noThreshold = AllSet with { FreeShippingThreshold = null, MinimumOrderAmount = null };

        Assert.Equal(Fee, ShippingCharge.Assess(noThreshold, 10m, hasPhysicalItem: true).Fee);
        Assert.Equal(Fee, ShippingCharge.Assess(noThreshold, 100000m, hasPhysicalItem: true).Fee);
        Assert.Equal(0m, ShippingCharge.Assess(noThreshold with { ShippingFee = 0m }, 10m, hasPhysicalItem: true).Fee);
    }

    // ---- KK5: mixed rates are split in proportion, the left-over kuruş on the largest part ----

    [Fact]
    public void AMixedRateFee_IsSplit_AndTheOrderFreezesTheParts_KK5()
    {
        // 49,99 over physical amounts 100 (20%) and 50 (10%): 33,33 and 16,66 rounded down, the left-over kuruş on the largest part. A digital item gives no rate.
        IReadOnlyList<ShippingVatPart> parts = ItemPricing.SplitShippingFee(49.99m, [
            new ShippingBasisLine(ProductType.Physical, 20m, 100m),
            new ShippingBasisLine(ProductType.Physical, 10m, 50m),
            new ShippingBasisLine(ProductType.Digital, 1m, 900m),
        ]);
        Assert.Equal([33.33m, 16.66m], parts.Select(p => p.Part));
        Assert.Equal([5.56m, 1.51m], parts.Select(p => p.Vat));

        Order order = NewOrder(49.99m);
        order.FreezeShippingFeeVatBreakdown(parts);

        Assert.Equal([new ShippingFeeVatShare(20m, 33.33m, 5.56m), new ShippingFeeVatShare(10m, 16.66m, 1.51m)], order.ShippingFeeVatBreakdown);
    }

    [Fact]
    public void TheValidationRowsExample_SplitsTenLiraIntoItsParts_AndTheOrderFreezesThem_VAL_A797()
    {
        // 12 VAL-A797: 10,00 over physical amounts 100,00 (20%) and 200,00 (10%). 10 x 100 / 300 = 3,333.. -> 3,33; 10 x 200 / 300 = 6,666.. -> 6,66; the sum 9,99
        // leaves 0,01 for the largest part: 6,67. VAT: 3,33 x 20 / 120 = 0,555 -> 0,56; 6,67 x 10 / 110 = 0,606.. -> 0,61.
        IReadOnlyList<ShippingVatPart> parts = ItemPricing.SplitShippingFee(10.00m, [
            new ShippingBasisLine(ProductType.Physical, 20m, 100.00m),
            new ShippingBasisLine(ProductType.Physical, 10m, 200.00m),
        ]);

        Assert.Equal([new ShippingVatPart(20m, 3.33m, 0.56m), new ShippingVatPart(10m, 6.67m, 0.61m)], parts);
        Assert.Equal(10.00m, parts.Sum(p => p.Part));

        Order order = NewOrder(10.00m);
        order.FreezeShippingFeeVatBreakdown(parts);

        Assert.Equal([new ShippingFeeVatShare(20m, 3.33m, 0.56m), new ShippingFeeVatShare(10m, 6.67m, 0.61m)], order.ShippingFeeVatBreakdown);
    }

    [Fact]
    public void TheFrozenBreakdown_MustAddUpToTheFee_AndAFreeOrderHasNone_KK5()
    {
        Assert.Throws<ArgumentException>(() => NewOrder(50m).FreezeShippingFeeVatBreakdown([new ShippingVatPart(20m, 49m, 8.17m)]));

        Order free = NewOrder(0m);
        free.FreezeShippingFeeVatBreakdown([]);
        Assert.Null(free.ShippingFeeVatBreakdown);
    }

    // ---- KK6: at and above the threshold the fee is zero ----

    [Theory]
    [InlineData(499.99, 50, 0.01)]
    [InlineData(500, 0, 0)]
    [InlineData(750, 0, 0)]
    public void AtAndAboveTheThreshold_TheFeeIsZero_KK6(double basis, double fee, double remaining)
    {
        ShippingAssessment assessment = ShippingCharge.Assess(AllSet with { MinimumOrderAmount = null }, (decimal)basis, hasPhysicalItem: true);

        Assert.Equal((decimal)fee, assessment.Fee);
        Assert.Equal((decimal)remaining, assessment.RemainingToFreeShipping);
        Assert.Equal(fee == 0, assessment.IsFreeByThreshold);
    }

    // ---- KK8: the remaining amount message depends on the threshold and a physical item ----

    [Fact]
    public void TheThresholdMessage_ShowsOnlyWithAThresholdAndAPhysicalItem_KK8()
    {
        Assert.Equal("Ücretsiz kargoya 80 TL kaldı", ShippingCharge.Assess(AllSet, 420m, hasPhysicalItem: true).FreeShippingMessage);
        Assert.Equal("Ücretsiz kargoya 0,50 TL kaldı", ShippingCharge.Assess(AllSet, 499.50m, hasPhysicalItem: true).FreeShippingMessage);
        Assert.Equal("Kargo ücretsiz", ShippingCharge.Assess(AllSet, 500m, hasPhysicalItem: true).FreeShippingMessage);
        Assert.Null(ShippingCharge.Assess(AllSet with { FreeShippingThreshold = null }, 420m, hasPhysicalItem: true).FreeShippingMessage);
        Assert.Null(ShippingCharge.Assess(AllSet, 420m, hasPhysicalItem: false).FreeShippingMessage);
    }

    // ---- KK9: the fee and its split freeze on the order ----

    [Fact]
    public void TheBreakdown_IsWrittenOnce_ASecondWriteIsAProgrammingError_KK9()
    {
        Order order = NewOrder(50m);
        order.FreezeShippingFeeVatBreakdown([new ShippingVatPart(20m, 50m, 8.33m)]);

        Assert.Throws<InvalidOperationException>(() => order.FreezeShippingFeeVatBreakdown([new ShippingVatPart(10m, 50m, 4.55m)]));
        Assert.Equal([new ShippingFeeVatShare(20m, 50m, 8.33m)], order.ShippingFeeVatBreakdown);
    }

    [Fact]
    public void AnAmountOutsideItsRule_IsAProgrammingError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ShippingCharge.Assess(AllSet, -1m, hasPhysicalItem: true));
        Assert.Throws<ArgumentOutOfRangeException>(() => ShippingCharge.Assess(AllSet, 1.001m, hasPhysicalItem: true));
        Assert.Throws<ArgumentOutOfRangeException>(() => ShippingCharge.Assess(AllSet with { ShippingFee = -5m }, 1m, hasPhysicalItem: true));
    }

    private static Order NewOrder(decimal shippingFee)
        => Order.Create("2026-000001", "access-placeholder", "confirm-placeholder", PaymentMethod.Card, true, "ali@example.com", 120m + shippingFee, shippingFee, Now.AddHours(1), Now);
}
