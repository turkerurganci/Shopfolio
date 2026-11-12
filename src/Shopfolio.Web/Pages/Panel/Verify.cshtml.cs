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

/// <summary>The state of a landing screen of E-54 (04 9.25.4, 9.25.5): what the link's record and the result code in the address allow.</summary>
internal enum LandingStep
{
    /// <summary>The link is valid: the one sentence and the one button.</summary>
    Valid,

    /// <summary>The operation is done (the link is used and the address carries the result code).</summary>
    Done,

    /// <summary>The link is used, run out, replaced or unknown.</summary>
    Invalid,

    /// <summary>The address of the operation belongs to another administrator account: nothing was written (422).</summary>
    Rejected,
}

/// <summary>
/// The landing screen of the new address's verification link (T521; 04 9.25.4; 07 UC-108, UC-186; E-54): no frame and independent of a session (04 2.2.9). Opening only reads the link's record - the state is the
/// record's, the result code in the address counts only for a link that is used (K-904, K-780). The button consumes the link with one conditional update and changes the address; the module queues the earlier
/// address's notice and writes the trail row. The new address having become another administrator's answers 422 and consumes nothing; a link spent between the page and the button answers 200 and writes nothing.
/// No session is opened: the landing's next step is "Hesabım" when the panel's session is open, else the sign-in.
/// </summary>
[NoStore]
internal sealed class VerifyModel : PageModel
{
    private readonly IAdministrationModule _administration;
    private readonly ITransactionRunner _transactions;

    public VerifyModel(IAdministrationModule administration, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(transactions);
        _administration = administration;
        _transactions = transactions;
    }

    public LandingStep Step { get; private set; } = LandingStep.Invalid;

    public string Key { get; private set; } = string.Empty;

    public string NextPath { get; private set; } = PanelFramePaths.SignIn;

    public string NextText { get; private set; } = PanelAccessTexts.ToSignInLink;

    public async Task<IActionResult> OnGetAsync(string key, CancellationToken ct)
    {
        Key = key;
        AdminLinkReading reading = await _administration.ReadNewEmailLinkAsync(key, ct);
        Step = reading.Status switch
        {
            Shopfolio.Infrastructure.Security.AccountLinkStatus.Valid => LandingStep.Valid,
            Shopfolio.Infrastructure.Security.AccountLinkStatus.Used when NoticeParameter.Read(Request, NoticeKind.Result) == NoticeCode.EmailVerified => LandingStep.Done,
            _ => LandingStep.Invalid,
        };
        await SetNextStepAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostVerifyEmailAsync(string key, CancellationToken ct)
    {
        Key = key;
        AdminEmailVerificationResult result = await _transactions.RunAsync(async token => await _administration.VerifyNewEmailAsync(key, token), ct);
        if (result.IsVerified)
        {
            return FormResults.SeeOther(PanelFramePaths.EmailVerification + "/" + Uri.EscapeDataString(key), NoticeCode.EmailVerified);
        }

        await SetNextStepAsync();
        if (result.IsTaken)
        {
            Step = LandingStep.Rejected;
            return FormResults.Redisplay(this);
        }

        // The link was spent, replaced or run out between the page and the button: nothing was written and the page says so, 200 (07 UC-186; 2.5.3).
        Step = LandingStep.Invalid;
        return Page();
    }

    private async Task SetNextStepAsync() => (NextPath, NextText) = await LandingNextStep.ResolveAsync(HttpContext);
}
