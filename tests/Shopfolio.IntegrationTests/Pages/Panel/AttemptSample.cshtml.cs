using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.IntegrationTests.Pages.Panel;

/// <summary>
/// The test-only sample page of the attempt limit pattern (T215; 07 2.8.5): the handler counts the attempt on the contact form limit (L-4) by the
/// client address before it works; when the limit is exceeded it does nothing and shows the page again with the sent value, the neutral message beside
/// the button and the status 429 (<see cref="FormResults.AttemptLimitExceeded"/>); otherwise it ends in 303 to the GET. The counter is taken from the
/// request's services in the handler because the counter is internal to the application and a page model is public.
/// </summary>
public sealed class AttemptSampleModel : PageModel
{
    public async Task<IActionResult> OnPostSendAsync(AttemptInput input)
    {
        AttemptCounter counter = HttpContext.RequestServices.GetRequiredService<AttemptCounter>();
        AttemptSubject subject = AttemptSubject.Ip(HttpContext);
        await counter.RecordAsync(AttemptLimit.L4, subject, HttpContext.RequestAborted);
        if (await counter.IsExceededAsync(AttemptLimit.L4, subject, HttpContext.RequestAborted))
        {
            return FormResults.AttemptLimitExceeded(this, ErrorCode.ContactRequestLimitExceeded);
        }

        return FormResults.SeeOther("/panel/deneme-ornek");
    }
}

/// <summary>The input of the <c>Send</c> handler.</summary>
public sealed class AttemptInput
{
    [StringLength(100)]
    public string Message { get; set; } = string.Empty;
}
