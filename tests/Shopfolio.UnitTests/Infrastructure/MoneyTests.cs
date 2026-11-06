using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The kuruş rounding of 09 14.1 item 6 (T227 criteria 6, 7; KK6, KK7; 05 7.2.6; 02 3.8.3): one amount to the nearest kuruş with an exact
/// half up, and the proportional split whose parts are rounded down with the left-over kuruş on the largest part.
/// </summary>
public sealed class MoneyTests
{
    [Theory]
    [InlineData("12.345", "12.35")]
    [InlineData("12.344", "12.34")]
    [InlineData("0.005", "0.01")]
    [InlineData("0.004", "0.00")]
    [InlineData("12.34", "12.34")]
    [InlineData("1000000.005", "1000000.01")]
    [InlineData("12.355", "12.36")]
    [InlineData("12.365", "12.37")]
    [InlineData("0", "0")]
    public void Round_Amount_GoesToNearestKurusWithExactHalfUp(string amount, string expected)
    {
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), Money.Round(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void Round_ExactHalf_DoesNotGoToTheEvenDigit()
    {
        // .NET's default (to even) gives 12,34 here; the rule gives 12,35 (09 14.1 item 1).
        Assert.Equal(12.35m, Money.Round(12.345m));
        Assert.NotEqual(Math.Round(12.345m, 2), Money.Round(12.345m));
    }

    [Fact]
    public void Round_NegativeAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Money.Round(-0.005m));
    }

    [Fact]
    public void Allocate_HundredInThreeEqualWeights_GivesTheLeftOverKurusToTheFirst()
    {
        // 33,33 x 3 = 99,99; the left-over 0,01 goes to the largest weight - the first of equal ones (09 14.1 item 1).
        Assert.Equal([33.34m, 33.33m, 33.33m], Money.Allocate(100m, [1m, 1m, 1m]));
    }

    [Fact]
    public void Allocate_TenInWeightsOneTwoSeven_RoundsPartsDownAndGivesTheLeftOverToTheLargest()
    {
        IReadOnlyList<decimal> parts = Money.Allocate(10m, [1m, 2m, 7m]);

        Assert.Equal([1.00m, 2.00m, 7.00m], parts);
        Assert.Equal(10m, parts.Sum());
    }

    [Fact]
    public void Allocate_LeftOverKurus_GoesToTheLargestWeightWhereverItIs()
    {
        // 1,00 in 1:1:4: 0,1666.. -> 0,16; 0,16; 0,6666.. -> 0,66; sum 0,98, the left-over 0,02 goes to the third part.
        Assert.Equal([0.16m, 0.16m, 0.68m], Money.Allocate(1m, [1m, 1m, 4m]));
    }

    [Fact]
    public void Allocate_EqualLargestWeights_GivesTheLeftOverToTheFirstOfThem()
    {
        // 0,10 in 1:3:3: 0,0142.. -> 0,01; 0,0428.. -> 0,04; 0,04; sum 0,09, the left-over 0,01 goes to the second (the first of the two 3s).
        Assert.Equal([0.01m, 0.05m, 0.04m], Money.Allocate(0.10m, [1m, 3m, 3m]));
    }

    [Fact]
    public void Allocate_OnePart_TakesTheWholeAmount()
    {
        Assert.Equal([123.45m], Money.Allocate(123.45m, [7.5m]));
    }

    [Fact]
    public void Allocate_ZeroTotal_GivesZeroParts()
    {
        Assert.Equal([0m, 0m], Money.Allocate(0m, [1m, 2m]));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void Allocate_ZeroOrNegativeWeight_Throws(string weight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Money.Allocate(10m, [1m, decimal.Parse(weight, System.Globalization.CultureInfo.InvariantCulture)]));
    }

    [Fact]
    public void Allocate_NoWeight_Throws()
    {
        Assert.Throws<ArgumentException>(() => Money.Allocate(10m, []));
    }

    [Theory]
    [InlineData("-0.01")]
    [InlineData("10.001")]
    public void Allocate_NegativeTotalOrTotalBelowKurus_Throws(string total)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Money.Allocate(decimal.Parse(total, System.Globalization.CultureInfo.InvariantCulture), [1m, 1m]));
    }

    /// <summary>
    /// Property test without a library (09 14.1 item 6): a fixed seed, a thousand rounds. The parts add up to the total, none is negative,
    /// each has at most two decimals, each is at least the proportional amount rounded down and at most n-1 kuruş above it; only the part of
    /// the largest weight (the first of equal ones) can be above it.
    /// </summary>
    [Fact]
    public void Allocate_RandomTotalsAndWeights_KeepsTheSumAndTheRoundingDownRule()
    {
        Random random = new(20261104);
        for (int round = 0; round < 1000; round++)
        {
            decimal total = random.NextInt64(0, 100_000_000_00L) / 100m;
            int count = random.Next(1, 9);
            decimal[] weights = new decimal[count];
            for (int i = 0; i < count; i++)
            {
                weights[i] = random.Next(1, 1_000_000) / 100m;
            }

            IReadOnlyList<decimal> parts = Money.Allocate(total, weights);

            decimal weightSum = weights.Sum();
            int largest = Array.IndexOf(weights, weights.Max());
            Assert.Equal(count, parts.Count);
            Assert.Equal(total, parts.Sum());
            for (int i = 0; i < count; i++)
            {
                decimal flooredShare = decimal.Floor(total * weights[i] / weightSum * 100) / 100;
                Assert.True(parts[i] >= 0, $"round {round}: part {i} is negative");
                Assert.Equal(0m, parts[i] * 100 % 1);
                Assert.True(parts[i] >= flooredShare, $"round {round}: part {i} is below its rounded-down share");
                Assert.True(parts[i] - flooredShare <= (count - 1) / 100m, $"round {round}: part {i} is more than n-1 kuruş above its share");
                if (i != largest)
                {
                    Assert.Equal(flooredShare, parts[i]);
                }
            }
        }
    }

    // T216 (07 2.9.4; 06 2.5.1; K-1214): the limit of an amount field is the limit of numeric(12,2) - 9.999.999.999,99 TL - read from the money type.
    [Fact]
    public void MaxAmount_IsTheLargestValueOfTheMoneyType()
    {
        Assert.Equal((12, 2), Shopfolio.Infrastructure.DataModel.ModelBuilderExtensions.MoneyType);
        Assert.Equal(9999999999.99m, Money.MaxAmount);
        Assert.Equal(12, Money.MaxAmount.ToString(System.Globalization.CultureInfo.InvariantCulture).Count(char.IsAsciiDigit));
    }

    // 04 7.3.2.9: a rate is under one hundred with at most two decimals (the scale of numeric(5,2)).
    [Fact]
    public void MaxPercentage_IsOneHundredLessOneStepOfThePercentType()
    {
        Assert.Equal(2, Shopfolio.Infrastructure.DataModel.ModelBuilderExtensions.PercentType.Scale);
        Assert.Equal(99.99m, Money.MaxPercentage);
    }
}
