using Shopfolio.Infrastructure.Notices;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Orders;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The words of E-18 and E-19 and the root their answers go back to (T513 KK1, KK2, KK4, KK7; 04 5.18, 5.19; 07 2.1.3, 6.3.3): the reason of a return, the refund and result
/// sentences of the withdrawal, and the root a screen's address stands under.
/// </summary>
public sealed class OrderScreenTextsTests
{
    // ---- The root of a screen (07 2.1.3) ----

    [Theory]
    [InlineData("/hesabim/siparisler/SP-1/cayma", "cayma", "/hesabim/siparisler/SP-1")]
    [InlineData("/siparis/AbCdEf123/ayip-talebi", "ayip-talebi", "/siparis/AbCdEf123")]
    public void TheRootOfAScreen_IsItsAddressWithoutTheLastSegment_AsWritten_KK1(string path, string screen, string root)
    {
        Assert.Equal(root, OrderRoots.RootOf(path, screen));
    }

    [Fact]
    public void TheRootOfAScreen_KeepsTheCaseOfTheKey_KK1()
    {
        Assert.Equal("/siparis/aBc", OrderRoots.RootOf("/siparis/aBc/cayma", "cayma"));
    }

    [Fact]
    public void AScreenOffTheList_IsRefused()
    {
        Assert.Throws<ArgumentException>(() => OrderRoots.RootOf("/siparis/abc/iade", "iade"));
    }

    // ---- The reason of a return (07 6.3.3) ----

    [Fact]
    public void TheTwoReturnCodes_HaveTheirOwnWords_KK4_KK7()
    {
        Assert.Equal(OrderScreenTexts.OperationClosedText, OrderScreenTexts.ReasonOf(NoticeCode.OperationClosed));
        Assert.Equal(OrderScreenTexts.QuantityExceedsOpenText, OrderScreenTexts.ReasonOf(NoticeCode.QuantityExceedsOpen));
    }

    [Fact]
    public void NoCode_OrACodeOffTheList_SaysNothing()
    {
        Assert.Null(OrderScreenTexts.ReasonOf(null));
        Assert.Null(OrderScreenTexts.ReasonOf(NoticeCode.SocialLoginFailed));
    }

    // ---- The refund and the last step of the withdrawal (04 5.18.6, 5.18.10; K-785) ----

    [Fact]
    public void TheRefund_NamesItsWayAndItsStart_KK1()
    {
        Assert.Equal(
            "Geri ödeme malın firmaya ulaşmasından itibaren 14 gün içinde kartınıza yapılır.",
            OrderScreenTexts.Refund(PaymentMethod.Card, fromGoodsArrival: true, 14));
        Assert.Equal(
            "Geri ödeme beyanınızdan itibaren 14 gün içinde IBAN'ınıza yapılır.",
            OrderScreenTexts.Refund(PaymentMethod.BankTransfer, fromGoodsArrival: false, 14));
    }

    [Fact]
    public void TheResultSentence_CountsTheUnitsChosen_KK2()
    {
        Assert.Equal(
            "Seçtiğiniz 3 ürün için cayma beyanınız kaydedilecek; geri ödeme beyanınızdan itibaren 14 gün içinde kartınıza yapılacak.",
            OrderScreenTexts.WithdrawalSentence(3, PaymentMethod.Card, fromGoodsArrival: false, 14));
    }

    [Fact]
    public void TheSendingPeriod_NamesTheDaysAndTheLastDay_KK1()
    {
        Assert.Equal(
            "Ürünü beyanınızdan itibaren 10 gün içinde, en geç 5 Ocak 2027 tarihine kadar gönderin.",
            OrderScreenTexts.SendingPeriod(10, new DateOnly(2027, 1, 5)));
    }

    [Fact]
    public void FurtherInformation_GoesToTheFirmsEmail_WhenThereIsOne_KK6()
    {
        Assert.Contains("destek@example.com", OrderScreenTexts.MoreInformation("destek@example.com"), StringComparison.Ordinal);
        Assert.DoesNotContain("@", OrderScreenTexts.MoreInformation(null), StringComparison.Ordinal);
    }

    [Fact]
    public void TheTexts_NeverWriteAPeriodOfTheirOwn()
    {
        // K-785: the periods are the module's values; the fixed sentences carry no number of days.
        string[] fixedTexts =
        [
            OrderScreenTexts.OperationClosedText, OrderScreenTexts.QuantityExceedsOpenText, OrderScreenTexts.PackagingConditionText, OrderScreenTexts.CarrierReturnsText,
            OrderScreenTexts.SendingWayText, OrderScreenTexts.PartlyPerformedServiceText, OrderScreenTexts.ShippingFeeText, OrderScreenTexts.ReopenIsSameClaimText,
        ];
        Assert.All(fixedTexts, t => Assert.DoesNotMatch(@"\d", t));
    }
}
