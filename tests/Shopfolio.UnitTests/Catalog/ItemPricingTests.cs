using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The price and VAT calculation as pure functions (T336 criteria 3-7; KK3-KK7; 02 3.8.1-3.8.4, 3.19.2; 09 14.1): the item's order is fixed - unit price times
/// quantity, the coupon share taken off, the VAT separated from the rest and rounded in the item -; the order total is the sum of the rounded items; the shipping
/// fee follows the physical items' rates, split in proportion with the parts rounded down and the left-over kuruş on the largest; a price is in kuruş and above zero.
/// The expected values are worked out in the comments from the rule, not read back from the code.
/// </summary>
public sealed class ItemPricingTests
{
    // ---- KK5: the three steps in their order ----

    [Fact]
    public void AnItemWithACoupon_TakesTheShareOffFirst_ThenSeparatesTheVatFromWhatIsLeft()
    {
        // (1) 100,00 x 3 = 300,00; (2) less the coupon share 30,00 = 270,00; (3) VAT at 20%: 270 x 20 / 120 = 45,00; net 225,00.
        ItemAmounts amounts = ItemPricing.Price(new ItemPriceInput(100.00m, 3, 30.00m, 20m));

        Assert.Equal(new ItemAmounts(300.00m, 30.00m, 270.00m, 45.00m, 225.00m), amounts);
    }

    [Fact]
    public void TheVatOfACouponItem_IsNotTheVatOfItsAmountWithoutTheCoupon()
    {
        // A discounted unit price already rounded (83,33) x 2 = 166,66; coupon share 16,67 leaves 149,99; VAT 149,99 x 20 / 120 = 24,998.. -> 25,00; net 124,99.
        // Without taking the coupon off first the VAT would be 166,66 x 20 / 120 = 27,776.. -> 27,78 - the coupon does not enter the VAT base (02 3.8.3; KDV Kanunu m.25/a).
        ItemAmounts amounts = ItemPricing.Price(new ItemPriceInput(83.33m, 2, 16.67m, 20m));

        Assert.Equal(new ItemAmounts(166.66m, 16.67m, 149.99m, 25.00m, 124.99m), amounts);
        Assert.NotEqual(Money.VatOf(166.66m, 20m), amounts.Vat);
    }

    [Fact]
    public void AnItemWithoutACoupon_SeparatesTheVatFromItsAmount_AndTheNetIsTheRestNotRoundedAgain()
    {
        // 45,50 x 1 at 10%: 45,50 x 10 / 110 = 4,136.. -> 4,14; net 45,50 - 4,14 = 41,36.
        Assert.Equal(new ItemAmounts(45.50m, 0m, 45.50m, 4.14m, 41.36m), ItemPricing.Price(new ItemPriceInput(45.50m, 1, 0m, 10m)));

        // At 0% there is no VAT and the net is the whole amount.
        Assert.Equal(new ItemAmounts(20.00m, 0m, 20.00m, 0.00m, 20.00m), ItemPricing.Price(new ItemPriceInput(10.00m, 2, 0m, 0m)));
    }

    [Fact]
    public void TheItemAmount_IsNotRounded_AndTheVatIsRoundedOnceInTheItem()
    {
        // 0,07 x 3 = 0,21 (no rounding step); VAT at 20%: 0,21 x 20 / 120 = 0,035 - an exact half kuruş, up to 0,04; net 0,17.
        Assert.Equal(new ItemAmounts(0.21m, 0m, 0.21m, 0.04m, 0.17m), ItemPricing.Price(new ItemPriceInput(0.07m, 3, 0m, 20m)));
    }

    // ---- KK4: the total is the sum of the items, not derived again ----

    [Fact]
    public void TheOrderTotal_IsTheSumOfThePayableItemsAndTheShippingFee_NotTheVatOfTheSum()
    {
        // Three items of 0,10 at 20%: each VAT 0,10 x 20 / 120 = 0,0166.. -> 0,02; the items' VAT adds to 0,06, while the VAT of the sum 0,30 would be 0,05.
        List<ItemAmounts> items = [.. Enumerable.Range(0, 3).Select(_ => ItemPricing.Price(new ItemPriceInput(0.10m, 1, 0m, 20m)))];

        Assert.All(items, i => Assert.Equal(0.02m, i.Vat));
        Assert.Equal(0.06m, items.Sum(i => i.Vat));
        Assert.Equal(0.05m, Money.VatOf(0.30m, 20m));

        // The total is what the customer pays: the items as they are and the fee - 0,30 + 39,90.
        Assert.Equal(40.20m, ItemPricing.SumOrderTotal(items, 39.90m));
        Assert.Equal(items.Sum(i => i.Net + i.Vat) + 39.90m, ItemPricing.SumOrderTotal(items, 39.90m));
    }

    [Fact]
    public void TheOrderTotal_TakesTheCouponOffEachItem()
    {
        ItemAmounts first = ItemPricing.Price(new ItemPriceInput(100.00m, 3, 30.00m, 20m));
        ItemAmounts second = ItemPricing.Price(new ItemPriceInput(45.50m, 1, 5.05m, 10m));

        // 270,00 + 40,45 + no shipping fee.
        Assert.Equal(310.45m, ItemPricing.SumOrderTotal([first, second], 0m));
    }

    // ---- KK3: the shipping fee follows the physical items' rates ----

    [Fact]
    public void TheShippingFee_TakesThePhysicalItemsRate_NotTheDigitalOrServiceItemsRate()
    {
        // The digital item (20%) and the service (20%) give no rate; the one physical item (10%) takes the whole fee: 30,00 x 10 / 110 = 2,727.. -> 2,73.
        IReadOnlyList<ShippingVatPart> parts = ItemPricing.SplitShippingFee(
            30.00m,
            [new(ProductType.Digital, 20m, 1000m), new(ProductType.Physical, 10m, 50m), new(ProductType.Service, 20m, 400m)]);

        Assert.Equal([new ShippingVatPart(10m, 30.00m, 2.73m)], parts);
    }

    [Fact]
    public void TheShippingFee_OverTwoRates_IsSplitInProportionToTheirItems_PartsDownAndTheLeftOverOnTheLargest()
    {
        // Rate 20 has items of 100 and 50 (150), rate 10 one of 200. 10,00 x 150 / 350 = 4,285.. -> 4,28; 10,00 x 200 / 350 = 5,714.. -> 5,71; the sum 9,99 leaves
        // 0,01 for the largest part (rate 10): 5,72. VAT: 4,28 x 20 / 120 = 0,713.. -> 0,71; 5,72 x 10 / 110 = 0,52. The rates come in the order they first appear.
        IReadOnlyList<ShippingVatPart> parts = ItemPricing.SplitShippingFee(
            10.00m,
            [new(ProductType.Physical, 20m, 100m), new(ProductType.Physical, 10m, 200m), new(ProductType.Physical, 20m, 50m)]);

        Assert.Equal([new ShippingVatPart(20m, 4.28m, 0.71m), new ShippingVatPart(10m, 5.72m, 0.52m)], parts);
        Assert.Equal(10.00m, parts.Sum(p => p.Part));
    }

    [Fact]
    public void AFreeShippingFee_HasNoParts()
    {
        Assert.Empty(ItemPricing.SplitShippingFee(0m, [new(ProductType.Physical, 20m, 100m)]));
    }

    [Fact]
    public void AShippingFeeWithoutAPhysicalItem_IsAProgrammingError()
    {
        Assert.Throws<ArgumentException>(() => ItemPricing.SplitShippingFee(30m, [new(ProductType.Digital, 20m, 100m)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemPricing.SplitShippingFee(30.005m, [new(ProductType.Physical, 20m, 100m)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemPricing.SplitShippingFee(-1m, [new(ProductType.Physical, 20m, 100m)]));
    }

    // ---- KK6: one amount to the nearest kuruş with a half up; a split's parts down with the left-over on the largest ----

    [Fact]
    public void RoundingDirection_OneAmountHalfUp_ASplitDownWithTheLeftOverOnTheLargest()
    {
        Assert.Equal(12.35m, Money.Round(12.345m));
        Assert.Equal([33.34m, 33.33m, 33.33m], Money.Allocate(100m, [1m, 1m, 1m]));
    }

    // ---- KK7: a price is in kuruş and above zero ----

    [Theory]
    [InlineData("12.345", false)]
    [InlineData("0.00", false)]
    [InlineData("0", false)]
    [InlineData("-0.01", false)]
    [InlineData("10000000000.00", false)]
    [InlineData("0.01", true)]
    [InlineData("12.34", true)]
    [InlineData("12.340", true)]
    [InlineData("9999999999.99", true)]
    public void APrice_IsAboveZeroWithAtMostTwoDecimals_InTheRuleAndInTheFormsAmountField(string value, bool valid)
    {
        decimal price = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(valid, ItemPricing.IsValidPrice(price));

        // The panel's amount field without AllowZero is the price field of 04 7.2.4.8: three decimals and 0,00 are refused there too.
        System.ComponentModel.DataAnnotations.ValidationResult? result =
            new InvalidAmountAttribute().GetValidationResult(price, new System.ComponentModel.DataAnnotations.ValidationContext(new object()));
        Assert.Equal(valid, result == System.ComponentModel.DataAnnotations.ValidationResult.Success);
    }

    [Fact]
    public void AnItemOutsideItsRules_IsAProgrammingError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemPricing.Price(new ItemPriceInput(12.345m, 1, 0m, 20m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemPricing.Price(new ItemPriceInput(0m, 1, 0m, 20m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemPricing.Price(new ItemPriceInput(10m, 0, 0m, 20m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemPricing.Price(new ItemPriceInput(10m, 1, 10.01m, 20m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemPricing.Price(new ItemPriceInput(10m, 1, 0.005m, 20m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemPricing.Price(new ItemPriceInput(10m, 1, -0.01m, 20m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemPricing.Price(new ItemPriceInput(10m, 1, 0m, 100m)));
    }

    /// <summary>
    /// Property test without a library (09 14.1 item 6): a fixed seed, a thousand items. The payable amount is the amount less the share, the VAT is the rounded
    /// VAT of the payable amount, the net and the VAT add up to the payable amount, and every amount is in kuruş and not negative.
    /// </summary>
    [Fact]
    public void RandomItems_KeepTheOrderAndAddUp()
    {
        Random random = new(20261107);
        decimal[] rates = [0m, 1m, 10m, 20m];
        for (int round = 0; round < 1000; round++)
        {
            decimal unitPrice = random.Next(1, 10_000_00) / 100m;
            int quantity = random.Next(1, 20);
            decimal amount = unitPrice * quantity;
            decimal share = random.NextInt64(0, (long)(amount * 100) + 1) / 100m;
            decimal rate = rates[random.Next(rates.Length)];

            ItemAmounts item = ItemPricing.Price(new ItemPriceInput(unitPrice, quantity, share, rate));

            Assert.Equal(amount, item.Amount);
            Assert.Equal(amount - share, item.Payable);
            Assert.Equal(Money.VatOf(item.Payable, rate), item.Vat);
            Assert.Equal(item.Payable, item.Net + item.Vat);
            Assert.True(item.Net >= 0 && item.Vat >= 0, $"round {round}: a negative amount");
            Assert.True(ItemPricing.HasAtMostDecimals(item.Net, 2) && ItemPricing.HasAtMostDecimals(item.Vat, 2), $"round {round}: an amount below the kuruş");
        }
    }
}
