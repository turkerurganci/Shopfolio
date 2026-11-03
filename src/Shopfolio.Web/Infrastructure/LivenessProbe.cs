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
    /// <remarks>
    /// The limit is enforced by waiting for the check with <see cref="Task.WaitAsync(TimeSpan, CancellationToken)"/>, not by the
    /// driver's cancellation: Npgsql does not honour a cancelled token while it opens a physical connection (measured), so a slow
    /// or unreachable server would outlast the limit. The check keeps running in the background until the driver gives up; it
    /// disposes its own connection and swallows its own failure, so nothing is left unobserved.
    /// </remarks>
    public static async Task<bool> CheckDatabaseAsync(NpgsqlDataSource dataSource, TimeSpan limit, ILogger logger, CancellationToken ct)
    {
        Deadline abort = Deadline.CreateLinkedTokenSource(ct);
        Task<bool> check = RunCheckAsync(dataSource, abort);
        try
        {
            bool alive = await check.WaitAsync(limit, ct);
            // A request cancelled while the check was ending is not a failure either (the check swallowed it as false).
            ct.ThrowIfCancellationRequested();
            return alive;
        }
        catch (TimeoutException)
        {
            Abandon(abort);
            // The one log line of a timeout (09 18.2). The exception is not written: its text can carry connection details.
            LogDbTimeout(logger, LogEvents.DbTimeout);
            return false;
        }
        catch (OperationCanceledException)
        {
            // The request was cancelled: not a failure; the cancellation passes up and nothing is logged.
            Abandon(abort);
            throw;
        }
    }

    // Owns and disposes the source. Never throws: a failure of the check is a false answer (09 8.1 item 6).
    private static async Task<bool> RunCheckAsync(NpgsqlDataSource dataSource, Deadline abort)
    {
        using (abort)
        {
            try
            {
                await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(abort.Token);
                await using NpgsqlCommand command = new("SELECT 1", connection);
                await command.ExecuteScalarAsync(abort.Token);
                return true;
            }
#pragma warning disable CA1031 // 09 8.1 item 6: the liveness probe is the sixth place a broad catch is allowed.
            catch (Exception)
#pragma warning restore CA1031
            {
                return false;
            }
        }
    }

    private static void Abandon(Deadline abort)
    {
        try
        {
            abort.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The check already finished and disposed its source.
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event}")]
    private static partial void LogDbTimeout(ILogger logger, string @event);
}
