using System.Globalization;

using Npgsql;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The two kinds of database session (09 10.7 item 2). The entry point picks the profile when the host is built; a request or a
/// command never does.
/// </summary>
public enum DatabaseSessionProfile
{
    /// <summary>The application service and the job queue runner in it (the liveness probe and the export included).</summary>
    Web,

    /// <summary>The migration service and the administration commands: the statement time and the client time are longer.</summary>
    Maintenance,
}

/// <summary>
/// The one place the session times reach a connection (09 10.7 item 2). The three server-side times travel in the connection string's
/// <c>Options</c> key and the client time in <c>Command Timeout</c>, so they are set when the connection is made and hold for the whole
/// session. A pooled connection keeps its settings; this is why the two profiles are two connection strings (two pools) and no code
/// changes a time on an open connection with a <c>SET</c> statement. The source scan of the <c>SET</c> forms allows this file and the tests.
/// </summary>
internal static class DatabaseConnection
{
    /// <summary>Writes the three server-side times and the client time of <paramref name="profile"/> into <paramref name="builder"/>.</summary>
    public static void Configure(NpgsqlConnectionStringBuilder builder, DatabaseSessionProfile profile)
    {
        ArgumentNullException.ThrowIfNull(builder);

        TimeSpan statement = profile switch
        {
            DatabaseSessionProfile.Web => DatabaseTimeouts.StatementTimeout,
            DatabaseSessionProfile.Maintenance => DatabaseTimeouts.MaintenanceStatementTimeout,
            _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, "Unknown database session profile."),
        };
        TimeSpan client = profile == DatabaseSessionProfile.Maintenance ? DatabaseTimeouts.MaintenanceCommandTimeout : DatabaseTimeouts.CommandTimeout;

        builder.Options = string.Create(
            CultureInfo.InvariantCulture,
            $"-c lock_timeout={(int)DatabaseTimeouts.LockTimeout.TotalMilliseconds} -c statement_timeout={(int)statement.TotalMilliseconds} -c idle_in_transaction_session_timeout={(int)DatabaseTimeouts.IdleInTransactionTimeout.TotalMilliseconds}");
        builder.CommandTimeout = (int)client.TotalSeconds;
    }

    /// <summary>The connection string of an installation setting, with the times of <paramref name="profile"/> written in.</summary>
    public static string Create(string connectionString, DatabaseSessionProfile profile)
    {
        DsnBuilder builder = new(connectionString);
        Configure(builder, profile);
        return builder.ConnectionString;
    }
}
