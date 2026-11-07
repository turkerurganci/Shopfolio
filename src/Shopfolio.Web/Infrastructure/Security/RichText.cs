using Microsoft.AspNetCore.Html;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The display of a stored long text (09 15.3 items 1, 4; 06 2.10.2): the stored value goes through the same allow list again - a gap the cleaner later closes is closed
/// for every text already stored, a published legal text version included - and the stored value is not rewritten. This is the only place that turns a text into markup
/// the page prints as it is; a page prints no other text unescaped (<c>RichTextSourceScan</c>).
/// </summary>
internal static class RichText
{
    public static IHtmlContent Render(string? stored)
    {
        return new HtmlString(RichTextSanitizer.Sanitize(stored));
    }
}
