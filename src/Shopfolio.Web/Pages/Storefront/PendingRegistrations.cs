using System.Net;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// What the two pages that show the pending-registration state share (T514, T515; 07 UC-47, UC-49, UC-55; 04 5.20.4, 5.20.7, 5.22.8): the protected value that names a pending registration and the
/// resend of its verification link. The registration screen (<c>/kayit</c>) and the sign-in screen (<c>/giris</c>) call the same code, so the resend is written once (D-1249). The value is
/// encrypted and signed, is not stored, and decides nothing but which account to read again (K-1186 (2)).
/// </summary>
internal static class PendingRegistrations
{
    /// <summary>The name of the address parameter and of the form field that carry the protected value (07 5.3.2).</summary>
    public const string Parameter = "pendingRegistration";

    /// <summary>The protected value of a pending registration's account, as the 303 carries it and the resend form sends it back.</summary>
    public static string Protect(ProtectedValues values, Guid customerId)
    {
        ArgumentNullException.ThrowIfNull(values);
        return values.Protect(DataProtectionPurposes.PendingRegistration, customerId.ToString("N"));
    }

    /// <summary>The account a protected value names; false when the value is damaged, foreign or empty.</summary>
    public static bool TryResolve(ProtectedValues values, string? value, out Guid customerId)
    {
        ArgumentNullException.ThrowIfNull(values);
        customerId = Guid.Empty;
        ProtectedValueResult unprotected = values.Unprotect(DataProtectionPurposes.PendingRegistration, value);
        return unprotected.IsValid && Guid.TryParseExact(unprotected.Payload, "N", out customerId);
    }

    /// <summary>
    /// The resend (07 UC-49): reads the pending registration, asks the module for a new link and queues the e-mail in the same transaction. A registration that is no longer pending sends nothing and counts
    /// nothing (04 5.20.7).
    /// </summary>
    public static Task<ResendOutcome> ResendAsync(
        IMembershipModule membership, INotificationsModule notifications, ITransactionRunner transactions, Guid customerId, IPAddress? ip, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(transactions);
        return transactions.RunAsync(
            async token =>
            {
                PendingRegistrationReading? pending = await membership.ReadPendingRegistrationAsync(customerId, token);
                if (pending is null)
                {
                    return ResendOutcome.Unresolved;
                }

                VerificationResendResult result = await membership.ResendVerificationLinkAsync(customerId, ip, token);
                if (result.IsIssued)
                {
                    await notifications.QueueAccountEmailAsync(new VerificationLinkEmail(customerId, result.LinkId.GetValueOrDefault(), pending.Email), token);
                }

                return new ResendOutcome(result, pending);
            },
            ct);
    }
}

/// <summary>The outcome of a resend: the module's answer, or none when the value named no pending registration.</summary>
internal sealed record ResendOutcome(VerificationResendResult? Result, PendingRegistrationReading? Pending) : IOperationResult
{
    public static ResendOutcome Unresolved { get; } = new(null, null);

    public bool IsRejected => Result is null || Result.IsRejected;

    /// <summary>True when the link was limited by L-9 (07 UC-49): the pending state stays and the neutral sentence stands beside the button.</summary>
    public bool IsLimited => Result is { Code: ErrorCode.VerificationLinkLimitExceeded };
}
