namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// The names of the withdrawal exemption reasons in the words of the Regulation's article 15 (02 7.3.4; K-509): the one home of the texts - the order summary says the reason with them and the
/// product form's closed list offers them (04 9.5.7).
/// </summary>
public static class WithdrawalExemptionReasonNames
{
    /// <summary>The reason's name.</summary>
    public static string Of(WithdrawalExemptionReason reason) => reason switch
    {
        WithdrawalExemptionReason.MadeToOrder => "tüketicinin istekleri veya kişisel ihtiyaçları doğrultusunda hazırlanan mal",
        WithdrawalExemptionReason.Perishable => "çabuk bozulabilen mal",
        WithdrawalExemptionReason.ExpiryDated => "son kullanma tarihi geçebilecek mal",
        WithdrawalExemptionReason.HygieneSensitive => "teslimden sonra koruyucu unsurları açılmış, iadesi sağlık ve hijyen açısından uygun olmayan mal",
        WithdrawalExemptionReason.SealedMedia => "teslimden sonra koruyucu unsurları açılmış kitap, dijital içerik ve bilgisayar sarf malzemesi",
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unknown withdrawal exemption reason."),
    };
}
