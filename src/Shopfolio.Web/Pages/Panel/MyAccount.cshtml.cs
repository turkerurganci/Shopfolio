using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Administration.Contracts.Results;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The administrator's own account page (T523; 04 9.21; 07 UC-104, UC-178 to UC-180, UC-188; E-50), inside the panel frame. Three sections: the name (its form is always open), the address and the
/// password. The address and the password change only through the re-verification step (E-54): the page opens with the operation and the single-use value in the address, asks the module whether the
/// session holds that permission bound to that value, and opens the form only then; any other request for the page drops the permission (05 6.1.5; 07 2.2.5). The pending address change shows its new
/// address and the last day of its link, with the button that asks the link again; while a revert link is open the address change is closed and K-819's sentence says until when. The password is never
/// written back into the page.
/// </summary>
[NoStore]
internal sealed class MyAccountModel : PageModel
{
    private readonly IAdministrationModule _administration;
    private readonly ITransactionRunner _transactions;

    private bool _loaded;

    public MyAccountModel(IAdministrationModule administration, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(transactions);
        _administration = administration;
        _transactions = transactions;
    }

    /// <summary>What the module read: the account, the pending change, the open revert link and the operation whose form stands open.</summary>
    public AdminOwnAccount Account { get; private set; } = new(string.Empty, string.Empty, null, null, null, null);

    /// <summary>The single-use value the open form carries in its hidden field; empty when no form is open.</summary>
    public string? HeldValue { get; private set; }

    /// <summary>The name the form shows: the sent one after a 422, the account's otherwise.</summary>
    public string? SentName { get; private set; }

    /// <summary>The address the form shows again after a 422.</summary>
    public string? SentEmail { get; private set; }

    /// <summary>The short-lived notice of a saved name (07 6.3.1).</summary>
    public MessageView? Notice { get; private set; }

    /// <summary>The lasting result of a password change or of the link asked again, told in place of its section (07 6.3.2).</summary>
    public MessageView? Result { get; private set; }

    /// <summary>K-819: the open revert link's sentence; null when none is open.</summary>
    public string? RevertSentence => Account.RevertEndsAt is { } ends ? MyAccountTexts.RevertOpen(ends) : null;

    /// <summary>A refusal that is not a field's (the limit, a change that is not there): the lasting message beside the button of its section.</summary>
    public MessageView? ActionMessage { get; private set; }

    /// <summary>The section the action message belongs to.</summary>
    public string? ActionSection { get; private set; }

    public bool EmailFormOpen => Account.OpenOperation == ReauthenticationOperation.EmailChange;

    public bool PassFormOpen => Account.OpenOperation == ReauthenticationOperation.PasswordChange;

    /// <summary>
    /// A form whose fields break their rules is answered 422 by the validation filter without the handler (09 3.4 item 3): the page it shows again needs the account and the form still open with its value.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (_loaded || executed.Exception is not null || executed.Result is not PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity })
        {
            return;
        }

        _loaded = true;
        ReauthenticationOperation? operation = null;
        string? held = null;
        foreach (object? argument in context.HandlerArguments.Values)
        {
            switch (argument)
            {
                case ChangeNameInput name:
                    SentName = name.Name;
                    break;
                case ChangeEmailInput email:
                    SentEmail = email.NewEmail;
                    operation = ReauthenticationOperation.EmailChange;
                    held = email.ReauthenticationToken;
                    break;
                case ChangePasswordInput:
                    operation = ReauthenticationOperation.PasswordChange;
                    held = ((ChangePasswordInput)argument).ReauthenticationToken;
                    break;
            }
        }

        await LoadAsync(operation, held, HttpContext.RequestAborted);
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        _loaded = true;
        ReauthenticationOperation? operation = null;
        string? held = null;
        if (Request.Query.TryGetValue(ReauthModel.OperationParameter, out Microsoft.Extensions.Primitives.StringValues operations) && operations.Count == 1
            && operations[0] is nameof(ReauthenticationOperation.PasswordChange) or nameof(ReauthenticationOperation.EmailChange)
            && Request.Query.TryGetValue(HeldValueParameter, out Microsoft.Extensions.Primitives.StringValues tokens) && tokens.Count == 1)
        {
            operation = Enum.Parse<ReauthenticationOperation>(operations.ToString());
            held = tokens[0];
        }

        if (!await LoadAsync(operation, held, ct))
        {
            return AccessDenial.NotFound();
        }

        NoticeCode? shortLived = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        Notice = shortLived == NoticeCode.Saved ? Messages.ShortLivedSuccess(NoticeTexts.Saved) : null;
        NoticeCode? result = NoticeParameter.Read(Request, NoticeKind.Result);
        if (result == NoticeCode.PasswordChanged)
        {
            Result = Messages.InPlace(MessageMeaning.Success, MyAccountTexts.PasswordChanged, "password-changed");
        }
        else if (result == NoticeCode.LinkResent && Account.PendingLinkEnd is { } linkEnd)
        {
            Result = Messages.InPlace(MessageMeaning.Success, RegistrationTexts.LinkResent(linkEnd), "link-resent");
        }

        return Page();
    }

    /// <summary>UC-178: the new name is saved; the form is always open, no re-verification (02 10.2.3).</summary>
    public async Task<IActionResult> OnPostChangeNameAsync(ChangeNameInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        if (!TryIds(out Guid adminId, out _))
        {
            return AccessDenial.NotFound();
        }

        AdminNameChangeResult result = await _transactions.RunAsync(token => _administration.ChangeNameAsync(adminId, input.Name ?? string.Empty, token), ct);
        if (result.IsDone)
        {
            return FormResults.SeeOther(PanelFramePaths.MyAccount, NoticeCode.Saved);
        }

        SentName = input.Name;
        ModelState.AddFieldError("Input.Name", (result.Code ?? ErrorCode.FieldRequired).ToString());
        await LoadAsync(null, null, ct);
        return FormResults.Redisplay(this);
    }

    /// <summary>UC-179: the new address waits for its verification link; the old one keeps working (02 10.2.4).</summary>
    public async Task<IActionResult> OnPostChangeEmailAsync(ChangeEmailInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        if (!TryIds(out Guid adminId, out Guid sessionId))
        {
            return AccessDenial.NotFound();
        }

        System.Net.IPAddress? address = ClientAddress.Of(HttpContext);
        AdminEmailChangeRequestResult result = await _transactions.RunAsync(
            cancel => _administration.RequestEmailChangeAsync(adminId, sessionId, input.NewEmail ?? string.Empty, input.ReauthenticationToken, address, cancel), ct);
        switch (result.Outcome)
        {
            case AdminEmailChangeOutcome.Requested:
                return FormResults.SeeOther(PanelFramePaths.MyAccount);
            case AdminEmailChangeOutcome.ReauthenticationRequired:
                return FormResults.SeeOther(ReauthPath(ReauthenticationOperation.EmailChange));
            case AdminEmailChangeOutcome.NotFound:
                return AccessDenial.NotFound();
            case AdminEmailChangeOutcome.Limited:
                await LoadAsync(ReauthenticationOperation.EmailChange, input.ReauthenticationToken, ct);
                SentEmail = input.NewEmail;
                return FormResults.AttemptLimitExceeded(this, ErrorCode.VerificationLinkLimitExceeded);
            case AdminEmailChangeOutcome.RevertPending:
                RejectionLog().Rejected(ErrorCode.EmailChangeRevertPending);
                SentEmail = input.NewEmail;
                await LoadAsync(ReauthenticationOperation.EmailChange, input.ReauthenticationToken, ct);
                ActionSection = "email";
                ActionMessage = Messages.InPlace(MessageMeaning.Error, result.RevertEndsAt is { } ends ? MyAccountTexts.RevertOpen(ends) : RevertSentence ?? string.Empty, "email-revert-open");
                return FormResults.Redisplay(this);
            default:
                SentEmail = input.NewEmail;
                if (result.Code == ErrorCode.AdminEmailTaken)
                {
                    RejectionLog().Rejected(ErrorCode.AdminEmailTaken);
                    ModelState.AddModelError("Input.NewEmail", MyAccountTexts.EmailTakenByOtherAdmin);
                }
                else
                {
                    ModelState.AddFieldError("Input.NewEmail", (result.Code ?? ErrorCode.FieldRequired).ToString());
                }

                await LoadAsync(ReauthenticationOperation.EmailChange, input.ReauthenticationToken, ct);
                return FormResults.Redisplay(this);
        }
    }

    /// <summary>UC-180: the pending change's link is asked again; the earlier links are replaced and the last day does not move (L-9).</summary>
    public async Task<IActionResult> OnPostResendEmailChangeLinkAsync(CancellationToken ct)
    {
        _loaded = true;
        if (!TryIds(out Guid adminId, out _))
        {
            return AccessDenial.NotFound();
        }

        System.Net.IPAddress? address = ClientAddress.Of(HttpContext);
        AdminEmailChangeResendResult result = await _transactions.RunAsync(token => _administration.ResendEmailChangeLinkAsync(adminId, address, token), ct);
        if (result.IsIssued)
        {
            return FormResults.SeeOther(PanelFramePaths.MyAccount, NoticeCode.LinkResent);
        }

        await LoadAsync(null, null, ct);
        if (result.IsLimited)
        {
            return FormResults.AttemptLimitExceeded(this, ErrorCode.VerificationLinkLimitExceeded);
        }

        RejectionLog().Rejected(ErrorCode.OperationNotAvailable);
        ActionSection = "email";
        ActionMessage = Messages.InPlace(MessageMeaning.Error, MyAccountTexts.NoPendingChange, "email-no-pending");
        return FormResults.Redisplay(this);
    }

    /// <summary>UC-188: the new password; every other session of the account ends and the recognized browsers' marks are replaced (02 3.13.10).</summary>
    public async Task<IActionResult> OnPostChangePasswordAsync(ChangePasswordInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        if (!TryIds(out Guid adminId, out Guid sessionId))
        {
            return AccessDenial.NotFound();
        }

        string? presentedMark = BrowserCookies.ReadKnownDevice(HttpContext);
        ChangeOutcome result = await _transactions.RunAsync(
            cancel => _administration.ChangePasswordAsync(adminId, sessionId, input.NewPassword ?? string.Empty, input.ReauthenticationToken, presentedMark, cancel), ct);
        if (result.IsDone && result.MarkValue is { } mark)
        {
            BrowserCookies.WriteKnownDevice(HttpContext, mark, result.MarkLifetime);
            return FormResults.SeeOther(PanelFramePaths.MyAccount, NoticeCode.PasswordChanged);
        }

        if (result.Code == ErrorCode.ReauthenticationRequired)
        {
            return FormResults.SeeOther(ReauthPath(ReauthenticationOperation.PasswordChange));
        }

        // The weak or empty password: the field's message; nothing was consumed, so the form stays open with the same value (the password itself is never written back).
        ErrorCode code = result.Code ?? ErrorCode.FieldRequired;
        ModelState.AddFieldError("Input.NewPassword", code.ToString(), code == ErrorCode.PasswordTooWeak ? DisplayFormat.Number(ProductConstants.PasswordMinLength) : null);
        await LoadAsync(ReauthenticationOperation.PasswordChange, input.ReauthenticationToken, ct);
        return FormResults.Redisplay(this);
    }

    internal const string HeldValueParameter = "reauthenticationToken";

    private static string ReauthPath(ReauthenticationOperation operation) =>
        PanelFramePaths.Reauthentication + "?" + ReauthModel.OperationParameter + "=" + Uri.EscapeDataString(operation.ToString());

    private RejectionLog RejectionLog() => HttpContext.RequestServices.GetRequiredService<RejectionLog>();

    private bool TryIds(out Guid adminId, out Guid sessionId)
    {
        sessionId = Guid.Empty;
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out adminId) && Guid.TryParse(User.FindFirstValue(SessionSchemes.SessionIdClaim), out sessionId);
    }

    private async Task<bool> LoadAsync(ReauthenticationOperation? operation, string? held, CancellationToken ct)
    {
        if (!TryIds(out Guid adminId, out Guid sessionId))
        {
            return false;
        }

        OwnAccountRead read = await _transactions.RunAsync(
            async cancel => new OwnAccountRead(await _administration.ReadOwnAccountAsync(adminId, sessionId, operation, held, cancel)),
            ct);
        if (read.Account is null)
        {
            return false;
        }

        Account = read.Account;
        HeldValue = Account.OpenOperation is not null ? held : null;
        SentName ??= Account.Name;
        return true;
    }

    private sealed record OwnAccountRead(AdminOwnAccount? Account) : IOperationResult
    {
        public bool IsRejected => false;
    }
}

/// <summary>The name form of UC-178 (07 5.2; shared with the storefront account page).</summary>
public sealed class ChangeNameInput : IFormInput
{
    [Display(Name = "Ad")]
    [FieldRequired]
    public string? Name { get; set; }
}

/// <summary>The address form of UC-179 (07 5.2): the new address and the single-use value the re-verification gave.</summary>
public sealed class ChangeEmailInput : IFormInput
{
    [Display(Name = "Yeni e-posta adresi")]
    [FieldRequired]
    [InvalidEmail]
    public string? NewEmail { get; set; }

    [SpecAssignedName]
    public string? ReauthenticationToken { get; set; }
}

/// <summary>The password form of UC-188 (07 5.2): the new password and the single-use value the re-verification gave.</summary>
public sealed class ChangePasswordInput : IFormInput
{
    [Display(Name = "Yeni şifre")]
    [SpecAssignedName]
    [PasswordField]
    [FieldRequired]
    [PasswordTooWeak]
    public string? NewPassword { get; set; }

    [SpecAssignedName]
    public string? ReauthenticationToken { get; set; }
}
