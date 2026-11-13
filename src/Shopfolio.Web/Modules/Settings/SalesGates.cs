using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The reader of the sales gate (02 3.1.5; 05 4.1.5; 09 10.4 item 1 and 5): it collects the facts - the identity's missing fields, the switch and the return address from
/// the settings row, the cookie policy's publication, and the three outside conditions the payment and legal text tasks bind - and hands them to <see cref="SalesGateRules"/>.
/// Every call reads the database; the gate is never stored and never cached, and the order confirmation's transaction calls this same method, so what it reads is the
/// row as it is in that transaction and not an earlier image. The method only reads and opens no transaction (09 6 item 3).
/// </summary>
internal sealed class SalesGates
{
    private readonly AppDbContext _db;
    private readonly CompanyIdentities _identities;
    private readonly ICardPaymentCondition _card;
    private readonly IBankTransferCondition _bankTransfer;
    private readonly IPrivacyNoticeCondition _privacyNotice;
    private readonly ICookiePolicyPublication _cookiePolicy;
    private readonly IAuditTrail _trail;

    public SalesGates(
        AppDbContext db,
        CompanyIdentities identities,
        ICardPaymentCondition card,
        IBankTransferCondition bankTransfer,
        IPrivacyNoticeCondition privacyNotice,
        ICookiePolicyPublication cookiePolicy,
        IAuditTrail trail)
    {
        _trail = trail;
        _db = db;
        _identities = identities;
        _card = card;
        _bankTransfer = bankTransfer;
        _privacyNotice = privacyNotice;
        _cookiePolicy = cookiePolicy;
    }

    public async Task<SalesGate> GetAsync(CancellationToken ct)
    {
        CompanyIdentityCompleteness identity = await _identities.GetCompletenessAsync(ct);
        var settings = await _db.CompanySettings.AsNoTracking()
            .Select(s => new
            {
                s.IsSalesTemporarilyClosed,
                ReturnAddressEntered = s.ReturnRecipientName != null && s.ReturnProvinceId != null && s.ReturnDistrict != null && s.ReturnAddressLine != null,
            })
            .SingleAsync(ct);
        bool paymentOpen = await _card.IsOpenAsync(ct) || await _bankTransfer.IsOpenAsync(ct);
        bool privacyNotice = await _privacyNotice.IsPublishedWithHostingLocationAsync(ct);
        bool cookiePolicy = await _cookiePolicy.IsPublishedAsync(ct);
        return SalesGateRules.Evaluate(new SalesGateFacts(identity.MissingFields, paymentOpen, settings.IsSalesTemporarilyClosed, privacyNotice, cookiePolicy, settings.ReturnAddressEntered));
    }

    /// <summary>
    /// The first moment sales were open in this installation (02 10.6.3 m6; 06 10.6 m6): derived by <see cref="SalesOpeningRules"/> from the settings row, the trail's
    /// changes of it and the legal texts' first publications. Only reads.
    /// </summary>
    public async Task<DateTimeOffset?> FindFirstOpenedAtAsync(CancellationToken ct)
    {
        var row = await _db.CompanySettings.AsNoTracking()
            .Select(s => new
            {
                s.IsBankTransferEnabled,
                s.BankTransferIban,
                s.IsSalesTemporarilyClosed,
                s.ReturnRecipientName,
                s.ReturnProvinceId,
                s.ReturnDistrict,
                s.ReturnAddressLine,
            })
            .SingleAsync(ct);
        Dictionary<string, string?> current = new()
        {
            [nameof(CompanySettings.IsBankTransferEnabled)] = row.IsBankTransferEnabled ? "true" : "false",
            [nameof(CompanySettings.BankTransferIban)] = row.BankTransferIban,
            [nameof(CompanySettings.IsSalesTemporarilyClosed)] = row.IsSalesTemporarilyClosed ? "true" : "false",
            [nameof(CompanySettings.ReturnRecipientName)] = row.ReturnRecipientName,
            [nameof(CompanySettings.ReturnProvinceId)] = row.ReturnProvinceId?.ToString(),
            [nameof(CompanySettings.ReturnDistrict)] = row.ReturnDistrict,
            [nameof(CompanySettings.ReturnAddressLine)] = row.ReturnAddressLine,
        };
        IReadOnlyList<AuditFieldChangeAt> changes = await _trail.ListFieldChangesAsync(AuditOperation.UpdateSettings, nameof(CompanySettings), ct);
        DateTimeOffset? privacyNotice = await _db.LegalTextVersions.AsNoTracking()
            .Where(l => l.LegalTextType == LegalTextType.PrivacyNotice && l.PublishedAt != null && l.HostingLocation != null)
            .MinAsync(l => l.PublishedAt, ct);
        DateTimeOffset? cookiePolicy = await _db.LegalTextVersions.AsNoTracking()
            .Where(l => l.LegalTextType == LegalTextType.CookiePolicy && l.PublishedAt != null)
            .MinAsync(l => l.PublishedAt, ct);
        return SalesOpeningRules.FirstOpenedAt(current, changes, await _card.IsOpenAsync(ct), privacyNotice, cookiePolicy);
    }
}

/// <summary>
/// The moment sales first opened in this installation (02 10.6.3 m6; K-418, K-486): the first moment the conditions of 02 3.1.5 were met together. It is not stored (06 10.6 m6);
/// it is derived, and the gate itself decides each step through <see cref="SalesGateRules.Evaluate"/>, so the derivation and the live gate cannot differ.
/// <para>
/// The sources: the settings row's condition fields as they are now, rewound with the old values of their trail changes (02 10.3.1 (6)); the first publication of the privacy notice
/// with its hosting location and of the cookie policy (02 10.3.1 (5): a published version is only ever added, so its publish time stays); the card line, an installation value that no
/// panel changes. The identity condition is implied by the privacy notice: a notice is published only with the identity complete (LegalTexts), and a filled identity field cannot be
/// emptied. Pure: no database, no clock. It stands in the gate reader's file because it names the sales switch, which only the gate's files may (SalesGateSourceScan).
/// </para>
/// </summary>
internal static class SalesOpeningRules
{
    /// <summary>The settings fields the gate reads (02 3.1.5 conditions 2-4); the trail rows of the settings row name them by these names.</summary>
    public static readonly IReadOnlyList<string> TrackedFields =
    [
        nameof(CompanySettings.IsBankTransferEnabled),
        nameof(CompanySettings.BankTransferIban),
        nameof(CompanySettings.IsSalesTemporarilyClosed),
        nameof(CompanySettings.ReturnRecipientName),
        nameof(CompanySettings.ReturnProvinceId),
        nameof(CompanySettings.ReturnDistrict),
        nameof(CompanySettings.ReturnAddressLine),
    ];

    /// <param name="current">The tracked fields as they are now, by <see cref="TrackedFields"/> names; a boolean is <c>"true"</c> or <c>"false"</c>, an empty field <see langword="null"/>.</param>
    /// <param name="changes">The settings row's field changes from the trail, oldest first; fields outside <see cref="TrackedFields"/> are ignored.</param>
    /// <param name="cardOpen">The card line of the payment method condition.</param>
    /// <param name="privacyNoticePublishedAt">The first publication of the privacy notice with its hosting location; <see langword="null"/> when there is none.</param>
    /// <param name="cookiePolicyPublishedAt">The first publication of the cookie policy; <see langword="null"/> when there is none.</param>
    /// <returns>The first moment the gate was open; <see langword="null"/> when it never was.</returns>
    public static DateTimeOffset? FirstOpenedAt(
        IReadOnlyDictionary<string, string?> current,
        IReadOnlyList<AuditFieldChangeAt> changes,
        bool cardOpen,
        DateTimeOffset? privacyNoticePublishedAt,
        DateTimeOffset? cookiePolicyPublishedAt)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(changes);
        if (privacyNoticePublishedAt is not { } privacy || cookiePolicyPublishedAt is not { } cookie)
        {
            return null; // both texts are only ever added: one never published means the gate was never open
        }

        List<AuditFieldChangeAt> tracked = [.. changes.Where(c => TrackedFields.Contains(c.Field)).OrderBy(c => c.At)];

        // Rewind: the state before the oldest change is the current state with every change's old value put back, newest first.
        Dictionary<string, string?> state = new(current);
        for (int i = tracked.Count - 1; i >= 0; i--)
        {
            state[tracked[i].Field] = tracked[i].OldValue;
        }

        // Replay forward: the gate can open only at a moment one of its sources changed.
        DateTimeOffset[] moments = [.. tracked.Select(c => c.At).Append(privacy).Append(cookie).Distinct().Order()];
        int applied = 0;
        foreach (DateTimeOffset moment in moments)
        {
            while (applied < tracked.Count && tracked[applied].At <= moment)
            {
                state[tracked[applied].Field] = tracked[applied].NewValue;
                applied++;
            }

            SalesGateFacts facts = new(
                MissingIdentityFields: [],
                PaymentMethodOpen: cardOpen || (IsTrue(state, nameof(CompanySettings.IsBankTransferEnabled)) && IsFilled(state, nameof(CompanySettings.BankTransferIban))),
                SalesTemporarilyClosed: IsTrue(state, nameof(CompanySettings.IsSalesTemporarilyClosed)),
                PrivacyNoticePublishedWithHostingLocation: privacy <= moment,
                CookiePolicyPublished: cookie <= moment,
                ReturnAddressEntered: IsFilled(state, nameof(CompanySettings.ReturnRecipientName))
                    && IsFilled(state, nameof(CompanySettings.ReturnProvinceId))
                    && IsFilled(state, nameof(CompanySettings.ReturnDistrict))
                    && IsFilled(state, nameof(CompanySettings.ReturnAddressLine)));
            if (SalesGateRules.Evaluate(facts).IsOpen)
            {
                return moment;
            }
        }

        return null;
    }

    private static bool IsTrue(Dictionary<string, string?> state, string field) => state.GetValueOrDefault(field) == "true";

    private static bool IsFilled(Dictionary<string, string?> state, string field) => !string.IsNullOrEmpty(state.GetValueOrDefault(field));
}
