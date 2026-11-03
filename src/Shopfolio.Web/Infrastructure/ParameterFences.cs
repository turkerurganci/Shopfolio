namespace Shopfolio.Infrastructure;

/// <summary>
/// The fences of the company settings (02 11.1, "Çit" column) that the database constraints repeat: a fence is a limit of a firm-set value, not
/// a product constant (09 3.5; the product constants of 02 11.2 live in <c>ProductConstants</c>). The check constraints of the settings and of the
/// products read the numbers here, so a number is written once. Each constant carries its inventory id.
/// </summary>
internal static class ParameterFences
{
    /// <summary>P-5 — upper fence of the shipping period, in business days; the lower fence is zero.</summary>
    public const int ShippingPeriodUpper = 10;

    /// <summary>P-7 — lower fence of the bank transfer payment window, in business days.</summary>
    public const int BankTransferPaymentWindowLower = 1;

    /// <summary>P-7 — upper fence of the bank transfer payment window, in business days.</summary>
    public const int BankTransferPaymentWindowUpper = 3;

    /// <summary>P-8 — lower fence of the download allowance per order line.</summary>
    public const int DownloadAllowanceLower = 1;
}
