using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The form of UC-161 (07 5.2 <c>ReorderContentInput</c>): the record's type (the address's <c>{tip}</c>) and key, the direction and the orders of the two records that change places as the list showed them
/// (<c>Original</c> fields, 07 2.4.3; K-1063). It carries no version: the order is not a field of the record's form (06 2.6.5).
/// </summary>
public sealed class ReorderContentInput : IFormInput
{
    [SpecAssignedName]
    public string? ContentKind { get; set; }

    [SpecAssignedName]
    public Guid ContentId { get; set; }

    [SpecAssignedName]
    public PanelContentMove? Direction { get; set; }

    [SpecAssignedName]
    public string? OriginalSortOrder { get; set; }

    [SpecAssignedName]
    public string? OriginalAdjacentSortOrder { get; set; }
}

/// <summary>
/// E-41's list (T534; 04 9.13.2, 9.13.3; 07 UC-96, UC-161): the seven groups in their order, each with all its records in the firm's manual order - never paged (K-843) - and, outside Hakkımızda and the
/// announcement, the "yukarı" and "aşağı" forms that carry the orders of the two records that change places. Each group has its own "Yeni" button; the two single records open from their own row. The
/// GET writes nothing. A move whose orders are no longer the shown ones writes nothing and the list is shown as it is now, with the lasting message in its place (409; K-1193 (4)).
/// </summary>
[NoStore]
internal sealed class ContentListModel : PageModel
{
    public const string Title = "Kurumsal içerik";

    public const string MenuNamePrefix = "Menüdeki adı: ";

    public const string UpLabel = "Yukarı";

    public const string DownLabel = "Aşağı";

    public const string NewButtonLabel = "Yeni";

    public const string AboutEmptyText = "Hakkımızda henüz yazılmadı.";

    public const string AnnouncementEmptyText = "Duyuru henüz yazılmadı.";

    /// <summary>04 2.10.2 on a list: the order changed while the screen was open; nothing was moved.</summary>
    public const string OrderChangedText = "Sıra siz bakarken değişti; kaydırma uygulanmadı. Listenin güncel sırası aşağıda.";

    /// <summary>07 UC-161 422: the record is at the edge of its group or has no manual order.</summary>
    public const string NotMovableText = "Bu kayıt bu yönde kaydırılamaz.";

    public const string NotShownNowText = "Yayında; ancak tarih aralığının dışında olduğu için sitede şu an görünmüyor.";

    public const string HomepageNoteText = "ana sayfada sıradaki ilk üç kayıt görünür";

    private readonly ICorporateContentModule _content;
    private readonly ITransactionRunner _transactions;
    private readonly RejectionLog _rejections;

    public ContentListModel(ICorporateContentModule content, ITransactionRunner transactions, RejectionLog rejections)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(rejections);
        _content = content;
        _transactions = transactions;
        _rejections = rejections;
    }

    public PanelContentList List { get; private set; } = new([]);

    /// <summary>The lasting message of a refused move (409, 422); null otherwise.</summary>
    public MessageView? Refusal { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        List = await _content.GetPanelContentListAsync(ct);
        return Page();
    }

    /// <summary>UC-161: moves a record one place in its group; written only while both records still carry the shown orders. 409 shows the list as it is now; 422 is the edge or a type with no order; 404 is no type or no record.</summary>
    public async Task<IActionResult> OnPostReorderContentAsync(ReorderContentInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!ContentRoutes.TryParse(input.ContentKind, out PanelContentType type) || input.Direction is not { } move || !Enum.IsDefined(move))
        {
            return NotFound();
        }

        // The shown orders are read as text (07 2.4.3): a value that is no shown number is no match - the order is no longer the one the screen showed.
        bool readable = ShownValue.TryReadNumber(input.OriginalSortOrder, out int? shown) & ShownValue.TryReadNumber(input.OriginalAdjacentSortOrder, out int? neighbour);
        PanelContentReorderResult result = readable && shown is { } own && neighbour is { } adjacent
            ? await _transactions.RunAsync(token => _content.ReorderPanelContentAsync(type, input.ContentId, move, own, adjacent, token), ct)
            : await _transactions.RunAsync(token => ExistsOrNotFoundAsync(type, input.ContentId, token), ct);
        if (!result.IsRejected)
        {
            return FormResults.SeeOther(PanelFramePaths.CorporateContent);
        }

        if (result.Outcome == PanelContentReorderOutcome.NotFound)
        {
            return NotFound();
        }

        List = await _content.GetPanelContentListAsync(ct);
        if (result.Outcome == PanelContentReorderOutcome.OrderChanged)
        {
            _rejections.Rejected(ErrorCode.DisplayedValueConflict);
            Refusal = Messages.InPlace(MessageMeaning.Warning, OrderChangedText, "content-order-changed");
            return FormResults.Conflict(this);
        }

        _rejections.Rejected(ErrorCode.OperationNotAvailable);
        Refusal = Messages.InPlace(MessageMeaning.Error, NotMovableText, "content-not-movable");
        return FormResults.Redisplay(this);
    }

    // An unreadable shown order never matches: the record must exist for it to be a 409 rather than a 404.
    private async Task<PanelContentReorderResult> ExistsOrNotFoundAsync(PanelContentType type, Guid id, CancellationToken ct)
    {
        PanelContentForm? form = await _content.GetPanelContentFormAsync(type, id, ct);
        return PanelContentReorderResult.Of(form is null ? PanelContentReorderOutcome.NotFound : PanelContentReorderOutcome.OrderChanged);
    }
}
