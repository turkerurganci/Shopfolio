namespace Shopfolio.Infrastructure;

/// <summary>The fence of a firm setting (02 11.1, "Çit" column): a side that has no fence is <see langword="null"/>.</summary>
internal readonly record struct FenceBounds(int? Lower, int? Upper);

/// <summary>
/// The fences of the ten firm settings by their inventory id (02 11.1; T381 criterion 6): the one place that says which setting has which fence and
/// checks a value against it. The numbers are <see cref="ParameterFences"/>' and the checks are <see cref="PeriodFences"/>' - this class only joins the
/// id to them, so the panel and the product form ask one question. A firm setting with no fence (P-1, P-3, P-4, P-10) has no bounds and every value is
/// within; an id that is not a firm setting is a programming error.
/// </summary>
internal static class FirmSettingFences
{
    /// <summary>The bounds of a firm setting; the shipping period of a product (P-6) takes the upper fence of the firm default (P-5).</summary>
    public static FenceBounds BoundsOf(string parameterId)
    {
        return parameterId switch
        {
            "P-5" or "P-6" => new FenceBounds(ParameterFences.ShippingPeriodLower, ParameterFences.ShippingPeriodUpper),
            "P-7" => new FenceBounds(ParameterFences.BankTransferPaymentWindowLower, ParameterFences.BankTransferPaymentWindowUpper),
            "P-8" => new FenceBounds(ParameterFences.DownloadAllowanceLower, null),
            "P-9" => new FenceBounds(ParameterFences.MaxQuantityPerOrderLower, null),
            "P-44" => new FenceBounds(ParameterFences.FulfilmentPeriodLower, null),
            "P-1" or "P-3" or "P-4" or "P-10" => new FenceBounds(null, null),
            _ => throw new ArgumentException($"{parameterId} is not a firm setting (02 11.1).", nameof(parameterId)),
        };
    }

    /// <summary>Where <paramref name="value"/> lies against the fence of the setting <paramref name="parameterId"/>.</summary>
    public static PeriodFences.Verdict Check(string parameterId, int value)
    {
        return parameterId switch
        {
            "P-5" or "P-6" => PeriodFences.CheckShippingPeriod(value),
            "P-7" => PeriodFences.CheckBankTransferPaymentWindow(value),
            "P-8" => PeriodFences.CheckDownloadAllowance(value),
            "P-9" => PeriodFences.CheckMaxQuantityPerOrder(value),
            "P-44" => PeriodFences.CheckFulfilmentPeriod(value),
            "P-1" or "P-3" or "P-4" or "P-10" => new PeriodFences.Verdict(FenceSide.Within, 0),
            _ => throw new ArgumentException($"{parameterId} is not a firm setting (02 11.1).", nameof(parameterId)),
        };
    }
}
