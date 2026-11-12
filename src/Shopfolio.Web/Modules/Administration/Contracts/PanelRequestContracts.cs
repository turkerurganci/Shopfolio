using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>The two kinds of request the panel's requests list holds (T532; 07 5.3.2 <c>RequestQuery</c> <c>requestType</c>; 02 1.2): the names are the parameter's closed list.</summary>
public enum PanelRequestKind
{
    ContactRequest,
    DefectClaim,
}

/// <summary>
/// The statuses of the requests list's filter (T532; 07 5.3.2 <c>requestStatus</c>): <see cref="Open"/> is the open contact request and the open defect claim, <see cref="Closed"/> only a contact request
/// and <see cref="Resolved"/> only a defect claim (04 9.10.3). The names are the parameter's closed list.
/// </summary>
public enum PanelRequestStatus
{
    Open,
    Closed,
    Resolved,
}

/// <summary>
/// One row of the requests list (T532; 04 9.10.4). Both kinds share the columns: the kind, the moment (<see cref="OpenedAt"/>: when the contact request reached the firm, when the defect claim was opened or last
/// reopened) and the status, and <see cref="CreatedAt"/>, the first opening (equal to <see cref="OpenedAt"/> for a contact request and for a defect claim never reopened). A contact request fills the topic and the sender; a defect claim fills the order, the item's labels and the defective units, and carries the "e-posta ulaşmadı" mark when its
/// notice B-9 did not reach the customer - a contact request row carries none (04 2.5.4).
/// </summary>
public sealed record PanelRequestRow(
    PanelRequestKind Kind,
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset OpenedAt,
    PanelRequestStatus Status,
    ContactTopic? Topic,
    string? SenderName,
    string? SenderEmail,
    Guid? OrderId,
    string? OrderNumber,
    string? ItemName,
    int? DefectiveQuantity,
    bool EmailNotDelivered);

/// <summary>One page of the requests list: the rows, the page shown (a page past the last is the last, 07 2.6.2), the number of pages and the number of rows of the filter.</summary>
public sealed record PanelRequestPage(IReadOnlyList<PanelRequestRow> Rows, int PageNumber, int PageCount, int TotalCount);

/// <summary>
/// The panel's requests list (T532; 07 UC-93; 04 9.10): the contact requests and the defect claims in one list, newest first - the contact request by the moment it reached the firm, the defect claim by its
/// last opening - with the kind and status filters applied together. The Administration module reads the two kinds only through their modules' surfaces (05 3.3.9) and merges them.
/// </summary>
public interface IPanelRequestList
{
    /// <summary>
    /// One page of the list. <paramref name="kinds"/> and <paramref name="statuses"/> are the filter's valid values (none named: no filter on that axis); a kind that no named status fits is not read.
    /// <paramref name="requestedPage"/> is clamped into the range. The number of reads is the same for every page size: one count and one list per kind, one item-label read and one mark read for the page
    /// (09 20 item 4). A plain read.
    /// </summary>
    Task<PanelRequestPage> ListAsync(
        IReadOnlyCollection<PanelRequestKind> kinds,
        IReadOnlyCollection<PanelRequestStatus> statuses,
        int requestedPage,
        int pageSize,
        CancellationToken ct);
}
