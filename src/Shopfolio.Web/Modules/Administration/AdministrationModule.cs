using System.ComponentModel.DataAnnotations;
using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Administration.Contracts.Results;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The Administration module's implementation of its open surface; internal, reached only through <see cref="IAdministrationModule"/>. The password, the reset, the reauthentication
/// and the session are the one rule of <see cref="PasswordRegime"/> and <see cref="SessionStore"/> with the administrator's adapter (02 3.13.16); this class only joins them to the
/// panel's sign-in record. Every writing method joins the caller's transaction and does not save (09 6 item 3).
/// </summary>
internal sealed class AdministrationModule : IAdministrationModule
{
    // The one home of the e-mail rule (04 7.3.2.1; 09 3.4 item 1), the customer registration's rule too.
    private static readonly InvalidEmailAttribute _emailRule = new();

    private readonly AppDbContext _db;
    private readonly PasswordRegime _passwords;
    private readonly AdminPasswords _accounts;
    private readonly SessionStore _sessions;
    private readonly LoginRecorder _logins;
    private readonly PasswordService _hashing;
    private readonly TimeProvider _time;

    private readonly AdminAccounts _admins;
    private readonly AdminEmailChanges _emailChanges;
    private readonly AdvisoryLocks _locks;

    public AdministrationModule(
        AppDbContext db,
        PasswordRegime passwords,
        AdminPasswords accounts,
        SessionStore sessions,
        LoginRecorder logins,
        PasswordService hashing,
        TimeProvider time,
        AdminAccounts admins,
        AdminEmailChanges emailChanges,
        AdvisoryLocks locks)
    {
        ArgumentNullException.ThrowIfNull(admins);
        ArgumentNullException.ThrowIfNull(emailChanges);
        ArgumentNullException.ThrowIfNull(locks);
        _admins = admins;
        _emailChanges = emailChanges;
        _locks = locks;
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(passwords);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(logins);
        ArgumentNullException.ThrowIfNull(hashing);
        ArgumentNullException.ThrowIfNull(time);
        _hashing = hashing;
        _time = time;
        _db = db;
        _passwords = passwords;
        _accounts = accounts;
        _sessions = sessions;
        _logins = logins;
    }

    public async Task<AdminCredentialCheckResult> CheckCredentialsAsync(string email, string password, IPAddress? ipAddress, string? recognizedBrowserMark, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);
        RequireTransaction("The sign-in check is made only inside a transaction (09 6 item 3).");

        SignInVerdict check = await _passwords.CheckPasswordAsync(_accounts, email, password, ipAddress, recognizedBrowserMark, ct);
        if (check.Outcome != VerdictKind.Matched || check.AccountId is not { } adminId)
        {
            await _logins.RecordAsync(ipAddress, AccountType.Admin, email, LoginOutcome.Failed, ct);
            return AdminCredentialCheckResult.Refused(check.Outcome == VerdictKind.Blocked ? ErrorCode.SignInLimitExceeded : ErrorCode.InvalidCredentials);
        }

        await _logins.RecordAsync(ipAddress, AccountType.Admin, email, LoginOutcome.Succeeded, ct);
        return AdminCredentialCheckResult.Granted(adminId);
    }

    public Task<SessionOpening> SignInAsync(Guid adminId, bool rememberMe, string? presentedMark, CancellationToken ct)
    {
        RequireTransaction("The session is written only inside a transaction (09 6 item 3).");
        return _sessions.OpenAsync(AccountType.Admin, adminId, rememberMe, presentedMark, ct);
    }

    public Task<bool> EndSessionAsync(Guid sessionId, CancellationToken ct) => _sessions.EndAsync(sessionId, ct);

    public Task<ResetRequested> RequestPasswordResetAsync(string email, IPAddress? ipAddress, CancellationToken ct)
    {
        RequireTransaction("The reset link is written only inside a transaction (09 6 item 3).");
        return _passwords.RequestResetAsync(_accounts, email, ipAddress, ct);
    }

    public Task<AccountLinkReading> ReadPasswordResetLinkAsync(string key, CancellationToken ct) => _passwords.ReadResetLinkAsync(AccountType.Admin, key, ct);

    public Task<ResetOutcome> ResetPasswordAsync(string key, string newPassword, string? recognizedBrowserMark, CancellationToken ct)
    {
        RequireTransaction("The reset is written only inside a transaction (09 6 item 3).");
        return _passwords.ResetPasswordAsync(_accounts, key, newPassword, recognizedBrowserMark, ct);
    }

    public Task<ReauthenticationResult> ReauthenticateAsync(Guid adminId, Guid sessionId, ReauthenticationOperation operation, string password, IPAddress? ipAddress, string? recognizedBrowserMark, CancellationToken ct)
    {
        RequireTransaction("The reauthentication is written only inside a transaction (09 6 item 3).");
        return _passwords.ReauthenticateAsync(_accounts, adminId, sessionId, operation, password, ipAddress, recognizedBrowserMark, ct);
    }

    public Task<ChangeOutcome> ChangePasswordAsync(Guid adminId, Guid sessionId, string newPassword, string? reauthenticationValue, string? recognizedBrowserMark, CancellationToken ct)
    {
        RequireTransaction("The password is written only inside a transaction (09 6 item 3).");
        return _passwords.ChangePasswordAsync(_accounts, adminId, sessionId, newPassword, reauthenticationValue, recognizedBrowserMark, ct);
    }

    public async Task<AdminCreationResult> CreateAdminAsync(Guid adminId, string name, string email, string password, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);
        RequireTransaction("The administrator is written only inside a transaction (09 6 item 3).");

        // The same rules as the customer's registration: the e-mail form rule and the one password policy (02 3.13.15, 3.13.16); no value is returned with a refusal.
        string cleanName = name.Trim();
        string cleanEmail = email.Trim();
        if (cleanName.Length == 0)
        {
            return AdminCreationResult.Refused(AdminCreationFields.Name, ErrorCode.FieldRequired);
        }

        if (cleanEmail.Length == 0)
        {
            return AdminCreationResult.Refused(AdminCreationFields.Email, ErrorCode.FieldRequired);
        }

        if (_emailRule.GetValidationResult(cleanEmail, new ValidationContext(cleanEmail)) is not null)
        {
            return AdminCreationResult.Refused(AdminCreationFields.Email, ErrorCode.InvalidEmail);
        }

        if (PasswordPolicy.Validate(password) is { } passwordRefusal)
        {
            return AdminCreationResult.Refused(AdminCreationFields.Password, passwordRefusal);
        }

        // The administrator set's lock is alone and first (05 7.3.5): the command and an invitation's acceptance open accounts one after the other.
        await _locks.LockAsync(AdvisoryLockScope.AdminSet, null, ct);
        string normalized = TextNormalizer.NormalizeIdentity(cleanEmail);
        if (await _db.Admins.AsNoTracking().AnyAsync(a => a.NormalizedEmail == normalized, ct))
        {
            return AdminCreationResult.Refused(AdminCreationFields.Email, ErrorCode.EmailAlreadyRegistered);
        }

        // An administrator account has no verified state and no role (06 4.37, 8.3.1, 8.3.2): the row is the whole account.
        Admin admin = Admin.Create(cleanName, cleanEmail, _hashing.Hash(password), _time.GetUtcNow());
        admin.Id = adminId;
        _db.Admins.Add(admin);
        return AdminCreationResult.Created(adminId);
    }

    public async Task<Guid?> FindAdminIdAsync(string email, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(email);
        string cleanEmail = email.Trim();
        if (cleanEmail.Length == 0)
        {
            return null;
        }

        PasswordAccount? account = await _accounts.FindByEmailAsync(TextNormalizer.NormalizeIdentity(cleanEmail), ct);
        return account?.Id;
    }

    public async Task<AdminAccountWorkResult> EndAdminSessionsAsync(Guid adminId, CancellationToken ct)
    {
        RequireTransaction("The sessions are deleted only inside a transaction (09 6 item 3).");

        // The account's row lock comes first, before the session rows, like every flow of the account (09 15.1 item 3).
        if (!await _accounts.LockAsync(adminId, ct))
        {
            return AdminAccountWorkResult.AccountMissing();
        }

        await _sessions.EndAllAsync(AccountType.Admin, adminId, null, ct);
        return AdminAccountWorkResult.Done();
    }

    public async Task<AdminAccountWorkResult> InvalidateAdminPasswordAsync(Guid adminId, CancellationToken ct)
    {
        RequireTransaction("The password is invalidated only inside a transaction (09 6 item 3).");

        // The same order as the reset (09 15.1 item 3): the account's row lock, the digest, then the sessions.
        if (!await _accounts.LockAsync(adminId, ct))
        {
            return AdminAccountWorkResult.AccountMissing();
        }

        await _accounts.InvalidatePasswordAsync(adminId, ct);
        await _sessions.EndAllAsync(AccountType.Admin, adminId, null, ct);
        return AdminAccountWorkResult.Done();
    }

    public Task<AdminInvitationSendResult> SendInvitationAsync(string email, CancellationToken ct) => _admins.SendAsync(email, ct);

    public Task<AdminInvitationWithdrawalResult> WithdrawInvitationAsync(Guid invitationId, CancellationToken ct) => _admins.WithdrawAsync(invitationId, ct);

    public Task<AdminInvitationReading> ReadInvitationLinkAsync(string key, CancellationToken ct) => _admins.ReadAsync(key, ct);

    public Task<AdminInvitationAcceptance> AcceptInvitationAsync(string key, string name, string password, CancellationToken ct) => _admins.AcceptInvitationAsync(key, name, password, ct);

    public Task<AdminRemovalResult> RemoveAdminAsync(Guid targetAdminId, bool confirmed, CancellationToken ct) => _admins.RemoveAsync(targetAdminId, confirmed, ct);

    public Task<DeletionReapplyResult> ReapplyAdminRemovalAsync(Guid adminId, CancellationToken ct) => _admins.ReapplyRemovalAsync(adminId, ct);

    public async Task<string?> GetAdminNameAsync(Guid adminId, CancellationToken ct) =>
        await _db.Admins.AsNoTracking().Where(a => a.Id == adminId).Select(a => a.Name).SingleOrDefaultAsync(ct);

    public Task<AdminNameChangeResult> ChangeNameAsync(Guid adminId, string name, CancellationToken ct) => _admins.ChangeNameAsync(adminId, name, ct);

    public Task<AdminEmailChangeRequestResult> RequestEmailChangeAsync(Guid adminId, Guid sessionId, string newEmail, string? reauthenticationValue, IPAddress? ipAddress, CancellationToken ct) =>
        _emailChanges.RequestAsync(adminId, sessionId, newEmail, reauthenticationValue, ipAddress, ct);

    public Task<AdminEmailChangeResendResult> ResendEmailChangeLinkAsync(Guid adminId, IPAddress? ipAddress, CancellationToken ct) => _emailChanges.ResendAsync(adminId, ipAddress, ct);

    public Task<AdminEmailVerificationResult> VerifyNewEmailAsync(string key, CancellationToken ct) => _emailChanges.VerifyNewEmailAsync(key, ct);

    public Task<AdminLinkReading> ReadNewEmailLinkAsync(string key, CancellationToken ct) => _emailChanges.ReadNewEmailLinkAsync(key, ct);

    public Task<AdminLinkReading> ReadEmailRevertLinkAsync(string key, CancellationToken ct) => _emailChanges.ReadRevertLinkAsync(key, ct);

    public Task<AdminEmailRevertResult> RevertPendingEmailChangeAsync(string key, CancellationToken ct) => _emailChanges.RevertPendingEmailChangeAsync(key, ct);

    public Task DropReauthenticationAsync(Guid adminId, Guid sessionId, CancellationToken ct)
    {
        RequireTransaction("The permission is dropped only inside a transaction (09 6 item 3).");
        return _sessions.DropReauthenticationAsync(AccountType.Admin, adminId, sessionId, ct);
    }

    private void RequireTransaction(string message)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(message);
        }
    }
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddAdministrationModule</c> once.</summary>
internal static class AdministrationModuleRegistration
{
    public static IServiceCollection AddAdministrationModule(this IServiceCollection services)
    {
        services.AddScoped<IAdministrationModule, AdministrationModule>();
        services.AddScoped<AdminPasswords>();
        // The acting administrator is read from the request; in the command host there is no request and the accessor answers with none (the commands write no trail row, 02 10.3.3).
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditActorSource, RequestAuditActorSource>();
        services.AddScoped<AdminAccounts>();
        services.AddScoped<AdminEmailChanges>();
        services.AddScoped<IAdminNotices, AdminNotices>();
        services.AddScoped<AuditTrail>();
        services.AddScoped<IAuditTrail>(p => p.GetRequiredService<AuditTrail>());
        services.AddScoped<ISalesSummary, SalesSummaryService>();
        services.AddScoped<IPanelHome, PanelHomeService>();
        services.AddScoped<IPanelHomeDetails, PanelHomeDetailsService>();
        services.AddDisposalPart<AdminInvitationDisposalPart>();
        services.TryAddSingleton<Shopfolio.Infrastructure.ExportTempDirectory>();
        services.AddScoped<IDataExport, DataExport>();
        services.AddHostedService<Shopfolio.Infrastructure.ExportDirectoryStartup>();
        return services;
    }
}
