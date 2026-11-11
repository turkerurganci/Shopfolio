using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Pages.Legal;

/// <summary>What a legal text page draws (E-11; 04 5.11.3): the heading and the whole text of the version in force, on one page.</summary>
/// <param name="Title">The page's heading.</param>
/// <param name="Text">The stored text of the last published version, the identity placeholders filled; it is shown only through the display helper (09 15.3 item 4).</param>
public sealed record LegalTextPageView(string Title, string Text);

/// <summary>The texts of the two firm-edited legal text pages, written once (04 5.11.3; 04 3.1.19).</summary>
public static class LegalTextPageTexts
{
    public const string PrivacyNoticeTitle = "Aydınlatma metni";

    public const string CookiePolicyTitle = "Çerez politikası";
}

/// <summary>
/// The page of the privacy notice (E-11, UC-16; 07 4.11.1; 04 5.11): the last published version, whole, on one page. A notice that was never published has no page - the address answers
/// E-06 and the footer shows no link (K-770). The page is open to everyone and writes nothing.
/// </summary>
public sealed class PrivacyNoticeModel : PageModel
{
    private readonly ISettingsModule _settings;

    public PrivacyNoticeModel(ISettingsModule settings)
    {
        _settings = settings;
    }

    public LegalTextPageView View { get; private set; } = new(LegalTextPageTexts.PrivacyNoticeTitle, string.Empty);

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        LegalTextVersionInfo? version = await _settings.GetLatestLegalTextVersionAsync(LegalTextType.PrivacyNotice, ct);
        if (version is null)
        {
            return NotFound();
        }

        View = new LegalTextPageView(LegalTextPageTexts.PrivacyNoticeTitle, version.Text);
        return Page();
    }
}

/// <summary>The page of the cookie policy (E-11, UC-17; 07 4.11.1; 04 5.11): the same skeleton and the same rule as the privacy notice's page.</summary>
public sealed class CookiePolicyModel : PageModel
{
    private readonly ISettingsModule _settings;

    public CookiePolicyModel(ISettingsModule settings)
    {
        _settings = settings;
    }

    public LegalTextPageView View { get; private set; } = new(LegalTextPageTexts.CookiePolicyTitle, string.Empty);

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        LegalTextVersionInfo? version = await _settings.GetLatestLegalTextVersionAsync(LegalTextType.CookiePolicy, ct);
        if (version is null)
        {
            return NotFound();
        }

        View = new LegalTextPageView(LegalTextPageTexts.CookiePolicyTitle, version.Text);
        return Page();
    }
}

/// <summary>
/// The "İşlem rehberi" page (E-11, UC-18; 07 4.11.1; 04 5.11.5; K-626): the product's fixed "how the order is placed" text. It reads no setting and no publication, so it answers 200 in every
/// state - while sales are closed too - and is not a condition of the sales gate.
/// </summary>
public sealed class ProcessGuideModel : PageModel
{
    public string Title => ProcessGuide.Title;

    public IReadOnlyList<ProcessGuideSection> Sections => ProcessGuide.Sections;

#pragma warning disable CA1822 // A page handler is an instance method the framework finds by its name; a static method is not a handler.
    public IActionResult OnGet() => Page();
#pragma warning restore CA1822
}
