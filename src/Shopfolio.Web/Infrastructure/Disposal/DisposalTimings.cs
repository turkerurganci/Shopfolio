namespace Shopfolio.Infrastructure.Disposal;

/// <summary>
/// The numbers of the disposal job that are not product constants (09 3.5 item 4; 05 7.4.1, 11.1.32). The product constants of the retention
/// periods (P-15, P-16, P-31..P-36, P-45, P-49) live in <c>ProductConstants</c>; the legal periods live with the part that applies them. The part
/// size is a row of the 09 3.5 table and <c>DisposalTimingsConsistencyTests</c> keeps it equal to that row.
/// </summary>
internal static class DisposalTimings
{
    /// <summary>The most rows (files, for a part that deletes files) one part of the disposal handles in one transaction: 500 (05 7.4.1; K-1350's pattern).</summary>
    public const int PartSize = 500;

    /// <summary>An unopened guest cart on the server goes after this many days: the cookie's life, 400 days (05 6.7.8, 11.1.32; 06 9.1 Z-6).</summary>
    public const int GuestCartIdleDays = 400;

    /// <summary>A file or an unlinked image goes only when it is at least this many days old: a file just written may not have its row yet (05 7.7.1, 7.7.2; K-980).</summary>
    public const int UnlinkedFileMinAgeDays = 1;
}
