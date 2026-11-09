using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>The threshold and the window of one limit on one axis (02 11.2): more than <see cref="Threshold"/> rows in <see cref="Window"/> is exceeded.</summary>
internal readonly record struct AttemptRule(int Threshold, TimeSpan Window);

/// <summary>The number of a limit counted from the orders (02 11.2): the maximum and the window, which is null when the limit counts what is open now (L-8).</summary>
internal readonly record struct OrderCountedRule(int Max, TimeSpan? Window);

/// <summary>
/// The attempt limits L-1 to L-6 and L-9 with the axes they count on (02 8.2, 11.2 P-31 to P-36, P-49; 09 15.4 item 5), in one table. Every number is
/// read from <see cref="ProductConstants"/>; none is written here or at a call site (a call passes the limit and the subject, and the table gives the
/// rule). L-7 and L-8 are counted from the orders and have no row here (05 6.3.2; T377). The window of a limit is the one the clean-up of the attempt
/// records reads (<see cref="Disposal.AttemptRecordDisposalPart.WindowMinutes"/>) - a test keeps the two equal (D-103).
/// </summary>
internal static class AttemptLimits
{
    private static readonly Dictionary<(AttemptLimit Limit, AttemptAxis Axis), AttemptRule> _rules = new()
    {
        [(AttemptLimit.L1, AttemptAxis.Email)] = Rule(ProductConstants.SignInAttemptsPerEmail, ProductConstants.SignInAttemptWindowMinutes),
        [(AttemptLimit.L1, AttemptAxis.Ip)] = Rule(ProductConstants.SignInAttemptsPerIp, ProductConstants.SignInAttemptWindowMinutes),
        [(AttemptLimit.L1, AttemptAxis.RecognizedBrowser)] = Rule(ProductConstants.SignInAttemptsPerEmail, ProductConstants.SignInAttemptWindowMinutes),
        [(AttemptLimit.L2, AttemptAxis.Email)] = Rule(ProductConstants.PasswordResetAttemptsPerEmail, ProductConstants.PasswordResetAttemptWindowMinutes),
        [(AttemptLimit.L2, AttemptAxis.Ip)] = Rule(ProductConstants.PasswordResetAttemptsPerIp, ProductConstants.PasswordResetAttemptWindowMinutes),
        [(AttemptLimit.L3, AttemptAxis.Email)] = Rule(ProductConstants.RegistrationAttemptsPerEmail, ProductConstants.RegistrationAttemptWindowMinutes),
        [(AttemptLimit.L3, AttemptAxis.Ip)] = Rule(ProductConstants.RegistrationAttemptsPerIp, ProductConstants.RegistrationAttemptWindowMinutes),
        [(AttemptLimit.L4, AttemptAxis.Ip)] = Rule(ProductConstants.ContactFormAttemptsPerIp, ProductConstants.ContactFormAttemptWindowMinutes),
        [(AttemptLimit.L5, AttemptAxis.Email)] = Rule(ProductConstants.GuestOrderLookupAttemptsPerEmail, ProductConstants.GuestOrderLookupAttemptWindowMinutes),
        [(AttemptLimit.L5, AttemptAxis.Ip)] = Rule(ProductConstants.GuestOrderLookupAttemptsPerIp, ProductConstants.GuestOrderLookupAttemptWindowMinutes),
        [(AttemptLimit.L6, AttemptAxis.Cart)] = Rule(ProductConstants.InvalidCouponAttemptsPerCart, ProductConstants.InvalidCouponAttemptWindowMinutes),
        [(AttemptLimit.L9, AttemptAxis.Email)] = Rule(ProductConstants.VerificationResendAttemptsPerEmail, ProductConstants.VerificationResendAttemptWindowMinutes),
        [(AttemptLimit.L9, AttemptAxis.Ip)] = Rule(ProductConstants.VerificationResendAttemptsPerIp, ProductConstants.VerificationResendAttemptWindowMinutes),
    };

    /// <summary>
    /// The rule of a limit that is counted from the orders (L-7, L-8; 02 11.2 P-37, P-46): the number is the product constant, never written elsewhere. L-7 has a window
    /// (P-37); L-8 has none - it counts the orders that are open at this moment (02 8.2), so its window is null.
    /// </summary>
    public static OrderCountedRule OrderCountedOf(AttemptLimit limit) => limit switch
    {
        AttemptLimit.L7 => new OrderCountedRule(ProductConstants.UnpaidOrderCancellationsMax, TimeSpan.FromHours(ProductConstants.UnpaidOrderCancellationWindowHours)),
        AttemptLimit.L8 => new OrderCountedRule(ProductConstants.OpenUnpaidOrdersMax, null),
        _ => throw new ArgumentException($"Limit {limit} is not counted from the orders (02 8.2).", nameof(limit)),
    };

    /// <summary>The limit and axis pairs the table counts, in the order of the table.</summary>
    public static IReadOnlyCollection<(AttemptLimit Limit, AttemptAxis Axis)> Counted => _rules.Keys;

    /// <summary>The rule of a limit on an axis; an argument error when the limit does not count on that axis (the check constraint of the table says the same, 06 4.43).</summary>
    public static AttemptRule Of(AttemptLimit limit, AttemptAxis axis)
    {
        if (!_rules.TryGetValue((limit, axis), out AttemptRule rule))
        {
            throw new ArgumentException($"Limit {limit} is not counted on the {axis} axis by the attempt records (02 8.2; 06 4.43).", nameof(axis));
        }

        return rule;
    }

    private static AttemptRule Rule(int threshold, int windowMinutes) => new(threshold, TimeSpan.FromMinutes(windowMinutes));
}
