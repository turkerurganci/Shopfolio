using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The maker of <see cref="OrderNoticeContext"/> (T385; D-232, D-293; 08 2.2.6 items 2-4; 05 4.4.2): at the time of sending it reads the order's own facts through
/// <see cref="IOrderNoticeFacts"/> (09 6 item 2), the two frozen legal texts of a B-1 through <see cref="IOrderDocuments"/> - already filled with the values of the confirmation
/// moment by <c>PlaceholderFiller</c> there (09 11.4 item 4) -, the province names an address-writing notice needs (B-9, B-10) through <see cref="IProvinceList"/>, and the
/// firm's contact from the legal identity (B-6, B-7). The first e-mail's moment of a re-sent row comes from the row's own event values. Nothing is written; a row whose order is gone
/// has no context (null) and makes no body.
/// </summary>
internal sealed class OrderNoticeContexts
{
    private readonly IOrderNoticeFacts _facts;
    private readonly IOrderDocuments _documents;
    private readonly IProvinceList _provinces;
    private readonly ISettingsModule _settings;

    public OrderNoticeContexts(IOrderNoticeFacts facts, IOrderDocuments documents, IProvinceList provinces, ISettingsModule settings)
    {
        ArgumentNullException.ThrowIfNull(facts);
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(provinces);
        ArgumentNullException.ThrowIfNull(settings);
        _facts = facts;
        _documents = documents;
        _provinces = provinces;
        _settings = settings;
    }

    /// <summary>The context of the given customer notice (its values are read from the row's event values), or null when the order does not exist.</summary>
    public async Task<OrderNoticeContext?> MakeAsync(CustomerNotice notice, string? eventValues, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notice);
        OrderNoticeFacts? facts = await _facts.GetAsync(notice.OrderId, ct);
        if (facts is null)
        {
            return null;
        }

        string? preInformation = null;
        string? contract = null;
        if (notice is OrderReceivedNotice)
        {
            preInformation = await _documents.GetTextAsync(notice.OrderId, LegalTextType.PreInformationForm, ct);
            contract = await _documents.GetTextAsync(notice.OrderId, LegalTextType.DistanceSalesContract, ct);
        }

        // The firm's contact closes several notices (B-6, B-7, B-10, B-14, B-15 ...): the template that does not use it ignores it.
        LegalIdentity identity = await _settings.GetLegalIdentityAsync(ct);
        string firmEmail = identity.Email?.Trim() ?? string.Empty;
        string firmPhone = identity.Phone?.Trim() ?? string.Empty;

        IReadOnlyDictionary<Guid, string>? provinceNames = null;
        Guid[] provinceIds = [.. AddressesOf(notice).Select(a => a.ProvinceId).Distinct()];
        if (provinceIds.Length > 0)
        {
            provinceNames = await _provinces.GetNamesAsync(provinceIds, ct);
        }

        return new OrderNoticeContext(
            facts.OrderNumber,
            facts.AccessKey,
            facts.IsGuestOrder,
            facts.IsAccountDeleted,
            facts.BankTransferIban,
            facts.HasDigitalItem,
            preInformation,
            contract,
            firmEmail,
            firmPhone,
            CustomerNoticeValues.ReadFirstEmailSentAt(eventValues),
            provinceNames);
    }

    private static IEnumerable<NoticeAddress> AddressesOf(CustomerNotice notice)
    {
        switch (notice)
        {
            case DeclarationReceivedNotice { ReturnAddress: { } returnAddress }:
                yield return returnAddress;
                break;
            case AddressCorrectedNotice corrected:
                yield return corrected.NewAddress;
                break;
        }
    }
}
