namespace Shopfolio.Infrastructure;

/// <summary>
/// The check of the three fenced periods (02 4.1.5, 11.1): the shipping period has an upper fence, the bank transfer payment window has a lower
/// and an upper one, and the fulfilment period of a service has a lower one. The numbers are <see cref="ParameterFences"/>' - written once -
/// and the panel rejects a value outside them and says why: the verdict carries the side and the fence value it broke, the page turns it into
/// the message (02 12.1.4). The upper fence of the shipping period is counted back from the legal thirty days (02 4.1.5): the longest settings
/// of the payment window and the shipping period together leave room to carry the goods. The fence is a calculation, not a guarantee.
/// </summary>
internal static class PeriodFences
{
    /// <summary>The verdict on a value: where it lies and the fence it crossed (the fence value is zero for a value within).</summary>
    public readonly record struct Verdict(FenceSide Side, int Fence)
    {
        public bool IsWithin => Side == FenceSide.Within;
    }

    /// <summary>The shipping period in business days, the firm default (P-5) and a product's own (P-6): from zero up to the upper fence.</summary>
    public static Verdict CheckShippingPeriod(int businessDays)
    {
        return Check(businessDays, ParameterFences.ShippingPeriodLower, ParameterFences.ShippingPeriodUpper);
    }

    /// <summary>The bank transfer payment window in business days (P-7): between the lower and the upper fence.</summary>
    public static Verdict CheckBankTransferPaymentWindow(int businessDays)
    {
        return Check(businessDays, ParameterFences.BankTransferPaymentWindowLower, ParameterFences.BankTransferPaymentWindowUpper);
    }

    /// <summary>The fulfilment period of a service in days (P-44): at the lower fence or above; it has no upper fence.</summary>
    public static Verdict CheckFulfilmentPeriod(int days)
    {
        return Check(days, ParameterFences.FulfilmentPeriodLower, int.MaxValue);
    }

    /// <summary>The download allowance of an order line (P-8): at the lower fence or above; it has no upper fence.</summary>
    public static Verdict CheckDownloadAllowance(int downloads)
    {
        return Check(downloads, ParameterFences.DownloadAllowanceLower, int.MaxValue);
    }

    /// <summary>The most a product may be bought in one order (P-9): at the lower fence or above; it has no upper fence.</summary>
    public static Verdict CheckMaxQuantityPerOrder(int quantity)
    {
        return Check(quantity, ParameterFences.MaxQuantityPerOrderLower, int.MaxValue);
    }

    private static Verdict Check(int value, int lower, int upper)
    {
        if (value < lower)
        {
            return new Verdict(FenceSide.BelowLower, lower);
        }

        return value > upper ? new Verdict(FenceSide.AboveUpper, upper) : new Verdict(FenceSide.Within, 0);
    }
}
