using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

using Shopfolio.Infrastructure.Forms;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The protection against cross-site forgery of every page POST (07 2.5.4; 05 6.5.1; 09 15.2 item 1). The framework's own check of a page handler
/// answers a bare 400 and the form's content is lost; the product wants the page again, with what the user typed, a new token and a message at the
/// level of the action (04 2.7.3.1). The page is shown only after the model is bound, so the check lives here, in a page filter that runs after the
/// binding and before every other page filter (<see cref="FilterOrder"/>), and the framework's check is switched off for the pages by the policy
/// marker <see cref="ForgeryProtectionPolicy"/>: the framework validates only when no other policy of a higher order is in force.
/// A failed check writes nothing - the handler does not run - and the answer is 400 (<see cref="FormResults.ForgeryRefused"/>).
/// </summary>
internal sealed class ForgeryProtectionFilter : IAsyncPageFilter, IOrderedFilter
{
    /// <summary>Before the field validation (order 0), so a forged request is refused as a forgery whatever its fields hold.</summary>
    public const int FilterOrder = -100;

    /// <summary>The action-level message of the refusal (07 6.2.1 <c>AntiforgeryValidationFailed</c>; 04 2.7.3.1): what was typed is kept, the user tries again.</summary>
    public const string Message = "İşlem tamamlanamadı. Sayfanın süresi dolmuş olabilir; yazdıklarınız korundu, aynı düğmeyle yeniden deneyin.";

    private readonly IAntiforgery _antiforgery;

    public ForgeryProtectionFilter(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public int Order => FilterOrder;

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (!HandlerRequiredFilter.IsWritingMethod(context.HttpContext.Request.Method) || !IsInForce(context.Filters))
        {
            await next();
            return;
        }

        if (await _antiforgery.IsRequestValidAsync(context.HttpContext))
        {
            await next();
            return;
        }

        if (context.HandlerInstance is not PageModel page)
        {
            context.Result = new StatusCodeResult(FormResults.ForgeryRefusedStatus);
            return;
        }

        // The binding has run and may have put rule codes into the model state; a code never reaches the page (07 2.7.3). The sent values stay.
        foreach (ModelStateEntry entry in page.ModelState.Values)
        {
            entry.Errors.Clear();
            entry.ValidationState = ModelValidationState.Unvalidated;
        }

        page.ModelState.AddModelError(string.Empty, Message);
        context.Result = FormResults.ForgeryRefused(page);
    }

    /// <summary>
    /// Whether the check is in force for this page: whenever the marker <see cref="ForgeryProtectionPolicy"/> is registered, and the page cannot take
    /// it back. The marker's order (1001) is above the framework's ignore attribute (1000), so even a page that carried that attribute is checked
    /// here (fail closed: a forgotten or foreign mark never opens a page, 05 6.5.5); a source scan keeps the attribute out of the application besides.
    /// </summary>
    internal static bool IsInForce(IList<IFilterMetadata> filters)
    {
        return filters.OfType<ForgeryProtectionPolicy>().Any();
    }
}

/// <summary>
/// The policy marker that makes the framework's own page check stand aside (see <see cref="ForgeryProtectionFilter"/>). Order 1001: above the framework's
/// automatic check and the ignore attribute (both 1000), and the last policy wins, so the framework's check stands aside and the ignore attribute
/// cannot switch the protection off.
/// </summary>
internal sealed class ForgeryProtectionPolicy : IAntiforgeryPolicy, IOrderedFilter
{
    public const int PolicyOrder = 1001;

    public int Order => PolicyOrder;
}
