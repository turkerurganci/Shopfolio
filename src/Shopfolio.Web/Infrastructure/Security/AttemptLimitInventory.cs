using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>Where a limit's count comes from (05 6.3.1, 6.3.2).</summary>
internal enum AttemptCounting
{
    /// <summary>The attempt record, one row per failed attempt: the row is forgotten when the work succeeds (L-1, L-5, L-6).</summary>
    OnFailure,

    /// <summary>The attempt record, one row per request, never forgotten (L-2, L-3, L-4, L-9).</summary>
    EveryRequest,

    /// <summary>The orders themselves; the attempt record has no row for the limit (L-7, L-8).</summary>
    FromOrders,
}

/// <summary>One row of the limit inventory (02 8.2): the limit, its name in code, how it is counted, the axes it counts on, and the rows of the parameter table that carry its numbers.</summary>
internal sealed record AttemptLimitEntry(AttemptLimit Limit, string Name, AttemptCounting Counting, IReadOnlyList<AttemptAxis> Axes, IReadOnlyList<string> ParameterIds);

/// <summary>
/// The nine attempt limits of 02 8.2 in one inventory (T376 criterion 1, 9; 09 15.4): their names, their counting kind, their axes and the parameter rows that
/// hold their numbers (02 11.2 P-31 to P-37, P-46, P-49). The inventory holds no threshold and no window - those are the product constants, read through
/// <see cref="AttemptLimits"/> (the attempt record) or <see cref="AttemptLimits.OrderCountedOf"/> (the orders) - so a number is written once. A consistency
/// test keeps the inventory equal to the table of the document; a limit that is added or removed there is added or removed here, or that test fails.
/// </summary>
internal static class AttemptLimitInventory
{
    private static readonly AttemptAxis[] _emailAndIp = [AttemptAxis.Email, AttemptAxis.Ip];

    /// <summary>The nine limits in the order of the table (L-1 to L-9).</summary>
    public static IReadOnlyList<AttemptLimitEntry> All { get; } =
    [
        new(AttemptLimit.L1, "SignIn", AttemptCounting.OnFailure, [AttemptAxis.Email, AttemptAxis.Ip, AttemptAxis.RecognizedBrowser], ["P-31"]),
        new(AttemptLimit.L2, "PasswordReset", AttemptCounting.EveryRequest, _emailAndIp, ["P-32"]),
        new(AttemptLimit.L3, "Registration", AttemptCounting.EveryRequest, _emailAndIp, ["P-33"]),
        new(AttemptLimit.L4, "ContactForm", AttemptCounting.EveryRequest, [AttemptAxis.Ip], ["P-34"]),
        new(AttemptLimit.L5, "GuestOrderLookup", AttemptCounting.OnFailure, _emailAndIp, ["P-35"]),
        new(AttemptLimit.L6, "InvalidCoupon", AttemptCounting.OnFailure, [AttemptAxis.Cart], ["P-36"]),
        new(AttemptLimit.L7, "UnpaidOrderCancellation", AttemptCounting.FromOrders, _emailAndIp, ["P-37"]),
        new(AttemptLimit.L8, "OpenUnpaidOrder", AttemptCounting.FromOrders, _emailAndIp, ["P-46"]),
        new(AttemptLimit.L9, "VerificationResend", AttemptCounting.EveryRequest, _emailAndIp, ["P-49"]),
    ];

    /// <summary>The entry of a limit.</summary>
    public static AttemptLimitEntry Of(AttemptLimit limit) => All.Single(e => e.Limit == limit);
}
