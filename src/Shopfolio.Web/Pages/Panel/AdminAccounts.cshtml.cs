using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Administration.Contracts.Results;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>The form of UC-175 (07 5.1.5; 04 7.2.23.1): the invited address. The e-mail form is the one home of the rule (09 3.4 item 1).</summary>
public sealed class SendAdminInvitationInput : IFormInput
{
    [Display(Name = "E-posta")]
    [FieldRequired]
    [InvalidEmail]
    public string? Email { get; set; }
}

/// <summary>The form of UC-176 (07 5.1.5): the invitation's internal key, which stands only in the page's forms (07 2.1.6).</summary>
public sealed class RevokeAdminInvitationInput : IFormInput
{
    public Guid AdminInvitationId { get; set; }
}

/// <summary>The form of UC-177 (07 5.1.5): the administrator's internal key and the confirmation mark; without the mark the answer is the confirmation state and nothing is written.</summary>
public sealed class RemoveAdminInput : IFormInput
{
    public Guid AdminId { get; set; }

    public bool Confirmed { get; set; }
}

/// <summary>One administrator row of the screen (04 9.20.3): the removal button stands only on another administrator's row and only while there is more than one administrator.</summary>
public sealed record AdminAccountRowView(Guid Id, string Name, string Email, bool IsSelf, bool CanRemove);

/// <summary>One invitation row of the screen (04 9.20.4): the dates are Turkey days; the mark is the text of the undelivered invitation e-mail.</summary>
public sealed record AdminInvitationRowView(Guid Id, string Email, string SenderName, string SentOn, string LinkEndsOn, bool EmailNotDelivered);

/// <summary>
/// E-49, the administrator accounts screen (T522; 04 9.20; 07 UC-103, UC-175, UC-176, UC-177): the administrators in alphabetical order, the valid invitations newest first and the invitation form. The GET
/// writes nothing; each operation is a named POST handler (07 2.2) of the module's write methods, one transaction each (09 9.1). An invitation and a withdrawal ask no confirmation; the removal does (OB-04).
/// The acting administrator's right is checked inside the module under its lock (07 UC-177): a request of an account that is no administrator any more writes nothing and goes to the sign-in.
/// </summary>
internal sealed class AdminAccountsModel : PageModel
{
    /// <summary>K-820's field message of an address that belongs to an administrator account (04 9.20.7).</summary>
    public const string AdminEmailTakenText = "Bu adres zaten bir yönetici hesabına ait.";

    /// <summary>K-820's message of the last administrator and of the own row; it stays in place (04 9.20.11; 2.3.3).</summary>
    public const string LastAdminText = "Son yönetici kaldırılamaz.";

    /// <summary>04 9.20.5 (K-820): what the form says when the address already has a valid invitation.</summary>
    public const string ReplacesInvitationText = "Bu adrese gönderilmiş geçerli bir davet var; yeni davet onu geçersiz kılar.";

    public const string YouText = "siz";

    public const string RemoveLabel = "Yöneticiyi kaldır";

    public const string RevokeLabel = "Daveti geri çek";

    public const string ResendLabel = "Yeni davet gönder";

    public const string InviteButtonLabel = "Daveti gönder";

    public const string InviteHeading = "Yönetici davet et";

    /// <summary>The field name the invitation form and the row buttons post the address under.</summary>
    public const string EmailFieldName = "Input.Email";

    private readonly IAdminAccountList _list;
    private readonly IAdministrationModule _administration;
    private readonly ITransactionRunner _transactions;
    private readonly RejectionLog _rejections;

    public AdminAccountsModel(IAdminAccountList list, IAdministrationModule administration, ITransactionRunner transactions, RejectionLog rejections)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(rejections);
        _list = list;
        _administration = administration;
        _transactions = transactions;
        _rejections = rejections;
    }

    public IReadOnlyList<AdminAccountRowView> Admins { get; private set; } = [];

    /// <summary>The valid invitations, newest first; empty means the list is not drawn (04 9.20.4).</summary>
    public IReadOnlyList<AdminInvitationRowView> Invitations { get; private set; } = [];

    /// <summary>The short-lived notice the redirect after a sent invitation carries (04 9.20.7); null otherwise.</summary>
    public MessageView? Notice { get; private set; }

    /// <summary>The confirmation state of the removal (04 9.20.10); null when no removal waits.</summary>
    public ConfirmView? Confirmation { get; private set; }

    /// <summary>The permanent in-place message of a refused removal (04 9.20.11); null otherwise.</summary>
    public MessageView? RemovalRefusal { get; private set; }

    /// <summary>The address the form shows again after a 422.</summary>
    public string? SentEmail { get; private set; }

    /// <summary>True when the address the form shows again has a valid invitation that the new one would end (04 9.20.5).</summary>
    public bool SentEmailHasValidInvitation { get; private set; }

    public const string ListRoot = PanelFramePaths.AdminAccounts;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await LoadAsync(ct);
        return Page();
    }

    /// <summary>UC-175: sends the invitation. Success is 303 to the list with the short-lived notice; a refusal is the field's message, 422 (04 9.20.12).</summary>
    public async Task<IActionResult> OnPostSendAdminInvitationAsync(SendAdminInvitationInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        string email = input.Email ?? string.Empty;
        AdminInvitationSendResult result = await _transactions.RunAsync(token => _administration.SendInvitationAsync(email, token), ct);
        switch (result.Outcome)
        {
            case AdminInvitationSendOutcome.Sent:
                return FormResults.SeeOther(ListRoot, NoticeCode.InvitationSent);
            case AdminInvitationSendOutcome.ActorNotAdministrator:
                return FormResults.SeeOther(PanelFramePaths.SignIn);
            default:
                ErrorCode code = result.Code ?? ErrorCode.FieldRequired;
                _rejections.Rejected(code);
                if (code == ErrorCode.AdminEmailTaken)
                {
                    ModelState.AddModelError(EmailFieldName, AdminEmailTakenText);
                }
                else
                {
                    ModelState.AddFieldError(EmailFieldName, code.ToString());
                }

                SentEmail = email;
                await LoadAsync(ct);
                return FormResults.Redisplay(this);
        }
    }

    /// <summary>UC-176: withdraws the invitation. Withdrawn and no-longer-active are both 303 to the list (nothing is written in the second); a missing row is 404.</summary>
    public async Task<IActionResult> OnPostRevokeAdminInvitationAsync(RevokeAdminInvitationInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        AdminInvitationWithdrawalResult result = await _transactions.RunAsync(token => _administration.WithdrawInvitationAsync(input.AdminInvitationId, token), ct);
        switch (result.Outcome)
        {
            case AdminInvitationWithdrawalOutcome.NotFound:
                return NotFound();
            case AdminInvitationWithdrawalOutcome.ActorNotAdministrator:
                return FormResults.SeeOther(PanelFramePaths.SignIn);
            default:
                return FormResults.SeeOther(ListRoot);
        }
    }

    /// <summary>
    /// UC-177: removes another administrator. Without the mark the answer is the confirmation state (200, nothing written); with it the module deletes the account, its sessions and links, ends its
    /// invitations and writes the trail row, the disposal record and the F-6 notice. The own row and the last administrator are 422 with the permanent message.
    /// </summary>
    public async Task<IActionResult> OnPostRemoveAdminAsync(RemoveAdminInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        AdminRemovalResult result = await _transactions.RunAsync(token => _administration.RemoveAdminAsync(input.AdminId, input.Confirmed, token), ct);
        switch (result.Outcome)
        {
            case AdminRemovalOutcome.Removed:
                return FormResults.SeeOther(ListRoot);
            case AdminRemovalOutcome.NotFound:
                return NotFound();
            case AdminRemovalOutcome.ActorNotAdministrator:
                return FormResults.SeeOther(PanelFramePaths.SignIn);
            case AdminRemovalOutcome.Refused:
                _rejections.Rejected(ErrorCode.LastAdminRemoval);
                RemovalRefusal = Messages.InPlace(MessageMeaning.Error, LastAdminText, "admin-removal-refused");
                await LoadAsync(ct);
                return FormResults.Redisplay(this);
            default:
                Confirmation = Confirmations.ForAdminRemoval(
                    result.RemovedName ?? string.Empty,
                    result.RemovedEmail ?? string.Empty,
                    result.InvalidatedInvitations,
                    ListRoot,
                    "RemoveAdmin",
                    ListRoot,
                    [
                        new("Input.AdminId", input.AdminId.ToString("D")),
                        new("Input.Confirmed", "true"),
                    ]);
                await LoadAsync(ct);
                return Page();
        }
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        AdminAccountList list = await _list.ListAsync(ct);
        Guid? self = User.AccountId();
        bool several = list.Admins.Count > 1;
        Admins = [.. list.Admins.Select(a => new AdminAccountRowView(a.Id, a.Name, a.Email, a.Id == self, several && a.Id != self))];
        Invitations = [.. list.Invitations.Select(i => new AdminInvitationRowView(i.Id, i.Email, i.SenderName, DisplayFormat.Date(i.SentAt), DisplayFormat.Date(i.LinkEndsAt), i.EmailNotDelivered))];
        SentEmailHasValidInvitation = SentEmail is { Length: > 0 } sent
            && list.Invitations.Any(i => string.Equals(TextNormalizer.NormalizeIdentity(i.Email), TextNormalizer.NormalizeIdentity(sent.Trim()), StringComparison.Ordinal));
        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        Notice = notice is { } code && NoticeTexts.TryShortLived(code, out string? text) ? Messages.ShortLivedSuccess(text) : null;
    }
}
