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
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Panel;

namespace Shopfolio.Pages.Storefront;

/// <summary>Which of the reset's states the page shows (07 4.23; 04 5.23.3, 5.23.4, 5.23.8).</summary>
internal enum PasswordResetStep
{
    /// <summary>The request form, or - after the request - the neutral sentence in its place.</summary>
    Request,

    /// <summary>The new password form of a valid link; the account has a password.</summary>
    Set,

    /// <summary>The new password form of a valid link; the account has no password (a Google account): the step names the password-setting (02 3.13.8).</summary>
    SetPassword,

    /// <summary>The password is set and every session is closed: the link is used and the address carries the result code.</summary>
    Completed,

    /// <summary>The link is used, run out, replaced or unknown: the setting step is not opened and the page leads to the request step.</summary>
    Invalid,
}

/// <summary>
/// The customer's password reset: the request (UC-56, UC-57; L-2) and the setting of the new password (UC-58, UC-59) on one page, two addresses (T516; 07 4.23; 04 E-23 5.23). The rules are the one
/// password rule's (02 3.13.16): the same lifetimes, the same policy, the same limits as the panel's reset. The request answers the same whether or not the address has a verified customer account; the link and the
/// e-mail row are written in one transaction and only for such an account (an unverified registration and an unknown address get none, 02 8.1.4; 07 UC-57). A member who is signed in sees the account's own
/// address in the field - the address does not carry it. The setting consumes the link with one conditional update, closes every session of the account and opens no session (02 3.13.10): the person signs in with
/// the new password. A link used, run out, replaced or unknown never opens the setting step; the key is another type's link or an unknown key is the same invalid state (07 UC-58). The key is never written on a
/// page except in the form's action, no referrer leaves the page (09 15.1 item 4) and the response is not stored.
/// </summary>
[KeyedLinkPage]
[NoStore]
internal sealed class PasswordResetModel : PageModel
{
    private readonly IMembershipModule _membership;
    private readonly INotificationsModule _notifications;
    private readonly ITransactionRunner _transactions;

    private bool _loaded;

    public PasswordResetModel(IMembershipModule membership, INotificationsModule notifications, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(transactions);
        _membership = membership;
        _notifications = notifications;
        _transactions = transactions;
    }

    public PasswordResetStep Step { get; private set; } = PasswordResetStep.Request;

    /// <summary>The key of the address, for the setting step's form; null in the request step.</summary>
    public string? Key { get; private set; }

    /// <summary>True when the address carries the result code of a request: the neutral sentence stands in the place of the form (04 5.23.3).</summary>
    public bool RequestSent { get; private set; }

    /// <summary>True after a 429: the neutral limit sentence stands beside the button (04 5.23.5).</summary>
    public bool LimitReached { get; private set; }

    /// <summary>The e-mail the request form shows: the sent value after a 422 or a 429, else the signed-in member's own address, else nothing (07 UC-56).</summary>
    public string? SentEmail { get; private set; }

    /// <summary>
    /// A form whose fields break their rules is answered 422 by the validation filter without the handler (09 3.4 item 3): the setting step it shows again needs the link as it stands, and the request step
    /// needs the sent address.
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

            // A signed-in member's own address fills the field (04 5.23.3); the address carries nothing, and the neutral sentence replaces the form after the request anyway.
            if (!RequestSent && StorefrontFrameBuilder.CustomerOf(HttpContext) is Guid customerId)
            {
                SentEmail = (await _membership.GetSignInMethodsAsync(customerId, ct))?.Email;
            }

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

        // The link and the e-mail row are written in one transaction (08 2.2.8). The e-mail goes only when a link was made - a verified customer account's address - and to that address; the answer is the same
        // either way, and every request counts for L-2 inside the module (02 8.1.4; 07 UC-57).
        ResetRequested result = await _transactions.RunAsync(
            async token =>
            {
                ResetRequested requested = await _membership.RequestPasswordResetAsync(entered, address, token);
                if (requested is { IsIssued: true, AccountId: { } accountId, LinkId: { } linkId })
                {
                    await _notifications.QueueAccountEmailAsync(new PasswordResetLinkEmail(DispatchRelatedRecordType.Customer, accountId, linkId, entered, false), token);
                }

                return requested;
            },
            ct);

        if (result.Code == ErrorCode.PasswordResetLimitExceeded)
        {
            LimitReached = true;
            return FormResults.AttemptLimitExceeded(this, ErrorCode.PasswordResetLimitExceeded);
        }

        return FormResults.SeeOther(FramePaths.PasswordReset, NoticeCode.PasswordResetRequested);
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
        ResetOutcome outcome = await _transactions.RunAsync(async token => await _membership.ResetPasswordAsync(key, input.NewPassword ?? string.Empty, presentedMark, token), ct);

        if (outcome is { IsDone: true, MarkValue: { } mark })
        {
            // The reset opens no session; the browser that made it becomes the account's known browser (03 6.1.3.3; 06 4.41).
            BrowserCookies.WriteKnownDevice(HttpContext, mark, outcome.MarkLifetime);
            return FormResults.SeeOther(FramePaths.PasswordReset + "/" + Uri.EscapeDataString(key), NoticeCode.PasswordResetCompleted);
        }

        if (outcome.Code is { } refusal)
        {
            // The policy refused the new password and the link was not consumed (07 UC-59 422).
            ModelState.AddFieldError("Input.NewPassword", refusal.ToString(), refusal == ErrorCode.PasswordTooWeak ? DisplayFormat.Number(ProductConstants.PasswordMinLength) : null);
            await LoadStepAsync(key, ct);
            return FormResults.Redisplay(this);
        }

        // The link was spent between the page and the button: nothing was written and the page tells the link is not valid, 200 (07 UC-59; 2.5.3).
        Step = PasswordResetStep.Invalid;
        return Page();
    }

    // The step the link allows: a valid link opens the setting (the heading depends on whether the account has a password); a used one with the result code is the completed state; every other state -
    // used without the code, run out, replaced, unknown - is the invalid state (07 UC-58).
    private async Task LoadStepAsync(string key, CancellationToken ct)
    {
        Key = key;
        AccountLinkReading reading = await _membership.ReadPasswordResetLinkAsync(key, ct);
        if (reading.Status == AccountLinkStatus.Valid && reading.AccountId is { } accountId)
        {
            SignInMethods? methods = await _membership.GetSignInMethodsAsync(accountId, ct);
            Step = methods is { HasPassword: false } ? PasswordResetStep.SetPassword : PasswordResetStep.Set;
            return;
        }

        Step = reading.Status == AccountLinkStatus.Used && NoticeParameter.Read(Request, NoticeKind.Result) == NoticeCode.PasswordResetCompleted
            ? PasswordResetStep.Completed
            : PasswordResetStep.Invalid;
    }
}
