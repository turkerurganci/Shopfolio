// Deliberate violation sample of the SQL clock scan (09 7.1 items 1, 5): SQL text outside the migrations that reads the database clock.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal static class SqlClockQuery
{
    public const string DueJobs = "SELECT id FROM jobs WHERE run_at <= now()";
    public const string Stamped = "UPDATE jobs SET picked_at = CURRENT_TIMESTAMP";
    public const string Clock = "SELECT clock_timestamp()";
    // A comment that says now() is not a finding.
    public const string Parameter = "SELECT id FROM jobs WHERE run_at <= @now";
}
