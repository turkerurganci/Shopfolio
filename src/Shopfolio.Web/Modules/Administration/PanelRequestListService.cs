using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The panel's requests list (T532; 07 UC-93; 04 9.10; 05 3.3.9): the contact requests and the defect claims merged into one list. Each module gives its own filtered, ordered first rows - the Administration
/// module reads no other module's table - and this class cuts the page out of their merge. A page needs the first <c>page x size</c> rows of each kind, never the whole set (09 20 item 2); the item labels and the
/// "e-posta ulaşmadı" marks of the page's claims are one read each, so the number of reads does not grow with the page size (09 20 item 4).
/// </summary>
internal sealed class PanelRequestListService : IPanelRequestList
{
    private readonly ICorporateContentModule _content;
    private readonly IAfterSalesModule _afterSales;
    private readonly IOrderingModule _ordering;
    private readonly INotificationsModule _notifications;

    public PanelRequestListService(ICorporateContentModule content, IAfterSalesModule afterSales, IOrderingModule ordering, INotificationsModule notifications)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(afterSales);
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(notifications);
        _content = content;
        _afterSales = afterSales;
        _ordering = ordering;
        _notifications = notifications;
    }

    public async Task<PanelRequestPage> ListAsync(
        IReadOnlyCollection<PanelRequestKind> kinds,
        IReadOnlyCollection<PanelRequestStatus> statuses,
        int requestedPage,
        int pageSize,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(kinds);
        ArgumentNullException.ThrowIfNull(statuses);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        // "Kapatıldı" fits only a contact request and "Çözüldü" only a defect claim (04 9.10.3): a kind none of the named statuses fits is not read at all.
        ContactRequestStatus[] contactStatuses = ContactStatusesOf(statuses);
        DefectClaimStatus[] defectStatuses = DefectStatusesOf(statuses);
        bool readsContact = (kinds.Count == 0 || kinds.Contains(PanelRequestKind.ContactRequest)) && (statuses.Count == 0 || contactStatuses.Length > 0);
        bool readsDefect = (kinds.Count == 0 || kinds.Contains(PanelRequestKind.DefectClaim)) && (statuses.Count == 0 || defectStatuses.Length > 0);

        int contactCount = readsContact ? await _content.CountPanelContactRequestsAsync(contactStatuses, ct) : 0;
        int defectCount = readsDefect ? await _afterSales.CountPanelDefectClaimsAsync(defectStatuses, ct) : 0;
        int total = checked(contactCount + defectCount);
        int pageCount = Paging.PageCountOf(total, pageSize);
        int page = Math.Clamp(requestedPage, PageNumber.First, pageCount);
        int skip = checked((int)((long)(page - 1) * pageSize));
        int take = checked(skip + pageSize);

        IReadOnlyList<PanelContactRequestRow> contacts = contactCount > 0 ? await _content.ListPanelContactRequestsAsync(contactStatuses, take, ct) : [];
        IReadOnlyList<PanelDefectClaimRow> claims = defectCount > 0 ? await _afterSales.ListPanelDefectClaimsAsync(defectStatuses, take, ct) : [];

        // Each source is already in the database's order; the stable sort keeps it inside a kind, and a contact request stands before a defect claim of the same moment.
        List<Entry> merged =
        [
            .. contacts.Select(c => new Entry(PanelRequestKind.ContactRequest, c.CreatedAt, c, null)),
            .. claims.Select(c => new Entry(PanelRequestKind.DefectClaim, c.OpenedAt, null, c)),
        ];
        List<Entry> shown = [.. merged.OrderByDescending(e => e.At).ThenBy(e => e.Kind).Skip(skip).Take(pageSize)];

        PanelDefectClaimRow[] shownClaims = [.. shown.Where(e => e.Claim is not null).Select(e => e.Claim!)];
        IReadOnlyDictionary<Guid, OrderItemLabel> labels = shownClaims.Length == 0
            ? new Dictionary<Guid, OrderItemLabel>()
            : (await _ordering.FindItemLabelsAsync([.. shownClaims.Select(c => c.OrderItemId)], ct)).ToDictionary(l => l.OrderItemId);
        IReadOnlySet<Guid> marked = shownClaims.Length == 0
            ? new HashSet<Guid>()
            : await _notifications.FindClaimsWithUndeliveredMarkAsync([.. shownClaims.Select(c => c.ClaimId)], ct);

        List<PanelRequestRow> rows = [];
        foreach (Entry entry in shown)
        {
            if (entry.Contact is { } contact)
            {
                rows.Add(new PanelRequestRow(
                    PanelRequestKind.ContactRequest, contact.Id, contact.CreatedAt, contact.CreatedAt, ToPanel(contact.Status), contact.Topic, contact.Name, contact.Email, null, null, null, null, false));
            }
            else if (entry.Claim is { } claim)
            {
                labels.TryGetValue(claim.OrderItemId, out OrderItemLabel? label);
                rows.Add(new PanelRequestRow(
                    PanelRequestKind.DefectClaim, claim.ClaimId, claim.CreatedAt, claim.OpenedAt, ToPanel(claim.Status), null, null, null,
                    label?.OrderId, label?.OrderNumber, label is null ? null : ItemNameOf(label), claim.Quantity, marked.Contains(claim.ClaimId)));
            }
        }

        return new PanelRequestPage(rows, page, pageCount, total);
    }

    /// <summary>The item's name as the list writes it: the product name the order recorded, then the option values the item was bought with in parentheses.</summary>
    internal static string ItemNameOf(OrderItemLabel label)
    {
        ArgumentNullException.ThrowIfNull(label);
        string[] options = [.. new[] { label.FirstOptionValue, label.SecondOptionValue }.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!)];
        return options.Length == 0 ? label.ProductName : $"{label.ProductName} ({string.Join(", ", options)})";
    }

    private static ContactRequestStatus[] ContactStatusesOf(IReadOnlyCollection<PanelRequestStatus> statuses) =>
        [.. statuses.Select(s => s switch
        {
            PanelRequestStatus.Open => (ContactRequestStatus?)ContactRequestStatus.Open,
            PanelRequestStatus.Closed => ContactRequestStatus.Closed,
            _ => null,
        }).Where(s => s is not null).Select(s => s!.Value).Distinct()];

    private static DefectClaimStatus[] DefectStatusesOf(IReadOnlyCollection<PanelRequestStatus> statuses) =>
        [.. statuses.Select(s => s switch
        {
            PanelRequestStatus.Open => (DefectClaimStatus?)DefectClaimStatus.Open,
            PanelRequestStatus.Resolved => DefectClaimStatus.Resolved,
            _ => null,
        }).Where(s => s is not null).Select(s => s!.Value).Distinct()];

    private static PanelRequestStatus ToPanel(ContactRequestStatus status) => status switch
    {
        ContactRequestStatus.Open => PanelRequestStatus.Open,
        ContactRequestStatus.Closed => PanelRequestStatus.Closed,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Not a contact request status."),
    };

    private static PanelRequestStatus ToPanel(DefectClaimStatus status) => status switch
    {
        DefectClaimStatus.Open => PanelRequestStatus.Open,
        DefectClaimStatus.Resolved => PanelRequestStatus.Resolved,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Not a defect claim status."),
    };

    private sealed record Entry(PanelRequestKind Kind, DateTimeOffset At, PanelContactRequestRow? Contact, PanelDefectClaimRow? Claim);
}
