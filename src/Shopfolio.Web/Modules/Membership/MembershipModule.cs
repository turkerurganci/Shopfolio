using System.ComponentModel.DataAnnotations;
using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// The Membership module's implementation of its open surface; internal, reached only through <see cref="IMembershipModule"/>. Every method that writes joins the caller's
/// transaction and never opens one (09 6 item 3); the counters of the attempt limits and the sign-in record are the two writes that are not part of it - they are written in
/// transactions of their own so a refusal does not take them back (K-972).
/// </summary>
internal sealed class MembershipModule : IMembershipModule
{
    // The one home of the e-mail rule (04 7.3.2.1; 09 3.4 item 1): the form of the guest order's address and of the registration's is one rule.
    private static readonly InvalidEmailAttribute EmailRule = new();

    private readonly AppDbContext _db;
    private readonly ISettingsModule _settings;
    private readonly IOrderingModule _ordering;
    private readonly AttemptCounter _attempts;
    private readonly LinkRecords _links;
    private readonly LinkKeys _linkKeys;
    private readonly LoginRecorder _logins;
    private readonly PasswordService _hashing;
    private readonly PasswordRegime _passwords;
    private readonly CustomerPasswords _accounts;
    private readonly TimeProvider _time;
    private readonly AddressBooks _addresses;
    private readonly GoogleSignIns _google;
    private readonly EmailChanges _emailChanges;
    private readonly AccountDeletions _deletions;
    private readonly MemberRecords _memberRecords;
    private readonly GoogleReauthentications _googleReauthentications;

    public MembershipModule(
        AppDbContext db,
        ISettingsModule settings,
        IOrderingModule ordering,
        AttemptCounter attempts,
        LinkRecords links,
        LinkKeys linkKeys,
        LoginRecorder logins,
        PasswordService hashing,
        PasswordRegime passwords,
        CustomerPasswords accounts,
        TimeProvider time,
        AddressBooks addresses,
        GoogleSignIns google,
        EmailChanges emailChanges,
        AccountDeletions deletions,
        MemberRecords memberRecords,
        GoogleReauthentications googleReauthentications)
    {
        _googleReauthentications = googleReauthentications;
        _deletions = deletions;
        _memberRecords = memberRecords;
        _google = google;
        _emailChanges = emailChanges;
        _passwords = passwords;
        _accounts = accounts;
        _addresses = addresses;
        _db = db;
        _settings = settings;
        _ordering = ordering;
        _attempts = attempts;
        _links = links;
        _linkKeys = linkKeys;
        _logins = logins;
        _hashing = hashing;
        _time = time;
    }

    public async Task<RegistrationResult> RegisterAsync(RegisterInput input, IPAddress? ipAddress, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        RequireTransaction("The registration is written only inside a transaction (09 6 item 3).");

        // The door of the data-collecting entries comes first (02 3.13.19): a closed gate counts no attempt and reads no field.
        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        GateRefusal? refusal = gate.RefusalOf(DataCollectionEntry.AccountRegistration);
        if (refusal is not null)
        {
            return RegistrationResult.GateClosed(refusal);
        }

        string name = (input.Name ?? string.Empty).Trim();
        string email = (input.Email ?? string.Empty).Trim();
        string entered = input.Password ?? string.Empty;
        List<MembershipFieldError> fields = [];
        if (name.Length == 0)
        {
            fields.Add(new MembershipFieldError(nameof(RegisterInput.Name), ErrorCode.FieldRequired));
        }

        if (email.Length == 0)
        {
            fields.Add(new MembershipFieldError(nameof(RegisterInput.Email), ErrorCode.FieldRequired));
        }
        else if (EmailRule.GetValidationResult(email, new ValidationContext(email)) is not null)
        {
            fields.Add(new MembershipFieldError(nameof(RegisterInput.Email), ErrorCode.InvalidEmail));
        }

        // The one home of the password policy (02 3.13.15; D-251): a refusal is a field error and, like every field error, comes before L-3 and is not counted.
        if (PasswordPolicy.Validate(entered) is { } passwordRefusal)
        {
            fields.Add(new MembershipFieldError(nameof(RegisterInput.Password), passwordRefusal));
        }

        if (fields.Count > 0)
        {
            return RegistrationResult.FieldsRefused(fields);
        }

        // L-3 is counted on every request, before the work, on the e-mail and on the IP (02 8.2; 09 15.4 item 6).
        AttemptSubject byEmail = AttemptSubject.Email(email);
        AttemptSubject byIp = AttemptSubject.Ip(ipAddress);
        await _attempts.RecordAsync(AttemptLimit.L3, byEmail, ct);
        await _attempts.RecordAsync(AttemptLimit.L3, byIp, ct);
        if (await _attempts.IsExceededAsync(AttemptLimit.L3, byEmail, ct) || await _attempts.IsExceededAsync(AttemptLimit.L3, byIp, ct))
        {
            return RegistrationResult.Refused(ErrorCode.RegistrationLimitExceeded);
        }

        DateTimeOffset now = _time.GetUtcNow();
        string normalized = TextNormalizer.NormalizeIdentity(email);
        Guid? existingId = await _db.Customers.AsNoTracking()
            .Where(c => c.NormalizedEmail == normalized)
            .Select(c => (Guid?)c.Id)
            .SingleOrDefaultAsync(ct);
        if (existingId is { } existing)
        {
            // A pending registration whose first link has ended is gone and its address is free again (02 3.13.5; Z-1). The nightly deletion is
            // PendingRegistrationDisposalPart (T324); this is the lazy half, done when the address is asked for - both read PendingRecordExpiry's rule.
            int deleted = await _db.Customers
                .Where(c => c.Id == existing)
                .Where(PendingRecordExpiry.EndedRegistration(_db, now))
                .ExecuteDeleteAsync(ct);
            if (deleted == 0)
            {
                return RegistrationResult.Refused(ErrorCode.EmailAlreadyRegistered);
            }

            _db.DisposalRecords.Add(DisposalRecord.Counted(DisposalRecordType.PendingRegistration, deleted, now));
        }

        DateTimeOffset expiresAt = now.AddHours(ProductConstants.EmailVerificationLinkHours);
        Customer customer = Customer.Create(name, email, _hashing.Hash(entered), emailVerifiedAt: null, googleLinkedAt: null, now);
        LinkRecord link = LinkRecord.ForCustomer(_linkKeys, LinkType.EmailVerification, customer.Id, expiresAt, now);
        _db.Customers.Add(customer);
        _db.LinkRecords.Add(link);
        return RegistrationResult.Registered(customer.Id, link.Id, expiresAt);
    }

    public async Task<VerificationLinkReading> ReadVerificationLinkAsync(string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        LinkReading reading = await _links.ReadAsync(key, ct);
        if (reading.Record is not { LinkType: LinkType.EmailVerification } record || reading.State is not { } state)
        {
            return new VerificationLinkReading(VerificationLinkStatus.Missing, null);
        }

        return new VerificationLinkReading(StatusOf(state), record.CustomerId);
    }

    public async Task<EmailVerificationResult> VerifyEmailAsync(string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        RequireTransaction("The verification is written only inside a transaction (02 3.13.5; 09 6 item 3).");

        LinkReading reading = await _links.ReadAsync(key, ct);
        if (reading.Record is not { LinkType: LinkType.EmailVerification, CustomerId: Guid customerId } record || reading.State is not { } state)
        {
            return EmailVerificationResult.LinkInvalid(VerificationLinkStatus.Missing);
        }

        if (state != LinkState.Valid)
        {
            return EmailVerificationResult.LinkInvalid(StatusOf(state));
        }

        // One conditional update: of two requests at once exactly one takes the link (07 2.5.3).
        if (!await _links.TryConsumeAsync(record.Id, ct))
        {
            LinkReading again = await _links.ReadAsync(key, ct);
            return EmailVerificationResult.LinkInvalid(again.State is { } now ? StatusOf(now) : VerificationLinkStatus.Missing);
        }

        DateTimeOffset moment = _time.GetUtcNow();
        int verified = await _db.Customers
            .Where(c => c.Id == customerId && c.EmailVerifiedAt == null)
            .ExecuteUpdateAsync(set => set.SetProperty(c => c.EmailVerifiedAt, moment), ct);
        if (verified != 1)
        {
            return EmailVerificationResult.LinkInvalid(VerificationLinkStatus.Used);
        }

        string email = await _db.Customers.AsNoTracking().Where(c => c.Id == customerId).Select(c => c.Email).SingleAsync(ct);
        int linked = await _ordering.LinkGuestOrdersAsync(customerId, email, ct);
        return EmailVerificationResult.Verified(customerId, linked);
    }

    public async Task<VerificationResendResult> ResendVerificationLinkAsync(Guid customerId, IPAddress? ipAddress, CancellationToken ct)
    {
        RequireTransaction("The verification link is written only inside a transaction (09 6 item 3).");

        var customer = await _db.Customers.AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => new { c.NormalizedEmail, c.EmailVerifiedAt })
            .SingleOrDefaultAsync(ct);
        if (customer is null)
        {
            return VerificationResendResult.Refused(ErrorCode.PendingRegistrationUnresolved);
        }

        // L-9 is counted on every request, on the e-mail and on the IP (02 8.2).
        AttemptSubject byEmail = AttemptSubject.Email(customer.NormalizedEmail);
        AttemptSubject byIp = AttemptSubject.Ip(ipAddress);
        await _attempts.RecordAsync(AttemptLimit.L9, byEmail, ct);
        await _attempts.RecordAsync(AttemptLimit.L9, byIp, ct);
        if (await _attempts.IsExceededAsync(AttemptLimit.L9, byEmail, ct) || await _attempts.IsExceededAsync(AttemptLimit.L9, byIp, ct))
        {
            return VerificationResendResult.Refused(ErrorCode.VerificationLinkLimitExceeded);
        }

        DateTimeOffset now = _time.GetUtcNow();
        DateTimeOffset? firstEnd = customer.EmailVerifiedAt is null ? await FirstLinkEndAsync(customerId, ct) : null;
        if (firstEnd is not { } expiresAt || expiresAt <= now)
        {
            return VerificationResendResult.Refused(ErrorCode.PendingRegistrationUnresolved);
        }

        // The new link ends where the first one ends: asking again does not lengthen the pending registration (02 3.13.5).
        LinkRecord link = LinkRecord.ForCustomer(_linkKeys, LinkType.EmailVerification, customerId, expiresAt, now);
        _db.LinkRecords.Add(link);
        await _links.SupersedeEarlierAsync(LinkType.EmailVerification, customerId, null, link.Id, ct);
        return VerificationResendResult.Issued(link.Id, expiresAt);
    }

    public async Task<PendingRegistrationReading?> ReadPendingRegistrationAsync(Guid customerId, CancellationToken ct)
    {
        string? email = await _db.Customers.AsNoTracking()
            .Where(c => c.Id == customerId && c.EmailVerifiedAt == null)
            .Select(c => c.Email)
            .SingleOrDefaultAsync(ct);
        if (email is null)
        {
            return null;
        }

        DateTimeOffset? end = await FirstLinkEndAsync(customerId, ct);
        return end is { } endsAt && endsAt > _time.GetUtcNow() ? new PendingRegistrationReading(email, endsAt) : null;
    }

    public async Task<CredentialCheckResult> CheckCredentialsAsync(string email, string entered, IPAddress? ipAddress, string? recognizedBrowserMark, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(entered);
        RequireTransaction("The sign-in check is made only inside a transaction (09 6 item 3).");

        // The limit L-1, the hashing work and the verified-address gate are the one password rule's (02 3.13.4, 3.13.17); the sign-in record is this module's.
        SignInVerdict check = await _passwords.CheckPasswordAsync(_accounts, email, entered, ipAddress, recognizedBrowserMark, ct);
        if (check.Outcome != VerdictKind.Matched || check.AccountId is not { } customerId)
        {
            // A blocked attempt is written as a failed one: the record keeps every attempt (02 3.13.19).
            await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Failed, ct);
            return check.Outcome switch
            {
                VerdictKind.Blocked => CredentialCheckResult.Refused(ErrorCode.SignInLimitExceeded),
                VerdictKind.NotEligible => CredentialCheckResult.Refused(ErrorCode.EmailNotVerified, check.AccountId),
                _ => CredentialCheckResult.Refused(ErrorCode.InvalidCredentials),
            };
        }

        await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Succeeded, ct);
        return CredentialCheckResult.Granted(customerId);
    }

    public Task<ResetRequested> RequestPasswordResetAsync(string email, IPAddress? ipAddress, CancellationToken ct)
    {
        RequireTransaction("The reset link is written only inside a transaction (09 6 item 3).");
        return _passwords.RequestResetAsync(_accounts, email, ipAddress, ct);
    }

    public Task<AccountLinkReading> ReadPasswordResetLinkAsync(string key, CancellationToken ct) => _passwords.ReadResetLinkAsync(AccountType.Customer, key, ct);

    public Task<ResetOutcome> ResetPasswordAsync(string key, string newPassword, string? recognizedBrowserMark, CancellationToken ct)
    {
        RequireTransaction("The reset is written only inside a transaction (02 3.13.9; 09 6 item 3).");
        return _passwords.ResetPasswordAsync(_accounts, key, newPassword, recognizedBrowserMark, ct);
    }

    public Task<ReauthenticationResult> ReauthenticateAsync(Guid customerId, Guid sessionId, ReauthenticationOperation operation, string password, IPAddress? ipAddress, string? recognizedBrowserMark, CancellationToken ct)
    {
        RequireTransaction("The reauthentication is written only inside a transaction (09 6 item 3).");
        return _passwords.ReauthenticateAsync(_accounts, customerId, sessionId, operation, password, ipAddress, recognizedBrowserMark, ct);
    }

    public Task<ChangeOutcome> ChangePasswordAsync(Guid customerId, Guid sessionId, string newPassword, string? reauthenticationValue, string? recognizedBrowserMark, CancellationToken ct)
    {
        RequireTransaction("The password is written only inside a transaction (09 6 item 3).");
        return _passwords.ChangePasswordAsync(_accounts, customerId, sessionId, newPassword, reauthenticationValue, recognizedBrowserMark, ct);
    }

    public async Task<OrderAccountResolution> ResolveOrderAccountAsync(Guid? signedInCustomerId, string contactEmail, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(contactEmail);
        if (signedInCustomerId is Guid signedIn)
        {
            var account = await _db.Customers.AsNoTracking()
                .Where(c => c.Id == signedIn)
                .Select(c => new { c.EmailVerifiedAt })
                .SingleOrDefaultAsync(ct);
            if (account is null)
            {
                return new OrderAccountResolution(OrderAccountKind.Guest, null);
            }

            return account.EmailVerifiedAt is null
                ? new OrderAccountResolution(OrderAccountKind.RefusedUnverified, signedIn)
                : new OrderAccountResolution(OrderAccountKind.SignedIn, signedIn);
        }

        // A guest: the account takes the order only when its address is verified; a pending registration never does (02 3.13.3; 8.1.2).
        string normalized = TextNormalizer.NormalizeIdentity(contactEmail);
        Guid? bound = await _db.Customers.AsNoTracking()
            .Where(c => c.NormalizedEmail == normalized && c.EmailVerifiedAt != null)
            .Select(c => (Guid?)c.Id)
            .SingleOrDefaultAsync(ct);
        return bound is { } id
            ? new OrderAccountResolution(OrderAccountKind.GuestBoundToAccount, id)
            : new OrderAccountResolution(OrderAccountKind.Guest, null);
    }

    public async Task<string?> GetMemberNameAsync(Guid customerId, CancellationToken ct)
    {
        return await _db.Customers.AsNoTracking().Where(c => c.Id == customerId).Select(c => c.Name).SingleOrDefaultAsync(ct);
    }

    public async Task<MemberContact?> GetMemberContactAsync(Guid customerId, CancellationToken ct)
    {
        return await _db.Customers.AsNoTracking().Where(c => c.Id == customerId).Select(c => new MemberContact(c.Name, c.Email)).SingleOrDefaultAsync(ct);
    }

    public async Task<SignInMethods?> GetSignInMethodsAsync(Guid customerId, CancellationToken ct)
    {
        return await _db.Customers.AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => new SignInMethods(c.Email, c.PasswordHash != null && c.PasswordHash != string.Empty, c.GoogleLinkedAt != null))
            .SingleOrDefaultAsync(ct);
    }

    public Task<GoogleReauthenticationResult> GrantGoogleReauthenticationAsync(
        Guid customerId, Guid sessionId, ReauthenticationOperation operation, GoogleReauthenticationIdentity identity, DateTimeOffset flowStartedAt, CancellationToken ct)
    {
        RequireTransaction("The Google re-authentication is written only inside a transaction (09 6 item 3).");
        return _googleReauthentications.GrantAsync(customerId, sessionId, operation, identity, flowStartedAt, ct);
    }

    public async Task<NameChangeResult> ChangeNameAsync(Guid customerId, string name, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(name);
        RequireTransaction("The name is written only inside a transaction (09 6 item 3).");

        string trimmed = name.Trim();
        if (trimmed.Length == 0)
        {
            return NameChangeResult.FieldRefused(new MembershipFieldError(nameof(RegisterInput.Name), ErrorCode.FieldRequired));
        }

        // No new verification and no order is touched: the recipient's name on an order is the order's own copy (02 3.13.21).
        int changed = await _db.Customers.Where(c => c.Id == customerId).ExecuteUpdateAsync(set => set.SetProperty(c => c.Name, trimmed), ct);
        return changed == 1 ? NameChangeResult.Changed() : NameChangeResult.Missing();
    }

    public bool IsGoogleSignInOpen => _google.IsOpen;

    public Task<GoogleSignInResult> SignInWithGoogleAsync(GoogleIdentity identity, IPAddress? ipAddress, CancellationToken ct) => _google.SignInAsync(identity, ipAddress, ct);

    public Task<GoogleLinkConfirmationResult> ConfirmGoogleLinkAsync(string key, CancellationToken ct) => _google.ConfirmGoogleLinkAsync(key, ct);

    public async Task<EmailLinkReading> ReadEmailLinkAsync(string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        LinkReading reading = await _links.ReadAsync(key, ct);
        if (reading.Record is not { CustomerId: Guid customerId } record || reading.State is not { } state)
        {
            return new EmailLinkReading(null, VerificationLinkStatus.Missing, null);
        }

        EmailLinkKind? kind = record.LinkType switch
        {
            LinkType.EmailVerification => EmailLinkKind.Registration,
            LinkType.NewEmailVerification => EmailLinkKind.NewAddress,
            LinkType.GoogleLinkConfirmation => EmailLinkKind.GoogleConfirmation,
            _ => null,
        };
        return kind is null ? new EmailLinkReading(null, VerificationLinkStatus.Missing, null) : new EmailLinkReading(kind, StatusOf(state), customerId);
    }

    public Task<EmailChangeRequestResult> RequestEmailChangeAsync(Guid customerId, string newEmail, IPAddress? ipAddress, CancellationToken ct) =>
        _emailChanges.RequestAsync(customerId, newEmail, ipAddress, ct);

    public Task<EmailChangeResendResult> ResendEmailChangeLinkAsync(Guid customerId, IPAddress? ipAddress, CancellationToken ct) =>
        _emailChanges.ResendAsync(customerId, ipAddress, ct);

    public Task<NewEmailVerificationResult> VerifyNewEmailAsync(string key, CancellationToken ct) => _emailChanges.VerifyNewEmailAsync(key, ct);

    public Task<IReadOnlyList<AddressBookEntry>> GetAddressBookAsync(Guid customerId, CancellationToken ct) => _addresses.ListAsync(customerId, ct);

    public Task<AddressBookEntry?> GetAddressBookEntryAsync(Guid customerId, Guid entryId, CancellationToken ct) => _addresses.GetAsync(customerId, entryId, ct);

    public Task<AddressBookSaveResult> CreateAddressBookEntryAsync(Guid customerId, AddressBookInput input, CancellationToken ct) => _addresses.CreateAsync(customerId, input, ct);

    public Task<AddressBookSaveResult> UpdateAddressBookEntryAsync(Guid customerId, Guid entryId, AddressBookInput input, CancellationToken ct) =>
        _addresses.UpdateAsync(customerId, entryId, input, ct);

    public Task<AddressBookDeleteResult> DeleteAddressBookEntryAsync(Guid customerId, Guid entryId, CancellationToken ct) => _addresses.DeleteAsync(customerId, entryId, ct);

    public AddressRequirement GetAddressRequirement(IReadOnlyCollection<ProductType> itemTypes) => AddressFieldRules.RequirementOf(itemTypes);

    public Task<OrderAddressResolution> ResolveOrderAddressesAsync(OrderAddressRequest request, CancellationToken ct) => _addresses.ResolveAsync(request, ct);

    public Task<AccountDeletionResult> DeleteAccountAsync(Guid customerId, Guid sessionId, string? reauthenticationValue, CancellationToken ct) => _deletions.DeleteOwnAsync(customerId, sessionId, reauthenticationValue, ct);

    public Task<AccountDeletionResult> DeleteAccountOnRequestAsync(Guid customerId, CancellationToken ct) => _deletions.DeleteByAdministratorAsync(customerId, ct);

    public Task<DeletionReapplyResult> ReapplyAccountDeletionAsync(Guid customerId, CancellationToken ct) => _deletions.ReapplyAsync(customerId, ct);

    public Task<MemberRecord> FindMemberRecordAsync(string email, CancellationToken ct) => _memberRecords.FindAsync(email, ct);

    public Task<DateTimeOffset?> GetDeletionBlockedUntilAsync(Guid customerId, CancellationToken ct) => _memberRecords.DeletionBlockedUntilAsync(customerId, ct);

    public Task<int> WriteCustomerExportAsync(Stream destination, long maxBytes, CancellationToken ct) => new CustomerExport(_db).WriteAsync(destination, maxBytes, ct);

    private async Task<DateTimeOffset?> FirstLinkEndAsync(Guid customerId, CancellationToken ct)
    {
        return await _db.LinkRecords.AsNoTracking()
            .Where(l => l.CustomerId == customerId && l.LinkType == LinkType.EmailVerification)
            .OrderBy(l => l.CreatedAt)
            .Select(l => (DateTimeOffset?)l.ExpiresAt)
            .FirstOrDefaultAsync(ct);
    }

    private void RequireTransaction(string message)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(message);
        }
    }

    internal static VerificationLinkStatus StatusOf(LinkState state) => state switch
    {
        LinkState.Valid => VerificationLinkStatus.Valid,
        LinkState.Used => VerificationLinkStatus.Used,
        LinkState.Superseded => VerificationLinkStatus.Superseded,
        LinkState.Expired => VerificationLinkStatus.Expired,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Not a link state."),
    };

}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddMembershipModule</c> once.</summary>
internal static class MembershipModuleRegistration
{
    public static IServiceCollection AddMembershipModule(this IServiceCollection services)
    {
        services.AddScoped<AddressBooks>();
        services.AddScoped<IMembershipModule, MembershipModule>();
        services.AddScoped<CustomerPasswords>();
        services.AddScoped<GoogleSignIns>();
        services.AddScoped<GoogleReauthentications>();
        services.AddScoped<EmailChanges>();
        services.AddScoped<AccountDeletions>();
        services.AddScoped<MemberRecords>();
        services.AddScoped<IAdminPendingEmailChanges, AdminPendingEmailChanges>();
        services.AddScoped<ISessionService, SessionService>();

        // Z-1's nightly deletion (T324; D-253): the unverified registrations and the pending address changes whose link has ended.
        services.AddDisposalPart<PendingRegistrationDisposalPart>();
        services.AddDisposalPart<PendingEmailChangeDisposalPart>();
        return services;
    }
}
