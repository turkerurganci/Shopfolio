namespace Shopfolio.Infrastructure.StartupDatabase;

/// <summary>
/// The wording of a failed database check (06 8.2.1; 09 18.1 item 1). A failure names the missing record and says what it means; it never carries
/// a value, because the check looks at the existence of the row and reads no value of it.
/// </summary>
internal static class DatabaseFailures
{
    public static string EmptyList(string record) =>
        record + " list is empty or its table is missing; the migration was left half done.";

    public static string MissingRow(string record) =>
        record + " row is missing; the migration was left half done.";
}

/// <summary>
/// The database side of the start-up check (06 8.2.1; 05 8.4.6; 09 3.5 item 2): the province list and the country list are not empty and the
/// six single-row records the start data creates exist. A pure function of what the context found, so a test needs no application. The
/// records outside the check (06 8.2.2) and the settings (06 8.2.3, <c>SettingsAudit</c>) are not looked at here.
/// </summary>
internal static class DatabaseAudit
{
    public static IReadOnlyList<string> FindFailures(IEnumerable<StartupRecordPresence> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        return [.. records.Where(r => !r.IsPresent).Select(r => r.IsList ? DatabaseFailures.EmptyList(r.Record) : DatabaseFailures.MissingRow(r.Record))];
    }
}
