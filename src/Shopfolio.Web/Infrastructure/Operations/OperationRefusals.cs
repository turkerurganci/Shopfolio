using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;

namespace Shopfolio.Infrastructure.Operations;

/// <summary>
/// The answer to a refused operation, by the operation's class (07 8.1.3; 09 8.2 item 3, 8.3 item 4). Nothing was written; the answer is the page's, never an
/// error page. The class decides the answer, the rule does not:
/// <list type="bullet">
/// <item>The customer's operation page (cancellation, late-delivery termination, withdrawal, defect claim): 303 to the order page with the reason in the
/// <c>notice</c> parameter - <see cref="NoticeCode.OperationClosed"/> or <see cref="NoticeCode.QuantityExceedsOpen"/> (07 6.3.3; K-1170 (1), (4)); the order page says the reason in the
/// operation's place and shows the order as it is.</item>
/// <item>Every other operation (the panel's operations on a record with the version equal, a member's account operation): 422, the page shown again with the sent values and a message
/// at the level of the action (07 2.7.1; <see cref="ErrorCode.OperationNotAvailable"/>, <see cref="ErrorCode.QuantityExceedsOpen"/>). The text is the screen's - the reason is in the endpoint's
/// definition - so the caller passes it; this class invents none.</item>
/// </list>
/// The panel's order operation whose version changed is the conflict (<see cref="FormResults.Conflict"/>, 409) and is decided before the rule is asked. Each refusal writes its one
/// rejection line with the code (07 6.1.3; <see cref="RejectionLog"/>): the customer's "closed" is <see cref="ErrorCode.OperationNotAvailable"/> in the line, because the notice code
/// is not an error code.
/// </summary>
internal static class OperationRefusals
{
    /// <summary>The error code of a refused verdict: the code of the line and of the 422 answer.</summary>
    public static ErrorCode CodeOf(OperationDecision decision) => decision switch
    {
        OperationDecision.Closed => ErrorCode.OperationNotAvailable,
        OperationDecision.QuantityExceedsOpen => ErrorCode.QuantityExceedsOpen,
        _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, "An allowed operation is not refused."),
    };

    /// <summary>The reason code of the customer's return to the order page.</summary>
    public static NoticeCode NoticeOf(OperationDecision decision) => decision switch
    {
        OperationDecision.Closed => NoticeCode.OperationClosed,
        OperationDecision.QuantityExceedsOpen => NoticeCode.QuantityExceedsOpen,
        _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, "An allowed operation is not refused."),
    };

    /// <summary>303 to the customer's order page with the reason (07 8.1.3).</summary>
    public static IActionResult ToOrderPage(PageModel page, OperationVerdict verdict, string orderPagePath)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(orderPagePath);
        ErrorCode code = CodeOf(verdict.Decision);
        Log(page, code);
        return FormResults.SeeOther(orderPagePath, NoticeOf(verdict.Decision));
    }

    /// <summary>422: the page again with the sent values and <paramref name="message"/> at the level of the action (07 2.7.1, 8.1.3).</summary>
    public static IActionResult Redisplay(PageModel page, OperationVerdict verdict, string message)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ErrorCode code = CodeOf(verdict.Decision);
        Log(page, code);
        page.ModelState.AddModelError(string.Empty, message);
        return FormResults.Redisplay(page);
    }

    private static void Log(PageModel page, ErrorCode code) =>
        page.HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(code);
}
