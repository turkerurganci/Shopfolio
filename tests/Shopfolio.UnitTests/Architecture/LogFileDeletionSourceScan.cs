using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The scan of the logging code for the one thing it must never do (T225 criterion 5 and 6; 05 9.2.1, 9.2.2; 09 18.1 item 3): delete or
/// overwrite a log file, leave a limit of the library open (a limit on the number, the age or the size of the files deletes or stops writing), or
/// write a disposal record. Only the disposal job (T201) deletes a log file and writes the disposal record. The scan reads
/// <c>src/Shopfolio.Web/Infrastructure/Logging/</c> only — the folder that holds the writers; comments are not scanned.
/// </summary>
internal static partial class LogFileDeletionSourceScan
{
    public const string FileDeleted = "LogFileDeletedByTheLoggingCode";

    public const string FileOverwritten = "LogFileOverwrittenByTheLoggingCode";

    public const string LibraryLimitOpen = "LibraryLimitOnTheFilesIsNotClosed";

    public const string DisposalRecordWritten = "DisposalRecordInTheLoggingCode";

    private const string LoggingFolder = "src/Shopfolio.Web/Infrastructure/Logging/";

    /// <summary>True when the scan applies to the file: a source file of the logging folder.</summary>
    public static bool AppliesTo(string relativePath) =>
        relativePath.Replace('\\', '/').Contains(LoggingFolder, StringComparison.Ordinal);

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        string withoutBlocks = BlockComment().Replace(text.ReplaceLineEndings("\n"), match => new string('\n', match.Value.Count(c => c == '\n')));
        string[] lines = withoutBlocks.Split('\n');
        List<(string Rule, int Line)> findings = [];
        for (int i = 0; i < lines.Length; i++)
        {
            string code = StripComment(lines[i]);
            if (Delete().IsMatch(code))
            {
                findings.Add((FileDeleted, i + 1));
            }

            if (Overwrite().IsMatch(code))
            {
                findings.Add((FileOverwritten, i + 1));
            }

            if (OpenLimit().IsMatch(code))
            {
                findings.Add((LibraryLimitOpen, i + 1));
            }

            if (DisposalRecord().IsMatch(code))
            {
                findings.Add((DisposalRecordWritten, i + 1));
            }
        }

        return findings;
    }

    private static string StripComment(string line)
    {
        int comment = line.IndexOf("//", StringComparison.Ordinal);
        return comment < 0 ? line : line[..comment];
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    // File.Delete(..), Directory.Delete(..), info.Delete(), FileSystem.DeleteFile(..), DeleteAsync(..): any member whose name starts with Delete.
    [GeneratedRegex(@"\.\s*Delete\w*\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Delete();

    // Opening an existing file with Create or Truncate, or writing a whole file over it, empties the day's file.
    [GeneratedRegex(@"FileMode\s*\.\s*(Create|Truncate)\b|\bFile\s*\.\s*(WriteAll\w+|Create|Replace)\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Overwrite();

    // retainedFileCountLimit / retainedFileTimeLimit / fileSizeLimitBytes with anything but null; rollOnFileSizeLimit: true.
    [GeneratedRegex(@"\b(retainedFileCountLimit|retainedFileTimeLimit|fileSizeLimitBytes)\s*:\s*(?!null\b)\S|\brollOnFileSizeLimit\s*:\s*true\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OpenLimit();

    [GeneratedRegex(@"\bDisposal\w*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DisposalRecord();
}
