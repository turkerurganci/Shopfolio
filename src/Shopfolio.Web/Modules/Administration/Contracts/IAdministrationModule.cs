using System.Net;

using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration.Contracts.Results;

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

    /// <summary>
    /// The internal id of the administrator account that has the e-mail (T389; 05 8.5.3, 8.5.4; 09 13.2 item 5), found the way the sign-in finds it (the normalized form); null when there is
    /// none. A plain read: the management commands that name an account by e-mail look the id up first, write their log line with the id, and only then work on the id.
    /// </summary>
    Task<Guid?> FindAdminIdAsync(string email, CancellationToken ct);

    /// <summary>
    /// Ends every session of the administrator account (T389; 05 8.5.3): takes the account's row lock first, like every flow that writes the account's sessions (09 15.1 item 3), and
    /// deletes all the session records. Refused when the account is gone. It writes no operation trail line and makes no notification (05 8.5.7). Joins the caller's transaction.
    /// </summary>
    Task<AdminAccountWorkResult> EndAdminSessionsAsync(Guid adminId, CancellationToken ct);

    /// <summary>
    /// Makes the administrator's password unusable and ends every session of the account in the same transaction (T389; 05 8.5.4; 06 4.37): the account's row lock first, then the digest is
    /// emptied, then the sessions are deleted - so a sign-in that checked the old digest is either committed before and deleted here, or waits and fails. The account opens again only through the
    /// password reset. Refused when the account is gone. It writes no operation trail line and makes no notification (05 8.5.7). Joins the caller's transaction.
    /// </summary>
    Task<AdminAccountWorkResult> InvalidateAdminPasswordAsync(Guid adminId, CancellationToken ct);

    /// <summary>Sends an invitation to an address (T379; 07 UC-175; 02 10.2.1, 10.2.6): the earlier unused invitation of the address is made invalid, the e-mail, the trail row and the F-6 notice are written.</summary>
    Task<AdminInvitationSendResult> SendInvitationAsync(string email, CancellationToken ct);

    /// <summary>Withdraws an unused invitation (07 UC-176; 02 10.2.6): the link ends at once; nobody is notified.</summary>
    Task<AdminInvitationWithdrawalResult> WithdrawInvitationAsync(Guid invitationId, CancellationToken ct);

    /// <summary>What the invitation page shows when its link opens (07 UC-112): only reads.</summary>
    Task<AdminInvitationReading> ReadInvitationLinkAsync(string key, CancellationToken ct);

    /// <summary>The invitee's button (07 UC-113): the name and the password pass, the link is consumed and the account is born.</summary>
    Task<AdminInvitationAcceptance> AcceptInvitationAsync(string key, string name, string password, CancellationToken ct);

    /// <summary>Removes another administrator (07 UC-177; 02 10.2.2); without the confirmation it only tells what the removal does.</summary>
    Task<AdminRemovalResult> RemoveAdminAsync(Guid targetAdminId, bool confirmed, CancellationToken ct);

    /// <summary>
    /// The administrator's removal applied again after the database was returned to an earlier moment (T388; 05 8.5.10), for the <c>reapply-deletions</c> command, in the caller's transaction,
    /// which saves: the same removal as <see cref="RemoveAdminAsync"/> (the unused invitations the administrator sent are ended, the account is deleted and the disposal record written) with
    /// none of its gates - no acting administrator, no self or last-administrator refusal - and no trail row or notice: both were written the first time. An account that is not there already is skipped.
    /// </summary>
    Task<DeletionReapplyResult> ReapplyAdminRemovalAsync(Guid adminId, CancellationToken ct);

    /// <summary>The administrator's name for the panel frame's top row (T540; 07 5.5.1; 04 2.2.8); null when the account is gone. A plain read.</summary>
    Task<string?> GetAdminNameAsync(Guid adminId, CancellationToken ct);

    /// <summary>The administrator's own name (07 UC-178; 02 10.2.4): no reauthentication.</summary>
    Task<AdminNameChangeResult> ChangeNameAsync(Guid adminId, string name, CancellationToken ct);

    /// <summary>The administrator's request to change the e-mail address (07 UC-179; 02 10.2.4), after the reauthentication.</summary>
    Task<AdminEmailChangeRequestResult> RequestEmailChangeAsync(Guid adminId, Guid sessionId, string newEmail, string? reauthenticationValue, IPAddress? ipAddress, CancellationToken ct);

    /// <summary>Asks the pending change's link again (07 UC-180); L-9 counts it.</summary>
    Task<AdminEmailChangeResendResult> ResendEmailChangeLinkAsync(Guid adminId, IPAddress? ipAddress, CancellationToken ct);

    /// <summary>The new address's button (07 UC-186): the address changes, the old one is told with a revert link, the trail row is written.</summary>
    Task<AdminEmailVerificationResult> VerifyNewEmailAsync(string key, CancellationToken ct);

    /// <summary>What the new address's landing page shows when its link opens (07 UC-108): only reads; opening changes nothing (05 6.4.7; K-904).</summary>
    Task<AdminLinkReading> ReadNewEmailLinkAsync(string key, CancellationToken ct);

    /// <summary>What the revert link's landing page shows when it opens (07 UC-108): only reads.</summary>
    Task<AdminLinkReading> ReadEmailRevertLinkAsync(string key, CancellationToken ct);

    /// <summary>
    /// The revert link's button (07 UC-187; 02 10.2.4; K-1210): the address goes back, every session ends, the marks and the password become invalid, the earlier address gets the password
    /// reset link, the trail row is written. The earlier address being another administrator's refuses (422) and the caller rolls back.
    /// </summary>
    Task<AdminEmailRevertResult> RevertPendingEmailChangeAsync(string key, CancellationToken ct);

    /// <summary>Drops the earlier permission of a session when the re-verification page opens (07 UC-108; 05 6.1.5): the new form is the one that counts. Joins the caller's transaction.</summary>
    Task DropReauthenticationAsync(Guid adminId, Guid sessionId, CancellationToken ct);
}
