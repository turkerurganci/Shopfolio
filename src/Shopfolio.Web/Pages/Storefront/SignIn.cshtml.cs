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
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Panel;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The customer's sign-in screen (T515; 07 UC-54, UC-55, UC-50, UC-49; 04 E-22 5.22). The page searches the customer accounts only - the administrator signs in at the panel (K-588). The check and the session are
/// one transaction: the check holds the account row's lock, so a reset that is running ends first or waits (09 15.1 item 3); the failed attempt and the sign-in record are written by the module in
/// transactions of their own, so a refusal's rollback does not take them back (K-972). The cart merge is another transaction (D-558: the merge takes the guest cart's lock and the account lock is a family of
/// its own, 05 7.3.5). A wrong e-mail or password is one sentence that does not say which (K-779 (4)); the right pair of an unverified account opens no session and tells that the verification is awaited, with a
/// button for the link in place of the form (K-779 (3), K-1186 (2)); an unverified account whose link has ended is told as a wrong pair (03 3.4.2). Over the limit L-1 the neutral sentence stands beside the button.
/// "Beni hatırla" arrives unticked. The response is personal and not stored.
/// </summary>
[NoStore]
internal sealed class SignInModel : PageModel
{
    private readonly IMembershipModule _membership;
    private readonly ISessionService _sessions;
    private readonly IOrderingModule _ordering;
    private readonly INotificationsModule _notifications;
    private readonly ITransactionRunner _transactions;
    private readonly ProtectedValues _protectedValues;
    private readonly GoogleSignInFlow _flow;

    private bool _loaded;

    public SignInModel(
        IMembershipModule membership,
        ISessionService sessions,
        IOrderingModule ordering,
        INotificationsModule notifications,
        ITransactionRunner transactions,
        ProtectedValues protectedValues,
        GoogleSignInFlow flow)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(protectedValues);
        ArgumentNullException.ThrowIfNull(flow);
        _membership = membership;
        _sessions = sessions;
        _ordering = ordering;
        _notifications = notifications;
        _transactions = transactions;
        _protectedValues = protectedValues;
        _flow = flow;
    }

    /// <summary>What the form shows: the sent values after a 422 or a 429, else the return address of the address.</summary>
    public SignInInput Input { get; private set; } = new();

    /// <summary>True when the Google application is defined: only then the screen has "Google ile giriş" (04 5.22.11; D-81).</summary>
    public bool GoogleOpen => _membership.IsGoogleSignInOpen;

    /// <summary>The lasting message at the head of the form: the closed session or the Google return's result (04 5.22.12, 5.22.15); null when the address carries no such code.</summary>
    public MessageView? ResultMessage { get; private set; }

    /// <summary>True after a 422 for wrong credentials: the K-779 sentence stands at the head of the form.</summary>
    public bool CredentialsRefused { get; private set; }

    /// <summary>True after a 429: the neutral limit sentence stands beside the button (04 5.22.7).</summary>
    public bool LimitReached { get; private set; }

    /// <summary>The pending registration the page shows the awaited-verification state for, in the place of the form; null for the form (04 5.22.8).</summary>
    public PendingRegistrationReading? Pending { get; private set; }

    /// <summary>The protected value of the pending registration, as the resend form carries it.</summary>
    public string? PendingValue { get; private set; }

    /// <summary>True when the result code of the request is the resend's: the new link's last day is told (04 5.20.7).</summary>
    public bool LinkWasResent { get; private set; }

    /// <summary>True after a 429 of the resend (L-9): the neutral sentence stands beside the resend button.</summary>
    public bool ResendLimitReached { get; private set; }

    /// <summary>A form whose fields break their rules is answered 422 by the validation filter without the handler (09 3.4 item 3): the page it shows again still needs the sent values.</summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult)
        {
            if (context.HandlerArguments.Values.OfType<SignInInput>().FirstOrDefault() is { } sent)
            {
                Input = sent;
                Input.ReturnUrl = ReturnPath.IsLocalPath(sent.ReturnUrl) ? sent.ReturnUrl : null;
            }

            _loaded = true;
        }
    }

    public async Task<IActionResult> OnGetAsync(string? pendingRegistration, CancellationToken ct)
    {
        _loaded = true;
        string? returnUrl = Request.Query[ReturnPath.ParameterName];
        Input = new SignInInput { ReturnUrl = ReturnPath.IsLocalPath(returnUrl) ? returnUrl : null };

        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.Result);
        if (SignInTexts.ResultOf(notice) is { } result)
        {
            ResultMessage = Messages.InPlace(result.Meaning, result.Text, "sign-in-result");
        }

        // The resend's 303 returns here with the value and its code; a value without the code, or a code without a pending registration, shows the form (04 5.20.4, 5.22.8).
        if (notice == NoticeCode.LinkResent && PendingRegistrations.TryResolve(_protectedValues, pendingRegistration, out Guid customerId))
        {
            PendingRegistrationReading? pending = await _membership.ReadPendingRegistrationAsync(customerId, ct);
            if (pending is not null)
            {
                SetPending(pending, pendingRegistration!);
                LinkWasResent = true;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSignInAsync(SignInInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        Input = input;
        Input.ReturnUrl = ReturnPath.IsLocalPath(input.ReturnUrl) ? input.ReturnUrl : null;

        string? presentedMark = BrowserCookies.ReadKnownDevice(HttpContext);
        System.Net.IPAddress? address = ClientAddress.Of(HttpContext);

        // The check and the session are one transaction (09 15.1 item 3); the refusal rolls back, the module's own transactions keep the attempt and the record (K-972).
        SignInOutcome outcome = await _transactions.RunAsync(
            async token =>
            {
                CredentialCheckResult check = await _membership.CheckCredentialsAsync(input.Email ?? string.Empty, input.Password ?? string.Empty, address, presentedMark, token);
                if (check.CustomerId is not { } customerId)
                {
                    return new SignInOutcome(check, null);
                }

                return new SignInOutcome(check, await _sessions.SignInAsync(customerId, input.IsLongLived, presentedMark, token));
            },
            ct);

        if (outcome.Opening is not { } opening || outcome.Check.CustomerId is not { } signedIn)
        {
            return await RefuseAsync(outcome.Check, ct);
        }

        BrowserCookies.Write(HttpContext, CookieNames.Session, opening.SessionKey, opening.CookieLifetime);
        BrowserCookies.WriteKnownDevice(HttpContext, opening.MarkValue, opening.MarkLifetime);
        await GuestCartMerge.RunAsync(HttpContext, _transactions, _ordering, signedIn, ct);
        return FormResults.SeeOther(ReturnPath.AfterSignIn(input.ReturnUrl));
    }

    public IActionResult OnPostStartSocialLogin(StartSocialLoginInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        Input = new SignInInput { ReturnUrl = ReturnPath.IsLocalPath(input.ReturnUrl) ? input.ReturnUrl : null };

        // Google is not defined: the operation is not offered and not done; the page is shown again without the button (04 5.22.11; 07 UC-50).
        if (!_membership.IsGoogleSignInOpen)
        {
            return FormResults.Redisplay(this);
        }

        return FormResults.SeeOtherToGoogle(_flow.Start(HttpContext, GoogleFlowKind.SignInOrRegistration, null, Input.ReturnUrl));
    }

    public async Task<IActionResult> OnPostResendVerificationLinkAsync(ResendVerificationLinkInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        string sent = input.PendingRegistration ?? string.Empty;
        if (!PendingRegistrations.TryResolve(_protectedValues, sent, out Guid customerId))
        {
            return GoneAddress();
        }

        ResendOutcome outcome = await PendingRegistrations.ResendAsync(_membership, _notifications, _transactions, customerId, ClientAddress.Of(HttpContext), ct);
        if (outcome.Result is null)
        {
            return GoneAddress();
        }

        if (outcome.Result.IsIssued)
        {
            return FormResults.SeeOther(FramePaths.SignIn + "?" + PendingRegistrations.Parameter + "=" + Uri.EscapeDataString(sent), NoticeCode.LinkResent);
        }

        if (outcome.IsLimited && outcome.Pending is { } pending)
        {
            // The awaited-verification state stays drawn and the neutral sentence stands beside the button (04 5.22.8; 03 3.4.14).
            SetPending(pending, sent);
            ResendLimitReached = true;
            return FormResults.AttemptLimitExceeded(this, ErrorCode.VerificationLinkLimitExceeded);
        }

        return GoneAddress();
    }

    // A refused check: the limit's neutral sentence, the awaited verification of a pending registration, or the one sentence of a wrong pair (07 UC-55).
    private async Task<IActionResult> RefuseAsync(CredentialCheckResult check, CancellationToken ct)
    {
        if (check.Code == ErrorCode.SignInLimitExceeded)
        {
            LimitReached = true;
            return FormResults.AttemptLimitExceeded(this, ErrorCode.SignInLimitExceeded);
        }

        if (check.Code == ErrorCode.EmailNotVerified && check.UnverifiedCustomerId is { } unverified
            && await _membership.ReadPendingRegistrationAsync(unverified, ct) is { } pending)
        {
            SetPending(pending, PendingRegistrations.Protect(_protectedValues, unverified));
            return FormResults.Redisplay(this);
        }

        // A registration whose link has ended is not pending any more: the right pair is told as a wrong one (03 3.4.2).
        CredentialsRefused = true;
        return FormResults.Redisplay(this);
    }

    private void SetPending(PendingRegistrationReading pending, string value)
    {
        Pending = pending;
        PendingValue = value;
    }

    // A value that names nothing pending: back to the sign-in screen without the value - the form in its empty state (07 4.20.3).
    private static IActionResult GoneAddress() => FormResults.SeeOther(FramePaths.SignIn);

    private sealed record SignInOutcome(CredentialCheckResult Check, SessionOpening? Opening) : IOperationResult
    {
        public bool IsRejected => Opening is null;
    }
}

/// <summary>The form of UC-50 (07 5.2.1): the return address on the sign-in screen; the registration screen's form has no field.</summary>
public sealed class StartSocialLoginInput : IFormInput
{
    [SpecAssignedName]
    public string? ReturnUrl { get; set; }
}
