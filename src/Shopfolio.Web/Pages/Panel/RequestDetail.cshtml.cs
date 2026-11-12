using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// E-39, the contact request's screen (T532; 04 9.11; 07 UC-94, UC-155, UC-156): the topic, the status, the sender, the whole message, the moment the request reached the firm, the last closing and the one
/// operation - close an open request, reopen a closed one. The GET writes nothing; each POST is a named handler that changes the status in one transaction and answers 303 with the short-lived notice. A
/// request that is not there - never was, or destroyed at the end of its retention - answers 404 on the three handlers (K-1192).
/// </summary>
public sealed class RequestDetailModel : PageModel
{
    public const string CloseLabel = "Talebi kapat";

    public const string ReopenLabel = "Yeniden aç";

    public const string BackLabel = "Listeye dön";

    public const string SearchOrdersLabel = "Siparişlerde ara";

    public const string SearchMembersLabel = "Üye kaydında ara";

    public const string ReceivedLabel = "Ulaştığı an";

    public const string LastClosedLabel = "Son kapatılış";

    /// <summary>The line beside the last closing of a request that is open again: reopening does not stop the retention period (04 9.11.3; K-1099).</summary>
    public const string RetentionKeepsRunningText = "Saklama süresi bu tarihten işlemeye devam ediyor; talep açık dururken de sürenin sonunda imha edilir.";

    /// <summary>The line beside the last closing of a closed request: the retention period runs from it (02 3.32.4).</summary>
    public const string RetentionRunsText = "Saklama süresi bu tarihten işler.";

    private readonly ICorporateContentModule _content;

    public RequestDetailModel(ICorporateContentModule content)
    {
        ArgumentNullException.ThrowIfNull(content);
        _content = content;
    }

    public ContactRequestDetail Detail { get; private set; } = null!; // non-null: the GET sets it before the view renders; a missing request answers 404 first

    public Badge StatusBadge { get; private set; } = null!; // non-null: the GET sets it before the view renders; a missing request answers 404 first

    public string TopicText { get; private set; } = string.Empty;

    public string ReceivedText { get; private set; } = string.Empty;

    /// <summary>The last closing and what it means for the retention period; null when the request was never closed.</summary>
    public string? LastClosedText { get; private set; }

    public string? RetentionText { get; private set; }

    public string ActionLabel { get; private set; } = string.Empty;

    /// <summary>The handler of the one operation: <c>CloseContactRequest</c> for an open request, <c>ReopenContactRequest</c> for a closed one.</summary>
    public string ActionHandler { get; private set; } = string.Empty;

    public string ActionPath { get; private set; } = string.Empty;

    public string BackPath { get; private set; } = PanelFramePaths.Requests;

    public string SearchOrdersPath { get; private set; } = string.Empty;

    public string SearchMembersPath { get; private set; } = string.Empty;

    /// <summary>The short-lived notice the redirect after a close or a reopening carries (07 6.3.1); null when the request has none.</summary>
    public MessageView? Notice { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        ContactRequestDetail? found = await _content.FindContactRequestAsync(id, ct);
        if (found is null)
        {
            return NotFound();
        }

        Show(found);
        NoticeCode? notice = NoticeParameter.Read(HttpContext.Request, NoticeKind.ShortLived);
        Notice = notice is { } code && NoticeTexts.TryShortLived(code, out string? text) ? Messages.ShortLivedSuccess(text) : null;
        return Page();
    }

    /// <summary>UC-155: closes an open contact request; a request already closed is not touched (303 all the same).</summary>
    public async Task<IActionResult> OnPostCloseContactRequestAsync(Guid id, CancellationToken ct)
    {
        Guid adminId = AdminId();
        ContactRequestChangeResult result = await Transactions.RunAsync(token => _content.CloseContactRequestAsync(id, adminId, token), ct);
        return AnswerOf(id, result, NoticeCode.ContactRequestClosed);
    }

    /// <summary>UC-156: reopens a closed contact request; the last closing moment stays, so the retention period keeps running (K-1099).</summary>
    public async Task<IActionResult> OnPostReopenContactRequestAsync(Guid id, CancellationToken ct)
    {
        Guid adminId = AdminId();
        ContactRequestChangeResult result = await Transactions.RunAsync(token => _content.ReopenContactRequestAsync(id, adminId, token), ct);
        return AnswerOf(id, result, NoticeCode.ContactRequestReopened);
    }

    private static IActionResult AnswerOf(Guid id, ContactRequestChangeResult result, NoticeCode notice)
    {
        string path = PathOf(id);
        return result.Outcome switch
        {
            ContactRequestChangeOutcome.Changed => FormResults.SeeOther(path, notice),
            ContactRequestChangeOutcome.Unchanged => FormResults.SeeOther(path),
            _ => new NotFoundResult(),
        };
    }

    private ITransactionRunner Transactions => HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();

    private Guid AdminId() =>
        User.AccountId() ?? throw new InvalidOperationException("A panel request carries the administrator's session (the panel filter refuses the others).");

    private static string PathOf(Guid id) => $"{PanelFramePaths.Requests}/{id:D}";

    private void Show(ContactRequestDetail detail)
    {
        Detail = detail;
        StatusBadge = StatusBadges.For(detail.Status);
        TopicText = RequestTexts.TopicOf(detail.Topic);
        ReceivedText = DisplayFormat.DateAndTime(detail.CreatedAt);
        if (detail.LastClosedAt is { } closedAt)
        {
            LastClosedText = DisplayFormat.DateAndTime(closedAt);
            RetentionText = detail.Status == ContactRequestStatus.Open ? RetentionKeepsRunningText : RetentionRunsText;
        }

        bool isOpen = detail.Status == ContactRequestStatus.Open;
        ActionLabel = isOpen ? CloseLabel : ReopenLabel;
        ActionHandler = isOpen ? "CloseContactRequest" : "ReopenContactRequest";
        ActionPath = PathOf(detail.Id);
        BackPath = ListReturn.Target(HttpContext.Request.Query[ReturnPath.ParameterName], PanelFramePaths.Requests);
        string encodedEmail = Uri.EscapeDataString(detail.Email);
        SearchOrdersPath = $"{PanelFramePaths.Orders}?search={encodedEmail}";
        SearchMembersPath = $"{PanelFramePaths.Members}?search={encodedEmail}";
    }
}
