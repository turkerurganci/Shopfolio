using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>
/// The test-only page of the notice tests (T214; 07 2.2.3, 6.3): a successful POST redirects to the page itself with the notice code the form names, and the
/// GET shows the short-lived notice of the list, the result state of the list and the reason of the list - each read from its own list only. The page draws the
/// text of a notice and never the code or the raw value (09 8.2 item 4).
/// </summary>
public sealed class NoticeSampleModel : PageModel
{
    public const string Path = "/odeme/bildirim-ornegi";

    public string? ShortLivedText { get; private set; }

    public string? ResultText { get; private set; }

    public string? ReasonText { get; private set; }

    public void OnGet()
    {
        ShortLivedText = TextOf(NoticeParameter.Read(Request, NoticeKind.ShortLived));
        ResultText = TextOf(NoticeParameter.Read(Request, NoticeKind.Result));
        ReasonText = TextOf(NoticeParameter.Read(Request, NoticeKind.Reason));
    }

    public IActionResult OnPostSave(NoticeSampleInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return ModelState.IsValid ? FormResults.SeeOther(Path + "?page=2", Enum.Parse<NoticeCode>(input.Notice)) : FormResults.Redisplay(this);
    }

    // The texts of the test page only: the message catalog of the notices belongs to the screens that show them.
    // The text is the member's number, so a test sees that a text was drawn and that the code's name was not.
    public static string TextOf(NoticeCode code) => $"Metin-{(int)code}";

    private static string? TextOf(NoticeCode? code) => code is null ? null : TextOf(code.Value);
}

/// <summary>The form of the notice sample page: the name of the notice code the POST redirects with.</summary>
public sealed class NoticeSampleInput : IFormInput
{
    public string Notice { get; set; } = string.Empty;
}
