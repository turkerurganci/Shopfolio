// Deliberate violation sample of the database session scan (09 10.7 item 2): SQL text that changes a session time on an open connection.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal static class SessionTimeoutChange
{
    public const string Statement = "SET statement_timeout = 600000";
    public const string Lock = "set lock_timeout to 1000";
    public const string Local = "SET LOCAL idle_in_transaction_session_timeout = 0";
    public const string Function = "SELECT set_config('statement_timeout', '0', false)";
    public const string SplitOverLines = @"SET
        lock_timeout = 0";
    // A comment that says SET statement_timeout is not a finding.
    public const string Other = "SET search_path = public";
    public const string Read = "SHOW statement_timeout";
    public static readonly string Retry = ".UseNpgsql(dsn, o => o.EnableRetryOnFailure())";
    public static bool Handled(PostgresException e) => e.SqlState == "55P03" || e.SqlState == "57014";
}
