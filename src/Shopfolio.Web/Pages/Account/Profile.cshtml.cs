using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.Pages.Account;

/// <summary>The form of UC-64 (07 5.1.5; 04 7.1.15.1): the account's name. The name asks for no re-authentication (02 3.13.21).</summary>
public sealed class ChangeNameInput : IFormInput
{
    [Display(Name = "Ad")]
    [FieldRequired]
    public string Name { get; set; } = string.Empty;
}

/// <summary>The form of UC-65 (07 5.1.5; 04 7.1.15.3): the new password and the single-use value of the re-authentication, in the hidden field.</summary>
public sealed class ChangePasswordInput : IFormInput
{
    [Display(Name = "Yeni şifre")]
    [SpecAssignedName]
    [PasswordField]
    [FieldRequired]
    [PasswordTooWeak]
    public string NewPassword { get; set; } = string.Empty;

    [SpecAssignedName]
    [NotTrimmed]
    public string? ReauthenticationToken { get; set; }
}

/// <summary>The form of UC-66 (07 5.1.5; 04 7.1.15.2): the new address (06 4.40) and the single-use value of the re-authentication, in the hidden field.</summary>
public sealed class ChangeEmailInput : IFormInput
{
    [Display(Name = "Yeni e-posta")]
    [FieldRequired]
    [InvalidEmail]
    public string NewEmail { get; set; } = string.Empty;

    [SpecAssignedName]
    [NotTrimmed]
    public string? ReauthenticationToken { get; set; }
}

/// <summary>The form of UC-68 (07 5.1.5; 04 5.25.13): the single-use value of the re-authentication and the confirmation mark of the last step (OB-04; 07 2.2.4).</summary>
public sealed class DeleteAccountInput : IFormInput
{
    [SpecAssignedName]
    [NotTrimmed]
    public string? ReauthenticationToken { get; set; }

    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>
/// E-25, the account screen (T517; 07 UC-63 to UC-68; 04 5.25): five sections - the name, the e-mail, the password, the ways of signing in and the account's deletion - under the inner navigation of the account screens.
/// The page is in the account folder, so the customer's session is required. The GET writes nothing but this: the form of an operation opens only when the session holds the permission for that very operation and the
/// value's digest is the session's own (the address carries the operation and the single-use value from the re-authentication, 07 UC-63; K-971); a parameter that does not hold is ignored and the section is closed. A
/// page request that does not show the permission's form drops the permission (05 6.1.5; K-1186 (5)). The three operations that need the re-authentication - the password, the e-mail and the deletion - send a missing or
/// spent value to the re-authentication screen (303); the value is taken in the same transaction as the write, so a refusal never spends it (07 2.5.2). While the revert link of an e-mail change is open
/// (Z-45) the e-mail change and the deletion are closed and the reason with the date stands in their place (02 3.13.14, 3.15.6; K-779 (6)). The deletion is the customer's last step (OB-04) and an uncertain outcome is
/// the 303 back to this screen (07 6.4.2). The response is personal and not stored.
/// </summary>
[NoStore]
internal sealed class ProfileModel : PageModel, IOutcomeTarget
{
    private readonly IMembershipModule _membership;
    private readonly ISessionService _sessions;
    private readonly ITransactionRunner _transactions;
    private readonly IAuditLineWriter _audit;

    private bool _loaded;

    public ProfileModel(IMembershipModule membership, ISessionService sessions, ITransactionRunner transactions, IAuditLineWriter audit)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(audit);
        _membership = membership;
        _sessions = sessions;
        _transactions = transactions;
        _audit = audit;
    }

    /// <summary>The order of the name form's fields, for the error summary.</summary>
    public static IReadOnlyList<string> NameFields { get; } = ["Input.Name"];

    /// <summary>The account's name as stored.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The account's address as stored; the account works with it until a new one is verified.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>The ways the account is entered: the password and the Google link (04 5.25.5, 5.25.6).</summary>
    public SignInMethods Methods { get; private set; } = new(string.Empty, false, false);

    /// <summary>The pending e-mail change that still lives: the new address and the last day of its link (04 5.25.4); null when none.</summary>
    public PendingEmailChangeReading? Pending { get; private set; }

    /// <summary>The end of the open revert link (Z-45) while it is open: the e-mail change and the deletion are closed until then (04 5.25.4, 5.25.7); null otherwise.</summary>
    public DateTimeOffset? RevertEndsAt { get; private set; }

    /// <summary>The operation whose form is open, or null when every section is closed (07 UC-63).</summary>
    public ReauthenticationOperation? OpenOperation { get; private set; }

    /// <summary>The single-use value the open form carries in its hidden field.</summary>
    public string? OpenToken { get; private set; }

    /// <summary>The open form's value for the hidden field; empty when no form is open.</summary>
    public string OpenValue => OpenToken ?? string.Empty;

    /// <summary>The name the form shows again after a refusal; null shows the stored name.</summary>
    public string? SentName { get; private set; }

    /// <summary>The address the form shows again after a refusal.</summary>
    public string? SentNewEmail { get; private set; }

    /// <summary>The short-lived "Kaydedildi" of the name's save (04 5.25.9); null otherwise.</summary>
    public MessageView? Notice { get; private set; }

    /// <summary>The lasting message of the password section: the changed password, or the refusal of an account without a password.</summary>
    public MessageView? AccessMessage { get; private set; }

    /// <summary>The lasting message of the e-mail section: the renewed link (K-779 (2)) or the refusals of the section.</summary>
    public MessageView? EmailMessage { get; private set; }

    /// <summary>The lasting message of the deletion section: the open revert link's reason.</summary>
    public MessageView? DeleteMessage { get; private set; }

    /// <summary>True after a 429 of the e-mail change: the neutral limit sentence stands beside the button (04 5.25.14; 07 UC-66).</summary>
    public bool EmailLimitReached { get; private set; }

    /// <summary>True after a 429 of the link's renewal.</summary>
    public bool ResendLimitReached { get; private set; }

    /// <summary>The last step of the deletion (OB-04, the customer's step; 04 5.25.13); null when the deletion's section shows no step.</summary>
    public ConfirmView? Confirmation { get; private set; }

    /// <summary>The screen that shows the result when the outcome of the deletion is uncertain (07 6.4.2: UC-68 to this screen).</summary>
    public string OutcomeScreen() => FramePaths.Account;

    /// <summary>
    /// A form the validation filter refused (422, without the handler) is still drawn: it needs the account's state, the sent values and - for the operations that carry one - the same value in the hidden field,
    /// which the refusal did not spend (09 3.4 item 3; 07 2.5.2).
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        // This filter runs before the validation filter: the value of the re-authentication is read first, so a missing or spent value is the 303 to the re-authentication screen whatever else is wrong with the form
        // (a malformed address, a weak password, an unconfirmed deletion, an address already held); no refusal that follows can tell an account's existence to a session that does not hold the permission (02 3.13.11; K-786).
        if (SessionOf() is { } heldBy && ReauthenticatedOperationOf(context.HandlerArguments) is (var operation, var presentedValue)
            && !await _sessions.HoldsReauthenticationAsync(heldBy.CustomerId, heldBy.SessionId, operation, presentedValue, HttpContext.RequestAborted))
        {
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.ReauthenticationRequired);
            context.Result = FormResults.SeeOther(ReauthenticationAddresses.Screen(operation));
            return;
        }

        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity })
        {
            _loaded = true;
            await LoadAsync(HttpContext.RequestAborted);
            foreach (object? argument in context.HandlerArguments.Values)
            {
                switch (argument)
                {
                    case ChangeNameInput name:
                        SentName = name.Name;
                        break;
                    case ChangePasswordInput password:
                        Open(ReauthenticationOperation.PasswordChange, password.ReauthenticationToken);
                        break;
                    case ChangeEmailInput email:
                        SentNewEmail = email.NewEmail;
                        Open(ReauthenticationOperation.EmailChange, email.ReauthenticationToken);
                        break;
                }
            }
        }
    }

    // The operation a handler's form asks the re-authentication for, with the value the form carries; null for a handler that asks for none (the name, the link's renewal, the read).
    private static (ReauthenticationOperation Operation, string? Value)? ReauthenticatedOperationOf(IDictionary<string, object?> arguments)
    {
        foreach (object? argument in arguments.Values)
        {
            switch (argument)
            {
                case ChangePasswordInput password:
                    return (ReauthenticationOperation.PasswordChange, password.ReauthenticationToken);
                case ChangeEmailInput email:
                    return (ReauthenticationOperation.EmailChange, email.ReauthenticationToken);
                case DeleteAccountInput deletion:
                    return (ReauthenticationOperation.AccountDeletion, deletion.ReauthenticationToken);
            }
        }

        return null;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        _loaded = true;
        if (SessionOf() is not { } session || !await LoadAsync(ct))
        {
            return AccessDenial.NotFound();
        }

        // The form of an operation opens only when the session holds the permission for it under this value; a parameter that does not hold is ignored (07 UC-63; 05 6.1.5; K-971).
        bool opened = false;
        if (ReauthenticationAddresses.TryParse(Request.Query[ReauthenticationAddresses.OperationParameter], out ReauthenticationOperation operation)
            && Request.Query[ReauthenticationAddresses.TokenParameter].ToString() is { Length: > 0 } presentedValue
            && await _sessions.HoldsReauthenticationAsync(session.CustomerId, session.SessionId, operation, presentedValue, ct))
        {
            Open(operation, presentedValue);
            opened = OpenOperation is not null;
        }

        // The page request that does not show the permission's form drops the permission (07 UC-63; 05 6.1.5; K-1186 (5)).
        if (!opened)
        {
            await _transactions.RunAsync(
                async cancel =>
                {
                    await _sessions.DropReauthenticationAsync(session.CustomerId, session.SessionId, cancel);
                    return new Done();
                },
                ct);
        }

        ReadNotices();
        return Page();
    }

    /// <summary>UC-64: writes the name. 303 to this screen with the short-lived "Kaydedildi"; an empty name is 422 (04 5.25.9). No re-authentication, no order's recipient name is touched.</summary>
    public async Task<IActionResult> OnPostChangeNameAsync(ChangeNameInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        if (SessionOf() is not { } session)
        {
            return AccessDenial.NotFound();
        }

        NameChangeResult result = await _transactions.RunAsync(async cancel => await _membership.ChangeNameAsync(session.CustomerId, input.Name, cancel), ct);
        if (result.IsChanged)
        {
            return FormResults.SeeOther(FramePaths.Account, NoticeCode.Saved);
        }

        if (result.IsAccountMissing)
        {
            return AccessDenial.NotFound();
        }

        SentName = input.Name;
        foreach (MembershipFieldError field in result.Fields)
        {
            ModelState.AddFieldError("Input.Name", field.Code.ToString());
        }

        await LoadAsync(ct);
        return FormResults.Redisplay(this);
    }

    /// <summary>
    /// UC-65: changes the password. The value of the re-authentication is taken in the same transaction as the write and only after the policy has passed; a missing or spent value is the 303 to the re-authentication
    /// screen; a password under P-17 or a common one is 422 and takes nothing. The other sessions end, this one stays, the other browsers' marks end (02 3.13.10); an account without a password is not offered the operation.
    /// </summary>
    public async Task<IActionResult> OnPostChangePasswordAsync(ChangePasswordInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        if (SessionOf() is not { } session || !await LoadAsync(ct))
        {
            return AccessDenial.NotFound();
        }

        if (!Methods.HasPassword)
        {
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.OperationNotAvailable);
            AccessMessage = Messages.InPlace(MessageMeaning.Warning, AccountTexts.NoPassword, "account-no-password");
            return FormResults.Redisplay(this);
        }

        string? presentedMark = BrowserCookies.ReadKnownDevice(HttpContext);
        ChangeOutcome outcome = await _transactions.RunAsync(
            async cancel => await _membership.ChangePasswordAsync(session.CustomerId, session.SessionId, input.NewPassword, input.ReauthenticationToken, presentedMark, cancel),
            ct);

        if (outcome is { IsDone: true, MarkValue: { } mark })
        {
            // This browser stays the account's known browser; the other browsers' marks ended (02 3.13.10; 06 4.41).
            BrowserCookies.WriteKnownDevice(HttpContext, mark, outcome.MarkLifetime);
            return FormResults.SeeOther(FramePaths.Account, NoticeCode.PasswordChanged);
        }

        if (outcome.Code == ErrorCode.ReauthenticationRequired)
        {
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.ReauthenticationRequired);
            return FormResults.SeeOther(ReauthenticationAddresses.Screen(ReauthenticationOperation.PasswordChange));
        }

        ErrorCode refusal = outcome.Code ?? ErrorCode.FieldRequired;
        ModelState.AddFieldError("Input.NewPassword", refusal.ToString(), refusal == ErrorCode.PasswordTooWeak ? DisplayFormat.Number(ProductConstants.PasswordMinLength) : null);
        Open(ReauthenticationOperation.PasswordChange, input.ReauthenticationToken);
        return FormResults.Redisplay(this);
    }

    /// <summary>
    /// UC-66: asks for the new address. The pending change is written and the link goes to the new address; the account keeps the old address until the link's button. A biçim, an address of another verified account and an
    /// open revert link are 422 and take nothing; 429 (L-9) only when the request replaces a pending change; a missing or spent value is the 303 to the re-authentication screen (07 UC-66; K-734; K-1134).
    /// </summary>
    public async Task<IActionResult> OnPostChangeEmailAsync(ChangeEmailInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        if (SessionOf() is not { } session || !await LoadAsync(ct))
        {
            return AccessDenial.NotFound();
        }

        EmailChangeRequestResult result = await _transactions.RunAsync(
            async cancel => await _membership.RequestEmailChangeAsync(session.CustomerId, session.SessionId, input.NewEmail, input.ReauthenticationToken, ClientAddress.Of(HttpContext), cancel),
            ct);

        if (result.IsRequested)
        {
            return FormResults.SeeOther(FramePaths.Account);
        }

        RejectionLog rejections = HttpContext.RequestServices.GetRequiredService<RejectionLog>();
        SentNewEmail = input.NewEmail;
        switch (result.Code)
        {
            case ErrorCode.ReauthenticationRequired:
                rejections.Rejected(ErrorCode.ReauthenticationRequired);
                return FormResults.SeeOther(ReauthenticationAddresses.Screen(ReauthenticationOperation.EmailChange));
            case ErrorCode.EmailChangeRevertPending:
                rejections.Rejected(ErrorCode.EmailChangeRevertPending);
                await LoadAsync(ct);
                return FormResults.Redisplay(this);
            case ErrorCode.VerificationLinkLimitExceeded:
                EmailLimitReached = true;
                Open(ReauthenticationOperation.EmailChange, input.ReauthenticationToken);
                return FormResults.AttemptLimitExceeded(this, ErrorCode.VerificationLinkLimitExceeded);
            case ErrorCode.EmailTakenByOtherAccount:
                rejections.Rejected(ErrorCode.EmailTakenByOtherAccount);
                ModelState.AddModelError("Input.NewEmail", AccountTexts.EmailTaken);
                break;
            default:
                foreach (MembershipFieldError field in result.Fields)
                {
                    ModelState.AddFieldError("Input.NewEmail", field.Code.ToString());
                }

                break;
        }

        Open(ReauthenticationOperation.EmailChange, input.ReauthenticationToken);
        return FormResults.Redisplay(this);
    }

    /// <summary>UC-67: sends the pending change's link again; the earlier ones end and the life does not lengthen (K-779 (2)). No pending change is 422; 429 (L-9).</summary>
    public async Task<IActionResult> OnPostResendEmailChangeLinkAsync(CancellationToken ct)
    {
        _loaded = true;
        if (SessionOf() is not { } session || !await LoadAsync(ct))
        {
            return AccessDenial.NotFound();
        }

        EmailChangeResendResult result = await _transactions.RunAsync(async cancel => await _membership.ResendEmailChangeLinkAsync(session.CustomerId, ClientAddress.Of(HttpContext), cancel), ct);
        if (result.IsIssued)
        {
            return FormResults.SeeOther(FramePaths.Account, NoticeCode.LinkResent);
        }

        if (result.Code == ErrorCode.VerificationLinkLimitExceeded)
        {
            ResendLimitReached = true;
            return FormResults.AttemptLimitExceeded(this, ErrorCode.VerificationLinkLimitExceeded);
        }

        HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.OperationNotAvailable);
        EmailMessage = Messages.InPlace(MessageMeaning.Warning, AccountTexts.NothingPending, "account-nothing-pending");
        return FormResults.Redisplay(this);
    }

    /// <summary>
    /// UC-68: deletes the account. Without the confirmation mark the answer is the last step (200): nothing is written and the value is not spent. With it, one transaction takes the value, ends the account and what hangs on it
    /// and the answer is 303 to the deletion's result state; the open revert link closes it (422 with the reason and the date); a missing or spent value is the 303 to the re-authentication screen. After the commit the
    /// instant deletion line is written (05 7.4.5; D-700). Anything uncertain returns to this screen (07 6.4.2).
    /// </summary>
    [OutcomeUncertain("UC-68")]
    public async Task<IActionResult> OnPostDeleteAccountAsync(DeleteAccountInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        if (SessionOf() is not { } session || !await LoadAsync(ct))
        {
            return AccessDenial.NotFound();
        }

        if (!input.Confirmed)
        {
            if (RevertEndsAt is not null)
            {
                HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.EmailChangeRevertPending);
                return FormResults.Redisplay(this);
            }

            Open(ReauthenticationOperation.AccountDeletion, input.ReauthenticationToken);
            return Page();
        }

        AccountDeletionResult result = await _transactions.RunAsync(
            async cancel => await _membership.DeleteAccountAsync(session.CustomerId, session.SessionId, input.ReauthenticationToken, cancel),
            ct);

        if (result.IsDeleted)
        {
            // The line is written after the transaction committed; a rolled-back deletion writes none (05 7.4.5; D-700).
            _audit.Write(AuditLine.InstantDeletion(InstantDeletionKinds.CustomerAccount, session.CustomerId));

            // The response does not delete the session cookie: its being left behind with no session is the sign that shows the result state once (07 UC-69; K-1184).
            return FormResults.SeeOther(FramePaths.AccountDeleted);
        }

        RejectionLog rejections = HttpContext.RequestServices.GetRequiredService<RejectionLog>();
        switch (result.Code)
        {
            case ErrorCode.ReauthenticationRequired:
                rejections.Rejected(ErrorCode.ReauthenticationRequired);
                return FormResults.SeeOther(ReauthenticationAddresses.Screen(ReauthenticationOperation.AccountDeletion));
            case ErrorCode.EmailChangeRevertPending:
                rejections.Rejected(ErrorCode.EmailChangeRevertPending);
                await LoadAsync(ct);
                return FormResults.Redisplay(this);
            default:
                // No verified account any more (the second send of a deletion that happened): the screen that opens shows the account if it stands, the sign-in if it does not (04 5.25.17).
                return FormResults.SeeOther(FramePaths.Account);
        }
    }

    private async Task<bool> LoadAsync(CancellationToken ct)
    {
        if (SessionOf() is not { } session
            || await _membership.GetMemberContactAsync(session.CustomerId, ct) is not { } contact
            || await _membership.GetSignInMethodsAsync(session.CustomerId, ct) is not { } methods)
        {
            return false;
        }

        Name = contact.Name;
        Email = contact.Email;
        Methods = methods;
        Pending = await _membership.ReadPendingEmailChangeAsync(session.CustomerId, ct);
        RevertEndsAt = await _membership.GetDeletionBlockedUntilAsync(session.CustomerId, ct);
        if (RevertEndsAt is { } ends)
        {
            // The revert link closes the e-mail change and the deletion and stands in their place with the reason and the date; the pending change and the revert never stand together (04 5.25.15).
            DeleteMessage = Messages.InPlace(MessageMeaning.Info, AccountTexts.RevertOpen(ends), "account-revert-open");
        }

        return true;
    }

    // The form of an operation opens only when the section is open for the account: no form of the e-mail change or the deletion while the revert link is open, no password form for an account without a password.
    private void Open(ReauthenticationOperation operation, string? permitValue)
    {
        bool closed = operation == ReauthenticationOperation.PasswordChange
            ? !Methods.HasPassword
            : RevertEndsAt is not null;
        if (closed || string.IsNullOrEmpty(permitValue))
        {
            return;
        }

        OpenOperation = operation;
        OpenToken = permitValue;
        if (operation == ReauthenticationOperation.AccountDeletion)
        {
            Confirmation = Confirmations.ForCustomer(
                AccountTexts.DeleteOperationName,
                AccountTexts.DeleteSentence,
                AccountTexts.DeleteConfirmLabel,
                FramePaths.Account,
                "DeleteAccount",
                FramePaths.Account,
                [
                    new("Input." + nameof(DeleteAccountInput.ReauthenticationToken), permitValue),
                    new("Input." + nameof(DeleteAccountInput.Confirmed), "true"),
                ]);
        }
    }

    // The result states and the short-lived notice the address carries (07 6.3): read from the closed list, never from free text.
    private void ReadNotices()
    {
        NoticeCode? shortLived = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        if (shortLived is { } code && NoticeTexts.TryShortLived(code, out string? text) && code == NoticeCode.Saved)
        {
            Notice = Messages.ShortLivedSuccess(text);
        }

        NoticeCode? result = NoticeParameter.Read(Request, NoticeKind.Result);
        if (result == NoticeCode.PasswordChanged)
        {
            AccessMessage = Messages.InPlace(MessageMeaning.Success, AccountTexts.PasswordChanged, "account-password-changed");
        }
        else if (result == NoticeCode.LinkResent && Pending is not null)
        {
            EmailMessage = Messages.InPlace(MessageMeaning.Success, RegistrationTexts.LinkResent(Pending.LinkEndsAt), "account-link-resent");
        }
    }

    private (Guid CustomerId, Guid SessionId)? SessionOf()
    {
        return User.AccountId() is Guid customerId && Guid.TryParse(User.FindFirstValue(SessionSchemes.SessionIdClaim), out Guid sessionId) ? (customerId, sessionId) : null;
    }

    private sealed record Done : IOperationResult
    {
        public bool IsRejected => false;
    }
}
