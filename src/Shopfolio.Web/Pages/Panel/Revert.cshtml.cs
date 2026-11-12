using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Administration.Contracts.Results;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The landing screen of the "I did not make this change" link (T521; 04 9.25.5; 07 UC-108, UC-187; E-54): no frame and independent of a session (04 2.2.9). Opening only reads; the button consumes the link
/// with one conditional update. The address goes back, every session of the account ends (the open one too), the known-browser marks and the password become invalid, and the earlier address gets the
/// password reset link, queued by the module in the same transaction. The earlier address having become another administrator's answers 422 and the consumption rolls back (K-1210,
/// K-1218); a link spent in between answers 200. The password is not asked: it is set again through the reset link (E-29).
/// </summary>
[NoStore]
internal sealed class RevertModel : PageModel
{
    private readonly IAdministrationModule _administration;
    private readonly ITransactionRunner _transactions;

    public RevertModel(IAdministrationModule administration, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(transactions);
        _administration = administration;
        _transactions = transactions;
    }

    public LandingStep Step { get; private set; } = LandingStep.Invalid;

    public string Key { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string key, CancellationToken ct)
    {
        Key = key;
        AdminLinkReading reading = await _administration.ReadEmailRestoreLinkAsync(key, ct);
        Step = reading.Status switch
        {
            Shopfolio.Infrastructure.Security.AccountLinkStatus.Valid => LandingStep.Valid,
            Shopfolio.Infrastructure.Security.AccountLinkStatus.Used when NoticeParameter.Read(Request, NoticeKind.Result) == NoticeCode.EmailChangeReverted => LandingStep.Done,
            _ => LandingStep.Invalid,
        };
        return Page();
    }

    public async Task<IActionResult> OnPostRevertPendingEmailChangeAsync(string key, CancellationToken ct)
    {
        Key = key;
        RevertOutcome outcome = await _transactions.RunAsync(
            async cancel =>
            {
                AdminEmailRevertResult reverted = await _administration.RestorePreviousEmailAsync(key, cancel);
                return new RevertOutcome(reverted);
            },
            ct);

        if (outcome.Result.IsReverted)
        {
            BrowserCookies.Delete(HttpContext, CookieNames.AdminSession);
            return FormResults.SeeOther(PanelFramePaths.EmailRevert + "/" + Uri.EscapeDataString(key), NoticeCode.EmailChangeReverted);
        }

        if (outcome.Result.IsTaken)
        {
            Step = LandingStep.Rejected;
            return FormResults.Redisplay(this);
        }

        Step = LandingStep.Invalid;
        return Page();
    }

    private sealed record RevertOutcome(AdminEmailRevertResult Result) : IOperationResult
    {
        public bool IsRejected => Result.IsRejected;
    }
}
