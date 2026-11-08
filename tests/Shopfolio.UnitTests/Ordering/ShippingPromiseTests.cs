using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The delivery promise as pure rules (T350 criteria 5-7; KK5-KK7; 02 3.20.4-3.20.6; 04 5.4.5): only a physical item has a period, the product's own period overrides the firm default, the
/// order promises the longest of its items, the text says business days, the period counts from the payment approval in business days, and a product's period is held to the P-6 fence.
/// </summary>
public sealed class ShippingPromiseTests
{
    private const int CompanyDefault = 2;

    [Theory]
    [InlineData(ProductType.Physical, null, CompanyDefault)]
    [InlineData(ProductType.Physical, 5, 5)]
    [InlineData(ProductType.Physical, 0, 0)]
    public void APhysicalItem_TakesItsProductsPeriod_AndTheFirmDefaultOtherwise_KK6(ProductType type, int? product, int expected)
    {
        Assert.Equal(expected, ShippingPromise.Effective(type, product, CompanyDefault));
    }

    [Theory]
    [InlineData(ProductType.Digital)]
    [InlineData(ProductType.Service)]
    public void ADigitalOrAServiceItem_MakesNoPromise_WhateverItsProductOrTheFirmSays_KK5(ProductType type)
    {
        Assert.Null(ShippingPromise.Effective(type, null, CompanyDefault));
        Assert.Null(ShippingPromise.Effective(type, 7, CompanyDefault));
    }

    [Fact]
    public void TheOrdersPromise_IsTheLongestOfItsItems_AndEmptyWhenNoItemIsPhysical_KK6()
    {
        Assert.Equal(7, ShippingPromise.OfOrder([2, null, 7, 3]));
        Assert.Equal(0, ShippingPromise.OfOrder([0, null]));
        Assert.Null(ShippingPromise.OfOrder([null, null]));
        Assert.Null(ShippingPromise.OfOrder([]));
    }

    [Theory]
    [InlineData(1, "1 iş günü içinde kargoya verilir")]
    [InlineData(3, "3 iş günü içinde kargoya verilir")]
    [InlineData(10, "10 iş günü içinde kargoya verilir")]
    public void TheStorefrontText_NamesTheNumberOfBusinessDays_KK5(int days, string expected)
    {
        Assert.Equal(expected, ShippingPromise.Text(days));
    }

    [Fact]
    public void TheNegativePeriod_IsNotAPromise()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ShippingPromise.Text(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ShippingPromise.PromiseEnds(DateTimeOffset.UnixEpoch, -1));
    }

    [Fact]
    public void ThePromiseEnd_IsTheShippingPeriodCountedInBusinessDaysFromTheApproval_KK7()
    {
        BusinessCalendar calendar = BusinessCalendar.Product;
        DateTimeOffset approved = new(2026, 11, 9, 10, 30, 0, TimeSpan.FromHours(3));

        foreach (int days in new[] { 1, 2, 5, ParameterFences.ShippingPeriodUpper })
        {
            DateTimeOffset expected = PeriodCounting.EndOf(CountedPeriod.ShippingPeriod, approved, days, calendar);
            Assert.Equal(expected, ShippingPromise.PromiseEnds(approved, days));
            Assert.Equal(DayCounting.BusinessDay, PeriodCounting.CountingOf(CountedPeriod.ShippingPeriod));
        }

        // The approval's own day is not counted: one business day after a Monday approval ends at 00:00 of Wednesday.
        Assert.Equal(TurkeyTime.StartOf(new DateOnly(2026, 11, 11)), ShippingPromise.PromiseEnds(approved, 1));
    }

    [Fact]
    public void AWeekendApproval_StartsCountingOnTheNextBusinessDay_KK7()
    {
        DateTimeOffset saturday = new(2026, 11, 7, 12, 0, 0, TimeSpan.FromHours(3));

        Assert.Equal(TurkeyTime.StartOf(new DateOnly(2026, 11, 11)), ShippingPromise.PromiseEnds(saturday, 2));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(0, true)]
    [InlineData(10, true)]
    [InlineData(11, false)]
    [InlineData(-1, false)]
    public void AProductsOwnPeriod_IsHeldToTheFenceOfP6_KK5(int? period, bool within)
    {
        Assert.Equal(within, ProductDefinitionRules.CheckShippingPeriod(period));
        IReadOnlyList<ProductDefinitionProblem> problems = ProductDefinitionRules.Check(ProductType.Physical, PublishStatus.Draft, null, null, Guid.Parse("019a4e3a-0000-7002-8000-000000000001"), period);
        ProductDefinitionProblem[] expected = within ? [] : [ProductDefinitionProblem.ShippingPeriodOutsideTheFence];
        Assert.Equal(expected, problems);
    }
}
