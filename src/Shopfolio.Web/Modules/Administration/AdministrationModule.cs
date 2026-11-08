using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The Administration module's implementation of its open surface; internal, reached only through <see cref="IAdministrationModule"/>. The password, the reset, the reauthentication
/// and the session are the one rule of <see cref="PasswordRegime"/> and <see cref="SessionStore"/> with the administrator's adapter (02 3.13.16); this class only joins them to the
/// panel's sign-in record. Every writing method joins the caller's transaction and does not save (09 6 item 3).
/// </summary>
internal sealed class AdministrationModule : IAdministrationModule
{
    private readonly AppDbContext _db;
    private readonly PasswordRegime _passwords;
    private readonly AdminPasswords _accounts;
    private readonly SessionStore _sessions;
    private readonly LoginRecorder _logins;

    public AdministrationModule(AppDbContext db, PasswordRegime passwords, AdminPasswords accounts, SessionStore sessions, LoginRecorder logins)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(passwords);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(logins);
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
        services.AddScoped<IAuditActorSource, RequestAuditActorSource>();
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddDisposalPart<AdminInvitationDisposalPart>();
        return services;
    }
}
