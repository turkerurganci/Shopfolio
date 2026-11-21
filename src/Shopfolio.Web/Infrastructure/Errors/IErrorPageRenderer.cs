namespace Shopfolio.Infrastructure.Errors;

/// <summary>
/// The words of an error page: the title and the one short explanation (04 2.7.3.2); never a code, a request id or a technical detail (07 2.7.3). <paramref name="Status"/> is the page's HTTP status
/// - 500 for the unexpected error, 429 for the general limit and 413 for the body ceiling (D-1146) - and <paramref name="OffersRetry"/> is false for those two refusals: no "Yeniden dene".
/// </summary>
internal sealed record ErrorPageText(string Title, string Message, int Status = StatusCodes.Status500InternalServerError, bool OffersRetry = true);

/// <summary>
/// Draws the page-level error page inside the frame of the surface that failed (04 2.7.3.2; 09 8.1 item 3; D-135). The infrastructure defines the seam and the pages implement it
/// (<c>FramedErrorPageRenderer</c>), because the common infrastructure does not reference the page layer (09 6 item 2). The single catching layer uses it when it is registered and falls
/// back to the plainest page when it is not or when it fails (05 10.3.6).
/// </summary>
internal interface IErrorPageRenderer
{
    Task RenderAsync(HttpContext context, ErrorPageText page);
}
