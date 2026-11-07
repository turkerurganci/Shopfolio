namespace Shopfolio.Infrastructure;

/// <summary>
/// Database session times (09 10.7 item 1). Every value is one row of the 09 3.5 table and has no other place: the connection string
/// (<see cref="DatabaseConnection"/>), the tests and the liveness check read these constants and never write a number of their own.
/// The times are not parameters of the inventory of 02 11.2, so they do not live in <c>ProductConstants</c> (09 3.5 item 1).
/// </summary>
internal static class DatabaseTimeouts
{
    /// <summary>The server-side time a statement waits for a lock (<c>lock_timeout</c>; value in the 09 3.5 table).</summary>
    public static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(5);

    /// <summary>The server-side time a statement runs, web profile (<c>statement_timeout</c>; value in the 09 3.5 table).</summary>
    public static readonly TimeSpan StatementTimeout = TimeSpan.FromSeconds(15);

    /// <summary>The server-side time a statement runs, maintenance profile: the migration service and the commands (value in the 09 3.5 table).</summary>
    public static readonly TimeSpan MaintenanceStatementTimeout = TimeSpan.FromMinutes(10);

    /// <summary>The server-side time an open transaction may stand idle (<c>idle_in_transaction_session_timeout</c>; value in the 09 3.5 table).</summary>
    public static readonly TimeSpan IdleInTransactionTimeout = TimeSpan.FromSeconds(30);

    /// <summary>The client command time of the web profile (value in the 09 3.5 table); it is the backup of the server-side times.</summary>
    public static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(20);

    /// <summary>The client command time of the maintenance profile (value in the 09 3.5 table); the server cancels first.</summary>
    public static readonly TimeSpan MaintenanceCommandTimeout = TimeSpan.FromMinutes(10) + TimeSpan.FromSeconds(5);
}
