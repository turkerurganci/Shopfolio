using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the SQL text in the application code outside the migrations (T227; 09 7.1 items 1, 5): the database is not a time source,
/// so no SQL reads its clock (<c>now()</c>, <c>current_timestamp</c>, <c>clock_timestamp()</c> and their kin) - the instant the application
/// read is passed as a parameter. The migrations have their own scan with the one allowed line, the disposal function's clamp
/// (<see cref="MigrationSourceScan"/>); this scan covers every other source file. Comment lines are dropped; the scan is line based.
/// </summary>
internal static partial class SqlClockSourceScan
{
    public const string DatabaseClock = "reads the database clock in SQL (pass the instant the application read as a parameter)";

    /// <summary>The scan applies to every application source file but the migrations, which <see cref="MigrationSourceScan"/> covers.</summary>
    public static bool Applies(string relativePath)
    {
        return !MigrationSourceScan.Applies(relativePath);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            if (ClockFunction().IsMatch(lines[i]))
            {
                findings.Add((DatabaseClock, i + 1));
            }
        }

        return findings;
    }

    [GeneratedRegex(@"\b(now|clock_timestamp|statement_timestamp|transaction_timestamp)\s*\(|\b(current_timestamp|localtimestamp|current_date)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ClockFunction();
}
