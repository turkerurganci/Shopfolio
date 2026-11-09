using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The firm's cancellation reason (T368 criteria 7 and 9, KK7, KK9; 02 7.2.4, 8.3.8): the closed list of six, the note only and always on "Other", and the firm's
/// confirmation of the owner only and always on "müşteriyle anlaşıldı (müşteri talebi)".
/// </summary>
public sealed class FirmReasonRulesTests
{
    [Fact]
    public void TheReasonList_IsClosed_AndHasTheSixReasonsOfTheDocument_KK7()
    {
        // 02 7.2.4: stokta bulunamadı · ürün hasarlı ya da satılamaz · adrese gönderim yapılamıyor · teslim edilemedi — müşteri kaynaklı · müşteriyle anlaşıldı · diğer.
        CancellationReason[] documented =
        [
            CancellationReason.OutOfStock,
            CancellationReason.DamagedOrUnsellable,
            CancellationReason.CannotShipToAddress,
            CancellationReason.UndeliveredBuyerCaused,
            CancellationReason.AgreedWithBuyer,
            CancellationReason.Other,
        ];
        Assert.Equal(documented, Enum.GetValues<CancellationReason>());
    }

    [Theory]
    [InlineData(CancellationReason.OutOfStock)]
    [InlineData(CancellationReason.DamagedOrUnsellable)]
    [InlineData(CancellationReason.CannotShipToAddress)]
    [InlineData(CancellationReason.UndeliveredBuyerCaused)]
    public void AReasonOfTheList_TakesNoNote_AndNoConfirmation_KK7(CancellationReason reason)
    {
        Assert.True(FirmReasonRules.IsComplete(reason, null, false));
        Assert.True(FirmReasonRules.IsComplete(reason, "  ", false));
        Assert.False(FirmReasonRules.IsComplete(reason, "açıklama", false));
        Assert.False(FirmReasonRules.IsComplete(reason, null, true));
    }

    [Fact]
    public void Other_NeedsItsNote_KK7()
    {
        Assert.True(FirmReasonRules.IsComplete(CancellationReason.Other, "Tedarikçi üretimi durdurdu", false));
        Assert.False(FirmReasonRules.IsComplete(CancellationReason.Other, null, false));
        Assert.False(FirmReasonRules.IsComplete(CancellationReason.Other, " ", false));
        Assert.False(FirmReasonRules.IsComplete(CancellationReason.Other, "not", true));
    }

    [Fact]
    public void AgreedWithTheCustomer_NeedsTheFirmsConfirmationOfTheOwner_NoCancellationWithoutIt_KK9()
    {
        Assert.True(FirmReasonRules.IsComplete(CancellationReason.AgreedWithBuyer, null, true));
        Assert.False(FirmReasonRules.IsComplete(CancellationReason.AgreedWithBuyer, null, false));
        Assert.False(FirmReasonRules.IsComplete(CancellationReason.AgreedWithBuyer, "not", true));
    }
}
