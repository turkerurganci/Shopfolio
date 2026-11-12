using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The re-verification step of E-54 (T521; 04 9.25.3; 07 UC-108, UC-185): inside the panel frame, so the session is required. Two operations only - the password change and the e-mail change; the account
/// deletion does not exist for an administrator (02 10.2.2) - and the check is by password only: there is no Google here (02 10.2.5). Opening the page drops the session's earlier permission (the new form is the
/// one that counts; 05 6.1.5). The right password answers 303 to the account page with the operation and the single-use value; a wrong one is the field message and counts for L-1; over the limit is the neutral
/// sentence and nothing is done; an operation outside the list answers 303 to the account page (07 UC-185).
/// </summary>
[NoStore]
internal sealed class ReauthModel : PageModel
{
    /// <summary>The name of the query parameter that carries the operation (07 5.6.8; K-1186 (4)).</summary>
    internal const string OperationParameter = "reauthenticatedOperation";

    private readonly IAdministrationModule _administration;
    private readonly ITransactionRunner _transactions;

    private bool _loaded;

    public ReauthModel(IAdministrationModule administration, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(transactions);
        _administration = administration;
        _transactions = transactions;
    }

    /// <summary>The operation the form is for.</summary>
    public ReauthenticationOperation Operation { get; private set; } = ReauthenticationOperation.PasswordChange;

    /// <summary>The sentence that names the operation (04 9.25.2).</summary>
    public string OperationLine => Operation == ReauthenticationOperation.EmailChange ? PanelAccessTexts.ReauthEmailChangeLine : PanelAccessTexts.ReauthPasswordChangeLine;

    /// <summary>True after a 429: the neutral limit sentence stands beside the button.</summary>
    public bool LimitReached { get; private set; }

    /// <summary>
    /// A form whose fields break their rules (the password is required) is answered 422 by the validation filter without the handler: the page it shows again still needs the operation.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context, Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity }
            && context.HandlerArguments.Values.OfType<ReauthenticateInput>().FirstOrDefault() is { } sent
            && TryParse(sent.ReauthenticatedOperation, out ReauthenticationOperation operation))
        {
            _loaded = true;
            Operation = operation;
        }
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        _loaded = true;
        if (!TryParse(Request.Query[OperationParameter], out ReauthenticationOperation operation))
        {
            return FormResults.SeeOther(PanelFramePaths.MyAccount);
        }

        Operation = operation;
        if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid adminId) && Guid.TryParse(User.FindFirstValue(SessionSchemes.SessionIdClaim), out Guid sessionId))
        {
            // Opening the page drops the earlier permission: the form that is about to be sent is the one that counts (05 6.1.5).
            await _transactions.RunAsync(
                async token =>
                {
                    await _administration.DropReauthenticationAsync(adminId, sessionId, token);
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
        if (!TryParse(input.ReauthenticatedOperation, out ReauthenticationOperation operation))
        {
            return FormResults.SeeOther(PanelFramePaths.MyAccount);
        }

        Operation = operation;
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid adminId) || !Guid.TryParse(User.FindFirstValue(SessionSchemes.SessionIdClaim), out Guid sessionId))
        {
            return AccessDenial.NotFound();
        }

        string? presentedMark = BrowserCookies.ReadKnownDevice(HttpContext);
        System.Net.IPAddress? address = ClientAddress.Of(HttpContext);

        // The failed attempt is counted by the module in a transaction of its own, so the refusal's rollback does not take it back (K-972).
        ReauthenticationResult result = await _transactions.RunAsync(
            async cancel =>
            {
                string entered = input.Password ?? string.Empty;
                return await _administration.ReauthenticateAsync(adminId, sessionId, operation, entered, address, presentedMark, cancel);
            },
            ct);

        if (result.Succeeded && result.Value is { } value)
        {
            return FormResults.SeeOther(
                PanelFramePaths.MyAccount + "?" + OperationParameter + "=" + Uri.EscapeDataString(operation.ToString()) + "&reauthenticationToken=" + Uri.EscapeDataString(value));
        }

        if (result.Code == ErrorCode.SignInLimitExceeded)
        {
            LimitReached = true;
            return FormResults.AttemptLimitExceeded(this, ErrorCode.SignInLimitExceeded);
        }

        if (result.Code == ErrorCode.ReauthenticationOperationInvalid)
        {
            return FormResults.SeeOther(PanelFramePaths.MyAccount);
        }

        // The wrong password is the field's own message and counted for L-1 by the module (04 9.25.9).
        ModelState.AddModelError("Input.Password", PanelAccessTexts.ReauthPasswordWrong);
        return FormResults.Redisplay(this);
    }

    // The two operations an administrator has; the account deletion does not exist for the panel (02 10.2.2; 07 9.25.2).
    private static bool TryParse(string? raw, out ReauthenticationOperation operation)
    {
        operation = default;
        return raw is nameof(ReauthenticationOperation.PasswordChange) or nameof(ReauthenticationOperation.EmailChange)
            && Enum.TryParse(raw, ignoreCase: false, out operation);
    }

    private sealed record Dropped : IOperationResult
    {
        public bool IsRejected => false;
    }
}

/// <summary>The form of UC-185 (07 UC-185; 04 7.1.14): the password and the operation the page was opened for.</summary>
public sealed class ReauthenticateInput : IFormInput
{
    [Display(Name = "Şifre")]
    [SpecAssignedName]
    [PasswordField]
    [FieldRequired]
    public string? Password { get; set; }

    [SpecAssignedName]
    public string? ReauthenticatedOperation { get; set; }
}
