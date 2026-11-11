using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>Which of the reset's three steps the page shows (07 9.1.1; 04 9.1.4, 9.1.5, 9.1.12).</summary>
internal enum ResetStep
{
    /// <summary>The request form, or - after the request - the neutral sentence in its place.</summary>
    Request,

    /// <summary>The new password form: the link is valid.</summary>
    Set,

    /// <summary>The password is set, every session is closed, the way back to the sign-in (the link is used and the address carries the result code).</summary>
    Completed,

    /// <summary>The link is used, run out, replaced or unknown: the setting step is not opened and the page leads to the request step.</summary>
    Invalid,
}

/// <summary>
/// The panel's password reset: the request (UC-111, L-2) and the setting of the new password (UC-112) on one page, three addresses (T520; 07 9.1; 04 E-29 9.1.4, 9.1.5). The rules are the customer's
/// (02 10.2.4): the same lifetimes, the same policy, the same limits. The request answers the same whether the address is an administrator's or not; the e-mail is queued, in the same transaction as the
/// link, only for an administrator's address and goes to that address (02 10.2.5, 8.1.4). The setting consumes the link with one conditional update, closes every session of the account and opens no session
/// (K-1190 (1)): the administrator signs in with the new password. A link used, run out, replaced or unknown never opens the setting step (04 9.1.12). The key is the address's own and is never written on a page
/// except in the form's action.
/// </summary>
[NoStore]
internal sealed class ResetModel : PageModel
{
    private readonly IAdministrationModule _administration;
    private readonly INotificationsModule _notifications;
    private readonly ITransactionRunner _transactions;

    private bool _loaded;

    public ResetModel(IAdministrationModule administration, INotificationsModule notifications, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(transactions);
        _administration = administration;
        _notifications = notifications;
        _transactions = transactions;
    }

    public ResetStep Step { get; private set; } = ResetStep.Request;

    /// <summary>The key of the address, for the setting step's form; null in the request step.</summary>
    public string? Key { get; private set; }

    /// <summary>True when the address carries the result code of a request: the neutral sentence stands in the place of the form (04 9.1.4).</summary>
    public bool RequestSent { get; private set; }

    /// <summary>True after a 429: the neutral limit sentence stands beside the button (04 9.1.9).</summary>
    public bool LimitReached { get; private set; }

    /// <summary>The e-mail the request form shows again after a 422 or a 429.</summary>
    public string? SentEmail { get; private set; }

    /// <summary>
    /// A form whose fields break their rules is answered 422 by the validation filter without the handler (09 3.4 item 3): the setting step it shows again needs the link as it stands, and the request
    /// step needs the sent address.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity })
        {
            _loaded = true;
            if (context.HandlerArguments.Values.OfType<RequestPasswordResetInput>().FirstOrDefault() is { } request)
            {
                SentEmail = request.Email;
                return;
            }

            if (RouteData.Values["key"] is string key)
            {
                await LoadStepAsync(key, HttpContext.RequestAborted);
            }
        }
    }

    public async Task<IActionResult> OnGetAsync(string? key, CancellationToken ct)
    {
        _loaded = true;
        if (string.IsNullOrEmpty(key))
        {
            RequestSent = NoticeParameter.Read(Request, NoticeKind.Result) == NoticeCode.PasswordResetRequested;
            return Page();
        }

        await LoadStepAsync(key, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostRequestPasswordResetAsync(RequestPasswordResetInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        SentEmail = input.Email;
        string entered = input.Email ?? string.Empty;
        System.Net.IPAddress? address = ClientAddress.Of(HttpContext);

        // The link and the e-mail row are written in one transaction (08 2.2.8). The e-mail goes only when a link was made - an administrator's address - and to that address; the answer is the same
        // either way, and every request counts for L-2 inside the module (02 8.1.4; 07 9.1.3).
        ResetRequested result = await _transactions.RunAsync(
            async token =>
            {
                ResetRequested requested = await _administration.RequestPasswordResetAsync(entered, address, token);
                if (requested is { IsIssued: true, AccountId: { } accountId, LinkId: { } linkId })
                {
                    await _notifications.QueueAccountEmailAsync(new PasswordResetLinkEmail(DispatchRelatedRecordType.Admin, accountId, linkId, entered, false), token);
                }

                return requested;
            },
            ct);

        if (result.Code == ErrorCode.PasswordResetLimitExceeded)
        {
            LimitReached = true;
            return FormResults.AttemptLimitExceeded(this, ErrorCode.PasswordResetLimitExceeded);
        }

        return FormResults.SeeOther(PanelFramePaths.PasswordReset, NoticeCode.PasswordResetRequested);
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(string? key, ResetPasswordInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        if (string.IsNullOrEmpty(key))
        {
            return AccessDenial.NotFound();
        }

        Key = key;
        string? presentedMark = BrowserCookies.ReadKnownDevice(HttpContext);
        ResetOutcome outcome = await _transactions.RunAsync(async token => await _administration.ResetPasswordAsync(key, input.NewPassword ?? string.Empty, presentedMark, token), ct);

        if (outcome is { IsDone: true, MarkValue: { } mark })
        {
            // The reset opens no session; the browser that made it becomes the account's known browser (03 6.1.3.3; 06 4.41).
            BrowserCookies.WriteKnownDevice(HttpContext, mark, outcome.MarkLifetime);
            return FormResults.SeeOther(PanelFramePaths.PasswordReset + "/" + Uri.EscapeDataString(key), NoticeCode.PasswordResetCompleted);
        }

        if (outcome.Code is { } refusal)
        {
            // The policy refused the new password and the link was not consumed (07 9.1.4 422).
            ModelState.AddFieldError("Input.NewPassword", refusal.ToString(), refusal == ErrorCode.PasswordTooWeak ? DisplayFormat.Number(ProductConstants.PasswordMinLength) : null);
            Step = ResetStep.Set;
            return FormResults.Redisplay(this);
        }

        // The link was spent between the page and the button: nothing was written and the page tells the link is not valid, 200 (07 9.1.4; 2.5.3).
        Step = ResetStep.Invalid;
        return Page();
    }

    // The step the link allows: a valid link opens the setting; a used one with the result code is the completed state; every other state - used without the code, run out, replaced, unknown - is the invalid state.
    private async Task LoadStepAsync(string key, CancellationToken ct)
    {
        Key = key;
        AccountLinkReading reading = await _administration.ReadPasswordResetLinkAsync(key, ct);
        Step = reading.Status switch
        {
            AccountLinkStatus.Valid => ResetStep.Set,
            AccountLinkStatus.Used when NoticeParameter.Read(Request, NoticeKind.Result) == NoticeCode.PasswordResetCompleted => ResetStep.Completed,
            _ => ResetStep.Invalid,
        };
    }
}

/// <summary>The form of UC-111 (07 9.1.3; 04 7.2.1.4): the e-mail.</summary>
public sealed class RequestPasswordResetInput : IFormInput
{
    [Display(Name = "E-posta")]
    [FieldRequired]
    [InvalidEmail]
    public string? Email { get; set; }
}

/// <summary>The form of UC-112 (07 9.1.4; 04 7.2.1.5): the new password.</summary>
public sealed class ResetPasswordInput : IFormInput
{
    [Display(Name = "Yeni şifre")]
    [PasswordField]
    [FieldRequired]
    [PasswordTooWeak]
    public string? NewPassword { get; set; }
}
