using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The rules of the digital item that need no database (T340; 02 3.12.1, 3.12.4, 3.12.9): the one reading of "the payment is confirmed", the digital quantity, the answer
/// of the add check and the fixed instances of the download results.
/// </summary>
public sealed class DownloadRulesTests
{
    [Theory]
    [InlineData(PaymentStatus.Paid, true)]
    [InlineData(PaymentStatus.PartiallyRefunded, true)]
    [InlineData(PaymentStatus.Pending, false)]
    [InlineData(PaymentStatus.Failed, false)]
    [InlineData(PaymentStatus.Refunded, false)]
    public void IsPaymentConfirmed_IsTrueOnlyForPaidAndPartlyRefunded(PaymentStatus status, bool expected)
    {
        Assert.Equal(expected, DownloadRules.IsPaymentConfirmed(status));
    }

    [Fact]
    public void IsPaymentConfirmed_AndTheQueryCondition_AgreeOnEveryStatus()
    {
        Func<Order, bool> condition = DownloadRules.PaymentConfirmed.Compile();
        foreach (PaymentStatus status in Enum.GetValues<PaymentStatus>())
        {
            Order order = NewOrder();
            ApplyPayment(order, status);

            Assert.Equal(DownloadRules.IsPaymentConfirmed(status), condition(order));
        }
    }

    [Fact]
    public void IsQuantityAllowed_ForADigitalVariant_IsOnlyTheDigitalQuantity()
    {
        Assert.True(DownloadRules.IsQuantityAllowed(ProductType.Digital, DownloadRules.DigitalQuantity));
        Assert.False(DownloadRules.IsQuantityAllowed(ProductType.Digital, DownloadRules.DigitalQuantity + 1));
        Assert.False(DownloadRules.IsQuantityAllowed(ProductType.Digital, 0));
    }

    [Theory]
    [InlineData(ProductType.Physical)]
    [InlineData(ProductType.Service)]
    public void IsQuantityAllowed_ForAnotherType_IsLeftToTheCartsOwnRules(ProductType type)
    {
        Assert.True(DownloadRules.IsQuantityAllowed(type, 7));
    }

    [Theory]
    [InlineData(true, false, false, false, false)]
    [InlineData(true, false, true, false, true)]
    [InlineData(true, true, true, true, false)]
    [InlineData(false, false, true, true, false)]
    [InlineData(false, true, false, true, false)]
    public void DigitalAddCheck_BlocksOnAQuantityOrADuplicate_AndWarnsOnlyWhenNothingBlocks(bool quantityAllowed, bool inCart, bool boughtBefore, bool blocked, bool warns)
    {
        DigitalAddCheck check = new(quantityAllowed, inCart, boughtBefore);

        Assert.Equal(blocked, check.IsBlocked);
        Assert.Equal(warns, check.ShowsRepurchaseWarning);
    }

    [Fact]
    public void DownloadResults_AreFixedInstances_AndTheirRefusalsCarryAReason()
    {
        Assert.True(DownloadTakeResult.Taken.IsTaken);
        Assert.False(DownloadTakeResult.Taken.IsRejected);
        Assert.Null(DownloadTakeResult.Taken.Reason);
        Assert.All([DownloadTakeResult.AllowanceUsed, DownloadTakeResult.NotAvailable], r => Assert.True(r.IsRejected));
        Assert.NotEqual(DownloadTakeResult.AllowanceUsed.Reason, DownloadTakeResult.NotAvailable.Reason);
        Assert.True(DownloadRenewalResult.Renewed.IsRenewed);
        Assert.All([DownloadRenewalResult.NotFound, DownloadRenewalResult.NotAvailable, DownloadRenewalResult.VersionConflict], r => Assert.True(r.IsRejected));
        Assert.Equal(3, new[] { DownloadRenewalResult.NotFound.Reason, DownloadRenewalResult.NotAvailable.Reason, DownloadRenewalResult.VersionConflict.Reason }.Distinct().Count());
    }

    private static Order NewOrder()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Order.Create("2026-DOWNLOAD", "key-digest", "access-digest", PaymentMethod.BankTransfer, true, "alici@example.com", 100m, 0m, now.AddDays(1), now);
    }

    private static void ApplyPayment(Order order, PaymentStatus status)
    {
        order.ApplyCorrection(status == PaymentStatus.Failed ? OrderStatus.Cancelled : OrderStatus.Placed, status);
    }
}
