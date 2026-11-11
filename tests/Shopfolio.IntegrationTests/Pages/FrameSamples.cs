using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>What the sample pages of the frame tests print: the frame the request carries, as JSON, or an empty object when the response has none (T537).</summary>
public static class FrameJson
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new JsonStringEnumConverter() } };

    public static string Of(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        StorefrontFrame? frame = context.GetStorefrontFrame();
        return frame is null ? "{}" : JsonSerializer.Serialize(frame, Options);
    }
}

/// <summary>A storefront page that draws itself and nothing of the frame: the frame is not in the model, the filter put it in the request (T537; 07 5.5.1, KK1).</summary>
public sealed class FrameSampleModel : PageModel
{
    public IActionResult OnGet() => Page();

    /// <summary>A POST that answers with a redirect: no frame is built and no merge mark is read for it (07 4.34.3).</summary>
    public IActionResult OnPostLeave() => RedirectToPage();
}

/// <summary>The payment step's own address: the frame is the reduced one there (04 2.2.7).</summary>
public sealed class FrameCheckoutSampleModel : PageModel
{
    public IActionResult OnGet() => Page();
}
