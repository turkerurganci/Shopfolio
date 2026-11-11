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
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The panel's sign-in (T520; 07 9.1 UC-84, UC-110; 04 E-29 9.1.3). The page shows no panel frame (04 2.2.9). "Beni hatırla" arrives unticked. The result states travel in the address as codes (07 2.2.3):
/// the session that closed by itself says so, and the account an invitation opened fills the e-mail field from the invitation's protected value - a value that does not unprotect, or names no account, is
/// ignored (04 3.4.18; K-807, K-1190 (2)). The button opens the panel's session with a new key at every sign-in and writes the known-browser mark (05 6.1.1, 6.7.6); the storefront's session and cart are another
/// scheme and are not touched (K-899). A wrong e-mail or password is one sentence that does not say which (K-779, K-1204); over the limit L-1 is the neutral sentence and the attempts are counted by the module (K-972).
/// </summary>
[NoStore]
internal sealed class SignInModel : PageModel
{
    /// <summary>The name of the query parameter that carries the invitation's protected value (07 9.2.2; K-1190 (2)).</summary>
    internal const string AcceptedInvitationParameter = "acceptedInvitation";

    private readonly IAdministrationModule _administration;
    private readonly ITransactionRunner _transactions;
    private readonly ProtectedValues _protectedValues;

    private bool _loaded;

    public SignInModel(IAdministrationModule administration, ITransactionRunner transactions, ProtectedValues protectedValues)
    {
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(protectedValues);
        _administration = administration;
        _transactions = transactions;
        _protectedValues = protectedValues;
    }

    /// <summary>What the form shows: the sent values after a 422 or a 429, else the invitation's address and the return address of the address.</summary>
    public SignInInput Input { get; private set; } = new();

    /// <summary>The lasting message at the head of the form: the closed session or the opened account (04 9.1.12, 9.2.6); null when the address carries no code.</summary>
    public MessageView? ResultMessage { get; private set; }

    /// <summary>True after a 422 for wrong credentials: the K-779 sentence stands at the head of the form.</summary>
    public bool CredentialsRefused { get; private set; }

    /// <summary>True after a 429: the neutral limit sentence stands beside the button (04 9.1.8).</summary>
    public bool LimitReached { get; private set; }

    /// <summary>
    /// A form whose fields break their rules is answered 422 by the validation filter without the handler (09 3.4 item 3): the page it shows again still needs the sent values.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity }
            && context.HandlerArguments.Values.OfType<SignInInput>().FirstOrDefault() is { } sent)
        {
            Input = sent;
            _loaded = true;
        }
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        _loaded = true;
        string? returnUrl = Request.Query[ReturnPath.ParameterName];
        Input = new SignInInput { ReturnUrl = ReturnPath.IsLocalPath(returnUrl) ? returnUrl : null };

        switch (NoticeParameter.Read(Request, NoticeKind.Result))
        {
            case NoticeCode.SessionExpired:
                ResultMessage = PanelAccessTexts.InPlace(MessageMeaning.Warning, PanelAccessTexts.SessionClosed, "sign-in-result");
                break;
            case NoticeCode.AdminAccountCreated:
                ResultMessage = PanelAccessTexts.InPlace(MessageMeaning.Success, PanelAccessTexts.AccountCreated, "sign-in-result");
                Input.Email = await AcceptedAddressAsync(ct);
                break;
            default:
                break;
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

        // The check and the session are one transaction: the check holds the account row's lock, so a reset or a password change that is running ends first or waits (09 15.1 item 3).
        // The failed attempt and the sign-in record are written by the module in transactions of their own, so a refusal's rollback does not take them back (K-972).
        SignInOutcome outcome = await _transactions.RunAsync(
            async token =>
            {
                AdminCredentialCheckResult check = await _administration.CheckCredentialsAsync(input.Email ?? string.Empty, input.Password ?? string.Empty, address, presentedMark, token);
                if (check.AdminId is not { } adminId)
                {
                    return new SignInOutcome(check, null);
                }

                return new SignInOutcome(check, await _administration.SignInAsync(adminId, input.RememberMe, presentedMark, token));
            },
            ct);

        if (outcome.Opening is not { } opening)
        {
            if (outcome.Check.Code == ErrorCode.SignInLimitExceeded)
            {
                LimitReached = true;
                return FormResults.AttemptLimitExceeded(this, ErrorCode.SignInLimitExceeded);
            }

            CredentialsRefused = true;
            return FormResults.Redisplay(this);
        }

        BrowserCookies.Write(HttpContext, CookieNames.AdminSession, opening.SessionKey, opening.CookieLifetime);
        BrowserCookies.WriteKnownDevice(HttpContext, opening.MarkValue, opening.MarkLifetime);
        return FormResults.SeeOther(ReturnPath.Resolve(input.ReturnUrl, ReturnArea.Panel));
    }

    // The address the invitation's protected value names, when it unprotects and the account is there (K-1190 (2)); else null and the field stays empty.
    private async Task<string?> AcceptedAddressAsync(CancellationToken ct)
    {
        string? raw = Request.Query[AcceptedInvitationParameter];
        ProtectedValueResult value = _protectedValues.Unprotect(DataProtectionPurposes.AcceptedInvitation, raw);
        if (!value.IsValid)
        {
            return null;
        }

        return await _administration.FindAdminIdAsync(value.Payload, ct) is null ? null : value.Payload;
    }

    private sealed record SignInOutcome(AdminCredentialCheckResult Check, SessionOpening? Opening) : IOperationResult
    {
        public bool IsRejected => Opening is null;
    }
}

/// <summary>The form of UC-110 (07 9.1.2; 04 7.2.1.1-7.2.1.3): e-mail, password, "beni hatırla" and the return address.</summary>
public sealed class SignInInput : IFormInput
{
    [Display(Name = "E-posta")]
    [FieldRequired]
    [InvalidEmail]
    public string? Email { get; set; }

    [Display(Name = "Şifre")]
    [PasswordField]
    [FieldRequired]
    public string? Password { get; set; }

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
