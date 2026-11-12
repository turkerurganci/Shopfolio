using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
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
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

using MembershipRegisterInput = Shopfolio.Modules.Membership.Contracts.RegisterInput;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The registration screen (T514; 07 4.9 UC-47, UC-48, UC-49; 04 E-20). The form (name, e-mail, secret) opens a pending registration - a verification link that lives Z-1 and its e-mail, in the
/// same transaction as the account (D-252) - and no session; the state after the send is the same page: the 303 carries the protected id of the pending registration in
/// <c>pendingRegistration</c> and a result code, and the page reads the account again from the id (07 UC-47; K-1186 (2)). The value is encrypted and signed and is not stored (06 4.36). While the
/// gate of the data-collecting entries is closed the form says so and has no button (04 5.20.10). The Google button and its handler are T515's (K-850). The response is personal and not stored.
/// </summary>
[NoStore]
internal sealed class RegisterModel : PageModel
{
    public const string PendingParameter = "pendingRegistration";

    private readonly IMembershipModule _membership;
    private readonly INotificationsModule _notifications;
    private readonly ISettingsModule _settings;
    private readonly ITransactionRunner _transactions;
    private readonly ProtectedValues _protectedValues;

    private bool _loaded;

    public RegisterModel(IMembershipModule membership, INotificationsModule notifications, ISettingsModule settings, ITransactionRunner transactions, ProtectedValues protectedValues)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(protectedValues);
        _membership = membership;
        _notifications = notifications;
        _settings = settings;
        _transactions = transactions;
        _protectedValues = protectedValues;
    }

    /// <summary>The order of the form's fields: the error summary lists the wrong ones in it (04 2.3.2).</summary>
    public static IReadOnlyList<string> FormOrder { get; } = [nameof(RegisterInput.Name), nameof(RegisterInput.Email), nameof(RegisterInput.Password)];

    /// <summary>True while the gate of the data-collecting entries is open (02 3.13.19; 04 5.20.10); a closed gate shows the form closed and no button.</summary>
    public bool IsOpen { get; private set; } = true;

    /// <summary>The pending registration the page shows the after-send state for; null for the form.</summary>
    public PendingRegistrationReading? Pending { get; private set; }

    /// <summary>The protected value of the pending registration, as the resend form carries it.</summary>
    public string? PendingValue { get; private set; }

    /// <summary>True when the result code of the request is the resend's (03 9.1.3).</summary>
    public bool LinkWasResent { get; private set; }

    /// <summary>The name the form shows again after a refusal (07 2.2.2).</summary>
    public string? SentName { get; private set; }

    /// <summary>The e-mail the form shows again after a refusal (07 2.2.2). The secret is never shown again.</summary>
    public string? SentEmail { get; private set; }

    /// <summary>True when the e-mail's message is "already registered": the sign-in link goes beside it (04 5.20.9).</summary>
    public bool EmailIsTaken { get; private set; }

    /// <summary>
    /// A form the validation filter refused (422, without the handler) or one the forgery check refused is still drawn: it needs the gate's state and the sent values. They are read after the filters,
    /// so a request the forgery check refused writes nothing.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult && Request.HasFormContentType)
        {
            RememberSent(context.HandlerArguments.Values.OfType<RegisterInput>().FirstOrDefault());
            await LoadAsync(Request.Form[PendingParameter].ToString(), HttpContext.RequestAborted);
        }
    }

    public async Task<IActionResult> OnGetAsync(string? pendingRegistration, CancellationToken ct)
    {
        await LoadAsync(pendingRegistration, ct);

        // The result code only decides which wording the pending state gets; a code without a pending registration shows the form (04 5.20.4).
        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.Result);
        LinkWasResent = notice == NoticeCode.LinkResent;
        if (Pending is not null && notice is not (NoticeCode.RegistrationSubmitted or NoticeCode.LinkResent))
        {
            Pending = null;
            PendingValue = null;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRegisterAsync(RegisterInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        RememberSent(input);
        System.Net.IPAddress? ip = ClientAddress.Of(HttpContext);

        RegistrationOutcome outcome = await _transactions.RunAsync(
            async token =>
            {
                RegistrationResult result = await _membership.RegisterAsync(new MembershipRegisterInput(input.Name, input.Email, input.Password), ip, token);
                if (result.IsRegistered)
                {
                    // The e-mail is a row of the same transaction (D-252): the account and its link never exist without the message that carries the link.
                    await _notifications.QueueAccountEmailAsync(new VerificationLinkEmail(result.CustomerId.GetValueOrDefault(), result.LinkId.GetValueOrDefault(), input.Email.Trim()), token);
                }

                return new RegistrationOutcome(result);
            },
            RegistrationIndexes.RetryOnUniqueIndexes,
            ct);

        RegistrationResult registration = outcome.Result;
        if (registration.IsRegistered)
        {
            string value = _protectedValues.Protect(DataProtectionPurposes.PendingRegistration, registration.CustomerId.GetValueOrDefault().ToString("N"));
            return FormResults.SeeOther(FramePaths.Register + "?" + PendingParameter + "=" + Uri.EscapeDataString(value), NoticeCode.RegistrationSubmitted);
        }

        await LoadAsync(null, ct);
        RejectionLog rejections = HttpContext.RequestServices.GetRequiredService<RejectionLog>();
        switch (registration.Code)
        {
            case ErrorCode.RegistrationLimitExceeded:
                return FormResults.AttemptLimitExceeded(this, ErrorCode.RegistrationLimitExceeded);
            case ErrorCode.EmailAlreadyRegistered:
                // The screen tells that the address is taken - on purpose (04 5.20.9; 03 3.4.3); L-3 narrows the asking.
                EmailIsTaken = true;
                ModelState.AddModelError(nameof(RegisterInput.Email), RegistrationTexts.EmailAlreadyRegistered);
                rejections.Rejected(ErrorCode.EmailAlreadyRegistered);
                break;
            case var _ when registration.Gate is not null:
                rejections.Rejected(registration.Gate.Code);
                break;
            default:
                // The module checks the fields again after the attributes; its refusal is the same field error with the same code.
                foreach (MembershipFieldError field in registration.Fields)
                {
                    ModelState.AddFieldError(field.Field, field.Code.ToString(), field.Code == ErrorCode.PasswordTooWeak ? DisplayFormat.Number(ProductConstants.PasswordMinLength) : field.Detail);
                }

                break;
        }

        return FormResults.Redisplay(this);
    }

    public async Task<IActionResult> OnPostResendVerificationLinkAsync(ResendVerificationLinkInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        string sent = input.PendingRegistration ?? string.Empty;
        if (!TryResolve(sent, out Guid customerId))
        {
            return GoneAddress();
        }

        System.Net.IPAddress? ip = ClientAddress.Of(HttpContext);
        ResendOutcome outcome = await _transactions.RunAsync(
            async token =>
            {
                // A value that names no pending registration sends nothing and counts nothing (04 5.20.7; 07 UC-49).
                PendingRegistrationReading? pending = await _membership.ReadPendingRegistrationAsync(customerId, token);
                if (pending is null)
                {
                    return ResendOutcome.Unresolved;
                }

                VerificationResendResult result = await _membership.ResendVerificationLinkAsync(customerId, ip, token);
                if (result.IsIssued)
                {
                    await _notifications.QueueAccountEmailAsync(new VerificationLinkEmail(customerId, result.LinkId.GetValueOrDefault(), pending.Email), token);
                }

                return new ResendOutcome(result, pending);
            },
            ct);

        if (outcome.Result is null)
        {
            return GoneAddress();
        }

        if (outcome.Result.IsIssued)
        {
            return FormResults.SeeOther(FramePaths.Register + "?" + PendingParameter + "=" + Uri.EscapeDataString(sent), NoticeCode.LinkResent);
        }

        if (outcome.Result.Code == ErrorCode.VerificationLinkLimitExceeded)
        {
            // The pending state stays drawn and the neutral sentence stands beside the button (04 5.20.7; 03 3.4.14).
            await LoadAsync(sent, ct);
            return FormResults.AttemptLimitExceeded(this, ErrorCode.VerificationLinkLimitExceeded);
        }

        return GoneAddress();
    }

    // The value is read again at every request: nothing of the registration is trusted from the address but the id the value protects.
    private async Task LoadAsync(string? pendingValue, CancellationToken ct)
    {
        _loaded = true;
        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        IsOpen = gate.RefusalOf(DataCollectionEntry.AccountRegistration) is null;
        PendingValue = null;
        Pending = null;
        if (string.IsNullOrEmpty(pendingValue))
        {
            return;
        }

        PendingRegistrationReading? pending = TryResolve(pendingValue, out Guid customerId) ? await _membership.ReadPendingRegistrationAsync(customerId, ct) : null;
        if (pending is null)
        {
            // A value that names nothing pending is ignored: the form in its empty state (07 4.20.1).
            return;
        }

        Pending = pending;
        PendingValue = pendingValue;
    }

    private bool TryResolve(string value, out Guid customerId)
    {
        customerId = Guid.Empty;
        ProtectedValueResult unprotected = _protectedValues.Unprotect(DataProtectionPurposes.PendingRegistration, value);
        return unprotected.IsValid && Guid.TryParseExact(unprotected.Payload, "N", out customerId);
    }

    // A value that names nothing pending: back to the screen the 303 came from, without the value - the form in its empty state (07 4.20.1, 4.20.3).
    private static IActionResult GoneAddress() => FormResults.SeeOther(FramePaths.Register);

    private void RememberSent(RegisterInput? input)
    {
        if (input is not null)
        {
            SentName = input.Name;
            SentEmail = input.Email;
        }
    }

    private sealed record RegistrationOutcome(RegistrationResult Result) : IOperationResult
    {
        public bool IsRejected => Result.IsRejected;
    }

    private sealed record ResendOutcome(VerificationResendResult? Result, PendingRegistrationReading? Pending) : IOperationResult
    {
        public static ResendOutcome Unresolved { get; } = new(null, null);

        public bool IsRejected => Result is null || Result.IsRejected;
    }
}

/// <summary>The form of UC-48 (07 5.2.1): the name, the e-mail and the secret - all three required; no marketing consent, no agreement box (04 5.20.5).</summary>
public sealed class RegisterInput : IFormInput
{
    [Display(Name = "Ad")]
    [FieldRequired]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "E-posta")]
    [FieldRequired]
    [InvalidEmail]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Şifre")]
    [SpecAssignedName]
    [PasswordField]
    [FieldRequired]
    [PasswordTooWeak]
    public string Password { get; set; } = string.Empty;
}

/// <summary>The form of UC-49 (07 5.2.1): the protected value of the pending registration, the only thing the resend names.</summary>
public sealed class ResendVerificationLinkInput : IFormInput
{
    [SpecAssignedName]
    [NotTrimmed]
    public string? PendingRegistration { get; set; }
}
