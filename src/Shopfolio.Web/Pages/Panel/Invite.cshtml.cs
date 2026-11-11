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
using Shopfolio.Modules.Administration.Contracts.Results;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The invitation page (T520; 07 9.2 UC-85, UC-113; 04 E-30). It does not depend on a session and shows no frame (04 2.2.9). A valid invitation shows the address the account will be opened for, read only,
/// with the name and the password; every other state - used, run out, withdrawn, replaced by a new invitation, ended with its sender, or the address became an administrator's (K-1085) - opens no form and
/// says one sentence that does not tell the reason (K-799) with the way to the panel's sign-in (04 9.2.4). The button opens the account with the invitation's address, consumes the link, writes the trail
/// row (inside the module) and opens no session: it leads to the sign-in with the result code and the invitation's protected value, which fills the sign-in's e-mail field (07 9.2.2; K-807, K-1190 (2)).
/// A link that went invalid between the page and the button writes nothing and the page tells so, 200 (07 9.2.2).
/// </summary>
[NoStore]
internal sealed class InviteModel : PageModel
{
    private readonly IAdministrationModule _administration;
    private readonly ITransactionRunner _transactions;
    private readonly ProtectedValues _protectedValues;

    private bool _loaded;

    public InviteModel(IAdministrationModule administration, ITransactionRunner transactions, ProtectedValues protectedValues)
    {
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(protectedValues);
        _administration = administration;
        _transactions = transactions;
        _protectedValues = protectedValues;
    }

    /// <summary>True when the invitation is valid and the form is open; false is the invalid-link state.</summary>
    public bool IsValid { get; private set; }

    /// <summary>The key of the address, for the form's action.</summary>
    public string? Key { get; private set; }

    /// <summary>The address the account will be opened for (a valid invitation only).</summary>
    public string? InvitedEmail { get; private set; }

    /// <summary>The name the form shows again after a 422.</summary>
    public string? SentName { get; private set; }

    /// <summary>
    /// A form whose fields break their rules is answered 422 by the validation filter without the handler (09 3.4 item 3): the form it shows again needs the invitation as it stands and the sent name.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity })
        {
            _loaded = true;
            SentName = context.HandlerArguments.Values.OfType<AcceptAdminInvitationInput>().FirstOrDefault()?.Name;
            if (RouteData.Values["key"] is string key)
            {
                await LoadAsync(key, HttpContext.RequestAborted);
            }
        }
    }

    public async Task<IActionResult> OnGetAsync(string key, CancellationToken ct)
    {
        _loaded = true;
        await LoadAsync(key, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAdminInvitationAsync(string key, AcceptAdminInvitationInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        SentName = input.Name;
        AdminInvitationAcceptance result = await _transactions.RunAsync(async token => await _administration.AcceptInvitationAsync(key, input.Name ?? string.Empty, input.Password ?? string.Empty, token), ct);

        switch (result.Outcome)
        {
            case AdminInvitationAcceptanceOutcome.Created when result.Email is { } email:
                // The account is open and no session is: the sign-in follows, with the result code and the invitation's address as a protected value (K-807, K-1190 (2)).
                string protectedAddress = _protectedValues.Protect(DataProtectionPurposes.AcceptedInvitation, email);
                return FormResults.SeeOther(PanelFramePaths.SignIn + "?" + SignInModel.AcceptedInvitationParameter + "=" + Uri.EscapeDataString(protectedAddress), NoticeCode.AdminAccountCreated);
            case AdminInvitationAcceptanceOutcome.FieldRefused:
                // The module refused a field the page's rules let through; the link was not consumed (07 9.2.2 422).
                string field = result.Field == AdminAccountFields.Name ? "Input.Name" : "Input.Password";
                ModelState.AddFieldError(field, (result.Code ?? ErrorCode.FieldRequired).ToString(), result.Code == ErrorCode.PasswordTooWeak ? DisplayFormat.Number(ProductConstants.PasswordMinLength) : null);
                await LoadAsync(key, ct);
                return FormResults.Redisplay(this);
            default:
                // The link went invalid between the page and the button: no account, the page tells it (07 9.2.2 200).
                IsValid = false;
                return Page();
        }
    }

    private async Task LoadAsync(string key, CancellationToken ct)
    {
        Key = key;
        AdminInvitationReading reading = await _administration.ReadInvitationLinkAsync(key, ct);
        IsValid = reading.Status == AccountLinkStatus.Valid && reading.Email is not null;
        InvitedEmail = IsValid ? reading.Email : null;
    }
}

/// <summary>The form of UC-113 (07 9.2.2; 04 7.2.2): the name and the password; the address is the invitation's and is not carried.</summary>
public sealed class AcceptAdminInvitationInput : IFormInput
{
    [Display(Name = "Ad")]
    [FieldRequired]
    public string? Name { get; set; }

    [Display(Name = "Şifre")]
    [PasswordField]
    [FieldRequired]
    [PasswordTooWeak]
    public string? Password { get; set; }
}
