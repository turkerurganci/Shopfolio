using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.AfterSales;

/// <summary>
/// What E-17 says about the refund before the last step (T512 KK9, KK10; 04 5.17.5; K-789): the shipping fee comes back only when the selection covers every open unit of
/// every physical item - the rule home's trigger (1), read over the open units - and the period is the rule home's. The IBAN field's optional cases are the card line's two
/// exceptions (04 2.12.3.1).
/// </summary>
public sealed class RefundPreviewTests
{
    private static readonly Guid First = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private static readonly Guid Second = Guid.Parse("00000000-0000-0000-0000-000000000002");

    private static readonly Guid Digital = Guid.Parse("00000000-0000-0000-0000-000000000003");

    [Fact]
    public void EveryOpenPhysicalUnitSelected_PaysTheShippingFeeBack_KK9()
    {
        Dictionary<Guid, int> open = new() { [First] = 2, [Second] = 1 };

        Assert.True(RefundPreview.CancellationRefundsShippingFee(open, new Dictionary<Guid, int> { [First] = 2, [Second] = 1 }));
        Assert.True(RefundPreview.CancellationRefundsShippingFee(open, new Dictionary<Guid, int> { [Second] = 1, [First] = 2, [Digital] = 1 }));
    }

    [Fact]
    public void AUnitThatStillGoes_KeepsTheShippingFee_KK9()
    {
        Dictionary<Guid, int> open = new() { [First] = 2, [Second] = 1 };

        Assert.False(RefundPreview.CancellationRefundsShippingFee(open, new Dictionary<Guid, int> { [First] = 1, [Second] = 1 }));
        Assert.False(RefundPreview.CancellationRefundsShippingFee(open, new Dictionary<Guid, int> { [First] = 2 }));
        Assert.False(RefundPreview.CancellationRefundsShippingFee(open, new Dictionary<Guid, int> { [Digital] = 1 }));
    }

    [Fact]
    public void AnOrderWithoutAnOpenPhysicalUnit_HasNoTrigger()
    {
        Assert.False(RefundPreview.CancellationRefundsShippingFee(new Dictionary<Guid, int>(), new Dictionary<Guid, int> { [Digital] = 1 }));
    }

    [Fact]
    public void ThePeriod_IsTheRuleHomes()
    {
        Assert.Equal(AfterSalesPeriodRules.RefundPeriodDays, RefundPreview.RefundPeriodDays);
    }

    [Theory]
    [InlineData(RefundIbanRequestReason.CardRefundFailed, true, true)]
    [InlineData(RefundIbanRequestReason.CardRefundFailed, false, true)]
    [InlineData(RefundIbanRequestReason.NoticeWithoutIban, true, false)]
    [InlineData(RefundIbanRequestReason.TransferFailed, false, true)]
    [InlineData(null, true, false)]
    public void TheIbanField_IsOptionalOnlyOnTheCardLinesTwoExceptions(RefundIbanRequestReason? reason, bool onItem, bool optional)
    {
        RefundIbanFieldView field = new(onItem ? First : null, reason is null ? "TR330006100519786457841326" : null, reason);

        Assert.Equal(optional, field.IsOptional);
        Assert.Equal(reason is null, field.IsCorrection);
    }
}
