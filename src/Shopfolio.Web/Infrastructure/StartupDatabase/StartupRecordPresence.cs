namespace Shopfolio.Infrastructure.StartupDatabase;

/// <summary>
/// Whether one record the start-up data check looks for exists (06 8.2.1): <paramref name="Record"/> is the record's name, <paramref name="IsList"/>
/// tells a list (the provinces, the countries) from a single-row record, <paramref name="IsPresent"/> is the existence of the row or rows. It never
/// carries a value of the row (06 8.2.1: the check is the row's existence, not its value).
/// </summary>
internal sealed record StartupRecordPresence(string Record, bool IsList, bool IsPresent);
