namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// The withdrawal exemptions as a pure rule (06 5.3.7; 02 7.3.4; 11 T371 criterion 8), the one home every module reads: the conditional reasons - (ç) the
/// hygiene-sensitive good and (e) the sealed media - keep the withdrawal button open while the protective packaging is unopened, the summary and the form
/// write that condition, and only their returned goods may be rejected at the receipt step (02 7.4.9; K-656). The absolute reasons - (b) made to order,
/// (c) perishable and expiry dated - exclude the withdrawal outright: no button. No reason excludes nothing. Nothing here reads a table or a clock.
/// </summary>
public static class WithdrawalExemptions
{
    /// <summary>True for the two conditional reasons (02 7.3.4 (ç), (e)): the right holds while the protective packaging is unopened.</summary>
    public static bool IsConditional(WithdrawalExemptionReason? reason) =>
        reason is WithdrawalExemptionReason.HygieneSensitive or WithdrawalExemptionReason.SealedMedia;
}
