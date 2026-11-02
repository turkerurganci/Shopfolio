using Npgsql;

using Shopfolio.Infrastructure.Logging;

using Deadline = System.Threading.CancellationTokenSource;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The database check of the liveness endpoint (09 10.7 item 6): opens a connection and runs <c>SELECT 1</c> within the limit.
/// </summary>
internal static partial class LivenessProbe
{
    /// <summary>
    /// Returns true when the database answered within <paramref name="limit"/>. A timeout or a connection or command failure
    /// returns false; a cancelled request (<paramref name="ct"/>) is not a failure and lets the cancellation pass up.
    /// </summary>
    public static async Task<bool> CheckDatabaseAsync(NpgsqlDataSource dataSource, TimeSpan limit, ILogger logger, CancellationToken ct)
    {
        using Deadline timeout = Deadline.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(limit);
        try
        {
            await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(timeout.Token);
            await using NpgsqlCommand command = new("SELECT 1", connection);
            await command.ExecuteScalarAsync(timeout.Token);
            return true;
        }
#pragma warning disable CA1031 // 09 8.1 item 6: the liveness probe is the sixth place a broad catch is allowed; a request cancellation stays out through the filter.
        catch (Exception) when (!ct.IsCancellationRequested)
#pragma warning restore CA1031
        {
            if (timeout.IsCancellationRequested)
            {
                // The one log line of a timeout (09 18.2). The exception is not written: its text can carry connection details.
                LogDbTimeout(logger, LogEvents.DbTimeout);
            }

            return false;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event}")]
    private static partial void LogDbTimeout(ILogger logger, string @event);
}
