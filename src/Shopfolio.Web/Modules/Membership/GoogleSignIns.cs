using System.ComponentModel.DataAnnotations;
using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// For which addresses Google is the authority (02 3.13.7; K-1281): the address must end at <c>@gmail.com</c> exactly, or the account must carry a Google Workspace domain
/// (<c>hd</c>). Any other address's <c>email_verified</c> shows the verification at the moment the Google account was opened and the mailbox may have changed hands since.
/// </summary>
internal static class GoogleAddressAuthority
{
    private const string GoogleMailDomain = "gmail.com";

    public static bool IsAuthoritative(string email, string? hostedDomain)
    {
        ArgumentNullException.ThrowIfNull(email);
        if (!string.IsNullOrWhiteSpace(hostedDomain))
        {
            return true;
        }

        int at = email.LastIndexOf('@');
        return at > 0 && string.Equals(email[(at + 1)..].Trim(), GoogleMailDomain, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// The Google sign-in's account rules (02 3.13.6 to 3.13.8, 3.13.17, 3.13.19; T343): the core's decision over a verified Google identity, and the confirmation button of the
/// link that a non-authoritative address needs. The Google application is a conditional installation setting - the line's state is read from the one singleton
/// (<see cref="ConditionalLine"/>, D-81), never worked out again. Everything joins the caller's transaction and never saves; the account's row is locked first and the call
/// into the Notifications module is made under that lock, so two sign-ins of one customer make one confirmation link and one e-mail (D-234). A Google sign-in is not counted to L-1
/// (02 3.13.17). Each attempt but the closed line is written to the sign-in record (02 3.13.20).
/// </summary>
internal sealed class GoogleSignIns
{
    private static readonly InvalidEmailAttribute EmailRule = new();

    private readonly AppDbContext _db;
    private readonly ISettingsModule _settings;
    private readonly INotificationsModule _notifications;
    private readonly LoginRecorder _logins;
    private readonly LinkRecords _links;
    private readonly IReadOnlyList<ConditionalLine> _lines;
    private readonly TimeProvider _time;

    public GoogleSignIns(
        AppDbContext db,
        ISettingsModule settings,
        INotificationsModule notifications,
        LoginRecorder logins,
        LinkRecords links,
        IReadOnlyList<ConditionalLine> lines,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(logins);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _settings = settings;
        _notifications = notifications;
        _logins = logins;
        _links = links;
        _lines = lines;
        _time = time;
    }

    /// <summary>True when the installation has the Google application's client id and secret (02 3.13.6): the button shows and the flow runs; otherwise neither exists.</summary>
    public bool IsOpen => _lines.Any(line => line.Name == ConditionalLine.GoogleSignIn && line.IsOpen);

    public async Task<GoogleSignInResult> SignInAsync(GoogleIdentity identity, IPAddress? ipAddress, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(identity);
        RequireTransaction("The Google sign-in is decided only inside a transaction (09 6 item 3).");
        if (!IsOpen)
        {
            return GoogleSignInResult.Unavailable();
        }

        string email = (identity.Email ?? string.Empty).Trim();
        if (email.Length == 0 || EmailRule.GetValidationResult(email, new ValidationContext(email)) is not null)
        {
            // An empty address names no account, so there is nothing to record (the login record's address is required).
            if (email.Length > 0)
            {
                await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Failed, ct);
            }

            return GoogleSignInResult.Refused(email.Length == 0 ? ErrorCode.FieldRequired : ErrorCode.InvalidEmail);
        }

        // An address Google gives as not verified binds nothing and opens nothing (02 3.13.7; K-697).
        if (!identity.EmailVerified)
        {
            await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Failed, ct);
            return GoogleSignInResult.NotVerified();
        }

        // The account's row is locked before it is read, and stays locked to the end of the transaction: the session opens under it (09 15.1 item 3).
        string normalized = TextNormalizer.NormalizeIdentity(email);
        Guid? lockedId = await AccountLocks.LockByEmailAsync(_db, AccountType.Customer, normalized, ct);
        var existing = lockedId is { } id
            ? await _db.Customers.AsNoTracking().Where(c => c.Id == id).Select(c => new { c.Id, c.Email, c.EmailVerifiedAt, c.GoogleLinkedAt }).SingleOrDefaultAsync(ct)
            : null;

        if (existing is null || existing.EmailVerifiedAt is null)
        {
            return await OpenAccountAsync(identity, email, existing?.Id, ipAddress, ct);
        }

        if (existing.GoogleLinkedAt is not null)
        {
            await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Succeeded, ct);
            return GoogleSignInResult.Existing(existing.Id, linkMade: false);
        }

        if (GoogleAddressAuthority.IsAuthoritative(email, identity.HostedDomain))
        {
            DateTimeOffset now = _time.GetUtcNow();
            await _db.Customers.Where(c => c.Id == existing.Id && c.GoogleLinkedAt == null).ExecuteUpdateAsync(set => set.SetProperty(c => c.GoogleLinkedAt, now), ct);
            await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Succeeded, ct);
            return GoogleSignInResult.Existing(existing.Id, linkMade: true);
        }

        // Google is not the authority of the address and the account is not linked: no link, no session; the account's address gets a confirmation link (K-1281, K-1282).
        bool queued = await _notifications.QueueGoogleLinkConfirmationAsync(existing.Id, existing.Email, ct);
        await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Failed, ct);
        return GoogleSignInResult.NeedsConfirmation(existing.Id, queued);
    }

    /// <summary>
    /// The confirmation button of the Google link (02 3.13.7; 07 UC-53): in the caller's transaction, the account's row is locked, the link is consumed with one conditional update and
    /// the account is linked to Google; no session is opened. A link of another type, a used, superseded or expired one is a refusal that names its state and writes nothing.
    /// </summary>
    public async Task<GoogleLinkConfirmationResult> ConfirmGoogleLinkAsync(string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        RequireTransaction("The Google link is confirmed only inside a transaction (02 3.13.7; 09 6 item 3).");

        LinkReading reading = await _links.ReadAsync(key, ct);
        if (reading.Record is not { LinkType: LinkType.GoogleLinkConfirmation, CustomerId: Guid customerId })
        {
            return GoogleLinkConfirmationResult.LinkInvalid(VerificationLinkStatus.Missing);
        }

        // The account's row first, then the link's: the one lock order (05 6.1.2).
        if (!await AccountLocks.LockAsync(_db, AccountType.Customer, customerId, ct))
        {
            return GoogleLinkConfirmationResult.LinkInvalid(VerificationLinkStatus.Missing);
        }

        reading = await _links.ReadAsync(key, ct);
        if (reading.Record is not { LinkType: LinkType.GoogleLinkConfirmation, CustomerId: Guid } record || reading.State is not { } state)
        {
            return GoogleLinkConfirmationResult.LinkInvalid(VerificationLinkStatus.Missing);
        }

        if (state != LinkState.Valid)
        {
            return GoogleLinkConfirmationResult.LinkInvalid(MembershipModule.StatusOf(state));
        }

        if (!await _db.Customers.AsNoTracking().AnyAsync(c => c.Id == customerId && c.EmailVerifiedAt != null, ct))
        {
            return GoogleLinkConfirmationResult.LinkInvalid(VerificationLinkStatus.Missing);
        }

        // One conditional update: of two requests at once exactly one takes the link (07 2.5.3).
        if (!await _links.TryConsumeAsync(record.Id, ct))
        {
            LinkReading again = await _links.ReadAsync(key, ct);
            return GoogleLinkConfirmationResult.LinkInvalid(again.State is { } now ? MembershipModule.StatusOf(now) : VerificationLinkStatus.Missing);
        }

        DateTimeOffset moment = _time.GetUtcNow();
        await _db.Customers.Where(c => c.Id == customerId && c.GoogleLinkedAt == null).ExecuteUpdateAsync(set => set.SetProperty(c => c.GoogleLinkedAt, moment), ct);
        return GoogleLinkConfirmationResult.Confirmed(customerId);
    }

    private async Task<GoogleSignInResult> OpenAccountAsync(GoogleIdentity identity, string email, Guid? pendingId, IPAddress? ipAddress, CancellationToken ct)
    {
        // A new account is a data-collecting entry (02 3.13.19): a closed gate opens nothing and deletes nothing - a pending registration stays until its life ends.
        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        GateRefusal? refusal = gate.RefusalOf(DataCollectionEntry.NewAccountGoogleSignIn);
        if (refusal is not null)
        {
            await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Failed, ct);
            return GoogleSignInResult.GateClosed(refusal);
        }

        DateTimeOffset now = _time.GetUtcNow();
        bool replaced = false;
        if (pendingId is { } pending)
        {
            // The pending registration is not an account: its name and password go with it, its links and cart cascade (02 3.13.7; K-696).
            int deleted = await _db.Customers.Where(c => c.Id == pending && c.EmailVerifiedAt == null).ExecuteDeleteAsync(ct);
            if (deleted > 0)
            {
                _db.DisposalRecords.Add(DisposalRecord.Counted(DisposalRecordType.PendingRegistration, deleted, now));
                replaced = true;
            }
        }

        // The account is born passwordless, verified and linked, with Google's name or, with none, the part of the address before the at sign (02 3.13.21).
        string name = identity.Name?.Trim() ?? string.Empty;
        if (name.Length == 0)
        {
            name = email[..email.LastIndexOf('@')];
        }

        Customer customer = Customer.Create(name, email, passwordHash: null, emailVerifiedAt: now, googleLinkedAt: now, now);
        _db.Customers.Add(customer);
        await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Succeeded, ct);
        return GoogleSignInResult.Born(customer.Id, replaced);
    }

    private void RequireTransaction(string message)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(message);
        }
    }
}
