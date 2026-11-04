using Npgsql;

using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Infrastructure.StartupDatabase;

/// <summary>
/// Registers the database side of the start-up check (06 8.2.1). It is added after the settings check, so a missing setting stops the start
/// before the database is asked anything.
/// </summary>
internal static class StartupDatabaseRegistration
{
    public static IServiceCollection AddStartupDatabaseCheck(this IServiceCollection services)
    {
        services.AddHostedService<StartupDatabaseCheck>();
        return services;
    }
}

/// <summary>
/// Checks the start data when the application starts (06 8.2.1; the pattern of <c>StartupSettingsCheck</c>): a missing record is written to the
/// log by its name, then the start is refused. A database that cannot be reached is not a missing record: the start goes on and the liveness
/// endpoint answers 503 (09 10.7 item 6), with one warning line that the check did not run. Lines go through <c>ILogger&lt;T&gt;</c>
/// (the file is T230's).
/// </summary>
internal sealed partial class StartupDatabaseCheck : IHostedService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StartupDatabaseCheck> _logger;

    public StartupDatabaseCheck(NpgsqlDataSource dataSource, IServiceScopeFactory scopeFactory, ILogger<StartupDatabaseCheck> logger)
    {
        _dataSource = dataSource;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!await CanConnectAsync(cancellationToken))
        {
            LogNotChecked(_logger, LogEvents.StartupDatabaseChecked);
            return;
        }

        IReadOnlyList<StartupRecordPresence> records;
        await using (AsyncServiceScope scope = _scopeFactory.CreateAsyncScope())
        {
            records = await scope.ServiceProvider.GetRequiredService<AppDbContext>().ReadStartupRecordPresenceAsync(cancellationToken);
        }

        IReadOnlyList<string> failures = DatabaseAudit.FindFailures(records);
        foreach (string failure in failures)
        {
            LogFailure(_logger, LogEvents.StartupDatabaseChecked, failure);
        }

        if (failures.Count > 0)
        {
            throw new InvalidOperationException("The application does not open: " + string.Join(" ", failures));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // Opens a connection through the data source first: the context's own failed open writes an error line with the server's address, which a
    // refused start-up check does not need (the liveness probe keeps connection details out of the log too, 09 10.7 item 7).
    private async Task<bool> CanConnectAsync(CancellationToken ct)
    {
        try
        {
            await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(ct);
            return true;
        }
        catch (NpgsqlException)
        {
            // The exception is not written: its text can carry connection details.
            return false;
        }
    }

    [LoggerMessage(Level = LogLevel.Critical, Message = "{Event}: refused. {Failure}")]
    private static partial void LogFailure(ILogger logger, string @event, string failure);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event}: the database could not be reached; the start data was not checked.")]
    private static partial void LogNotChecked(ILogger logger, string @event);
}
