using System.Net;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>
/// The open surface of the Administration module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns the operation trail and the admin accounts (05 3.3.9).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3). The administrator's sign-in, session, reset, reauthentication and password change are the customer's code with the administrator's account
/// (T342; 02 3.13.16): the same lifetimes, the same policy, the same limits; there is no Google sign-in, no account deletion and no two-step verification.
/// </summary>
public interface IAdministrationModule
{
    /// <summary>The panel's credential check: the limit L-1, the hashing work and the sign-in record, as the customer's; an administrator account has no verified state.</summary>
    Task<AdminCredentialCheckResult> CheckCredentialsAsync(string email, string password, IPAddress? ipAddress, string? recognizedBrowserMark, CancellationToken ct);

    /// <summary>Opens the administrator's session: the same lifetimes as the customer's (P-15 hours, P-16 days) and the same mark. It runs in the credential check's transaction, under the account row lock the check took (09 15.1 item 3).</summary>
    Task<SessionOpening> SignInAsync(Guid adminId, bool rememberMe, string? presentedMark, CancellationToken ct);

    /// <summary>Ends one session of the panel (the sign-out).</summary>
    Task<bool> EndSessionAsync(Guid sessionId, CancellationToken ct);

    /// <summary>The reset's request (L-2): the same neutral success as the customer's.</summary>
    Task<ResetRequested> RequestPasswordResetAsync(string email, IPAddress? ipAddress, CancellationToken ct);

    /// <summary>What the panel's reset page shows when the link opens: only reads.</summary>
    Task<AccountLinkReading> ReadPasswordResetLinkAsync(string key, CancellationToken ct);

    /// <summary>The panel's reset button: the same policy, the same one-use link, every session of the administrator ends.</summary>
    Task<ResetOutcome> ResetPasswordAsync(string key, string newPassword, string? recognizedBrowserMark, CancellationToken ct);

    /// <summary>The administrator's reauthentication for the password change; the account deletion is refused (<c>ReauthenticationOperationInvalid</c>).</summary>
    Task<ReauthenticationResult> ReauthenticateAsync(Guid adminId, Guid sessionId, ReauthenticationOperation operation, string password, IPAddress? ipAddress, string? recognizedBrowserMark, CancellationToken ct);

    /// <summary>The administrator's password change: the policy, the reauthentication value, the other sessions and marks end.</summary>
    Task<ChangeOutcome> ChangePasswordAsync(Guid adminId, Guid sessionId, string newPassword, string? reauthenticationValue, string? recognizedBrowserMark, CancellationToken ct);

    /// <summary>
    /// Opens an administrator account for the management command (T303; 06 8.3.1; 05 8.5.2): the name, the e-mail and the password pass the same rules as every other account (the
    /// e-mail form rule and the password policy of 02 3.13.15), the account is born verified and with no role (06 8.3.2), and the id is the one the caller gave, so the command can
    /// write its line to its own log before the work (05 8.5.7). It writes no operation trail line and makes no notification (02 10.2.7). Joins the caller's transaction.
    /// </summary>
    Task<AdminCreationResult> CreateAdminAsync(Guid adminId, string name, string email, string password, CancellationToken ct);
}
