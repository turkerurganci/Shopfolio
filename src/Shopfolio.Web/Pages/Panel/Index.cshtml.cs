using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>One warning of the home page (04 3.4.3): its sentence and the screen or list it leads to; the channel warning leads nowhere - its mending is the setup setting, not a screen (K-769).</summary>
public sealed record HomeWarningView(string Id, string Text, string? Href);

/// <summary>One open item of the setup checklist (04 3.4.4): what is missing, the screen of its own setting and, under the notice item, the line about the closed entrances (04 9.3.4; K-850).</summary>
public sealed record HomeChecklistView(string Text, string Href, string? Note = null);

/// <summary>
/// The words of the panel home (T521; 04 3.4; 02 10.6.1, 10.8.2): written here once. The warning sentences are K-799's; the sentence of a card refund whose result is awaited is the product's own and says the
/// state the provider's panel will show (K-940).
/// </summary>
internal static class PanelHomeTexts
{
    public const string Title = "Ana sayfa";

    public const string SalesClosedTemporary = "Satış kapalı — geçici olarak kapatıldı.";

    public const string ChannelBroken = "E-posta gönderilemiyor — kurulum ayarını kontrol edin.";

    public const string ChecklistTitle = "Kurulum listesi";

    public static string SalesClosed(IEnumerable<string> missing) => "Satış kapalı — " + string.Join(", ", missing) + ".";

    public static string FirmNoticesBroken(string address) => $"Firma bildirimleri ulaşmıyor — {address} adresine giden e-postalar teslim edilemedi.";

    public static string EmailNotDelivered(int orders) => $"{DisplayFormat.Number(orders)} siparişte müşteriye e-posta ulaşmadı.";

    public static string CardRefundFailed(int orders) => $"{DisplayFormat.Number(orders)} siparişte kart iadesi gerçekleşmedi.";

    public static string CardRefundOutcomePending(int orders) => $"{DisplayFormat.Number(orders)} siparişte kart iadesinin sonucu bekleniyor — iade sağlayıcıda gerçekleşmiş olabilir, önce sağlayıcının panelinde bakın.";

    /// <summary>The line under the notice item: until both legal texts are published the entrances that collect data are closed (04 9.3.4; 04 2.8.3; K-850).</summary>
    public const string ClosedEntrances = "İki metin de yayına alınana kadar iletişim formu, hesap kaydı ve yeni hesap açan Google girişi kapalıdır.";

    /// <summary>The item's name on the refund of a late payment, which has no item (04 9.3.6; 02 7.2.8; K-1108).</summary>
    public const string LatePaymentRefund = "geç gelen ödemenin geri ödemesi";

    /// <summary>The reason of an IBAN request in a short sentence for the firm (04 9.3.6); the customer's own wording is the e-mail's.</summary>
    public static string IbanReason(RefundIbanRequestReason reason) => reason switch
    {
        RefundIbanRequestReason.ReturnAfterIbanDeleted => "İade edilen ürün ulaştı",
        RefundIbanRequestReason.TransferFailed => "Geri ödeme havalesi gerçekleşmedi",
        RefundIbanRequestReason.CardRefundFailed => "Kart iadesi gerçekleşmedi",
        RefundIbanRequestReason.NoticeWithoutIban => "Cayma ya da fesih bildirimi IBAN içermiyordu",
        RefundIbanRequestReason.CancellationOrRemoval => "Ödenmiş kalem iptal edildi ya da siparişten çıkarıldı",
        RefundIbanRequestReason.AdminRequest => "Firma IBAN istedi",
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "Not an IBAN request reason."),
    };

    /// <summary>The checklist item's name, as 02 10.8.2 writes it.</summary>
    public static string ChecklistName(SetupChecklistItemKind kind) => kind switch
    {
        SetupChecklistItemKind.CompanyIdentity => "Firma kimliği",
        SetupChecklistItemKind.PaymentMethod => "Ödeme yöntemi",
        SetupChecklistItemKind.PrivacyNotice => "Aydınlatma metni",
        SetupChecklistItemKind.CookiePolicy => "Çerez politikası",
        SetupChecklistItemKind.ReturnAddress => "İade adresi",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a setup checklist item."),
    };

    /// <summary>The screen of the item's own setting (04 3.4.4).</summary>
    public static string ChecklistPath(SetupChecklistItemKind kind) => kind switch
    {
        SetupChecklistItemKind.CompanyIdentity => PanelFramePaths.FirmIdentityAndSales,
        SetupChecklistItemKind.PaymentMethod => PanelFramePaths.PaymentMethods,
        SetupChecklistItemKind.PrivacyNotice or SetupChecklistItemKind.CookiePolicy => PanelFramePaths.LegalTexts,
        SetupChecklistItemKind.ReturnAddress => PanelFramePaths.ShippingAndDefaults,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a setup checklist item."),
    };

    /// <summary>The name of a sales gate condition that is not met, for the closed-sales warning.</summary>
    public static string ConditionName(SalesGateCondition condition) => condition switch
    {
        SalesGateCondition.CompanyIdentity => "firma kimliği eksik",
        SalesGateCondition.PaymentMethod => "açık bir ödeme yöntemi yok",
        SalesGateCondition.PrivacyNotice => "aydınlatma metni yayında değil",
        SalesGateCondition.CookiePolicy => "çerez politikası yayında değil",
        SalesGateCondition.ReturnAddress => "iade adresi eksik",
        _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, "Not a sales gate condition."),
    };
}

/// <summary>
/// The panel home (T521; 07 UC-86; 04 3.4, E-31): the warnings, the setup checklist, the six counters and the "Müşteriden beklenenler" list, in that order; a section with nothing to show is not drawn. Everything is
/// computed from the records in this request and nothing is stored (K-405, K-467; 02 10.8.2). The counters lead to the order and request lists by filter, the checklist items to the screens of their own settings,
/// the warnings to the screen that mends them. The page has no form and no write.
/// </summary>
[NoStore]
internal sealed class PanelHomeModel : PageModel
{
    private readonly IPanelHome _home;
    private readonly IPanelHomeDetails _details;
    private readonly ISettingsModule _settings;
    private readonly TimeProvider _time;

    public PanelHomeModel(IPanelHome home, IPanelHomeDetails details, ISettingsModule settings, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(home);
        ArgumentNullException.ThrowIfNull(details);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(time);
        _home = home;
        _details = details;
        _settings = settings;
        _time = time;
    }

    public IReadOnlyList<HomeWarningView> Warnings { get; private set; } = [];

    /// <summary>The open items of the checklist; empty when every item is complete (the section is then not drawn).</summary>
    public IReadOnlyList<HomeChecklistView> Checklist { get; private set; } = [];

    public IReadOnlyList<PendingCounterItem> Counters { get; private set; } = [];

    public AwaitingCustomerList Awaiting { get; private set; } = AwaitingCustomerList.Of([]);

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        PanelHome home = await _home.GetAsync(ct);
        PanelHomeDetails details = await _details.GetAsync(ct);
        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        List<HomeWarningView> warnings = [];

        if (!gate.IsOpen)
        {
            string[] missing = [.. gate.UnmetConditions.Where(c => c != SalesGateCondition.TemporaryClosure).Select(PanelHomeTexts.ConditionName)];
            string text = missing.Length > 0 ? PanelHomeTexts.SalesClosed(missing) : PanelHomeTexts.SalesClosedTemporary;
            warnings.Add(new HomeWarningView("home-warning-sales", text, missing.Length > 0 ? WarningPathOf(gate) : PanelFramePaths.SalesStateSection));
        }

        if (home.ChannelWarningOpen)
        {
            warnings.Add(new HomeWarningView("home-warning-channel", PanelHomeTexts.ChannelBroken, null));
        }

        if (home.FirmNoticesWarningOpen)
        {
            CompanyIdentity identity = await _settings.GetCompanyIdentityAsync(ct);
            warnings.Add(new HomeWarningView("home-warning-firm-notices", PanelHomeTexts.FirmNoticesBroken(identity.Email ?? string.Empty), PanelFramePaths.FirmIdentityAndSales));
        }

        if (details.EmailNotDeliveredOrders > 0)
        {
            warnings.Add(new HomeWarningView("home-warning-email-undelivered", PanelHomeTexts.EmailNotDelivered(details.EmailNotDeliveredOrders), PanelFramePaths.Orders + "?pendingWork=EmailUndeliveredMark"));
        }

        if (details.CardRefundFailedOrders > 0)
        {
            warnings.Add(new HomeWarningView("home-warning-card-refund-failed", PanelHomeTexts.CardRefundFailed(details.CardRefundFailedOrders), PanelFramePaths.Orders + "?pendingWork=AwaitingReturnOrRefund"));
        }

        if (details.CardRefundOutcomePendingOrders > 0)
        {
            warnings.Add(new HomeWarningView("home-warning-card-refund-pending", PanelHomeTexts.CardRefundOutcomePending(details.CardRefundOutcomePendingOrders), PanelFramePaths.Orders + "?pendingWork=AwaitingReturnOrRefund"));
        }

        Warnings = warnings;
        Checklist = home.SetupChecklist is { } items ? ChecklistOf(items) : [];
        Counters = PendingCounters.Of(home.Counters);

        DateTimeOffset now = _time.GetUtcNow();
        Awaiting = AwaitingCustomerList.Of(
            details.AwaitingIban.Select(row => new AwaitingCustomerRow(
                row.OrderNumber,
                PanelFramePaths.Orders + "/" + row.OrderId.ToString("D"),
                "IBAN bekleniyor",
                row.RefundLastDay is { } lastDay
                    ? new PeriodIndicator("İade süresi", CountedPeriod.RefundPeriod, PeriodStanding.Of(CountedPeriod.RefundPeriod, lastDay, now, BusinessCalendar.Product))
                    : null,
                row.ItemName ?? PanelHomeTexts.LatePaymentRefund,
                PanelHomeTexts.IbanReason(row.Reason),
                row.RequestedAt)));
        return Page();
    }

    // The open items, each with its screen; under the notice item - or under the cookie policy once the notice is done - the line that says the entrances that collect data stay closed until both texts are published (04 9.3.4; K-850).
    private static List<HomeChecklistView> ChecklistOf(IReadOnlyList<SetupChecklistItem> items)
    {
        bool noticeOpen = items.Any(i => !i.IsDone && i.Kind == SetupChecklistItemKind.PrivacyNotice);
        return
        [
            .. items.Where(i => !i.IsDone).Select(i => new HomeChecklistView(
                ChecklistText(i),
                PanelHomeTexts.ChecklistPath(i.Kind),
                i.Kind == SetupChecklistItemKind.PrivacyNotice || (i.Kind == SetupChecklistItemKind.CookiePolicy && !noticeOpen) ? PanelHomeTexts.ClosedEntrances : null)),
        ];
    }

    private static string ChecklistText(SetupChecklistItem item) =>
        item.Kind == SetupChecklistItemKind.CompanyIdentity && item.MissingFields.Count > 0
            ? PanelHomeTexts.ChecklistName(item.Kind) + " — eksik: " + string.Join(", ", item.MissingFields.Select(LegalTextsModel.LabelOf))
            : PanelHomeTexts.ChecklistName(item.Kind);

    // The screen the closed sales warning leads to: the first missing condition's own setting (04 3.4.3).
    private static string WarningPathOf(SalesGate gate)
    {
        SalesGateCondition first = gate.UnmetConditions.First(c => c != SalesGateCondition.TemporaryClosure);
        return first switch
        {
            SalesGateCondition.CompanyIdentity => PanelHomeTexts.ChecklistPath(SetupChecklistItemKind.CompanyIdentity),
            SalesGateCondition.PaymentMethod => PanelHomeTexts.ChecklistPath(SetupChecklistItemKind.PaymentMethod),
            SalesGateCondition.PrivacyNotice => PanelHomeTexts.ChecklistPath(SetupChecklistItemKind.PrivacyNotice),
            SalesGateCondition.CookiePolicy => PanelHomeTexts.ChecklistPath(SetupChecklistItemKind.CookiePolicy),
            _ => PanelHomeTexts.ChecklistPath(SetupChecklistItemKind.ReturnAddress),
        };
    }
}
