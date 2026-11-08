using System.Net;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The one rule of the password for both account types (02 3.13.9 to 3.13.17; 09 15.1, 15.4): the sign-in check with the limit L-1, the reset's request, reading and button, the
/// reauthentication and the change at the account. A customer's module and the administrator's module call the same methods with their own <see cref="IPasswordAccounts"/>, so the
/// administrator gets the same lifetimes, policy and limits and no second copy of the code exists (02 3.13.16). Writes join the caller's transaction and never save; the counters of
/// the limits are written in their own short transactions (K-972) and survive a refusal.
/// </summary>
internal sealed class PasswordRegime
{
    private readonly AttemptCounter _attempts;
    private readonly LinkRecords _links;
    private readonly LinkKeys _linkKeys;
    private readonly PasswordService _hashing;
    private readonly SessionStore _sessions;
    private readonly TimeProvider _time;

    public PasswordRegime(AttemptCounter attempts, LinkRecords links, LinkKeys linkKeys, PasswordService hashing, SessionStore sessions, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(attempts);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(linkKeys);
        ArgumentNullException.ThrowIfNull(hashing);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(time);
        _attempts = attempts;
        _links = links;
        _linkKeys = linkKeys;
        _hashing = hashing;
        _sessions = sessions;
        _time = time;
    }

    /// <summary>
    /// The sign-in's check (02 3.13.4, 3.13.17; L-1). Every attempt is counted before the work on the IP and, by the browser, on its recognized mark when the cookie's mark is valid for
    /// the account (its own counter, the address is not counted) or on the e-mail when it is not. When any count is over the threshold nothing is hashed and the attempt rows are
    /// forgotten, so the block does not lengthen itself; the account is never locked. A right password of a verified account forgets the rows and may renew the digest; a wrong one
    /// leaves them. The same hashing work is done whether or not the address has an account (05 6.1.4).
    /// </summary>
    public async Task<SignInVerdict> CheckPasswordAsync(IPasswordAccounts accounts, string email, string entered, IPAddress? ipAddress, string? presentedMark, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(entered);
        PasswordAccount? account = await accounts.FindByEmailAsync(TextNormalizer.NormalizeIdentity(email), ct);
        return await CheckAsync(accounts, account, email, entered, ipAddress, presentedMark, ct);
    }

    private async Task<SignInVerdict> CheckAsync(IPasswordAccounts accounts, PasswordAccount? account, string email, string entered, IPAddress? ipAddress, string? presentedMark, CancellationToken ct)
    {
        Guid? markId = account is null ? null : await _sessions.FindMarkAsync(accounts.Type, account.Id, presentedMark, ct);
        AttemptSubject[] subjects = [AttemptSubject.Ip(ipAddress), markId is { } known ? AttemptSubject.RecognizedBrowser(known) : AttemptSubject.Email(email)];

        List<Guid> rows = [];
        bool exceeded = false;
        foreach (AttemptSubject subject in subjects)
        {
            rows.Add(await _attempts.RecordAsync(AttemptLimit.L1, subject, ct));
        }

        foreach (AttemptSubject subject in subjects)
        {
            exceeded |= await _attempts.IsExceededAsync(AttemptLimit.L1, subject, ct);
        }

        if (exceeded)
        {
            await ForgetAllAsync(rows, ct);
            return new SignInVerdict(VerdictKind.Blocked, null);
        }

        HashCheck check = account?.PasswordHash is { } digest ? _hashing.Verify(digest, entered) : _hashing.VerifyNobody(entered);
        if (account is null || !check.Matched)
        {
            return new SignInVerdict(VerdictKind.Mismatch, null);
        }

        if (!account.IsVerified)
        {
            return new SignInVerdict(VerdictKind.NotEligible, null);
        }

        await ForgetAllAsync(rows, ct);
        if (check.NeedsRehash)
        {
            await accounts.SetPasswordAsync(account.Id, _hashing.Hash(entered), ct);
        }

        return new SignInVerdict(VerdictKind.Matched, account.Id);
    }

    /// <summary>
    /// A reset asked for (02 3.13.11, 3.13.17; L-2): counted on the e-mail and on the IP on every request, nothing forgotten; over the limit the request is refused and nothing else
    /// happens. An address without an account, or with an unverified one, gets the same success and no link; an eligible account gets a link that lives P-13 minutes and makes the
    /// earlier reset links invalid.
    /// </summary>
    public async Task<ResetRequested> RequestResetAsync(IPasswordAccounts accounts, string email, IPAddress? ipAddress, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(email);
        AttemptSubject byEmail = AttemptSubject.Email(email);
        AttemptSubject byIp = AttemptSubject.Ip(ipAddress);
        await _attempts.RecordAsync(AttemptLimit.L2, byEmail, ct);
        await _attempts.RecordAsync(AttemptLimit.L2, byIp, ct);
        if (await _attempts.IsExceededAsync(AttemptLimit.L2, byEmail, ct) || await _attempts.IsExceededAsync(AttemptLimit.L2, byIp, ct))
        {
            return ResetRequested.Limited();
        }

        PasswordAccount? account = await accounts.FindByEmailAsync(TextNormalizer.NormalizeIdentity(email), ct);
        if (account is null || !account.IsVerified)
        {
            return ResetRequested.NotIssued();
        }

        DateTimeOffset now = _time.GetUtcNow();
        DateTimeOffset expiresAt = now.AddMinutes(ProductConstants.PasswordResetLinkMinutes);
        LinkRecord link = accounts.Type == AccountType.Admin
            ? LinkRecord.ForAdmin(_linkKeys, LinkType.PasswordReset, account.Id, expiresAt, now)
            : LinkRecord.ForCustomer(_linkKeys, LinkType.PasswordReset, account.Id, expiresAt, now);
        _links.Add(link);
        await _links.SupersedeEarlierAsync(
            LinkType.PasswordReset,
            accounts.Type == AccountType.Customer ? account.Id : null,
            accounts.Type == AccountType.Admin ? account.Id : null,
            link.Id,
            ct);
        return ResetRequested.Issued(account.Id, link.Id, expiresAt);
    }

    /// <summary>What the reset page shows when the link opens (05 6.4.6): reads only. A key of another type or of the other account type is missing.</summary>
    public async Task<AccountLinkReading> ReadResetLinkAsync(AccountType type, string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        LinkReading reading = await _links.ReadAsync(key, ct);
        Guid? owner = OwnerOf(type, reading.Record);
        return owner is null || reading.State is not { } state ? new AccountLinkReading(AccountLinkStatus.Missing, null) : new AccountLinkReading(StatusOf(state), owner);
    }

    /// <summary>
    /// The reset's button (02 3.13.9, 3.13.10): the link is read; a policy refusal of the new password comes before the link is consumed, so the link stays and the person may try
    /// again; one conditional update then takes the link (of two requests at once exactly one wins), the password is written, every session of the account ends, every mark but the
    /// presented one ends, and the mark to write is given.
    /// </summary>
    public async Task<ResetOutcome> ResetPasswordAsync(IPasswordAccounts accounts, string key, string newPassword, string? presentedMark, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(key);
        LinkReading reading = await _links.ReadAsync(key, ct);
        if (OwnerOf(accounts.Type, reading.Record) is not { } accountId || reading.Record is null || reading.State is not { } state)
        {
            return ResetOutcome.Invalid(AccountLinkStatus.Missing);
        }

        if (state != LinkState.Valid)
        {
            return ResetOutcome.Invalid(StatusOf(state));
        }

        if (PasswordPolicy.Validate(newPassword) is { } refusal)
        {
            return ResetOutcome.Refused(refusal);
        }

        if (!await _links.TryConsumeAsync(reading.Record.Id, ct))
        {
            LinkReading again = await _links.ReadAsync(key, ct);
            return ResetOutcome.Invalid(again.State is { } now ? StatusOf(now) : AccountLinkStatus.Missing);
        }

        await accounts.SetPasswordAsync(accountId, _hashing.Hash(newPassword), ct);
        await _sessions.EndAllAsync(accounts.Type, accountId, null, ct);
        await _sessions.InvalidateMarksAsync(accounts.Type, accountId, presentedMark, ct);
        string mark = await _sessions.IssueMarkAsync(accounts.Type, accountId, presentedMark, ct);
        return ResetOutcome.Done(accountId, mark, SessionStore.MarkLifetime);
    }

    /// <summary>
    /// A reauthentication (02 3.13.13): the account's own password is checked against the same limit L-1 as the sign-in, so a wrong password counts; on success the session gets the
    /// permission for the one operation and the form's value is returned. An administrator has no account deletion. A session that is not the account's is refused.
    /// </summary>
    public async Task<ReauthenticationResult> ReauthenticateAsync(
        IPasswordAccounts accounts,
        Guid accountId,
        Guid sessionId,
        ReauthenticationOperation operation,
        string entered,
        IPAddress? ipAddress,
        string? presentedMark,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(entered);
        if (accounts.Type == AccountType.Admin && operation == ReauthenticationOperation.AccountDeletion)
        {
            return ReauthenticationResult.Refused(ErrorCode.ReauthenticationOperationInvalid);
        }

        PasswordAccount? account = await accounts.FindAsync(accountId, ct);
        if (account is null || !await _sessions.OwnsAsync(accounts.Type, accountId, sessionId, ct))
        {
            return ReauthenticationResult.Refused(ErrorCode.ReauthenticationFailed);
        }

        SignInVerdict check = await CheckAsync(accounts, account, account.Email, entered, ipAddress, presentedMark, ct);
        if (check.Outcome == VerdictKind.Blocked)
        {
            return ReauthenticationResult.Refused(ErrorCode.SignInLimitExceeded);
        }

        if (check.Outcome != VerdictKind.Matched)
        {
            return ReauthenticationResult.Refused(ErrorCode.ReauthenticationFailed);
        }

        string? value = await _sessions.GrantReauthenticationAsync(accounts.Type, accountId, sessionId, operation, ct);
        return value is null ? ReauthenticationResult.Refused(ErrorCode.ReauthenticationFailed) : ReauthenticationResult.Granted(value);
    }

    /// <summary>
    /// The password change at the account (02 3.13.10, 3.13.13): the policy first (a refusal consumes nothing), then the reauthentication value is taken (none: the form asks again),
    /// the password is written, the other sessions end and the session that changed it stays, and the other browsers' marks end.
    /// </summary>
    public async Task<ChangeOutcome> ChangePasswordAsync(
        IPasswordAccounts accounts,
        Guid accountId,
        Guid sessionId,
        string newPassword,
        string? reauthenticationValue,
        string? presentedMark,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        if (PasswordPolicy.Validate(newPassword) is { } refusal)
        {
            return ChangeOutcome.Refused(refusal);
        }

        if (!await _sessions.TryRedeemReauthenticationAsync(accounts.Type, accountId, sessionId, ReauthenticationOperation.PasswordChange, reauthenticationValue, ct))
        {
            return ChangeOutcome.Refused(ErrorCode.ReauthenticationRequired);
        }

        await accounts.SetPasswordAsync(accountId, _hashing.Hash(newPassword), ct);
        await _sessions.EndAllAsync(accounts.Type, accountId, sessionId, ct);
        await _sessions.InvalidateMarksAsync(accounts.Type, accountId, presentedMark, ct);
        string mark = await _sessions.IssueMarkAsync(accounts.Type, accountId, presentedMark, ct);
        return ChangeOutcome.Done(mark, SessionStore.MarkLifetime);
    }

    private async Task ForgetAllAsync(List<Guid> rows, CancellationToken ct)
    {
        foreach (Guid row in rows)
        {
            await _attempts.ForgetAsync(row, ct);
        }
    }

    // The owner of a password reset link when it is the type's: another link type or the other account type's owner is no link here.
    private static Guid? OwnerOf(AccountType type, LinkRecord? record)
    {
        if (record is not { LinkType: LinkType.PasswordReset })
        {
            return null;
        }

        return type == AccountType.Admin ? record.AdminId : record.CustomerId;
    }

    private static AccountLinkStatus StatusOf(LinkState state) => state switch
    {
        LinkState.Valid => AccountLinkStatus.Valid,
        LinkState.Used => AccountLinkStatus.Used,
        LinkState.Superseded => AccountLinkStatus.Superseded,
        LinkState.Expired => AccountLinkStatus.Expired,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Not a link state."),
    };
}
