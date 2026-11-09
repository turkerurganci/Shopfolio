using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering.Documents;

/// <summary>
/// The one place that produces and shows an order's two documents (T356; 09 9.3 items 1, 3, 4, 5; 09 11.4 item 4). <see cref="Produce"/> is the confirmation's step (T348 calls it before the
/// order's first save and hands the result to <c>Order.FreezeDocuments</c>): the form is the confirmation summary followed by the form version's text, the contract is the contract version's
/// text, both with the order's frozen values written and the customer's tokens left standing - the copy holds no personal data. <see cref="GetTextAsync"/> reads the copy and fills the
/// tokens from the confirmation's parties; the order page, the panel, the confirmation e-mail's body and the panel's resend all call it, so they show the same text. The step before the
/// order exists fills the same tokens from the customer's own values through <see cref="PersonalValues"/>, so what the customer read is what the copy says. The folder is in the scope of
/// the rendering scan (D-167): no raw markup, no request object.
/// </summary>
internal sealed class OrderDocuments : IOrderDocuments
{
    /// <summary>What the order's number reads as before it is born (the confirmation step's text): the number is given when the order is confirmed.</summary>
    public const string OrderNumberBeforeConfirmation = "sipariş onayında verilir";

    private readonly AppDbContext _db;
    private readonly ISettingsModule _settings;

    public OrderDocuments(AppDbContext db, ISettingsModule settings)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(settings);
        _db = db;
        _settings = settings;
    }

    /// <summary>
    /// The two copies of a confirmed order (09 9.3 item 1): the form is the summary of the order's frozen items and then the form version's text; both texts have the frozen values written
    /// (number, total, shipping fee, shipping period) and keep every personal token. A version text that holds a personal token the copy would fill is not possible: the copy is written with
    /// the tokens standing, so the second stage can read the parties from the history.
    /// </summary>
    public static (string PreInformationForm, string DistanceSalesContract) Produce(Order order, IReadOnlyList<OrderItem> items, string formVersionText, string contractVersionText)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(formVersionText);
        ArgumentNullException.ThrowIfNull(contractVersionText);
        ConfirmationSummary summary = new([.. items.Select(ConfirmationSummaryText.LineOf)], order.ShippingFee, order.Total, order.PaymentMethod, order.ShippingPeriod, null, null, null);
        IReadOnlyDictionary<string, string?> frozen = FrozenValues(order.OrderNumber, order.Total, order.ShippingFee, order.ShippingPeriod);
        string form = ConfirmationSummaryText.Render(summary) + "\n\n" + PlaceholderFiller.Fill(formVersionText, frozen, requireAll: false);
        string contract = PlaceholderFiller.Fill(contractVersionText, frozen, requireAll: false);
        return (form, contract);
    }

    public async Task<string> GetTextAsync(Guid orderId, LegalTextType type, CancellationToken ct)
    {
        if (type is not (LegalTextType.PreInformationForm or LegalTextType.DistanceSalesContract))
        {
            throw new ArgumentException("Only the Pre-Information Form and the Distance Sales Contract are order documents (09 9.3 item 4).", nameof(type));
        }

        Order order = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("The order does not exist.");
        List<OrderAddressHistory> addresses = await _db.OrderAddressHistories.AsNoTracking().Where(h => h.OrderId == orderId).ToListAsync(ct);
        List<OrderEmailHistory> emails = await _db.OrderEmailHistories.AsNoTracking().Where(h => h.OrderId == orderId).ToListAsync(ct);
        ConfirmationParties parties = OrderFreeze.PartiesAtConfirmation(order, addresses, emails);
        IReadOnlyDictionary<Guid, string> provinces = (await _settings.GetProvincesAsync(ct)).ToDictionary(p => p.Id, p => p.Name);
        string copy = type == LegalTextType.PreInformationForm ? order.PreInformationFormText : order.DistanceSalesContractText;

        // An erased field is an empty token value (09 9.3 item 3): the filler turns a null into an empty text.
        return PlaceholderFiller.Fill(copy, PersonalValues(parties.ContactEmail, parties.Shipping, parties.Billing, provinces), requireAll: false);
    }

    /// <summary>The values an order freezes at its making, written into the copy (09 9.3 item 2): the order's number, the total, the shipping fee and the shipping period.</summary>
    public static IReadOnlyDictionary<string, string?> FrozenValues(string orderNumber, decimal total, decimal shippingFee, int? shippingPeriod)
    {
        return new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["OrderNumber"] = orderNumber,
            ["Total"] = DisplayFormat.Amount(total),
            ["ShippingFee"] = DisplayFormat.Amount(shippingFee),
            ["ShippingPeriod"] = shippingPeriod is int days ? DisplayFormat.Number(days) : null,
        };
    }

    /// <summary>
    /// The ten personal tokens of 09 9.3 item 2 with their values (the province token holds the name; the billing address has no telephone). An absent address or e-mail is an empty text.
    /// A province id the list does not know is an empty text too.
    /// </summary>
    public static IReadOnlyDictionary<string, string?> PersonalValues(string? contactEmail, PartyAddress? shipping, PartyAddress? billing, IReadOnlyDictionary<Guid, string> provinces)
    {
        ArgumentNullException.ThrowIfNull(provinces);
        string? Province(PartyAddress? address) => address is not null && provinces.TryGetValue(address.ProvinceId, out string? name) ? name : null;
        return new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["ShippingRecipientName"] = shipping?.RecipientName,
            ["ShippingPhone"] = shipping?.Phone,
            ["ShippingProvince"] = Province(shipping),
            ["ShippingDistrict"] = shipping?.District,
            ["ShippingAddressLine"] = shipping?.AddressLine,
            ["BillingRecipientName"] = billing?.RecipientName,
            ["BillingProvince"] = Province(billing),
            ["BillingDistrict"] = billing?.District,
            ["BillingAddressLine"] = billing?.AddressLine,
            ["ContactEmail"] = contactEmail,
        };
    }
}
