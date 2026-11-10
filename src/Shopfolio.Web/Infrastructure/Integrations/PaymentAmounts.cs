using System.Globalization;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The conversions of an amount to the form spoken with the payment provider (09 14.2; 08 2.1.4-2.1.9; K-1257, K-1258, K-1351): every conversion goes through this class and none
/// rounds - a rounding need is already an error of the calculation (09 14.1). T387 brings the first function, <see cref="ToKurus"/>, for the installer's payment result (05 8.5.12: the
/// panel's amount is compared with <c>Order.Total</c> in whole kuruş); T401 brings <see cref="ToPlainText"/> for the start's basket price; T402 brings <see cref="FromKurus"/> and
/// the comparison of item 3, <see cref="NotificationMatches"/>, for the notification; the last function of item 1 and the final query's comparison come with T403.
/// </summary>
internal static class PaymentAmounts
{
    /// <summary>
    /// The amount as plain text (09 14.2 item 1): culture-independent, a decimal point, always two places, no thousands separator (<c>34.56</c>, <c>10.00</c>). The places and the
    /// sign are checked first with <see cref="ToKurus"/>'s check and exception - a format string would round a third place; nothing is rounded here.
    /// </summary>
    public static string ToPlainText(decimal amount)
    {
        long kurus = ToKurus(amount);
        return (kurus / 100).ToString(CultureInfo.InvariantCulture) + "." + (kurus % 100).ToString("00", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// The amount × 100 as whole kuruş (09 14.2 item 1): an amount with more than two decimal places or a negative amount is refused with
    /// <see cref="ArgumentOutOfRangeException"/>; the multiplication is checked and nothing is rounded. The places are checked on the value (<c>amount * 100 % 1 != 0</c>).
    /// </summary>
    public static long ToKurus(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        decimal kurus = amount * 100m;
        if (kurus % 1 != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "An amount has at most two decimal places.");
        }

        return checked((long)kurus);
    }

    /// <summary>Whole kuruş as an amount (09 14.2 item 1): <c>kurus / 100m</c>, nothing rounded (3456 → 34.56).</summary>
    public static decimal FromKurus(long kurus) => kurus / 100m;

    /// <summary>
    /// The amount condition of a successful payment's notification (09 14.2 item 3; 08 2.1.5 "Tutar karşılaştırması"; K-1255 (4)): the started amount equals the order's
    /// total and the collected total is at least the same, both in whole kuruş - the difference is the instalment difference. The order's total goes through
    /// <see cref="ToKurus"/>; nothing is compared as a decimal.
    /// </summary>
    public static bool NotificationMatches(long paymentAmountKurus, long totalAmountKurus, decimal orderTotal)
    {
        long expected = ToKurus(orderTotal);
        return paymentAmountKurus == expected && totalAmountKurus >= expected;
    }
}
