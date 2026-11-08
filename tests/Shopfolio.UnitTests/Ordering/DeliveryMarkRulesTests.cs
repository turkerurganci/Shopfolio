using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The rules of the delivery mark that need no database (T351 KK2, KK5; 02 3.20.9, 3.20.11, 5.6.1): when the payment lets the service item's "completed" mark be set and offered, and which
/// delivery dates of a physical delivery are refused with which reason.
/// </summary>
public sealed class DeliveryMarkRulesTests
{
    private static readonly DateOnly Today = new(2026, 11, 8);

    [Theory]
    [InlineData(PaymentStatus.Paid, true)]
    [InlineData(PaymentStatus.PartiallyRefunded, true)]
    [InlineData(PaymentStatus.Refunded, true)]
    [InlineData(PaymentStatus.Pending, false)]
    [InlineData(PaymentStatus.Failed, false)]
    public void ThePaymentIsOpenToTheMark_ExceptWhilePendingOrFailed_KK2(PaymentStatus payment, bool expected)
    {
        Assert.Equal(expected, DeliveryMarkRules.IsPaymentOpenToMark(payment));
    }

    [Fact]
    public void TheMarkIsOfferedOnEveryStatusButPendingAndFailed_AndNeverOnAPaymentThatIsNotOpenToIt_KK2()
    {
        foreach (PaymentStatus payment in Enum.GetValues<PaymentStatus>())
        {
            bool offered = DeliveryMarkRules.IsServiceMarkOffered(ProductType.Service, payment, 1, false);

            Assert.Equal(payment is not (PaymentStatus.Pending or PaymentStatus.Failed), offered);
            Assert.Equal(DeliveryMarkRules.IsPaymentOpenToMark(payment), offered);
        }
    }

    [Theory]
    [InlineData(ProductType.Physical, PaymentStatus.Paid, 1, false)]
    [InlineData(ProductType.Digital, PaymentStatus.Paid, 1, false)]
    [InlineData(ProductType.Service, PaymentStatus.Paid, 0, false)]
    [InlineData(ProductType.Service, PaymentStatus.Paid, 1, true)]
    public void TheMarkIsOfferedOnlyOnAServiceItemWithAnOpenQuantityAndNoMark_KK1(ProductType type, PaymentStatus payment, int open, bool isMarked)
    {
        Assert.False(DeliveryMarkRules.IsServiceMarkOffered(type, payment, open, isMarked));
    }

    [Fact]
    public void AServiceItemWithAnOpenQuantityNoMarkAndAPaidOrder_IsOffered_KK1()
    {
        Assert.True(DeliveryMarkRules.IsServiceMarkOffered(ProductType.Service, PaymentStatus.Paid, 2, false));
        Assert.True(DeliveryMarkRules.IsServiceMarkOffered(ProductType.Service, PaymentStatus.PartiallyRefunded, 2, false));
    }

    [Fact]
    public void ADateInThePast_AndTheDayOfTheShipment_AndToday_AreAllowed_KK5()
    {
        DateOnly shipped = Today.AddDays(-3);

        Assert.Null(DeliveryMarkRules.CheckDeliveryDate(Today.AddDays(-1), Today, shipped));
        Assert.Null(DeliveryMarkRules.CheckDeliveryDate(shipped, Today, shipped));
        Assert.Null(DeliveryMarkRules.CheckDeliveryDate(Today, Today, shipped));
    }

    [Fact]
    public void ADateAfterToday_IsRefusedAsInTheFuture_KK5()
    {
        Assert.Equal(DeliveryDateObstacle.InTheFuture, DeliveryMarkRules.CheckDeliveryDate(Today.AddDays(1), Today, Today.AddDays(-3)));
        Assert.Equal(DeliveryDateObstacle.InTheFuture, DeliveryMarkRules.CheckDeliveryDate(Today.AddDays(1), Today, null));
    }

    [Fact]
    public void ADateBeforeTheDayOfTheShipment_IsRefusedAsBeforeTheShipment_KK5()
    {
        Assert.Equal(DeliveryDateObstacle.BeforeShipment, DeliveryMarkRules.CheckDeliveryDate(Today.AddDays(-4), Today, Today.AddDays(-3)));
    }

    [Fact]
    public void WithNoShipmentRecorded_OnlyTheFutureIsRefused_KK5()
    {
        Assert.Null(DeliveryMarkRules.CheckDeliveryDate(Today.AddYears(-1), Today, null));
        Assert.Equal(DeliveryDateObstacle.InTheFuture, DeliveryMarkRules.CheckDeliveryDate(Today.AddDays(1), Today, null));
    }
}
