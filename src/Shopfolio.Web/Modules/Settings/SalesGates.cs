using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
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

    public SalesGates(
        AppDbContext db,
        CompanyIdentities identities,
        ICardPaymentCondition card,
        IBankTransferCondition bankTransfer,
        IPrivacyNoticeCondition privacyNotice,
        ICookiePolicyPublication cookiePolicy)
    {
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
}
