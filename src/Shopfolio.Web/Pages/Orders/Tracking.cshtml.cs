using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Pages.Orders;

/// <summary>
/// E-15 — the order tracking entry (07 UC-31, UC-32; 04 5.15; T511). The GET gives the form and the reminder of the two other ways (the e-mail's link, the member's history). The
/// <c>FindOrder</c> handler counts the attempt on both axes of L-5 - the e-mail written in the query and the client address - before it works; over either threshold nothing is looked
/// up, the attempt is forgotten and the page comes back with the neutral limit message beside the button and the status 429. A match forgets the attempt and answers 303 to the
/// access-key root - the guest's only way (07 2.3.4); the key never appears as text. A miss keeps the attempt row (L-5 counts failures; 06 4.43) and shows the neutral message at the
/// top of the form, which says neither which field was wrong nor whether the order exists (K-772). The page is closed to search engines by its address
/// (<see cref="Infrastructure.SearchEngine.SearchEngineClosure"/>).
/// </summary>
public sealed class TrackingModel : PageModel
{
    /// <summary>The address of the page (07 UC-31); the frame's "Sipariş takibi" link is the same constant.</summary>
    public const string Path = Frame.FramePaths.OrderTracking;

    /// <summary>The neutral text of a query that matched nothing (K-772; 04 5.15.4): the same sentence whichever field was wrong and whether or not the order exists.</summary>
    public const string NotMatchedText = "Bu sipariş numarası ve e-posta adresiyle eşleşen bir sipariş bulunamadı. Bilgileri kontrol edip yeniden deneyin.";

    private readonly IOrderingModule _ordering;

    public TrackingModel(IOrderingModule ordering)
    {
        _ordering = ordering;
    }

    public OrderLookupInput Input { get; private set; } = new();

    /// <summary>The fields in the order of the form, for the error summary.</summary>
    public IReadOnlyList<string> FormOrder { get; } = [$"{nameof(Input)}.{nameof(OrderLookupInput.OrderNumber)}", $"{nameof(Input)}.{nameof(OrderLookupInput.Email)}"];

    /// <summary>True after a well-formed query that matched no order: the neutral message stands at the top of the form.</summary>
    public bool IsNotMatched { get; private set; }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostFindOrderAsync([Bind(Prefix = nameof(Input))] OrderLookupInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        Input = input;
        if (!ModelState.IsValid)
        {
            return FormResults.Redisplay(this);
        }

        CancellationToken ct = HttpContext.RequestAborted;
        AttemptCounter counter = HttpContext.RequestServices.GetRequiredService<AttemptCounter>();
        AttemptSubject byEmail = AttemptSubject.Email(input.Email);
        AttemptSubject byIp = AttemptSubject.Ip(HttpContext);
        Guid emailAttempt = await counter.RecordAsync(AttemptLimit.L5, byEmail, ct);
        Guid ipAttempt = await counter.RecordAsync(AttemptLimit.L5, byIp, ct);

        // The two axes are counted apart (07 UC-32; 03 6.1.1.5): either one over its threshold refuses the query.
        if (await counter.IsExceededAsync(AttemptLimit.L5, byEmail, ct) || await counter.IsExceededAsync(AttemptLimit.L5, byIp, ct))
        {
            await counter.ForgetAsync(emailAttempt, ct);
            await counter.ForgetAsync(ipAttempt, ct);
            return FormResults.AttemptLimitExceeded(this, ErrorCode.OrderLookupLimitExceeded);
        }

        OrderAccessLookup? found = await _ordering.FindOrderByNumberAndEmailAsync(input.OrderNumber, input.Email, ct);
        if (found is null)
        {
            // The failed query stays in the attempt record (L-5); the rejection line names the code, the page names nothing (09 8.2 item 3).
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.OrderLookupFailed);
            IsNotMatched = true;
            return FormResults.Redisplay(this);
        }

        await counter.ForgetAsync(emailAttempt, ct);
        await counter.ForgetAsync(ipAttempt, ct);
        return FormResults.SeeOther(OrderRoutes.AccessPath(found.AccessKey));
    }
}

/// <summary>The query of the tracking form (04 7.1.6): both fields required, the e-mail in its form (7.3.2.1). The number is matched without regard to letter case by the lookup.</summary>
public sealed class OrderLookupInput : IFormInput
{
    [Display(Name = "Sipariş numarası")]
    [FieldRequired]
    public string OrderNumber { get; set; } = string.Empty;

    [Display(Name = "E-posta")]
    [FieldRequired]
    [InvalidEmail]
    public string Email { get; set; } = string.Empty;
}
