using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;

namespace Shopfolio.Endpoints;

/// <summary>
/// UC-81: <c>GET /sistem/canlilik</c> (07 4.33.1, 05 9.4.1). An open machine endpoint: no signature, no cookie, no session, no body.
/// It answers 200 when the database answers within the setting's time and 503 otherwise. It is a minimal endpoint, not a page,
/// so the page handler's error pattern does not touch it (09 4.1, 8.1 item 3).
/// </summary>
internal static class LivenessEndpoint
{
    public const string Path = "/sistem/canlilik";

    public static IEndpointRouteBuilder MapLivenessEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Path, HandleAsync);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        NpgsqlDataSource dataSource,
        IOptions<DatabaseOptions> options,
        ILoggerFactory loggerFactory,
        HttpContext context,
        CancellationToken ct)
    {
        ILogger logger = loggerFactory.CreateLogger(typeof(LivenessProbe).FullName ?? nameof(LivenessProbe));
        bool alive = await LivenessProbe.CheckDatabaseAsync(dataSource, options.Value.LivenessTimeout, logger, ct);

        context.Response.Headers.CacheControl = "no-store";
        return Results.StatusCode(alive ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
    }
}
