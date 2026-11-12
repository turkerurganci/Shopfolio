using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Panel;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.Pages.Account;

/// <summary>
/// The customer's re-authentication screen (T516; 07 UC-60, UC-61, UC-62; 04 E-24 5.24; 02 3.13.13; 05 6.1.5): inside the account folder, so the customer's session is required. Three operations - the password
/// change, the e-mail change and the account deletion - and no other; the proof is the account's password, or Google when the account is linked to it (an account that has both shows both). Opening the screen
/// drops the session's earlier permission (07 UC-60; 05 6.1.5): the form that is about to be sent is the one that counts. The right password answers 303 to the account page with the operation and the single-use
/// value, and the session keeps one permission only; a wrong one is the field message and counts for L-1; over the limit is the neutral sentence and nothing is done. The Google route starts a flow that asks Google
/// for a fresh sign-in and does not count to L-1; its return is <c>/giris/google</c> (UC-51), which grants the permission or comes back here with a code. An operation outside the list answers 303 to the account page.
/// The response is personal and not stored.
/// </summary>
[NoStore]
internal sealed class ReauthenticationModel : PageModel
{
    private readonly IMembershipModule _membership;
    private readonly ISessionService _sessions;
    private readonly ITransactionRunner _transactions;
    private readonly GoogleSignInFlow _flow;

    private bool _loaded;

    public ReauthenticationModel(IMembershipModule membership, ISessionService sessions, ITransactionRunner transactions, GoogleSignInFlow flow)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(flow);
        _membership = membership;
        _sessions = sessions;
        _transactions = transactions;
        _flow = flow;
    }

    /// <summary>The operation the screen is for.</summary>
    public ReauthenticationOperation Operation { get; private set; } = ReauthenticationOperation.PasswordChange;

    /// <summary>The ways the account proves itself: the password field and the Google button follow from it (04 5.24.3).</summary>
    public SignInMethods Methods { get; private set; } = new(string.Empty, false, false);

    /// <summary>The lasting message of Google's failed or unreachable return (04 5.24.7); null when the address carries no such code.</summary>
    public MessageView? ResultMessage { get; private set; }

    /// <summary>True after a 429: the neutral limit sentence stands beside the button (04 5.24.6).</summary>
    public bool LimitReached { get; private set; }

    /// <summary>True after the 422 of the Google route: the account is not linked to Google or the Google application is not defined (07 UC-62).</summary>
    public bool GoogleNotOffered { get; private set; }

    /// <summary>
    /// A form whose fields break their rules (the password is required) is answered 422 by the validation filter without the handler: the screen it shows again still needs the operation and the account's ways.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity }
            && context.HandlerArguments.Values.OfType<ReauthenticateInput>().FirstOrDefault() is { } sent
            && ReauthenticationAddresses.TryParse(sent.ReauthenticatedOperation, out ReauthenticationOperation operation))
        {
            _loaded = true;
            Operation = operation;
            await LoadMethodsAsync(HttpContext.RequestAborted);
        }
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        _loaded = true;
        if (!ReauthenticationAddresses.TryParse(Request.Query[ReauthenticationAddresses.OperationParameter], out ReauthenticationOperation operation))
        {
            return NotAnOperation();
        }

        Operation = operation;
        if (!await LoadMethodsAsync(ct))
        {
            return FormResults.SeeOther(FramePaths.Account);
        }

        if (ReauthenticationTexts.ResultOf(NoticeParameter.Read(Request, NoticeKind.Result)) is { } told)
        {
            ResultMessage = Messages.InPlace(told.Meaning, told.Text, "reauth-result");
        }

        // Opening the screen drops the earlier permission: the form that is about to be sent is the one that counts (07 UC-60; 05 6.1.5).
        if (SessionOf() is { } session)
        {
            await _transactions.RunAsync(
                async token =>
                {
                    await _sessions.DropReauthenticationAsync(session.CustomerId, session.SessionId, token);
                    return new Dropped();
                },
                ct);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostReauthenticateAsync(ReauthenticateInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        if (!ReauthenticationAddresses.TryParse(input.ReauthenticatedOperation, out ReauthenticationOperation operation))
        {
            return NotAnOperation();
        }

        Operation = operation;
        if (SessionOf() is not { } session)
        {
            return AccessDenial.NotFound();
        }

        string? presentedMark = BrowserCookies.ReadKnownDevice(HttpContext);
        System.Net.IPAddress? address = ClientAddress.Of(HttpContext);

        // The failed attempt is counted by the module in a transaction of its own, so the refusal's rollback does not take it back (K-972).
        ReauthenticationResult result = await _transactions.RunAsync(
            async cancel => await _membership.ReauthenticateAsync(session.CustomerId, session.SessionId, operation, input.Password ?? string.Empty, address, presentedMark, cancel),
            ct);

        if (result.Succeeded && result.Value is { } value)
        {
            return FormResults.SeeOther(ReauthenticationAddresses.Granted(operation, value));
        }

        if (result.Code == ErrorCode.ReauthenticationOperationInvalid)
        {
            return NotAnOperation();
        }

        await LoadMethodsAsync(ct);
        if (result.Code == ErrorCode.SignInLimitExceeded)
        {
            LimitReached = true;
            return FormResults.AttemptLimitExceeded(this, ErrorCode.SignInLimitExceeded);
        }

        // The wrong password is the field's own message and counted for L-1 by the module (07 UC-61; 04 5.24.6).
        HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.ReauthenticationFailed);
        ModelState.AddModelError("Input.Password", ReauthenticationTexts.PasswordWrong);
        return FormResults.Redisplay(this);
    }

    public async Task<IActionResult> OnPostStartSocialReauthenticationAsync(StartSocialReauthenticationInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        if (!ReauthenticationAddresses.TryParse(input.ReauthenticatedOperation, out ReauthenticationOperation operation))
        {
            return NotAnOperation();
        }

        Operation = operation;
        if (!await LoadMethodsAsync(ct))
        {
            return FormResults.SeeOther(FramePaths.Account);
        }

        // The account is not linked to Google or the Google application is not defined: the route is not offered and nothing is done; the screen is shown again (07 UC-62 422, 8.1.3).
        if (!Methods.HasGoogleLink || !_membership.IsGoogleSignInOpen)
        {
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.OperationNotAvailable);
            GoogleNotOffered = true;
            return FormResults.Redisplay(this);
        }

        // The flow asks Google for a fresh sign-in; the operation and the start moment travel in the protected state (05 6.1.5). It does not count to L-1 (04 5.24.6).
        return FormResults.SeeOtherToGoogle(_flow.Start(HttpContext, GoogleFlowKind.Reauthentication, operation.ToString(), null));
    }

    // An operation outside the closed list, or none: nothing is done and the account page is the answer (07 UC-60 to UC-62; 07 6.2 ReauthenticationOperationInvalid).
    private IActionResult NotAnOperation()
    {
        HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.ReauthenticationOperationInvalid);
        return FormResults.SeeOther(FramePaths.Account);
    }

    private async Task<bool> LoadMethodsAsync(CancellationToken ct)
    {
        if (SessionOf() is not { } session || await _membership.GetSignInMethodsAsync(session.CustomerId, ct) is not { } methods)
        {
            return false;
        }

        Methods = methods;
        return true;
    }

    private (Guid CustomerId, Guid SessionId)? SessionOf()
    {
        return User.AccountId() is Guid customerId && Guid.TryParse(User.FindFirstValue(SessionSchemes.SessionIdClaim), out Guid sessionId) ? (customerId, sessionId) : null;
    }

    private sealed record Dropped : IOperationResult
    {
        public bool IsRejected => false;
    }
}

/// <summary>The form of UC-62 (07 UC-62): the operation the Google route is started for.</summary>
public sealed class StartSocialReauthenticationInput : IFormInput
{
    public string? ReauthenticatedOperation { get; set; }
}
