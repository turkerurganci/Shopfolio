using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The query model of the requests list (E-38; 07 5.3.2 <c>RequestQuery</c>; UC-93): the request kind and the status, both repeating (07 2.6.4), and <c>page</c>. Every field is text so that a wrong value is never a
/// binding error (07 2.6.6): <see cref="RequestsModel"/> reads the closed lists and drops a value outside them. There is no search and no topic filter (04 9.10.7), and no sort (07 2.6.5).
/// </summary>
public sealed class RequestQuery : PagedQuery
{
    [SpecAssignedName]
    public string[]? RequestType { get; init; }

    [SpecAssignedName]
    public string[]? RequestStatus { get; init; }
}

/// <summary>One option of the list's filter (04 9.10.3): the parameter's value, its name and whether the request selected it.</summary>
public sealed record RequestFilterOption(string Value, string Label, bool IsSelected);

/// <summary>A selected filter value shown at the top of the list with the address that removes it (04 9.10.8; 2.13.3).</summary>
public sealed record RequestFilterChip(string Label, string RemovePath);

/// <summary>
/// E-38, the requests list (T532; 04 9.10; 07 UC-93): the contact requests and the defect claims in one list, newest first, with the kind and the status filters; a row opens the contact request's screen (E-39) or
/// the order's defect claims section (E-37), carrying the list's address in <c>returnUrl</c> (07 2.6.8). The GET writes nothing; the screen has no operation of its own (04 9.10.9).
/// </summary>
public sealed class RequestsModel : PageModel
{
    /// <summary>The sentence of a list with no request at all (04 9.10.12): what is not there; the requests come from the storefront, so there is no button that opens one.</summary>
    public const string NoRequestsText = "Henüz talep yok.";

    /// <summary>The sentence of a filter whose result is empty; its way out removes the filter.</summary>
    public const string NoMatchText = "Bu süzgece uyan talep yok.";

    public const string RemoveFilterLabel = "Süzgeci kaldır";

    public const string HomeLabel = "Ana sayfaya dön";

    private readonly IPanelRequestList _requests;

    public RequestsModel(IPanelRequestList requests)
    {
        ArgumentNullException.ThrowIfNull(requests);
        _requests = requests;
    }

    /// <summary>The table of the page's rows; null when the list is empty (the empty-state line stands in its place).</summary>
    public PanelTable? Table { get; private set; }

    public Pager Pager { get; private set; } = new(null, null, []);

    public EmptyStateView? Empty { get; private set; }

    public IReadOnlyList<RequestFilterOption> TypeOptions { get; private set; } = [];

    public IReadOnlyList<RequestFilterOption> StatusOptions { get; private set; } = [];

    /// <summary>The selected filter values, each with the address that removes only it.</summary>
    public IReadOnlyList<RequestFilterChip> Chips { get; private set; } = [];

    /// <summary>True when the request carries a filter that applies; a value outside the closed lists applies nothing (07 2.6.6).</summary>
    public bool IsFilterApplied { get; private set; }

    public string ListRoot => PanelFramePaths.Requests;

    public async Task<IActionResult> OnGetAsync(RequestQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        IReadOnlyList<PanelRequestKind> kinds = ListFilters.ReadCodes<PanelRequestKind>(query.RequestType);
        IReadOnlyList<PanelRequestStatus> statuses = ListFilters.ReadCodes<PanelRequestStatus>(query.RequestStatus);
        IsFilterApplied = kinds.Count > 0 || statuses.Count > 0;

        PanelRequestPage page = await _requests.ListAsync(kinds, statuses, query.RequestedPage(), ListCatalog.PageSizeOf(ListKind.PanelRequests), ct);
        string listAddress = ListAddress.Of(ListRoot, query, page.PageNumber);

        TypeOptions = [.. Enum.GetValues<PanelRequestKind>().Select(k => new RequestFilterOption(k.ToString(), RequestTexts.KindOf(k), kinds.Contains(k)))];
        StatusOptions = [.. Enum.GetValues<PanelRequestStatus>().Select(s => new RequestFilterOption(s.ToString(), RequestTexts.StatusOf(s), statuses.Contains(s)))];
        Chips =
        [
            .. kinds.Select(k => new RequestFilterChip(RequestTexts.KindOf(k), ListAddress.Of(ListRoot, Without(kinds.Where(x => x != k), statuses), PageNumber.First))),
            .. statuses.Select(s => new RequestFilterChip(RequestTexts.StatusOf(s), ListAddress.Of(ListRoot, Without(kinds, statuses.Where(x => x != s)), PageNumber.First))),
        ];

        if (page.Rows.Count == 0)
        {
            Empty = IsFilterApplied
                ? EmptyStates.WithExit(NoMatchText, RemoveFilterLabel, ListRoot)
                : EmptyStates.WithExit(NoRequestsText, HomeLabel, PanelFramePaths.Home);
            return Page();
        }

        Table = new PanelTable("Talepler", Columns, [.. page.Rows.Select(row => RowOf(row, listAddress))]);
        Pager = Pager.Of(ListRoot, query, page.PageNumber, page.PageCount);
        return Page();
    }

    private static readonly PanelTableColumn[] Columns =
    [
        new("Tür", true),
        new("Talep", true),
        new("E-posta / sipariş", false),
        new("Açılış tarihi", false),
        new("Durum", true),
        new("İşaret", true),
    ];

    private static RequestQuery Without(IEnumerable<PanelRequestKind> kinds, IEnumerable<PanelRequestStatus> statuses) =>
        new() { RequestType = [.. kinds.Select(k => k.ToString())], RequestStatus = [.. statuses.Select(s => s.ToString())] };

    private static PanelTableRow RowOf(PanelRequestRow row, string listAddress)
    {
        Badge badge = RequestTexts.BadgeOf(row.Kind, row.Status);
        bool isContact = row.Kind == PanelRequestKind.ContactRequest;
        string summary = isContact
            ? $"{RequestTexts.TopicOf(row.Topic ?? throw new InvalidOperationException("A contact request row names its topic."))} · {row.SenderName}"
            : $"{row.ItemName ?? "—"} · {row.DefectiveQuantity} adet ayıplı";
        string detail = isContact ? row.SenderEmail ?? string.Empty : $"Sipariş {row.OrderNumber ?? "—"}";
        string opened = OpenedTextOf(row);
        string? path = isContact
            ? ListReturn.LinkTo($"{PanelFramePaths.Requests}/{row.Id}", listAddress)
            : row.OrderId is { } orderId ? ListReturn.LinkTo($"{PanelFramePaths.Orders}/{orderId}", listAddress) + DefectClaimFragmentOf(row.Id) : null;
        return new PanelTableRow(
            [RequestTexts.KindOf(row.Kind), summary, detail, opened, badge.Text, row.EmailNotDelivered ? RequestTexts.EmailNotDeliveredMark : string.Empty],
            path,
            new Dictionary<int, Badge> { [4] = badge });
    }

    /// <summary>The fragment of the order page's defect claims section that focuses the claim's row (07 UC-93: the part identifier of the address, not part of the request).</summary>
    public static string DefectClaimFragmentOf(Guid claimId) => "#ayip-talebi-" + claimId.ToString("D");

    // A defect claim reopened since it was first opened shows its last opening beside the first (04 9.10.4).
    private static string OpenedTextOf(PanelRequestRow row) =>
        row.OpenedAt > row.CreatedAt
            ? $"{DisplayFormat.DateAndTime(row.CreatedAt)} · son açılış {DisplayFormat.DateAndTime(row.OpenedAt)}"
            : DisplayFormat.DateAndTime(row.CreatedAt);
}
