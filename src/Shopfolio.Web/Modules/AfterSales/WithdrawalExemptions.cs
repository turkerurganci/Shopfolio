using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The withdrawal exemptions (06 5.3.7; 02 7.3.4): the conditional ones - the hygiene-sensitive good and the sealed media - hold only while the protective packaging is
/// unopened, so only their returned goods may be rejected at the receipt step (02 7.4.9; K-656). The others exclude the withdrawal outright.
/// </summary>
internal static class WithdrawalExemptions
{
    public static bool IsConditional(WithdrawalExemptionReason? reason) =>
        reason is WithdrawalExemptionReason.HygieneSensitive or WithdrawalExemptionReason.SealedMedia;
}
