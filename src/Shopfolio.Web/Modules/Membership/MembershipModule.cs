using System.ComponentModel.DataAnnotations;
using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
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
    private readonly TimeProvider _time;
    private readonly AddressBooks _addresses;

    public MembershipModule(
        AppDbContext db,
        ISettingsModule settings,
        IOrderingModule ordering,
        AttemptCounter attempts,
        LinkRecords links,
        LinkKeys linkKeys,
        LoginRecorder logins,
        PasswordService hashing,
        TimeProvider time,
        AddressBooks addresses)
    {
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

        // The password's length and common-password policy is T342's (D-251); here it must only not be empty.
        if (entered.Length == 0)
        {
            fields.Add(new MembershipFieldError(nameof(RegisterInput.Password), ErrorCode.FieldRequired));
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
        var existing = await _db.Customers.AsNoTracking()
            .Where(c => c.NormalizedEmail == normalized)
            .Select(c => new { c.Id, c.EmailVerifiedAt })
            .SingleOrDefaultAsync(ct);
        if (existing is not null)
        {
            // A pending registration whose first link has ended is gone and its address is free again (02 3.13.5; Z-1). The nightly deletion is T324's (D-253); this is the
            // lazy half, done when the address is asked for.
            DateTimeOffset? firstEnd = existing.EmailVerifiedAt is null ? await FirstLinkEndAsync(existing.Id, ct) : null;
            bool endedPending = existing.EmailVerifiedAt is null && (firstEnd is null || firstEnd <= now);
            if (!endedPending)
            {
                return RegistrationResult.Refused(ErrorCode.EmailAlreadyRegistered);
            }

            await _db.Customers.Where(c => c.Id == existing.Id && c.EmailVerifiedAt == null).ExecuteDeleteAsync(ct);
            _db.DisposalRecords.Add(DisposalRecord.Counted(DisposalRecordType.PendingRegistration, 1, now));
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

    public async Task<CredentialCheckResult> CheckCredentialsAsync(string email, string entered, IPAddress? ipAddress, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(entered);
        RequireTransaction("The sign-in check is made only inside a transaction (09 6 item 3).");

        string normalized = TextNormalizer.NormalizeIdentity(email);
        Customer? customer = await _db.Customers.SingleOrDefaultAsync(c => c.NormalizedEmail == normalized, ct);

        // The same work whether or not the address has an account (05 6.1.4).
        HashCheck check = customer?.PasswordHash is { } digest ? _hashing.Verify(digest, entered) : _hashing.VerifyNobody(entered);
        if (customer is null || !check.Matched)
        {
            await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Failed, ct);
            return CredentialCheckResult.Refused(ErrorCode.InvalidCredentials);
        }

        if (customer.EmailVerifiedAt is null)
        {
            // The hard gate (02 3.13.4): the right password does not open an unverified account, and the attempt is a failed one.
            await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Failed, ct);
            return CredentialCheckResult.Refused(ErrorCode.EmailNotVerified);
        }

        if (check.NeedsRehash)
        {
            customer.RenewDigest(_hashing.Hash(entered));
        }

        await _logins.RecordAsync(ipAddress, AccountType.Customer, email, LoginOutcome.Succeeded, ct);
        return CredentialCheckResult.Granted(customer.Id);
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

    public Task<IReadOnlyList<AddressBookEntry>> GetAddressBookAsync(Guid customerId, CancellationToken ct) => _addresses.ListAsync(customerId, ct);

    public Task<AddressBookEntry?> GetAddressBookEntryAsync(Guid customerId, Guid entryId, CancellationToken ct) => _addresses.GetAsync(customerId, entryId, ct);

    public Task<AddressBookSaveResult> CreateAddressBookEntryAsync(Guid customerId, AddressBookInput input, CancellationToken ct) => _addresses.CreateAsync(customerId, input, ct);

    public Task<AddressBookSaveResult> UpdateAddressBookEntryAsync(Guid customerId, Guid entryId, AddressBookInput input, CancellationToken ct) =>
        _addresses.UpdateAsync(customerId, entryId, input, ct);

    public Task<AddressBookDeleteResult> DeleteAddressBookEntryAsync(Guid customerId, Guid entryId, CancellationToken ct) => _addresses.DeleteAsync(customerId, entryId, ct);

    public AddressRequirement GetAddressRequirement(IReadOnlyCollection<ProductType> itemTypes) => AddressFieldRules.RequirementOf(itemTypes);

    public Task<OrderAddressResolution> ResolveOrderAddressesAsync(OrderAddressRequest request, CancellationToken ct) => _addresses.ResolveAsync(request, ct);

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

    private static VerificationLinkStatus StatusOf(LinkState state) => state switch
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
        return services;
    }
}
