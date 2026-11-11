namespace Shopfolio.Infrastructure.Errors;

/// <summary>
/// Draws the "page not found" page E-06 inside the storefront's full frame (T504; 04 5.6.3; 07 UC-06). The infrastructure defines the seam and the pages implement it
/// (<c>FramedNotFoundPageRenderer</c>), because the common infrastructure does not reference the page layer (09 6 item 2). The single catching layer calls it for every 404 that has
/// no body of its own and falls back to the empty 404 when it is not registered or when it fails (05 10.3.6).
/// </summary>
internal interface INotFoundPageRenderer
{
    Task RenderAsync(HttpContext context);
}
