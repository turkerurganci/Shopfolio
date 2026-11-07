using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Status;

namespace Shopfolio.Endpoints;

/// <summary>
/// UC-82: <c>GET /sistem/durum</c> (07 4.33.2, 05 9.4.1). An open machine endpoint: no signature, no cookie, no session, no body. It answers
/// 200 when no warning condition is open and 503 when one is; which one is not in the answer. It writes nothing. The warning conditions are
/// not in the liveness endpoint (05 9.4.15). A minimal endpoint, not a page, so the page handler's error pattern does not touch it.
/// </summary>
internal static class StatusEndpoint
{
    public const string Path = "/sistem/durum";

    public static IEndpointRouteBuilder MapStatusEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Path, HandleAsync).WithMetadata(MachineEndpointMetadata.Instance);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(StatusEvaluator evaluator, HttpContext context, CancellationToken ct)
    {
        IReadOnlyList<string> open = await evaluator.OpenConditionsAsync(ct);

        context.Response.Headers.CacheControl = "no-store";
        return Results.StatusCode(open.Count == 0 ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
    }
}
